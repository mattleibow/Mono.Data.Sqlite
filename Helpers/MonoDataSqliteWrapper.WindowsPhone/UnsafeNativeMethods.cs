//using SqliteConnectionHandle = Community.CsharpSqlite.Sqlite3.sqlite3;
//using SqliteValueHandle = Community.CsharpSqlite.Sqlite3.Mem;
//using SqliteStatementHandle = Community.CsharpSqlite.Sqlite3.Vdbe;
//using SqliteUpdateHookDelegate = Community.CsharpSqlite.Sqlite3.dxUpdateCallback;
//using SqliteCommitHookDelegate = Community.CsharpSqlite.Sqlite3.dxCommitCallback;
//using SqliteRollbackHookDelegate = Community.CsharpSqlite.Sqlite3.dxRollbackCallback;
//using SQLiteFinalCallback = Community.CsharpSqlite.Sqlite3.dxFinal;
//using SQLiteCallback = Community.CsharpSqlite.Sqlite3.dxFunc;
//using SQLiteCollation = Community.CsharpSqlite.Sqlite3.dxCompare;
//using SqliteContextHandle = Community.CsharpSqlite.Sqlite3.sqlite3_context;

namespace MonoDataSqliteWrapper
{
    /// <summary>
    /// An internal callback delegate declaration.
    /// </summary>
    /// <param name="context">Raw context pointer for the user function</param>
    /// <param name="nArgs">Count of arguments to the function</param>
    /// <param name="argsptr">A pointer to the array of argument pointers</param>
    public delegate void SQLiteCallback(SqliteContextHandle context, int nArgs, SqliteValueHandle[] argsptr);
    /// <summary>
    /// An internal final callback delegate declaration.
    /// </summary>
    /// <param name="context">Raw context pointer for the user function</param>
    public delegate void SQLiteFinalCallback(SqliteContextHandle context);
    /// <summary>
    /// Internal callback delegate for implementing collation sequences
    /// </summary>
    /// <param name="puser">Not used</param>
    /// <param name="len1">Length of the string pv1</param>
    /// <param name="pv1">Pointer to the first string to compare</param>
    /// <param name="len2">Length of the string pv2</param>
    /// <param name="pv2">Pointer to the second string to compare</param>
    /// <returns>Returns -1 if the first string is less than the second.  0 if they are equal, or 1 if the first string is greater
    /// than the second.</returns>
    public delegate int SQLiteCollation(object puser, int len1, string pv1, int len2, string pv2);

    public delegate void SqliteUpdateHookDelegate(object argument, int b, string c, string d, long e);
    public delegate int SqliteCommitHookDelegate(object argument);
    public delegate void SqliteRollbackHookDelegate(object argument);

    /// <summary>
    /// Utility class for wrapping sqlite3 "handles".
    /// </summary>
    public sealed class SqliteConnectionHandle
    {
        internal SqliteConnectionHandle(Community.CsharpSqlite.Sqlite3.sqlite3 db)
        {
            _handle = db;
        }

        internal Community.CsharpSqlite.Sqlite3.sqlite3 Handle
        {
            get
            {
                return _handle;
            }
        }

        private Community.CsharpSqlite.Sqlite3.sqlite3 _handle;
    }

    /// <summary>
    /// Utility class for wrapping sqlite3 "handles".
    /// </summary>
    public sealed class SqliteValueHandle
    {
        internal SqliteValueHandle(Community.CsharpSqlite.Sqlite3.Mem db)
        {
            _handle = db;
        }

        internal Community.CsharpSqlite.Sqlite3.Mem Handle
        {
            get
            {
                return _handle;
            }
        }

        private Community.CsharpSqlite.Sqlite3.Mem _handle;
    }

    /// <summary>
    /// Utility class for wrapping sqlite3_stmt "handles".
    /// </summary>
    public sealed class SqliteStatementHandle
    {
        internal SqliteStatementHandle(Community.CsharpSqlite.Sqlite3.Vdbe statement)
        {
            _handle = (statement);
        }

        internal Community.CsharpSqlite.Sqlite3.Vdbe Handle
        {
            get
            {
                return _handle;
            }
        }

        private Community.CsharpSqlite.Sqlite3.Vdbe _handle;
    };

    /// <summary>
    /// Utility class for wrapping sqlite3_context "handles".
    /// </summary>
    public sealed class SqliteContextHandle
    {
        internal SqliteContextHandle(Community.CsharpSqlite.Sqlite3.sqlite3_context context)
        {
            _handle = (context);
        }

