using System.Globalization;
using System.Reflection;
using Nini.Config;

using hsupdater.Logger;
using hsupdater.Setup;
using hsupdater.Services;


using System;
using System.Timers;
using System.Collections;
using System.Diagnostics;
using System.Management;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.ServiceProcess;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;



namespace hsupdater
{
	public class Run
	{
		public Run()
		{
			
		}

		private static System.Timers.Timer _versionTimer = new System.Timers.Timer();
		private static System.Timers.Timer _hscTimer = new System.Timers.Timer();
		private static System.Timers.Timer _deleteLogsTimer = new System.Timers.Timer();
		private static String FILE_PATH = Application.StartupPath + "\\files";
		private static bool CHECKING_VERSION = false;

		//public static HsErrorList errorList = new HsErrorList();

		[STAThread]

			//	static void Main( String[] args ) 
		public static void StartService( String[] args )
		{
			//errorList.MAC = MacAddress;
			SysLog logger = new SysLog();
			logger.Log( "Starting the HS Updater." );
							
			try
			{
				/* Launch HS Updater. */
				if( !Directory.Exists( FILE_PATH ) )
				{
					Directory.CreateDirectory( FILE_PATH );
				}

				Settings settings = new Settings( Application.StartupPath );
			//	errorList.Clear("hsupdater.Run");
				logger.Log( "Loading the settings from the ini file." );				
				settings.Load( false ); // load ini settings

				/* Load timer that will check file versions. */
				_versionTimer.Interval = settings.VersionInterval;
				logger.Log( "	...loading the _versionTimer at " + _versionTimer.Interval + " milliseconds." );
				_versionTimer.Elapsed += new ElapsedEventHandler( CheckVersions );
				_versionTimer.Enabled = true;

				/* Load timer that will check if HS Connect is running. */
				_hscTimer.Interval = settings.HsConnectInterval;
				logger.Log( "	...loading the _hscTimer at " + _hscTimer.Interval + " milliseconds." );
				_hscTimer.Elapsed += new ElapsedEventHandler( CheckHsConnect );
				_hscTimer.Enabled = true;

				/* Load timer that will delete old logs. */
				_deleteLogsTimer.Interval = settings.DeleteLogs;
				logger.Log( "	...loading the _deleteLogsTimer at " + _deleteLogsTimer.Interval + " milliseconds." );
				_deleteLogsTimer.Elapsed += new ElapsedEventHandler( DeleteLogs );
				_deleteLogsTimer.Enabled = true;
				
				CheckVersions( new object(), null );
				//CheckHsConnect( new object(), null );
				//logger.Debug("Collected, total now in use:  " + GC.GetTotalMemory(false)/1000);
				//logger.Debug("Collected, total after GC:  " + GC.GetTotalMemory(true)/1000);
				return;
			}
			catch( Exception ex )
			{
				logger.Error( ex.ToString() );
			//	errorList.Add(ex);
			}
			finally
			{
			//	int x = errorList.Send();
			}
		}


		public static void StopService( )
		{
			SysLog logger = new SysLog();
			logger.Log( "Stopping the HS Updater." );
			_deleteLogsTimer.Enabled = false;
			_hscTimer.Enabled = false;
			_versionTimer.Enabled = false;
			//StopConsole();
			logger.Log("console stopped");
			StopHSCnx();
			logger.Log("_hscnx stopped");
			logger.Log("_hscnxConsole stopped");
			return;
		}
		
		private static void CheckVersions( object sender , ElapsedEventArgs e )
		{
			SysLog logger = new SysLog();
			//errorList.Clear("hsupdater.Run");
			logger.Log( "Checking file versions." );
			
			//load fresh settings, and apply new timer value
			Settings settings = new Settings( Application.StartupPath );
			settings.Load( true );
			
			//setting interval here should cause Elapsed event after settings.VersionInterval ms
 	        _versionTimer.Interval = settings.VersionInterval;

			bool saveSettings = false;
			CHECKING_VERSION = true;
            
			try
			{

				foreach( HsFile file in settings.LocalFileList )
				{
					if( settings.Properties.RemoteFiles.ContainsKey( file.Name ) )
					{
						HsFile remoteFile = (HsFile) settings.Properties.RemoteFiles[ file.Name ];
						if( remoteFile.Version != file.Version )
						{
							logger.Log( "Setting " + file.Name + " from version " + file.Version + " to verion " + remoteFile.Version + "." );
							file.Status = HsFile.UPDATE;
							file.Path = remoteFile.Path;
							file.Version = remoteFile.Version;
							saveSettings = true;
						}
					}
					else
					{
						logger.Log( "Setting " + file.Name + " to be deleted." );
						file.Status = HsFile.DELETE;
						saveSettings = true;
					}
				}

				foreach( HsFile file in settings.Properties.RemoteFileList )
				{
					logger.Log( "Contains key: " + settings.LocalFiles.ContainsKey( file.Name ).ToString() );
					logger.Log( "File exists: " + File.Exists( FILE_PATH + "\\" + file.Name ).ToString() );
					logger.Log( "" );
					if( !settings.LocalFiles.ContainsKey( file.Name )  ||
						!File.Exists( FILE_PATH + "\\" + file.Name ) )
					{
						HsFile newFile = new HsFile();
						newFile.Name = file.Name;
						newFile.Version = file.Version;
						newFile.Status = HsFile.ADD;
						newFile.Path = file.Path;
						logger.Log( "Setting " + file.Name + " to be added." );
						saveSettings = true;
						settings.AddFile( newFile );
					}
				}
			
				PropertiesService properties = new PropertiesService();
				if(saveSettings)
				{
					StopHSCnx();
					Thread.Sleep(3000);
					if( settings.SaveFileSettings() )
					{
						properties.setUpdate( MacAddress , 0 );
						logger.Log( "The files were successfully updated." );
					}
					else
					{
						properties.setUpdate( MacAddress , 1 );
						logger.Error( "The was an error updating the files." );
					}		
				}
				else
				{
					logger.Log( "No updates were found." );
				}
				
			}
			catch( Exception ex )
			{
				logger.Error( ex.ToString() );
			//	errorList.Add(ex);
			}
			finally
			{
				CHECKING_VERSION = false;
			//	errorList.Send();
				CheckHsConnect(sender, e);
			}

		}

