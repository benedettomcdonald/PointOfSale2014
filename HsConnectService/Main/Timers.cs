using HsConnect.Modules;
using HsSharedObjects;
using HsSharedObjects.Client;
using HsSharedObjects.Client.Module;
using HsSharedObjects.Client.CustomModule;
using HsConnect.Data;
using HsConnect.Services;
using HsConnect.Services.Wss;

using System;
using System.Timers;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;
using System.ServiceProcess;


namespace HsConnect.Main
{
	/// <summary>
	/// Summary description for Timers.
	/// </summary>
	public class Timers
	{
		public Timers()
		{
			
		}

		public Timers(ClientDetails details)
		{
			initializeTimers();
			this.logger = new SysLog( this.GetType() );
			this.clientDetails = details;
			this.timer1.Interval = HsSettings.GetMilliseconds( 30 );
			try
			{
				ServiceController[] services = ServiceController.GetServices();
				foreach( ServiceController serv in services )
				{
					if( serv.ServiceName.Equals( "_hscnx"))
					{
						service = serv;
						logger.Debug("found service:  "+ service.DisplayName);
						break;
					}
				}
			}
			catch( Exception ex )
			{
				logger.Error("could not get service handle:" +  ex.ToString());
			}

			this.update_module_timers();
			
			
			//			if( details.PosName.Equals( "Posi" ) )
			//			{
			//				this.POSI_TIMER.Enabled = true;
			//			}
		}
		//public System.Timers.ContextMenu contextMenu1;
		//private System.ComponentModel.IContainer components;
		//private System.Timers.NotifyIcon notifyIcon1;


		private System.Timers.Timer timer1 = new System.Timers.Timer();
		private System.Timers.Timer timer2 = new System.Timers.Timer();
		private System.Timers.Timer EMP_TIMER = new System.Timers.Timer();
		private System.Timers.Timer POSI_TIMER = new System.Timers.Timer();
		private System.Timers.Timer SALES_TIMER = new System.Timers.Timer();
		private System.Timers.Timer LABOR_TIMER = new System.Timers.Timer();
		private System.Timers.Timer SLS_VER_TIMER = new System.Timers.Timer();
		private System.Timers.Timer EMPLOYEE_TIMER = new System.Timers.Timer();
		private System.Timers.Timer APPROVAL_ALERT = new System.Timers.Timer();
		private System.Timers.Timer SCHED_DBF_TIMER = new System.Timers.Timer();
		private System.Timers.Timer CARINOS_GC_TIMER = new System.Timers.Timer();
		private System.Timers.Timer PERIOD_LABOR_TIMER = new System.Timers.Timer();
		private System.Timers.Timer REG_CLOCK_IN_TIMER = new System.Timers.Timer();
		//	private System.Timers.Timer POSI_TIMECARD_IMPORT_TIMER = new System.Timers.Timer();
		private System.Timers.Timer PUNCH_RECORD_TIMER = new System.Timers.Timer();
		private System.Timers.Timer FILE_XFER_TIMER = new System.Timers.Timer();
		private System.Timers.Timer GUEST_COUNT_TIMER = new System.Timers.Timer();
        private System.Timers.Timer HISTORICAL_SALES_TIMER = new System.Timers.Timer();
        


		private SysLog logger;
		private ClientDetails clientDetails ;
		private static String TIMER = "System.Timers.Timer";
		private static SyncManager syncManager = new SyncManager();
		private static String MENU_ITEM = "System.Windows.Forms.MenuItem";
		
		private ServiceController service;
		private bool _waiting = false;

