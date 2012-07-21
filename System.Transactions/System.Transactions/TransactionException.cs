//
// TransactionException.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//



#if NET_2_0

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

#endif
