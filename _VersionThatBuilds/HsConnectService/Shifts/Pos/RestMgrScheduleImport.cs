using HsConnect.Shifts;
using HsConnect.Main;
using HsConnect.Data;

using System;
using System.Data;
using System.IO;
using Microsoft.Data.Odbc;

namespace HsConnect.Shifts.Pos
{
	public class RestMgrScheduleImport : ScheduleImportImpl
	{
		public RestMgrScheduleImport()
		{
			this.logger = new SysLog( this.GetType() );
		}

		private SysLog logger;
		private HsData data = new HsData();

		/**This method executes when a Restaurant manager site imports a schedule.
		 * To import a schedule, the code creates a copy of EMPSCHED.DBF in the 
		 * hsTemp directory(section 1), populates it with the relevant schedule 
		 * items(section 2) and then moves the new file into the working directory 
		 * for RM(section 3). The code also has to delete the EmpSched.ntx index 
		 * file from the working directory or else RM will use the outdated index. 
		 */
		public override void Execute()
		{
			HsFile hsFile = new HsFile();
			String empCnxStr = this.Cnx.ConnectionString + @"\hstemp";
			OdbcConnection newConnection = this.cnx.GetCustomOdbc( empCnxStr );
			
			try
			{
				//SECTION 1
				logger.Debug( "opening " + empCnxStr );
				newConnection.Open();
				logger.Debug( "connection opened" );
				String tblName = "EMPSCHED";
				hsFile.Delete( this.Cnx.Dsn+@"\hstemp\"+tblName+".DBF" );
				//String createSch = "CREATE TABLE "+tblName+" ( EMP_NO int, JOB_NO int , SHIFTNUM number , "
				//+"[DATE] date , STARTTIME text(8) , ENDTIME text(8) , EMP_NOTIFY number, LOCKED number, PRIORITY number)";
				String createSch = "CREATE TABLE "+tblName+" ( EMP_NO int, JOB_NO int , [DATE] date , "
					+"STARTTIME text(8) , ENDTIME text(8) , NOTIFIED number)";
				OdbcCommand createCmd = new OdbcCommand( createSch , newConnection );
				int rows = createCmd.ExecuteNonQuery();

				//SECTION 2
				ShiftList hsShifts = this.Shifts;
				foreach( Shift shift in hsShifts )
				{
					try
					{
						String inTime = shift.ClockIn.ToString( "HH:mm:ss" );
						String outTime = shift.ClockOut.ToString( "HH:mm:ss" );
						String insertSch = "INSERT INTO "+tblName+" ( EMP_NO , JOB_NO , [DATE] , STARTTIME , ENDTIME ) " +
							" VALUES( "+shift.PosEmpId.ToString()+", "+shift.PosJobId.ToString()+
							", '"+shift.ClockIn.Date.ToString("yyyy/MM/dd")+"' , '"+inTime+"' , '"+outTime+"' )" ; 
						OdbcCommand insertCmd = new OdbcCommand( insertSch , newConnection );
						int cnt = insertCmd.ExecuteNonQuery();
					}
					catch(Exception ex)
					{
						logger.Error("Error adding Shift:  " + ex.ToString() + ex.StackTrace);
					}
				}
				//SECTION 3
				File.Copy( this.Cnx.Dsn+@"\hstemp\"+tblName+".DBF", this.Cnx.Dsn+@"\"+tblName+".DBF", true );
				File.Delete(this.Cnx.Dsn +@"\"+ tblName + ".ntx");
			}
			catch( Exception ex )
			{
				logger.Error( ex.ToString() );
			}
			finally
			{
				newConnection.Close();
			}
		}
			
		#region unused overrides
		public override void GetWeekDates()
		{
			return;
		}

		public override DateTime CurrentWeek{ get{ return new DateTime(0); } set{ this.CurrentWeek = value; }}
		#endregion

	}
}
