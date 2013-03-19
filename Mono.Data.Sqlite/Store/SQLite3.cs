/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/
 
namespace Mono.Data.Sqlite
{
    using System;
    using System.Data;
    using MonoDataSqliteWrapper;
#if SILVERLIGHT
#else
    using System.Runtime.InteropServices;
#endif

    /// <summary>
    /// This class implements SQLiteBase completely, and is the guts of the code that interop's SQLite with .NET
    /// </summary>
    internal class SQLite3 : SQLiteBase
    {
        /// <summary>
        /// The opaque pointer returned to us by the sqlite provider
        /// </summary>
        protected SqliteConnectionHandle _sql;

        protected string _fileName;
        protected bool _usePool;
        protected int _poolVersion = 0;

        private bool _buildingSchema = false;

        /// <summary>
        /// The user-defined functions registered on this connection
        /// </summary>
        protected SqliteFunction[] _functionsArray;

        internal SQLite3(SQLiteDateFormats fmt)
            : base(fmt)
        {
        }

        protected override void Dispose(bool bDisposing)
        {
            if (bDisposing)
                Close();
        }

        // It isn't necessary to cleanup any functions we've registered.  If the connection
        // goes to the pool and is resurrected later, re-registered functions will overwrite the
        // previous functions.  The SqliteFunctionCookieHandle will take care of freeing unmanaged
        // resources belonging to the previously-registered functions.
        internal override void Close()
        {
            if (_sql != null)
            {
                if (_usePool)
                {
                    ResetConnection(_sql);
                    SqliteConnectionPool.Add(_fileName, _sql, _poolVersion);
                }
                else
                {
                    _sql.Dispose();
                }
            }

            _sql = null;
        }

        internal override void Cancel()
        {
            UnsafeNativeMethods.sqlite3_interrupt(_sql);
        }

        internal override string Version
        {
            get { return SQLite3.SQLiteVersion; }
        }

        internal static string SQLiteVersion
        {
            get { return UTF8ToString(UnsafeNativeMethods.sqlite3_libversion(), -1); }
        }

        internal override int Changes
        {
            get { return UnsafeNativeMethods.sqlite3_changes(_sql); }
        }

        internal override void Open(string strFilename, SQLiteOpenFlagsEnum flags, int maxPoolSize, bool usePool)
        {
            if (_sql != null)
            {
                return;
            }

            _usePool = usePool;
            if (usePool)
            {
                _fileName = strFilename;
                _sql = SqliteConnectionPool.Remove(strFilename, maxPoolSize, out _poolVersion);
            }

            if (_sql == null)
            {
                if ((flags & SQLiteOpenFlagsEnum.Create) == 0 && FileExists(strFilename) == false)
                {
                    throw new SqliteException((int)SQLiteErrorCode.CantOpen, strFilename);
                }

                SqliteConnectionHandle db;
                int n = UnsafeNativeMethods.sqlite3_open_v2(ToUTF8(strFilename), out db, (int)flags, string.Empty);
                if (n > 0)
                {
                    throw new SqliteException(n, null);
                }

                _sql = db;
            }
            // Bind functions to this connection.  If any previous functions of the same name
            // were already bound, then the new bindings replace the old.
            _functionsArray = SqliteFunction.BindFunctions(this);
            SetTimeout(0);
        }

        internal override void ClearPool()
        {
            SqliteConnectionPool.ClearPool(_fileName);
        }

        internal override void SetTimeout(int timeout)
        {
            int n = UnsafeNativeMethods.sqlite3_busy_timeout(_sql, timeout);
            if (n > 0) throw new SqliteException(n, SQLiteLastError());
        }

        internal override bool Step(SqliteStatement stmt)
        {
            var rnd = new Random();

            var starttick = (uint)Environment.TickCount;
            var timeout = (uint)(stmt._command._commandTimeout * 1000);

            while (true)
            {
                int n = UnsafeNativeMethods.sqlite3_step(stmt._sqlite_stmt);

                if (n == 100)
                {
                    return true;
                }
                if (n == 101)
                {
                    return false;
                }

                if (n > 0)
                {
                    // An error occurred, attempt to reset the statement.  If the reset worked because the
                    // schema has changed, re-try the step again.  If it errored our because the database
                    // is locked, then keep retrying until the command timeout occurs.
                    int r = this.Reset(stmt);

                    if (r == 0)
                    {
                        throw new SqliteException(n, SQLiteLastError());
                    }
                    
                    if ((r == 6 || r == 5) && stmt._command != null) // SQLITE_LOCKED || SQLITE_BUSY
                    {
                        // Keep trying, but if we've exceeded the command's timeout, give up and throw an error
                        if ((uint)Environment.TickCount - starttick > timeout)
                        {
                            throw new SqliteException(r, SQLiteLastError());
                        }
                        
                        // Otherwise sleep for a random amount of time up to 150ms
                        Sleep(rnd.Next(1, 150));
                    }
                }
            }
        }

