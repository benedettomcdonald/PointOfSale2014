using System;
using System.Net;
using System.Diagnostics;
using System.Windows.Forms;
using HsSharedObjects;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.ServiceProcess;
using HsProperties;
using System.Threading;
using System.Runtime.InteropServices; // DllImport
using System.IO;

using HsSharedObjects.Client;
using HsSharedObjects.Main;

namespace HsCnxConsole
{
	/// <summary>
	/// Summary description for Run.
	/// </summary>
	public class Run
	{
		public Run(){}

		public static TrayIcon icon;
		public static HsCnxMethods hsCnx;
		public static WizardSharedMethods wiz;
		private static String Computer;
		private static String HsSharedPort;
		private static bool Remote;
		private static SysLog logger = new SysLog( "HsCnxConsole.Run" );
		//testing

		private const int STATIC = 0;
		private const int DYNAMIC = 1;
        private const int CPK_DYNAMIC = 2;

//		[DllImport("kernel32")]
//		static extern bool SetProcessWorkingSetSize(IntPtr handle, int minSize, int maxSize);

		private static String getDynamicIP(string[] args, String suffix)
		{
            return getDynamicIP(args, suffix, false);
        }
        
        private static String getDynamicIP(string[] args, String suffix, bool increment)
        {
			//CCF SPECIFIC CODE TO DYNAMICALLY CREATE IP OF REMOTE MACHINE
			String dynamicIP = "";
			String[] split = new String[4];
			try
			{
				String strHostName = "";
				if (args.Length == 0)
				{
					strHostName = Dns.GetHostName();
					Console.WriteLine ("Local Machine's Host Name: " +  strHostName);
				}
				else
				{
					strHostName = args[0];
				}
				IPHostEntry ipEntry = Dns.GetHostByName (strHostName);
				IPAddress [] addr = ipEntry.AddressList;
				char[] splitter = {'.'};
				split = addr[0].ToString().Split(splitter, 4);
                if (increment)
                {
                    int last = Int32.Parse(split[3]);
                    last--;
                    dynamicIP = split[0] + "." + split[1] + "." + split[2] + "." + last;
                    logger.Debug(dynamicIP);
                }
                else
                {
                    dynamicIP = split[0] + "." + split[1] + "." + split[2] + "." + suffix;
                }
			}   
			catch(Exception ex)
			{
				logger.Error("Getting the IP did not work");
				logger.Error(ex.ToString());
			}
			//END CCF SPECIFIC CODE
			return dynamicIP;
		}

