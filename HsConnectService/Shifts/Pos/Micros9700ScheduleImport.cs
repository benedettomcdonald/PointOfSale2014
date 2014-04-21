using HsConnect.Shifts;
using HsConnect.Main;
using HsConnect.Data;

using System;
using System.IO;
using System.Text;
using System.Data;
using Microsoft.Data.Odbc;

namespace HsConnect.Shifts.Pos
{
	public class Micros9700ScheduleImport : ScheduleImportImpl
	{
		/*Default Constructor
		*/
		public Micros9700ScheduleImport()
		{
			this.logger = new SysLog( this.GetType() );
			nextSeq = 0;
		}

		private SysLog logger;
		private int nextSeq;
		private HsData data = new HsData();
		private static String ID = "Row0";
		private static String EMP_NUM = "Row1";
		private static String CLOCK_IN = "Row2";
		private static String CLOCK_OUT = "Row3";
		private static String JOB_ID = "Row4";


		/*Execute method
		 * Called when the ScheduleImport executes
		 * and the pos-type is Micros 9700.
		 * Gets posShifts and compares them with the hsShifts.
		 * Adds, updates and deletes shifts as necessary
		 */ 
		public override void Execute()
		{
			Console.WriteLine("BEGIN:  9700 Execute method for ScheduleImport");		
			ShiftList posShifts = GetMicros9700Schedules();
			ShiftList hsShifts = this.Shifts;
			// create lists for insert,delete,and update
			ShiftList addShifts = new ShiftList();
			ShiftList deleteShifts = new ShiftList();
			ShiftList updateShifts = new ShiftList();

			foreach( Shift shift in hsShifts )
			{
				if( posShifts.Contains( shift ) )
				{
					logger.Debug( "found match!" );
					Shift posShift = posShifts.Get( shift );
					if( shift.ClockIn.Hour != posShift.ClockIn.Hour ||
						shift.ClockIn.Minute != posShift.ClockIn.Minute ||
						shift.ClockOut.Hour != posShift.ClockOut.Hour ||
						shift.ClockOut.Minute != posShift.ClockOut.Minute )
					{
						logger.Debug( "adding to update list" );
						shift.PosId = posShift.PosId;
						if( shift.PosEmpId > 0 ) addShifts.Add( shift );
					}
				} 
				else
				{
					addShifts.Add( shift );
				}
			}

			foreach( Shift shift in posShifts )
			{
				if( !hsShifts.Contains( shift ) )
				{
					if( shift.PosEmpId > 0 ) deleteShifts.Add( shift );
				}
			}
			DeleteShiftsFromMicros9700();
			AddShiftsToMicros9700( addShifts );
			//DeleteShiftsFromMicros9700( deleteShifts );
			//UpdateShiftsInMicros9700( updateShifts );


			Console.WriteLine("END:  9700 Execute method for ScheduleImport");		
		}

