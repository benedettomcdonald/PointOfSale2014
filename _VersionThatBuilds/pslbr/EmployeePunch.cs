using System;
using System.Globalization;

namespace pslbr
{
	public class EmployeePunch
	{
        public EmployeePunch(EmployeeWeek week, string clkIn, string clkOut, string date, EmployeeJob job, int type, string payType)
		{
			this.empWeek = week;

			DateTimeFormatInfo myDTFI = new CultureInfo( "en-US", false ).DateTimeFormat;
			DateTime startDate = DateTime.ParseExact( StripQuotes( date ), "MM/dd/yy", myDTFI );

			TimeSpan inTime = new TimeSpan( GetHours( clkIn ), GetMinutes( clkIn ), 0 );
			TimeSpan outTime = new TimeSpan( GetHours( clkOut ), GetMinutes( clkOut ), 0 );

			DateTime inDate = new DateTime( startDate.Year, startDate.Month, startDate.Day, inTime.Hours, inTime.Minutes, 0 );

			DateTime outDate = outTime < inTime ? startDate.AddDays( 1.0 ) : startDate.AddDays( 0 );
			outDate = outDate.AddHours( outTime.Hours );
			outDate = outDate.AddMinutes( outTime.Minutes );

			this.clockIn = inDate;
			this.clockOut = outDate;
			this.job = job;
            this.type = type;
            this.payType = payType;
		}

		private EmployeeWeek empWeek;
		private DateTime clockIn = new DateTime(0);
		private DateTime clockOut = new DateTime(0);
		private EmployeeJob job;
        private int type;
	    private string payType;

        public bool Adjusted
        {
            get { return this.type != 1; }
        }

		public DateTime ClockIn
		{
			get{ return this.clockIn; }
			set{ this.clockIn = value; }
		}

		public DateTime ClockOut
		{
			get{ return this.clockOut; }
			set{ this.clockOut = value; }
		}

		public EmployeeJob Job
		{
			get{ return this.job; }
			set{ this.job = value; }
		}

		private string StripQuotes( String str )
		{
			return str.Substring( str.IndexOf('"')+1, str.Length - 2 ).Trim();
		}

		private int GetHours( String str )
		{
			int hours = Convert.ToInt32( str.Substring( str.IndexOf('"')+1, str.Length - 6 ) );
			if( string.Compare( str.Substring( str.Length - 2, 1 ), "p", true ) == 0 && hours != 12 ) hours += 12;
			if( string.Compare( str.Substring( str.Length - 2, 1 ), "a", true ) == 0 && hours == 12 ) hours = 0; 
			return hours;
		}

		private int GetMinutes( String str )
		{
			return Convert.ToInt32( str.Substring( str.IndexOf(':')+1, 2 ) );
		}

	    public String PayType
	    {
            get { return payType; }
            set { payType = value; }
	    }

	}
}
