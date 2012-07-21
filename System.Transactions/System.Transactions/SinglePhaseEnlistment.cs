//
// SinglePhaseEnlistment.cs
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
    public class SinglePhaseEnlistment : Enlistment
    {
        //		bool committed;
        private readonly ISinglePhaseNotification enlisted;
        private readonly Transaction tx;

        internal SinglePhaseEnlistment(Transaction tx, ISinglePhaseNotification enlisted)
        {
            this.tx = tx;
            this.enlisted = enlisted;
        }

        public void Aborted()
        {
            this.Aborted(null);
        }

        public void Aborted(Exception e)
        {
            this.tx.Rollback(e, this.enlisted);
        }

        [MonoTODO]
        public void Committed()
        {
            /* FIXME */
            //			committed = true;
        }

        [MonoTODO("Not implemented")]
        public void InDoubt()
        {
            throw new NotImplementedException();
        }

        [MonoTODO("Not implemented")]
        public void InDoubt(Exception e)
        {
            throw new NotImplementedException();
        }
    }
}

#endif
