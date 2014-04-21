using HsConnect.Data;
using HsConnect.Jobs;
using HsConnect.Services;

using System;
using System.Data;
using System.Collections;
using Microsoft.Data.Odbc;

namespace HsConnect.EmpJobs.PosList
{
	public class RestMgrEmpJobList : EmpJobImpl
	{
		public RestMgrEmpJobList(){}

		private HsData data = new HsData();
		private JobList hsJobList;

		public override void DbInsert(){}
		public override void DbUpdate(){}

		public override void SetHsJobList(JobList hsJobs)
		{
			this.hsJobList = hsJobs;
		}

		/**Called by Employee Sync.  Restaurant Manager stores its employee job data in
		 * the EMPLOYEE.DBF file.  This method copies both the EMPLOYEE.DBF and EMPLOYEE.dbt
		 * files into the hsTemp directory.  It also needs the JOBCLASS.DBF file, so it copies
		 * it as well.  It then retrieves all relevant data from the dbf.  For RM, the job 
		 * information is stored in sets of 5 columns, EMP_JC1-5, EMP_RATE1-5 and OT_RATE1-5. 
		 * It must match the job code from JOBCLASS with the job codes in each of the 5
		 * EMP_JC columns, and then retrieve the corresponding EMP_RATE and OT_RATE data.
		 * After getting the correct data, the method creates a EmpJob object from each
		 * returned row and adds each to the EmpJobList. 
		 */ 
		public override void DbLoad()
		{
			HsFile hsFile = new HsFile();
			hsFile.Copy( this.Dsn, this.Dsn + @"\hstemp", "EMPLOYEE.DBF" );
			hsFile.Copy( this.Dsn, this.Dsn + @"\hstemp", "EMPLOYEE.dbt" );
			hsFile.Copy( this.Dsn, this.Dsn + @"\hstemp", "JOBCLASS.DBF" );
			String empCnxStr = this.cnx.ConnectionString + @"\hstemp";
			OdbcConnection newConnection = this.cnx.GetCustomOdbc( empCnxStr );
			Hashtable empJobs = new Hashtable();
			try
			{
				DataSet dataSet = new DataSet();
				OdbcDataAdapter dataAdapter = new OdbcDataAdapter(
					"SELECT EMP_NO, EMP_JC1, EMP_JC2, EMP_JC3, EMP_JC4, EMP_JC5, "
						+"EMP_RATE1, EMP_RATE2, EMP_RATE3, EMP_RATE4, EMP_RATE5, EMP_ACTIVE, "
						+"OT_RATE1, OT_RATE2, OT_RATE3, OT_RATE4, OT_RATE5, "
						+"JOB_NO, JOB_DESC, JOB_SCHED FROM EMPLOYEE, JOBCLASS "
						+"WHERE EMP_JC1 = JOB_NO or EMP_JC2 = JOB_NO OR EMP_JC3 = JOB_NO "
						+"OR EMP_JC4 = JOB_NO OR EMP_JC5 = JOB_NO" , newConnection );
				dataAdapter.Fill( dataSet , "EMPLOYEE" );
				dataAdapter.Dispose();
				DataRowCollection rows = dataSet.Tables[0].Rows;
				foreach( DataRow row in rows )
				{
					try
					{
						EmpJob empJob = new EmpJob();
						empJob.PosId = data.GetInt( row , "EMP_NO" );
						empJob.JobCode = data.GetInt( row , "JOB_NO" );
						int jcNum = -1;
						for(int i = 1; i <=5; i++)
						{
							if(data.GetInt( row , "EMP_JC"+i ) == empJob.JobCode )
								jcNum = i;
						}
						empJob.RegWage = data.GetFloat( row , "EMP_RATE"+jcNum );
						float otRate = data.GetFloat( row , "OT_RATE"+jcNum );
						if(otRate  == 0)
							otRate = empJob.RegWage * 1.5f;
						empJob.OvtWage = otRate;			
						empJob.JobName = data.GetString( row , "JOB_DESC" );
						if( hsJobList != null && hsJobList.GetJobByExtId( empJob.JobCode ) != null ) 
							empJob.HsJobId = hsJobList.GetJobByExtId( empJob.JobCode ).HsId;
						this.Add( empJob );
						if ( !empJobs.ContainsKey( empJob.PosId + " | " + empJob.JobCode ) ) 
							empJobs.Add( empJob.PosId + " | " + empJob.JobCode , empJob );
					}
					catch( Exception ex ) 
					{
						logger.Error( ex.ToString() );
					}
				}
			}
			catch( Exception ex ) 
			{
				logger.Error( ex.ToString() );
			}
			finally
			{
				newConnection.Close();
			}
		}
	}
}