		private void initializeTimers()
		{
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Interval = 60000;
			this.timer1.Elapsed += new ElapsedEventHandler(this.update_details);
			// 
			// SALES_TIMER
			// 
			this.SALES_TIMER.Elapsed += new ElapsedEventHandler(this.SalesSync);
			// 
			// LABOR_TIMER
			// 
			this.LABOR_TIMER.Elapsed += new ElapsedEventHandler(this.LaborSync);
			// 
			// EMPLOYEE_TIMER
			// 
			//	this.EMPLOYEE_TIMER.Elapsed += new ElapsedEventHandler(this.EmpSync);
			this.EMP_TIMER.Elapsed += new System.Timers.ElapsedEventHandler(EmpSync);
			// 
			// CARINOS_GC_TIMER
			// 
			this.CARINOS_GC_TIMER.Elapsed += new ElapsedEventHandler(this.CarinosGcSync);
			// 
			// SLS_VER_TIMER
			// 
			this.SLS_VER_TIMER.Elapsed += new ElapsedEventHandler(this.SalesVerify);
			// 
			// PERIOD_LABOR_TIMER
			// 
			this.PERIOD_LABOR_TIMER.Elapsed += new ElapsedEventHandler(this.PeriodLabor);
			// 
			// REG_CLOCK_IN_TIMER
			// 
			this.REG_CLOCK_IN_TIMER.Elapsed += new ElapsedEventHandler(this.RegClockIn);
			// 
			// POSI_TIMER
			// 
			this.POSI_TIMER.Interval = 120000;
			this.POSI_TIMER.Elapsed += new ElapsedEventHandler(this.POSI_TIMER_Tick);
			// 
			// timer2
			// 
			this.timer2.Enabled = true;
			this.timer2.Interval = 30000;
			this.timer2.Elapsed += new ElapsedEventHandler(this.timer2_Tick);
			// 
			// SCHED_DBF_TIMER
			// 
			this.SCHED_DBF_TIMER.Elapsed += new ElapsedEventHandler(this.SchedDbf);
			// 
			// POSI_TIMECARD_IMPORT_TIMER
			// 
			//	this.POSI_TIMECARD_IMPORT_TIMER.Enabled = true;
			//	this.POSI_TIMECARD_IMPORT_TIMER.Elapsed += new ElapsedEventHandler(this.POSI_TIME_CARD_IMPORT_Tick);
			// 
			// PUNCH_RECORD_TIMER
			// 
			this.PUNCH_RECORD_TIMER.Elapsed += new ElapsedEventHandler(this.PunchRecordSync);
			this.FILE_XFER_TIMER.Elapsed += new ElapsedEventHandler(this.FileTransfer);
			this.GUEST_COUNT_TIMER.Elapsed += new ElapsedEventHandler(this.GuestCount);

            // 
			// HISTORICAL SALES SYNC TIMER
			// 
            this.HISTORICAL_SALES_TIMER.Elapsed += new ElapsedEventHandler(this.HistoricalSalesSync);

		}



		private void update_details(object sender, ElapsedEventArgs e)
		{
			_waiting = true;
			logger.Debug( "updating details in Timers.." );
			try
			{
				LoadClient load = new LoadClient( clientDetails.ClientId );
				ClientDetails cdTemp = null;
				try
				{
					cdTemp = load.GetClientDetails();	
				}
				catch(InvalidOperationException ioex)
				{
					//	RemoteLogger.Log(this.clientDetails.ClientId, RemoteLogger.THREAD_POOL_FAIL);
					logger.Error(ioex.ToString());
					return;
				}
				
				if(cdTemp != null)
				{
					this.clientDetails = cdTemp;
					HsCnxData.Details = this.clientDetails;
				}

				update_module_timers();
			}
			catch( Exception ex )
			{
				logger.Error( ex.ToString() );
			}
			finally
			{
				_waiting = false;
			}
		}

