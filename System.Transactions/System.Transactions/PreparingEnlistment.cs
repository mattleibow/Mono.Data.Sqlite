//
// PreparingEnlistment.cs
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

    public class PreparingEnlistment : Enlistment, IDisposable
    {
        private readonly IEnlistmentNotification enlisted;
        private readonly Transaction tx;
        private readonly WaitHandle waitHandle;
        private bool prepared;

        internal PreparingEnlistment(Transaction tx, IEnlistmentNotification enlisted)
        {
            this.tx = tx;
            this.enlisted = enlisted;
            this.waitHandle = new ManualResetEvent(false);
        }

        internal bool IsPrepared
        {
            get { return this.prepared; }
        }

        internal WaitHandle WaitHandle
        {
            get { return this.waitHandle; }
        }

        internal IEnlistmentNotification EnlistmentNotification
        {
            get { return this.enlisted; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            this.Dispose(true);
        }

        #endregion

        public void ForceRollback()
        {
            this.ForceRollback(null);
        }

        [MonoTODO]
        public void ForceRollback(Exception ex)
        {
            this.tx.Rollback(ex, this.enlisted);
            /* See test RMFail2 */
            ((ManualResetEvent) this.waitHandle).Set();
        }

        [MonoTODO]
        public void Prepared()
        {
            this.prepared = true;
            /* See test RMFail2 */
            ((ManualResetEvent) this.waitHandle).Set();
        }

        [MonoTODO]
        public byte[] RecoveryInformation()
        {
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool includeManaged)
        {
            if (includeManaged)
            {
                this.waitHandle.Dispose();
            }
        }
    }
}

#endif
