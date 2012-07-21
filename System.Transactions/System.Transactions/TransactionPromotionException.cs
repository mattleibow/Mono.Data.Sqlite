//
// TransactionPromotionException.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//



#if NET_2_0

namespace System.Transactions
{
    public class TransactionPromotionException : TransactionException
    {
        protected TransactionPromotionException()
        {
        }

        public TransactionPromotionException(string message)
            : base(message)
        {
        }

        public TransactionPromotionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

#endif
