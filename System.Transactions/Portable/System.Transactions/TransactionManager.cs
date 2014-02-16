//
// TransactionManager.cs
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
    public static class TransactionManager
    {
        private static readonly TimeSpan defaultTimeout = new TimeSpan(0, 1, 0); /* 60 secs */
        private static readonly TimeSpan maxTimeout = new TimeSpan(0, 10, 0); /* 10 mins */

        static TransactionManager()
        {
        }

        public static TimeSpan DefaultTimeout
        {
            get
            {
                // Obtain timeout from configuration setting..
                //		- http://msdn.microsoft.com/en-us/library/ms973865.aspx
                //		- http://sankarsan.wordpress.com/2009/02/01/transaction-timeout-in-systemtransactions/
                //	1. sys.txs/defaultSettings[@timeout]
                //	2. defaultTimeout
                return defaultTimeout;
            }
        }

        [MonoTODO("Not implemented")]
        public static HostCurrentTransactionCallback HostCurrentCallback
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public static TimeSpan MaximumTimeout
        {
            get { return maxTimeout; }
        }

        [MonoTODO("Not implemented")]
        public static void RecoveryComplete(Guid manager)
        {
            throw new NotImplementedException();
        }

        [MonoTODO("Not implemented")]
        public static Enlistment Reenlist(Guid manager,
                                          byte[] recoveryInfo,
                                          IEnlistmentNotification notification)
        {
            throw new NotImplementedException();
        }

        public static event TransactionStartedEventHandler
            DistributedTransactionStarted;
    }
}
