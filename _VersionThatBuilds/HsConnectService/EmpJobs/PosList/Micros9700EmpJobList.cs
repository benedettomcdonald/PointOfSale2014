using HsConnect.Data;
using HsConnect.Jobs;
using HsConnect.Services;

using System;
using System.Data;
using System.Collections;
using Microsoft.Data.Odbc;


namespace HsConnect.EmpJobs.PosList
{
	public class Micros9700EmpJobList : EmpJobImpl
	{
		public Micros9700EmpJobList(){}

		private HsData data = new HsData();
		private JobList hsJobList;
		
		//STATICS
		//private static String JOBFILE = "empl_job_def";
		//EMP_NUMBER, JOB_CODE, REG_RATE, OVT_RATE, JOB_NAME correspond to
		//column names in the data table.  These must be changed if the 
		//structure of the data table or sql query is changed
		public static String EMP_NUMBER = "Row1";
		public static String JOB_CODE = "Row2";
		public static String REG_RATE = "Row3";
		public static String OVT_RATE = "Row4";
		public static String JOB_NAME = "Row5";
		public static String RATE_NUM = "Row6";
		
		#region unused methods
		public override void DbInsert()
		{
			Console.WriteLine("This is the 9700 DBInsert method for EmpJobList");
		}
		public override void DbUpdate()
		{
			Console.WriteLine("This is the 9700 DBUpdate method for EmpJobList");
		}
		#endregion

		public override void SetHsJobList(JobList hsJobs)
		{
			this.hsJobList = hsJobs;
		}

		/**Used with Employee Sync.  This method will query the 9700 database 
		 * and will create csv files from it.
		 * These csv files will be used to populate all of the Employee objects
		 * for the EmployeeList object
		 **/
		public override void DbLoad()
		{
			Hashtable empJobs = new Hashtable();
			DataTableBuilder builder = new DataTableBuilder();
			Micros9700Control.CombineFiles(Micros9700Control.JOBS);
			DataTable dt = builder.GetTableFromCSV( Micros9700Control.PATH_NAME,  Micros9700Control.JOB_FILE );
			DataRowCollection rows = dt.Rows;
			foreach( DataRow row in rows )
			{
				try
				{
					EmpJob empJob = new EmpJob();
					empJob.PosId = data.GetInt( row , EMP_NUMBER );
					empJob.JobCode = data.GetInt( row , JOB_CODE );
					empJob.RegWage = data.GetFloat( row , REG_RATE );
					empJob.OvtWage = data.GetFloat( row , OVT_RATE );
					empJob.JobName = data.GetString( row, JOB_NAME );
					if( hsJobList != null && hsJobList.GetJobByExtId( empJob.JobCode ) != null ) empJob.HsJobId = hsJobList.GetJobByExtId( empJob.JobCode ).HsId;
					if( !empJobs.ContainsKey( empJob.PosId + " | " + empJob.JobCode ) )
					{
						this.Add( empJob );
						empJobs.Add( empJob.PosId + " | " + empJob.JobCode , empJob );							
					}
				}
				catch( Exception ex )
				{
					logger.Error( ex.ToString() );
				}
			}
		}//end DbLoad
	}
}
