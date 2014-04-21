using HsConnect.Main;
using HsConnect.Modules;
using HsSharedObjects.Client;
using HsSharedObjects.Client.Module;
using HsSharedObjects.Client.CustomModule;
using HsConnect.Data;
using HsConnect.Services;
using HsConnect.Services.Wss;

using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;

namespace HsConnect.Forms
{
	/// <summary>
	/// Summary description for TrayIcon.
	/// </summary>
	public class TrayIcon : System.ComponentModel.Component
	{
		private System.Windows.Forms.NotifyIcon notifyIcon1;
		private System.Windows.Forms.Timer timer1;
		public System.Windows.Forms.ContextMenu contextMenu1;
		private System.Windows.Forms.Timer SALES_TIMER;
		private System.ComponentModel.IContainer components;
		private static SyncManager syncManager = new SyncManager();
		private System.Windows.Forms.Timer LABOR_TIMER;
		private System.Windows.Forms.Timer EMPLOYEE_TIMER;
		private System.Windows.Forms.Timer CARINOS_GC_TIMER;
		private SysLog logger;
		private static String TIMER = "System.Windows.Forms.Timer";
		private System.Windows.Forms.Timer SLS_VER_TIMER;
		private System.Windows.Forms.Timer PERIOD_LABOR_TIMER;
		private System.Windows.Forms.Timer REG_CLOCK_IN_TIMER;
		private System.Windows.Forms.Timer APPROVAL_ALERT;
		private System.Windows.Forms.Timer POSI_TIMER;
		private System.Windows.Forms.Timer timer2;
		private System.Windows.Forms.Timer SCHED_DBF_TIMER;
		private static String MENU_ITEM = "System.Windows.Forms.MenuItem";

