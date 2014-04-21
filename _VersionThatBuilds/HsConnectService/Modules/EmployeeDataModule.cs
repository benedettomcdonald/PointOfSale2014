using HsSharedObjects.Client;
using HsSharedObjects.Client.Module;
using HsSharedObjects.Client.Preferences;
using HsConnect.Jobs;
using HsConnect.EmpJobs;
using HsConnect.Employees;
using HsConnect.Employees.PosList;
using HsConnect.Employees.Workday;
using HsConnect.Services;
using HsConnect.Services.Wss;
using HsConnect.Main;

using System;
using System.IO;
using System.Collections;

namespace HsConnect.Modules
{
	public class EmployeeDataModule : ModuleImpl
	{
		public EmployeeDataModule()
		{
			this.logger = new SysLog( this.GetType() );
		}

		private SysLog logger;

		public override bool Execute()
		{
	
			if( Details.ModuleList.IsActive( ClientModule.EMPLOYEE_DATA ) )
			{
				logger.Debug( "executing EmployeeDataModule" );

                if (Details.Preferences.PrefExists(Preference.ENABLE_GOHIRE_EMPLOYEE_INSERTS))
                {
                    //do it
                    try
                    {
                        insertGoHireEmployees();
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Problem inserting gohire employees, inserts may not have taken place.", ex);
                    }
                }


				EmployeeManager empManager = new EmployeeManager( Details );
				HsEmployeeList hsList = empManager.GetHsEmployeeList();
				logger.Debug( "executing EmployeeDataModule:hsList.DBLOAD" );
				hsList.DbLoad();
                if (Details.PosName.Equals("AppleDos") && Details.Preferences.PrefExists(1022))
                {
                    logger.Debug("Apple DOS expbin run");
                    System.Diagnostics.Process proc = new System.Diagnostics.Process();
                    proc.EnableRaisingEvents = false;
                    proc.StartInfo.WorkingDirectory = @"C:\network\touchit\data2\";
                    proc.StartInfo.FileName = @"expbin.exe";
                    proc.Start();
                    proc.WaitForExit();
                    logger.Debug("exited EXPBIN.exe process");
                }
				EmployeeList posList = empManager.GetPosEmployeeList();
				posList.Details = this.Details;
				logger.Debug( "executing EmployeeDataModule:posList.DBLOAD" );
				posList.DbLoad();
                logger.Debug("POS EMP LIST:  "  + posList.GetXmlString());
				JobList hsJobList = (JobList) new HsJobList( Details.ClientId );
				hsJobList.DbLoad();
                logger.Debug("HS JOB LIST:  " + hsJobList.GetXmlString());
                JobManager jobManager = new JobManager( Details );
				JobList posJobList = jobManager.GetPosJobList();
				posJobList.DbLoad();
                logger.Debug("POS JOB LIST:  " + posJobList.GetXmlString());
				EmpJobList posEmpJob = empManager.GetPosEmpJobList( hsJobList );
				posEmpJob.DbLoad();
                logger.Debug("POS EMPJOB LIST:  " + posEmpJob.GetXmlString());

                if (Details.Preferences.PrefExists(Preference.SYNC_ONLY_EMPS_WITH_JOBCODES))
                {
                    RemoveEmployeesWithoutJobs(posList, posEmpJob);
                }


				ClientJobsWss jobService = new ClientJobsWss();
                logger.Debug("Syncing POS JOB LIST...");
				jobService.syncClientJobs(Details.ClientId, posJobList.GetXmlString() );
				EmpSyncManager syncManager = new EmpSyncManager( Details );
				syncManager.HsList = hsList;
				syncManager.PosList = posList;
				if( Details.TransferRule == ClientDetails.POS_TO_HS || Details.TransferRule == ClientDetails.SYNC )
				{
                    logger.Debug("Syncing Emp Data, POS -> HS");
					syncManager.SyncPosEmployees();
					syncManager.UpdateHsEmployees();
                    if (this.Details.Preferences.PrefExists(HsSharedObjects.Client.Preferences.Preference.IMPORT_ADDRESSES))
					{
						if(this.Details.PosName.Equals("Micros"))
						{
							bool isHsList = hsList.GetType().Equals(typeof(HsEmployeeList));
							logger.Debug("hsList instanceof HsEmployeeList : " + isHsList);
							hsList.Cnx = posList.Cnx;
							((HsEmployeeList)hsList).AddressUpdate(posList);
						}
					}
					
				}

				if( Details.TransferRule == ClientDetails.HS_TO_POS || Details.TransferRule == ClientDetails.SYNC )
				{
                    logger.Debug("Syncing Emp Data, HS -> POS");
					syncManager.SyncHsEmployees();
					syncManager.UpdatePosEmployees();
				}
				ClientEmployeesWss empService = new ClientEmployeesWss();
				int empJobs = empService.syncUserJobs( Details.ClientId , posEmpJob.GetXmlString() );
				logger.Debug( "synchronized["+empJobs+"] employee jobs" );
				
				

				RemoteLogger.Log( Details.ClientId, RemoteLogger.IMPORT_EMPLOYEE_SUCCESS );
                if (Details.PosName.Equals("AppleDos") && Details.Preferences.PrefExists(1022))
                {
                    logger.Debug("deleting EXPORT files");
                    foreach (String f in Directory.GetFiles(@"C:\network\touchit\data2\export\", "*.*"))
                    {
                        File.Delete(f);
                    }
                }
			}
			logger.Debug( "executed EmployeeDataModule" );

			return true;
		}

        private void RemoveEmployeesWithoutJobs(EmployeeList posList, EmpJobList posEmpJob)
        {
            Employee[] myEmps = posList.ToArray();
            for (int i = 0; i < myEmps.Length; i++)
            {
                Employee myEmp = myEmps[i];
                //remove from list only if no jobs, and emp is active. we want jobless terminated emps to still sync as term
                if ((!(myEmp.Status.StatusCode == -1)) && !(posEmpJob.JobExistsForEmp(myEmp.PosId)))
                {
                    posList.Remove(myEmp);
                }
            }
        }

        private void insertGoHireEmployees()
        {
            logger.Debug("Begin insertGoHireEmployees()");
            ClientEmployeesWss wss = new ClientEmployeesWss();
            string xmlStr = wss.syncGoHireEmployees(Details.ClientId);

            logger.Debug("syncGoHireEmployees xml: " + xmlStr);

            GoHireEmpInsertList list = null;

            try
            {
                list = new GoHireEmpInsertList(Details.ClientId);
                list.parseXml(xmlStr);
            }
            catch (Exception ex)
            {
                logger.Error("Problem parsing employee insert xml from HS, bailing out on these gohire inserts", ex);
                return;
            }

            try
            {
                //insert employee data
                EmployeeManager empManager = new EmployeeManager(Details);
                EmployeeList posList = empManager.GetPosEmployeeList();
                logger.Debug("Obtained posList, " + posList.ToString());
                posList.DbInsert(list);
            }
            catch (Exception ex)
            {
                logger.Error("Error when performing DbInsert, insert may not have completed successfully. Statuses will still be reported back to HS.", ex);
            }

            try
            {
                String insertStatusXml = list.getInsertStatuses();
                logger.Debug("Reporting insert statuses: " + insertStatusXml);
                wss.checkGoHireEmployeeInsertStatuses(Details.ClientId, insertStatusXml);
            }
            catch (Exception ex)
            {
                logger.Error("Error when reporting insert statuses back to hotSchedules, bailing out on finalizing this insert operation.", ex);
            }

            logger.Debug("End insertGoHireEmployees()");
        }//insertGoHireEmployees()
	}
}