        internal override int Reset(SqliteStatement stmt)
        {
            int n = UnsafeNativeMethods.sqlite3_reset(stmt._sqlite_stmt);

            // If the schema changed, try and re-prepare it
            if (n == 17) // SQLITE_SCHEMA
            {
                // Recreate a dummy statement
                var timeout = (uint)(stmt._command._commandTimeout * 1000);
                string str;
                using (SqliteStatement tmp = Prepare(null, stmt._sqlStatement, null, timeout, out str))
                {
                    // Finalize the existing statement
                    stmt._sqlite_stmt.Dispose();
                    // Reassign a new statement pointer to the old statement and clear the temporary one
                    stmt._sqlite_stmt = tmp._sqlite_stmt;
                    tmp._sqlite_stmt = null;

                    // Reapply parameters
                    stmt.BindParameters();
                }

                return -1; // Reset was OK, with schema change
            }

            if (n == 6 || n == 5) // SQLITE_LOCKED || SQLITE_BUSY
            {
                return n;
            }

            if (n > 0)
            {
                throw new SqliteException(n, SQLiteLastError());
            }

            return 0; // We reset OK, no schema changes
        }

        internal override string SQLiteLastError()
        {
            return SQLiteLastError(_sql);
        }

        internal override SqliteStatement Prepare(SqliteConnection cnn, string strSql, SqliteStatement previous,
                                                  uint timeout, out string strRemain)
        {
            SqliteStatementHandle stmt = null;
            string ptr = null;
     
            int len = 0;
            int n = 17;
            int retries = 0;
            SqliteStatement cmd = null;
            var rnd = new Random();
            var starttick = (uint) Environment.TickCount;

                while ((n == 17 || n == 6 || n == 5) && retries < 3)
                {
                    n = UnsafeNativeMethods.sqlite3_prepare(_sql, strSql, strSql.Length, out stmt, out ptr);
                    len = -1;

                    if (n == 17)
                    {
                        retries++;
                    }
                    else if (n == 1)
                    {
                        if (String.Compare(SQLiteLastError(), "near \"TYPES\": syntax error",
                                           StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            int pos = strSql.IndexOf(';');
                            if (pos == -1)
                            {
                                pos = strSql.Length - 1;
                            }

                            string typedefs = strSql.Substring(0, pos + 1);
                            strSql = strSql.Substring(pos + 1);

                            strRemain = "";

                            while (cmd == null && strSql.Length > 0)
                            {
                                cmd = Prepare(cnn, strSql, previous, timeout, out strRemain);
                                strSql = strRemain;
                            }

                            if (cmd != null)
                            {
                                cmd.SetTypes(typedefs);
                            }

                            return cmd;
                        }
                        else if (_buildingSchema == false &&
                                 String.Compare(SQLiteLastError(), 0, "no such table: TEMP.SCHEMA", 0, 26,
                                                StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            strRemain = "";
                            _buildingSchema = true;
                            try
                            {
                                while (cmd == null && strSql.Length > 0)
                                {
                                    cmd = Prepare(cnn, strSql, previous, timeout, out strRemain);
                                    strSql = strRemain;
                                }

                                return cmd;
                            }
                            finally
                            {
                                _buildingSchema = false;
                            }
                        }
                    }
                    else if (n == 6 || n == 5) // Locked -- delay a small amount before retrying
                    {
                        // Keep trying, but if we've exceeded the command's timeout, give up and throw an error
                        if ((uint) Environment.TickCount - starttick > timeout)
                        {
                            throw new SqliteException(n, SQLiteLastError());
                        }
                        else
                        {
                            // Otherwise sleep for a random amount of time up to 150ms
                            Sleep(rnd.Next(1, 150));
                        }
                    }
                }

                if (n > 0) throw new SqliteException(n, SQLiteLastError());

                strRemain = UTF8ToString(ptr, len);

                var hdl = stmt;
                if (stmt != null)
                {
                    cmd = new SqliteStatement(this, hdl, strSql.Substring(0, strSql.Length - strRemain.Length), previous);
                }

                return cmd;
        }

        internal override void Bind_Double(SqliteStatement stmt, int index, double value)
        {
            int n = UnsafeNativeMethods.sqlite3_bind_double(stmt._sqlite_stmt, index, value);
            if (n > 0) throw new SqliteException(n, SQLiteLastError());
        }

        internal override void Bind_Int32(SqliteStatement stmt, int index, int value)
        {
            int n = UnsafeNativeMethods.sqlite3_bind_int(stmt._sqlite_stmt, index, value);
            if (n > 0) throw new SqliteException(n, SQLiteLastError());
        }

        internal override void Bind_Int64(SqliteStatement stmt, int index, long value)
        {
            int n = UnsafeNativeMethods.sqlite3_bind_int64(stmt._sqlite_stmt, index, value);
            if (n > 0) throw new SqliteException(n, SQLiteLastError());
        }

        internal override void Bind_Text(SqliteStatement stmt, int index, string value)
        {
            int n = UnsafeNativeMethods.sqlite3_bind_text(stmt._sqlite_stmt, index, value, value.Length, null);
            if (n > 0) throw new SqliteException(n, SQLiteLastError());
        }

        internal override void Bind_DateTime(SqliteStatement stmt, int index, DateTime dt)
        {
            var value = ToUTF8(dt);
            int n = UnsafeNativeMethods.sqlite3_bind_text(stmt._sqlite_stmt, index, value, value.Length, null);
            if (n > 0) throw new SqliteException(n, SQLiteLastError());
        }

        internal override void Bind_Blob(SqliteStatement stmt, int index, byte[] blobData)
        {
            int n = UnsafeNativeMethods.sqlite3_bind_blob(stmt._sqlite_stmt, index, blobData, blobData.Length, null);
            if (n > 0) throw new SqliteException(n, SQLiteLastError());
        }

        internal override void Bind_Null(SqliteStatement stmt, int index)
        {
            int n = UnsafeNativeMethods.sqlite3_bind_null(stmt._sqlite_stmt, index);
            if (n > 0) throw new SqliteException(n, SQLiteLastError());
        }

        internal override int Bind_ParamCount(SqliteStatement stmt)
        {
            return UnsafeNativeMethods.sqlite3_bind_parameter_count(stmt._sqlite_stmt);
        }

        internal override string Bind_ParamName(SqliteStatement stmt, int index)
        {
            return UTF8ToString(UnsafeNativeMethods.sqlite3_bind_parameter_name(stmt._sqlite_stmt, index), -1);
        }

        internal override int Bind_ParamIndex(SqliteStatement stmt, string paramName)
        {
            return UnsafeNativeMethods.sqlite3_bind_parameter_index(stmt._sqlite_stmt, ToUTF8(paramName));
        }

        internal override int ColumnCount(SqliteStatement stmt)
        {
            return UnsafeNativeMethods.sqlite3_column_count(stmt._sqlite_stmt);
        }

        internal override string ColumnName(SqliteStatement stmt, int index)
        {
            return UTF8ToString(UnsafeNativeMethods.sqlite3_column_name(stmt._sqlite_stmt, index), -1);
        }

        internal override TypeAffinity ColumnAffinity(SqliteStatement stmt, int index)
        {
            return (TypeAffinity) UnsafeNativeMethods.sqlite3_column_type(stmt._sqlite_stmt, index);
        }

        internal override string ColumnType(SqliteStatement stmt, int index, out TypeAffinity nAffinity)
        {
            var p = UnsafeNativeMethods.sqlite3_column_decltype(stmt._sqlite_stmt, index);
            nAffinity = ColumnAffinity(stmt, index);

            if (!string.IsNullOrEmpty(p))
            {
                return UTF8ToString(p, -1);
            }
            
            string[] ar = stmt.TypeDefinitions;
            if (ar != null)
            {
                if (index < ar.Length && ar[index] != null)
                    return ar[index];
            }

            return String.Empty;
        }

        internal override int ColumnIndex(SqliteStatement stmt, string columnName)
        {
            int x = ColumnCount(stmt);

            for (int n = 0; n < x; n++)
            {
                if (String.Compare(columnName, ColumnName(stmt, n), StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return n;
                }
            }

            return -1;
        }

        internal override string ColumnOriginalName(SqliteStatement stmt, int index)
        {
            return UTF8ToString(UnsafeNativeMethods.sqlite3_column_origin_name(stmt._sqlite_stmt, index), -1);
        }

        internal override string ColumnDatabaseName(SqliteStatement stmt, int index)
        {
            return UTF8ToString(UnsafeNativeMethods.sqlite3_column_database_name(stmt._sqlite_stmt, index), -1);
        }

        internal override string ColumnTableName(SqliteStatement stmt, int index)
        {
            return UTF8ToString(UnsafeNativeMethods.sqlite3_column_table_name(stmt._sqlite_stmt, index), -1);
        }

        internal override void ColumnMetaData(string dataBase, string table, string column, out string dataType,
                                              out string collateSequence, out bool notNull, out bool primaryKey,
                                              out bool autoIncrement)
        {
            string dataTypePtr = string.Empty;
            string collSeqPtr = string.Empty;
            int nnotNull = 0;
            int nprimaryKey = 0;
            int nautoInc = 0;

            int n = UnsafeNativeMethods.sqlite3_table_column_metadata(_sql, ToUTF8(dataBase), ToUTF8(table), 
                                                                      ToUTF8(column), out dataTypePtr, out collSeqPtr, 
                                                                      out nnotNull, out nprimaryKey, out nautoInc);
            if (n > 0) throw new SqliteException(n, SQLiteLastError());

            dataType = UTF8ToString(dataTypePtr, -1);
            collateSequence = UTF8ToString(collSeqPtr, -1);

            notNull = (nnotNull == 1);
            primaryKey = (nprimaryKey == 1);
            autoIncrement = (nautoInc == 1);
        }

        internal override double GetDouble(SqliteStatement stmt, int index)
        {
            return UnsafeNativeMethods.sqlite3_column_double(stmt._sqlite_stmt, index);
        }

        internal override int GetInt32(SqliteStatement stmt, int index)
        {
            return UnsafeNativeMethods.sqlite3_column_int(stmt._sqlite_stmt, index);
        }

        internal override long GetInt64(SqliteStatement stmt, int index)
        {
            return UnsafeNativeMethods.sqlite3_column_int64(stmt._sqlite_stmt, index);
        }

        internal override string GetText(SqliteStatement stmt, int index)
        {
            return UTF8ToString(UnsafeNativeMethods.sqlite3_column_text(stmt._sqlite_stmt, index), -1);
        }

        internal override DateTime GetDateTime(SqliteStatement stmt, int index)
        {
            return ToDateTime(UnsafeNativeMethods.sqlite3_column_text(stmt._sqlite_stmt, index), -1);
        }

        internal override long GetBytes(SqliteStatement stmt, int index, int nDataOffset, byte[] bDest, int nStart,
                                        int nLength)
        {
            int nCopied = nLength;
            int nlen = UnsafeNativeMethods.sqlite3_column_bytes(stmt._sqlite_stmt, index);
            var ptr = UnsafeNativeMethods.sqlite3_column_blob(stmt._sqlite_stmt, index);

            if (bDest == null)
            {
                return nlen;
            }
            
            if (nCopied + nStart > bDest.Length)
            {
                nCopied = bDest.Length - nStart;
            }
            if (nCopied + nDataOffset > nlen)
            {
                nCopied = nlen - nDataOffset;
            }

            if (nCopied > 0)
            {
                Array.Copy(ptr, nStart + nDataOffset, bDest, 0, nCopied);
            }
            else
            {
                nCopied = 0;
            }

            return nCopied;
        }

        internal override long GetChars(SqliteStatement stmt, int index, int nDataOffset, char[] bDest, int nStart,
                                        int nLength)
        {
            int nCopied = nLength;

            string str = GetText(stmt, index);
            int nlen = str.Length;

            if (bDest == null)
            {
                return nlen;
            }

            if (nCopied + nStart > bDest.Length)
            {
                nCopied = bDest.Length - nStart;
            }
            if (nCopied + nDataOffset > nlen)
            {
                nCopied = nlen - nDataOffset;
            }

            if (nCopied > 0)
            {
                str.CopyTo(nDataOffset, bDest, nStart, nCopied);
            }
            else
            {
                nCopied = 0;
            }

            return nCopied;
        }

        internal override bool IsNull(SqliteStatement stmt, int index)
        {
            return this.ColumnAffinity(stmt, index) == TypeAffinity.Null;
        }

        internal override void CreateFunction(string strFunction, int nArgs, bool needCollSeq, SQLiteCallback func,
                                              SQLiteCallback funcstep, SQLiteFinalCallback funcfinal)
        {
            //int n = UnsafeNativeMethods.sqlite3_create_function(_sql, ToUTF8(strFunction), nArgs, 4, IntPtr.Zero, func,
            //                                                    funcstep, funcfinal);
            //if (n == 0)
            //{
            //    n = UnsafeNativeMethods.sqlite3_create_function(_sql, ToUTF8(strFunction), nArgs, 1, IntPtr.Zero, func,
            //                                                    funcstep, funcfinal);
            //}

            //if (n > 0)
            //{
            //    throw new SqliteException(n, SQLiteLastError());
            //}

            throw new NotImplementedException();
        }

        internal override void CreateCollation(string strCollation, SQLiteCollation func, SQLiteCollation func16)
        {
            //int n = UnsafeNativeMethods.sqlite3_create_collation(_sql, ToUTF8(strCollation), 2, IntPtr.Zero, func16);
            //if (n == 0)
            //{
            //    UnsafeNativeMethods.sqlite3_create_collation(_sql, ToUTF8(strCollation), 1, IntPtr.Zero, func);
            //}

            //if (n > 0)
            //{
            //    throw new SqliteException(n, SQLiteLastError());
            //}

            throw new NotImplementedException();
        }

        internal override int ContextCollateCompare(CollationEncodingEnum enc, SqliteContextHandle context, string s1,
                                                    string s2)
        {
            throw new NotImplementedException();
        }

        internal override int ContextCollateCompare(CollationEncodingEnum enc, SqliteContextHandle context, char[] c1,
                                                    char[] c2)
        {
            throw new NotImplementedException();
        }

        internal override CollationSequence GetCollationSequence(SqliteFunction func, SqliteContextHandle context)
        {
            throw new NotImplementedException();
        }

        internal override long GetParamValueBytes(SqliteValueHandle p, int nDataOffset, byte[] bDest, int nStart,
                                                  int nLength)
        {
            int nCopied = nLength;

            int nlen = UnsafeNativeMethods.sqlite3_value_bytes(p);
            var ptr = UnsafeNativeMethods.sqlite3_value_blob(p);

            if (bDest == null) return nlen;

            if (nCopied + nStart > bDest.Length)
            {
                nCopied = bDest.Length - nStart;
            }
            if (nCopied + nDataOffset > nlen)
            {
                nCopied = nlen - nDataOffset;
            }

            if (nCopied > 0)
            {
                Array.Copy(ptr, nStart + nDataOffset, bDest, 0, nCopied);
            }
            else
            {
                nCopied = 0;
            }

            return nCopied;
        }

        internal override double GetParamValueDouble(SqliteValueHandle ptr)
        {
            return UnsafeNativeMethods.sqlite3_value_double(ptr);
        }

        internal override int GetParamValueInt32(SqliteValueHandle ptr)
        {
            return UnsafeNativeMethods.sqlite3_value_int(ptr);
        }

        internal override long GetParamValueInt64(SqliteValueHandle ptr)
        {
            return UnsafeNativeMethods.sqlite3_value_int64(ptr);
        }

        internal override string GetParamValueText(SqliteValueHandle ptr)
        {
            return UTF8ToString(UnsafeNativeMethods.sqlite3_value_text(ptr), -1);
        }

        internal override TypeAffinity GetParamValueType(SqliteValueHandle ptr)
        {
            return (TypeAffinity) UnsafeNativeMethods.sqlite3_value_type(ptr);
        }

        internal override void ReturnBlob(SqliteContextHandle context, byte[] value)
        {
            UnsafeNativeMethods.sqlite3_result_blob(context, value, value.Length, null);
        }

        internal override void ReturnDouble(SqliteContextHandle context, double value)
        {
            UnsafeNativeMethods.sqlite3_result_double(context, value);
        }

        internal override void ReturnError(SqliteContextHandle context, string value)
        {
            UnsafeNativeMethods.sqlite3_result_error(context, ToUTF8(value), value.Length);
        }

        internal override void ReturnInt32(SqliteContextHandle context, int value)
        {
            UnsafeNativeMethods.sqlite3_result_int(context, value);
        }

        internal override void ReturnInt64(SqliteContextHandle context, long value)
        {
            UnsafeNativeMethods.sqlite3_result_int64(context, value);
        }

        internal override void ReturnNull(SqliteContextHandle context)
        {
            UnsafeNativeMethods.sqlite3_result_null(context);
        }

        internal override void ReturnText(SqliteContextHandle context, string value)
        {
            UnsafeNativeMethods.sqlite3_result_text(context, value, value.Length, null);
        }

        internal override SqliteValueHandle AggregateContext(SqliteContextHandle context)
        {
            return UnsafeNativeMethods.sqlite3_aggregate_context(context, 1);
        }
        internal override void SetPassword(string passwordBytes)
        {
            throw new NotImplementedException();

            //int n = UnsafeNativeMethods.sqlite3_key(_sql, passwordBytes, passwordBytes.Length);
            //if (n > 0) throw new SqliteException(n, SQLiteLastError());
        }

        internal override void ChangePassword(string newPasswordBytes)
        {
            throw new NotImplementedException();

            //int keylen = newPasswordBytes == null ? 0 : newPasswordBytes.Length;
            //int n = UnsafeNativeMethods.sqlite3_rekey(_sql, newPasswordBytes, keylen);
            //if (n > 0) throw new SqliteException(n, SQLiteLastError());
        }

        internal override void SetUpdateHook(SqliteUpdateHookDelegate func)
        {
            UnsafeNativeMethods.sqlite3_update_hook(_sql, func, null);
        }

        internal override void SetCommitHook(SqliteCommitHookDelegate func)
        {
            UnsafeNativeMethods.sqlite3_commit_hook(_sql, func, null);
        }

        internal override void SetRollbackHook(SqliteRollbackHookDelegate func)
        {
            UnsafeNativeMethods.sqlite3_rollback_hook(_sql, func, null);
        }

        /// <summary>
        /// Helper function to retrieve a column of data from an active statement.
        /// </summary>
        /// <param name="stmt">The statement being step()'d through</param>
        /// <param name="index">The column index to retrieve</param>
        /// <param name="typ">The type of data contained in the column.  If Uninitialized, this function will retrieve the datatype information.</param>
        /// <returns>Returns the data in the column</returns>
        internal override object GetValue(SqliteStatement stmt, int index, SQLiteType typ)
        {
            if (IsNull(stmt, index)) return DBNull.Value;
            TypeAffinity aff = typ.Affinity;
            Type t = null;

            if (typ.Type != DbType.Object)
            {
                t = SQLiteTypeToType(typ);
                aff = TypeToAffinity(t);
            }

            switch (aff)
            {
                case TypeAffinity.Blob:
                    if (typ.Type == DbType.Guid && typ.Affinity == TypeAffinity.Text)
                    {
                        return new Guid(GetText(stmt, index));
                    }

                    var n = (int) GetBytes(stmt, index, 0, null, 0, 0);
                    var b = new byte[n];
                    GetBytes(stmt, index, 0, b, 0, n);

                    if (typ.Type == DbType.Guid && n == 16)
                    {
                        return new Guid(b);
                    }

                    return b;
                case TypeAffinity.DateTime:
                    return GetDateTime(stmt, index);
                case TypeAffinity.Double:
                    if (t == null)
                    {
                        return GetDouble(stmt, index);
                    }
                    return Convert.ChangeType(GetDouble(stmt, index), t, null);
                case TypeAffinity.Int64:
                    if (t == null)
                    {
                        return GetInt64(stmt, index);
                    }
                    return Convert.ChangeType(GetInt64(stmt, index), t, null);
                default:
                    return GetText(stmt, index);
            }
        }

        internal override int GetCursorForTable(SqliteStatement stmt, int db, int rootPage)
        {
            return -1;
        }

        internal override long GetRowIdForCursor(SqliteStatement stmt, int cursor)
        {
            return 0;
        }

        internal override void GetIndexColumnExtendedInfo(string database, string index, string column, out int sortMode,
                                                          out int onError, out string collationSequence)
        {
            sortMode = 0;
            onError = 2;
            collationSequence = "BINARY";
        }

        private static void Sleep(int ms)
        {
            var end = Environment.TickCount + ms;
            while (Environment.TickCount < end)
            {
                // just no nothing....
            }
        }
    }
}
