using System;

namespace pslbr
{
	public class LaborWeek
	{
		public LaborWeek( int ovtType, DateTime startDate )
		{
			this.ovtType = ovtType;
			this.startDate = startDate;
		}

		private int ovtType = -1;
		private DateTime startDate = new DateTime(0);
		private DateTime endDate = new DateTime(0);

		public int OvtType
		{
			get { return this.ovtType; }
			set	{ this.ovtType = value; }
		}

		public DateTime StartDate
		{
			get { return this.startDate; }
			set	{ this.startDate = value; }
		}

		public DateTime EndDate
		{
			get { return this.endDate; }
			set { this.endDate = value; }
		}

	}
}
