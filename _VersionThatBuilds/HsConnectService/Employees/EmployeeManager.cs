using HsSharedObjects.Client;
using HsConnect.Employees.PosList;
using HsConnect.EmpJobs.PosList;
using HsConnect.EmpJobs;
using HsConnect.Jobs;
using HsConnect.Services;

using System;
using System.Reflection;

namespace HsConnect.Employees
{
	public class EmployeeManager
	{
		public EmployeeManager( ClientDetails details )
		{
			this.details = details;
		}

		private ClientDetails details;

		public EmployeeList GetPosEmployeeList()
		{
			Type type = Type.GetType( "HsConnect.Employees.PosList." + details.PosName + "EmployeeList" );
			EmployeeList empList = (EmployeeList) Activator.CreateInstance ( type );
			empList.SetDataConnection( details.GetConnectionString() );
			empList.Cnx.Dsn = details.Dsn;
			return empList;
		}

		public EmpJobList GetPosEmpJobList( JobList hsJobList )
		{
			Type type = Type.GetType( "HsConnect.EmpJobs.PosList." + details.PosName + "EmpJobList" );
			EmpJobList jobList = (EmpJobList) Activator.CreateInstance ( type );
			jobList.SetHsJobList( hsJobList );
			jobList.Dsn = details.Dsn;
			jobList.SetDataConnection( details.GetConnectionString() );
			jobList.Details = details;
			return jobList;
		}

		public HsEmployeeList GetHsEmployeeList()
		{
			HsEmployeeList empList = /*(EmployeeList)*/ new HsEmployeeList( details.ClientId );
			return empList;
		}

	}
}
