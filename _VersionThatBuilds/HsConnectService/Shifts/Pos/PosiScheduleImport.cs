using HsConnect.Shifts;
using HsConnect.Main;
using HsConnect.Data;
using HsConnect.Employees;
using HsConnect.Employees.PosList;
using HsConnect.EmpJobs;
using HsConnect.EmpJobs.PosList;
using HsConnect.Jobs;
using HsConnect.Jobs.PosList;
using HsSharedObjects.Client.Preferences;

using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Xml;
using System.Windows.Forms;
using Microsoft.Data.Odbc;

namespace HsConnect.Shifts.Pos
{
	public class PosiScheduleImport : ScheduleImportImpl
	{
		public PosiScheduleImport()
		{
			this.logger = new SysLog( this.GetType() );
		}

		private SysLog logger;
		private HsData data = new HsData();
		private XmlDocument employeeXml;
		private DateTime lastWeek = new DateTime(0);
		private DateTime currWeek = new DateTime(0);
		private DateTime nextWeek = new DateTime(0);
		private static String drive = Data.PosiControl.Drive;
        private bool altFlag = false;
        private Hashtable empMapping = null;
        private bool deleteAllFirst = false;
        private string dateFormatString = "MM/dd/yyyy";

		public override DateTime CurrentWeek
		{
			get
			{
				if( DateTime.Now >= this.nextWeek ) return this.nextWeek;
				return this.currWeek;
			}
			set{ this.currWeek = value; }
		}

		public override void Execute()
		{
            SetPreferences();
			PosiControl.Run( PosiControl.TAW_EXPORT );
			LoadPosiEmployees();
            if (deleteAllFirst)
                DeleteShifts();
			XmlDocument doc = employeeXml;
			ShiftList hsShifts = this.Shifts;
			foreach( Shift shift in hsShifts )
			{
                XmlNode node = null;
                if (altFlag)
                {
                    node = doc.SelectSingleNode("/Employees/UpdateEmployee[AltEmplNumber = " + shift.PosEmpId.ToString() + "]");
                }
                else
                {
                    node = doc.SelectSingleNode("/Employees/UpdateEmployee[EmployeeNumber = " + shift.PosEmpId.ToString() + "]");
                }
				LoadScheduleRecord( doc, node, shift );
			}
			logger.Debug( doc.OuterXml );
			CreateFile( doc.OuterXml );
			PosiControl.Run( PosiControl.TAXML );
		}

        private void SetPreferences()
        {
            if (this.Details.Preferences.PrefExists(Preference.USE_ALTID_NOT_EXTID))
            {
                altFlag = true;
                empMapping = PosiControl.MapEmployees(false);
            }
            if (this.Details.Preferences.PrefExists(Preference.DELETE_ALL_SHIFTS_POSI_SCHEDULES))
            {
                deleteAllFirst = true;
            }
            if (this.Details.Preferences.PrefExists(Preference.POSI_CUSTOM_DATE_FORMAT))
            {
                dateFormatString = this.Details.Preferences.GetPreferenceById(Preference.POSI_CUSTOM_DATE_FORMAT).Val2;
            }
        }

        private string GetEmpNumber(string id)
        {
            if (altFlag)
            {
                string ret = (string)empMapping[id];
                if (ret != null && ret.Length > 0)
                {
                    return ret;
                }
                else
                {
                    return id;
                }
            }
            else
            {
                return id;
            }
        }

		private void LoadScheduleRecord( XmlDocument doc, XmlNode node, Shift shift )
		{			
			XmlNode schNode = node.SelectSingleNode( "Schedules" );
			
			if( schNode == null )
			{
				schNode = doc.CreateElement( "Schedules" );
				LoadDeleteSchedules( doc, schNode, shift );
			}
			
			XmlElement shiftEle = doc.CreateElement( "ScheduleRecord" );

			XmlElement wkLbl = doc.CreateElement( "Week" );
			wkLbl.InnerText = GetWeekLabel( shift.ClockIn.Date );
			shiftEle.AppendChild( wkLbl );

			XmlElement inDay = doc.CreateElement( "InDay" );
			inDay.InnerText = shift.ClockIn.ToString( dateFormatString );
			shiftEle.AppendChild( inDay );

			XmlElement inTime = doc.CreateElement( "InTime" );
			inTime.InnerText = shift.ClockIn.ToString( "HH:mm" );
			shiftEle.AppendChild( inTime );

			XmlElement outDay = doc.CreateElement( "OutDay" );
            outDay.InnerText = shift.ClockOut.ToString( dateFormatString );
			shiftEle.AppendChild( outDay );

			XmlElement outTime = doc.CreateElement( "OutTime" );
			outTime.InnerText = shift.ClockOut.ToString( "HH:mm" );
			shiftEle.AppendChild( outTime );
			
			bool error = false;
			XmlElement jobNumber = doc.CreateElement( "JobNumber" );

			if( this.Details.Preferences.PrefExists( Preference.POSI_ALT_JOB ) )
			{
				Hashtable jobHash = GetAltHash();			
				if( jobHash.ContainsKey( shift.PosJobId.ToString() ) )
				{
					jobNumber.InnerText = (String) jobHash[ shift.PosJobId.ToString() ];
				}
			}
			else
			{
				jobNumber.InnerText = shift.PosJobId.ToString();
			}
            
			shiftEle.AppendChild( jobNumber );

			if( !error )
			{
				schNode.AppendChild( shiftEle );
				node.AppendChild( schNode );
			}
		}

