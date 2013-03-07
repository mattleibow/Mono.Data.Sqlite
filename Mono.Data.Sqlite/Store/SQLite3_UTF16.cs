/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace Mono.Data.Sqlite
{
    using System;
#if SILVERLIGHT
    using SqliteConnectionHandle = Community.CsharpSqlite.Sqlite3.sqlite3;
    using UnsafeNativeMethods = Community.CsharpSqlite.Sqlite3;
    using SqliteConnectionHandle = Community.CsharpSqlite.Sqlite3.sqlite3;
    using SqliteValueHandle = Community.CsharpSqlite.Sqlite3.Mem;
    using SqliteValueHandle = Community.CsharpSqlite.Sqlite3.Mem;
    using SqliteStatementHandle = Community.CsharpSqlite.Sqlite3.Vdbe;
    using SQLiteUpdateCallback = Community.CsharpSqlite.Sqlite3.dxUpdateCallback;
    using SQLiteCommitCallback = Community.CsharpSqlite.Sqlite3.dxCommitCallback;
    using SQLiteRollbackCallback = Community.CsharpSqlite.Sqlite3.dxRollbackCallback;
    using SQLiteFinalCallback = Community.CsharpSqlite.Sqlite3.dxFinal;
    using SQLiteCallback = Community.CsharpSqlite.Sqlite3.dxFunc;
    using SQLiteStepCallback = Community.CsharpSqlite.Sqlite3.dxStep;
    using SQLiteCollation = Community.CsharpSqlite.Sqlite3.dxCompare;
    using SqliteContextHandle = Community.CsharpSqlite.Sqlite3.sqlite3_context;
#else
    using MonoDataSqliteWrapper;
    using System.Runtime.InteropServices;
#endif

    /// <summary>
    /// Alternate SQLite3 object, overriding many text behaviors to support UTF-16 (Unicode)
    /// </summary>
    internal class SQLite3_UTF16 : SQLite3
    {
        internal SQLite3_UTF16(SQLiteDateFormats fmt)
            : base(fmt)
        {
        }

        /// <summary>
        /// Overrides SqliteConvert.ToString() to marshal UTF-16 strings instead of UTF-8
        /// </summary>
        /// <param name="b">A pointer to a UTF-16 string</param>
        /// <param name="nbytelen">The length (IN BYTES) of the string</param>
        public static string UTF16ToString(string p, int nbytelen)
        {
            return p;
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
                SqliteConnectionHandle db;

                if ((flags & SQLiteOpenFlagsEnum.Create) == 0 && FileExists(strFilename) == false)
                {
                    throw new SqliteException((int) SQLiteErrorCode.CantOpen, strFilename);
                }

#if SILVERLIGHT
                int n = UnsafeNativeMethods.sqlite3_open(strFilename, out db);
#else
                int n = UnsafeNativeMethods.sqlite3_open16(strFilename, out db);
#endif
                if (n > 0) throw new SqliteException(n, null);

                _sql = db;
            }

            _functionsArray = SqliteFunction.BindFunctions(this);
        }

        internal override void Bind_DateTime(SqliteStatement stmt, int index, DateTime dt)
        {
            Bind_Text(stmt, index, ToString(dt));
        }

        internal override void Bind_Text(SqliteStatement stmt, int index, string value)
        {
#if SILVERLIGHT
            int n = UnsafeNativeMethods.sqlite3_bind_text(stmt._sqlite_stmt, index, value, value.Length*2, NullPointer);
#else
            int n = UnsafeNativeMethods.sqlite3_bind_text16(stmt._sqlite_stmt, index, value, value.Length*2);
#endif
            if (n > 0) throw new SqliteException(n, SQLiteLastError());
        }

        internal override DateTime GetDateTime(SqliteStatement stmt, int index)
        {
            return ToDateTime(GetText(stmt, index));
        }

        internal override string ColumnName(SqliteStatement stmt, int index)
        {
#if SILVERLIGHT
            return UTF16ToString(UnsafeNativeMethods.sqlite3_column_name(stmt._sqlite_stmt, index), -1);
#else
            return UTF16ToString(UnsafeNativeMethods.sqlite3_column_name16(stmt._sqlite_stmt, index), -1);
#endif
        }

        internal override string GetText(SqliteStatement stmt, int index)
        {
#if SILVERLIGHT
            return UTF16ToString(UnsafeNativeMethods.sqlite3_column_text(stmt._sqlite_stmt, index), -1);
#else
            return UTF16ToString(UnsafeNativeMethods.sqlite3_column_text16(stmt._sqlite_stmt, index), -1);
#endif
        }

        internal override string ColumnOriginalName(SqliteStatement stmt, int index)
        {
#if SILVERLIGHT
            return UTF16ToString(UnsafeNativeMethods.sqlite3_column_origin_name(stmt._sqlite_stmt, index), -1);
#else
            return UTF16ToString(UnsafeNativeMethods.sqlite3_column_origin_name16(stmt._sqlite_stmt, index), -1);
#endif
        }

        internal override string ColumnDatabaseName(SqliteStatement stmt, int index)
        {
#if SILVERLIGHT
            return UTF16ToString(UnsafeNativeMethods.sqlite3_column_database_name(stmt._sqlite_stmt, index), -1);
#else
            return UTF16ToString(UnsafeNativeMethods.sqlite3_column_database_name16(stmt._sqlite_stmt, index), -1);
#endif
        }

        internal override string ColumnTableName(SqliteStatement stmt, int index)
        {
#if SILVERLIGHT
            return UTF16ToString(UnsafeNativeMethods.sqlite3_column_table_name(stmt._sqlite_stmt, index), -1);
#else
            return UTF16ToString(UnsafeNativeMethods.sqlite3_column_table_name16(stmt._sqlite_stmt, index), -1);
#endif
        }

        internal override string GetParamValueText(SqliteValueHandle ptr)
        {
#if SILVERLIGHT
            return UTF16ToString(UnsafeNativeMethods.sqlite3_value_text(ptr), -1);
#else
            return UTF16ToString(UnsafeNativeMethods.sqlite3_value_text16(ptr), -1);
#endif
        }

        internal override void ReturnError(SqliteContextHandle context, string value)
        {
#if SILVERLIGHT
            UnsafeNativeMethods.sqlite3_result_error(context, value, value.Length*2);
#else
            UnsafeNativeMethods.sqlite3_result_error16(context, value, value.Length*2);
#endif
        }

        internal override void ReturnText(SqliteContextHandle context, string value)
        {
#if SILVERLIGHT
            UnsafeNativeMethods.sqlite3_result_text(context, value, value.Length*2, NullPointer);
#else
            UnsafeNativeMethods.sqlite3_result_text16(context, value, value.Length*2);
#endif
        }
    }
}
