using HsConnect.Data;
using HsConnect.Jobs;
using HsConnect.Services;
using HsConnect.Machine;
using HsSharedObjects.Client.Preferences;

using System;
using System.Data;
using System.Collections;
using Microsoft.Data.Odbc;

namespace HsConnect.EmpJobs.PosList
{
	public class PosiEmpJobList : EmpJobImpl
	{
		public PosiEmpJobList(){}

		private HsData data = new HsData();
		private JobList hsJobList;
		private static String drive = Data.PosiControl.Drive;
        private bool altFlag = false;
        private Hashtable empMapping = null;

        private void SetPreferences()
        {
            if (this.Details.Preferences.PrefExists(Preference.USE_ALTID_NOT_EXTID))
            {
                altFlag = true;
                empMapping = PosiControl.MapEmployees(false);
            }
        }

        private string GetEmpNumber(string id)
        {
            if (altFlag)
            {
                string ret = (string)empMapping[id];
                if (ret != null && ret.Length > 0)
                {
                    return ret;
                }
                else
                {
                    return id;
                }
            }
            else
            {
                return id;
            }
        }

		public override void DbInsert(){}
		public override void DbUpdate(){}

		public override void SetHsJobList(JobList hsJobs)
		{
			this.hsJobList = hsJobs;
		}

		public override void DbLoad()
		{
            SetPreferences();
			Hashtable empJobs = new Hashtable();
			try
			{
				DataTableBuilder builder = new DataTableBuilder();
				DataTable dt = builder.GetTableFromDBF( drive + @":\ALTDBF", @"C:\", "JOBFILE" );
				//DataTable dt = builder.GetTableFromDBF( @"L:\SC", @"C:\", "JOBFILE" );
				DataRowCollection rows = dt.Rows;
				Hashtable altHash = GetAltHash();						
				foreach( DataRow row in rows )
				{
					try
					{
						//String active = data.GetString( row, "ACTIVE_JOB" );
						//if( string.Compare( active, "Y", true ) == 0 )
						//{
                        bool isActiveJob = true;//default is true because i am unsure if all posi clients have this column
                        try
                        {
                            String active = data.GetString(row, "ACTIVE_JOB");
                            isActiveJob = active.ToUpper().Equals("Y");
                        }
                        catch (Exception ex)
                        {
                            logger.Error("error reading 'ACTIVE_JOB' column", ex);
                        }
                        if (isActiveJob)
                        {
                            EmpJob empJob = new EmpJob();
                            empJob.PosId = Convert.ToInt32(GetEmpNumber(""+data.GetInt(row, "EMP_NUMBER")));

                            empJob.JobCode = data.GetInt(row, "JOB_CODE");
                            //logger.Debug( "Emp Job Alt code = " + (String) altHash[ empJob.JobCode+"" ] );
                            if (this.Details.Preferences.PrefExists(Preference.POSI_ALT_JOB))
                            {
                                //logger.Debug( "Using ALT code in EmpJobs" );
                                empJob.JobCode = Convert.ToInt32((String)altHash[empJob.JobCode + ""]);
                            }
                            empJob.RegWage = data.GetFloat(row, "RATE");
                            empJob.OvtWage = empJob.RegWage * 1.5f;
                            if (hsJobList != null && hsJobList.GetJobByExtId(empJob.JobCode) != null) empJob.HsJobId = hsJobList.GetJobByExtId(empJob.JobCode).HsId;
                            if (!empJobs.ContainsKey(empJob.PosId + " | " + empJob.JobCode))
                            {
                                this.Add(empJob);
                                empJobs.Add(empJob.PosId + " | " + empJob.JobCode, empJob);
                            }
                        }
						//}
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
		}

		private Hashtable GetAltHash()
		{
			Hashtable jobs = new Hashtable();
			try
			{
				DataTableBuilder builder = new DataTableBuilder();
				DataTable dt = builder.GetTableFromDBF( drive + @":\ALTDBF", @"C:\", "JOBLIST" );
				//DataTable dt = builder.GetTableFromDBF( @"L:\SC", @"C:\", "JOBLIST" );
				DataRowCollection rows = dt.Rows;
				foreach( DataRow row in rows )
				{
					try
					{
						if( !jobs.ContainsKey( data.GetString( row , "JOB_CODE" ) ) )
						{
							jobs.Add( data.GetInt( row , "JOB_CODE" )+"", data.GetInt( row , "ALT_CODE" )+"" );
						}
					}
					catch( Exception ex )
					{
						logger.Error( ex.ToString() );
					    Main.Run.errorList.Add(ex);
					}
				}
			}
			catch( Exception ex ) 
			{
				logger.Error( ex.ToString() );
			    Main.Run.errorList.Add(ex);
			}
			return jobs;
		}
	}
}
