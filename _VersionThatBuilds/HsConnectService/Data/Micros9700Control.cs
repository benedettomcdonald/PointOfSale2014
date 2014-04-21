using HsConnect.Main;

using System;
using System.Threading;
using System.IO;
using System.Text;
using System.Data;
using System.Collections;

namespace HsConnect.Data
{
	public class Micros9700Control
	{
		public Micros9700Control(){
			logger = new SysLog( this.GetType() );
		}
        
		//STATICS
		//Path and file names for easier reuse
		public static String PATH_NAME = @"C:\hsTemp\";
		public static String INPUT_NAME = @"C:\Program Files\hotschedules\hs connect\";
		public static String RVC_FILE = "rvc_ttls";
		public static String SALES_FILE = "clsd_chk_ttls";
		public static String JOB_FILE = "empl_job_def";
		public static String TIME_FILE = "time_card";
		public static String SCHEDULE_FILE = "schedule";
		public static String EXT = ".csv";

		//used to make using the switch statement easier in CreateSqlFile, in case of reuse
		public static int JOBS = 0;
		public static int SALES = 1;
        public static int RVC = 2;

		// Represent the different batch files that can be called for certain syncs
		public static int EMPLOYEE_SYNC = 0;
		public static int JOB_SYNC = 1;
		public static int RVC_SYNC = 2;
		public static int SALES_SYNC = 3;
		public static int EMPLOYEE_JOB_SYNC = 4;
		public static int TIME_CARD_SYNC = 5;
		public static int IMPORT_SCHEDULE = 6;
		public static int ADD_SHIFTS = 7;
		public static int UPDATE_SHIFTS = 8;
		public static int DELETE_SHIFTS = 9;

		//statics used for setting the period set to be used in finding sales items
		/*we can also use the following period sets if needed in the future
		 *  1 = current shift
		 *	2 = previous shift
		 *	3 = yesterday
		 *	4 = previous week (this is starting on a sunday it appears, not based on today)
		 *	5 = previous pay period
		 *	6 = previous month
		 *	7 = previous quarter
		 *	211 = week-to-date
		 *	213 = month-to-date
		 *	215 = payperiod-to-date
		 *	221-227 = days of the week (monday(221) - sunday(227))
		 */
		public static int YESTERDAY = 3;
		public static int PREV_WEEK = 4;
		public static int WEEK_TO_DATE = 211;
		public static int PERIOD = YESTERDAY;

		protected static SysLog logger;
		
	
		public static void Run( int cmdCode )
		{
			Run( cmdCode, "", "" );
		}

		public static void Run( String cmd, String args )
		{
			Run( 99, cmd, args );
		}

		public static void Run( int cmdCode, String command, String arg )
		{
			SysLog logger = new SysLog( "HsConnect.Data.Micros9700Control" );
			String cmd = "";
			String args= "";
			try
			{
				switch( cmdCode )
				{				
					case 0://import employees
						cmd = "HS9700Employees.bat";
						args = "";
						break;
					case 1://import job codes
						cmd = "HS9700Jobs.bat";
						args = "";
						break;
					case 2://import rvc totals that are > 0
						cmd = "HS9700Rvc.bat";
						args = "";
						break;
					case 3://import sales items
						cmd = "HS9700Sales.bat";
						args = "";
						break;
					case 4://import employee jobs
						cmd = "HS9700EmployeeJobs.bat";
						args = "";
						break;
					case 5://import time cards
						cmd = "HS9700Time.bat";
						args = "";
						break;
					case 6://import schedule
						cmd = "HS9700ImportSched.bat";
						args = "";
						break;
					case 7://export new shifts
						cmd = "HS9700AddShifts.bat";
						args = "";
						break;
					case 8://export updated shifts
						cmd = "HS9700UpdateShifts.bat";
						args = "";
						break;
					case 9://remove shifts
						cmd = "HS9700DeleteShifts.bat";
						args = "";
						break;
					case 99:
						cmd = command;
						args = arg;
						break;
				}
				//if( !Application.ProductName.Equals( "DEBUG" ) )
				//{
					waitInLine();
					System.Diagnostics.Process proc = new System.Diagnostics.Process();
					proc.EnableRaisingEvents=false;
					proc.StartInfo.WorkingDirectory = INPUT_NAME;
					proc.StartInfo.FileName=cmd;
					proc.StartInfo.Arguments=args;
					proc.Start();
					proc.WaitForExit();
				//}
			}	
			catch( Exception ex )
			{
				logger.Error( ex.ToString() );
			}
		}
		#region unedited posi code
		private static void waitInLine()
		{
			while( File.Exists( @"C:\SC\TA.OPN" ) )
			{
				Thread.Sleep( 30000 );
			}
		}
		#endregion
		
