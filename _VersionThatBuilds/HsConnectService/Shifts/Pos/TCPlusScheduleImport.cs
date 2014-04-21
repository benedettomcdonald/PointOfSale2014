using HsConnect.Shifts;
using HsConnect.Main;
using HsConnect.Data;

using System;
using System.Collections;
using System.Data;
using System.IO;
using Microsoft.Data.Odbc;

//using HsConnect.Services;
using HsConnect.Services.Wss;
using HsSharedObjects.Client.Preferences;

using System.Xml;

namespace HsConnect.Shifts.Pos
{
	public class TCPlusScheduleImport : ScheduleImportImpl
	{
		public TCPlusScheduleImport()
		{
			this.logger = new SysLog( this.GetType() );
		}

		private SysLog logger;
		private HsData data = new HsData();

		public override void Execute()
		{
			HsFile hsFile = new HsFile();
			String empCnxStr = this.cnx.ConnectionString + @"\hstmp";
			OdbcConnection newConnection = this.cnx.GetCustomOdbc( empCnxStr );
			try
			{
				int dayOfWeek = this.Details.WorkWeekStart - 1;
				DateTime tempDate = DateTime.Today;
				if( dayOfWeek < 0 ) throw new Exception( "Business start day of week error" );
				while( String.Compare( GetDayOfWeek( dayOfWeek ) , tempDate.DayOfWeek.ToString(), true ) != 0 )
				{
					tempDate = tempDate.AddDays( -1.0 );
				}
				DateTime compareDate = new DateTime(tempDate.Ticks);
				logger.Debug( "opening " + empCnxStr );
				newConnection.Open();
				logger.Debug( "connection opened" );

				String tblName = createDBFFile(tempDate, newConnection);

				ShiftList hsShifts = this.Shifts;
				Hashtable empHash = null;
				tempDate = tempDate.AddDays(6);
				foreach( Shift shift in hsShifts )
				{
					if(shift.ClockIn.Date.CompareTo(tempDate) > 0)
					{
							File.Copy( this.Cnx.Dsn+@"\hstmp\"+tblName+".DBF", this.Cnx.Dsn+@"\scheds\"+tblName+".DBF", true );
							tempDate = tempDate.AddDays(1);
							tblName = createDBFFile(tempDate, newConnection);
							tempDate = tempDate.AddDays(6);
							compareDate = compareDate.AddDays(7);
					}
					int inTime = Int32.Parse((shift.ClockIn.Hour)+"" + shift.ClockIn.Minute + (shift.ClockIn.Minute < 10 ? "0" : ""));
					int outTime = Int32.Parse((shift.ClockOut.Hour)+"" + shift.ClockOut.Minute + (shift.ClockOut.Minute < 10 ? "0" : ""));
					
					int empId = shift.PosEmpId;
					int brk = 0;
					int flags = 0;
					int job = shift.PosJobId;

					int day = GetDay(compareDate, shift.ClockIn); 

					String desc = "HS shift:"+DateTime.Today.ToShortDateString();

					String insertSch = "INSERT INTO "+"S0"+tblName+" ( EMPNUM, DAY, JOBCODE, TIMEIN, TIMEOUT, BRK, [DESC], FLAGS ) " +
						" VALUES( "+empId+" , "+day+
						" , "+job+" , "+ inTime+" , "+outTime+" , "+brk+" , '"+ desc +"' , "+ flags +" )" ; 

					OdbcCommand insertCmd = new OdbcCommand( insertSch , newConnection );
					int cnt = insertCmd.ExecuteNonQuery();
				}
				
				File.Copy( this.Cnx.Dsn+@"\hstmp\S0"+tblName+".DBF", this.Cnx.Dsn+@"\scheds\S0"+tblName+".DBF", true );
				File.Copy( this.Cnx.Dsn+@"\hstmp\N0"+tblName+".DBF", this.Cnx.Dsn+@"\scheds\N0"+tblName+".DBF", true );
				hsFile.Delete( this.Cnx.Dsn+@"\hstmp\" +"S0" + tblName+".MDX" );
				hsFile.Delete( this.Cnx.Dsn+@"\hstmp\" +"N0" + tblName+".MDX" );
			}
			catch( Exception ex )
			{
				logger.Error( ex.ToString() );
			}
			finally
			{
			//	reader.Close();
				newConnection.Close();
			}
		}
			

		private String GetDayOfWeek( int i )
		{
			switch( i )
			{
				case 0:
					return "Sunday";
				case 1:
					return "Monday";
				case 2:
					return "Tuesday";
				case 3:
					return "Wednesday";
				case 4:
					return "Thursday";
				case 5:
					return "Friday";
				case 6:
					return "Saturday";
			}
			return "";
		}

		private int GetDay(DateTime compare, DateTime shift)
		{
			if(compare.Year == shift.Year)
			{
				return shift.DayOfYear - compare.DayOfYear;
			}

			DateTime shiftDate = new DateTime(shift.Ticks);
			int count = 0;
			while(shiftDate.CompareTo(compare) > 0)
			{
				count++;
				shiftDate.AddDays(-1);
			}
			return count;
		}

		public override void GetWeekDates()
		{
			return;
		}

		public override DateTime CurrentWeek{ get{ return new DateTime(0); } set{ this.CurrentWeek = value; }}

		private Hashtable EmpIdHash()
		{
			Hashtable hash = new Hashtable();
			ScheduleWss service = new ScheduleWss();
			XmlDocument doc = new XmlDocument();
			logger.Debug( "client id = " + this.Details.ClientId );
			if( this.Details.Preferences.PrefExists( Preference.IGNORE_DL_ID ) )
			{
				foreach( Shift shift in this.shifts )
				{
					if( !hash.ContainsKey( shift.PosEmpId + "" ) )
					{
						logger.Log( shift.PosEmpId + "" +","+ shift.PosEmpId + "" );
						hash.Add( shift.PosEmpId + "" , shift.PosEmpId + "" );
					}
				}
			}
			else
			{
				String mapXml = service.getDLMappingXML( this.Details.ClientId );
				logger.Debug( mapXml );
				doc.LoadXml( mapXml );
				foreach( XmlNode node in doc.SelectNodes( "/employee-id-mapping/employee" ) )
				{
					if( !hash.ContainsKey( node.Attributes["dl-id"].InnerText ) )
					{
						logger.Log( node.Attributes["dl-id"].InnerText +","+ node.Attributes["pos-id"].InnerText );
						hash.Add( node.Attributes["dl-id"].InnerText , node.Attributes["pos-id"].InnerText );
					}
				}
			}
			logger.Debug( "done mapping" );
			return hash;
		}

		private String createDBFFile(DateTime tempDate, OdbcConnection newConnection)
		{
			HsFile hsFile = new HsFile();
			String tblName = tempDate.ToString( "yyMMdd" );
			hsFile.Delete( this.Cnx.Dsn+@"\hstmp\" +"S0" + tblName+".DBF" );
			hsFile.Delete( this.Cnx.Dsn+@"\hstmp\" +"N0" + tblName+".DBF" );

			String tblName2 = "N0" + tempDate.ToString( "yyMMdd" );
			String createDBF1 = "CREATE TABLE "+"S0" + tblName+" ( EMPNUM number, DAY number , JOBCODE number , TIMEIN number , TIMEOUT number , BRK number , RATE number , [DESC] char(20) , FLAGS number )";
		
			OdbcCommand createCmd = new OdbcCommand( createDBF1 , newConnection );
			int rows = createCmd.ExecuteNonQuery();

			String createDBF2 = "CREATE TABLE "+"N0" + tblName+" ( DAY char(1) , JOBCODE char(10) , [DESC] char(20), TIMEIN number , TIMEOUT number, EMPNUM number)";
		
			createCmd = new OdbcCommand( createDBF2 , newConnection );
			rows = createCmd.ExecuteNonQuery();
			return tblName;
		}
	}
}