		private void update_module_timers()
		{	
			logger.Debug("Updating module timers");
			try
			{
				if (clientDetails.ModuleList.IsActive(ClientModule.NET_SALES))
				{
					ClientModule module = clientDetails.ModuleList.GetModuleById(ClientModule.NET_SALES);
					int interval = module.SyncInterval;
					int newInterval = HsSettings.GetMilliseconds(interval > 0 ? interval : 1);
					if (this.SALES_TIMER.Interval != newInterval)
					{
						this.SALES_TIMER.Interval = newInterval;
					}
					logger.Debug("SALES TIMER SET TO " + this.SALES_TIMER.Interval);
					if (!this.SALES_TIMER.Enabled)
						this.SALES_TIMER.Enabled = true;
					if (module.Force == 1) ForceSync(ClientModule.NET_SALES);
				}
				else this.SALES_TIMER.Enabled = false;

				if (clientDetails.ModuleList.IsActive(ClientModule.LABOR_ITEMS))
				{
					ClientModule module = clientDetails.ModuleList.GetModuleById(ClientModule.LABOR_ITEMS);
					int interval = module.SyncInterval;
					int newInterval = HsSettings.GetMilliseconds(interval > 0 ? interval : 1);
					if (this.LABOR_TIMER.Interval != newInterval)
					{
						this.LABOR_TIMER.Interval = newInterval;
					}
					logger.Debug("LABOR TIMER SET TO " + this.LABOR_TIMER.Interval);
					if (!this.LABOR_TIMER.Enabled)
						this.LABOR_TIMER.Enabled = true;
					if (module.Force == 1) ForceSync(ClientModule.LABOR_ITEMS);
				}
				else this.LABOR_TIMER.Enabled = false;

				if (clientDetails.ModuleList.IsActive(ClientModule.EMPLOYEE_DATA))
				{
					ClientModule module = clientDetails.ModuleList.GetModuleById(ClientModule.EMPLOYEE_DATA);
					int interval = module.SyncInterval;
					int newInterval = HsSettings.GetMilliseconds(interval > 0 ? interval : 1);
					if (this.EMP_TIMER.Interval != newInterval)
					{
						this.EMP_TIMER.Interval = newInterval;
					}
					this.EMP_TIMER.Enabled = true;
					if (module.Force == 1) ForceSync(ClientModule.EMPLOYEE_DATA);
				}
				else this.EMPLOYEE_TIMER.Enabled = false;

				if (clientDetails.ModuleList.IsActive(ClientModule.REG_CLK_IN))
				{
					ClientModule module = clientDetails.ModuleList.GetModuleById(ClientModule.REG_CLK_IN);
					int interval = module.SyncInterval;
					int newInterval = HsSettings.GetMilliseconds(interval > 0 ? interval : 1);
					if (this.REG_CLOCK_IN_TIMER.Interval != newInterval)
						this.REG_CLOCK_IN_TIMER.Interval = newInterval;
					logger.Debug("Regulated Clock-In set to " + (REG_CLOCK_IN_TIMER.Interval / 60000) + " minute intervals");
					if (!this.REG_CLOCK_IN_TIMER.Enabled)
						this.REG_CLOCK_IN_TIMER.Enabled = true;
					if (module.Force == 1) ForceSync(ClientModule.REG_CLK_IN);
				}
				else this.REG_CLOCK_IN_TIMER.Enabled = false;

				if (clientDetails.ModuleList.IsActive(ClientModule.APPROVAL_ALERT))
				{
					ClientModule module = clientDetails.ModuleList.GetModuleById(ClientModule.APPROVAL_ALERT);
					int interval = module.SyncInterval;
					int newInterval = HsSettings.GetMilliseconds(interval > 0 ? interval : 1);
					if (this.APPROVAL_ALERT.Interval != newInterval)
					{
						this.APPROVAL_ALERT.Interval = newInterval;
					}
					if (!this.APPROVAL_ALERT.Enabled)
						this.APPROVAL_ALERT.Enabled = true;
					if (module.Force == 1) ForceSync(ClientModule.APPROVAL_ALERT);
				}
				else this.APPROVAL_ALERT.Enabled = false;

				if (clientDetails.CustomModuleList.IsActive(ClientCustomModule.CARINOS_GUEST_COUNT))
				{
					int interval = clientDetails.CustomModuleList.GetModuleById(ClientCustomModule.CARINOS_GUEST_COUNT).SyncInterval;
					int newInterval = HsSettings.GetMilliseconds(interval > 0 ? interval : 1);
					if (this.CARINOS_GC_TIMER.Interval != newInterval)
					{
						this.CARINOS_GC_TIMER.Interval = newInterval;
					}
					if (!this.CARINOS_GC_TIMER.Enabled)
						this.CARINOS_GC_TIMER.Enabled = true;
				}
				else this.CARINOS_GC_TIMER.Enabled = false;

				if (clientDetails.ModuleList.IsActive(ClientModule.SALES_VERIFICATION))
				{
					ClientModule module = clientDetails.ModuleList.GetModuleById(ClientModule.SALES_VERIFICATION);
					int interval = module.SyncInterval;
					int newInterval = HsSettings.GetMilliseconds(interval > 0 ? interval : 1);
					if (this.SLS_VER_TIMER.Interval != newInterval)
					{
						this.SLS_VER_TIMER.Interval = newInterval;
					}
					if (!this.SLS_VER_TIMER.Enabled)
						this.SLS_VER_TIMER.Enabled = true;
					if (module.Force == 1) ForceSync(ClientModule.SALES_VERIFICATION);
				}
				else this.SLS_VER_TIMER.Enabled = false;

				if (clientDetails.ModuleList.IsActive(ClientModule.PERIOD_LABOR))
				{
					ClientModule module = clientDetails.ModuleList.GetModuleById(ClientModule.PERIOD_LABOR);
					int interval = module.SyncInterval;
					int newInterval = HsSettings.GetMilliseconds(interval > 0 ? interval : 1);
					if (this.PERIOD_LABOR_TIMER.Interval != newInterval)
					{
						this.PERIOD_LABOR_TIMER.Interval = newInterval;
					}
					if (!this.PERIOD_LABOR_TIMER.Enabled)
						this.PERIOD_LABOR_TIMER.Enabled = true;
					if (module.Force == 1) ForceSync(ClientModule.PERIOD_LABOR);
				}
				else this.PERIOD_LABOR_TIMER.Enabled = false;

				if (clientDetails.ModuleList.IsActive(ClientModule.SCHED_DBF))
				{
					ClientModule module = clientDetails.ModuleList.GetModuleById(ClientModule.SCHED_DBF);
					int interval = module.SyncInterval;
					int newInterval = HsSettings.GetMilliseconds(interval > 0 ? interval : 1);
					if (this.SCHED_DBF_TIMER.Interval != newInterval)
					{
						this.SCHED_DBF_TIMER.Interval = newInterval;
					}
					if (!this.SCHED_DBF_TIMER.Enabled)
						this.SCHED_DBF_TIMER.Enabled = true;
					if (module.Force == 1) ForceSync(ClientModule.SCHED_DBF);
				}
				else this.SCHED_DBF_TIMER.Enabled = false;
				/*	//PUNCH RECORD STUFF
					if( clientDetails.CustomModuleList.IsActive( ClientCustomModule.BJS_TIMECARD_IMPORT) )
					{
						ClientCustomModule module = clientDetails.CustomModuleList.GetModuleById( ClientCustomModule.BJS_TIMECARD_IMPORT);
						int interval = module.SyncInterval;
						this.POSI_TIMECARD_IMPORT_TIMER.Interval = HsSettings.GetMilliseconds( interval > 0 ? interval : 1 );
						this.POSI_TIMECARD_IMPORT_TIMER.Enabled = true;
				} */
				//else this.POSI_TIMECARD_IMPORT_TIMER.Enabled = false;
				if (clientDetails.ModuleList.IsActive(ClientModule.PUNCH_RECORDS))
				{
					ClientModule module = clientDetails.ModuleList.GetModuleById(ClientModule.PUNCH_RECORDS);
					int interval = module.SyncInterval;
					int newInterval = HsSettings.GetMilliseconds(interval > 0 ? interval : 1);
					if (this.PUNCH_RECORD_TIMER.Interval != newInterval)
					{
						this.PUNCH_RECORD_TIMER.Interval = newInterval;
					}
					if (!this.PUNCH_RECORD_TIMER.Enabled)
						this.PUNCH_RECORD_TIMER.Enabled = true;
					if (module.Force == 1) ForceSync(ClientModule.PUNCH_RECORDS);
				}
				else this.PUNCH_RECORD_TIMER.Enabled = false;
				//END PUNCH RECORD STUFF

				if (clientDetails.ModuleList.IsActive(ClientModule.FILE_TRANSFER))
				{
					ClientModule module = clientDetails.ModuleList.GetModuleById(ClientModule.FILE_TRANSFER);
					int interval = module.SyncInterval;
					int newInterval = HsSettings.GetMilliseconds(interval > 0 ? interval : 1);
					if (this.FILE_XFER_TIMER.Interval != newInterval)
						this.FILE_XFER_TIMER.Interval = newInterval;
					logger.Debug("File Transfer set to " + (FILE_XFER_TIMER.Interval / 60000) + " minute intervals");
					if (!this.FILE_XFER_TIMER.Enabled)
						this.FILE_XFER_TIMER.Enabled = true;
					if (module.Force == 1) ForceSync(ClientModule.FILE_TRANSFER);
				}
				else this.FILE_XFER_TIMER.Enabled = false;

				if (clientDetails.ModuleList.IsActive(ClientModule.GUEST_COUNT))
				{
					ClientModule module = clientDetails.ModuleList.GetModuleById(ClientModule.GUEST_COUNT);
					int interval = module.SyncInterval;
					int newInterval = HsSettings.GetMilliseconds(interval > 0 ? interval : 1);
					if (this.GUEST_COUNT_TIMER.Interval != newInterval)
					{
						this.GUEST_COUNT_TIMER.Interval = newInterval;
					}
					if (!this.GUEST_COUNT_TIMER.Enabled)
						this.GUEST_COUNT_TIMER.Enabled = true;
					if (module.Force == 1) ForceSync(ClientModule.GUEST_COUNT);
				}
				else this.GUEST_COUNT_TIMER.Enabled = false;

                if (clientDetails.ModuleList.IsActive(ClientModule.HISTORICAL_SALES))
                {
                    ClientModule module = clientDetails.ModuleList.GetModuleById(ClientModule.HISTORICAL_SALES);
                    int interval = module.SyncInterval;
                    int newInterval = HsSettings.GetMilliseconds(interval > 0 ? interval : 1);
                    if (this.HISTORICAL_SALES_TIMER.Interval != newInterval)
                    {
                        this.HISTORICAL_SALES_TIMER.Interval = newInterval;
                    }
                    if (!this.HISTORICAL_SALES_TIMER.Enabled)
                    {
                        this.HISTORICAL_SALES_TIMER.Enabled = true;
                    }
                }
                else
                {
                    this.HISTORICAL_SALES_TIMER.Enabled = false;
                }
			}
			catch (Exception e)
			{
				logger.Error("Error updating module timers", e);
			}
			
		}