		public TrayIcon(System.ComponentModel.IContainer container)
		{
			///
			/// Required for Windows.Forms Class Composition Designer support
			///
			container.Add(this);
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		
		public TrayIcon( ClientDetails details )
		{
			InitializeComponent();
			this.logger = new SysLog( this.GetType() );
			this.clientDetails = details;
			this.timer1.Interval = HsSettings.GetMilliseconds( 15 );
			this.update_context_menu();
			if( details.PosName.Equals( "Posi" ) )
			{
				this.POSI_TIMER.Enabled = true;
			}
		}

		private ClientDetails clientDetails ;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}


		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(TrayIcon));
			this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
			this.contextMenu1 = new System.Windows.Forms.ContextMenu();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.SALES_TIMER = new System.Windows.Forms.Timer(this.components);
			this.LABOR_TIMER = new System.Windows.Forms.Timer(this.components);
			this.EMPLOYEE_TIMER = new System.Windows.Forms.Timer(this.components);
			this.CARINOS_GC_TIMER = new System.Windows.Forms.Timer(this.components);
			this.SLS_VER_TIMER = new System.Windows.Forms.Timer(this.components);
			this.PERIOD_LABOR_TIMER = new System.Windows.Forms.Timer(this.components);
			this.REG_CLOCK_IN_TIMER = new System.Windows.Forms.Timer(this.components);
			this.APPROVAL_ALERT = new System.Windows.Forms.Timer(this.components);
			this.POSI_TIMER = new System.Windows.Forms.Timer(this.components);
			this.timer2 = new System.Windows.Forms.Timer(this.components);
			this.SCHED_DBF_TIMER = new System.Windows.Forms.Timer(this.components);
			// 
			// notifyIcon1
			// 
			this.notifyIcon1.ContextMenu = this.contextMenu1;
			this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
			this.notifyIcon1.Text = "HS Connect";
			this.notifyIcon1.Visible = true;
			this.notifyIcon1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDown);
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Interval = 60000;
			this.timer1.Tick += new System.EventHandler(this.update_details);
			// 
			// SALES_TIMER
			// 
			this.SALES_TIMER.Tick += new System.EventHandler(this.SalesSync);
			// 
			// LABOR_TIMER
			// 
			this.LABOR_TIMER.Tick += new System.EventHandler(this.LaborSync);
			// 
			// EMPLOYEE_TIMER
			// 
			this.EMPLOYEE_TIMER.Tick += new System.EventHandler(this.EmpSync);
			// 
			// CARINOS_GC_TIMER
			// 
			this.CARINOS_GC_TIMER.Tick += new System.EventHandler(this.CarinosGcSync);
			// 
			// SLS_VER_TIMER
			// 
			this.SLS_VER_TIMER.Tick += new System.EventHandler(this.SalesVerify);
			// 
			// PERIOD_LABOR_TIMER
			// 
			this.PERIOD_LABOR_TIMER.Tick += new System.EventHandler(this.PeriodLabor);
			// 
			// REG_CLOCK_IN_TIMER
			// 
			this.REG_CLOCK_IN_TIMER.Enabled = true;
			this.REG_CLOCK_IN_TIMER.Tick += new System.EventHandler(this.RegClockIn);
			// 
			// POSI_TIMER
			// 
			this.POSI_TIMER.Interval = 120000;
			this.POSI_TIMER.Tick += new System.EventHandler(this.POSI_TIMER_Tick);
			// 
			// timer2
			// 
			this.timer2.Enabled = true;
			this.timer2.Interval = 30000;
			this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
			// 
			// SCHED_DBF_TIMER
			// 
			this.SCHED_DBF_TIMER.Tick += new System.EventHandler(this.SchedDbf);

		}
		#endregion

		private void notifyIcon1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			
		}

		public void IconOn( String msg )
		{
			System.Reflection.Assembly myAssembly = System.Reflection.Assembly.GetExecutingAssembly();
			System.IO.Stream s = myAssembly.GetManifestResourceStream("HsConnect.icon2.ico");
			notifyIcon1.Icon = new System.Drawing.Icon( s );
			s.Close();
		}

		public void IconOff( String msg )
		{
			System.Reflection.Assembly myAssembly = System.Reflection.Assembly.GetExecutingAssembly();
			System.IO.Stream s = myAssembly.GetManifestResourceStream("HsConnect.icon1.ico");
			notifyIcon1.Icon = new System.Drawing.Icon( s );
			s.Close();
		}

		private void update_details(object sender, System.EventArgs e)
		{
			logger.Debug( "updating details.." );
			try
			{
				LoadClient load = new LoadClient( clientDetails.ClientId );
				this.clientDetails = load.GetClientDetails();
				update_context_menu();
			}
			catch( Exception ex )
			{
				logger.Error( ex.ToString() );
			}
		}

		private void update_context_menu()
		{			
			this.contextMenu1.MenuItems.Clear();
		
			int menuCnt = clientDetails.ModuleList.Count + clientDetails.CustomModuleList.Count;
			if( Application.ProductName.Equals( "DEBUG" ) ) menuCnt++;
			if ( clientDetails.ModuleList.IsActive( ClientModule.SALES_VERIFICATION ) ) menuCnt--;
			if ( clientDetails.ModuleList.IsActive( ClientModule.PERIOD_LABOR ) ) menuCnt--;
			MenuItem[] menuItems = new MenuItem[ menuCnt ];
			logger.Debug("menu count: " + menuCnt.ToString() );
			int index = 0;

			if( Application.ProductName.Equals( "DEBUG" ) )
			{
				MenuItem item = MakeMenuItem( "Control Panel" );
				item.Click += new System.EventHandler( this.LaunchControlPanel );
				menuItems[ index ] = item;
				index++;
			}

			if( clientDetails.ModuleList.IsActive( ClientModule.NET_SALES ) )
			{
				ClientModule module = clientDetails.ModuleList.GetModuleById( ClientModule.NET_SALES );
				int interval = module.SyncInterval;
				this.SALES_TIMER.Interval = HsSettings.GetMilliseconds( interval > 0 ? interval : 1 );
				this.SALES_TIMER.Enabled = true;
				MenuItem item = MakeMenuItem( "Sales Sync" );
				item.Click += new System.EventHandler( this.SalesSync );
				menuItems[ index ] = item;
				index++;
				if( module.Force == 1 ) ForceSync( ClientModule.NET_SALES );
			} else this.SALES_TIMER.Enabled = false;

			if( clientDetails.ModuleList.IsActive( ClientModule.LABOR_ITEMS ) )
			{
				ClientModule module = clientDetails.ModuleList.GetModuleById( ClientModule.LABOR_ITEMS );
				int interval = module.SyncInterval;
				this.LABOR_TIMER.Interval = HsSettings.GetMilliseconds( interval > 0 ? interval : 1 );
				this.LABOR_TIMER.Enabled = true;
				MenuItem item = MakeMenuItem( "Labor Sync" );
				item.Click += new System.EventHandler( this.LaborSync );
				menuItems[ index ] = item;
				index++;
				if( module.Force == 1 ) ForceSync( ClientModule.LABOR_ITEMS );
			} else this.LABOR_TIMER.Enabled = false;

			if( clientDetails.ModuleList.IsActive( ClientModule.EMPLOYEE_DATA ) )
			{
				ClientModule module = clientDetails.ModuleList.GetModuleById( ClientModule.EMPLOYEE_DATA );
				int interval = module.SyncInterval;
				this.EMPLOYEE_TIMER.Interval = HsSettings.GetMilliseconds( interval > 0 ? interval : 1 );
				this.EMPLOYEE_TIMER.Enabled = true;
				MenuItem item = MakeMenuItem( "Employee Sync" );
				item.Click += new System.EventHandler( this.EmpSync );
				menuItems[ index ] = item;
				index++;
				if( module.Force == 1 ) ForceSync( ClientModule.EMPLOYEE_DATA );
			} else this.EMPLOYEE_TIMER.Enabled = false;

			if( clientDetails.ModuleList.IsActive( ClientModule.REG_CLK_IN ) )
			{
				ClientModule module = clientDetails.ModuleList.GetModuleById( ClientModule.REG_CLK_IN );
				int interval = module.SyncInterval;
				this.REG_CLOCK_IN_TIMER.Interval = HsSettings.GetMilliseconds( interval > 0 ? interval : 1 );
				this.REG_CLOCK_IN_TIMER.Enabled = true;
				MenuItem item = MakeMenuItem( "Import Schedule" );
				item.Click += new System.EventHandler( this.RegClockIn );
				menuItems[ index ] = item;
				index++;
				if( module.Force == 1 ) ForceSync( ClientModule.REG_CLK_IN );
			} 
			else this.REG_CLOCK_IN_TIMER.Enabled = false;

			if( clientDetails.ModuleList.IsActive( ClientModule.APPROVAL_ALERT ) )
			{
				ClientModule module = clientDetails.ModuleList.GetModuleById( ClientModule.APPROVAL_ALERT );
				int interval = module.SyncInterval;
				this.APPROVAL_ALERT.Interval = HsSettings.GetMilliseconds( interval > 0 ? interval : 1 );
				this.APPROVAL_ALERT.Enabled = true;
				MenuItem item = MakeMenuItem( "Approval Alert" );
				item.Click += new System.EventHandler( this.ApprovalAlert );
				menuItems[ index ] = item;
				index++;
				if( module.Force == 1 ) ForceSync( ClientModule.APPROVAL_ALERT );
			} 
			else this.APPROVAL_ALERT.Enabled = false;

			if( clientDetails.CustomModuleList.IsActive( ClientCustomModule.CARINOS_GUEST_COUNT ) )
			{
				int interval = clientDetails.CustomModuleList.GetModuleById( ClientCustomModule.CARINOS_GUEST_COUNT ).SyncInterval;
				this.CARINOS_GC_TIMER.Interval = HsSettings.GetMilliseconds( interval > 0 ? interval : 1 );
				this.CARINOS_GC_TIMER.Enabled = true;
				MenuItem item = MakeMenuItem( "Guest Count Sync" );
				item.Click += new System.EventHandler( this.CarinosGcSync );
				menuItems[ index ] = item;
				index++;
			} else this.CARINOS_GC_TIMER.Enabled = false;

			if( clientDetails.ModuleList.IsActive( ClientModule.SALES_VERIFICATION ) )
			{
				ClientModule module = clientDetails.ModuleList.GetModuleById( ClientModule.SALES_VERIFICATION );
				int interval = module.SyncInterval;
				this.SLS_VER_TIMER.Interval = HsSettings.GetMilliseconds( interval > 0 ? interval : 1 );
				this.SLS_VER_TIMER.Enabled = true;
				if( module.Force == 1 ) ForceSync( ClientModule.SALES_VERIFICATION );
			} else this.SLS_VER_TIMER.Enabled = false;

			if( clientDetails.ModuleList.IsActive( ClientModule.PERIOD_LABOR ) )
			{
				ClientModule module = clientDetails.ModuleList.GetModuleById( ClientModule.PERIOD_LABOR );
				int interval = module.SyncInterval;
				this.PERIOD_LABOR_TIMER.Interval = HsSettings.GetMilliseconds( interval > 0 ? interval : 1 );
				this.PERIOD_LABOR_TIMER.Enabled = true;
				if( module.Force == 1 ) ForceSync( ClientModule.PERIOD_LABOR );
			} else this.PERIOD_LABOR_TIMER.Enabled = false;

			if( clientDetails.ModuleList.IsActive( ClientModule.SCHED_DBF ) )
			{
				ClientModule module = clientDetails.ModuleList.GetModuleById( ClientModule.SCHED_DBF );
				int interval = module.SyncInterval;
				this.SCHED_DBF_TIMER.Interval = HsSettings.GetMilliseconds( interval > 0 ? interval : 1 );
				this.SCHED_DBF_TIMER.Enabled = true;
				MenuItem item = MakeMenuItem( "Import Data" );
				item.Click += new System.EventHandler( this.SchedDbf );
				menuItems[ index ] = item;
				index++;
				if( module.Force == 1 ) ForceSync( ClientModule.SCHED_DBF );
			} 
			else this.SCHED_DBF_TIMER.Enabled = false;

			this.contextMenu1.MenuItems.AddRange( menuItems );
		}

		private void SalesSync(object sender, System.EventArgs e)
		{
			ClientModule mod = clientDetails.ModuleList.GetModuleById( ClientModule.NET_SALES );
			if( sender.GetType().ToString().Equals( TIMER ) && mod.SyncType == ClientModule.FIXED_SYNC )
			{
				if( mod.SyncHour == getHour() && mod.SyncMinute == getMinute() )
				{
					NetSalesModule salesMod = new NetSalesModule();
					salesMod.AutoSync = true;
					salesMod.Details = clientDetails;
					syncManager.SyncModule( salesMod );
				}
			} 
			else if( sender.GetType().ToString().Equals( MENU_ITEM ) || mod.SyncType == ClientModule.TIMED_SYNC )
			{
				NetSalesModule salesMod = new NetSalesModule();
				salesMod.Details = clientDetails;
				if( sender.GetType().ToString().Equals( TIMER ) ) salesMod.AutoSync = true;
				syncManager.SyncModule( salesMod );
			}
		}

		private void LaborSync(object sender, System.EventArgs e)
		{
			ClientModule mod = clientDetails.ModuleList.GetModuleById( ClientModule.LABOR_ITEMS );
			if( sender.GetType().ToString().Equals( TIMER ) && mod.SyncType == ClientModule.FIXED_SYNC )
			{
				if( mod.SyncHour == getHour() && mod.SyncMinute == getMinute() )
				{
					LaborItemModule laborMod = new LaborItemModule();
					laborMod.AutoSync = true;
					laborMod.Details = clientDetails;
					syncManager.SyncModule( laborMod );
				}
			} 
			else if( sender.GetType().ToString().Equals( MENU_ITEM ) || mod.SyncType == ClientModule.TIMED_SYNC )
			{
				LaborItemModule laborMod = new LaborItemModule();
				laborMod.Details = clientDetails;
				if( sender.GetType().ToString().Equals( TIMER ) ) laborMod.AutoSync = true;
				syncManager.SyncModule( laborMod );
			}
		}

		private void EmpSync(object sender, System.EventArgs e)
		{
			ClientModule mod = clientDetails.ModuleList.GetModuleById( ClientModule.EMPLOYEE_DATA );
			if( sender.GetType().ToString().Equals( TIMER ) && mod.SyncType == ClientModule.FIXED_SYNC )
			{
				if( mod.SyncHour == getHour() && mod.SyncMinute == getMinute() )
				{
					EmployeeDataModule empDataMod = new EmployeeDataModule();
					empDataMod.Details = clientDetails;
					syncManager.SyncModule( empDataMod );
				}
			} 
			else if( sender.GetType().ToString().Equals( MENU_ITEM ) || mod.SyncType == ClientModule.TIMED_SYNC )
			{
				EmployeeDataModule empDataMod = new EmployeeDataModule();
				empDataMod.Details = clientDetails;
				syncManager.SyncModule( empDataMod );
			}
		}

		private void CarinosGcSync(object sender, System.EventArgs e)
		{
			ClientCustomModule mod = clientDetails.CustomModuleList.GetModuleById( ClientCustomModule.CARINOS_GUEST_COUNT );
			if( sender.GetType().ToString().Equals( TIMER ) && mod.SyncType == ClientModule.FIXED_SYNC )
			{
				if( mod.SyncHour == getHour() && mod.SyncMinute == getMinute() )
				{
					CarinosGuestCount carinosMod = new CarinosGuestCount();
					carinosMod.Details = clientDetails;
					syncManager.SyncModule( carinosMod );
				}
			} 
			else if( sender.GetType().ToString().Equals( MENU_ITEM ) || mod.SyncType == ClientModule.TIMED_SYNC )
			{
				CarinosGuestCount carinosMod = new CarinosGuestCount();
				carinosMod.Details = clientDetails;
				syncManager.SyncModule( carinosMod );
			}
		}

		private void SalesVerify(object sender, System.EventArgs e)
		{
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
		
		
		private void LaunchControlPanel(object sender, System.EventArgs e)
		{
			ControlPanel panel = new ControlPanel();
			panel.Show();
		}

		private MenuItem MakeMenuItem( String text )
		{
			MenuItem item = new MenuItem();
			item.Text = text;
			return item;
		}

		private int getMinute()
		{
			return DateTime.Now.Minute;
		}

		private int getHour()
		{
			return DateTime.Now.Hour;
		}

		private void PeriodLabor(object sender, System.EventArgs e)
		{
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
		}

		private void RegClockIn(object sender, System.EventArgs e)
		{
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

		private void ApprovalAlert(object sender, System.EventArgs e)
		{
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

		private void POSI_TIMER_Tick(object sender, System.EventArgs e)
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
				}
			}
			catch( Exception ex )
			{
				logger.Error( "Error in force sync: " + ex.ToString() );
			}
		}

		private void timer2_Tick(object sender, System.EventArgs e)
		{
			// delete log files
			SysLog logger = new SysLog(this.GetType());
			logger.loadDebugProperties();
			logger.DeleteLogs( "_debug" );
			logger.DeleteLogs( "_error" );
			logger.DeleteLogs( "_log" );
		}

		private void SchedDbf(object sender, System.EventArgs e)
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

	}
}
