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
    using System.Threading;

    internal static class UnsafeNativeMethods
    {
        private const string SQLITE_DLL = "sqlite3";

        #region standard versions of interop functions

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_close(IntPtr db);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_create_function(IntPtr db, byte[] strName, int nArgs, int nType,
                                                           IntPtr pvUser, SQLiteCallback func, SQLiteCallback fstep,
                                                           SQLiteFinalCallback ffinal);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_finalize(IntPtr stmt);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_open_v2(byte[] utf8Filename, out IntPtr db, int flags, IntPtr vfs);

        // Compatibility with versions < 3.5.0
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_open(byte[] utf8Filename, out IntPtr db);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        internal static extern int sqlite3_open16(string fileName, out IntPtr db);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_reset(IntPtr stmt);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sqlite3_bind_parameter_name(IntPtr stmt, int index);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sqlite3_column_database_name(IntPtr stmt, int index);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sqlite3_column_database_name16(IntPtr stmt, int index);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sqlite3_column_decltype(IntPtr stmt, int index);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sqlite3_column_decltype16(IntPtr stmt, int index);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sqlite3_column_name(IntPtr stmt, int index);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sqlite3_column_name16(IntPtr stmt, int index);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sqlite3_column_origin_name(IntPtr stmt, int index);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sqlite3_column_origin_name16(IntPtr stmt, int index);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sqlite3_column_table_name(IntPtr stmt, int index);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sqlite3_column_table_name16(IntPtr stmt, int index);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sqlite3_column_text(IntPtr stmt, int index);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sqlite3_column_text16(IntPtr stmt, int index);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sqlite3_errmsg(IntPtr db);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_prepare(IntPtr db, IntPtr pSql, int nBytes, out IntPtr stmt,
                                                   out IntPtr ptrRemain);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_table_column_metadata(IntPtr db, byte[] dbName, byte[] tblName,
                                                                 byte[] colName, out IntPtr ptrDataType,
                                                                 out IntPtr ptrCollSeq, out int notNull,
                                                                 out int primaryKey, out int autoInc);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sqlite3_value_text(IntPtr p);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sqlite3_value_text16(IntPtr p);

        #endregion

        #region standard sqlite api calls

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sqlite3_libversion();

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void sqlite3_interrupt(IntPtr db);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_changes(IntPtr db);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_busy_timeout(IntPtr db, int ms);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_bind_blob(IntPtr stmt, int index, Byte[] value, int nSize, IntPtr nTransient);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_bind_double(IntPtr stmt, int index, double value);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_bind_int(IntPtr stmt, int index, int value);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_bind_int64(IntPtr stmt, int index, long value);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_bind_null(IntPtr stmt, int index);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_bind_text(IntPtr stmt, int index, byte[] value, int nlen, IntPtr pvReserved);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_bind_parameter_count(IntPtr stmt);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_bind_parameter_index(IntPtr stmt, byte[] strName);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_column_count(IntPtr stmt);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_step(IntPtr stmt);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern double sqlite3_column_double(IntPtr stmt, int index);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_column_int(IntPtr stmt, int index);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern long sqlite3_column_int64(IntPtr stmt, int index);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sqlite3_column_blob(IntPtr stmt, int index);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_column_bytes(IntPtr stmt, int index);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern TypeAffinity sqlite3_column_type(IntPtr stmt, int index);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_create_collation(IntPtr db, byte[] strName, int nType, IntPtr pvUser,
                                                            SQLiteCollation func);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_aggregate_count(IntPtr context);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sqlite3_value_blob(IntPtr p);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_value_bytes(IntPtr p);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern double sqlite3_value_double(IntPtr p);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_value_int(IntPtr p);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern long sqlite3_value_int64(IntPtr p);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern TypeAffinity sqlite3_value_type(IntPtr p);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void sqlite3_result_blob(IntPtr context, byte[] value, int nSize, IntPtr pvReserved);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void sqlite3_result_double(IntPtr context, double value);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void sqlite3_result_error(IntPtr context, byte[] strErr, int nLen);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void sqlite3_result_int(IntPtr context, int value);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void sqlite3_result_int64(IntPtr context, long value);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void sqlite3_result_null(IntPtr context);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void sqlite3_result_text(IntPtr context, byte[] value, int nLen, IntPtr pvReserved);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sqlite3_aggregate_context(IntPtr context, int nBytes);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        internal static extern int sqlite3_bind_text16(IntPtr stmt, int index, string value, int nlen, IntPtr pvReserved);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        internal static extern void sqlite3_result_error16(IntPtr context, string strName, int nLen);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        internal static extern void sqlite3_result_text16(IntPtr context, string strName, int nLen, IntPtr pvReserved);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_key(IntPtr db, byte[] key, int keylen);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_rekey(IntPtr db, byte[] key, int keylen);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sqlite3_update_hook(IntPtr db, SQLiteUpdateCallback func, IntPtr pvUser);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sqlite3_commit_hook(IntPtr db, SQLiteCommitCallback func, IntPtr pvUser);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sqlite3_rollback_hook(IntPtr db, SQLiteRollbackCallback func, IntPtr pvUser);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sqlite3_db_handle(IntPtr stmt);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sqlite3_next_stmt(IntPtr db, IntPtr stmt);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_exec(IntPtr db, byte[] strSql, IntPtr pvCallback, IntPtr pvParam,
                                                out IntPtr errMsg);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_config(SQLiteConfig config);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_free(IntPtr ptr);

        #endregion
    }

    // Handles the unmanaged database pointer, and provides finalization support for it.
    internal class SqliteConnectionHandle : CriticalHandle
    {
        private SqliteConnectionHandle(IntPtr db)
            : this()
        {
            this.SetHandle(db);
        }

        internal SqliteConnectionHandle()
            : base(IntPtr.Zero)
        {
        }

        public override bool IsInvalid
        {
            get { return (this.handle == IntPtr.Zero); }
        }

        public static implicit operator IntPtr(SqliteConnectionHandle db)
        {
            return db.handle;
        }

        public static implicit operator SqliteConnectionHandle(IntPtr db)
        {
            return new SqliteConnectionHandle(db);
        }

        protected override bool ReleaseHandle()
        {
            try
            {
                IntPtr localHandle = Interlocked.Exchange(ref this.handle, IntPtr.Zero);

                if (localHandle != IntPtr.Zero)
                {
                    SQLiteBase.CloseConnection(this, localHandle);
                }
            }
            catch (SqliteException)
            {
            }
            finally
            {
                this.handle = IntPtr.Zero;
                this.SetHandleAsInvalid();
            }
            return true;
        }
    }

    // Provides finalization support for unmanaged SQLite statements.
    internal class SqliteStatementHandle : CriticalHandle
    {
        private readonly SqliteConnectionHandle _cnn;

        internal SqliteStatementHandle(SqliteConnectionHandle cnn, IntPtr stmt)
            : this()
        {
            this._cnn = cnn;
            this.SetHandle(stmt);
        }

        internal SqliteStatementHandle()
            : base(IntPtr.Zero)
        {
        }

        public override bool IsInvalid
        {
            get { return (this.handle == IntPtr.Zero); }
        }

        public static implicit operator IntPtr(SqliteStatementHandle stmt)
        {
            return (stmt != null) ? stmt.handle : IntPtr.Zero;
        }

        protected override bool ReleaseHandle()
        {
            try
            {
                lock (this._cnn)
                {
                    SQLiteBase.FinalizeStatement(this);
                    Interlocked.Exchange(ref this.handle, IntPtr.Zero);
                }
            }
            catch (SqliteException)
            {
            }
            finally
            {
                this.SetHandleAsInvalid();
            }
            return true;
        }
    }
}
