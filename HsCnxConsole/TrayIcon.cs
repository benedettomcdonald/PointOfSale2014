using HsSharedObjects.Main;
using HsSharedObjects.Client;
using HsSharedObjects.Client.Module;
using HsSharedObjects.Client.CustomModule;

using System;
using System.Windows.Forms;

namespace HsCnxConsole
{
	/// <summary>
	/// Summary description for TrayIcon.
	/// </summary>
	public class TrayIcon : System.ComponentModel.Component
	{
		private System.Windows.Forms.NotifyIcon notifyIcon1;
		private System.Windows.Forms.Timer timer1;
		public System.Windows.Forms.ContextMenu contextMenu1;
		
		private System.ComponentModel.IContainer components;
		private SysLog logger;
		
		public TrayIcon(System.ComponentModel.IContainer container)
		{
			///
			/// Required for Windows.Forms Class Composition Designer support
			///
			container.Add(this);
			InitializeComponent();

		}

		public TrayIcon()
		{
			InitializeComponent();
			this.logger = new SysLog( this.GetType() );
			String bob = Run.hsCnx.MacAddress;
			this.clientDetails = Run.hsCnx.Details;
			this.timer1.Interval = HsSettings.GetMilliseconds( 10 );
			this.update_context_menu();
		}

		public TrayIcon( ClientDetails details )
		{
			InitializeComponent();
			this.logger = new SysLog( this.GetType() );
			this.clientDetails = details;
			this.timer1.Interval = HsSettings.GetMilliseconds( 10 );
			this.update_context_menu();
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
			this.timer1.Interval = 30000;
			this.timer1.Tick += new System.EventHandler(this.update_details);

		}
		#endregion

		private void notifyIcon1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			
		}

		public void IconOn( String msg )
		{
			System.Reflection.Assembly myAssembly = System.Reflection.Assembly.GetExecutingAssembly();
			System.IO.Stream s = myAssembly.GetManifestResourceStream("HsCnxConsole.hs-on.ico");
			notifyIcon1.Icon = new System.Drawing.Icon( s );
			s.Close();
		}

		public void IconOff( String msg )
		{
			System.Reflection.Assembly myAssembly = System.Reflection.Assembly.GetExecutingAssembly();
			System.IO.Stream s = myAssembly.GetManifestResourceStream("HsCnxConsole.hs.ico");
			notifyIcon1.Icon = new System.Drawing.Icon( s );
			s.Close();
		}

		private void update_details(object sender, System.EventArgs e)
		{
			logger.Debug( "updating details.." );
			try
			{
				//Run.hsCnx.UpdateDetails();
				//logger.Log("Console updating details");
				if(Run.hsCnx.Details != null)
				{
					this.clientDetails = Run.hsCnx.Details;
				}
				//update_context_menu();
				if( this.clientDetails != null ) update_context_menu();
			}
			catch( Exception ex )
			{
				logger.Error( ex.ToString() );
			}
		}

