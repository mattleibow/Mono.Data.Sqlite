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
        private readonly DateTime creation_time;
        private readonly string local_id;
        private Guid dtcId = Guid.Empty;
        private TransactionStatus status;

        internal TransactionInformation()
        {
            this.status = TransactionStatus.Active;
            this.creation_time = DateTime.Now.ToUniversalTime();
            this.local_id = Guid.NewGuid().ToString() + ":1";
        }

        private TransactionInformation(TransactionInformation other)
        {
            this.local_id = other.local_id;
            this.dtcId = other.dtcId;
            this.creation_time = other.creation_time;
            this.status = other.status;
        }

        public DateTime CreationTime
        {
            get { return this.creation_time; }
        }

        public Guid DistributedIdentifier
        {
            get { return this.dtcId; }
            internal set { this.dtcId = value; }
        }

        public string LocalIdentifier
        {
            get { return this.local_id; }
        }

        public TransactionStatus Status
        {
            get { return this.status; }
            internal set { this.status = value; }
        }

        internal TransactionInformation Clone(
            TransactionInformation other)
        {
            return new TransactionInformation(other);
        }
    }
}

#endif
