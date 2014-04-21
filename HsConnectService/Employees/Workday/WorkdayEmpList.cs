using System;

namespace HsConnect.Employees.Workday
{
	/// <summary>
	/// Summary description for WorkdayEmpList.
	/// </summary>
	public class WorkdayEmpList : EmployeeListImpl
	{
		public WorkdayEmpList()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public override void DbLoad(){ DbLoad(false);}
		public override void DbLoad( bool activeOnly ){ 
			this.Add(new WorkdayEmployee("Lars", "Testman", 3, 1, 1, 1));
			this.Add(new WorkdayEmployee("momma", "hass", 1, 1, 1, 1));
		}
		public override void DbUpdate(){}
		public override void DbInsert(){}
        public override void DbInsert(GoHireEmpInsertList ls) { }
	}
}
