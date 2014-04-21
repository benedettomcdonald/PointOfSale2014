using System;
using System.Collections;

namespace HsConnect.Data.Punch2
{
	public class Punch2TimeCard
	{
		public Punch2TimeCard( Punch2Item item )
		{
			this.serialIn = item.SerialNumber;
			this.empNumber = item.EmpNumber;
			this.jobNumber = item.JobNumber;
			this.date = item.Date;
			this.inTime = item.InTime;
		}

		private int serialIn = -1;
		private int serialOut = -1;
		private int empNumber = -1;
		private int jobNumber = -1;
		private DateTime date = new DateTime(0);
		private TimeSpan inTime = new TimeSpan(0);
		private TimeSpan outTime = new TimeSpan(0);

		private ArrayList adjustments = new ArrayList();

		public int SerialNumberIn
		{
			get{ return serialIn; }
			set{ this.serialIn = value; }
		}

		public int SerialNumberOut
		{
			get{ return serialOut; }
			set{ this.serialOut = value; }
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
