//
// Transaction.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//	Ankit Jain	 <JAnkit@novell.com>
//
// (C)2005 Novell Inc,
// (C)2006 Novell Inc,
//

namespace System.Transactions
{
    using System.Collections.Generic;
    using System.Threading;

    public class Transaction : IDisposable
    {
        // this is because WP does not support the [ThreadStatic]

        private static readonly Dictionary<int, Transaction> _privateInstances = new Dictionary<int, Transaction>();


        private readonly List<object> dependents = new List<object>();
        private readonly TransactionInformation info;
        private readonly IsolationLevel level;
        private bool aborted;

        /* Volatile enlistments */

        private AsyncCommit asyncCommit;
        private bool committed;
        private bool committing;
        private List<ISinglePhaseNotification> durables;

        private Exception innerException;
        private Guid tag = Guid.NewGuid();
        private List<IEnlistmentNotification> volatiles;

        internal Transaction()
        {
            this.info = new TransactionInformation();
            this.level = IsolationLevel.Serializable;
        }

        internal Transaction(Transaction other)
        {
            this.level = other.level;
            this.info = other.info;
            this.dependents = other.dependents;
            this.volatiles = other.Volatiles;
            this.durables = other.Durables;
        }

        private static Transaction ambient
        {
            get
            {
                lock (_privateInstances)
                {
                    Transaction instance;
                    _privateInstances.TryGetValue(Thread.CurrentThread.ManagedThreadId, out instance);
                    return instance;
                }
            }

            set
            {
                lock (_privateInstances)
                {
                    if (_privateInstances.ContainsKey(Thread.CurrentThread.ManagedThreadId))
                    {
                        _privateInstances[Thread.CurrentThread.ManagedThreadId] = value;
                    }
                    else
                    {
                        _privateInstances.Add(Thread.CurrentThread.ManagedThreadId, value);
                    }
                }
            }
        }

        private List<IEnlistmentNotification> Volatiles
        {
            get
            {
                if (this.volatiles == null)
                {
                    this.volatiles = new List<IEnlistmentNotification>();
                }
                return this.volatiles;
            }
        }

        private List<ISinglePhaseNotification> Durables
        {
            get
            {
                if (this.durables == null)
                {
                    this.durables = new List<ISinglePhaseNotification>();
                }
                return this.durables;
            }
        }

        public static Transaction Current
        {
            get
            {
                EnsureIncompleteCurrentScope();
                return CurrentInternal;
            }
            set
            {
                EnsureIncompleteCurrentScope();
                CurrentInternal = value;
            }
        }

        internal static Transaction CurrentInternal
        {
            get { return ambient; }
            set { ambient = value; }
        }

        public IsolationLevel IsolationLevel
        {
            get
            {
                EnsureIncompleteCurrentScope();
                return this.level;
            }
        }

        public TransactionInformation TransactionInformation
        {
            get
            {
                EnsureIncompleteCurrentScope();
                return this.info;
            }
        }

        private bool Aborted
        {
            get { return this.aborted; }
            set
            {
                this.aborted = value;
                if (this.aborted)
                {
                    this.info.Status = TransactionStatus.Aborted;
                }
            }
        }

        internal TransactionScope Scope { get; set; }

        #region IDisposable Members

        public void Dispose()
        {
            this.Dispose(true);
        }

        #endregion

        protected virtual void Dispose(bool includeManaged)
        {
            if (includeManaged)
            {
                if (this.TransactionInformation.Status == TransactionStatus.Active)
                {
                    this.Rollback();
                }
            }
        }

        public event TransactionCompletedEventHandler TransactionCompleted;

        public Transaction Clone()
        {
            return new Transaction(this);
        }

        [MonoTODO]
        public DependentTransaction DependentClone(DependentCloneOption option)
        {
            var d = new DependentTransaction(this, option);
            this.dependents.Add(d);
            return d;
        }

        [MonoTODO("Only SinglePhase commit supported for durable resource managers.")]
        public Enlistment EnlistDurable(Guid manager,
                                        IEnlistmentNotification notification,
                                        EnlistmentOptions options)
        {
            throw new NotImplementedException("Only SinglePhase commit supported for durable resource managers.");
        }

