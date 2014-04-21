using HsSharedObjects.Client;
using HsSharedObjects.Client.Module;
using HsConnect.Data;
using HsConnect.Services;
using HsConnect.Services.Wss;
using HsConnect.Shifts;
using HsConnect.Shifts.Forecast;
using HsConnect.Main;

using System;
using System.Xml;
using System.Data;
using System.Collections;
using System.IO;
using System.Net;

using Microsoft.Data.Odbc;
using HsSharedObjects.Client.Preferences;

namespace HsConnect.Modules
{
	public class ScheduleDbfModule : ModuleImpl
	{
		public ScheduleDbfModule()
		{
			this.logger = new SysLog( this.GetType() );

			String ipAdress = getDynamicIP();
			_remoteTemplateInfoPath = @"\\" + ipAdress + @"\C\SC\Labor Templates\TemplateInfo97.mdb";
			_remoteScheduleDBFPath = @"\\" + ipAdress + @"\C\SC\SCHEDULE.DBF";
		}

		private SysLog logger;
		private Hashtable _applied = new Hashtable();
		private DateTime lastWeek;
		private DateTime currentWeek;
		private DateTime nextWeek;
		private HsData data = new HsData();
		private static String _remoteTemplateInfoPath = "";
		private static String _remoteScheduleDBFPath = "";
		private static String _localCnx = @"Driver={Microsoft Access Driver (*.mdb)};Dbq="+System.Windows.Forms.Application.StartupPath+"\\TemplateInfo97.mdb;Uid=Admin;Pwd=;";
        private bool altFlag = false;
        private Hashtable empMapping = null;

