//
// TransactionAbortedException.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//

#if NET_2_0

namespace System.Transactions
{
	public class TransactionAbortedException : TransactionException
	{
		public TransactionAbortedException ()
		{
		}

		public TransactionAbortedException (string message)
			: base (message)
		{
		}

		public TransactionAbortedException (string message, Exception innerException)
			: base (message, innerException)
		{
		}

	}
}

#endif