        [MonoTODO(
            "Only Local Transaction Manager supported. Cannot have more than 1 durable resource per transaction. Only EnlistmentOptions.None supported yet."
            )]
        public Enlistment EnlistDurable(Guid manager,
                                        ISinglePhaseNotification notification,
                                        EnlistmentOptions options)
        {
            List<ISinglePhaseNotification> durables = this.Durables;
            if (durables.Count == 1)
            {
                throw new NotImplementedException(
                    "Only LTM supported. Cannot have more than 1 durable resource per transaction.");
            }

            EnsureIncompleteCurrentScope();

            if (options != EnlistmentOptions.None)
            {
                throw new NotImplementedException("Implement me");
            }

            durables.Add(notification);

            /* FIXME: Enlistment ?? */
            return new Enlistment();
        }

        [MonoTODO]
        public bool EnlistPromotableSinglePhase(
            IPromotableSinglePhaseNotification notification)
        {
            throw new NotImplementedException();
        }

        [MonoTODO("EnlistmentOptions being ignored")]
        public Enlistment EnlistVolatile(
            IEnlistmentNotification notification,
            EnlistmentOptions options)
        {
            return this.EnlistVolatileInternal(notification, options);
        }

        [MonoTODO("EnlistmentOptions being ignored")]
        public Enlistment EnlistVolatile(
            ISinglePhaseNotification notification,
            EnlistmentOptions options)
        {
            /* FIXME: Anything extra reqd for this? */
            return this.EnlistVolatileInternal(notification, options);
        }

        private Enlistment EnlistVolatileInternal(
            IEnlistmentNotification notification,
            EnlistmentOptions options)
        {
            EnsureIncompleteCurrentScope();
            /* FIXME: Handle options.EnlistDuringPrepareRequired */
            this.Volatiles.Add(notification);

            /* FIXME: Enlistment.. ? */
            return new Enlistment();
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Transaction);
        }

        // FIXME: Check whether this is correct (currently, GetHashCode() uses 'dependents' but this doesn't)
        private bool Equals(Transaction t)
        {
            if (ReferenceEquals(t, this))
            {
                return true;
            }
            if (ReferenceEquals(t, null))
            {
                return false;
            }
            return this.level == t.level &&
                   this.info == t.info;
        }

        public static bool operator ==(Transaction x, Transaction y)
        {
            if (ReferenceEquals(x, null))
            {
                return ReferenceEquals(y, null);
            }
            return x.Equals(y);
        }

        public static bool operator !=(Transaction x, Transaction y)
        {
            return !(x == y);
        }

        public override int GetHashCode()
        {
            return (int) this.level ^ this.info.GetHashCode() ^ this.dependents.GetHashCode();
        }

        public void Rollback()
        {
            this.Rollback(null);
        }

        public void Rollback(Exception ex)
        {
            EnsureIncompleteCurrentScope();
            this.Rollback(ex, null);
        }

        internal void Rollback(Exception ex, IEnlistmentNotification enlisted)
        {
            if (this.aborted)
            {
                this.FireCompleted();
                return;
            }

            /* See test ExplicitTransaction7 */
            if (this.info.Status == TransactionStatus.Committed)
            {
                throw new TransactionException("Transaction has already been committed. Cannot accept any new work.");
            }

            this.innerException = ex;
            var e = new Enlistment();
            foreach (IEnlistmentNotification prep in this.Volatiles)
            {
                if (prep != enlisted)
                {
                    prep.Rollback(e);
                }
            }

            List<ISinglePhaseNotification> durables = this.Durables;
            if (durables.Count > 0 && durables[0] != enlisted)
            {
                durables[0].Rollback(e);
            }

            this.Aborted = true;

            this.FireCompleted();
        }

        protected IAsyncResult BeginCommitInternal(AsyncCallback callback)
        {
            if (this.committed || this.committing)
            {
                throw new InvalidOperationException("Commit has already been called for this transaction.");
            }

            this.committing = true;

            this.asyncCommit = this.DoCommit;
            return this.asyncCommit.BeginInvoke(callback, null);
        }

        protected void EndCommitInternal(IAsyncResult ar)
        {
            this.asyncCommit.EndInvoke(ar);
        }

