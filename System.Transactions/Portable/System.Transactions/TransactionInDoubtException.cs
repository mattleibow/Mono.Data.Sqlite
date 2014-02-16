//
// TransactionInDoubtException.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//

namespace System.Transactions
{
    public class TransactionInDoubtException : TransactionException
    {
        protected TransactionInDoubtException()
        {
        }

        public TransactionInDoubtException(string message)
            : base(message)
        {
        }

        public TransactionInDoubtException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
