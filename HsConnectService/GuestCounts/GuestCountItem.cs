using System;
using System.Collections;
using HsConnect.Main;

namespace HsConnect.GuestCounts
{
	public class GuestCountItem
	{
		public GuestCountItem()	{}

		private int guestCount = -1;
		private DateTime dateTime = new DateTime( 0 );
        private Hashtable rvcTotals = new Hashtable();
        private Hashtable rvcNames = new Hashtable();
       
		public int GuestCount
		{
			get { return this.guestCount; }
			set { this.guestCount = value; }
		}

		public DateTime Date
		{
			get { return this.dateTime; }
			set { this.dateTime = value; }
		}

        public void addRvc(int total, int rvcId, String rvcName)
        {
            try
            {
                rvcTotals.Add(rvcId, total);
                rvcNames.Add(rvcId, rvcName);
            }
            catch (Exception ex)
            {
                SysLog logger = new SysLog("GuestCountIem");
                logger.Error("This RVC already exists in the hash map");
                logger.Error(ex.ToString());
            }
        }

        public Hashtable RVCTotals
        {
            get { return this.rvcTotals; }
            set { this.rvcTotals = value; }
        }
        public Hashtable RVCNames
        {
            get { return this.rvcNames; }
            set { this.rvcNames = value; }
        }
	}
}
