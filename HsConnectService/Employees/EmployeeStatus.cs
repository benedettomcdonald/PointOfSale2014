using System;

namespace HsConnect.Employees
{
	public class EmployeeStatus
	{
		public EmployeeStatus(){}

		private int statusCode = -99;
		private DateTime inactiveFrom = new DateTime(1,1,1);
		private DateTime inactiveTo = new DateTime(1,1,1);
		private DateTime terminatedOn = new DateTime(1,1,1);
		private DateTime rehiredOn = new DateTime(1,1,1);
		private bool updated = false;

		public static int ACTIVE = 1;
		public static int INACTIVE = 0;
		public static int TERMINATED = -1;

		public bool Updated
		{
			get { return this.updated; }
			set { this.updated = value; }
		}

		public int StatusCode
		{
			get
			{
				return this.statusCode;
			}
			set 
			{
				this.statusCode = value; 
			}
		}

		public DateTime InactiveFrom
		{
			get { return this.inactiveFrom; }
			set { this.inactiveFrom = value; }
		}

		public DateTime InactiveTo
		{
			get { return this.inactiveTo; }
			set { this.inactiveTo = value; }
		}

		public DateTime TerminatedOn
		{
			get { return this.terminatedOn; }
			set { this.terminatedOn = value; }
		}

		public DateTime RehiredOn
		{
			get { return this.rehiredOn; }
			set { this.rehiredOn = value; }
		}

	}
}
