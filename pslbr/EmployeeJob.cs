using System;

namespace pslbr
{
	public class EmployeeJob
	{
		public EmployeeJob( EmployeeWeek week )
		{
			this.employeeWeek = week;
			this.sortOrder = sortOrder;
		}

		private EmployeeWeek employeeWeek;
		private int jobCode = -1;
		private int jobDept = -1;
		private float rate = 0.0f;
		private float weekHours = 0.0f;
		private float weekDlrs = 0.0f;
		private int sortOrder = 0;

		public int JobCode
		{
			get{ return this.jobCode; }
			set{ this.jobCode = value; }
		}

		public int JobDept
		{
			get{ return this.jobDept; }
			set{ this.jobDept = value; }
		}

		public int SortOrder
		{
			get{ return this.sortOrder; }
			set{ this.sortOrder = value; }
		}

		public float Rate
		{
			get	{ return this.rate; }
			set{ this.rate = value; }
		}

		public float WeekHours
		{
			get	{ return this.weekHours; }
			set{ this.weekHours = value; }
		}

		public float WeekDlrs
		{
			get	{ return this.weekDlrs; }
			set{ this.weekDlrs = value; }
		}

	}
}
