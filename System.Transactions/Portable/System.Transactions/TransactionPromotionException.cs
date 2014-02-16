//
// TransactionPromotionException.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//

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
