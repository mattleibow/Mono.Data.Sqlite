//
// TransactionInDoubtException.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//

#if NET_2_0

namespace System.Transactions
{
	public class TransactionInDoubtException : TransactionException
	{
		protected TransactionInDoubtException ()
		{
		}

		public TransactionInDoubtException (string message)
			: base (message)
		{
		}

		public TransactionInDoubtException (string message, Exception innerException)
			: base (message, innerException)
		{
		}

	}
}

#endif