		private void SalesSync(object sender, ElapsedEventArgs e)
		{
			logger.Debug("SalesSync Start");
			WaitForUpdate();
			logger.Debug("SalesSync after wait");
			ClientModule mod = clientDetails.ModuleList.GetModuleById( ClientModule.NET_SALES );
			if( sender.GetType().ToString().Equals( TIMER ) && mod.SyncType == ClientModule.FIXED_SYNC )
			{
				if( mod.SyncHour == getHour() && mod.SyncMinute == getMinute() )
				{
					logger.Log( "synching SALES" );
					NetSalesModule salesMod = new NetSalesModule();
					salesMod.AutoSync = true;
					salesMod.Details = clientDetails;
					syncManager.SyncModule( salesMod );
				}
			} 
			else if( sender.GetType().ToString().Equals( MENU_ITEM ) || mod.SyncType == ClientModule.TIMED_SYNC )
			{
				logger.Debug("SalesSync inside timed_sync");
				logger.Log( "synching SALES" );
				NetSalesModule salesMod = new NetSalesModule();
				salesMod.Details = clientDetails;
				if( sender.GetType().ToString().Equals( TIMER ) ) salesMod.AutoSync = true;
				syncManager.SyncModule( salesMod );
				logger.Debug("SalesSync end timed_sync");
			}
			logger.Debug("SalesSync End");
		}

