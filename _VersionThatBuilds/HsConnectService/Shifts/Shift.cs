using System;

namespace HsConnect.Shifts
{
	public class Shift
	{
		public Shift()
		{			
		}

		private int posId = -1;
		private int posEmpId = -1;
		private int posJobId = -1;
		private int locId = -1;
		private String locName = "";
		private String schedName = "";
		private DateTime clockIn = new DateTime(0);
		private DateTime clockOut = new DateTime(0);
        private bool isPosted;

		//use client shifts
		private int clientShift = 0;

		public int PosId
		{
			get {return this.posId;}
			set {this.posId = value;}
		}

		public int PosEmpId
		{
			get {return this.posEmpId;}
			set {this.posEmpId = value;}
		}
		
		public int PosJobId
		{
			get {return this.posJobId;}
			set {this.posJobId = value;}
		}

		public int LocId
		{
			get {return this.locId;}
			set {this.locId = value;}
		}

		public String LocName
		{
			get {return this.locName;}
			set {this.locName = value;}
		}

		public String SchedName
		{
			get {return this.schedName;}
			set {this.schedName = value;}
		}

		public int ClientShift
		{
			get {return this.clientShift;}
			set {this.clientShift = value;}
		}

		public DateTime ClockIn
		{
			get{ return this.clockIn;}
			set{ this.clockIn = value;}
		}

		public DateTime ClockOut
		{
			get{ return this.clockOut;}
			set{ this.clockOut = value;}
		}

        public bool IsPosted
        {
            get { return isPosted; }
            set { isPosted = value; }

        }

		public override String ToString()
		{
			return "Emp:" + posEmpId + " | Job:" + posJobId + " | In:" + ClockIn + " | Out:" + ClockOut;
		}
	}
}