		static void Main(string[] args)
		{
			try
			{
				logger.Debug( "Creating console connection." );
				Process[] proc = Process.GetProcessesByName( "_hscnxConsole" );
				if ( proc.Length <= 1 )
				{
					try
					{
						try
						{
							logger.Debug( "Reading in properties." );
							String path = System.Windows.Forms.Application.StartupPath+ @"\properties.xml";
							Properties p = new Properties(path);
							int group = p.GroupId;
							logger.Debug( "Group id = " + group );
                            switch (group)
                            {
                                case STATIC:
                                    Computer =  p.Computer;
                                    break;
                                case DYNAMIC:
                                    Computer = getDynamicIP(args, p.Suffix);
                                    break;
                                case CPK_DYNAMIC:
                                    Computer = getDynamicIP(args, "", true);
                                    break;
                            }
							
							HsSharedPort = p.SharedPort;
							logger.Debug( "Added port" );
							Remote = p.Remote;
						}
						catch(Exception ex)
						{
							logger.Error("Error reading properties");
							logger.Error(ex.ToString());
							logger.Error(ex.StackTrace);
						}
						logger.Debug( "Added local/remote" );
						ChannelServices.RegisterChannel(new TcpClientChannel());
						logger.Debug( "Registered TCP channel." );
						hsCnx = (HsCnxMethods)Activator.GetObject(
							typeof(HsCnxMethods), "tcp://"+ Computer+":"+HsSharedPort+"/HsCnxMethods");
						logger.Debug( "Retrieved object methods." );
						wiz = (WizardSharedMethods)Activator.GetObject(
							typeof(WizardSharedMethods), "tcp://"+ Computer+":"+HsSharedPort+"/WizardSharedMethods");
						logger.Debug( "Retrieved wizard methods." );
						
					}
					catch(Exception ex)
					{
						logger.Error("Error during connection with service");
						logger.Error(ex.ToString());
						logger.Error(ex.StackTrace);
					}
					/* Determine if an instance of hs connect is running. */
					try
					{
						logger.Debug( "Setting service controller." );
						ServiceController[] listOfServices = null;
						if(!Remote)
						{
							logger.Debug( "NOT REMOTE" );
							try
							{
								listOfServices = ServiceController.GetServices();
								logger.Debug( "Initialized list of services." );								
							}
							catch(Exception ex)
							{
								logger.Error("Error getting list of services");
								logger.Error(ex.ToString());
								logger.Error(ex.StackTrace);
							}
							int count = 0;
							while(!CheckForService(listOfServices))
							{
								Thread.Sleep(10000);
								listOfServices = ServiceController.GetServices(Computer);
								logger.Debug( "Retrieved list of services." );	
								count++;
								if(count >= 18)
								{
									System.Windows.Forms.MessageBox.Show("The HS Connect Service is not running.  Please Start the services before running the console");
									return;
								}
							}
							listOfServices = null;
						}
						
					}
					catch(Exception ex)
					{
						logger.Error("Error while checking for local/remote");
						logger.Error(ex.ToString());
						logger.Error(ex.StackTrace);
					}
					try
					{
						logger.Debug("about to test for login status");
						bool loggedIn = false;
                        int ct = 5;
                        try
                        {
                            //if there is a file named HsConsole.ini, then read the count from it
                            //this count, times 5, is the number of seconds that the console will
                            //delay waiting for HSC to finish its startup.  this is a workaround for
                            //the OSI sites that are seeing the popup.
                            String path = System.Windows.Forms.Application.StartupPath + @"\HsConsole.ini";
                            if (File.Exists(path))
                            {
                                logger.Debug("reading timeout value from the ini");
                                StreamReader reader = File.OpenText(path);
                                String countStr = "6";
                                try
                                {
                                    countStr = reader.ReadLine();
                                    ct = Int32.Parse(countStr.Trim());
                                }
                                finally
                                {
                                    reader.Close();
                                }

                            }

                        }
                        catch (Exception ex)
                        {
                            logger.Error("error reading from console.ini file");
                            logger.Error(ex.StackTrace);
                        }
                        logger.Debug("ct value = " + ct);
                        Properties p = new Properties(System.Windows.Forms.Application.StartupPath + @"\properties.xml");
					    bool useReg = p.UseRegistry;
						while(useReg || ct > 0)
						{
							loggedIn = hsCnx.LoggedIn;
							if(!loggedIn)
							{
								ct--;
								Thread.Sleep(5000);
							}
							else
								break;


						}
						if(!hsCnx.LoggedIn)
						{
							logger.Debug("starting the login methods");
							System.Windows.Forms.Application.Run(new HsCnxConsole.LoginForm());
							if(!hsCnx.LoggedIn)
								return;
						}
					}
					catch(Exception ex)
					{
						logger.Error("Error checking login status (Check connections and ports)");
						logger.Error(ex.ToString());
						logger.Error(ex.StackTrace);
						return;
					}
					try
					{

                        int waitTime;
                        for (waitTime = 1; hsCnx == null && waitTime <= 128; waitTime *= 2)
                            Thread.Sleep((int)(1000 * waitTime));
                        if(waitTime>1) logger.Debug("Waited " + (2*waitTime-1) + " seconds for hsCnx");

                        for (waitTime = 1; hsCnx.Details == null && waitTime <= 128; waitTime *= 2)
                            Thread.Sleep((int)(1000 * waitTime));
                        if(waitTime>1) logger.Debug("Waited " + (2*waitTime-1) + " seconds for hsCnx.Details");

						if( hsCnx.Details.Status == ClientDetails.ACTIVE )
						{
							if( hsCnx.Details.UseWizard == ClientDetails.WIZARD_ON )
							{
								/** Run the sync wizard, which only links POS jobs and roles by ids,
											 * as well as employees by id ONLY - this is separate from data sync 
											 **/
								Wizard wizard = new Wizard( hsCnx.Details );
								wizard.Show();
							}
						} 
                        icon = new TrayIcon();
						logger.Debug("Collected, total now in use:  " + GC.GetTotalMemory(false)/1000);
						logger.Debug("Collected, total after GC:  " + GC.GetTotalMemory(true)/1000);
						//	SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
                        Application.Run(); 
					}
					catch(Exception ex)
					{
						logger.Error("Failed to initialize console");
						logger.Error(ex.ToString());
						logger.Error(ex.StackTrace);
					}
				}
			}
			catch(Exception ex)
			{
				logger.Error("Error starting console");
				logger.Error(ex.ToString());
				logger.Error(ex.StackTrace);
			}
		}

		private static bool CheckForService(ServiceController[] listOfServices)
		{
			/* Determine if an instance of hs connect is running. */
			try
			{
					
				foreach( ServiceController serv in listOfServices )
				{
					if( serv.ServiceName.Equals( "_hscnx") && serv.Status == ServiceControllerStatus.Running)
					{
						return true;
					}
				}
				
				foreach( Process p in Process.GetProcesses() )
				{
					if( p.ProcessName.Equals( "_hscnx" ) )
					{
						return true;
					}
				}
			}
			catch( Exception ex )
			{
			}
			return false;
		}
	}
}