		private void LaborSync(object sender, ElapsedEventArgs e)
		{
			logger.Debug("LaborSync Start");
			WaitForUpdate();
			logger.Debug("LaborSync after wait");
			ClientModule mod = clientDetails.ModuleList.GetModuleById( ClientModule.LABOR_ITEMS );
			if( sender.GetType().ToString().Equals( TIMER ) && mod.SyncType == ClientModule.FIXED_SYNC )
			{
				if( mod.SyncHour == getHour() && mod.SyncMinute == getMinute() )
				{
						logger.Log( "synching LABOR" );
					LaborItemModule laborMod = new LaborItemModule();
					laborMod.AutoSync = true;
					laborMod.Details = clientDetails;
					syncManager.SyncModule( laborMod );
				}
			} 
			else if( sender.GetType().ToString().Equals( MENU_ITEM ) || mod.SyncType == ClientModule.TIMED_SYNC )
			{
				logger.Debug("LaborSync inside timed_sync");
				logger.Log( "synching LABOR" );
				LaborItemModule laborMod = new LaborItemModule();
				laborMod.Details = clientDetails;
				if( sender.GetType().ToString().Equals( TIMER ) ) laborMod.AutoSync = true;
				syncManager.SyncModule( laborMod );
				logger.Debug("LaborSync end timed_sync");
			}
			logger.Debug("LaborSync end");
		}

		private void EmpSync(object sender, ElapsedEventArgs e)
		{
			WaitForUpdate();
			ClientModule mod = clientDetails.ModuleList.GetModuleById( ClientModule.EMPLOYEE_DATA );
			if( sender.GetType().ToString().Equals( TIMER ) && mod.SyncType == ClientModule.FIXED_SYNC )
			{
				if( mod.SyncHour == getHour() && mod.SyncMinute == getMinute() )
				{
					logger.Log( "synching EMPLOYEES" );
					EmployeeDataModule empDataMod = new EmployeeDataModule();
					empDataMod.Details = clientDetails;
					syncManager.SyncModule( empDataMod );
				}
			} 
			else if( sender.GetType().ToString().Equals( MENU_ITEM ) || mod.SyncType == ClientModule.TIMED_SYNC )
			{
					logger.Log( "synching EMPLOYEES" );
				EmployeeDataModule empDataMod = new EmployeeDataModule();
				empDataMod.Details = clientDetails;
				syncManager.SyncModule( empDataMod );
			}
		}

		private void CarinosGcSync(object sender, ElapsedEventArgs e)
		{
			WaitForUpdate();
			ClientCustomModule mod = clientDetails.CustomModuleList.GetModuleById( ClientCustomModule.CARINOS_GUEST_COUNT );
			if( sender.GetType().ToString().Equals( TIMER ) && mod.SyncType == ClientModule.FIXED_SYNC )
			{
				if( mod.SyncHour == getHour() && mod.SyncMinute == getMinute() )
				{
					CarinosGuestCount carinosMod = new CarinosGuestCount();
					carinosMod.Details = clientDetails;
					carinosMod.AutoSync = true;
					syncManager.SyncModule( carinosMod );
				}
			} 
			else if( sender.GetType().ToString().Equals( MENU_ITEM ) || mod.SyncType == ClientModule.TIMED_SYNC )
			{
				CarinosGuestCount carinosMod = new CarinosGuestCount();
				carinosMod.Details = clientDetails;
				if( sender.GetType().ToString().Equals( TIMER ) ) carinosMod.AutoSync = true;
				syncManager.SyncModule( carinosMod );
			}
		}