        internal Community.CsharpSqlite.Sqlite3.sqlite3_context Handle
        {
            get
            {
                return _handle;
            }
        }

        private Community.CsharpSqlite.Sqlite3.sqlite3_context _handle;
    };


    public static class UnsafeNativeMethods
    {
        /// <summary>
        /// An internal callback delegate declaration.
        /// </summary>
        /// <param name="context">Raw context pointer for the user function</param>
        /// <param name="nArgs">Count of arguments to the function</param>
        /// <param name="argsptr">A pointer to the array of argument pointers</param>
        internal delegate void SQLiteCallback(SqliteContextHandle context, int nArgs, SqliteValueHandle[] argsptr);
        /// <summary>
        /// An internal final callback delegate declaration.
        /// </summary>
        /// <param name="context">Raw context pointer for the user function</param>
        internal delegate void SQLiteFinalCallback(SqliteContextHandle context);
        /// <summary>
        /// Internal callback delegate for implementing collation sequences
        /// </summary>
        /// <param name="puser">Not used</param>
        /// <param name="len1">Length of the string pv1</param>
        /// <param name="pv1">Pointer to the first string to compare</param>
        /// <param name="len2">Length of the string pv2</param>
        /// <param name="pv2">Pointer to the second string to compare</param>
        /// <returns>Returns -1 if the first string is less than the second.  0 if they are equal, or 1 if the first string is greater
        /// than the second.</returns>
        internal delegate int SQLiteCollation(object puser, int len1, string pv1, int len2, string pv2);


        public static void sqlite3_interrupt(SqliteConnectionHandle connection)
        {
            Community.CsharpSqlite.Sqlite3.sqlite3_interrupt(connection.Handle);
        }

        public static string sqlite3_libversion()
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_libversion();
        }

