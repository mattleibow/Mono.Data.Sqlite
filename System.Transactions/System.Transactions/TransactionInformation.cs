//
// TransactionInformation.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//


#if NET_2_0

namespace System.Transactions
{
    public class TransactionInformation
    {
        private string local_id;
        private Guid dtcId = Guid.Empty;
        private DateTime creation_time;
        private TransactionStatus status;

        internal TransactionInformation()
        {
            status = TransactionStatus.Active;
            creation_time = DateTime.Now.ToUniversalTime();
            local_id = Guid.NewGuid().ToString() + ":1";
        }

        private TransactionInformation(TransactionInformation other)
        {
            local_id = other.local_id;
            dtcId = other.dtcId;
            creation_time = other.creation_time;
            status = other.status;
        }

        public DateTime CreationTime
        {
            get { return creation_time; }
        }

        public Guid DistributedIdentifier
        {
            get { return dtcId; }
            internal set { dtcId = value; }
        }

        public string LocalIdentifier
        {
            get { return local_id; }
        }

        public TransactionStatus Status
        {
            get { return status; }
            internal set { status = value; }
        }

        internal TransactionInformation Clone(
            TransactionInformation other)
        {
            return new TransactionInformation(other);
        }
    }
}

#endif