		private void SalesVerify(object sender, ElapsedEventArgs e)
		{
			WaitForUpdate();
			ClientModule mod = clientDetails.ModuleList.GetModuleById( ClientModule.SALES_VERIFICATION );
			if( sender.GetType().ToString().Equals( TIMER ) && mod.SyncType == ClientModule.FIXED_SYNC )
			{
				if( mod.SyncHour == getHour() && mod.SyncMinute == getMinute() )
				{
					SalesVerification verifyMod = new SalesVerification();
					verifyMod.Details = clientDetails;
					syncManager.SyncModule( verifyMod );
				}
			}
		}

		private int getMinute()
		{
			return DateTime.Now.Minute;
		}

		private int getHour()
		{
			return DateTime.Now.Hour;
		}

		private void PeriodLabor(object sender, ElapsedEventArgs e)
		{
			WaitForUpdate();
			ClientModule mod = clientDetails.ModuleList.GetModuleById( ClientModule.PERIOD_LABOR );
			if( sender.GetType().ToString().Equals( TIMER ) && mod.SyncType == ClientModule.FIXED_SYNC )
			{
				if( mod.SyncHour == getHour() && mod.SyncMinute == getMinute() )
				{
					PeriodLabor laborMod = new PeriodLabor();
					laborMod.Details = clientDetails;
					syncManager.SyncModule( laborMod );
				}
			}
            else if (sender.GetType().ToString().Equals(MENU_ITEM) || mod.SyncType == ClientModule.TIMED_SYNC)
            {
                PeriodLabor laborMod = new PeriodLabor();
                laborMod.Details = clientDetails;
                syncManager.SyncModule(laborMod);
            }
		}

		private void RegClockIn(object sender, ElapsedEventArgs e)
		{
			WaitForUpdate();
			ClientModule mod = clientDetails.ModuleList.GetModuleById( ClientModule.REG_CLK_IN );
			if( sender.GetType().ToString().Equals( TIMER ) && mod.SyncType == ClientModule.FIXED_SYNC )
			{
				if( mod.SyncHour == getHour() && mod.SyncMinute == getMinute() )
				{
					RegClockInModule regClockInMod = new RegClockInModule();
					regClockInMod.AutoSync = true;
					regClockInMod.Details = clientDetails;
					syncManager.SyncModule( regClockInMod );
				}
			} 
			else if( sender.GetType().ToString().Equals( MENU_ITEM ) || mod.SyncType == ClientModule.TIMED_SYNC )
			{
				RegClockInModule regClockInMod = new RegClockInModule();
				if( sender.GetType().ToString().Equals( TIMER ) ) regClockInMod.AutoSync = true;
				regClockInMod.Details = clientDetails;
				syncManager.SyncModule( regClockInMod );
			}
		}

		private void ApprovalAlert(object sender, ElapsedEventArgs e)
		{
			WaitForUpdate();
			ClientModule mod = clientDetails.ModuleList.GetModuleById( ClientModule.APPROVAL_ALERT );
			if( sender.GetType().ToString().Equals( TIMER ) && mod.SyncType == ClientModule.FIXED_SYNC )
			{
				if( mod.SyncHour == getHour() && mod.SyncMinute == getMinute() )
				{
					ApprovalAlertModule approvalAlert = new ApprovalAlertModule();
					approvalAlert.Details = clientDetails;
					syncManager.SyncModule( approvalAlert );
				}
			} 
			else if( sender.GetType().ToString().Equals( MENU_ITEM ) || mod.SyncType == ClientModule.TIMED_SYNC )
			{
				ApprovalAlertModule approvalAlert = new ApprovalAlertModule();
				approvalAlert.Details = clientDetails;
				syncManager.SyncModule( approvalAlert );
			}
		}

		private void FileTransfer(object sender, ElapsedEventArgs e)
		{
			logger.Debug("Starting FileTransfer Module");
			WaitForUpdate();
			ClientModule mod = clientDetails.ModuleList.GetModuleById( ClientModule.FILE_TRANSFER );
			if( sender.GetType().ToString().Equals( TIMER ) && mod.SyncType == ClientModule.FIXED_SYNC )
			{
				logger.Debug("FIXED FileTransfer");
				if( mod.SyncHour == getHour() && mod.SyncMinute == getMinute() )
				{
					FileTransferModule xferMod = new FileTransferModule();
					xferMod.Details = clientDetails;
					syncManager.SyncModule( xferMod );
				}
			}
			else if(sender.GetType().ToString().Equals( MENU_ITEM ) || mod.SyncType==ClientModule.TIMED_SYNC)
			{
				logger.Debug("MENU or TIMED FileTransfer");
				FileTransferModule xferMod = new FileTransferModule();
				xferMod.Details = clientDetails;
				syncManager.SyncModule(xferMod);
			}
		}

