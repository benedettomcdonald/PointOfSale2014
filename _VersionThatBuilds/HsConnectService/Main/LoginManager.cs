using System.Windows.Forms;
using HsConnect.Services;
using HsConnect.Forms;
using HsConnect.Data;
using HsSharedObjects;
using HsProperties;
using HsSharedObjects.Client;
using System.Runtime.InteropServices;
using System;
using System.Net;
using System.Timers;
using System.ServiceProcess;
using HsSharedObjects.Machine;
using HsSharedObjects.Client.Preferences;


//using System.Windows.Forms;

namespace HsConnect.Main
{
	/// <summary>
	/// Retrieves the client settings and loads the login form if necessary.

	/// </summary>
	
	public class LoginManager
	{
		public LoginManager() 
		{ 
			logger = new SysLog( this.GetType() );
            this.CHECK_FOR_STARTUP_ERROR_TIMER.Enabled = false;
            this.CHECK_FOR_STARTUP_ERROR_TIMER.Interval = 10000;
            this.CHECK_FOR_STARTUP_ERROR_TIMER.Elapsed += new ElapsedEventHandler(this.CheckStartupStatus);
			Load();
		}

		private int clientId;
		private SysLog logger;
        private System.Timers.Timer CHECK_FOR_STARTUP_ERROR_TIMER = new System.Timers.Timer();
	
		private void Load()
		{
			
			lock(this)
			{
				try
				{
					ClientVerifyService verifyServ = new ClientVerifyService();
                    Properties p = new Properties(Application.StartupPath + @"\properties.xml");
                    if (p.UseRegistry)
                    {
                        RegistryIdentification regId = Identification.RegistryId;
                        clientId = verifyServ.getClientId2(regId.Concept, regId.Unit);
                        logger.Debug("Registry identification: Concept=" + regId.Concept + " Unit=" + regId.Unit);
                    }
                    else
                    {
                        clientId = verifyServ.getClientId(Identification.MacAddress);
					logger.Debug( "MAC identification: " + Identification.MacAddress );
                    }
					
					
//					clientId = verifyServ.getClientId( Identification.MacAddress );
					logger.Log( clientId > 0 ? "Found Client " + clientId : "No Client found" );
					
					if( clientId < 0 )
					{
						HsCnxData.LoggedIn = false;
						return;
					}
					HsCnxData.LoggedIn = true;
					LoadClient loadClient = new LoadClient( clientId );
					ClientDetails details = loadClient.GetClientDetails();
					logger.Debug(details.ToString());

                    if (details.PosName.Equals("Micros9700"))
                    {
                        if(details.Preferences.PrefExists(Preference.MICROS9700_OVERRIDE_WORK_DIR))
                        {
                            Micros9700Control.SetupWorkingDirectory(details.Preferences.GetPreferenceById(Preference.MICROS9700_OVERRIDE_WORK_DIR).Val2);
                        }
                    }

					Run.errorList.ClientId = details.ClientId;
					Run.timers = new Timers( details );
					ClientSyncManager syncManager = new ClientSyncManager( details );
					try
					{
						logger.Debug( "Executing syncManager" );
						syncManager.Execute();
						logger.Debug("after execute");
					}
					catch( Exception ex )
					{
						logger.Error( ex.ToString() );
						Main.Run.errorList.Add(ex);
					}
				}
				catch( Exception ex )
				{
                    this.CHECK_FOR_STARTUP_ERROR_TIMER.Enabled = true;
					throw;
				}
			}
		}

        private void CheckStartupStatus(object sender, ElapsedEventArgs e)
        {
            if (!Run.IsStarted)
            {
                ServiceController sc = null;
                /* Determine if an instance of hs connect is running. */
                try
                {
                    logger.Log("getting ServcieController");
                    sc = new ServiceController("_hscnx");
                    sc.MachineName = ".";
                    sc.Stop();
                }
                catch (Exception ee)
                {
                    logger.Error("Error looking for _hscnx service. ", ee);
                    throw;
                }
            }
            this.CHECK_FOR_STARTUP_ERROR_TIMER.Enabled = false;
        }
	}
}