        public static int sqlite3_changes(SqliteConnectionHandle connection)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_changes(connection.Handle);
        }

        public static int sqlite3_open_v2(string filename, out SqliteConnectionHandle connection, int flags, string zvfs)
        {
            Community.CsharpSqlite.Sqlite3.sqlite3 innerConn;
            var res = Community.CsharpSqlite.Sqlite3.sqlite3_open_v2(filename, out innerConn, flags, zvfs);
            connection = new SqliteConnectionHandle(innerConn);
            return res;
        }

        public static int sqlite3_busy_timeout(SqliteConnectionHandle connection, int timeout)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_busy_timeout(connection.Handle, timeout);
        }

        public static int sqlite3_step(SqliteStatementHandle statement)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_step(statement.Handle);
        }

        public static int sqlite3_reset(SqliteStatementHandle statement)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_reset(statement.Handle);
        }

        public static int sqlite3_prepare16(
            SqliteConnectionHandle connection, string sql, int sqlLength,
            out SqliteStatementHandle statement, out string remainingSql)
        {
            SqliteStatementHandle innerStmt;
            var res = sqlite3_prepare(connection, sql, sqlLength, out innerStmt, out remainingSql);
            statement = innerStmt;
            return res;
        }

        public static int sqlite3_prepare(
            SqliteConnectionHandle connection, string sql, int sqlLength,
            out SqliteStatementHandle statement, out string remainingSql)
        {
            Community.CsharpSqlite.Sqlite3.Vdbe stmt = null;
            string remSql = null;
            var result = Community.CsharpSqlite.Sqlite3.sqlite3_prepare(connection.Handle, sql, sqlLength, ref stmt, ref remSql);
            statement = new SqliteStatementHandle(stmt);
            remainingSql = remSql;
            return result;
        }

        public static int sqlite3_bind_blob(SqliteStatementHandle statement, int index, byte[] data, int dataLength, object callback)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_bind_blob(statement.Handle, index, data, dataLength, null);
        }

        public static int sqlite3_bind_double(SqliteStatementHandle statement, int index, double value)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_bind_double(statement.Handle, index, value);
        }

        public static int sqlite3_bind_int(SqliteStatementHandle statement, int index, int value)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_bind_int(statement.Handle, index, value);
        }

        public static int sqlite3_bind_int64(SqliteStatementHandle statement, int index, long value)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_bind_int64(statement.Handle, index, value);
        }

        public static int sqlite3_bind_null(SqliteStatementHandle statement, int index)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_bind_null(statement.Handle, index);
        }

        public static int sqlite3_bind_text(SqliteStatementHandle statement, int index, string text, int textLength, object callback)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_bind_text(statement.Handle, index, text, textLength, null);
        }

        public static int sqlite3_bind_parameter_count(SqliteStatementHandle statement)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_bind_parameter_count(statement.Handle);
        }

        public static string sqlite3_bind_parameter_name(SqliteStatementHandle statement, int index)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_bind_parameter_name(statement.Handle, index);
        }

        public static int sqlite3_bind_parameter_index(SqliteStatementHandle statement, string name)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_bind_parameter_index(statement.Handle, name);
        }


        public static int sqlite3_column_count(SqliteStatementHandle statement)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_column_count(statement.Handle);
        }

        public static string sqlite3_column_name(SqliteStatementHandle statement, int index)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_column_name(statement.Handle, index);
        }


        public static byte[] sqlite3_column_blob(SqliteStatementHandle statement, int index)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_column_blob(statement.Handle, index);
        }

        public static int sqlite3_column_bytes(SqliteStatementHandle statement, int index)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_column_bytes(statement.Handle, index);
        }

        public static double sqlite3_column_double(SqliteStatementHandle statement, int index)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_column_double(statement.Handle, index);
        }

        public static int sqlite3_column_int(SqliteStatementHandle statement, int index)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_column_int(statement.Handle, index);
        }

        public static long sqlite3_column_int64(SqliteStatementHandle statement, int index)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_column_int64(statement.Handle, index);
        }

        public static string sqlite3_column_text(SqliteStatementHandle statement, int index)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_column_text(statement.Handle, index);
        }

        public static int sqlite3_column_type(SqliteStatementHandle statement, int index)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_column_type(statement.Handle, index);
        }

        public static string sqlite3_column_decltype(SqliteStatementHandle statement, int index)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_column_decltype(statement.Handle, index);
        }

        public static string sqlite3_column_origin_name(SqliteStatementHandle statement, int index)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_column_origin_name(statement.Handle, index);
        }

        public static string sqlite3_column_database_name(SqliteStatementHandle statement, int index)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_column_database_name(statement.Handle, index);
        }

        public static string sqlite3_column_table_name(SqliteStatementHandle statement, int index)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_column_table_name(statement.Handle, index);
        }

        public static byte[] sqlite3_value_blob(SqliteValueHandle value)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_value_blob(value.Handle);
        }

        public static int sqlite3_value_bytes(SqliteValueHandle value)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_value_bytes(value.Handle);
        }

        public static double sqlite3_value_double(SqliteValueHandle value)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_value_double(value.Handle);
        }

        public static int sqlite3_value_int(SqliteValueHandle value)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_value_int(value.Handle);
        }

        public static long sqlite3_value_int64(SqliteValueHandle value)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_value_int64(value.Handle);
        }

        public static string sqlite3_value_text(SqliteValueHandle value)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_value_text(value.Handle);
        }

        public static int sqlite3_value_type(SqliteValueHandle value)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_value_type(value.Handle);
        }


        public static void sqlite3_result_blob(SqliteContextHandle context, byte[] data, int dataLength, object callback)
        {
            var value = new System.Text.UnicodeEncoding().GetString(data, 0, data.Length);
            Community.CsharpSqlite.Sqlite3.sqlite3_result_blob(context.Handle, value, dataLength, null);
        }

        public static void sqlite3_result_double(SqliteContextHandle context, double value)
        {
            Community.CsharpSqlite.Sqlite3.sqlite3_result_double(context.Handle, value);
        }

        public static void sqlite3_result_int(SqliteContextHandle context, int value)
        {
            Community.CsharpSqlite.Sqlite3.sqlite3_result_int(context.Handle, value);
        }

        public static void sqlite3_result_int64(SqliteContextHandle context, long value)
        {
            Community.CsharpSqlite.Sqlite3.sqlite3_result_int64(context.Handle, value);
        }

        public static void sqlite3_result_null(SqliteContextHandle context)
        {
            Community.CsharpSqlite.Sqlite3.sqlite3_result_null(context.Handle);
        }

        public static void sqlite3_result_text(SqliteContextHandle context, string text, int textLength, object callback)
        {
            Community.CsharpSqlite.Sqlite3.sqlite3_result_text(context.Handle, text, textLength, null);
        }

        public static void sqlite3_result_error(SqliteContextHandle context, string error, int errorLength)
        {
            Community.CsharpSqlite.Sqlite3.sqlite3_result_error(context.Handle, error, errorLength);
        }


        public static int sqlite3_table_column_metadata(
            SqliteConnectionHandle connection,
            string database, string table, string column,
            out string dataTypePtr, out string collSeqPtr,
            out int nnotNull, out int nprimaryKey, out int nautoInc)
        {
            string realdataTypePtr = null;
            string realcollSeqPtr = null;
            int realnnotNull = 0;
            int realnprimaryKey = 0;
            int realnautoInc = 0;

            var result = Community.CsharpSqlite.Sqlite3.sqlite3_table_column_metadata(
                connection.Handle,
                database, table, column,
                ref realdataTypePtr, ref realcollSeqPtr,
                ref realnnotNull, ref realnprimaryKey, ref realnautoInc);

            dataTypePtr = realdataTypePtr;
            collSeqPtr = realcollSeqPtr;
            nnotNull = realnnotNull;
            nprimaryKey = realnprimaryKey;
            nautoInc = realnautoInc;

            return result;
        }

        public static SqliteValueHandle sqlite3_aggregate_context(SqliteContextHandle context, int theByte)
        {
            var res = Community.CsharpSqlite.Sqlite3.sqlite3_aggregate_context(context.Handle, theByte);
            if (res == null)
                return null;
            return new SqliteValueHandle(res);
        }


        public static object sqlite3_update_hook(SqliteConnectionHandle connection, SqliteUpdateHookDelegate callback, object arg)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_update_hook(connection.Handle, (a, b, c, d, e) => callback(a, b, c, d, e), arg);
        }

        public static object sqlite3_commit_hook(SqliteConnectionHandle connection, SqliteCommitHookDelegate callback, object arg)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_commit_hook(connection.Handle, (a) => callback(a), arg);
        }

        public static object sqlite3_rollback_hook(SqliteConnectionHandle connection, SqliteRollbackHookDelegate callback, object arg)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_rollback_hook(connection.Handle, (a) => callback(a), arg);
        }

        public static int sqlite3_open(string filename, out SqliteConnectionHandle connection)
        {
            Community.CsharpSqlite.Sqlite3.sqlite3 innerConn;
            var res = Community.CsharpSqlite.Sqlite3.sqlite3_open(filename, out innerConn);
            connection = new SqliteConnectionHandle(innerConn);
            return res;
        }


        public static int sqlite3_open16(string filename, out SqliteConnectionHandle connection)
        {
            return sqlite3_open(filename, out connection);
        }

        public static int sqlite3_bind_text16(SqliteStatementHandle statement, int index, string value, int textLength)
        {
            return sqlite3_bind_text(statement, index, value, textLength, null);
        }

        public static string sqlite3_column_name16(SqliteStatementHandle statement, int index)
        {
            return sqlite3_column_name(statement, index);
        }

        public static string sqlite3_column_text16(SqliteStatementHandle statement, int index)
        {
            return sqlite3_column_text(statement, index);
        }

        public static string sqlite3_column_origin_name16(SqliteStatementHandle statement, int index)
        {
            return sqlite3_column_origin_name(statement, index);
        }

        public static string sqlite3_column_database_name16(SqliteStatementHandle statement, int index)
        {
            return sqlite3_column_database_name(statement, index);
        }

        public static string sqlite3_column_table_name16(SqliteStatementHandle statement, int index)
        {
            return sqlite3_column_table_name(statement, index);
        }

        public static string sqlite3_value_text16(SqliteValueHandle value)
        {
            return sqlite3_value_text(value);
        }

        public static void sqlite3_result_error16(SqliteContextHandle context, string error, int errorLength)
        {
            sqlite3_result_error(context, error, errorLength);
        }

        public static void sqlite3_result_text16(SqliteContextHandle context, string value, int valueLength, object callback)
        {
            sqlite3_result_text(context, value, valueLength, null);
        }

        public static void sqlite3_exec(SqliteConnectionHandle connection, string query, out string msg)
        {
            string realMsg = null;
            Community.CsharpSqlite.Sqlite3.sqlite3_exec(connection.Handle, query, null, null, ref realMsg);
            msg = realMsg;
        }

        public static string sqlite3_errmsg(SqliteConnectionHandle connection)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_errmsg(connection.Handle);
        }

        public static int sqlite3_finalize(SqliteStatementHandle statement)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_finalize(statement.Handle);
        }

        public static int sqlite3_close(SqliteConnectionHandle connection)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_close(connection.Handle);
        }

        public static SqliteStatementHandle sqlite3_next_stmt(SqliteConnectionHandle connection, SqliteStatementHandle statement)
        {
            var res = Community.CsharpSqlite.Sqlite3.sqlite3_next_stmt(connection.Handle, statement == null ? null : statement.Handle);
            if (res == null)
                return null;
            return new SqliteStatementHandle(res);
        }

        public static int sqlite3_config(int option, object[] args)
        {
            return Community.CsharpSqlite.Sqlite3.sqlite3_config(option, args);
        }
    }
}
