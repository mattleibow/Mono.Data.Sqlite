//
// SubordinateTransaction.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//

namespace System.Transactions
{
    public sealed class SubordinateTransaction : Transaction
    {
        public SubordinateTransaction(IsolationLevel level,
                                      ISimpleTransactionSuperior superior)
        {
            throw new NotImplementedException();
        }
    }
}
