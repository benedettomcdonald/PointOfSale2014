using System;

namespace HsConnect.Employees.Workday
{
	/// <summary>
	/// Summary description for WorkdayEmployee.
	/// </summary>
	public class WorkdayEmployee : Employee
	{
		private int userNumber = -1;
		int[] jobCodes = new int[10];
		int[] access = new int[10];
		//private EmployeeStatus status;		
		//private EmpJobList empJobs;
		//private int primaryJobCode;

		public WorkdayEmployee()
		{
		}
		public WorkdayEmployee(String fn, String ln, int i, int un, int job, int ac)
		{
			PosId = i;
			userNumber = un;
			jobCodes[0] = job;
			access[0] = ac;
			FirstName = fn;
			LastName = ln;
		}

		public int UserNumber
		{
			get { return userNumber; }
			set { this.userNumber = value; }
		}
		public int[] JobCodes
		{
			get { return jobCodes; }
			set { this.jobCodes = value; }
		}
		public int[] Access
		{
			get { return access; }
			set { this.access = value; }
		}
	}
}