		private void update_context_menu()
		{			
			try
			{
				this.contextMenu1.MenuItems.Clear();
		
				int menuCnt = clientDetails.ModuleList.Count + clientDetails.CustomModuleList.Count + 1;
				if( Application.ProductName.Equals( "DEBUG" ) ) menuCnt++;
				if ( clientDetails.ModuleList.IsActive( ClientModule.SALES_VERIFICATION ) ) menuCnt--;
				if ( clientDetails.ModuleList.IsActive( ClientCustomModule.BJS_TIMECARD_IMPORT ) ) menuCnt--;
                if (clientDetails.ModuleList.IsActive(ClientModule.PERIOD_LABOR) && clientDetails.ModuleList.IsActive(ClientModule.LABOR_ITEMS)) menuCnt--;
				if(clientDetails.ModuleList.IsActive( ClientModule.REG_CLK_IN ) && !showRegClkIn() ) menuCnt--;//NEW CODE
				MenuItem[] menuItems = new MenuItem[ menuCnt ];
				logger.Debug("menu count: " + menuCnt.ToString() );
				int index = 0;

				if( clientDetails.ModuleList.IsActive( ClientModule.NET_SALES ) )
				{
					ClientModule module = clientDetails.ModuleList.GetModuleById( ClientModule.NET_SALES );
					int interval = module.SyncInterval;
				
					MenuItem item = MakeMenuItem( "Sales Sync" );
					item.Click += new System.EventHandler( this.SalesSync );
					menuItems[ index ] = item;
					index++;
					if( module.Force == 1 ) ForceSync( ClientModule.NET_SALES );
				} 
			

				if( clientDetails.ModuleList.IsActive( ClientModule.LABOR_ITEMS ) )
				{
					ClientModule module = clientDetails.ModuleList.GetModuleById( ClientModule.LABOR_ITEMS );
					int interval = module.SyncInterval;
                    if (!clientDetails.ModuleList.IsActive(ClientModule.PERIOD_LABOR))
                    {
                        MenuItem item = MakeMenuItem("Labor Sync");
                        item.Click += new System.EventHandler(this.LaborSync);
                        menuItems[index] = item;
                        index++;
                    }
					if( module.Force == 1 ) ForceSync( ClientModule.LABOR_ITEMS );
				} 
			

				if( clientDetails.ModuleList.IsActive( ClientModule.EMPLOYEE_DATA ) )
				{
					ClientModule module = clientDetails.ModuleList.GetModuleById( ClientModule.EMPLOYEE_DATA );
					int interval = module.SyncInterval;
			
					MenuItem item = MakeMenuItem( "Employee Sync" );
					item.Click += new System.EventHandler( this.EmpSync );
					menuItems[ index ] = item;
					index++;
					if( module.Force == 1 ) ForceSync( ClientModule.EMPLOYEE_DATA );
				} 
			

				if( clientDetails.ModuleList.IsActive( ClientModule.REG_CLK_IN ) )
				{
					ClientModule module = clientDetails.ModuleList.GetModuleById( ClientModule.REG_CLK_IN );
					int interval = module.SyncInterval;

					if(showRegClkIn())
					{
						MenuItem item = MakeMenuItem( "Import Schedule" );
						item.Click += new System.EventHandler( this.RegClockIn );
						menuItems[ index ] = item;
						index++;
					}
					if( module.Force == 1 ) ForceSync( ClientModule.REG_CLK_IN );
				} 
			

				if( clientDetails.ModuleList.IsActive( ClientModule.APPROVAL_ALERT ) )
				{
					ClientModule module = clientDetails.ModuleList.GetModuleById( ClientModule.APPROVAL_ALERT );
					int interval = module.SyncInterval;
			
					MenuItem item = MakeMenuItem( "Approval Alert" );
					item.Click += new System.EventHandler( this.ApprovalAlert );
					menuItems[ index ] = item;
					index++;
					if( module.Force == 1 ) ForceSync( ClientModule.APPROVAL_ALERT );
				} 
			

				if( clientDetails.CustomModuleList.IsActive( ClientCustomModule.CARINOS_GUEST_COUNT ) )
				{
					int interval = clientDetails.CustomModuleList.GetModuleById( ClientCustomModule.CARINOS_GUEST_COUNT ).SyncInterval;
			
					MenuItem item = MakeMenuItem( "Guest Count Sync" );
					item.Click += new System.EventHandler( this.CarinosGcSync );
					menuItems[ index ] = item;
					index++;
				} 

				if( clientDetails.CustomModuleList.IsActive( ClientCustomModule.MICROS_CVR_CNT ) )
				{
					int interval = clientDetails.CustomModuleList.GetModuleById( ClientCustomModule.MICROS_CVR_CNT ).SyncInterval;
			
					MenuItem item = MakeMenuItem( "Guest Count Sync" );
					item.Click += new System.EventHandler( this.MicrosCvrCntSync );
					menuItems[ index ] = item;
					index++;
				} 			

				if( clientDetails.ModuleList.IsActive( ClientModule.SALES_VERIFICATION ) )
				{
					ClientModule module = clientDetails.ModuleList.GetModuleById( ClientModule.SALES_VERIFICATION );
					int interval = module.SyncInterval;
			
					if( module.Force == 1 ) ForceSync( ClientModule.SALES_VERIFICATION );
				} 
			

				if( clientDetails.ModuleList.IsActive( ClientModule.PERIOD_LABOR ) )
				{
					ClientModule module = clientDetails.ModuleList.GetModuleById( ClientModule.PERIOD_LABOR );
					int interval = module.SyncInterval;
				
					MenuItem item = MakeMenuItem( "Labor Sync" );
					item.Click += new System.EventHandler( this.PeriodLabor );
					menuItems[ index ] = item;
					index++;

					if( module.Force == 1 ) ForceSync( ClientModule.PERIOD_LABOR );
				} 
			

				if( clientDetails.ModuleList.IsActive( ClientModule.SCHED_DBF ) )
				{
					ClientModule module = clientDetails.ModuleList.GetModuleById( ClientModule.SCHED_DBF );
					int interval = module.SyncInterval;
			
					MenuItem item = MakeMenuItem( "Import Data" );
					item.Click += new System.EventHandler( this.SchedDbf );
					menuItems[ index ] = item;
					index++;
					if( module.Force == 1 ) ForceSync( ClientModule.SCHED_DBF );
				} 			
				//PUNCH RECORD STUFF
				
				if( clientDetails.CustomModuleList.IsActive( 303) )
				{
					ClientCustomModule module = clientDetails.CustomModuleList.GetModuleById( ClientCustomModule.BJS_TIMECARD_IMPORT);
					//int interval = module.SyncInterval;
				/*
					MenuItem item = MakeMenuItem( "Import Punch Records" );
					item.Click += new System.EventHandler( this.POSI_TIME_CARD_IMPORT_Tick );
					menuItems[ index ] = item;
					index++;*/
				} 

				if( clientDetails.ModuleList.IsActive( 209  ))
				{
					ClientModule module = clientDetails.ModuleList.GetModuleById( ClientModule.PUNCH_RECORDS );
					int interval = module.SyncInterval;
				
					MenuItem item = MakeMenuItem( "Punch Record Sync" );
					item.Click += new System.EventHandler( this.PunchRecordSync );
					menuItems[ index ] = item;
					index++;
					if( module.Force == 1 ) ForceSync( ClientModule.PUNCH_RECORDS );
				} 
				
				if( clientDetails.ModuleList.IsActive( 220/*new file Xfer Sync*/ ) )
				{
					ClientModule module = clientDetails.ModuleList.GetModuleById( 220 );
					int interval = module.SyncInterval;
			
					MenuItem item = MakeMenuItem( "Transfer Files" );
					item.Click += new System.EventHandler( this.FileTransfer );
					menuItems[ index ] = item;
					index++;
					if( module.Force == 1 ) ForceSync( 220 );
				}
                
                if (clientDetails.ModuleList.IsActive(221/*new file Xfer Sync*/ ))
                {
                    ClientModule module = clientDetails.ModuleList.GetModuleById(221);
                    int interval = module.SyncInterval;

                    MenuItem item = MakeMenuItem("Cover Counts");
                    item.Click += new System.EventHandler(this.GuestCount);
                    menuItems[index] = item;
                    index++;
                    if (module.Force == 1) ForceSync(221);
                }
			
			    //add test connection button
                if (true)
                {
                    logger.Debug("Adding test connection button to tray");
                    MenuItem tcItem = MakeMenuItem("Test Connection");
                    tcItem.Click += new System.EventHandler(this.TestConnection);
                    logger.Debug("Test Connection event handler set");
                    menuItems[index] = tcItem;
                    logger.Debug("Test Connection added to menu items");
                    index++;
                }


				this.contextMenu1.MenuItems.AddRange( menuItems );
			}
			catch(Exception ex)
			{
				logger.Error("Error Updating context menu");
				logger.Error(ex.ToString());
			}
		}
		#region sync methods
		private void Sync(ClientModule mod)
		{
			this.IconOn("");
			mod.IsSyncing = true;
			try
			{	
				logger.Log("Console sync:  " + mod.ModuleId);
				bool returned = Run.hsCnx.Sync(mod);
			}
			catch(Exception ex)
			{
				logger.Error(ex.StackTrace);
			}
			finally
			{
				mod.IsSyncing = false;
				this.IconOff("");
			}
		}

