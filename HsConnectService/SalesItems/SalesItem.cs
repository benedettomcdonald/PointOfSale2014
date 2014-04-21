using HsConnect.Main;

using System;

namespace HsConnect.SalesItems
{
	public class SalesItem
	{
		public SalesItem() {}

		private int hsId = -1;
		private long extId = -1;
		private double amount = 0.0;
		private int dayOfMonth = 1;
		private int month = 1;
		private int year = 1;
		private int minute = 1;
		private int hour = 1;
		private int rvc = -1;
	    private int category = -1;
	    private DateTime businessDate = new DateTime();

		public int HsId
		{
			get { return this.hsId; }
			set { this.hsId = value; }
		}

		public long ExtId
		{
			get { return this.extId; }
			set { this.extId = value; }
		}

		public double Amount
		{
			get { return this.amount; }
			set { this.amount = value; }
		}

		public int DayOfMonth
		{
			get { return this.dayOfMonth; }
			set { this.dayOfMonth = value; }
		}

		public int Month
		{
			get { return this.month; }
			set { this.month = value; }
		}

		public int Year
		{
			get { return this.year; }
			set { this.year = value; }
		}

		public int Hour
		{
			get { return this.hour; }
			set { this.hour = value; }
		}

		public int Minute
		{
			get { return this.minute; }
			set { this.minute = value; }
		}

		public int RVC
		{
			get{ return this.rvc; }
			set{ this.rvc = value; }
		}

	    public int Category
	    {
            get { return this.category; }
            set { this.category = value; }
	    }

	    public DateTime BusinessDate
	    {
            get { return this.businessDate; }
            set { this.businessDate = value; }
        }
	}
}
