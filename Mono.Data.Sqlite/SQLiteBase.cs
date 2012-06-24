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
    using Community.CsharpSqlite;

    /// <summary>
    /// This internal class provides the foundation of SQLite support.  It defines all the abstract members needed to implement
    /// a SQLite data provider, and inherits from SqliteConvert which allows for simple translations of string to and from SQLite.
    /// </summary>
    internal abstract class SQLiteBase : SqliteConvert, IDisposable
    {
        internal SQLiteBase(SQLiteDateFormats fmt)
            : base(fmt)
        {
        }

        internal static object _lock = new object();

        /// <summary>
        /// Returns a string representing the active version of SQLite
        /// </summary>
        internal abstract string Version { get; }

        /// <summary>
        /// Returns the number of changes the last executing insert/update caused.
        /// </summary>
        internal abstract int Changes { get; }

        /// <summary>
        /// Opens a database.
        /// </summary>
        /// <remarks>
        /// Implementers should call SqliteFunction.BindFunctions() and save the array after opening a connection
        /// to bind all attributed user-defined functions and collating sequences to the new connection.
        /// </remarks>
        /// <param name="strFilename">The filename of the database to open.  SQLite automatically creates it if it doesn't exist.</param>
        /// <param name="flags">The open flags to use when creating the connection</param>
        /// <param name="maxPoolSize">The maximum size of the pool for the given filename</param>
        /// <param name="usePool">If true, the connection can be pulled from the connection pool</param>
        internal abstract void Open(string strFilename, SQLiteOpenFlagsEnum flags, int maxPoolSize, bool usePool);

        /// <summary>
        /// Closes the currently-open database.
        /// </summary>
        /// <remarks>
        /// After the database has been closed implemeters should call SqliteFunction.UnbindFunctions() to deallocate all interop allocated
        /// memory associated with the user-defined functions and collating sequences tied to the closed connection.
        /// </remarks>
        internal abstract void Close();

        /// <summary>
        /// Sets the busy timeout on the connection.  SqliteCommand will call this before executing any command.
        /// </summary>
        /// <param name="nTimeoutMS">The number of milliseconds to wait before returning SQLITE_BUSY</param>
        internal abstract void SetTimeout(int nTimeoutMS);

        /// <summary>
        /// Returns the text of the last error issued by SQLite
        /// </summary>
        /// <returns></returns>
        internal abstract string SQLiteLastError();

        /// <summary>
        /// When pooling is enabled, force this connection to be disposed rather than returned to the pool
        /// </summary>
        internal abstract void ClearPool();

        /// <summary>
        /// Prepares a SQL statement for execution.
        /// </summary>
        /// <param name="cnn">The source connection preparing the command.  Can be null for any caller except LINQ</param>
        /// <param name="strSql">The SQL command text to prepare</param>
        /// <param name="previous">The previous statement in a multi-statement command, or null if no previous statement exists</param>
        /// <param name="timeoutMS">The timeout to wait before aborting the prepare</param>
        /// <param name="strRemain">The remainder of the statement that was not processed.  Each call to prepare parses the
        /// SQL up to to either the end of the text or to the first semi-colon delimiter.  The remaining text is returned
        /// here for a subsequent call to Prepare() until all the text has been processed.</param>
        /// <returns>Returns an initialized SqliteStatement.</returns>
        internal abstract SqliteStatement Prepare(SqliteConnection cnn, string strSql, SqliteStatement previous,
                                                  uint timeoutMS, out string strRemain);

        /// <summary>
        /// Steps through a prepared statement.
        /// </summary>
        /// <param name="stmt">The SqliteStatement to step through</param>
        /// <returns>True if a row was returned, False if not.</returns>
        internal abstract bool Step(SqliteStatement stmt);

        /// <summary>
        /// Resets a prepared statement so it can be executed again.  If the error returned is SQLITE_SCHEMA, 
        /// transparently attempt to rebuild the SQL statement and throw an error if that was not possible.
        /// </summary>
        /// <param name="stmt">The statement to reset</param>
        /// <returns>Returns -1 if the schema changed while resetting, 0 if the reset was sucessful or 6 (SQLITE_LOCKED) if the reset failed due to a lock</returns>
        internal abstract int Reset(SqliteStatement stmt);

        internal abstract void Cancel();

        internal abstract void Bind_Double(SqliteStatement stmt, int index, double value);
        internal abstract void Bind_Int32(SqliteStatement stmt, int index, Int32 value);
        internal abstract void Bind_Int64(SqliteStatement stmt, int index, Int64 value);
        internal abstract void Bind_Text(SqliteStatement stmt, int index, string value);
        internal abstract void Bind_Blob(SqliteStatement stmt, int index, byte[] blobData);
        internal abstract void Bind_DateTime(SqliteStatement stmt, int index, DateTime dt);
        internal abstract void Bind_Null(SqliteStatement stmt, int index);

        internal abstract int Bind_ParamCount(SqliteStatement stmt);
        internal abstract string Bind_ParamName(SqliteStatement stmt, int index);
        internal abstract int Bind_ParamIndex(SqliteStatement stmt, string paramName);

        internal abstract int ColumnCount(SqliteStatement stmt);
        internal abstract string ColumnName(SqliteStatement stmt, int index);
        internal abstract TypeAffinity ColumnAffinity(SqliteStatement stmt, int index);
        internal abstract string ColumnType(SqliteStatement stmt, int index, out TypeAffinity nAffinity);
        internal abstract int ColumnIndex(SqliteStatement stmt, string columnName);
        internal abstract string ColumnOriginalName(SqliteStatement stmt, int index);
        internal abstract string ColumnDatabaseName(SqliteStatement stmt, int index);
        internal abstract string ColumnTableName(SqliteStatement stmt, int index);

        internal abstract void ColumnMetaData(string dataBase, string table, string column, out string dataType,
                                              out string collateSequence, out bool notNull, out bool primaryKey,
                                              out bool autoIncrement);

        internal abstract void GetIndexColumnExtendedInfo(string database, string index, string column, out int sortMode,
                                                          out int onError, out string collationSequence);

        internal abstract double GetDouble(SqliteStatement stmt, int index);
        internal abstract Int32 GetInt32(SqliteStatement stmt, int index);
        internal abstract Int64 GetInt64(SqliteStatement stmt, int index);
        internal abstract string GetText(SqliteStatement stmt, int index);

        internal abstract long GetBytes(SqliteStatement stmt, int index, int nDataoffset, byte[] bDest, int nStart,
                                        int nLength);

        internal abstract long GetChars(SqliteStatement stmt, int index, int nDataoffset, char[] bDest, int nStart,
                                        int nLength);

        internal abstract DateTime GetDateTime(SqliteStatement stmt, int index);
        internal abstract bool IsNull(SqliteStatement stmt, int index);

        internal abstract void CreateCollation(string strCollation, Sqlite3.dxCompare func, Sqlite3.dxCompare func16);

        internal abstract void CreateFunction(string strFunction, int nArgs, bool needCollSeq, Sqlite3.dxFunc dxCallback,
                                              Sqlite3.dxStep funcstep, Sqlite3.dxFinal funcfinal);

        internal abstract CollationSequence GetCollationSequence(SqliteFunction func, Sqlite3.sqlite3_context context);

        internal abstract int ContextCollateCompare(CollationEncodingEnum enc, Sqlite3.sqlite3_context context,
                                                    string s1, string s2);

        internal abstract int ContextCollateCompare(CollationEncodingEnum enc, Sqlite3.sqlite3_context context,
                                                    char[] c1, char[] c2);

        // todo deprecated
        // internal abstract int AggregateCount(Sqlite3.sqlite3_context context);
        internal abstract Sqlite3.Mem AggregateContext(Sqlite3.sqlite3_context context);

        internal abstract long GetParamValueBytes(Sqlite3.Mem ptr, int nDataOffset, byte[] bDest, int nStart,
                                                  int nLength);

        internal abstract double GetParamValueDouble(Sqlite3.Mem ptr);
        internal abstract int GetParamValueInt32(Sqlite3.Mem ptr);
        internal abstract long GetParamValueInt64(Sqlite3.Mem ptr);
        internal abstract string GetParamValueText(Sqlite3.Mem ptr);
        internal abstract TypeAffinity GetParamValueType(Sqlite3.Mem ptr);

        internal abstract void ReturnBlob(Sqlite3.sqlite3_context context, byte[] value);
        internal abstract void ReturnDouble(Sqlite3.sqlite3_context context, double value);
        internal abstract void ReturnError(Sqlite3.sqlite3_context context, string value);
        internal abstract void ReturnInt32(Sqlite3.sqlite3_context context, Int32 value);
        internal abstract void ReturnInt64(Sqlite3.sqlite3_context context, Int64 value);
        internal abstract void ReturnNull(Sqlite3.sqlite3_context context);
        internal abstract void ReturnText(Sqlite3.sqlite3_context context, string value);

        internal abstract void SetPassword(string passwordBytes);
        internal abstract void ChangePassword(string newPasswordBytes);

        internal abstract void SetUpdateHook(Sqlite3.dxUpdateCallback func);
        internal abstract void SetCommitHook(Sqlite3.dxCommitCallback func);
        internal abstract void SetRollbackHook(Sqlite3.dxRollbackCallback func);

        internal abstract int GetCursorForTable(SqliteStatement stmt, int database, int rootPage);
        internal abstract long GetRowIdForCursor(SqliteStatement stmt, int cursor);

        internal abstract object GetValue(SqliteStatement stmt, int index, SQLiteType typ);

        protected virtual void Dispose(bool bDisposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
        }

        // These statics are here for lack of a better place to put them.
        // They exist here because they are called during the finalization of
        // a Sqlite3.Vdbe, Sqlite3.sqlite3, and SqliteFunctionCookieHandle.
        // Therefore these functions have to be static, and have to be low-level.

        internal static string SQLiteLastError(Sqlite3.sqlite3 db)
        {
#if !SQLITE_STANDARD
            int len;
            return UTF8ToString(Sqlite3.sqlite3_errmsg_interop(db, out len), len);
#else
            return Sqlite3.sqlite3_errmsg(db);
#endif
        }

        internal static void FinalizeStatement(Sqlite3.Vdbe stmt)
        {
            lock (_lock)
            {
#if !SQLITE_STANDARD
                int n = Sqlite3.sqlite3_finalize_interop(stmt);
#else
                int n = Sqlite3.sqlite3_finalize(stmt);
#endif
                if (n > 0) throw new SqliteException(n, null);
            }
        }

        internal static void CloseConnection(Sqlite3.sqlite3 db)
        {
            lock (_lock)
            {
#if !SQLITE_STANDARD
                int n = Sqlite3.sqlite3_close_interop(db);
#else
                ResetConnection(db);
                int n = Sqlite3.sqlite3_close(db);
#endif
                if (n > 0) throw new SqliteException(n, SQLiteLastError(db));
            }
        }

        internal static void ResetConnection(Sqlite3.sqlite3 db)
        {
            lock (_lock)
            {
                Sqlite3.Vdbe stmt = null;

                do
                {
                    stmt = Sqlite3.sqlite3_next_stmt(db, stmt);
                    if (stmt != null)
                    {
#if !SQLITE_STANDARD
                        Sqlite3.sqlite3_reset_interop(stmt);
#else
                        Sqlite3.sqlite3_reset(stmt);
#endif
                    }
                } while (stmt != null);

                // Not overly concerned with the return value from a rollback.
                string msg = null;
                Sqlite3.sqlite3_exec(db, "ROLLBACK", null, null, ref msg);
            }
        }
    }

    internal interface ISQLiteSchemaExtensions
    {
        void BuildTempSchema(SqliteConnection cnn);
    }

    [Flags]
    internal enum SQLiteOpenFlagsEnum
    {
        None = 0,
        ReadOnly = 0x01,
        ReadWrite = 0x02,
        Create = 0x04,
        //SharedCache = 0x01000000,
        Default = 0x06,

        // iOS Specific
        FileProtectionComplete = 0x00100000,
        FileProtectionCompleteUnlessOpen = 0x00200000,
        FileProtectionCompleteUntilFirstUserAuthentication = 0x00300000,
        FileProtectionNone = 0x00400000
    }

    // subset of the options available in http://www.sqlite.org/c3ref/c_config_getmalloc.html
    public enum SQLiteConfig
    {
        SingleThread = 1,
        MultiThread = 2,
        Serialized = 3,
    }

    internal static class Disposers
    {
        internal static void Dispose(this Sqlite3.Vdbe statement)
        {
            try
            {
                SQLiteBase.FinalizeStatement(statement);
            }
            catch (SqliteException)
            {
            }
        }

        internal static void Dispose(this Sqlite3.sqlite3 connection)
        {
            try
            {
                SQLiteBase.CloseConnection(connection);
            }
            catch (SqliteException)
            {
            }
        }

        internal static void Close(this Sqlite3.sqlite3 connection)
        {
            connection.Dispose();
        }
    }
}