        private void TestConnection(object sender, System.EventArgs e)
        {
            try
            {
                FormTestConnection ftc = new FormTestConnection();
                ftc.ShowDialog();
            }
            catch (Exception ex)
            {
                logger.Error("ERROR TESTING CONNECTION:  " + ex.ToString());
                logger.Error(ex.StackTrace);
            }
        }

		private void Sync(ClientCustomModule mod)
		{
			this.IconOn("");
			try
			{	
				logger.Log("Console Custom sync:  " + mod.ModuleId);
				bool returned = Run.hsCnx.Sync(mod);
			}
			catch(Exception ex)
			{
				logger.Error(ex.StackTrace);
			}
			finally
			{
				this.IconOff("");
			}
		}

		private void SalesSync(object sender, System.EventArgs e)
		{
			try
			{
				ClientModule mod = clientDetails.ModuleList.GetModuleById( ClientModule.NET_SALES );
			
				if(clientDetails.PosName.Equals("Aloha"))
				{
					SalesDateForm dateForm = new SalesDateForm();
					dateForm.MaxDate = DateTime.Today;
					dateForm.ShowDialog();
					if(dateForm.Cancel)
						return;
					Run.hsCnx.StartDate = dateForm.GetSalesStartDate();
					Run.hsCnx.EndDate = dateForm.GetSalesEndDate();
				}

				Sync(mod);
			}
			catch(Exception ex)
			{
				logger.Error("ERROR SYNCHING:  "+ex.ToString());
				logger.Error(ex.StackTrace);
			}
		}