		private static bool StopHSCnx()
		{
			SysLog logger = new SysLog();
			logger.Log("stopping HS Connect");			
			ServiceController sc = null;
			/* Determine if an instance of hs connect is running. */
			try
			{
				logger.Log("getting ServcieController");
				sc = new ServiceController("_hscnx");
				sc.MachineName = ".";
				logger.Log( "machine name: " + sc.MachineName );
				Thread.Sleep( 2000 );
			}
			catch( Exception ex )
			{
				logger.Error( "Error looking for _hscnx service. ", ex);
				throw;
			}
			try
			{
				sc.Refresh();
				if ( sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
				{
					logger.Log("in StopHsCnx():  _hscnx is started, need to stop it");
					try
					{
						Thread.Sleep(2000);
						sc.Stop();
						sc.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0,0,30));
					}
					catch(Exception ex)
					{
					    logger.Log("Unable to stop _hscnx service, attempting to kill the process instead");
					    try
					    {
					        KillProcess("_hscnx");
					    }
					    catch (Exception ex2)
					    {
					        logger.Error("Error stopping _hscnx service. Also failed to kill process", ex2);
					    }
					}
				}
			}
			catch(Exception ex)
			{
                logger.Error("Error stopping _hscnx service. ", ex);
				throw;
			}
			return sc.Status == ServiceControllerStatus.Stopped;
		}

		private static bool StartHSCnx()
		{
			SysLog logger = new SysLog();
			ServiceController sc = null;
			/* Determine if an instance of hs connect is running. */
			try
			{
				ServiceController[] listOfServices = ServiceController.GetServices();
					
				foreach( ServiceController serv in listOfServices )
				{
					if( serv.ServiceName.Equals( "_hscnx"))
					{
						sc = serv;
						break;
					}
				}
				Thread.Sleep( 2000 );
			}
			catch( Exception ex )
			{
				logger.Error( "Error looking for _hscnx service. " + ex.ToString() );
				throw;
			}
			try
			{
				if ( sc.Status == ServiceControllerStatus.Stopped ||  sc.Status == ServiceControllerStatus.StopPending )
				{
					string[] args = new string[1];
					args[0] = "hotschedulessuccess";
					logger.Log("right before start");
					sc.Start(args);
					logger.Log("HS Connect started");
					Thread.Sleep( 2000 );
				}
			}
			catch(Exception ex)
			{
				logger.Error( "Error starting _hscnx service. " + ex.ToString() );
				throw ex;
			}
			return true;
		}

		private static void StopConsole()
		{
			KillProcess("_hscnxConsole");
		}

		private static void CheckHsConnect( object sender , ElapsedEventArgs e )
		{
        	//load fresh settings, and apply new timer value
	        Settings settings = new Settings(Application.StartupPath);
 	 	 	settings.Load(true);
 	 	 	
			//setting interval here should cause Elapsed event after settings.HSConnectInterval ms
 	 	 	_hscTimer.Interval = settings.HsConnectInterval;

			if( !CHECKING_VERSION )
			{
				SysLog logger = new SysLog();
				//errorList.Clear("hsupdater.Run");
				logger.Log( "Checking HS Connect." );
				bool isRunning = false;
				try
				{
					isRunning = StartHSCnx();
					PropertiesService properties = new PropertiesService();
					int doRestart = 0;

					doRestart = properties.setRunning( MacAddress , (isRunning ? 1:0) , false);
					if(doRestart == 1)
					{
						isRunning = RestartHsConnect();
						properties.setRunning( MacAddress , (isRunning ? 1:0) , true);
					}
				}
				catch( Exception ex )
				{
					logger.Error( "Error in CheckHsConnect(): ", ex );
				//	errorList.Add(ex);
					logger.Error("sent from CheckHsConnect");
				//	errorList.Send();
				}
				if(isRunning)
					logger.Log( "Hs Connect is running!" );
			}

		}

