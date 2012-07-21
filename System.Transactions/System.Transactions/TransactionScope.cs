//
// TransactionScope.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//	Ankit Jain	 <JAnkit@novell.com>
//
// (C)2005 Novell Inc,
// (C)2006 Novell Inc,
//

#if NET_2_0

using DTCOption = System.Transactions.EnterpriseServicesInteropOption;

namespace System.Transactions
{
    public sealed class TransactionScope : IDisposable
    {
        private static readonly TransactionOptions defaultOptions =
            new TransactionOptions(0, TransactionManager.DefaultTimeout);

        private bool completed;
        private bool disposed;
        private bool isRoot;
        private int nested;

        private Transaction oldTransaction;
        private TransactionScope parentScope;
        private TimeSpan timeout;
        private Transaction transaction;

        /* Num of non-disposed nested scopes */

        public TransactionScope()
            : this(TransactionScopeOption.Required,
                   TransactionManager.DefaultTimeout)
        {
        }

        public TransactionScope(Transaction transaction)
            : this(transaction, TransactionManager.DefaultTimeout)
        {
        }

        public TransactionScope(Transaction transaction,
                                TimeSpan timeout)
            : this(transaction, timeout, DTCOption.None)
        {
        }

        [MonoTODO("EnterpriseServicesInteropOption not supported.")]
        public TransactionScope(Transaction transaction,
                                TimeSpan timeout, DTCOption opt)
        {
            this.Initialize(TransactionScopeOption.Required,
                            transaction, defaultOptions, opt, timeout);
        }

        public TransactionScope(TransactionScopeOption option)
            : this(option, TransactionManager.DefaultTimeout)
        {
        }

        [MonoTODO("No TimeoutException is thrown")]
        public TransactionScope(TransactionScopeOption option,
                                TimeSpan timeout)
        {
            this.Initialize(option, null, defaultOptions,
                            DTCOption.None, timeout);
        }

        public TransactionScope(TransactionScopeOption scopeOption,
                                TransactionOptions options)
            : this(scopeOption, options, DTCOption.None)
        {
        }

        [MonoTODO("EnterpriseServicesInteropOption not supported")]
        public TransactionScope(TransactionScopeOption scopeOption,
                                TransactionOptions options,
                                DTCOption opt)
        {
            this.Initialize(scopeOption, null, options, opt,
                            TransactionManager.DefaultTimeout);
        }

        internal bool IsComplete
        {
            get { return this.completed; }
        }

        internal TimeSpan Timeout
        {
            get { return this.timeout; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;

            if (this.parentScope != null)
            {
                this.parentScope.nested --;
            }

            if (this.nested > 0)
            {
                this.transaction.Rollback();
                throw new InvalidOperationException("TransactionScope nested incorrectly");
            }

            if (Transaction.CurrentInternal != this.transaction)
            {
                if (this.transaction != null)
                {
                    this.transaction.Rollback();
                }
                if (Transaction.CurrentInternal != null)
                {
                    Transaction.CurrentInternal.Rollback();
                }

                throw new InvalidOperationException("Transaction.Current has changed inside of the TransactionScope");
            }

            if (Transaction.CurrentInternal == this.oldTransaction && this.oldTransaction != null)
            {
                this.oldTransaction.Scope = this.parentScope;
            }

            Transaction.CurrentInternal = this.oldTransaction;

            if (this.transaction == null)
            {
                /* scope was not in a transaction, (Suppress) */
                return;
            }

            this.transaction.Scope = null;

            if (!this.IsComplete)
            {
                this.transaction.Rollback();
                return;
            }

            if (!this.isRoot)
            {
                /* Non-root scope has completed+ended */
                return;
            }

            this.transaction.CommitInternal();
        }

        #endregion

        private void Initialize(TransactionScopeOption scopeOption,
                                Transaction tx, TransactionOptions options,
                                DTCOption interop, TimeSpan timeout)
        {
            this.completed = false;
            this.isRoot = false;
            this.nested = 0;
            this.timeout = timeout;

            this.oldTransaction = Transaction.CurrentInternal;

            Transaction.CurrentInternal = this.transaction = this.InitTransaction(tx, scopeOption);
            if (this.transaction != null)
            {
                this.transaction.InitScope(this);
            }
            if (this.parentScope != null)
            {
                this.parentScope.nested ++;
            }
        }

        private Transaction InitTransaction(Transaction tx, TransactionScopeOption scopeOption)
        {
            if (tx != null)
            {
                return tx;
            }

            if (scopeOption == TransactionScopeOption.Suppress)
            {
                if (Transaction.CurrentInternal != null)
                {
                    this.parentScope = Transaction.CurrentInternal.Scope;
                }
                return null;
            }

            if (scopeOption == TransactionScopeOption.Required)
            {
                if (Transaction.CurrentInternal == null)
                {
                    this.isRoot = true;
                    return new Transaction();
                }

                this.parentScope = Transaction.CurrentInternal.Scope;
                return Transaction.CurrentInternal;
            }

            /* RequiresNew */
            if (Transaction.CurrentInternal != null)
            {
                this.parentScope = Transaction.CurrentInternal.Scope;
            }
            this.isRoot = true;
            return new Transaction();
        }

        public void Complete()
        {
            if (this.completed)
            {
                throw new InvalidOperationException(
                    "The current TransactionScope is already complete. You should dispose the TransactionScope.");
            }

            this.completed = true;
        }
    }
}

#endif