		private void LaborSync(object sender, System.EventArgs e)
		{
			try
			{
				ClientModule mod = clientDetails.ModuleList.GetModuleById( ClientModule.LABOR_ITEMS );
				Sync(mod);
			}
			catch(Exception ex)
			{
				logger.Error("ERROR SYNCHING:  "+ex.ToString());
				logger.Error(ex.StackTrace);
			}
		}

		private void EmpSync(object sender, System.EventArgs e)
		{
			try
			{
				ClientModule mod = clientDetails.ModuleList.GetModuleById( ClientModule.EMPLOYEE_DATA );
				Sync(mod);
			}
			catch(Exception ex)
			{
				logger.Error("ERROR SYNCHING:  "+ex.ToString());
				logger.Error(ex.StackTrace);
			}
		}

		private void CarinosGcSync(object sender, System.EventArgs e)
		{
			try
			{
				ClientCustomModule mod = clientDetails.CustomModuleList.GetModuleById( ClientCustomModule.CARINOS_GUEST_COUNT );
				this.IconOn("");
				try
				{	
				
					bool returned = Run.hsCnx.Sync(mod);
				}
				catch(Exception ex)
				{
					logger.Error(ex.StackTrace);
				}
				finally
				{
					this.IconOff("");
				}
			}
			catch(Exception ex)
			{
				logger.Error("ERROR SYNCHING:  "+ex.ToString());
				logger.Error(ex.StackTrace);
			}
		}

		private void MicrosCvrCntSync(object sender, System.EventArgs e)
		{
			try
			{
				ClientCustomModule mod = clientDetails.CustomModuleList.GetModuleById( ClientCustomModule.MICROS_CVR_CNT );
				this.IconOn("");
				try
				{	
					bool returned = Run.hsCnx.Sync(mod);
				}
				catch(Exception ex)
				{
					logger.Error(ex.StackTrace);
				}
				finally
				{
					this.IconOff("");
				}
			}
			catch(Exception ex)
			{
				logger.Error("ERROR SYNCHING:  "+ex.ToString());
				logger.Error(ex.StackTrace);
			}
		}

		private void SalesVerify(object sender, System.EventArgs e)
		{
			try
			{
				ClientModule mod = clientDetails.ModuleList.GetModuleById( ClientModule.SALES_VERIFICATION );
				Sync(mod);
			}
			catch(Exception ex)
			{
				logger.Error("ERROR SYNCHING:  "+ex.ToString());
				logger.Error(ex.StackTrace);
			}
		}
		
