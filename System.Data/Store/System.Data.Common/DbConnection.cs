//
// System.Data.Common.DbConnection
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
    using System.Transactions;
    using IsolationLevel = System.Data.IsolationLevel;

    public abstract class DbConnection : IDbConnection, IDisposable
    {
        #region Properties

        public abstract string DataSource { get; }

        public abstract string ServerVersion { get; }
        public abstract string ConnectionString { get; set; }

        public abstract string Database { get; }

        public abstract ConnectionState State { get; }

        public virtual int ConnectionTimeout
        {
            get { return 15; }
        }

        #endregion // Properties

        #region Methods

        public abstract void ChangeDatabase(string databaseName);
        public abstract void Close();

        IDbTransaction IDbConnection.BeginTransaction()
        {
            return this.BeginTransaction();
        }

        IDbTransaction IDbConnection.BeginTransaction(IsolationLevel il)
        {
            return this.BeginTransaction(il);
        }

        IDbCommand IDbConnection.CreateCommand()
        {
            return this.CreateCommand();
        }

        public abstract void Open();
        protected abstract DbTransaction BeginDbTransaction(IsolationLevel isolationLevel);

        public DbTransaction BeginTransaction()
        {
            return this.BeginDbTransaction(IsolationLevel.Unspecified);
        }

        public DbTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return this.BeginDbTransaction(isolationLevel);
        }

        public DbCommand CreateCommand()
        {
            return this.CreateDbCommand();
        }

        protected abstract DbCommand CreateDbCommand();

        public virtual void EnlistTransaction(Transaction transaction)
        {
            throw new NotSupportedException();
        }

        protected virtual void OnStateChange(StateChangeEventArgs stateChange)
        {
            StateChangeEventHandler handler = this.StateChange;

            if (handler != null)
            {
                handler(this, stateChange);
            }
        }

        #endregion // Methods

        #region IDbConnection Members

        public virtual void Dispose()
        {
        }

        #endregion

        public virtual event StateChangeEventHandler StateChange;
    }
}