		#region Deprecated Combine Methods
		/* This method will combine the job files into one csv file.
		 * The Micros 9700 system defines 8 parts of the structure.
		 * So we can safely iterate 8 times every time
		 * */
		public static void CombineJobFiles()
		{
			StreamWriter writer = new StreamWriter(PATH_NAME + JOB_FILE + EXT);
			StringBuilder inputName = new StringBuilder("empl_def_filtered_jobs1");
			for(int i = 1; i <=8; i++)
			{
				StreamReader reader= new  StreamReader(PATH_NAME + inputName + EXT);
				try   
				{    
					do
					{
						String tempStr = reader.ReadLine();
						if(tempStr != null)
						{
							writer.WriteLine(tempStr);
						}
					}   
					while(reader.Peek() != -1);
				}      
				catch (Exception ex)
				{ logger.Error( ex.ToString() ); }
				finally
				{ reader.Close(); }
				inputName.Replace(i.ToString(), (i+1).ToString());//iterate fileName to next file
			}
			writer.Close();
		}

		/*
		 * This method will combine the sales files into one csv file.
		 * The ArrayList nums contains the numbers representing the 
		 * existing RVCs
		 * */
		public static void CombineSalesFiles(ArrayList nums)
		{
			StreamWriter writer = new StreamWriter(PATH_NAME + SALES_FILE + EXT);
			StringBuilder inputName = new StringBuilder();
			foreach(Object o in nums)
			{
				int rvcNum = (Int32)o;
				inputName.Remove(0, inputName.Length);
				inputName.Append(SALES_FILE +  rvcNum);
				StreamReader reader= new  StreamReader(PATH_NAME + inputName + EXT);
				try
				{    
					do
					{
						String tempStr = reader.ReadLine();
						if(tempStr != null)
						{
							writer.WriteLine(tempStr);
						}
					}   
					while(reader.Peek() != -1);
				}catch (Exception ex)
				{ logger.Error( ex.ToString() ); }
				finally
				{ reader.Close(); }
			}
			writer.Close();
		}
		#endregion
		#region Combine Methods
		/* This is used for combining either job or sales files.
		 * It will parse a given list of files, 
		 * and create one master csv file for that data
		 */
		public static void CombineFiles(int type)
		{
			ArrayList nums = new ArrayList();
			for(int x = 1; x <= 8; x++)
			{
				nums.Add(x);
			}
			CombineFiles(type, nums);
		}
		public static void CombineFiles(int type, ArrayList nums)
		{
			StringBuilder inputName = new StringBuilder();
			String typeName = (type == SALES? SALES_FILE : JOB_FILE);
			StreamWriter writer = new StreamWriter(PATH_NAME + typeName + EXT);

			foreach(Object o in nums)
			{
				int num = (Int32)o;
				inputName.Remove(0, inputName.Length);
				inputName.Append(typeName +  num);
				StreamReader reader= new  StreamReader(PATH_NAME + inputName + EXT);
				try
				{    
					do
					{
						String tempStr = reader.ReadLine();
						if(type == JOBS )
						{
							tempStr = tempStr + num + ",";
						}
						if(tempStr != null)
						{
							writer.WriteLine(tempStr);
						}
					}   
					while(reader.Peek() != -1);
				}
				catch (Exception ex)
				{ logger.Error( ex.ToString() ); }
				finally
				{ reader.Close(); }
			}
			writer.Close();
		}

