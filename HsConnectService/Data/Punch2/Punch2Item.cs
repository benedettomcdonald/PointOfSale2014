using System;

namespace HsConnect.Data.Punch2
{
	public class Punch2Item
	{
		public Punch2Item( int serial, int emp, int job, int type )
		{
			this.SerialNumber = serial;
			this.EmpNumber = emp;
			this.JobNumber = job;
			this.PunchType = type;
		}

		public static int PUNCH_IN = 1;
		public static int PUNCH_OUT = 2;
		public static int REJECTED = 3;
		public static int ADJUSTED = 4;

		private int serialNumber = -1;
		private int empNumber = -1;
		private int jobNumber = -1;
		private int punchType = -1;
		private DateTime date = new DateTime(0);
		private TimeSpan inTime = new TimeSpan(0);
		private TimeSpan outTime = new TimeSpan(0);

		public int SerialNumber
		{
			get{ return serialNumber; }
			set{ this.serialNumber = value; }
		}

		public int EmpNumber
		{
			get{ return this.empNumber; }
			set{ this.empNumber = value; }
		}

		public int JobNumber
		{
			get{ return this.jobNumber; }
			set{ this.jobNumber = value; }
		}

		public int PunchType
		{
			get{ return this.punchType; }
			set{ this.punchType = value; }
		}

		public DateTime Date
		{
			get{ return this.date; }
			set{ this.date = value; }
		}

		public TimeSpan InTime
		{
			get{ return this.inTime; }
			set{ this.inTime = value; }
		}

		public TimeSpan OutTime
		{
			get{ return this.outTime; }
			set{ this.outTime = value; }
		}
	}
}
