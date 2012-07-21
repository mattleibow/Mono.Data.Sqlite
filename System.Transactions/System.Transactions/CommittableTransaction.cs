//
// CommittableTransaction.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//	Ankit Jain	 <JAnkit@novell.com>
//
// (C)2005 Novell Inc,
// (C)2006 Novell Inc,
//

#if NET_2_0
namespace System.Transactions
{
    using System.Threading;

    public sealed class CommittableTransaction : Transaction,
                                                 IDisposable, IAsyncResult
    {
        private readonly TransactionOptions options;
        private IAsyncResult asyncResult;

        private AsyncCallback callback;
        private object user_defined_state;

        public CommittableTransaction()
            : this(new TransactionOptions())
        {
        }

        public CommittableTransaction(TimeSpan timeout)
        {
            this.options = new TransactionOptions();
            this.options.Timeout = timeout;
        }

        public CommittableTransaction(TransactionOptions options)
        {
            this.options = options;
        }

        #region IAsyncResult Members

        object IAsyncResult.AsyncState
        {
            get { return this.user_defined_state; }
        }

        WaitHandle IAsyncResult.AsyncWaitHandle
        {
            get { return this.asyncResult.AsyncWaitHandle; }
        }

        bool IAsyncResult.CompletedSynchronously
        {
            get { return this.asyncResult.CompletedSynchronously; }
        }

        bool IAsyncResult.IsCompleted
        {
            get { return this.asyncResult.IsCompleted; }
        }

        #endregion

        public IAsyncResult BeginCommit(AsyncCallback callback,
                                        object user_defined_state)
        {
            this.callback = callback;
            this.user_defined_state = user_defined_state;

            AsyncCallback cb = null;
            if (callback != null)
            {
                cb = this.CommitCallback;
            }

            this.asyncResult = this.BeginCommitInternal(cb);
            return this;
        }

        public void EndCommit(IAsyncResult ar)
        {
            if (ar != this)
            {
                throw new ArgumentException(
                    "The IAsyncResult parameter must be the same parameter as returned by BeginCommit.", "asyncResult");
            }

            this.EndCommitInternal(this.asyncResult);
        }

        private void CommitCallback(IAsyncResult ar)
        {
            if (this.asyncResult == null && ar.CompletedSynchronously)
            {
                this.asyncResult = ar;
            }
            this.callback(this);
        }

        public void Commit()
        {
            this.CommitInternal();
        }
    }
}

#endif
