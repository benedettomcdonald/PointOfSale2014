using HsConnect.Data;
using HsConnect.Jobs;
using HsConnect.Services;

using System;
using System.Data;
using System.Collections;
using Microsoft.Data.Odbc;

namespace HsConnect.EmpJobs.PosList
{
	public class MicrosEmpJobList : EmpJobImpl
	{
		public MicrosEmpJobList(){}

		private HsData data = new HsData();
		private JobList hsJobList;

		public override void DbInsert(){}
		public override void DbUpdate(){}

		public override void SetHsJobList(JobList hsJobs)
		{
			this.hsJobList = hsJobs;
		}

		public override void DbLoad()
		{
			OdbcConnection newConnection = this.cnx.GetOdbc();
			Hashtable empJobs = new Hashtable();
			try
			{
				DataSet dataSet = new DataSet();
				OdbcDataAdapter dataAdapter = new OdbcDataAdapter(
					"SELECT a.emp_seq AS emp_id , a.job_seq , c.obj_num , c.deflt_reg_pay_rate , "+
					"override_reg_pay_rate, ob_primary_job from micros.job_rate_def a , micros.emp_def b , micros.job_def c "+
					"WHERE a.job_seq = c.job_seq AND a.emp_seq = b.emp_seq AND b.termination_date IS NULL" , newConnection );
				dataAdapter.Fill( dataSet , "micros.job_def" );
				dataAdapter.Dispose();
				DataRowCollection rows = dataSet.Tables[0].Rows;
				foreach( DataRow row in rows )
				{
					try
					{
						EmpJob empJob = new EmpJob();
						empJob.PosId = data.GetInt( row , "emp_id" );
						empJob.JobCode = data.GetInt( row , "obj_num" );
						empJob.Primary = (data.GetString( row , "ob_primary_job" ).Equals("Y")? true:false);
						empJob.RegWage = ((float) data.GetDouble( row , "override_reg_pay_rate" )) == 0 ? (float) data.GetDouble( row , "deflt_reg_pay_rate" ) : (float) data.GetDouble( row , "override_reg_pay_rate" );
						if( hsJobList != null && hsJobList.GetJobByExtId( empJob.JobCode ) != null ) empJob.HsJobId = hsJobList.GetJobByExtId( empJob.JobCode ).HsId;
						this.Add( empJob );
						empJobs.Add( empJob.PosId + " | " + empJob.JobCode , empJob );
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
			try
			{
				DataSet dataSet = new DataSet();
				OdbcDataAdapter dataAdapter = new OdbcDataAdapter(
					"SELECT a.emp_seq as emp_id,  c.obj_num, a.override_otm_pay_rate as override_otm_rate, b.deflt_otm_pay_rate as deflt_otm_rate FROM micros.emp_job_otm_lvl_def a , "+
					"micros.job_otm_lvl_def b, micros.job_def c, d.micros.emp_def d WHERE a.job_seq = b.job_seq "+
					"and c.job_seq = a.job_seq and d.emp_seq = a.emp_seq and	termination_date IS NULL" , newConnection );
				dataAdapter.Fill( dataSet , "micros.job_def" );
				dataAdapter.Dispose();
				DataRowCollection rows = dataSet.Tables[0].Rows;
				foreach( DataRow row in rows )
				{
					try
					{
						int empId = data.GetInt( row , "emp_id" );
						int jobId = data.GetInt( row , "obj_num" );
						float otRate = ((float) data.GetDouble( row , "override_otm_rate" )) == 0 ? (float) data.GetDouble( row , "deflt_otm_rate" ) : (float) data.GetDouble( row , "override_otm_rate" );
						if( empJobs.ContainsKey( empId + " | " + jobId ) )
						{
							EmpJob eJob = (EmpJob) empJobs[ empId + " | " + jobId ];
							eJob.OvtWage = otRate;
						}						
					}
					catch( Exception ex ) 
					{
						logger.Error( ex.ToString() );
					//	Main.Run.errorList.Add(ex);
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
