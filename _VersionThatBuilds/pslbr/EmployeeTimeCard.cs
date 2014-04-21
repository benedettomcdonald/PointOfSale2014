using System;

namespace pslbr
{
	public class EmployeeTimeCard
	{
		public EmployeeTimeCard( int empId )
		{
			this.empId = empId;
		}

		private int empId = -1;
		private int altCode = -1;
		private int jobDept = -1;
		private DateTime clockIn = new DateTime(0);
		private DateTime clockOut = new DateTime(0);
		private float regHours = 0.0f;
		private float otHours = 0.0f;
		private float regDollars = 0.0f;
		private float otDollars = 0.0f;
		private float spcHours = 0.0f;
        private float spcDollars = 0.0f; 
        private bool adjusted;
	    private string payType;

	    public bool Adjusted
        {
            get { return this.adjusted; }
            set { this.adjusted = value; }
        }

		public int EmpId
		{
			get{ return this.empId; }
			set{ this.empId = value; }
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

		public int AltCode
		{
			get{ return this.altCode; }
			set{ this.altCode = value; }
		}

		public int JobDept
		{
			get{ return this.jobDept; }
			set{ this.jobDept = value; }
		}

		public float RegHours
		{
			get{ return this.regHours; }
			set{ this.regHours = value; }
		}

		public float OtHours
		{
			get{ return this.otHours; }
			set{ this.otHours = value; }
		}

		public float RegDollars
		{
			get{ return this.regDollars; }
			set{ this.regDollars = value; }
		}

		public float OtDollars
		{
			get{ return this.otDollars; }
			set{ this.otDollars = value; }
		}

		public float SpcHours
		{
			get{ return this.spcHours; }
			set{ this.spcHours = value; }
		}

		public float SpcDollars
		{
			get{ return this.spcDollars; }
			set{ this.spcDollars = value; }
		}

	    public string PayType
	    {
            get { return this.payType; }
	        set { this.payType = value; }
	    }
	}
}
