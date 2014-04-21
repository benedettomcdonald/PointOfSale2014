using System;
using System.Collections;
using System.IO;
using HsConnect.Employees.PosList;
using HsConnect.Main;

namespace HsConnect.Employees.Workday
{
	/// <summary>
	/// Summary description for WorkdayUtil.
	/// </summary>
	public class WorkdayUtil
	{
		public WorkdayUtil()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		private static SysLog logger = new SysLog("WorkdayUtil");

		public static bool updatePOS(WorkdayEmpList wdList, AlohaEmployeeList alohaList, String dsn)
		{
			//compare the two lists.
			//make 2 lists.
			//	1)new employees
			//	2)employees needing update
			//for list 1, alohaList.DbInsert();
			//for list 2, alohaList.DbUpdate();
			alohaList.NewEmps = new ArrayList();
			alohaList.UpdateEmps = new ArrayList();
			foreach (WorkdayEmployee emp in wdList)
			{
				Employee posEmp = alohaList.GetEmployeeByExtId(emp.PosId);
				if(posEmp == null)
				{
					posEmp = new Employee();
					posEmp.FirstName = emp.FirstName;
					posEmp.LastName = emp.LastName;
					posEmp.PosId = emp.PosId;
					EmpJobs.EmpJob job = new EmpJobs.EmpJob();
					job.PosId = emp.JobCodes[0];
//					posEmp.EmployeeJobs = new EmpJobs.PosList.AlohaEmpJobList();
//					posEmp.EmployeeJobs.Add(job);
					alohaList.NewEmps.Add(posEmp);
				}
				else if(needsUpdate(emp, posEmp)){
					logger.Log("in else");
					posEmp.FirstName = emp.FirstName;
					posEmp.LastName = emp.LastName;
					posEmp.PosId = emp.PosId;
					EmpJobs.EmpJob job = new EmpJobs.EmpJob();
//					job.PosId = emp.JobCodes[0];
//					posEmp.EmployeeJobs.Add(job);
					alohaList.UpdateEmps.Add(posEmp);
				}
			}
			if(alohaList.NewEmps.Count > 0 || alohaList.UpdateEmps.Count > 0)
				backup(dsn);
			if(alohaList.NewEmps.Count > 0)
			{
				alohaList.DbInsert();
			}
			if(alohaList.UpdateEmps.Count > 0)
			{
				alohaList.DbUpdate();
			}

			return false;
		}

		private static bool jobExists(WorkdayEmployee w, Employee e)
		{
			foreach(EmpJobs.EmpJob ej in e.EmployeeJobs)
			{
				if(ej.PosId == w.JobCodes[0])
					return true;
			}
			return false;
		}

		private static bool accessExists(WorkdayEmployee w, Employee e)
		{
			return true;
		}

		private static bool needsUpdate(WorkdayEmployee w, Employee e)
		{
			return (!
					(w.FirstName.Equals(e.FirstName) 
					&& w.LastName.Equals(e.LastName)
					//&& jobExists(emp, posEmp)
					&& accessExists(w, e))
					);
				
		}

		private static void backup( String cnxStr)
		{
			DateTime date = DateTime.Now;
			String empCnxStr = cnxStr + @"\NEWDATA";
			if(!Directory.Exists(@"C:\hstmp"))
			{
				Directory.CreateDirectory(@"C:\hstmp");
			}
			if(!Directory.Exists(@"C:\hstmp\BACKUP"))
			{
				Directory.CreateDirectory(@"C:\hstmp\BACKUP");
			}
			if(!Directory.Exists(@"C:\hstmp\BACKUP\"+DateTime.Now.Year + DateTime.Now.Month+DateTime.Now.Day))
			{
				Directory.CreateDirectory(@"C:\hstmp\BACKUP\"+DateTime.Now.Year + DateTime.Now.Month+DateTime.Now.Day);
			}
			logger.Log(empCnxStr + @"\EMP.DBF");
			logger.Log(@"C:\hstmp\BACKUP\"+date.Year + date.Month+date.Day+@"\EMP" + date.ToString( "MMddyymmss" ) + ".DBF");
			File.Copy( empCnxStr + @"\EMP.DBF" , @"C:\hstmp\BACKUP\"+date.Year + date.Month+date.Day+@"\EMP" + date.ToString( "MMddyyhhmmss" ) + ".DBF" , true);
		}
	}
}
