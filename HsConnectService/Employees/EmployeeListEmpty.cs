using System;

namespace HsConnect.Employees
{
	public class EmployeeListEmpty : EmployeeListImpl
	{
		public EmployeeListEmpty(){}

		public override void DbLoad(){}
		public override void DbLoad( bool activeOnly ){}
		public override void DbUpdate(){}
		public override void DbInsert(){}
        public override void DbInsert(GoHireEmpInsertList ls){}
	}
}