		private MenuItem MakeMenuItem( String text )
		{
			MenuItem item = new MenuItem();
			item.Text = text;
			return item;
		}

		private void PeriodLabor(object sender, System.EventArgs e)
		{
			try
			{
				ClientModule mod = clientDetails.ModuleList.GetModuleById( ClientModule.PERIOD_LABOR );
				Sync(mod);
			}
			catch(Exception ex)
			{
				logger.Error("ERROR SYNCHING:  "+ex.ToString());
				logger.Error(ex.StackTrace);
			}
		}

		private void RegClockIn(object sender, System.EventArgs e)
		{
			try
			{
				ClientModule mod = clientDetails.ModuleList.GetModuleById( ClientModule.REG_CLK_IN );
				Sync(mod);
			}
			catch(Exception ex)
			{
				logger.Error("ERROR SYNCHING:  "+ex.ToString());
				logger.Error(ex.StackTrace);
			}
		}

		private void PunchRecordSync(object sender, System.EventArgs e)
		{
			ClientModule mod = clientDetails.ModuleList.GetModuleById( ClientModule.PUNCH_RECORDS );
			Sync(mod);
		}
		private void POSI_TIME_CARD_IMPORT_Tick(object sender, System.EventArgs e)
		{
			ClientCustomModule mod = clientDetails.CustomModuleList.GetModuleById( ClientCustomModule.BJS_TIMECARD_IMPORT );
			Sync(mod);
		}
		
		private void ApprovalAlert(object sender, System.EventArgs e)
		{
			try
			{
				ClientModule mod = clientDetails.ModuleList.GetModuleById( ClientModule.APPROVAL_ALERT );
				Sync(mod);
			}
			catch(Exception ex)
			{
				logger.Error("ERROR SYNCHING:  "+ex.ToString());
				logger.Error(ex.StackTrace);
			}
		}

		private void SchedDbf(object sender, System.EventArgs e)
		{
			try
			{
				ClientModule mod = clientDetails.ModuleList.GetModuleById( ClientModule.SCHED_DBF );
				Sync(mod);
				int cntr = 0;
				while( cntr <= 60 && mod.IsSyncing )
				{
					System.Threading.Thread.Sleep( 1000 );
					cntr++;
				}
				if( cntr > 60 && mod.IsSyncing )
				{
					MessageBox.Show( "Timed out." );
				}
				else
				{
					MessageBox.Show( "The data import is complete." );
				}
			}
			catch(Exception ex)
			{
				logger.Error("ERROR SYNCHING:  "+ex.ToString());
				logger.Error(ex.StackTrace);
			}
		}

		private void FileTransfer(object sender, System.EventArgs e)
		{
			try
			{
				ClientModule mod = clientDetails.ModuleList.GetModuleById( 220 );
				Sync(mod);
			}
			catch(Exception ex)
			{
				logger.Error("ERROR SYNCHING:  "+ex.ToString());
				logger.Error(ex.StackTrace);
			}
		}

        private void GuestCount(object sender, System.EventArgs e)
        {
            try
            {
                ClientModule mod = clientDetails.ModuleList.GetModuleById(221);
                Sync(mod);
            }
            catch (Exception ex)
            {
                logger.Error("ERROR SYNCHING:  " + ex.ToString());
                logger.Error(ex.StackTrace);
            }
        }

		private void ForceSync( int modId )
		{
			Run.hsCnx.ForceSync( modId );
		}
		#endregion

		private int getMinute()
		{
			return DateTime.Now.Minute;
		}

		private int getHour()
		{
			return DateTime.Now.Hour;
		}

		private bool showRegClkIn()
		{
			bool hide = clientDetails.Preferences.PrefExists(/*Hide Sched Import*/1014);
			DateTime dt = DateTime.Now;
			int nowHour = dt.Hour;
			HsSharedObjects.Client.Preferences.Preference pref = clientDetails.Preferences.GetPreferenceById( 1014 );
			bool doShow = (!hide || (nowHour >= Int32.Parse(pref.Val2) && nowHour < Int32.Parse(pref.Val3)));
			return doShow;
		}
	}
}