		/*public StaffList GetMicros9700Schedules
		 * this method runs the query in 9700 to import all of the shifts currently in 9700.
		 * it then loads the shifts for the appropriate days into the list
		 */
		private ShiftList GetMicros9700Schedules()
		{
			Console.WriteLine("This is the 9700 GetMicros9700Schedules method for ScheduleImport");		
			Micros9700Control.Run(Micros9700Control.IMPORT_SCHEDULE);
			DataTableBuilder builder = new DataTableBuilder();
			DataTable dt = builder.GetTableFromCSV( Micros9700Control.PATH_NAME, Micros9700Control.SCHEDULE_FILE );
			ShiftList posShifts = new ShiftList();
			if(dt != null)
			{
				DataRowCollection rows = dt.Rows;
				foreach ( DataRow row in rows)
				{
					Shift shift = new Shift();
					shift.PosEmpId = data.GetInt( row , EMP_NUM );
					shift.PosId = data.GetInt( row , ID );
					shift.PosJobId = data.GetInt( row , JOB_ID );
					String inTime = row[CLOCK_IN].ToString();
					String outTime = row[CLOCK_OUT].ToString();
					shift.ClockIn = DateTime.Parse(inTime);
					shift.ClockOut = DateTime.Parse(outTime);
					nextSeq = shift.PosId + 1;
					if(shift.ClockIn.CompareTo(DateTime.Today) >= 0 && shift.ClockIn.CompareTo(DateTime.Today.AddDays(2)) < 1)
					{
						posShifts.Add( shift );
					}

					if( shift.ClockIn.TimeOfDay >= new TimeSpan( 4, 0, 0 ) && shift.ClockIn.TimeOfDay <= new TimeSpan( 15, 30, 0 ) )
					{
						shift.ClientShift = 0;
					} 
					else shift.ClientShift = 1;
				}
			}
			return posShifts;
		}
		#region add/update/delete
		/*public void AddShiftsToMicros9700
		 * Takes in a list of shifts and creates a .csv file
		 * containing the info in these shift objects.  Then
		 * calls a script that executes the add query
		 */
		private void AddShiftsToMicros9700( ShiftList shifts )
		{
			logger.Debug( "Adding shifts to Micros" );
			StringBuilder inputStr;
			String typeName = "add_shifts";
			int next = 1;
			StreamWriter writer = new StreamWriter(Micros9700Control.PATH_NAME + typeName + Micros9700Control.EXT);
			Console.WriteLine("This is the 9700 AddShiftsToMicros9700 method for ScheduleImport");	
			foreach(Shift shift in shifts)
			{
				if(shift.PosJobId > 0 && shift.PosEmpId > 0)
				{
					inputStr =new StringBuilder();
					DateTime dt = shift.ClockIn;
					String hour = (dt.Hour < 10 ? "0"+dt.Hour: dt.Hour.ToString());
					String minute = (dt.Minute < 10 ? "0"+dt.Minute: dt.Minute.ToString());
					String month = (dt.Month < 10 ? "0"+dt.Month: dt.Month.ToString());
					String day = (dt.Day < 10 ? "0"+dt.Day: dt.Day.ToString());
					String inTime = "\"" +hour+":"+minute+" " + month + "-"+ day + "-"+dt.Year.ToString().Substring(2,2)+ "\"";

					DateTime odt = shift.ClockOut;
					String ohour = (odt.Hour < 10 ? "0"+odt.Hour: odt.Hour.ToString());
					String ominute = (odt.Minute < 10 ? "0"+odt.Minute: odt.Minute.ToString());
					String omonth = (odt.Month < 10 ? "0"+odt.Month: odt.Month.ToString());
					String oday = (odt.Day < 10 ? "0"+odt.Day: odt.Day.ToString());
					String outTime = "\"" +ohour+":"+ominute+" " + omonth + "-"+ oday + "-"+odt.Year.ToString().Substring(2,2)+ "\"";

					inputStr.Append(next++ + "," + shift.PosEmpId + "," + inTime + "," + outTime + "," + shift.PosJobId + "," + "0,");
					writer.WriteLine(inputStr);
				}
			}
			writer.Close();
			
			Micros9700Control.Run(Micros9700Control.ADD_SHIFTS);
		}

		/*public void DeleteShiftsFromMicros9700
		 * Takes in a list of shifts and creates a .sql file
		 * containing the info in these shift objects.  Then
		 * calls a script that executes the query to delete
		 * these objects.
		 * Currently also creates a csv file of the shifts, for debug purposes***
		 */
		private void DeleteShiftsFromMicros9700(  )
		{
			Micros9700Control.CreateDeleteShiftsSQLFile();
			Micros9700Control.Run(Micros9700Control.DELETE_SHIFTS);
		}

		/*public void UpdateShiftsInMicros9700
		 * Takes in a list of shifts and creates a .csv file
		 * containing the info in these shift objects.  Then
		 * calls a script that executes the update query
		 */
		private void UpdateShiftsInMicros9700( ShiftList shifts )
		{
			Console.WriteLine("This is the 9700 UpdateShiftsInMocros9700 method for ScheduleImport");		
			logger.Debug( "Adding shifts to Micros" );
			StringBuilder inputStr;
			String typeName = "update_shifts";
			StreamWriter writer = new StreamWriter(Micros9700Control.PATH_NAME + typeName + Micros9700Control.EXT);
			Console.WriteLine("This is the 9700 AddShiftsToMicros9700 method for ScheduleImport");	
			foreach(Shift shift in shifts)
			{
				inputStr =new StringBuilder();
				inputStr.Append(shift.PosId + "," + shift.PosEmpId + "," + shift.ClockIn + "," + shift.ClockOut + "," + shift.PosJobId + "," + "0");
				writer.WriteLine(inputStr);
			}
			writer.Close();
			Micros9700Control.Run(Micros9700Control.UPDATE_SHIFTS);
		}
		#endregion
		#region unused		
		//unused, but required by inheritence
		public override void GetWeekDates(){}
		public override DateTime CurrentWeek{ get{ return new DateTime(0); } set{ this.CurrentWeek = value; }}
		#endregion
	}
}