        private void SetPreferences()
        {
            if (this.Details.Preferences.PrefExists(Preference.USE_ALTID_NOT_EXTID))
            {
                altFlag = true;
                empMapping = PosiControl.MapEmployees(true);
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

		public override bool Execute()
		{
			if( Details.ModuleList.IsActive( ClientModule.SCHED_DBF ) )
			{		
				logger.Log( "Running SCHEDULE.DBF module." );
                SetPreferences();
				if( !File.Exists( System.Windows.Forms.Application.StartupPath+"\\TemplateInfo97.mdb" ) )
				{
					try
					{
						File.Copy( _remoteTemplateInfoPath, System.Windows.Forms.Application.StartupPath+"\\TemplateInfo97.mdb", false );
						System.Threading.Thread.Sleep( 2000 );
					}
					catch( Exception ex )
					{	
                        logger.Error( @"Error copying " + _remoteTemplateInfoPath );
						logger.Error( ex.ToString() );
					}
				}

				String cnxStr = @"Driver={Microsoft dBASE Driver (*.dbf)};DriverID=277;Dbq=" + System.Windows.Forms.Application.StartupPath;
				OdbcConnection newConnection = new OdbcConnection( cnxStr );
				
				// set up the start dates for this week, last and next
				SetUpWeeks();

				Hashtable jobHash = GetAltHash();

				ScheduleWss schedService = new ScheduleWss();
				ShiftList shiftList = new ShiftList();
				// set up the start dates for this week, last and next

                // delete schedules that are going to be replaced
                DeleteDBF();

                for(int i=0; i<3; i++ )
				{
					String startXml = GetDateXml( "start", lastWeek.AddDays( i * 7 ) );
					String endXml = GetDateXml( "end", currentWeek.AddDays( ( i * 7 ) - 1 ) );
                    String schedXml = "";
                    int errorCnt = 0;

                    while( errorCnt < 3 )
                    {
                        try
                        {
                            schedXml = schedService.getSchedulesXML(this.Details.ClientId, startXml, endXml);
                            break;
                        }
                        catch( Exception ex )
                        {
                            System.Threading.Thread.Sleep(5000);
                            errorCnt++;
                            logger.Error( ex.ToString() );
                        }
                    }

                    if( errorCnt >= 3 )
                    {
                        logger.Error("The SCHEDULE.DBF synch has attempted to get data for " + lastWeek.ToShortDateString()  + 
                            " at least 3 times, but has failed each attempt. The data in SCHEDULE.DBF may not be accurate.");
                    }

					shiftList.ImportScheduleXml( true, true, schedXml );
				}				

				CreateDBF(newConnection);
										
				newConnection.Open();
				int rowCnt = 0;
				int schedCode = 0;
                int tmpRowCnt = 0;
				
				try
				{
					logger.Debug( "Shifts in list: " + shiftList.Count );
					ArrayList badInserts = new ArrayList();
					int errorCnt = 0;
					foreach( Shift shift in shiftList )
					{
                        tmpRowCnt = 0;
                        OdbcCommand insert = new OdbcCommand("INSERT INTO SCHEDULE ([SCHED_CODE], [EMP_NUMBER], [SCHED_TYPE], " +
                                "[IN_DAY], [IN_TIME], [OUT_DAY], [OUT_TIME], [JOB_CODE], [WEEK_START], [TEMPL_NAME], [GROUP_NAME], " +
                                "[ASSGN_NAME], [SHIFT_CODE]) VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?)", newConnection);
                        try
                        {
                            schedCode++;

                            insert.Parameters.Add("p1", schedCode);
                            insert.Parameters.Add("p2", GetEmpNumber(shift.PosEmpId.ToString()));

                            int sType = 64;
                            if (shift.ClockIn.Date >= currentWeek)
                            {
                                sType = 65;
                            }
                            if (shift.ClockIn.Date >= nextWeek)
                            {
                                sType = 66;
                            }

                            insert.Parameters.Add("p3", sType);
                            insert.Parameters.Add("p4", shift.ClockIn.Date.ToString("MM/dd/yyyy"));
                            insert.Parameters.Add("p5", shift.ClockIn.ToString("HHmm"));
                            insert.Parameters.Add("p6", shift.ClockOut.Date.ToString("MM/dd/yyyy"));
                            insert.Parameters.Add("p7", shift.ClockOut.ToString("HHmm"));
                            logger.Debug(shift.PosJobId + "");
                            insert.Parameters.Add("p8", jobHash[shift.PosJobId + ""]);
                            insert.Parameters.Add("p9", GetWeek(shift.ClockIn.Date).ToString("MM/dd/yyyy"));
                            insert.Parameters.Add("p10", "N/A");
                            insert.Parameters.Add("p11", shift.SchedName);
                            insert.Parameters.Add("p12", shift.LocName);
                            insert.Parameters.Add("p13", shift.LocId);

                            //rowCnt += insert.ExecuteNonQuery();
                            tmpRowCnt = insert.ExecuteNonQuery();
                        }//try
                        catch (OdbcException odbcEx)
                        {
                            //handle cannot open any more tables odbc error
                            newConnection.Close();
                            newConnection = new OdbcConnection(cnxStr);
                            newConnection.Open();
                            if (tmpRowCnt <= 0)
                            {
                                try
                                {
                                    tmpRowCnt = insert.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    errorCnt++;
                                    badInserts.Add(shift);
                                    logger.Error(shift.ToString());
                                    logger.Error(ex.ToString());
                                }
                            }
                        }
                        catch (Exception exc)
                        {
                            errorCnt++;
                            badInserts.Add(shift);
                            logger.Error(shift.ToString());
                            logger.Error(exc.ToString());
                        }
                        rowCnt += tmpRowCnt;
					}//foreach
					logger.Log( rowCnt + " rows affected." );
					if( errorCnt > 0 )
					{
						logger.Error( "Error occured on " + errorCnt + " rows." );						
						foreach( Shift shift in badInserts )
						{
                            tmpRowCnt = 0;
                            OdbcCommand insert = new OdbcCommand("INSERT INTO SCHEDULE ([SCHED_CODE], [EMP_NUMBER], [SCHED_TYPE], " +
                                    "[IN_DAY], [IN_TIME], [OUT_DAY], [OUT_TIME], [JOB_CODE], [WEEK_START], [TEMPL_NAME], [GROUP_NAME], " +
                                    "[ASSGN_NAME], [SHIFT_CODE]) VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?)", newConnection);

                            try
                            {
                                schedCode++;

                                insert.Parameters.Add("p1", schedCode);
                                insert.Parameters.Add("p2", GetEmpNumber(shift.PosEmpId.ToString()));

                                int sType = 64;
                                if (shift.ClockIn.Date >= currentWeek)
                                {
                                    sType = 65;
                                }
                                if (shift.ClockIn.Date >= nextWeek)
                                {
                                    sType = 66;
                                }
                                insert.Parameters.Add("p3", sType);
                                insert.Parameters.Add("p4", shift.ClockIn.Date.ToString("MM/dd/yyyy"));
                                insert.Parameters.Add("p5", shift.ClockIn.ToString("HHmm"));
                                insert.Parameters.Add("p6", shift.ClockOut.Date.ToString("MM/dd/yyyy"));
                                insert.Parameters.Add("p7", shift.ClockOut.ToString("HHmm"));
                                logger.Debug(shift.PosJobId + "");
                                insert.Parameters.Add("p8", jobHash[shift.PosJobId + ""]);
                                insert.Parameters.Add("p9", GetWeek(shift.ClockIn.Date).ToString("MM/dd/yyyy"));
                                insert.Parameters.Add("p10", "N/A");
                                insert.Parameters.Add("p11", shift.SchedName);
                                insert.Parameters.Add("p12", shift.LocName);
                                insert.Parameters.Add("p13", shift.LocId);

                                //rowCnt += insert.ExecuteNonQuery();
                                tmpRowCnt = insert.ExecuteNonQuery();
                            }//try
                            catch (OdbcException odbcEx)
                            {
                                //handle cannot open any more tables odbc error
                                newConnection.Close();
                                newConnection = new OdbcConnection(cnxStr);
                                newConnection.Open();
                                if (tmpRowCnt <= 0)
                                {
                                    try
                                    {
                                        tmpRowCnt = insert.ExecuteNonQuery();
                                    }
                                    catch (Exception ex)
                                    {
                                        errorCnt++;
                                        badInserts.Add(shift);
                                        logger.Error(shift.ToString());
                                        logger.Error(ex.ToString());
                                    }
                                }
                            }
                            catch (Exception exc)
                            {
                                logger.Error("Reattempt: " + shift.ToString());
                                logger.Error(exc.ToString());
                            }
                            rowCnt += tmpRowCnt;
						}//foreach
					}
				}
				catch( Exception ex ) 
				{
					logger.Error( ex.ToString() );
				}
				finally
				{
					newConnection.Close();
				}
				
				File.Copy( System.Windows.Forms.Application.StartupPath + "\\SCHEDULE.DBF" , _remoteScheduleDBFPath );
				
				ForecastImport forecastImport = new ForecastImport();	
				forecastImport.AltJobHash = jobHash;
				
				logger.Debug( "************* LOADING SHIFTS INTO FORECAST LISTS ****************" );
					
				for(int i=0; i<13; i++ )
				{
					String startXml = GetDateXml( "start", currentWeek.AddDays( i * 7 ) );
					//String forecastXml = schedService.getForecastXML( 132054107 , startXml );
					String forecastXml = schedService.getForecastXML( this.Details.ClientId , startXml );
					forecastImport.XmlString = forecastXml;		
					forecastImport.Load();					
				}
				logger.Debug( "************* END ****************" );
				logger.Debug( "" );
				logger.Debug( "" );

				logger.Debug( forecastImport.ForecastLists.Count + " lists loaded." );
				logger.Debug( "" );

				// create required rows in other tables
				TemplatesTable( forecastImport.ForecastLists );
				AssignmentGroups( forecastImport.ForecastLists );

				logger.Debug( "" );
				logger.Debug( "" );
				logger.Debug( "************* DELETING OLD ROWS ****************" );
				DeleteForecastRows( forecastImport.ForecastLists );
				logger.Debug( "************* END ****************" );
				logger.Debug( "" );
				logger.Debug( "" );

				logger.Debug( "************* INSERTING NEW ROWS ****************" );
				InsertForecastRows( forecastImport.ForecastLists );	
				logger.Debug( "************* END ****************" );
				logger.Debug( "" );
				logger.Debug( "" );

				AppliedTemplates();

				System.Threading.Thread.Sleep( 2000 );

				try
				{
					File.Copy( System.Windows.Forms.Application.StartupPath+"\\TemplateInfo97.mdb", _remoteTemplateInfoPath, true );					
				}
				catch( Exception ex )
				{	
					logger.Error( @"Error copying " + _remoteTemplateInfoPath );
					logger.Error( ex.ToString() );
				}
				
			}
			return true;
		}

		private DateTime GetWeek( DateTime date )
		{
			if( date.Date < currentWeek.Date ) return lastWeek;
			if( date >= currentWeek.Date && date < nextWeek.Date ) return currentWeek;
			if( date.Date >= nextWeek.Date ) return nextWeek;
			return currentWeek;
		}

		private String GetDateXml( String desc, DateTime date )
		{
			logger.Log( desc + " = " + date.ToShortDateString() );

			XmlDocument doc = new XmlDocument();

			XmlElement start = doc.CreateElement( desc + "-date" );
			start.SetAttribute( "day" , date.Day+"" );
			start.SetAttribute( "month" , date.Month+"" );
			start.SetAttribute( "year" , date.Year+"" );

			return start.OuterXml;
		}

		private Hashtable GetAltHash()
		{
			Hashtable jobs = new Hashtable();
			try
			{
				DataTableBuilder builder = new DataTableBuilder();
				DataTable dt = builder.GetTableFromDBF( @"C:\ALTDBF", @"C:\", "JOBLIST" );
				DataRowCollection rows = dt.Rows;
				foreach( DataRow row in rows )
				{
					try
					{
						if( !jobs.ContainsKey( data.GetInt( row , "ALT_CODE" )+"" ) )
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
			}
			return jobs;
		}

		private void SetUpWeeks()
		{
			DateTime tmp = DateTime.Now;
			while( string.Compare( tmp.DayOfWeek.ToString(), this.Details.WeekStartDay, true ) != 0 )
			{
				tmp = tmp.AddDays( -1 );
			}
			currentWeek = tmp;
			lastWeek = new DateTime( tmp.Ticks ).AddDays( -7 );
			nextWeek = new DateTime( tmp.Ticks ).AddDays( 7 );
		}

		private void CreateDBF( OdbcConnection conn )
		{
			conn.Open();
		
			OdbcCommand insert =  new OdbcCommand("CREATE TABLE SCHEDULE ([SCHED_CODE] integer, [EMP_NUMBER] integer, " +
							"[SCHED_TYPE] integer, [IN_DAY] date, [IN_TIME] integer, [OUT_DAY] date, [OUT_TIME] integer, " +
							"[JOB_CODE] integer, [WEEK_START] date, [TEMPL_NAME] varchar, [GROUP_NAME] varchar, " +
							"[ASSGN_NAME] varchar, [SHIFT_CODE] integer)", conn);
			try
			{
				insert.ExecuteNonQuery();
				logger.Log( "SCHEDULE.DBF was created.");
			}
			catch( Exception ex ) 
			{
				logger.Error( ex.ToString() );
			}
			finally
			{
				conn.Close();
			}
		}

		private void DeleteDBF()
		{
			if( File.Exists( System.Windows.Forms.Application.StartupPath + "\\SCHEDULE.DBF" ) )
			{
				try
				{
					File.Delete( System.Windows.Forms.Application.StartupPath + "\\SCHEDULE.DBF" );
					logger.Log( "Deleted SCHEDULE.DBF." );
				}
				catch( Exception ex ) 
				{
					logger.Error( ex.ToString() );
				}
			}
			if( File.Exists( _remoteScheduleDBFPath ) )
			{
				try
				{
					File.Delete( _remoteScheduleDBFPath );
					logger.Log( "Deleted SCHEDULE.DBF." );
				}
				catch( Exception ex ) 
				{
					logger.Error( ex.ToString() );
				}
			}
		}

		private void TemplatesTable( ArrayList rows )
		{
			OdbcConnection newConnection = new OdbcConnection( _localCnx );
			newConnection.Open();

			OdbcDataReader reader = null;

			foreach( ForecastShiftList shiftList in rows )
			{
				int rowCnt = 0;			
				if( !shiftList.Applied && shiftList.Shifts.Count > 0 )
				{
					OdbcCommand selectCmd =  new OdbcCommand("SELECT COUNT(*) as ROW_CNT FROM TEMPLATES WHERE TemplateName = ?",newConnection);
					selectCmd.Parameters.Add( "dt", shiftList.Description );		
	
					try
					{
						reader = selectCmd.ExecuteReader();
						while( reader.Read() )
						{
							rowCnt = Convert.ToInt32( reader[ "ROW_CNT" ].ToString() );
						}
					}
					catch( Exception ex ) 
					{
						logger.Error( ex.ToString() );
					}
					finally
					{
						reader.Close();
					}

					if( rowCnt < 1 )
					{
						OdbcCommand insertCmd =  new OdbcCommand("INSERT INTO TEMPLATES (TemplateName,Description) VALUES (?,?)",newConnection);
						insertCmd.Parameters.Add( "dt", shiftList.Description );	
						insertCmd.Parameters.Add( "dt", "Created on " + DateTime.Now.ToShortDateString() );	
	
						try
						{
							if( insertCmd.ExecuteNonQuery() > 0 )
							{
								logger.Debug( "Entered " + shiftList.Description + " into Templates table." );
							} 
							else logger.Debug( "Did NOT enter " + shiftList.Description + " into Templates table." );
						}
						catch( Exception ex ) 
						{
							logger.Error( ex.ToString() );
						}
					}
				}
			}
		}

		private void AssignmentGroups( ArrayList rows )
		{
			OdbcConnection newConnection = new OdbcConnection( _localCnx );
			newConnection.Open();

			OdbcDataReader reader = null;

			foreach( ForecastShiftList shiftList in rows )
			{
				int rowCnt = 0;			
				if( !shiftList.Applied && shiftList.Shifts.Count > 0  )
				{
					OdbcCommand selectCmd =  new OdbcCommand("SELECT COUNT(*) as ROW_CNT FROM AssignmentGroups WHERE GroupName = ? AND TemplateName = ?",newConnection);
					selectCmd.Parameters.Add( "dt", shiftList.ScheduleName );
					selectCmd.Parameters.Add( "dt", shiftList.Description );
	
					try
					{
						reader = selectCmd.ExecuteReader();
						while( reader.Read() )
						{
							rowCnt = Convert.ToInt32( reader[ "ROW_CNT" ] );
						}
					}
					catch( Exception ex ) 
					{
						logger.Error( ex.ToString() );
					}
					finally
					{
						reader.Close();
					}

					if( rowCnt < 1 )
					{
						OdbcCommand insertCmd =  new OdbcCommand("INSERT INTO AssignmentGroups (GroupName,Description,TemplateName)"+
							" VALUES (?,?,?)",newConnection);
						insertCmd.Parameters.Add( "dt", shiftList.ScheduleName );
						insertCmd.Parameters.Add( "dt", shiftList.ScheduleName );
						insertCmd.Parameters.Add( "dt", shiftList.Description );	
	
						try
						{
							if( insertCmd.ExecuteNonQuery() > 0 )
							{
								logger.Debug( "Entered [" + shiftList.ScheduleName + "] " + shiftList.Description + " into AssignmentGroups table." );
							} 
							else logger.Debug( "Did NOT enter [" + shiftList.ScheduleName + "] " + shiftList.Description + " into AssignmentGroups table." );
						}
						catch( Exception ex ) 
						{
							logger.Error( ex.ToString() );
						}
					}
				}
			}
		}

		private void DeleteForecastRows( ArrayList rows )
		{
			OdbcConnection newConnection = new OdbcConnection( _localCnx );
			newConnection.Open();

			int rowCnt = 0;
			foreach( ForecastShiftList shiftList in rows )
			{
				if( !shiftList.Applied && shiftList.Shifts.Count > 0 )
				{
					DateTime endDate = new DateTime( shiftList.WeekStart.Ticks );

					OdbcCommand delCmd =  new OdbcCommand("DELETE FROM AppliedAssignments WHERE GroupName = '"+
						shiftList.ScheduleName+"' and WeekStart = ?",newConnection);
					delCmd.Parameters.Add( "dt", shiftList.WeekStart );	
					delCmd.Parameters.Add( "dt2", new DateTime( shiftList.WeekStart.Ticks ).AddDays( 7 ) );	
	
					logger.Debug("Delete GroupName["+shiftList.ScheduleName+"] WeekStart["+shiftList.WeekStart.ToShortDateString()+"]." );

					try
					{
						int cnt = delCmd.ExecuteNonQuery();
						logger.Debug( "Deleted " + cnt + " rows." );						
						rowCnt += cnt;
					}
					catch( Exception ex ) 
					{
						logger.Error( ex.ToString() );
					}
				}
				logger.Log( "" );
			}
			logger.Log( "Deleted " + rowCnt + " rows from TemplateInfo97.mdb." );
		}

		private void InsertForecastRows( ArrayList rows )
		{
			OdbcConnection newConnection = new OdbcConnection( _localCnx );			

			logger.Debug( "" );
			logger.Debug( "^^^^ BUILDING DATA ROW OBJECTS ^^^^" );
			ArrayList dataRows = GetForecastDataRows( rows );
			logger.Debug( "" );

			int rowCnt = 0;
			int index = 0;

			try
			{
				newConnection.Open();
				logger.Debug( "^^^^ ADDING DATA ROWS TO TEMPLATE INFO ^^^^" );
				foreach( ForecastDataRow dataRow in dataRows )
				{				
					OdbcCommand insertCmd =  new OdbcCommand("INSERT INTO AppliedAssignments (AssignmentName, JobCode, "+
						"GroupName, TemplateName, TimeIn1, TimeOut1, TimeIn2, TimeOut2, TimeIn3, TimeOut3, "+
						"TimeIn4, TimeOut4, TimeIn5, TimeOut5, TimeIn6, TimeOut6, TimeIn7, TimeOut7, WeekStart ) "+
						"VALUES( 'Loc " + index + "', ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ? )",newConnection);
				
					insertCmd.Parameters.Add( "p1", dataRow.JobCode );		
					insertCmd.Parameters.Add( "p2", dataRow.GroupName );	
					insertCmd.Parameters.Add( "p3", dataRow.TempName );	
					insertCmd.Parameters.Add( "p4", dataRow.Wed != null ? dataRow.Wed.InTime.TimeOfDay : new TimeSpan(0) );	
					insertCmd.Parameters.Add( "p5", dataRow.Wed != null ? dataRow.Wed.OutTime.TimeOfDay : new TimeSpan(0) );	
					insertCmd.Parameters.Add( "p4", dataRow.Thurs != null ? dataRow.Thurs.InTime.TimeOfDay : new TimeSpan(0) );	
					insertCmd.Parameters.Add( "p5", dataRow.Thurs != null ? dataRow.Thurs.OutTime.TimeOfDay : new TimeSpan(0) );
					insertCmd.Parameters.Add( "p4", dataRow.Fri != null ? dataRow.Fri.InTime.TimeOfDay : new TimeSpan(0) );	
					insertCmd.Parameters.Add( "p5", dataRow.Fri != null ? dataRow.Fri.OutTime.TimeOfDay : new TimeSpan(0) );
					insertCmd.Parameters.Add( "p4", dataRow.Sat != null ? dataRow.Sat.InTime.TimeOfDay : new TimeSpan(0) );	
					insertCmd.Parameters.Add( "p5", dataRow.Sat != null ? dataRow.Sat.OutTime.TimeOfDay : new TimeSpan(0) );
					insertCmd.Parameters.Add( "p4", dataRow.Sun != null ? dataRow.Sun.InTime.TimeOfDay : new TimeSpan(0) );	
					insertCmd.Parameters.Add( "p5", dataRow.Sun != null ? dataRow.Sun.OutTime.TimeOfDay : new TimeSpan(0) );
					insertCmd.Parameters.Add( "p4", dataRow.Mon != null ? dataRow.Mon.InTime.TimeOfDay : new TimeSpan(0) );	
					insertCmd.Parameters.Add( "p5", dataRow.Mon != null ? dataRow.Mon.OutTime.TimeOfDay : new TimeSpan(0) );
					insertCmd.Parameters.Add( "p4", dataRow.Tues != null ? dataRow.Tues.InTime.TimeOfDay : new TimeSpan(0) );	
					insertCmd.Parameters.Add( "p5", dataRow.Tues != null ? dataRow.Tues.OutTime.TimeOfDay : new TimeSpan(0) );
					insertCmd.Parameters.Add( "p18", dataRow.WeekStart );	

					if( !_applied.ContainsKey( dataRow.SchedId + "|" + dataRow.WeekStart.ToString( "MM/dd/yyyy" ) ) )
					{
						_applied.Add( dataRow.SchedId + "|" + dataRow.WeekStart.ToString( "MM/dd/yyyy" ), "true" );
					}
						
					try
					{
						int cnt = insertCmd.ExecuteNonQuery();
						rowCnt += cnt;
						if( cnt > 0 )
						{
							logger.Debug(
								"INSERTED [Loc "+index+"] " +
								"["+dataRow.JobCode+"] " +
								"["+dataRow.GroupName+"] " +
								"["+dataRow.TempName+"] " +
								"["+(dataRow.Wed != null ? dataRow.Wed.InTime.TimeOfDay : new TimeSpan(0)).ToString()+" - " +
								""+(dataRow.Wed != null ? dataRow.Wed.OutTime.TimeOfDay : new TimeSpan(0)).ToString()+"] " +
								"["+(dataRow.Thurs != null ? dataRow.Thurs.InTime.TimeOfDay : new TimeSpan(0)).ToString()+" - " +
								""+(dataRow.Thurs != null ? dataRow.Thurs.OutTime.TimeOfDay : new TimeSpan(0)).ToString()+"] " +
								"["+(dataRow.Fri != null ? dataRow.Fri.InTime.TimeOfDay : new TimeSpan(0)).ToString()+" - " +
								""+(dataRow.Fri != null ? dataRow.Fri.OutTime.TimeOfDay : new TimeSpan(0)).ToString()+"] " +
								"["+(dataRow.Sat != null ? dataRow.Sat.InTime.TimeOfDay : new TimeSpan(0)).ToString()+" - " +
								""+(dataRow.Sat != null ? dataRow.Sat.OutTime.TimeOfDay : new TimeSpan(0)).ToString()+"] " +
								"["+(dataRow.Sun != null ? dataRow.Sun.InTime.TimeOfDay : new TimeSpan(0)).ToString()+" - " +
								""+(dataRow.Sun != null ? dataRow.Sun.OutTime.TimeOfDay : new TimeSpan(0)).ToString()+"] " +
								"["+(dataRow.Mon != null ? dataRow.Mon.InTime.TimeOfDay : new TimeSpan(0)).ToString()+" - " +
								""+(dataRow.Mon != null ? dataRow.Mon.OutTime.TimeOfDay : new TimeSpan(0)).ToString()+"] " +
								"["+(dataRow.Tues != null ? dataRow.Tues.InTime.TimeOfDay : new TimeSpan(0)).ToString()+" - " +
								""+(dataRow.Tues != null ? dataRow.Tues.OutTime.TimeOfDay : new TimeSpan(0)).ToString()+"] " + 
								"[" + dataRow.WeekStart.ToShortDateString() + "]"
							);
						}
					}
					catch( Exception ex ) 
					{
						logger.Error( ex.ToString() );
					}
					index++;
				}
			}
			finally
			{
				newConnection.Close();
			}
			logger.Log( "Inserted " + rowCnt + " rows into TemplateInfo97.mdb." );
		}

		private ArrayList GetForecastDataRows( ArrayList shiftLists )
		{
			ArrayList lists = new ArrayList();
			ForecastDataRow dataRow = null;
			int rowCnt = 0;
			logger.Debug( "" );
			foreach( ForecastShiftList shiftList in shiftLists )
			{	
				ArrayList list = new ArrayList();
				logger.Debug( "There are " + shiftList.Shifts.Count + " shifts in the " + shiftList.ScheduleName + " schedule." );
				foreach( ForecastShift shift in shiftList.Shifts )
				{
					//logger.Debug( "		" + shift.ToPrintString() );
					//foreach( ForecastDataRow row in list )
					//{
						//logger.Debug( "		" + row.toPrintString() );
					//}
				
					//logger.Debug( "		" + shift.ToPrintString() );
					//logger.Debug( "		Row " + rowCnt );
					if( rowCnt == 0 )
					{
						dataRow = new ForecastDataRow();
						dataRow.SchedId = shiftList.SchedId;
						dataRow.TempId = shiftList.TempId;
						dataRow.TempName = shiftList.Description;
						dataRow.WeekStart = shiftList.WeekStart;
						dataRow.JobCode = shift.JobId;
						dataRow.GroupName = shiftList.ScheduleName;
						dataRow.AddShift( shift );
						list.Add( dataRow );
						rowCnt++;
						//logger.Debug( "		Created the initial row." );
					}
					else
					{
						//logger.Debug( "		looping through rows....." );
						int index = 0;
						foreach( ForecastDataRow row in list )
						{
							index++;
							if( row.AddShift( shift ) )
							{
								//logger.Debug( "		" + index + " added ...." );
								break;
							}
							else if( !row.AddShift( shift ) && index == list.Count )
							{
								dataRow = new ForecastDataRow();
								dataRow.SchedId = shiftList.SchedId;
								dataRow.TempId = shiftList.TempId;
								dataRow.TempName = shiftList.Description;
								dataRow.WeekStart = shiftList.WeekStart;
								dataRow.JobCode = shift.JobId;
								dataRow.GroupName = shiftList.ScheduleName;
								dataRow.AddShift( shift );
								dataRow.Add = true;
								//logger.Debug( "		" + index + " created another row ...." );
								rowCnt++;
								break;
							}
						}
					}
					if( dataRow.Add )
					{
						dataRow.Add = false;
						list.Add( dataRow );
						//logger.Debug( "		added row ...." );
					}
				}
				rowCnt = 0;
				lists.Add( list );
			}

			ArrayList masterList = new ArrayList();
			foreach( ArrayList list in lists )
			{
				foreach( ForecastDataRow row in list )
				{
					masterList.Add( row );
				}
			}			

			return masterList;
		}
		 
		private void AppliedTemplates()
		{
			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement( "applied-templates" );

			foreach( String key in _applied.Keys )
			{
				XmlElement tempEle = doc.CreateElement( "template" );
                
				String idStr = key.Substring( 0, key.IndexOf( "|" ) );
				String dateStr = key.Substring( key.IndexOf( "|" )+1 , (key.Length - idStr.Length)-1 );
				
				tempEle.SetAttribute( "schedule-id" , idStr+"" );

				XmlElement dateEle = doc.CreateElement( "start-date" );
				dateEle.SetAttribute( "day" , Convert.ToInt32( dateStr.Substring( 3 , 2 ) )+"" );
				dateEle.SetAttribute( "month" , Convert.ToInt32( dateStr.Substring( 0 , 2 ) )+"" );
				dateEle.SetAttribute( "year" , Convert.ToInt32( dateStr.Substring( 6 , 4 ) )+"" );

                tempEle.AppendChild( dateEle );
				root.AppendChild( tempEle );
			}

			ScheduleWss schedService = new ScheduleWss();
			int rows = schedService.applyForecast( this.Details.ClientId , root.OuterXml );

			logger.Log( "Updated " + rows + " rows for applied teplates." );
		}

		private static String getDynamicIP()
		{
			String dynamicIP = "";
			String[] split = new String[4];
			try
			{
				String strHostName = Dns.GetHostName();
				IPHostEntry ipEntry = Dns.GetHostByName (strHostName);
				IPAddress [] addr = ipEntry.AddressList;
				char[] splitter = {'.'};
				split = addr[0].ToString().Split(splitter, 4);
				dynamicIP = split[0] + "." + split[1] + "." + split[2] + "." + "22";
			}   
			catch(Exception ex)
			{
				Console.WriteLine("Getting the IP did not work");
				Console.WriteLine(ex.ToString());
			}
			return dynamicIP;
		}
	}
}
