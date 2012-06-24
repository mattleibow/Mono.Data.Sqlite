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
    using System.Runtime.InteropServices;
    using System.Collections.Generic;
    using System.Globalization;
    using Community.CsharpSqlite;

    /// <summary>
    /// This class implements SQLiteBase completely, and is the guts of the code that interop's SQLite with .NET
    /// </summary>
    internal class SQLite3 : SQLiteBase
    {
        /// <summary>
        /// The opaque pointer returned to us by the sqlite provider
        /// </summary>
        protected Sqlite3.sqlite3 _sql;

        protected string _fileName;
        protected bool _usePool;
        protected int _poolVersion = 0;

#if !PLATFORM_COMPACTFRAMEWORK
        private bool _buildingSchema = false;
#endif
#if MONOTOUCH
    GCHandle gch;
#endif

        /// <summary>
        /// The user-defined functions registered on this connection
        /// </summary>
        protected SqliteFunction[] _functionsArray;

        internal SQLite3(SQLiteDateFormats fmt)
            : base(fmt)
        {
#if MONOTOUCH
      gch = GCHandle.Alloc (this);
#endif
        }

        protected override void Dispose(bool bDisposing)
        {
            if (bDisposing)
                Close();
#if MONOTOUCH
      if (gch.IsAllocated)
        gch.Free ();
#endif
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
                    SQLiteBase.ResetConnection(_sql);
                    SqliteConnectionPool.Add(_fileName, _sql, _poolVersion);
                }
                else
                    _sql.Dispose();
            }

            _sql = null;
        }

        internal override void Cancel()
        {
            Sqlite3.sqlite3_interrupt(_sql);
        }

        internal override string Version
        {
            get { return SQLite3.SQLiteVersion; }
        }

        internal static string SQLiteVersion
        {
            get { return Sqlite3.sqlite3_libversion(); }
        }

        internal override int Changes
        {
            get { return Sqlite3.sqlite3_changes(_sql); }
        }

        internal override void Open(string strFilename, SQLiteOpenFlagsEnum flags, int maxPoolSize, bool usePool)
        {
            if (_sql != null) return;

            _usePool = usePool;
            if (usePool)
            {
                _fileName = strFilename;
                _sql = SqliteConnectionPool.Remove(strFilename, maxPoolSize, out _poolVersion);
            }

            if (_sql == null)
            {
                Sqlite3.sqlite3 db;

#if !SQLITE_STANDARD
                int n = Sqlite3.sqlite3_open_interop(ToUTF8(strFilename), (int)flags, out db);
#else
                // Compatibility with versions < 3.5.0
                int n;

                try
                {
                    n = Sqlite3.sqlite3_open_v2(strFilename, out db, (int) flags, null);
                }
                catch (EntryPointNotFoundException)
                {
                    Console.WriteLine("Your sqlite3 version is old - please upgrade to at least v3.5.0!");
                    n = Sqlite3.sqlite3_open(strFilename, out db);
                }

#endif
                if (n > 0) throw new SqliteException(n, null);

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

        internal override void SetTimeout(int nTimeoutMS)
        {
            int n = Sqlite3.sqlite3_busy_timeout(_sql, nTimeoutMS);
            if (n > 0) throw new SqliteException(n, SQLiteLastError());
        }

        internal override bool Step(SqliteStatement stmt)
        {
            int n;
            Random rnd = null;
            uint starttick = (uint) Environment.TickCount;
            uint timeout = (uint) (stmt._command._commandTimeout*1000);

            while (true)
            {
                n = Sqlite3.sqlite3_step(stmt._sqlite_stmt);

                if (n == 100) return true;
                if (n == 101) return false;

                if (n > 0)
                {
                    int r;

                    // An error occurred, attempt to reset the statement.  If the reset worked because the
                    // schema has changed, re-try the step again.  If it errored our because the database
                    // is locked, then keep retrying until the command timeout occurs.
                    r = Reset(stmt);

                    if (r == 0)
                        throw new SqliteException(n, SQLiteLastError());

                    else if ((r == 6 || r == 5) && stmt._command != null) // SQLITE_LOCKED || SQLITE_BUSY
                    {
                        // Keep trying
                        if (rnd == null) // First time we've encountered the lock
                            rnd = new Random();

                        // If we've exceeded the command's timeout, give up and throw an error
                        if ((uint) Environment.TickCount - starttick > timeout)
                        {
                            throw new SqliteException(r, SQLiteLastError());
                        }
                        else
                        {
                            // Otherwise sleep for a random amount of time up to 150ms
                            System.Threading.Thread.CurrentThread.Join(rnd.Next(1, 150));
                        }
                    }
                }
            }
        }

        internal override int Reset(SqliteStatement stmt)
        {
            int n;

#if !SQLITE_STANDARD
            n = Sqlite3.sqlite3_reset_interop(stmt._sqlite_stmt);
#else
            n = Sqlite3.sqlite3_reset(stmt._sqlite_stmt);
#endif

            // If the schema changed, try and re-prepare it
            if (n == 17) // SQLITE_SCHEMA
            {
                // Recreate a dummy statement
                string str;
                using (
                    SqliteStatement tmp = Prepare(null, stmt._sqlStatement, null,
                                                  (uint) (stmt._command._commandTimeout*1000), out str))
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
            else if (n == 6 || n == 5) // SQLITE_LOCKED || SQLITE_BUSY
                return n;

            if (n > 0)
                throw new SqliteException(n, SQLiteLastError());

            return 0; // We reset OK, no schema changes
        }

        internal override string SQLiteLastError()
        {
            return SQLiteBase.SQLiteLastError(_sql);
        }

        internal override SqliteStatement Prepare(SqliteConnection cnn, string strSql, SqliteStatement previous,
                                                  uint timeoutMS, out string strRemain)
        {
            Sqlite3.Vdbe stmt = null;
            string ptr = null;
            int len = 0;
            int n = 17;
            int retries = 0;
            string typedefs = null;
            SqliteStatement cmd = null;
            Random rnd = null;
            uint starttick = (uint) Environment.TickCount;

            while ((n == 17 || n == 6 || n == 5) && retries < 3)
            {
#if !SQLITE_STANDARD
                    n = Sqlite3.sqlite3_prepare_interop(_sql, psql, b.Length - 1, out stmt, out ptr, out len);
#else
                n = Sqlite3.sqlite3_prepare(_sql, strSql, strSql.Length, ref stmt, ref ptr);
                len = -1;
#endif

                if (n == 17)
                    retries++;
                else if (n == 1)
                {
                    if (
                        String.Compare(SQLiteLastError(), "near \"TYPES\": syntax error",
                                       StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        int pos = strSql.IndexOf(';');
                        if (pos == -1) pos = strSql.Length - 1;

                        typedefs = strSql.Substring(0, pos + 1);
                        strSql = strSql.Substring(pos + 1);

                        strRemain = "";

                        while (cmd == null && strSql.Length > 0)
                        {
                            cmd = Prepare(cnn, strSql, previous, timeoutMS, out strRemain);
                            strSql = strRemain;
                        }

                        if (cmd != null)
                            cmd.SetTypes(typedefs);

                        return cmd;
                    }
#if !PLATFORM_COMPACTFRAMEWORK
                    else if (_buildingSchema == false &&
                             String.Compare(SQLiteLastError(), 0, "no such table: TEMP.SCHEMA", 0, 26,
                                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        strRemain = "";
                        _buildingSchema = true;
                        try
                        {
                            // todo : this is in the mono source
                            //ISQLiteSchemaExtensions ext = ((IServiceProvider)SqliteFactory.Instance).GetService(typeof(ISQLiteSchemaExtensions)) as ISQLiteSchemaExtensions;
                            //
                            //if (ext != null)
                            //    ext.BuildTempSchema(cnn);

                            while (cmd == null && strSql.Length > 0)
                            {
                                cmd = Prepare(cnn, strSql, previous, timeoutMS, out strRemain);
                                strSql = strRemain;
                            }

                            return cmd;
                        }
                        finally
                        {
                            _buildingSchema = false;
                        }
                    }
#endif
                }
                else if (n == 6 || n == 5) // Locked -- delay a small amount before retrying
                {
                    // Keep trying
                    if (rnd == null) // First time we've encountered the lock
                        rnd = new Random();

                    // If we've exceeded the command's timeout, give up and throw an error
                    if ((uint) Environment.TickCount - starttick > timeoutMS)
                    {
                        throw new SqliteException(n, SQLiteLastError());
                    }
                    else
                    {
                        // Otherwise sleep for a random amount of time up to 150ms
                        System.Threading.Thread.CurrentThread.Join(rnd.Next(1, 150));
                    }
                }
            }

            if (n > 0) throw new SqliteException(n, SQLiteLastError());

            strRemain = ptr;

            if (stmt != null)
                cmd = new SqliteStatement(this, stmt, strSql.Substring(0, strSql.Length - strRemain.Length), previous);

            return cmd;
        }

        internal override void Bind_Double(SqliteStatement stmt, int index, double value)
        {
#if !PLATFORM_COMPACTFRAMEWORK
            int n = Sqlite3.sqlite3_bind_double(stmt._sqlite_stmt, index, value);
#else
      int n = Sqlite3.sqlite3_bind_double_interop(stmt._sqlite_stmt, index, ref value);
#endif
            if (n > 0) throw new SqliteException(n, SQLiteLastError());
        }

        internal override void Bind_Int32(SqliteStatement stmt, int index, int value)
        {
            int n = Sqlite3.sqlite3_bind_int(stmt._sqlite_stmt, index, value);
            if (n > 0) throw new SqliteException(n, SQLiteLastError());
        }

        internal override void Bind_Int64(SqliteStatement stmt, int index, long value)
        {
#if !PLATFORM_COMPACTFRAMEWORK
            int n = Sqlite3.sqlite3_bind_int64(stmt._sqlite_stmt, index, value);
#else
      int n = Sqlite3.sqlite3_bind_int64_interop(stmt._sqlite_stmt, index, ref value);
#endif
            if (n > 0) throw new SqliteException(n, SQLiteLastError());
        }

        internal override void Bind_Text(SqliteStatement stmt, int index, string value)
        {
            int n = Sqlite3.sqlite3_bind_text(stmt._sqlite_stmt, index, value, value.Length, null);
            if (n > 0) throw new SqliteException(n, SQLiteLastError());
        }

        internal override void Bind_DateTime(SqliteStatement stmt, int index, DateTime dt)
        {
            string b = ToString(dt);
            int n = Sqlite3.sqlite3_bind_text(stmt._sqlite_stmt, index, b, b.Length, null);
            if (n > 0) throw new SqliteException(n, SQLiteLastError());
        }

        internal override void Bind_Blob(SqliteStatement stmt, int index, byte[] blobData)
        {
            int n = Sqlite3.sqlite3_bind_blob(stmt._sqlite_stmt, index, blobData, blobData.Length, null);
            if (n > 0) throw new SqliteException(n, SQLiteLastError());
        }

        internal override void Bind_Null(SqliteStatement stmt, int index)
        {
            int n = Sqlite3.sqlite3_bind_null(stmt._sqlite_stmt, index);
            if (n > 0) throw new SqliteException(n, SQLiteLastError());
        }

        internal override int Bind_ParamCount(SqliteStatement stmt)
        {
            return Sqlite3.sqlite3_bind_parameter_count(stmt._sqlite_stmt);
        }

        internal override string Bind_ParamName(SqliteStatement stmt, int index)
        {
#if !SQLITE_STANDARD
            int len;
            return UTF8ToString(Sqlite3.sqlite3_bind_parameter_name_interop(stmt._sqlite_stmt, index, out len), len);
#else
            return Sqlite3.sqlite3_bind_parameter_name(stmt._sqlite_stmt, index);
#endif
        }

        internal override int Bind_ParamIndex(SqliteStatement stmt, string paramName)
        {
            return Sqlite3.sqlite3_bind_parameter_index(stmt._sqlite_stmt, paramName);
        }

        internal override int ColumnCount(SqliteStatement stmt)
        {
            return Sqlite3.sqlite3_column_count(stmt._sqlite_stmt);
        }

        internal override string ColumnName(SqliteStatement stmt, int index)
        {
#if !SQLITE_STANDARD
            int len;
            return UTF8ToString(Sqlite3.sqlite3_column_name_interop(stmt._sqlite_stmt, index, out len), len);
#else
            return Sqlite3.sqlite3_column_name(stmt._sqlite_stmt, index);
#endif
        }

        internal override TypeAffinity ColumnAffinity(SqliteStatement stmt, int index)
        {
            return (TypeAffinity) Sqlite3.sqlite3_column_type(stmt._sqlite_stmt, index);
        }

        internal override string ColumnType(SqliteStatement stmt, int index, out TypeAffinity nAffinity)
        {
            int len;
#if !SQLITE_STANDARD
            IntPtr p = Sqlite3.sqlite3_column_decltype_interop(stmt._sqlite_stmt, index, out len);
#else
            len = -1;
            string p = Sqlite3.sqlite3_column_decltype(stmt._sqlite_stmt, index);
#endif
            nAffinity = ColumnAffinity(stmt, index);

            if (p != null) return p;
            else
            {
                string[] ar = stmt.TypeDefinitions;
                if (ar != null)
                {
                    if (index < ar.Length && ar[index] != null)
                        return ar[index];
                }
                return String.Empty;

                //switch (nAffinity)
                //{
                //  case TypeAffinity.Int64:
                //    return "BIGINT";
                //  case TypeAffinity.Double:
                //    return "DOUBLE";
                //  case TypeAffinity.Blob:
                //    return "BLOB";
                //  default:
                //    return "TEXT";
                //}
            }
        }

        internal override int ColumnIndex(SqliteStatement stmt, string columnName)
        {
            int x = ColumnCount(stmt);

            for (int n = 0; n < x; n++)
            {
                if (String.Compare(columnName, ColumnName(stmt, n), true, CultureInfo.InvariantCulture) == 0)
                    return n;
            }
            return -1;
        }

        internal override string ColumnOriginalName(SqliteStatement stmt, int index)
        {
#if !SQLITE_STANDARD
            int len;
            return UTF8ToString(Sqlite3.sqlite3_column_origin_name_interop(stmt._sqlite_stmt, index, out len), len);
#else
            //todo:return Sqlite3.sqlite3_column_origin_name(stmt._sqlite_stmt, index);
            return Sqlite3.sqlite3_column_name(stmt._sqlite_stmt, index);
#endif
        }

        internal override string ColumnDatabaseName(SqliteStatement stmt, int index)
        {
#if !SQLITE_STANDARD
            int len;
            return UTF8ToString(Sqlite3.sqlite3_column_database_name_interop(stmt._sqlite_stmt, index, out len), len);
#else
            //todo:return Sqlite3.sqlite3_column_database_name(stmt._sqlite_stmt, index);
            return Sqlite3.sqlite3_column_name(stmt._sqlite_stmt, index);
#endif
        }

        internal override string ColumnTableName(SqliteStatement stmt, int index)
        {
#if !SQLITE_STANDARD
            int len;
            return UTF8ToString(Sqlite3.sqlite3_column_table_name_interop(stmt._sqlite_stmt, index, out len), len);
#else
            //todo:return Sqlite3.sqlite3_column_table_name(stmt._sqlite_stmt, index);
            return Sqlite3.sqlite3_column_name(stmt._sqlite_stmt, index);
#endif
        }

        internal override void ColumnMetaData(string dataBase, string table, string column, out string dataType,
                                              out string collateSequence, out bool notNull, out bool primaryKey,
                                              out bool autoIncrement)
        {
            string dataTypePtr = null;
            string collSeqPtr = null;
            int nnotNull = 0;
            int nprimaryKey = 0;
            int nautoInc = 0;
            int n;
            int dtLen;
            int csLen;

#if !SQLITE_STANDARD
            n = Sqlite3.sqlite3_table_column_metadata_interop(_sql, ToUTF8(dataBase), ToUTF8(table), ToUTF8(column), out dataTypePtr, out collSeqPtr, out nnotNull, out nprimaryKey, out nautoInc, out dtLen, out csLen);
#else
            dtLen = -1;
            csLen = -1;
            n = Sqlite3.sqlite3_table_column_metadata(_sql, dataBase, table, column, ref dataTypePtr, ref collSeqPtr,
                                                      ref nnotNull, ref nprimaryKey, ref nautoInc);
#endif
            if (n > 0) throw new SqliteException(n, SQLiteLastError());

            dataType = dataTypePtr;
            collateSequence = collSeqPtr;

            notNull = (nnotNull == 1);
            primaryKey = (nprimaryKey == 1);
            autoIncrement = (nautoInc == 1);
        }

        internal override double GetDouble(SqliteStatement stmt, int index)
        {
            double value;
#if !PLATFORM_COMPACTFRAMEWORK
            value = Sqlite3.sqlite3_column_double(stmt._sqlite_stmt, index);
#else
      Sqlite3.sqlite3_column_double_interop(stmt._sqlite_stmt, index, out value);
#endif
            return value;
        }

        internal override int GetInt32(SqliteStatement stmt, int index)
        {
            return Sqlite3.sqlite3_column_int(stmt._sqlite_stmt, index);
        }

        internal override long GetInt64(SqliteStatement stmt, int index)
        {
            long value;
#if !PLATFORM_COMPACTFRAMEWORK
            value = Sqlite3.sqlite3_column_int64(stmt._sqlite_stmt, index);
#else
      Sqlite3.sqlite3_column_int64_interop(stmt._sqlite_stmt, index, out value);
#endif
            return value;
        }

        internal override string GetText(SqliteStatement stmt, int index)
        {
#if !SQLITE_STANDARD
            int len;
            return UTF8ToString(Sqlite3.sqlite3_column_text_interop(stmt._sqlite_stmt, index, out len), len);
#else
            return Sqlite3.sqlite3_column_text(stmt._sqlite_stmt, index);
#endif
        }

        internal override DateTime GetDateTime(SqliteStatement stmt, int index)
        {
#if !SQLITE_STANDARD
            int len;
            return ToDateTime(Sqlite3.sqlite3_column_text_interop(stmt._sqlite_stmt, index, out len), len);
#else
            return ToDateTime(Sqlite3.sqlite3_column_text(stmt._sqlite_stmt, index));
#endif
        }

        internal override long GetBytes(SqliteStatement stmt, int index, int nDataOffset, byte[] bDest, int nStart,
                                        int nLength)
        {
            byte[] ptr;
            int nlen;
            int nCopied = nLength;

            nlen = Sqlite3.sqlite3_column_bytes(stmt._sqlite_stmt, index);
            ptr = Sqlite3.sqlite3_column_blob(stmt._sqlite_stmt, index);

            if (bDest == null) return nlen;

            if (nCopied + nStart > bDest.Length) nCopied = bDest.Length - nStart;
            if (nCopied + nDataOffset > nlen) nCopied = nlen - nDataOffset;

            {
                if (nCopied > 0)
                    Array.Copy(ptr, bDest, nCopied);
                else nCopied = 0;
            }

            return nCopied;
        }

        internal override long GetChars(SqliteStatement stmt, int index, int nDataOffset, char[] bDest, int nStart,
                                        int nLength)
        {
            int nlen;
            int nCopied = nLength;

            string str = GetText(stmt, index);
            nlen = str.Length;

            if (bDest == null) return nlen;

            if (nCopied + nStart > bDest.Length) nCopied = bDest.Length - nStart;
            if (nCopied + nDataOffset > nlen) nCopied = nlen - nDataOffset;

            if (nCopied > 0)
                str.CopyTo(nDataOffset, bDest, nStart, nCopied);
            else nCopied = 0;

            return nCopied;
        }

        internal override bool IsNull(SqliteStatement stmt, int index)
        {
            return (ColumnAffinity(stmt, index) == TypeAffinity.Null);
        }

        // todo deprecated
        //internal override int AggregateCount(Sqlite3.sqlite3_context context)
        //{
        //    return Sqlite3.sqlite3_aggregate_count(context);
        //}

        internal override void CreateFunction(string strFunction, int nArgs, bool needCollSeq, Sqlite3.dxFunc func,
                                              Sqlite3.dxStep funcstep, Sqlite3.dxFinal funcfinal)
        {
            int n;

#if !SQLITE_STANDARD
            n = Sqlite3.sqlite3_create_function_interop(_sql, ToUTF8(strFunction), nArgs, 4, IntPtr.Zero, func, funcstep, funcfinal, (needCollSeq == true) ? 1 : 0);
            if (n == 0) n = Sqlite3.sqlite3_create_function_interop(_sql, ToUTF8(strFunction), nArgs, 1, IntPtr.Zero, func, funcstep, funcfinal, (needCollSeq == true) ? 1 : 0);
#else
            n = Sqlite3.sqlite3_create_function(_sql, strFunction, nArgs, 4, IntPtr.Zero, func, funcstep, funcfinal);
            if (n == 0)
                n = Sqlite3.sqlite3_create_function(_sql, strFunction, nArgs, 1, IntPtr.Zero, func, funcstep, funcfinal);
#endif
            if (n > 0) throw new SqliteException(n, SQLiteLastError());
        }

        internal override void CreateCollation(string strCollation, Sqlite3.dxCompare func, Sqlite3.dxCompare func16)
        {
            int n = Sqlite3.sqlite3_create_collation(_sql, strCollation, 2, IntPtr.Zero, func16);
            if (n == 0) Sqlite3.sqlite3_create_collation(_sql, strCollation, 1, IntPtr.Zero, func);
            if (n > 0) throw new SqliteException(n, SQLiteLastError());
        }

        internal override int ContextCollateCompare(CollationEncodingEnum enc, Sqlite3.sqlite3_context context,
                                                    string s1, string s2)
        {
#if !SQLITE_STANDARD
            byte[] b1;
            byte[] b2;
            System.Text.Encoding converter = null;

            switch (enc)
            {
                case CollationEncodingEnum.UTF8:
                    converter = System.Text.Encoding.UTF8;
                    break;
                case CollationEncodingEnum.UTF16LE:
                    converter = System.Text.Encoding.Unicode;
                    break;
                case CollationEncodingEnum.UTF16BE:
                    converter = System.Text.Encoding.BigEndianUnicode;
                    break;
            }

            b1 = converter.GetBytes(s1);
            b2 = converter.GetBytes(s2);

            return Sqlite3.sqlite3_context_collcompare(context, b1, b1.Length, b2, b2.Length);
#else
            throw new NotImplementedException();
#endif
        }

        internal override int ContextCollateCompare(CollationEncodingEnum enc, Sqlite3.sqlite3_context context,
                                                    char[] c1, char[] c2)
        {
#if !SQLITE_STANDARD
            byte[] b1;
            byte[] b2;
            System.Text.Encoding converter = null;

            switch (enc)
            {
                case CollationEncodingEnum.UTF8:
                    converter = System.Text.Encoding.UTF8;
                    break;
                case CollationEncodingEnum.UTF16LE:
                    converter = System.Text.Encoding.Unicode;
                    break;
                case CollationEncodingEnum.UTF16BE:
                    converter = System.Text.Encoding.BigEndianUnicode;
                    break;
            }

            b1 = converter.GetBytes(c1);
            b2 = converter.GetBytes(c2);

            return Sqlite3.sqlite3_context_collcompare(context, b1, b1.Length, b2, b2.Length);
#else
            throw new NotImplementedException();
#endif
        }

        internal override CollationSequence GetCollationSequence(SqliteFunction func, Sqlite3.sqlite3_context context)
        {
#if !SQLITE_STANDARD
            CollationSequence seq = new CollationSequence();
            int len;
            int type;
            int enc;
            IntPtr p = Sqlite3.sqlite3_context_collseq(context, out type, out enc, out len);

            if (p != null) seq.Name = UTF8ToString(p, len);
            seq.Type = (CollationTypeEnum)type;
            seq._func = func;
            seq.Encoding = (CollationEncodingEnum)enc;

            return seq;
#else
            throw new NotImplementedException();
#endif
        }

        internal override long GetParamValueBytes(Sqlite3.Mem p, int nDataOffset, byte[] bDest, int nStart, int nLength)
        {
            byte[] ptr;
            int nlen;
            int nCopied = nLength;

            nlen = Sqlite3.sqlite3_value_bytes(p);
            ptr = Sqlite3.sqlite3_value_blob(p);

            if (bDest == null) return nlen;

            if (nCopied + nStart > bDest.Length) nCopied = bDest.Length - nStart;
            if (nCopied + nDataOffset > nlen) nCopied = nlen - nDataOffset;

            {
                if (nCopied > 0)
                    Array.Copy(ptr, bDest, nCopied);
                else nCopied = 0;
            }

            return nCopied;
        }

        internal override double GetParamValueDouble(Sqlite3.Mem ptr)
        {
            double value;
#if !PLATFORM_COMPACTFRAMEWORK
            value = Sqlite3.sqlite3_value_double(ptr);
#else
      Sqlite3.sqlite3_value_double_interop(ptr, out value);
#endif
            return value;
        }

        internal override int GetParamValueInt32(Sqlite3.Mem ptr)
        {
            return Sqlite3.sqlite3_value_int(ptr);
        }

        internal override long GetParamValueInt64(Sqlite3.Mem ptr)
        {
            Int64 value;
#if !PLATFORM_COMPACTFRAMEWORK
            value = Sqlite3.sqlite3_value_int64(ptr);
#else
      Sqlite3.sqlite3_value_int64_interop(ptr, out value);
#endif
            return value;
        }

        internal override string GetParamValueText(Sqlite3.Mem ptr)
        {
#if !SQLITE_STANDARD
            int len;
            return UTF8ToString(Sqlite3.sqlite3_value_text_interop(ptr, out len), len);
#else
            return Sqlite3.sqlite3_value_text(ptr);
#endif
        }

        internal override TypeAffinity GetParamValueType(Sqlite3.Mem ptr)
        {
            return (TypeAffinity) Sqlite3.sqlite3_value_type(ptr);
        }

        internal override void ReturnBlob(Sqlite3.sqlite3_context context, byte[] value)
        {
            // todo : strings <-> byte[]
            var str = new System.Text.UnicodeEncoding().GetString(value);
            Sqlite3.sqlite3_result_blob(context, str, str.Length, null);
        }

        internal override void ReturnDouble(Sqlite3.sqlite3_context context, double value)
        {
#if !PLATFORM_COMPACTFRAMEWORK
            Sqlite3.sqlite3_result_double(context, value);
#else
      Sqlite3.sqlite3_result_double_interop(context, ref value);
#endif
        }

        internal override void ReturnError(Sqlite3.sqlite3_context context, string value)
        {
            Sqlite3.sqlite3_result_error(context, value, value.Length);
        }

        internal override void ReturnInt32(Sqlite3.sqlite3_context context, int value)
        {
            Sqlite3.sqlite3_result_int(context, value);
        }

        internal override void ReturnInt64(Sqlite3.sqlite3_context context, long value)
        {
#if !PLATFORM_COMPACTFRAMEWORK
            Sqlite3.sqlite3_result_int64(context, value);
#else
      Sqlite3.sqlite3_result_int64_interop(context, ref value);
#endif
        }

        internal override void ReturnNull(Sqlite3.sqlite3_context context)
        {
            Sqlite3.sqlite3_result_null(context);
        }

        internal override void ReturnText(Sqlite3.sqlite3_context context, string value)
        {
            Sqlite3.sqlite3_result_text(context, value, value.Length, null);
        }

        internal override Sqlite3.Mem AggregateContext(Sqlite3.sqlite3_context context)
        {
            return Sqlite3.sqlite3_aggregate_context(context, 1);
        }

        internal override void SetPassword(string passwordBytes)
        {
            int n = Sqlite3.sqlite3_key(_sql, passwordBytes, passwordBytes.Length);
            if (n > 0) throw new SqliteException(n, SQLiteLastError());
        }

        internal override void ChangePassword(string newPasswordBytes)
        {
            int n = Sqlite3.sqlite3_rekey(_sql, newPasswordBytes,
                                          (newPasswordBytes == null) ? 0 : newPasswordBytes.Length);
            if (n > 0) throw new SqliteException(n, SQLiteLastError());
        }

#if MONOTOUCH
    SQLiteUpdateCallback update_callback;
    SQLiteCommitCallback commit_callback;
    SQLiteRollbackCallback rollback_callback;
		
    [MonoTouch.MonoPInvokeCallback (typeof (SQLiteUpdateCallback))]
    static void update (IntPtr puser, int type, IntPtr database, IntPtr table, Int64 rowid)
    {
      SQLite3 instance = GCHandle.FromIntPtr (puser).Target as SQLite3;
      instance.update_callback (puser, type, database, table, rowid);
    }
			
    internal override void SetUpdateHook (SQLiteUpdateCallback func)
    {
      update_callback = func;
      if (func == null)
        Sqlite3.sqlite3_update_hook (_sql, null, IntPtr.Zero);
      else
        Sqlite3.sqlite3_update_hook (_sql, update, GCHandle.ToIntPtr (gch));
    }

    [MonoTouch.MonoPInvokeCallback (typeof (SQLiteCommitCallback))]
    static int commit (IntPtr puser)
    {
      SQLite3 instance = GCHandle.FromIntPtr (puser).Target as SQLite3;
      return instance.commit_callback (puser);
    }
		
    internal override void SetCommitHook (SQLiteCommitCallback func)
    {
      commit_callback = func;
      if (func == null)
        Sqlite3.sqlite3_commit_hook (_sql, null, IntPtr.Zero);
      else
        Sqlite3.sqlite3_commit_hook (_sql, commit, GCHandle.ToIntPtr (gch));
    }

    [MonoTouch.MonoPInvokeCallback (typeof (SQLiteRollbackCallback))]
    static void rollback (IntPtr puser)
    {
      SQLite3 instance = GCHandle.FromIntPtr (puser).Target as SQLite3;
      instance.rollback_callback (puser);
    }

    internal override void SetRollbackHook (SQLiteRollbackCallback func)
    {
      rollback_callback = func;
      if (func == null)
        Sqlite3.sqlite3_rollback_hook (_sql, null, IntPtr.Zero);
      else
        Sqlite3.sqlite3_rollback_hook (_sql, rollback, GCHandle.ToIntPtr (gch));
    }
#else
        internal override void SetUpdateHook(Sqlite3.dxUpdateCallback func)
        {
            Sqlite3.sqlite3_update_hook(_sql, func, IntPtr.Zero);
        }

        internal override void SetCommitHook(Sqlite3.dxCommitCallback func)
        {
            Sqlite3.sqlite3_commit_hook(_sql, func, IntPtr.Zero);
        }

        internal override void SetRollbackHook(Sqlite3.dxRollbackCallback func)
        {
            Sqlite3.sqlite3_rollback_hook(_sql, func, IntPtr.Zero);
        }
#endif

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
                t = SqliteConvert.SQLiteTypeToType(typ);
                aff = TypeToAffinity(t);
            }

            switch (aff)
            {
                case TypeAffinity.Blob:
                    if (typ.Type == DbType.Guid && typ.Affinity == TypeAffinity.Text)
                        return new Guid(GetText(stmt, index));

                    int n = (int) GetBytes(stmt, index, 0, null, 0, 0);
                    byte[] b = new byte[n];
                    GetBytes(stmt, index, 0, b, 0, n);

                    if (typ.Type == DbType.Guid && n == 16)
                        return new Guid(b);

                    return b;
                case TypeAffinity.DateTime:
                    return GetDateTime(stmt, index);
                case TypeAffinity.Double:
                    if (t == null) return GetDouble(stmt, index);
                    else
                        return Convert.ChangeType(GetDouble(stmt, index), t, null);
                case TypeAffinity.Int64:
                    if (t == null) return GetInt64(stmt, index);
                    else
                        return Convert.ChangeType(GetInt64(stmt, index), t, null);
                default:
                    return GetText(stmt, index);
            }
        }

        internal override int GetCursorForTable(SqliteStatement stmt, int db, int rootPage)
        {
#if !SQLITE_STANDARD
            return Sqlite3.sqlite3_table_cursor(stmt._sqlite_stmt, db, rootPage);
#else
            return -1;
#endif
        }

        internal override long GetRowIdForCursor(SqliteStatement stmt, int cursor)
        {
#if !SQLITE_STANDARD
            long rowid;
            int rc = Sqlite3.sqlite3_cursor_rowid(stmt._sqlite_stmt, cursor, out rowid);
            if (rc == 0) return rowid;

            return 0;
#else
            return 0;
#endif
        }

        internal override void GetIndexColumnExtendedInfo(string database, string index, string column, out int sortMode,
                                                          out int onError, out string collationSequence)
        {
#if !SQLITE_STANDARD
            IntPtr coll;
            int colllen;
            int rc;

            rc = Sqlite3.sqlite3_index_column_info_interop(_sql, ToUTF8(database), ToUTF8(index), ToUTF8(column), out sortMode, out onError, out coll, out colllen);
            if (rc != 0) throw new SqliteException(rc, "");

            collationSequence = UTF8ToString(coll, colllen);
#else
            sortMode = 0;
            onError = 2;
            collationSequence = "BINARY";
#endif
        }
    }
}