		private void GuestCount(object sender, ElapsedEventArgs e)
		{
			WaitForUpdate();
			ClientModule mod = clientDetails.ModuleList.GetModuleById(ClientModule.GUEST_COUNT);
			if (sender.GetType().ToString().Equals(TIMER) && mod.SyncType == ClientModule.FIXED_SYNC)
			{
				if (mod.SyncHour == getHour() && mod.SyncMinute == getMinute())
				{
					GuestCountModule gcMod = new GuestCountModule();
					gcMod.Details = clientDetails;
					syncManager.SyncModule(gcMod);
				}
			}
		}

		private void POSI_TIMER_Tick(object sender, ElapsedEventArgs e)
		{
			String[] operations = HsReader.Execute();

			foreach( String op in operations )
			{
				if( op.Equals( "ImportSchedule" ) )
				{
					RegClockInModule regClockInMod = new RegClockInModule();
					regClockInMod.AutoSync = false;
					regClockInMod.Details = clientDetails;
					syncManager.SyncModule( regClockInMod );
				}
				if( op.Equals( "ImportEmployee" ) )
				{
					EmployeeDataModule empDataMod = new EmployeeDataModule();
					empDataMod.Details = clientDetails;
					syncManager.SyncModule( empDataMod );
				}
				if( op.Equals( "ImportTemplateInfo" ) )
				{
					ScheduleDbfModule schedDbfMod = new ScheduleDbfModule();
					schedDbfMod.AutoSync = false;
					schedDbfMod.Details = clientDetails;
					syncManager.SyncModule( schedDbfMod );
				}
			}			
		}

		private void ForceSync( int modId )
		{
			try
			{
				
				service.Refresh();
				if(!(service.Status == ServiceControllerStatus.Running))
				{
					logger.Log("In ForceSync():  not started so don't force");
					return;
				}
				ClientSettingsWss settings = new ClientSettingsWss();
				settings.resetForce( clientDetails.ClientId, modId );
				switch( modId )
				{
					case 201:
						NetSalesModule salesMod = new NetSalesModule();
						salesMod.AutoSync = true;
						salesMod.Details = clientDetails;
						syncManager.SyncModule( salesMod );
						break;
					case 202:
						LaborItemModule laborMod = new LaborItemModule();
						laborMod.AutoSync = true;
						laborMod.Details = clientDetails;
						syncManager.SyncModule( laborMod );
						break;
					case 203:
						EmployeeDataModule empDataMod = new EmployeeDataModule();
						empDataMod.Details = clientDetails;
						syncManager.SyncModule( empDataMod );
						break;
					case 204:
						SalesVerification verifyMod = new SalesVerification();
						verifyMod.Details = clientDetails;
						syncManager.SyncModule( verifyMod );
						break;
					case 205:
						PeriodLabor pLaborMod = new PeriodLabor();
						pLaborMod.Details = clientDetails;
						syncManager.SyncModule( pLaborMod );
						break;
					case 206:
						RegClockInModule regClockInMod = new RegClockInModule();
						regClockInMod.AutoSync = true;
						regClockInMod.Details = clientDetails;
						syncManager.SyncModule( regClockInMod );
						break;
					case 207:
						ApprovalAlertModule approvalAlert = new ApprovalAlertModule();
						approvalAlert.Details = clientDetails;
						syncManager.SyncModule( approvalAlert );
						break;
						/*
						case 209:
							PunchRecordModule punchMod = new PunchRecordModule();
							punchMod.Details = clientDetails;
							syncManager.SyncModule( punchMod );
							break;
						*/
					case 220:
						FileTransferModule xferMod = new FileTransferModule();
						xferMod.Details = clientDetails;
						syncManager.SyncModule( xferMod );
						break;
					case 221:
						GuestCountModule gcMod = new GuestCountModule();
						gcMod.Details = clientDetails;
						syncManager.SyncModule(gcMod);
						break;
				}

			}
			catch( Exception ex )
			{
				logger.Error( "Error in force sync: " + ex.ToString() );
			}
			finally
			{
				if(Run.Mapped)
					PosiControl.CloseMap();//for OSI
			}
		}

