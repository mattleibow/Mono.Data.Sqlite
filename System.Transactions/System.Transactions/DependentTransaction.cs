//
// DependentTransaction.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//



#if NET_2_0

namespace System.Transactions
{
    [MonoTODO("Not supported yet")]
    public sealed class DependentTransaction : Transaction
    {
        //		Transaction parent;
        //		DependentCloneOption option;
        private bool completed;

        internal DependentTransaction(Transaction parent,
                                      DependentCloneOption option)
        {
            //			this.parent = parent;
            //			this.option = option;
        }

        internal bool Completed
        {
            get { return this.completed; }
        }

        [MonoTODO]
        public void Complete()
        {
            throw new NotImplementedException();
        }
    }
}

#endif
