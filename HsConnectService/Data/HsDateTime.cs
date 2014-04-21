using System;

namespace HsConnect.Data
{
	/// <summary>
	/// Summary description for HsTime.
	/// </summary>
	public class HsDateTime
	{
		public HsDateTime(){}

		public HsDateTime( int day , int month , int year , int hour , int minutes )
		{
			this.dayOfMonth = day;
			this.month = month;
			this.year = year;
			this.hour = hour;
			this.minute = minutes;
		}

		private int dayOfMonth = 1;
		private int month = 1;
		private int year = 1;
		private int minute = 1;
		private int hour = 1;

		public DateTime GetDateTime()
		{
			return new DateTime( this.year , this.month , this.dayOfMonth , this.hour , this.minute , 0 );
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

	}
}
