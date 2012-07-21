//
// Enlistment.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//	Ankit Jain	 <JAnkit@novell.com>
//
// (C)2005 Novell Inc,
// (C)2006 Novell Inc,
//

namespace System.Transactions
{
    public class Enlistment
    {
        internal bool done;

        internal Enlistment()
        {
            this.done = false;
        }

        public void Done()
        {
            this.done = true;
        }
    }
}