		private String GetWeekLabel( DateTime date )
		{
            bool regWeek = Details.Preferences.PrefExists(Preference.POSI_REGULAR_WEEK);
			if( date >= this.lastWeek && date < this.currWeek )
			{
				return "LAST";
			} 
			else if( date >= this.currWeek && date < this.nextWeek  )
			{
				return "THIS";
			}
			else if( date >= this.nextWeek && date < this.nextWeek.AddDays(7.0)  )
			{
				return "NEXT";
            }
            else if (regWeek && date >= this.nextWeek.AddDays(7.0) && date < this.nextWeek.AddDays(14.0))
            {
                return "REGULAR";
            }
			return "";
		}

        private void DeleteShifts()
        {
            XmlDocument deleteXml = new XmlDocument();
            XmlNode empNode = deleteXml.CreateElement("Employees");
            int empId = 0;
            while (empId < 1000)
            {
                XmlNode node = deleteXml.CreateElement("UpdateEmployee");
                XmlNode numNode = deleteXml.CreateElement("EmployeeNumber");
                numNode.InnerText = "" + empId;
                node.AppendChild(numNode);

                empNode.AppendChild(node);
                XmlNode schNode = deleteXml.CreateElement("Schedules");
                LoadDeleteSchedules(deleteXml, schNode, null);
                empNode.AppendChild(schNode);
                empId++;
            }
            deleteXml.AppendChild(empNode);
            CreateFile(deleteXml.OuterXml);
            PosiControl.Run(PosiControl.TAXML);
        }

		private void LoadDeleteSchedules( XmlDocument doc, XmlNode node, Shift shift )
		{
            bool regWeek = Details.Preferences.PrefExists(Preference.POSI_REGULAR_WEEK);
            if (this.deleteAllFirst)
            {
                XmlNode delLast = doc.CreateElement("DeleteSchedules");
                delLast.InnerText = "LAST";
                node.AppendChild(delLast);
            }
			XmlNode delThis = doc.CreateElement( "DeleteSchedules" );
			delThis.InnerText = "THIS";
			node.AppendChild( delThis );
			
			XmlNode delNext = doc.CreateElement( "DeleteSchedules" );
			delNext.InnerText = "NEXT";
			node.AppendChild( delNext );
            if (regWeek || this.deleteAllFirst)
            {
                XmlNode delReg = doc.CreateElement("DeleteSchedules");
                delReg.InnerText = "REGULAR";
                node.AppendChild(delReg);
            }
		}

		private void LoadPosiEmployees()
		{
			try
			{
				ShiftList hsShifts = this.Shifts;
				Hashtable empTable = new Hashtable();
				foreach( Shift shift in hsShifts )
				{
                    //for the purpose of this export xml, the shift will have the right ID attached to it
                    string id = shift.PosEmpId.ToString();
					if( !empTable.ContainsKey( id ) ) empTable[id] = "";
				}
				employeeXml = new XmlDocument();
				XmlNode empNode = this.employeeXml.CreateElement( "Employees" );
				logger.Debug( "found " + empTable.Keys.Count + " employees" );
                // add global settings if using altId
                if (altFlag)
                {
                    XmlNode settingsNode = employeeXml.CreateElement("GlobalSettings");
                    XmlNode altSetting = employeeXml.CreateElement("UseAltEmplNumber");
                    altSetting.InnerText = "Y";
                    settingsNode.AppendChild(altSetting);
                    empNode.AppendChild(settingsNode);
                }
				foreach( object obj in empTable.Keys )
				{
					XmlNode node = employeeXml.CreateElement( "UpdateEmployee" );
                    if (altFlag)
                    {
                        XmlNode numNode = employeeXml.CreateElement("AltEmplNumber");
                        numNode.InnerText = (String)obj;
                        node.AppendChild(numNode);
                    }
                    else
                    {
                        XmlNode numNode = employeeXml.CreateElement("EmployeeNumber");
                        numNode.InnerText = (String)obj;
                        node.AppendChild(numNode);
                    }
                    empNode.AppendChild(node);
				}
				employeeXml.AppendChild( empNode );
			}
			catch( Exception ex )
			{
				logger.Error( ex.ToString() );
				Main.Run.errorList.Add(ex);
			}
		}