		private void timer2_Tick(object sender, ElapsedEventArgs e)
		{
			// delete log files
			SysLog logger = new SysLog(this.GetType());
			logger.loadDebugProperties();
			logger.DeleteLogs( "_debug" );
			logger.DeleteLogs( "_error" );
			logger.DeleteLogs( "_log" );
		}

		private void SchedDbf(object sender, ElapsedEventArgs e)
		{
			ClientModule mod = clientDetails.ModuleList.GetModuleById( ClientModule.SCHED_DBF );
			if( sender.GetType().ToString().Equals( TIMER ) && mod.SyncType == ClientModule.FIXED_SYNC )
			{
				if( mod.SyncHour == getHour() && mod.SyncMinute == getMinute() )
				{
					ScheduleDbfModule schedDbfMod = new ScheduleDbfModule();
					schedDbfMod.AutoSync = true;
					schedDbfMod.Details = clientDetails;
					syncManager.SyncModule( schedDbfMod );
				}
			} 
			else if( sender.GetType().ToString().Equals( MENU_ITEM ) || mod.SyncType == ClientModule.TIMED_SYNC )
			{
				ScheduleDbfModule schedDbfMod = new ScheduleDbfModule();
				if( sender.GetType().ToString().Equals( TIMER ) ) schedDbfMod.AutoSync = true;
				schedDbfMod.Details = clientDetails;
				syncManager.SyncModule( schedDbfMod );
			}
		}

		private void WaitForUpdate()
		{
			int cnt = 0;
			while( _waiting && cnt < 1000 )
			{
				System.Threading.Thread.Sleep( 2000 );
				cnt++;
			}
			logger.Debug("Done waiting for update");
		}
		/*	//MORE PUNCH RECORD STUFF
			private void POSI_TIME_CARD_IMPORT_Tick(object sender, ElapsedEventArgs e)
			{
				ClientCustomModule mod = clientDetails.CustomModuleList.GetModuleById( ClientCustomModule.BJS_TIMECARD_IMPORT );
				if( sender.GetType().ToString().Equals( TIMER ) && mod.SyncType == ClientModule.FIXED_SYNC )
				{
					if( mod.SyncHour == getHour() && mod.SyncMinute == getMinute() )
					{
						PosiTimeCardImportModule posiMod = new PosiTimeCardImportModule();
						posiMod.Details = clientDetails;
						syncManager.SyncModule( posiMod );
					}
				} 
				else if( sender.GetType().ToString().Equals( MENU_ITEM ) || mod.SyncType == ClientModule.TIMED_SYNC )
				{
					PosiTimeCardImportModule posiMod = new PosiTimeCardImportModule();
					posiMod.Details = clientDetails;
					syncManager.SyncModule( posiMod );
				}
			}*/
		
		private void PunchRecordSync(object sender, ElapsedEventArgs e)
		{
			ClientModule mod = clientDetails.ModuleList.GetModuleById( ClientModule.PUNCH_RECORDS );
			if( sender.GetType().ToString().Equals( TIMER ) && mod.SyncType == ClientModule.FIXED_SYNC )
			{
				if( mod.SyncHour == getHour() && mod.SyncMinute == getMinute() )
				{
					PunchRecordModule pMod = new PunchRecordModule();
					pMod.AutoSync = true;
					pMod.Details = clientDetails;
					syncManager.SyncModule( pMod );
				}
			} 
			else if( sender.GetType().ToString().Equals( MENU_ITEM ) || mod.SyncType == ClientModule.TIMED_SYNC )
			{
				PunchRecordModule pMod = new PunchRecordModule();
				pMod.Details = clientDetails;
				if( sender.GetType().ToString().Equals( TIMER ) ) pMod.AutoSync = true;
				syncManager.SyncModule( pMod );
			}
		}
		//END MORE PUNCH RECORD STUFF

        private void HistoricalSalesSync(object sender, ElapsedEventArgs e)
        {
            ClientModule mod = clientDetails.ModuleList.GetModuleById(ClientModule.HISTORICAL_SALES);
            if (sender.GetType().ToString().Equals(TIMER) && mod.SyncType == ClientModule.FIXED_SYNC)
            {
                if (mod.SyncHour == getHour() && mod.SyncMinute == getMinute())
                {
                    logger.Log("synching Historical Sales");
                    if (!HistoricalSalesModule._histSyncRunning)
                    {
                        logger.Debug("Historical Sales Sync Start");
                        HistoricalSalesModule histMod = new HistoricalSalesModule();
                        histMod.Details = clientDetails;
                        syncManager.SyncModule(histMod);
                    }
                    else
                    {
                        logger.Debug("Historical Sales sync already running");
                    }
                }
            }
        }//HistoricalSalesSync

      
        }
	}

