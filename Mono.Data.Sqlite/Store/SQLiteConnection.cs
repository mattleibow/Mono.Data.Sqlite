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
    using System.Data.Common;
    using System.Collections.Generic;
    using System.Globalization;
    using System.ComponentModel;

#if SILVERLIGHT
    using SqliteUpdateHookDelegate = Community.CsharpSqlite.Sqlite3.dxUpdateCallback;
    using SqliteCommitHookDelegate = Community.CsharpSqlite.Sqlite3.dxCommitCallback;
    using SqliteRollbackHookDelegate = Community.CsharpSqlite.Sqlite3.dxRollbackCallback;
    using UnsafeNativeMethods = Community.CsharpSqlite.Sqlite3;
#else
    using MonoDataSqliteWrapper;
    using System.Runtime.InteropServices;
#endif

    /// <summary>
    /// SQLite implentation of DbConnection.
    /// </summary>
    /// <remarks>
    /// The <see cref="ConnectionString">ConnectionString</see> property of the SqliteConnection class can contain the following parameter(s), delimited with a semi-colon:
    /// <list type="table">
    /// <listheader>
    /// <term>Parameter</term>
    /// <term>Values</term>
    /// <term>Required</term>
    /// <term>Default</term>
    /// </listheader>
    /// <item>
    /// <description>Data Source</description>
    /// <description>{filename}</description>
    /// <description>Y</description>
    /// <description></description>
    /// </item>
    /// <item>
    /// <description>Version</description>
    /// <description>3</description>
    /// <description>N</description>
    /// <description>3</description>
    /// </item>
    /// <item>
    /// <description>UseUTF16Encoding</description>
    /// <description><b>True</b><br/><b>False</b></description>
    /// <description>N</description>
    /// <description>False</description>
    /// </item>
    /// <item>
    /// <description>DateTimeFormat</description>
    /// <description><b>Ticks</b> - Use DateTime.Ticks<br/><b>ISO8601</b> - Use ISO8601 DateTime format</description>
    /// <description>N</description>
    /// <description>ISO8601</description>
    /// </item>
    /// <item>
    /// <description>BinaryGUID</description>
    /// <description><b>True</b> - Store GUID columns in binary form<br/><b>False</b> - Store GUID columns as text</description>
    /// <description>N</description>
    /// <description>True</description>
    /// </item>
    /// <item>
    /// <description>Cache Size</description>
    /// <description>{size in bytes}</description>
    /// <description>N</description>
    /// <description>2000</description>
    /// </item>
    /// <item>
    /// <description>Synchronous</description>
    /// <description><b>Normal</b> - Normal file flushing behavior<br/><b>Full</b> - Full flushing after all writes<br/><b>Off</b> - Underlying OS flushes I/O's</description>
    /// <description>N</description>
    /// <description>Normal</description>
    /// </item>
    /// <item>
    /// <description>Page Size</description>
    /// <description>{size in bytes}</description>
    /// <description>N</description>
    /// <description>1024</description>
    /// </item>
    /// <item>
    /// <description>Password</description>
    /// <description>{password}</description>
    /// <description>N</description>
    /// <description></description>
    /// </item>
    /// <item>
    /// <description>Enlist</description>
    /// <description><b>Y</b> - Automatically enlist in distributed transactions<br/><b>N</b> - No automatic enlistment</description>
    /// <description>N</description>
    /// <description>Y</description>
    /// </item>
    /// <item>
    /// <description>Pooling</description>
    /// <description><b>True</b> - Use connection pooling<br/><b>False</b> - Do not use connection pooling</description>
    /// <description>N</description>
    /// <description>False</description>
    /// </item>
    /// <item>
    /// <description>FailIfMissing</description>
    /// <description><b>True</b> - Don't create the database if it does not exist, throw an error instead<br/><b>False</b> - Automatically create the database if it does not exist</description>
    /// <description>N</description>
    /// <description>False</description>
    /// </item>
    /// <item>
    /// <description>Max Page Count</description>
    /// <description>{size in pages} - Limits the maximum number of pages (limits the size) of the database</description>
    /// <description>N</description>
    /// <description>0</description>
    /// </item>
    /// <item>
    /// <description>Legacy Format</description>
    /// <description><b>True</b> - Use the more compatible legacy 3.x database format<br/><b>False</b> - Use the newer 3.3x database format which compresses numbers more effectively</description>
    /// <description>N</description>
    /// <description>False</description>
    /// </item>
    /// <item>
    /// <description>Default Timeout</description>
    /// <description>{time in seconds}<br/>The default command timeout</description>
    /// <description>N</description>
    /// <description>30</description>
    /// </item>
    /// <item>
    /// <description>Journal Mode</description>
    /// <description><b>Delete</b> - Delete the journal file after a commit<br/><b>Persist</b> - Zero out and leave the journal file on disk after a commit<br/><b>Off</b> - Disable the rollback journal entirely</description>
    /// <description>N</description>
    /// <description>Delete</description>
    /// </item>
    /// <item>
    /// <description>Read Only</description>
    /// <description><b>True</b> - Open the database for read only access<br/><b>False</b> - Open the database for normal read/write access</description>
    /// <description>N</description>
    /// <description>False</description>
    /// </item>
    /// <item>
    /// <description>Max Pool Size</description>
    /// <description>The maximum number of connections for the given connection string that can be in the connection pool</description>
    /// <description>N</description>
    /// <description>100</description>
    /// </item>
    /// <item>
    /// <description>Default IsolationLevel</description>
    /// <description>The default transaciton isolation level</description>
    /// <description>N</description>
    /// <description>Serializable</description>
    /// </item>
    /// </list>
    /// </remarks>
    public sealed class SqliteConnection : DbConnection, ICloneable
    {
        /// <summary>
        /// State of the current connection
        /// </summary>
        private ConnectionState _connectionState;

        /// <summary>
        /// The connection string
        /// </summary>
        private string _connectionString;

        /// <summary>
        /// Nesting level of the transactions open on the connection
        /// </summary>
        internal int _transactionLevel;

        /// <summary>
        /// The default isolation level for new transactions
        /// </summary>
        private IsolationLevel _defaultIsolation;

        /// <summary>
        /// Whether or not the connection is enlisted in a distrubuted transaction
        /// </summary>
        internal SQLiteEnlistment _enlistment;

        /// <summary>
        /// The base SQLite object to interop with
        /// </summary>
        internal SQLiteBase _sql;

        /// <summary>
        /// The database filename minus path and extension
        /// </summary>
        private string _dataSource;

        /// <summary>
        /// Temporary password storage, emptied after the database has been opened
        /// </summary>
        private string _password;

        /// <summary>
        /// Default command timeout
        /// </summary>
        private int _defaultTimeout = 30;

        internal bool _binaryGuid;

        internal long _version;

        private event SQLiteUpdateEventHandler _updateHandler;
        private event SQLiteCommitHandler _commitHandler;
        private event EventHandler _rollbackHandler;

        private SqliteUpdateHookDelegate _updateCallback;
        private SqliteCommitHookDelegate _commitCallback;
        private SqliteRollbackHookDelegate _rollbackCallback;

        /// <summary>
        /// This event is raised whenever the database is opened or closed.
        /// </summary>
        public override event StateChangeEventHandler StateChange;

        ///<overloads>
        /// Constructs a new SqliteConnection object
        /// </overloads>
        /// <summary>
        /// Default constructor
        /// </summary>
        public SqliteConnection()
            : this("")
        {
        }

        /// <summary>
        /// Initializes the connection with the specified connection string
        /// </summary>
        /// <param name="connectionString">The connection string to use on the connection</param>
        public SqliteConnection(string connectionString)
        {
            _sql = null;
            _connectionState = ConnectionState.Closed;
            _connectionString = "";
            _transactionLevel = 0;
            _version = 0;
            //_commandList = new List<WeakReference>();

            if (connectionString != null)
                ConnectionString = connectionString;
        }

        /// <summary>
        /// Clones the settings and connection string from an existing connection.  If the existing connection is already open, this
        /// function will open its own connection, enumerate any attached databases of the original connection, and automatically
        /// attach to them.
        /// </summary>
        /// <param name="connection"></param>
        public SqliteConnection(SqliteConnection connection)
            : this(connection.ConnectionString)
        {
            if (connection.State == ConnectionState.Open)
            {
                Open();
            }
        }

        /// <summary>
        /// Creates a clone of the connection.  All attached databases and user-defined functions are cloned.  If the existing connection is open, the cloned connection 
        /// will also be opened.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new SqliteConnection(this);
        }

        /// <summary>
        /// Disposes of the SqliteConnection, closing it if it is active.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            Close();
        }

        /// <summary>
        /// Raises the state change event when the state of the connection changes
        /// </summary>
        /// <param name="newState">The new state.  If it is different from the previous state, an event is raised.</param>
        internal void OnStateChange(ConnectionState newState)
        {
            ConnectionState oldState = _connectionState;
            _connectionState = newState;

            StateChangeEventHandler handler = StateChange;
            if (handler != null && oldState != newState)
            {
                handler(this, new StateChangeEventArgs(oldState, newState));
            }
        }

        /// <summary>
        /// OBSOLETE.  Creates a new SqliteTransaction if one isn't already active on the connection.
        /// </summary>
        /// <param name="isolationLevel">This parameter is ignored.</param>
        /// <param name="deferredLock">When TRUE, SQLite defers obtaining a write lock until a write operation is requested.
        /// When FALSE, a writelock is obtained immediately.  The default is TRUE, but in a multi-threaded multi-writer 
        /// environment, one may instead choose to lock the database immediately to avoid any possible writer deadlock.</param>
        /// <returns>Returns a SqliteTransaction object.</returns>
        [Obsolete("Use one of the standard BeginTransaction methods, this one will be removed soon")]
        public SqliteTransaction BeginTransaction(IsolationLevel isolationLevel, bool deferredLock)
        {
            return
                (SqliteTransaction)
                BeginDbTransaction(deferredLock == false ? IsolationLevel.Serializable : IsolationLevel.ReadCommitted);
        }

        /// <summary>
        /// OBSOLETE.  Creates a new SqliteTransaction if one isn't already active on the connection.
        /// </summary>
        /// <param name="deferredLock">When TRUE, SQLite defers obtaining a write lock until a write operation is requested.
        /// When FALSE, a writelock is obtained immediately.  The default is false, but in a multi-threaded multi-writer 
        /// environment, one may instead choose to lock the database immediately to avoid any possible writer deadlock.</param>
        /// <returns>Returns a SqliteTransaction object.</returns>
        [Obsolete("Use one of the standard BeginTransaction methods, this one will be removed soon")]
        public SqliteTransaction BeginTransaction(bool deferredLock)
        {
            return
                (SqliteTransaction)
                BeginDbTransaction(deferredLock == false ? IsolationLevel.Serializable : IsolationLevel.ReadCommitted);
        }

        /// <summary>
        /// Creates a new SqliteTransaction if one isn't already active on the connection.
        /// </summary>
        /// <param name="isolationLevel">Supported isolation levels are Serializable, ReadCommitted and Unspecified.</param>
        /// <remarks>
        /// Unspecified will use the default isolation level specified in the connection string.  If no isolation level is specified in the 
        /// connection string, Serializable is used.
        /// Serializable transactions are the default.  In this mode, the engine gets an immediate lock on the database, and no other threads
        /// may begin a transaction.  Other threads may read from the database, but not write.
        /// With a ReadCommitted isolation level, locks are deferred and elevated as needed.  It is possible for multiple threads to start
        /// a transaction in ReadCommitted mode, but if a thread attempts to commit a transaction while another thread
        /// has a ReadCommitted lock, it may timeout or cause a deadlock on both threads until both threads' CommandTimeout's are reached.
        /// </remarks>
        /// <returns>Returns a SqliteTransaction object.</returns>
        public new SqliteTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return (SqliteTransaction) BeginDbTransaction(isolationLevel);
        }

        /// <summary>
        /// Creates a new SqliteTransaction if one isn't already active on the connection.
        /// </summary>
        /// <returns>Returns a SqliteTransaction object.</returns>
        public new SqliteTransaction BeginTransaction()
        {
            return (SqliteTransaction) BeginDbTransaction(_defaultIsolation);
        }

        /// <summary>
        /// Forwards to the local BeginTransaction() function
        /// </summary>
        /// <param name="isolationLevel">Supported isolation levels are Unspecified, Serializable, and ReadCommitted</param>
        /// <returns></returns>
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            if (_connectionState != ConnectionState.Open)
                throw new InvalidOperationException();

            if (isolationLevel == IsolationLevel.Unspecified) isolationLevel = _defaultIsolation;

            if (isolationLevel != IsolationLevel.Serializable && isolationLevel != IsolationLevel.ReadCommitted)
                throw new ArgumentException("isolationLevel");

            return new SqliteTransaction(this, isolationLevel != IsolationLevel.Serializable);
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="databaseName"></param>
        public override void ChangeDatabase(string databaseName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When the database connection is closed, all commands linked to this connection are automatically reset.
        /// </summary>
        public override void Close()
        {
            if (_sql != null)
            {
                if (_enlistment != null)
                {
                    // If the connection is enlisted in a transaction scope and the scope is still active,
                    // we cannot truly shut down this connection until the scope has completed.  Therefore make a 
                    // hidden connection temporarily to hold open the connection until the scope has completed.
                    var cnn = new SqliteConnection
                                  {
                                      _sql = this._sql,
                                      _transactionLevel = this._transactionLevel,
                                      _enlistment = this._enlistment,
                                      _connectionState = this._connectionState,
                                      _version = this._version
                                  };

                    cnn._enlistment._transaction._cnn = cnn;
                    cnn._enlistment._disposeConnection = true;
                    _sql = null;
                    _enlistment = null;
                }
                if (_sql != null)
                {
                    _sql.Close();
                }
                _sql = null;
                _transactionLevel = 0;
            }
            OnStateChange(ConnectionState.Closed);
        }

        /// <summary>
        /// Clears the connection pool associated with the connection.  Any other active connections using the same database file
        /// will be discarded instead of returned to the pool when they are closed.
        /// </summary>
        /// <param name="connection"></param>
        public static void ClearPool(SqliteConnection connection)
        {
            if (connection._sql == null) return;
            connection._sql.ClearPool();
        }

        /// <summary>
        /// Clears all connection pools.  Any active connections will be discarded instead of sent to the pool when they are closed.
        /// </summary>
        public static void ClearAllPools()
        {
            SqliteConnectionPool.ClearAllPools();
        }

        /// <summary>
        /// The connection string containing the parameters for the connection
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <listheader>
        /// <term>Parameter</term>
        /// <term>Values</term>
        /// <term>Required</term>
        /// <term>Default</term>
        /// </listheader>
        /// <item>
        /// <description>Data Source</description>
        /// <description>{filename}</description>
        /// <description>Y</description>
        /// <description></description>
        /// </item>
        /// <item>
        /// <description>Version</description>
        /// <description>3</description>
        /// <description>N</description>
        /// <description>3</description>
        /// </item>
        /// <item>
        /// <description>UseUTF16Encoding</description>
        /// <description><b>True</b><br/><b>False</b></description>
        /// <description>N</description>
        /// <description>False</description>
        /// </item>
        /// <item>
        /// <description>DateTimeFormat</description>
        /// <description><b>Ticks</b> - Use DateTime.Ticks<br/><b>ISO8601</b> - Use ISO8601 DateTime format<br/><b>JulianDay</b> - Use JulianDay format</description>
        /// <description>N</description>
        /// <description>ISO8601</description>
        /// </item>
        /// <item>
        /// <description>BinaryGUID</description>
        /// <description><b>Yes/On/1</b> - Store GUID columns in binary form<br/><b>No/Off/0</b> - Store GUID columns as text</description>
        /// <description>N</description>
        /// <description>On</description>
        /// </item>
        /// <item>
        /// <description>Cache Size</description>
        /// <description>{size in bytes}</description>
        /// <description>N</description>
        /// <description>2000</description>
        /// </item>
        /// <item>
        /// <description>Synchronous</description>
        /// <description><b>Normal</b> - Normal file flushing behavior<br/><b>Full</b> - Full flushing after all writes<br/><b>Off</b> - Underlying OS flushes I/O's</description>
        /// <description>N</description>
        /// <description>Normal</description>
        /// </item>
        /// <item>
        /// <description>Page Size</description>
        /// <description>{size in bytes}</description>
        /// <description>N</description>
        /// <description>1024</description>
        /// </item>
        /// <item>
        /// <description>Password</description>
        /// <description>{password}</description>
        /// <description>N</description>
        /// <description></description>
        /// </item>
        /// <item>
        /// <description>Enlist</description>
        /// <description><B>Y</B> - Automatically enlist in distributed transactions<br/><b>N</b> - No automatic enlistment</description>
        /// <description>N</description>
        /// <description>Y</description>
        /// </item>
        /// <item>
        /// <description>Pooling</description>
        /// <description><b>True</b> - Use connection pooling<br/><b>False</b> - Do not use connection pooling</description>
        /// <description>N</description>
        /// <description>False</description>
        /// </item>
        /// <item>
        /// <description>FailIfMissing</description>
        /// <description><b>True</b> - Don't create the database if it does not exist, throw an error instead<br/><b>False</b> - Automatically create the database if it does not exist</description>
        /// <description>N</description>
        /// <description>False</description>
        /// </item>
        /// <item>
        /// <description>Max Page Count</description>
        /// <description>{size in pages} - Limits the maximum number of pages (limits the size) of the database</description>
        /// <description>N</description>
        /// <description>0</description>
        /// </item>
        /// <item>
        /// <description>Legacy Format</description>
        /// <description><b>True</b> - Use the more compatible legacy 3.x database format<br/><b>False</b> - Use the newer 3.3x database format which compresses numbers more effectively</description>
        /// <description>N</description>
        /// <description>False</description>
        /// </item>
        /// <item>
        /// <description>Default Timeout</description>
        /// <description>{time in seconds}<br/>The default command timeout</description>
        /// <description>N</description>
        /// <description>30</description>
        /// </item>
        /// <item>
        /// <description>Journal Mode</description>
        /// <description><b>Delete</b> - Delete the journal file after a commit<br/><b>Persist</b> - Zero out and leave the journal file on disk after a commit<br/><b>Off</b> - Disable the rollback journal entirely</description>
        /// <description>N</description>
        /// <description>Delete</description>
        /// </item>
        /// <item>
        /// <description>Read Only</description>
        /// <description><b>True</b> - Open the database for read only access<br/><b>False</b> - Open the database for normal read/write access</description>
        /// <description>N</description>
        /// <description>False</description>
        /// </item>
        /// <item>
        /// <description>Max Pool Size</description>
        /// <description>The maximum number of connections for the given connection string that can be in the connection pool</description>
        /// <description>N</description>
        /// <description>100</description>
        /// </item>
        /// <item>
        /// <description>Default IsolationLevel</description>
        /// <description>The default transaciton isolation level</description>
        /// <description>N</description>
        /// <description>Serializable</description>
        /// </item>
        /// </list>
        /// </remarks>
        [DefaultValue("")]
        public override string ConnectionString
        {
            get { return _connectionString; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                else if (_connectionState != ConnectionState.Closed)
                    throw new InvalidOperationException();

                _connectionString = value;
            }
        }

        /// <summary>
        /// Create a new SqliteCommand and associate it with this connection.
        /// </summary>
        /// <returns>Returns an instantiated SqliteCommand object already assigned to this connection.</returns>
        public new SqliteCommand CreateCommand()
        {
            return new SqliteCommand(this);
        }

        /// <summary>
        /// Forwards to the local CreateCommand() function
        /// </summary>
        /// <returns></returns>
        protected override DbCommand CreateDbCommand()
        {
            return CreateCommand();
        }

        /// <summary>
        /// Returns the filename without extension or path
        /// </summary>
        public override string DataSource
        {
            get { return _dataSource; }
        }

        /// <summary>
        /// Returns an empty string
        /// </summary>
        public override string Database
        {
            get { return "main"; }
        }

        /// <summary>
        /// Maps mono-specific connection string keywords to the standard ones
        /// </summary>
        /// <returns>The mapped keyword name</returns>
        internal static void MapMonoKeyword(string[] arPiece, Dictionary<string, string> ls)
        {
            string keyword, value;

            switch (arPiece[0].ToLower())
            {
                case "uri":
                    keyword = "Data Source";
                    value = MapUriPath(arPiece[1]);
                    break;

                default:
                    keyword = arPiece[0];
                    value = arPiece[1];
                    break;
            }

            ls.Add(keyword, value);
        }

        internal static string MapUriPath(string path)
        {
            if (path.StartsWith("file://"))
            {
                return path.Substring(7);
            }
            
            if (path.StartsWith("file:"))
            {
                return path.Substring(5);
            }
            
            if (path.StartsWith("/"))
            {
                return path;
            }
            
            throw new InvalidOperationException("Invalid connection string: invalid URI");
        }

        /// <summary>
        /// Parses the connection string into component parts
        /// </summary>
        /// <param name="connectionString">The connection string to parse</param>
        /// <returns>An array of key-value pairs representing each parameter of the connection string</returns>
        internal static Dictionary<string, string> ParseConnectionString(string connectionString)
        {
            string s = connectionString.Replace(',', ';'); // Mono compatibility
            int n;
            var ls = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // First split into semi-colon delimited values.  The Split() function of SQLiteBase accounts for and properly
            // skips semi-colons in quoted strings
            string[] arParts = SqliteConvert.Split(s, ';');

            // For each semi-colon piece, split into key and value pairs by the presence of the = sign
            for (n = 0; n < arParts.Length; n++)
            {
                string[] arPiece = SqliteConvert.Split(arParts[n], '=');
                if (arPiece.Length == 2)
                {
                    MapMonoKeyword(arPiece, ls);
                }
                else
                {
                    string message = String.Format(CultureInfo.CurrentCulture,
                                                   "Invalid ConnectionString format for parameter \"{0}\"",
                                                   arPiece.Length > 0 ? arPiece[0] : "null");
                    throw new ArgumentException(message);
                }
            }
            return ls;
        }

        /// <summary>
        /// Manual distributed transaction enlistment support
        /// </summary>
        /// <param name="transaction">The distributed transaction to enlist in</param>
        public override void EnlistTransaction(System.Transactions.Transaction transaction)
        {
            if (_transactionLevel > 0 && transaction != null)
                throw new ArgumentException("Unable to enlist in transaction, a local transaction already exists");

            if (_enlistment != null && transaction != _enlistment._scope)
                throw new ArgumentException("Already enlisted in a transaction");

            _enlistment = new SQLiteEnlistment(this, transaction);
        }

        /// <summary>
        /// Looks for a key in the array of key/values of the parameter string.  If not found, return the specified default value
        /// </summary>
        /// <param name="items">The list to look in</param>
        /// <param name="key">The key to find</param>
        /// <param name="defValue">The default value to return if the key is not found</param>
        /// <returns>The value corresponding to the specified key, or the default value if not found.</returns>
        internal static string FindKey(Dictionary<string, string> items, string key, string defValue)
        {
            string ret;

            if (items.TryGetValue(key, out ret)) return ret;

            return defValue;
        }

        /// <summary>
        /// Opens the connection using the parameters found in the <see cref="ConnectionString">ConnectionString</see>
        /// </summary>
        public override void Open()
        {
            if (_connectionState != ConnectionState.Closed)
            {
                throw new InvalidOperationException();
            }

            Close();

            Dictionary<string, string> opts = ParseConnectionString(_connectionString);

            if (Convert.ToInt32(FindKey(opts, "Version", "3"), CultureInfo.InvariantCulture) != 3)
            {
                throw new NotSupportedException("Only SQLite Version 3 is supported at this time");
            }

            string fileName = FindKey(opts, "Data Source", "");
            if (String.IsNullOrEmpty(fileName))
            {
                fileName = FindKey(opts, "Uri", "");
                if (String.IsNullOrEmpty(fileName))
                {
                    throw new ArgumentException("Data Source cannot be empty.  Use :memory: to open an in-memory database");
                }

                fileName = MapUriPath(fileName);
            }
            if (String.Compare(fileName, ":MEMORY:", StringComparison.OrdinalIgnoreCase) == 0)
            {
                fileName = ":memory:";
            }
            else
            {
                fileName = this.ExpandFileName(fileName);
            }

            try
            {
                bool usePooling = SqliteConvert.ToBoolean(FindKey(opts, "Pooling", Boolean.FalseString));
                bool bUTF16 = SqliteConvert.ToBoolean(FindKey(opts, "UseUTF16Encoding", Boolean.FalseString));
                int maxPoolSize = Convert.ToInt32(FindKey(opts, "Max Pool Size", "100"));

                _defaultTimeout = Convert.ToInt32(FindKey(opts, "Default Timeout", "30"), CultureInfo.CurrentCulture);

                _defaultIsolation =
                    (IsolationLevel)
                    Enum.Parse(typeof (IsolationLevel), FindKey(opts, "Default IsolationLevel", "Serializable"), true);
                if (_defaultIsolation != IsolationLevel.Serializable &&
                    _defaultIsolation != IsolationLevel.ReadCommitted)
                {
                    throw new NotSupportedException("Invalid Default IsolationLevel specified");
                }

                var dateFormat =
                    (SQLiteDateFormats)
                    Enum.Parse(typeof(SQLiteDateFormats), FindKey(opts, "DateTimeFormat", "ISO8601"), true);
                //string temp = FindKey(opts, "DateTimeFormat", "ISO8601");
                //if (String.Compare(temp, "ticks", true, CultureInfo.InvariantCulture) == 0) dateFormat = SQLiteDateFormats.Ticks;
                //else if (String.Compare(temp, "julianday", true, CultureInfo.InvariantCulture) == 0) dateFormat = SQLiteDateFormats.JulianDay;

                // SQLite automatically sets the encoding of the database to UTF16 if called from sqlite3_open16()
                this._sql = bUTF16 ? new SQLite3_UTF16(dateFormat) : new SQLite3(dateFormat);

                SQLiteOpenFlagsEnum flags = SQLiteOpenFlagsEnum.None;
                if (SqliteConvert.ToBoolean(FindKey(opts, "Read Only", Boolean.FalseString)))
                {
                    flags |= SQLiteOpenFlagsEnum.ReadOnly;
                }
                else
                {
                    flags |= SQLiteOpenFlagsEnum.ReadWrite;
                    if (SqliteConvert.ToBoolean(FindKey(opts, "FailIfMissing", Boolean.FalseString)) == false)
                    {
                        flags |= SQLiteOpenFlagsEnum.Create;
                    }
                }
                if (SqliteConvert.ToBoolean(FindKey(opts, "FileProtectionComplete", Boolean.FalseString)))
                {
                    flags |= SQLiteOpenFlagsEnum.FileProtectionComplete;
                }
                if (SqliteConvert.ToBoolean(FindKey(opts, "FileProtectionCompleteUnlessOpen", Boolean.FalseString)))
                {
                    flags |= SQLiteOpenFlagsEnum.FileProtectionCompleteUnlessOpen;
                }
                if (SqliteConvert.ToBoolean(FindKey(opts, "FileProtectionCompleteUntilFirstUserAuthentication", Boolean.FalseString)))
                {
                    flags |= SQLiteOpenFlagsEnum.FileProtectionCompleteUntilFirstUserAuthentication;
                }
                if (SqliteConvert.ToBoolean(FindKey(opts, "FileProtectionNone", Boolean.FalseString)))
                {
                    flags |= SQLiteOpenFlagsEnum.FileProtectionNone;
                }

                _sql.Open(fileName, flags, maxPoolSize, usePooling);

                _binaryGuid = SqliteConvert.ToBoolean(FindKey(opts, "BinaryGUID", Boolean.TrueString));

                string password = FindKey(opts, "Password", null);
                if (String.IsNullOrEmpty(password) == false)
                {
                    _sql.SetPassword(password);
                }
                else if (_password != null)
                {
                    _sql.SetPassword(_password);
                }
                _password = null;

                // TODO : _dataSource = Path.GetFileNameWithoutExtension(fileName);
                _dataSource = fileName;

                OnStateChange(ConnectionState.Open);
                _version++;

                using (SqliteCommand cmd = CreateCommand())
                {
                    string defValue;

                    if (fileName != ":memory:")
                    {
                        defValue = FindKey(opts, "Page Size", "1024");
                        if (Convert.ToInt32(defValue, CultureInfo.InvariantCulture) != 1024)
                        {
                            cmd.CommandText = String.Format(CultureInfo.InvariantCulture, "PRAGMA page_size={0}",
                                                            defValue);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    defValue = FindKey(opts, "Max Page Count", "0");
                    if (Convert.ToInt32(defValue, CultureInfo.InvariantCulture) != 0)
                    {
                        cmd.CommandText = String.Format(CultureInfo.InvariantCulture, "PRAGMA max_page_count={0}",
                                                        defValue);
                        cmd.ExecuteNonQuery();
                    }

                    defValue = FindKey(opts, "Legacy Format", Boolean.FalseString);
                    cmd.CommandText = String.Format(CultureInfo.InvariantCulture, "PRAGMA legacy_file_format={0}",
                                                    SqliteConvert.ToBoolean(defValue) ? "ON" : "OFF");
                    cmd.ExecuteNonQuery();

                    defValue = FindKey(opts, "Synchronous", "Normal");
                    if (String.Compare(defValue, "Full", StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        cmd.CommandText = String.Format(CultureInfo.InvariantCulture, "PRAGMA synchronous={0}", defValue);
                        cmd.ExecuteNonQuery();
                    }

                    defValue = FindKey(opts, "Cache Size", "2000");
                    if (Convert.ToInt32(defValue, CultureInfo.InvariantCulture) != 2000)
                    {
                        cmd.CommandText = String.Format(CultureInfo.InvariantCulture, "PRAGMA cache_size={0}", defValue);
                        cmd.ExecuteNonQuery();
                    }

                    defValue = FindKey(opts, "Journal Mode", "Delete");
                    if (String.Compare(defValue, "Default", StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        cmd.CommandText = String.Format(CultureInfo.InvariantCulture, "PRAGMA journal_mode={0}",
                                                        defValue);
                        cmd.ExecuteNonQuery();
                    }
                }

                if (_commitHandler != null)
                {
                    _sql.SetCommitHook(_commitCallback);
                }

                if (_updateHandler != null)
                {
                    _sql.SetUpdateHook(_updateCallback);
                }

                if (_rollbackHandler != null)
                {
                    _sql.SetRollbackHook(_rollbackCallback);
                }

                if (global::System.Transactions.Transaction.Current != null && 
                    SqliteConvert.ToBoolean(FindKey(opts, "Enlist", Boolean.TrueString)))
                    EnlistTransaction(global::System.Transactions.Transaction.Current);
            }
            catch (SqliteException)
            {
                Close();

                throw;
            }
        }

        /// <summary>
        /// Gets/sets the default command timeout for newly-created commands.  This is especially useful for 
        /// commands used internally such as inside a SqliteTransaction, where setting the timeout is not possible.
        /// This can also be set in the ConnectionString with "Default Timeout"
        /// </summary>
        public int DefaultTimeout
        {
            get { return _defaultTimeout; }
            set { _defaultTimeout = value; }
        }

        /// <summary>
        /// Returns the version of the underlying SQLite database engine
        /// </summary>
        public override string ServerVersion
        {
            get
            {
                if (_connectionState != ConnectionState.Open)
                    throw new InvalidOperationException();

                return _sql.Version;
            }
        }

        /// <summary>
        /// Returns the version of the underlying SQLite database engine
        /// </summary>
        public static string SQLiteVersion
        {
            get { return SQLite3.SQLiteVersion; }
        }

        /// <summary>
        /// Returns the state of the connection.
        /// </summary>
        public override ConnectionState State
        {
            get { return _connectionState; }
        }

        /// <summary>
        /// Change the password (or assign a password) to an open database.
        /// </summary>
        /// <remarks>
        /// No readers or writers may be active for this process.  The database must already be open
        /// and if it already was password protected, the existing password must already have been supplied.
        /// </remarks>
        /// <param name="newPassword">The new password to assign to the database</param>
        public void ChangePassword(string newPassword)
        {
            if (_connectionState != ConnectionState.Open)
                throw new InvalidOperationException("Database must be opened before changing the password.");

            _sql.ChangePassword(newPassword);
        }

        /// <summary>
        /// Sets the password for a password-protected database.  A password-protected database is
        /// unusable for any operation until the password has been set.
        /// </summary>
        /// <param name="databasePassword">The password for the database</param>
        public void SetPassword(string databasePassword)
        {
            if (_connectionState != ConnectionState.Closed)
                throw new InvalidOperationException("Password can only be set before the database is opened.");

            if (databasePassword != null)
                if (databasePassword.Length == 0) databasePassword = null;

            _password = databasePassword;
        }

        /// <summary>
        /// Expand the filename of the data source, resolving the |DataDirectory| macro as appropriate.
        /// </summary>
        /// <param name="sourceFile">The database filename to expand</param>
        /// <returns>The expanded path and filename of the filename</returns>
        private string ExpandFileName(string sourceFile)
        {
            return sourceFile;
        }

        /// <summary>
        /// This event is raised whenever SQLite makes an update/delete/insert into the database on
        /// this connection.  It only applies to the given connection.
        /// </summary>
        public event SQLiteUpdateEventHandler Update
        {
            add
            {
                if (_updateHandler == null)
                {
                    _updateCallback = new SqliteUpdateHookDelegate(UpdateCallback);
                    if (_sql != null) _sql.SetUpdateHook(_updateCallback);
                }
                _updateHandler += value;
            }
            remove
            {
                _updateHandler -= value;
                if (_updateHandler == null)
                {
                    if (_sql != null) _sql.SetUpdateHook(null);
                    _updateCallback = null;
                }
            }
        }

        private void UpdateCallback(object puser, int type, string database, string table, Int64 rowid)
        {
            _updateHandler(this, new UpdateEventArgs(
                                     database,
                                     table,
                                     (UpdateEventType) type,
                                     rowid));
        }

        /// <summary>
        /// This event is raised whenever SQLite is committing a transaction.
        /// Return non-zero to trigger a rollback
        /// </summary>
        public event SQLiteCommitHandler Commit
        {
            add
            {
                if (_commitHandler == null)
                {
                    _commitCallback = new SqliteCommitHookDelegate(CommitCallback);
                    if (_sql != null) _sql.SetCommitHook(_commitCallback);
                }
                _commitHandler += value;
            }
            remove
            {
                _commitHandler -= value;
                if (_commitHandler == null)
                {
                    if (_sql != null) _sql.SetCommitHook(null);
                    _commitCallback = null;
                }
            }
        }

        /// <summary>
        /// This event is raised whenever SQLite is committing a transaction.
        /// Return non-zero to trigger a rollback
        /// </summary>
        public event EventHandler RollBack
        {
            add
            {
                if (_rollbackHandler == null)
                {
                    _rollbackCallback = new SqliteRollbackHookDelegate(RollbackCallback);
                    if (_sql != null) _sql.SetRollbackHook(_rollbackCallback);
                }
                _rollbackHandler += value;
            }
            remove
            {
                _rollbackHandler -= value;
                if (_rollbackHandler == null)
                {
                    if (_sql != null) _sql.SetRollbackHook(null);
                    _rollbackCallback = null;
                }
            }
        }


        private int CommitCallback(object parg)
        {
            var e = new CommitEventArgs();
            _commitHandler(this, e);
            return e.AbortTransaction ? 1 : 0;
        }

        private void RollbackCallback(object parg)
        {
            _rollbackHandler(this, EventArgs.Empty);
        }

        // http://www.sqlite.org/c3ref/config.html
        public static void SetConfig(SQLiteConfig config)
        {
            throw new NotImplementedException();
//#if SILVERLIGHT
//            var op = (int) config;
//#else
//            var op = config;
//#endif
//            int n = UnsafeNativeMethods.sqlite3_config(op);
//            if (n > 0) throw new SqliteException(n, null);
        }
    }

    /// <summary>
    /// The I/O file cache flushing behavior for the connection
    /// </summary>
    public enum SynchronizationModes
    {
        /// <summary>
        /// Normal file flushing at critical sections of the code
        /// </summary>
        Normal = 0,

        /// <summary>
        /// Full file flushing after every write operation
        /// </summary>
        Full = 1,

        /// <summary>
        /// Use the default operating system's file flushing, SQLite does not explicitly flush the file buffers after writing
        /// </summary>
        Off = 2,
    }

    /// <summary>
    /// Raised when a transaction is about to be committed.  To roll back a transaction, set the 
    /// rollbackTrans boolean value to true.
    /// </summary>
    /// <param name="sender">The connection committing the transaction</param>
    /// <param name="e">Event arguments on the transaction</param>
    public delegate void SQLiteCommitHandler(object sender, CommitEventArgs e);

    /// <summary>
    /// Raised when data is inserted, updated and deleted on a given connection
    /// </summary>
    /// <param name="sender">The connection committing the transaction</param>
    /// <param name="e">The event parameters which triggered the event</param>
    public delegate void SQLiteUpdateEventHandler(object sender, UpdateEventArgs e);

    /// <summary>
    /// Whenever an update event is triggered on a connection, this enum will indicate
    /// exactly what type of operation is being performed.
    /// </summary>
    public enum UpdateEventType
    {
        /// <summary>
        /// A row is being deleted from the given database and table
        /// </summary>
        Delete = 9,

        /// <summary>
        /// A row is being inserted into the table.
        /// </summary>
        Insert = 18,

        /// <summary>
        /// A row is being updated in the table.
        /// </summary>
        Update = 23,
    }

    /// <summary>
    /// Passed during an Update callback, these event arguments detail the type of update operation being performed
    /// on the given connection.
    /// </summary>
    public class UpdateEventArgs : EventArgs
    {
        /// <summary>
        /// The name of the database being updated (usually "main" but can be any attached or temporary database)
        /// </summary>
        public readonly string Database;

        /// <summary>
        /// The name of the table being updated
        /// </summary>
        public readonly string Table;

        /// <summary>
        /// The type of update being performed (insert/update/delete)
        /// </summary>
        public readonly UpdateEventType Event;

        /// <summary>
        /// The RowId affected by this update.
        /// </summary>
        public readonly Int64 RowId;

        internal UpdateEventArgs(string database, string table, UpdateEventType eventType, Int64 rowid)
        {
            Database = database;
            Table = table;
            Event = eventType;
            RowId = rowid;
        }
    }

    /// <summary>
    /// Event arguments raised when a transaction is being committed
    /// </summary>
    public class CommitEventArgs : EventArgs
    {
        internal CommitEventArgs()
        {
        }

        /// <summary>
        /// Set to true to abort the transaction and trigger a rollback
        /// </summary>
        public bool AbortTransaction;
    }
}
