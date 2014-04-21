using System;

namespace HsConnect.Data.Punch2
{
	public class Punch2Adjust : Punch2Item
	{
		public Punch2Adjust( int serial, int emp, int job, int type ) : base(serial,emp,job,type)
		{
			
		}

		private int adjSerialNumber = -1;
		private int adjJobNumber = -1;
		private DateTime adjDate = new DateTime(0);
		private TimeSpan adjInTime = new TimeSpan(0);
		private TimeSpan adjOutTime = new TimeSpan(0);

		public int AdjSerialNumber
		{
			get{ return adjSerialNumber; }
			set{ this.adjSerialNumber = value; }
		}

		public int AdjJobNumber
		{
			get{ return this.adjJobNumber; }
			set{ this.adjJobNumber = value; }
		}

		public DateTime AdjDate
		{
			get{ return this.adjDate; }
			set{ this.adjDate = value; }
		}

		public TimeSpan AdjInTime
		{
			get{ return this.adjInTime; }
			set{ this.adjInTime = value; }
		}

		public TimeSpan AdjOutTime
		{
			get{ return this.adjOutTime; }
			set{ this.adjOutTime = value; }
		}
	}
}
