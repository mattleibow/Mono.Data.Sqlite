//
// TransactionException.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//

namespace System.Transactions
{
    public class TransactionException : Exception
    {
        protected TransactionException()
        {
        }

        public TransactionException(string message)
            : base(message)
        {
        }

        public TransactionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
