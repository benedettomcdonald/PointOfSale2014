using HsConnect.Data;
using HsConnect.Jobs;
using HsConnect.Services;

using System;
using System.Data;
using System.Collections;
using Microsoft.Data.Odbc;

namespace HsConnect.EmpJobs.PosList
{
	public class AlohaEmpJobList : EmpJobImpl
	{
		public AlohaEmpJobList(){}

		private HsData data = new HsData();
		private JobList hsJobList;
        private Hashtable empJobs = new Hashtable();

		public override void DbInsert(){}
		public override void DbUpdate(){}

        public EmpJob GetEmpJobByIds(int empId, int jobId)
        {
            if(empJobs == null)
                return null;
            Object j = empJobs[empId + " | " + jobId];
            return j == null ? null : (EmpJob)j;

        }

		public override void SetHsJobList(JobList hsJobs)
		{
			this.hsJobList = hsJobs;
		}

		public override void DbLoad()
		{
			HsFile hsFile = new HsFile();
			hsFile.Copy( this.Dsn + @"\Newdata", this.Dsn + @"\hstemp", "EMP.DBF" );
			String empCnxStr = this.cnx.ConnectionString + @"\hstemp";
			OdbcConnection newConnection = this.cnx.GetCustomOdbc( empCnxStr );
			
			try
			{
				DataSet dataSet = new DataSet();
				OdbcDataAdapter dataAdapter = new OdbcDataAdapter(
					"SELECT * FROM EMP" , newConnection );
				dataAdapter.Fill( dataSet , "EMP" );
				dataAdapter.Dispose();
				DataRowCollection rows = dataSet.Tables[0].Rows;
				foreach( DataRow row in rows )
				{
					try
					{
						for( int i=1; i<11; i++ )
						{
							String col = "JOBCODE" + i.ToString();
							if( Convert.ToInt32( row[col].ToString() ) > 0 )
							{
								EmpJob empJob = new EmpJob();
								empJob.PosId = data.GetInt( row , "ID" );
								empJob.JobCode = data.GetInt( row , col );
								empJob.RegWage = data.GetFloat( row , "PAYRATE"+i.ToString() );
								if( hsJobList != null && hsJobList.GetJobByExtId( empJob.JobCode ) != null ) empJob.HsJobId = hsJobList.GetJobByExtId( empJob.JobCode ).HsId;
								this.Add( empJob );
								if ( !empJobs.ContainsKey( empJob.PosId + " | " + empJob.JobCode ) ) empJobs.Add( empJob.PosId + " | " + empJob.JobCode , empJob );
							}
						}
					}
					catch( Exception ex ) 
					{
						logger.Error( ex.ToString() );
						//Main.Run.errorList.Add(ex);
					}
				}
			}
			catch( Exception ex ) 
			{
				logger.Error( ex.ToString() );
				Main.Run.errorList.Add(ex);
			}
			finally
			{
				newConnection.Close();
			}
		}
	}
}
