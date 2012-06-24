/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace Mono.Data.Sqlite
{
    using System;
    using System.Runtime.InteropServices;
    using Sqlite3 = Community.CsharpSqlite.Sqlite3;

    /// <summary>
    /// Alternate SQLite3 object, overriding many text behaviors to support UTF-16 (Unicode)
    /// </summary>
    internal class SQLite3_UTF16 : SQLite3
    {
        internal SQLite3_UTF16(SQLiteDateFormats fmt)
            : base(fmt)
        {
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
                int n = Sqlite3.sqlite3_open16_interop(ToUTF8(strFilename), (int)flags, out db);
#else
                if ((flags & SQLiteOpenFlagsEnum.Create) == 0 && System.IO.File.Exists(strFilename) == false)
                    throw new SqliteException((int) SQLiteErrorCode.CantOpen, strFilename);

                int n = Sqlite3.sqlite3_open(strFilename, out db);
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
            int n = Sqlite3.sqlite3_bind_text(stmt._sqlite_stmt, index, value, value.Length*2, null);
            if (n > 0) throw new SqliteException(n, SQLiteLastError());
        }

        internal override DateTime GetDateTime(SqliteStatement stmt, int index)
        {
            return ToDateTime(GetText(stmt, index));
        }

        internal override string ColumnName(SqliteStatement stmt, int index)
        {
#if !SQLITE_STANDARD
            int len;
            return UTF16ToString(Sqlite3.sqlite3_column_name16_interop(stmt._sqlite_stmt, index, out len), len);
#else
            return Sqlite3.sqlite3_column_name(stmt._sqlite_stmt, index);
#endif
        }

        internal override string GetText(SqliteStatement stmt, int index)
        {
#if !SQLITE_STANDARD
            int len;
            return UTF16ToString(Sqlite3.sqlite3_column_text16_interop(stmt._sqlite_stmt, index, out len), len);
#else
            return Sqlite3.sqlite3_column_text(stmt._sqlite_stmt, index);
#endif
        }

        internal override string ColumnOriginalName(SqliteStatement stmt, int index)
        {
#if !SQLITE_STANDARD
            int len;
            return UTF16ToString(Sqlite3.sqlite3_column_origin_name16_interop(stmt._sqlite_stmt, index, out len), len);
#else
            return Sqlite3.sqlite3_column_origin_name(stmt._sqlite_stmt, index);
#endif
        }

        internal override string ColumnDatabaseName(SqliteStatement stmt, int index)
        {
#if !SQLITE_STANDARD
            int len;
            return UTF16ToString(Sqlite3.sqlite3_column_database_name16_interop(stmt._sqlite_stmt, index, out len), len);
#else
            return Sqlite3.sqlite3_column_database_name(stmt._sqlite_stmt, index);
#endif
        }

        internal override string ColumnTableName(SqliteStatement stmt, int index)
        {
#if !SQLITE_STANDARD
            int len;
            return UTF16ToString(Sqlite3.sqlite3_column_table_name16_interop(stmt._sqlite_stmt, index, out len), len);
#else
            return Sqlite3.sqlite3_column_table_name(stmt._sqlite_stmt, index);
#endif
        }

        internal override string GetParamValueText(Sqlite3.Mem ptr)
        {
#if !SQLITE_STANDARD
            int len;
            return UTF16ToString(Sqlite3.sqlite3_value_text16_interop(ptr, out len), len);
#else
            return Sqlite3.sqlite3_value_text(ptr);
#endif
        }

        internal override void ReturnError(Sqlite3.sqlite3_context context, string value)
        {
            Sqlite3.sqlite3_result_error(context, value, value.Length*2);
        }

        internal override void ReturnText(Sqlite3.sqlite3_context context, string value)
        {
            Sqlite3.sqlite3_result_text(context, value, value.Length*2, null);
        }
    }
}