        internal void CommitInternal()
        {
            if (this.committed || this.committing)
            {
                throw new InvalidOperationException("Commit has already been called for this transaction.");
            }

            this.committing = true;

            try
            {
                this.DoCommit();
            }
            catch (TransactionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new TransactionAbortedException("Transaction failed", ex);
            }
        }

        private void DoCommit()
        {
            /* Scope becomes null in TransactionScope.Dispose */
            if (this.Scope != null)
            {
                /* See test ExplicitTransaction8 */
                this.Rollback(null, null);
                this.CheckAborted();
            }

            List<IEnlistmentNotification> volatiles = this.Volatiles;
            List<ISinglePhaseNotification> durables = this.Durables;
            if (volatiles.Count == 1 && durables.Count == 0)
            {
                /* Special case */
                var single = volatiles[0] as ISinglePhaseNotification;
                if (single != null)
                {
                    this.DoSingleCommit(single);
                    this.Complete();
                    return;
                }
            }

            if (volatiles.Count > 0)
            {
                this.DoPreparePhase();
            }

            if (durables.Count > 0)
            {
                this.DoSingleCommit(durables[0]);
            }

            if (volatiles.Count > 0)
            {
                this.DoCommitPhase();
            }

            this.Complete();
        }

        private void Complete()
        {
            this.committing = false;
            this.committed = true;

            if (!this.aborted)
            {
                this.info.Status = TransactionStatus.Committed;
            }

            this.FireCompleted();
        }

        internal void InitScope(TransactionScope scope)
        {
            /* See test NestedTransactionScope10 */
            this.CheckAborted();

            /* See test ExplicitTransaction6a */
            if (this.committed)
            {
                throw new InvalidOperationException("Commit has already been called on this transaction.");
            }

            this.Scope = scope;
        }

        private static void PrepareCallbackWrapper(object state)
        {
            var enlist = state as PreparingEnlistment;
            enlist.EnlistmentNotification.Prepare(enlist);
        }

        private void DoPreparePhase()
        {
            // Call prepare on all volatile managers.
            foreach (IEnlistmentNotification enlist in this.Volatiles)
            {
                var pe = new PreparingEnlistment(this, enlist);
                ThreadPool.QueueUserWorkItem(PrepareCallbackWrapper, pe);

                /* Wait (with timeout) for manager to prepare */
                TimeSpan timeout = this.Scope != null ? this.Scope.Timeout : TransactionManager.DefaultTimeout;

                // FIXME: Should we managers in parallel or on-by-one?
                if (!pe.WaitHandle.WaitOne(timeout))
                {
                    this.Aborted = true;
                    throw new TimeoutException("Transaction timedout");
                }

                if (!pe.IsPrepared)
                {
                    /* FIXME: if not prepared & !aborted as yet, then 
						this is inDoubt ? . For now, setting aborted = true */
                    this.Aborted = true;
                    break;
                }
            }

            /* Either InDoubt(tmp) or Prepare failed and
			   Tx has rolledback */
            this.CheckAborted();
        }

        private void DoCommitPhase()
        {
            foreach (IEnlistmentNotification enlisted in this.Volatiles)
            {
                var e = new Enlistment();
                enlisted.Commit(e);
                /* Note: e.Done doesn't matter for volatile RMs */
            }
        }

        private void DoSingleCommit(ISinglePhaseNotification single)
        {
            if (single == null)
            {
                return;
            }

            var enlistment = new SinglePhaseEnlistment(this, single);
            single.SinglePhaseCommit(enlistment);
            this.CheckAborted();
        }

        private void CheckAborted()
        {
            if (this.aborted)
            {
                throw new TransactionAbortedException("Transaction has aborted", this.innerException);
            }
        }

        private void FireCompleted()
        {
            TransactionCompletedEventHandler handler = this.TransactionCompleted;

            if (handler != null)
            {
                handler(this, new TransactionEventArgs(this));
            }
        }

        private static void EnsureIncompleteCurrentScope()
        {
            if (CurrentInternal == null)
            {
                return;
            }
            if (CurrentInternal.Scope != null && CurrentInternal.Scope.IsComplete)
            {
                throw new InvalidOperationException("The current TransactionScope is already complete");
            }
        }

        #region Nested type: AsyncCommit

        private delegate void AsyncCommit();

        #endregion
    }
}