        public static void ClearFiles(int type, ArrayList nums)
        {
            StringBuilder inputName = new StringBuilder();
            String typeName = (type == SALES ? SALES_FILE : JOB_FILE);

            foreach (Object o in nums)
            {
                try
                {
                    int num = (Int32)o;
                    inputName.Remove(0, inputName.Length);
                    inputName.Append(typeName + num);
                    if (File.Exists(PATH_NAME + inputName + EXT))
                    {
                        File.Delete(PATH_NAME + inputName + EXT);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("failed to delete the file named " + inputName.ToString());
                }
            }
        }
		#endregion
		

		#region other util methods

		/**This method will check the current date against the "start date" from the module,
		 * and using this comparison will decide which period set needs to be used to query
		 * the correct sales data.  default is YESTERDAY(3)
		 */
		public static void setPeriod(DateTime startDate, DateTime today, int resetDay)
		{
			int daysPastReset = ((Int32)today.DayOfWeek) - resetDay;
			if(daysPastReset < 0)
				daysPastReset += 7;
			if(startDate.Date.CompareTo(today.Date.AddDays(-1)) < 0)
			{
				//decide if startDate is in current week or before current week
				if(startDate.Date.CompareTo(today.Date.AddDays(-daysPastReset)) < 0)
				{
					PERIOD = PREV_WEEK;
				}
				else
				{
					PERIOD = WEEK_TO_DATE;
				}

			}
			Console.WriteLine(PERIOD);
		}
		/*This mehtod sets the static PERIOD variable to a given int value
		*/ 
		public static void setPeriod(int per)
		{
			PERIOD = per;
		}

		/**This method will create a SQL file for execution in the 9700 batch file.
		 * It is currently for creating the sales sql files, which needs to be
		 * dynamic, based on RVCs
		 **/
		public static void CreateSQLFile(int type, ArrayList nums)
		{
			switch(type)
			{
				case 1:
					//STEP 1:  parse rvc_ttls file for valid RVCs
					DataTableBuilder builder = new DataTableBuilder();
					DataTable rvcDt = builder.GetTableFromCSV( PATH_NAME,  RVC_FILE );
					DataRowCollection rvcRows = rvcDt.Rows;
					HsData data = new HsData();
					foreach( DataRow row in rvcRows )
					{	
						int rvcNum = data.GetInt( row , "Row0" );
						nums.Add(rvcNum);
					}
					//STEP 2:  create dynamic SQL file
					StreamWriter writer = new StreamWriter(INPUT_NAME + RVC_FILE + ".sql");	
					writer.WriteLine("// Dynamically Generated file:  " + DateTime.Now.ToString());
					writer.WriteLine("PAD_NUMBERS ON ");
					writer.WriteLine("PAD_STRINGS ON ");
					writer.WriteLine("FIELD_SEPARATOR ,  ");
					writer.WriteLine("TOTALS_SET " + PERIOD);
				
					foreach(Object o in nums)
					{	
						int rvcNum = (Int32)o;
						writer.WriteLine("output_to " + PATH_NAME + "clsd_chk_ttls" + rvcNum + EXT);
						writer.WriteLine("SELECT rvc, number, chk_close_tm, sttl, reopened_to_chk_num, xfer_added_to_chk_num, order_type");
						writer.WriteLine("FROM clsd_chk_ttls." + rvcNum);
					}
					writer.Close();
					break;
                case 2:
                    /*We know the file contents, except for the TOTALS_SET X. 
                     * Regenerate the file with the correct totals_set.
                     * This will replace the existing HSSalesExtract.sql with the new file contents
                     */
                    StreamWriter writer2 = new StreamWriter(INPUT_NAME + "HSSalesExtract.sql");
                    writer2.WriteLine("// Dynamically Generated file:  " + DateTime.Now.ToString());
                    writer2.WriteLine("");
                    writer2.WriteLine("PAD_NUMBERS ON ");
					writer2.WriteLine("PAD_STRINGS ON ");
					writer2.WriteLine("FIELD_SEPARATOR ,  ");
					writer2.WriteLine("TOTALS_SET " + PERIOD);
                    writer2.WriteLine("");
                    writer2.WriteLine(@"output_to " + PATH_NAME + "rvc_ttls.csv ");
                    writer2.WriteLine("SELECT number, net_sls_ttl ");
                    writer2.WriteLine("FROM rvc_ttls ");
                    writer2.WriteLine("WHERE net_sls_ttl > 0 ");
					writer2.Close();
                    break;
				default:

					break;
			}
		}
		public static void CreateDeleteShiftsSQLFile()
		{
			DateTime dt = DateTime.Today;

			DateTime nineteenseventy = new DateTime(1970, 1, 1);
			TimeSpan ts = new TimeSpan(dt.Ticks - nineteenseventy.Ticks);
			long seconds = ((long)ts.TotalSeconds);

			//STEP 2:  create dynamic SQL file
			if(File.Exists(INPUT_NAME + "HSDeleteShifts" + ".sql")) File.Delete(INPUT_NAME + "HSDeleteShifts" + ".sql");
			StreamWriter writer = new StreamWriter(INPUT_NAME + "HSDeleteShifts" + ".sql");
			writer.WriteLine("// Dynamically Generated file:  " + DateTime.Now.ToString());
			writer.WriteLine("PAD_NUMBERS ON ");
			writer.WriteLine("PAD_STRINGS ON ");
			writer.WriteLine("DATE_TIME_NUMERIC ON ");
			writer.WriteLine("FIELD_SEPARATOR ,  ");
			writer.WriteLine("TOTALS_SET 222");


			writer.WriteLine("DELETE FROM time_clock_sched");
			writer.WriteLine("where clk_in_time > " + seconds);

			writer.Close();
		}
		#endregion

        /*
         * This method can be used to override the default working directory path of C:\Program Files\
         * 
         * It gets called from LoginManager, immediately after loading client details but before any syncs can take place
         */
        public static void SetupWorkingDirectory(string wd)
        {
            if (wd == null || wd.Length < 1)
            {
                return;
            }
            INPUT_NAME = wd + @"\hotschedules\hs connect\";
        }
	}
}