		private static void DeleteLogs( object sender , ElapsedEventArgs e )
		{
			//load fresh settings, and apply new timer value
 	 	 	Settings settings = new Settings(Application.StartupPath);
 	 	 	settings.Load(true);
 	 	 	
			//setting interval here should cause Elapsed event after settings.DeleteLogs ms
 	 	 	_deleteLogsTimer.Interval = settings.DeleteLogs;
 	 	 	
			SysLog logger = new SysLog();
			logger.Log( "Deleting logs." );
			logger.DeleteLogs();
		}

		public static Boolean UseSSL
		{
			get
			{
				Assembly a = Assembly.LoadFile(FILE_PATH + @"/HsProperties.dll");
				Type t = a.GetType("HsProperties.Properties");
				try
				{
					return (Boolean) t.InvokeMember(
						"UseSSLMeth",
						BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static,
						null,
						null,
						new  object[]{FILE_PATH + @"/properties.xml"});
				}
				catch(Exception e)
				{
					SysLog logger = new SysLog();
					logger.Error(e.StackTrace);
				}
				return true;
			}
		}

		public static String BaseURL
		{
			get
			{
				Assembly a = Assembly.LoadFile(FILE_PATH + @"/HsProperties.dll");
				Type t = a.GetType("HsProperties.Properties");
				try
				{
					return (String) t.InvokeMember(
						"BaseURLMeth",
						BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static,
						null,
						null,
						new  object[]{FILE_PATH + @"/properties.xml"});
				}
				catch(Exception e)
				{
					SysLog logger = new SysLog();
					logger.Error(e.StackTrace);
				}
				return "soap.hotschedules.com";
			}
		}

		public static String MacAddress
		{
			get
			{
			    Assembly a = Assembly.LoadFile(FILE_PATH + @"/HsProperties.dll");
			    Type t = a.GetType("HsProperties.Properties");
			    Boolean useRegistry = false;
                try
                {
                    useRegistry = (Boolean) t.InvokeMember(
                                                        "UseRegistryMeth",
                                                        BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static,
                                                        null,
                                                        null,
                                                        new  object[]{FILE_PATH + @"/properties.xml"});
                }
                catch(Exception e)
                {
                    SysLog logger = new SysLog();
                    logger.Error(e.StackTrace);
                }

//                Properties p = new Properties(FILE_PATH + @"\properties.xml");
                if(useRegistry)
                {
                    a = Assembly.LoadFile(FILE_PATH + @"/HsSharedObjects.dll");
                    t = a.GetType("HsSharedObjects.Machine.Identification");
                    long concept=-1, unit=-1;
                    try
                    {
                        concept = (long) t.InvokeMember(
                                             "GetRegistryConcept",
                                             BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static,
                                             null,
                                             null,
                                             null);
                        unit = (long)t.InvokeMember(
                                             "GetRegistryUnit",
                                             BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static,
                                             null,
                                             null,
                                             null);
                    }
                    catch(Exception e)
                    {
                        SysLog logger = new SysLog();
                        logger.Error(e.StackTrace);
                    }

//                    RegistryIdentification regId = Identification.RegistryId;
                    return concept + ":" + unit;
                }
                else
                {
				SysLog logger = new SysLog( "HsConnect.Machine.Identification" );
				String macAddress = "";
				try
				{
					ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
					ManagementObjectCollection moc = mc.GetInstances();
					foreach(ManagementObject mo in moc)
					{
						if( ( bool ) mo["IPEnabled"] == true )
							macAddress = mo["MacAddress"].ToString();
						mo.Dispose();
					}
				}
				catch( Exception ex )
				{
					logger.Error( ex.ToString() );
					IniConfigSource _source = new IniConfigSource( "hs.ini" );
					macAddress = _source.Configs["Identification"].GetString("MacAddress");
				}
				return macAddress;
			}
		}
		}

		private static void KillProcess(String process)
		{
			SysLog logger = new SysLog( "HsConnect.Machine.Identification" );
			logger.Log( "killing the console" );

			foreach( Process p in Process.GetProcesses() )
			{
				if( p.ProcessName.Equals( process ) )
				{
					logger.Log( "	killing" );
					p.Kill();
				}
			}

			logger.Log( "leaving method" );
		}
		
		private static bool RestartHsConnect()
		{	
			bool isRunning = false;
			try
			{
				if(StopHSCnx())
					isRunning = StartHSCnx();
			}
			catch(Exception ex)
			{
				throw;				
			}
			return isRunning;
		}
	}
}
