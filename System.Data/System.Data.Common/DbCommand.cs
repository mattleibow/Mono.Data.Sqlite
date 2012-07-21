//
// System.Data.Common.DbCommand
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace System.Data.Common
{
    public abstract class DbCommand : IDbCommand, IDisposable
    {
        #region Properties

        public DbConnection Connection
        {
            get { return this.DbConnection; }
            set { this.DbConnection = value; }
        }

        protected abstract DbConnection DbConnection { get; set; }
        protected abstract DbParameterCollection DbParameterCollection { get; }
        protected abstract DbTransaction DbTransaction { get; set; }

        public DbParameterCollection Parameters
        {
            get { return this.DbParameterCollection; }
        }

        public DbTransaction Transaction
        {
            get { return this.DbTransaction; }
            set { this.DbTransaction = value; }
        }

        public abstract string CommandText { get; set; }

        public abstract int CommandTimeout { get; set; }

        public abstract CommandType CommandType { get; set; }

        IDbConnection IDbCommand.Connection
        {
            get { return this.Connection; }
            set { this.Connection = (DbConnection) value; }
        }

        IDataParameterCollection IDbCommand.Parameters
        {
            get { return this.Parameters; }
        }

        IDbTransaction IDbCommand.Transaction
        {
            get { return this.Transaction; }
            set { this.Transaction = (DbTransaction) value; }
        }

        public abstract UpdateRowSource UpdatedRowSource { get; set; }

        #endregion // Properties

        #region Methods

        public abstract void Cancel();
        public abstract int ExecuteNonQuery();

        public abstract object ExecuteScalar();

        IDbDataParameter IDbCommand.CreateParameter()
        {
            return this.CreateParameter();
        }

        IDataReader IDbCommand.ExecuteReader()
        {
            return this.ExecuteReader();
        }

        IDataReader IDbCommand.ExecuteReader(CommandBehavior behavior)
        {
            return this.ExecuteReader(behavior);
        }

        public abstract void Prepare();
        protected abstract DbParameter CreateDbParameter();

        public DbParameter CreateParameter()
        {
            return this.CreateDbParameter();
        }

        protected abstract DbDataReader ExecuteDbDataReader(CommandBehavior behavior);

        public DbDataReader ExecuteReader()
        {
            return this.ExecuteDbDataReader(CommandBehavior.Default);
        }

        public DbDataReader ExecuteReader(CommandBehavior behavior)
        {
            return this.ExecuteDbDataReader(behavior);
        }

        #endregion // Methods

        #region IDbCommand Members

        public virtual void Dispose()
        {
        }

        #endregion
    }
}