		public override void GetWeekDates()
		{

            PosiControl.Run(PosiControl.TAW_EXPORT);
			DataTableBuilder builder = new DataTableBuilder();
			DataTable tbl = builder.GetTableFromDBF(drive + @":\ALTDBF", @"C:", "SCHSETUP" );
			//DataTable tbl = builder.GetTableFromDBF( @"L:\SC", @"C:", "SCHSETUP" );
			DataRowCollection rows = tbl.Rows;
			foreach( DataRow row in rows )
			{
				try
				{
					this.lastWeek = GetDate( data.GetString( row , "LAST_WEEK" ) );
					this.currWeek = GetDate( data.GetString( row , "CURR_WEEK" ) );
					this.nextWeek = GetDate( data.GetString( row , "NEXT_WEEK" ) );
				}
				catch( Exception ex )
				{
					logger.Error( ex.ToString() );
					Main.Run.errorList.Add(ex);
				}
			}
			logger.Debug( "LAST WEEK: " + this.lastWeek.ToString() );
			logger.Debug( "CURRENT WEEK: " + this.currWeek.ToString() );
			logger.Debug( "NEXT WEEK: " + this.nextWeek.ToString() );
		}

		private void CreateFile( String xml )
		{
			StreamWriter writer = null;
			String finalPath =drive + @":\SC\EMPLOYEE.XML";
            String defaultPath = drive + @":\SC\xml_out\quickxml\EMPLOYEE.XML";
            String defaultDir = drive + @":\SC\xml_out\quickxml\";
            bool keepCopy = this.Details.Preferences.PrefExists(HsSharedObjects.Client.Preferences.Preference.BJS_COPY_EMPLOYEE_XML);
			try
   			{
                if (keepCopy)
                {
                    if (!Directory.Exists(defaultDir)) Directory.CreateDirectory(defaultDir);
                    if (File.Exists(defaultPath)) File.Delete(defaultPath);
                    writer = File.CreateText(defaultPath);
                    writer.Write(xml);
                }
                else
                {
                    if (File.Exists(finalPath)) File.Delete(finalPath);
                    writer = File.CreateText(finalPath);
                    writer.Write(xml);
                }
			}	
			catch( Exception ex )
			{
				logger.Error( ex.ToString() );
				Main.Run.errorList.Add(ex);
			}
			finally
			{
				writer.Flush();
				writer.Close();
			}
            if (keepCopy)
            {
                try
                {
                    File.Copy(defaultPath, finalPath, true);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.StackTrace);
                }
            }
		}

		private DateTime GetDate( String fullStr )
		{
			int year = Convert.ToInt32( fullStr.Substring( 0, 4 ) );
			int month = Convert.ToInt32( fullStr.Substring( 4, 2 ) );
			int day = Convert.ToInt32( fullStr.Substring( 6, 2 ) );
			DateTime date = new DateTime( year, month, day);
			return date;
		}

		private bool isStartWeek( int dayInt )
		{
			String day = DateTime.Now.DayOfWeek.ToString();
			switch( dayInt )
			{
				case 1:
					if( string.Compare( day, "Sunday", true ) == 0 ) return true;
					break;				case 2:
					if( string.Compare( day, "Monday", true ) == 0 ) return true;
					break;
				case 3:
					if( string.Compare( day, "Tuesday", true ) == 0 ) return true;
					break;
				case 4:
					if( string.Compare( day, "Wednesday", true ) == 0 ) return true;
					break;
				case 5:
					if( string.Compare( day, "Thursday", true ) == 0 ) return true;
					break;
				case 6:
					if( string.Compare( day, "Friday", true ) == 0 ) return true;
					break;
				case 7:
					if( string.Compare( day, "Saturday", true ) == 0 ) return true;
					break;
			}
			return false;
		}

		private Hashtable GetAltHash()
		{
			Hashtable jobs = new Hashtable();
			try
			{
				DataTableBuilder builder = new DataTableBuilder();
				DataTable dt = builder.GetTableFromDBF(drive + @":\ALTDBF", @"C:\", "JOBLIST" );
				//DataTable dt = builder.GetTableFromDBF( @"L:\SC", @"C:\", "JOBLIST" );
				DataRowCollection rows = dt.Rows;
				foreach( DataRow row in rows )
				{
					try
					{
						if( !jobs.ContainsKey( data.GetString( row , "ALT_CODE" ) ) )
						{
							jobs.Add( data.GetInt( row , "ALT_CODE" )+"", data.GetInt( row , "JOB_CODE" )+"" );
						}
					}
					catch( Exception ex )
					{
						logger.Error( ex.ToString() );
					}
				}
			}
			catch( Exception ex ) 
			{
				logger.Error( ex.ToString() );
				Main.Run.errorList.Add(ex);
			}
			return jobs;
		}

	}
}
