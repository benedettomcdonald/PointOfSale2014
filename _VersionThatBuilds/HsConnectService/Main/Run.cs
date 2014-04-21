using HsConnect.Machine;
using HsConnect.Services;
using HsConnect.Forms;
using HsProperties;
using HsError;

using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.ServiceProcess;

namespace HsConnect.Main
{
	/// <summary>
	/// The entry point for HS Connect.
	/// </summary>
	public class Run
	{
		public Run() {	}

		public static TrayIcon icon;
		public static LoginManager load;
		public static Timers timers;
		private static string [] args;
		private static bool mapped = false;
		public static HsErrorList errorList = new HsErrorList();
        public static bool IsStarted = false;

		[STAThread]
		public static void Start( String[] args ) 
		{
			SysLog logger = new SysLog( "HsConnect.Main.Run" );
			errorList.Clear("HsConnect.Main.Run");

			if(args != null)
				Run.args = args;
			else
				args = Run.args;
			logger.Log( "Initializing HS Connect" );
			String path = System.Windows.Forms.Application.StartupPath+ @"\properties.xml";
			Properties p = new Properties(path);
			
			mapped = p.MapDrive;
			logger.Log("Mapped? " + mapped + " : " + p.MapDrive);
			try
			{
				logger.Log( "Loading LoginManager" );
				LoginManager load = new LoginManager();
                IsStarted = true;
			}
			catch(Exception ex)
			{
				logger.Error(ex.ToString());
				logger.Error("Failed to start correctly.  Shutting down process");
				errorList.Add(ex);
                errorList.Send();
                IsStarted = false;
				//Environment.Exit( 0 );//kill this
			}
			finally
			{
				errorList.Send();
			}

		}

		public static void Stop()
		{
			StopConsole();
		}

		private static void StopConsole()
		{
			KillProcess("_hscnxConsole");
		}

		private static void KillProcess(String process)
		{
			SysLog logger = new SysLog( "HsConnect.Machine.Identification" );
			logger.Log( "killing the console" );

			foreach( Process p in Process.GetProcesses() )
			{
				if( p.ProcessName.Equals( "_hscnxConsole" ) )
				{
					logger.Log( "	killing" );
					p.Kill();
				}
			}

			logger.Log( "leaving method" );
		}

		public static bool Mapped
		{
			get{return mapped;}
			
		}


		
	}

}
