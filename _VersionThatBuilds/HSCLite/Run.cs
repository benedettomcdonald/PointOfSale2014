using System;
using System.Timers;
using System.Threading;
using System.IO;
using System.Windows.Forms;

using HsError;
using HsSharedObjects.Main;

using HSCLite.Util;

namespace HSCLite
{
	/// <summary>
	/// Summary description for Run.
	/// </summary>
	public class Run
	{

		public static HsErrorList errorList = new HsErrorList("HSCLite");
		private static SysLog logger = new SysLog("HSCLite.Run");
		private static System.Timers.Timer _synchTimer = new System.Timers.Timer();
		private static System.Timers.Timer _checkInTimer = new System.Timers.Timer();
		private static String FILE_PATH = System.Windows.Forms.Application.StartupPath;
		private static int _storeNumber = -1;
		private static String clientName = "IHOP";
        private static String connectionString = @"DSN=micros;PWD=custom;UID=custom";//MICROS
		private static DateTime lastSynch = new DateTime(1982, 8, 23, 8, 46, 0);
		private static bool synching = false;
		private static bool success = false;
		private static bool retry = true;

		public Run()
		{
		}

		public static void StartService(String[] args)
		{
			try
			{
				errorList.Clear("HSCLite.Run.StartService()");
				logger.Log("Starting Service");
				/* Load timer that will run the synch */
				_synchTimer.Interval = 1  * 1000 * 10;
				logger.Log( "	...loading the _synchTimer at " + _synchTimer.Interval + " milliseconds." );
				_synchTimer.Elapsed += new ElapsedEventHandler( RunSynch );
				_synchTimer.Enabled = true;

				/* Load timer that will check in with server */
				_checkInTimer.Interval = 60  * 1000 * 60;
				logger.Log( "	...loading the _checkInTimer at " + _checkInTimer.Interval + " milliseconds." );
				_checkInTimer.Elapsed += new ElapsedEventHandler( CheckIn);
				_checkInTimer.Enabled = true;

				/*make sure everything we need to run is available*/
				verify();
				return;   
			}
			catch(Exception ex)
			{
				logger.Error(ex.ToString());
				errorList.Add(ex);
				errorList.Send();
				Environment.Exit(0);
			}
		}

		public static void StopService()
		{
			logger.Log("stopping service");
			errorList.Clear("HSCLite.Run.StopService()");
			try
			{
				_checkInTimer.Enabled = false;
				_synchTimer.Enabled = false;
			}
			catch(Exception ex)
			{
				logger.Error(ex.ToString());
				errorList.Add(ex);
				errorList.Send();
				Environment.Exit(0);
			}
		}

		private static void RunSynch( object sender , ElapsedEventArgs e )
		{
			try
			{
				synching = true;
				_synchTimer.Interval = 30 * 1000 * 60;
				success = false;
				errorList.Clear("HSCLite.Run.RunSynch()");
				Type type = Type.GetType( "HSCLite.Util." + clientName + "Util" );
				Util.Util util = (Util.Util) Activator.CreateInstance ( type );
				if(StoreNumber == -1 && util.NeedsStoreNumber)
				{
					throw new FormatException("Store Number could not be found:  value still set to " + _storeNumber + ".  Aborting sync", null);
				}
				success = util.RunSynch();
				lastSynch = DateTime.Now;
			}
			catch(TypeLoadException tlex)
			{
				logger.Error("Could not load Class:  HSCLite.Util." + clientName + "Util");
				throw;
			}
			catch(System.Reflection.TargetInvocationException tiex)
			{
				logger.Error("Could not load Class:  HSCLite.Util." + clientName + "Util");
				throw;
			}
			catch(Exception ex)
			{
				logger.Error(ex.ToString());
				errorList.Add(ex);
				errorList.Send();
			}
			finally
			{
				synching = false;
				if(!success && retry)
				{
					_synchTimer.Interval = 15 * 60000;
				}
				if(success)
				{
					_synchTimer.Interval = 360 * 60000;
				}
			}
		}

		//log status of HSCLite in server DB
		private static void CheckIn( object sender , ElapsedEventArgs e )
		{	int timeout = 0;
			while(synching && timeout < 4)
			{
				Thread.Sleep(30000);
				timeout ++;
			}
			Services.CheckinService chkServ = new HSCLite.Services.CheckinService();
			chkServ.CheckIn(StoreNumber, lastSynch.Ticks, success);
		}

		//check anything that we will need to run.  This makes sure we will run smoothly
		private static void verify()
		{
			
		}

		public static int StoreNumber
		{
			get{
				if(_storeNumber == -1)
				{
					Type type = Type.GetType( "HSCLite.Util." + clientName + "Util" );
					Util.Util util = (Util.Util) Activator.CreateInstance ( type );
					_storeNumber = util.GetStoreNumber();
				}
				
				String path = System.Windows.Forms.Application.StartupPath + @"\storenumber.ini";
				if( File.Exists( path ) )
				{
					_storeNumber = GetStoreFromIni();
				}

				return _storeNumber; 
			}
			set{ _storeNumber = value; }
		}

		public static String ClientName
		{
			get{ return clientName; }
		}

		public static String ConnectionString
		{
			get{ return connectionString; }
		}

		private static int GetStoreFromIni()
		{
			int storeNumber = -1;
			try
			{
				String path = System.Windows.Forms.Application.StartupPath + @"\storenumber.ini";
				if( File.Exists( path ) )
				{
					StreamReader reader = File.OpenText( path );
					try
					{
						storeNumber = Convert.ToInt32( reader.ReadLine() );
					} 
					finally
					{
						reader.Close();
					}				
				}
			}
			catch( Exception ex )
			{
				logger.Error( ex.ToString() );
			}
			return storeNumber;
		}
	}
}
