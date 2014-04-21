using HsConnect.Data;
using HsConnect.Forms;
using HsConnect.SalesItems;
using HsSharedObjects.Client.Preferences;
using HsSharedObjects.Client.CustomModule;

using System;
using System.IO;
using System.Data;
using System.Collections;
using HsSharedObjects.Client.Shift;
using Microsoft.Data.Odbc;
using ConvertDBF;
using pslbr;

namespace HsConnect.TimeCards.PosList
{
	public class PosiTimeCardList : TimeCardListImpl
	{
		public PosiTimeCardList()
		{
		}

		private HsData data = new HsData();
		private Hashtable jobs = new Hashtable();
		public DateTime lastWeek = new DateTime(0);
		public DateTime CurrWeek = new DateTime(0);
		public DateTime nextWeek = new DateTime(0);
		private static String drive =  Data.PosiControl.Drive;
	    private static ClientShift _midnightClientShift;
        private bool altFlag = false;
        private Hashtable empMapping = null;

        private void SetPreferences()
        {
            if (this.Details.Preferences.PrefExists(Preference.USE_ALTID_NOT_EXTID))
            {
                altFlag = true;
                empMapping = PosiControl.MapEmployees(false);
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

	    public override bool PeriodLabor
		{
			get
			{
				return this.periodLabor;
			}
			set
			{
				this.periodLabor = value;
			}
		}


		public override void DbLoad()
		{
            SetPreferences();
			/** when manually called, this does the current week
			 *	upon auto sync, it does the current week and the 2 previous
			 */
			logger.Debug( "Loading Time Card list" );
			this.GetWeekDates();
            Hashtable waivers = null;
            if (this.Details.Preferences.PrefExists(HsSharedObjects.Client.Preferences.Preference.PSLBR_WAIVERS))
            {
                ClientEmployeeWaiverList waiverList = new ClientEmployeeWaiverList(this.Details.ClientId);
                waivers = waiverList.EmpWaiverMap;
            }
            else
            {
                waivers = new Hashtable();
            }
            
			//DropCards = 1;
			DataTableBuilder builder = new DataTableBuilder();
			// load current job list
			DataTable dt = builder.GetTableFromDBF(drive + @":\ALTDBF", @"C:\", "JOBLIST" );
			//DataTable dt = builder.GetTableFromDBF( @"L:\SC", @"C:\", "JOBLIST" );
			DataRowCollection rows = dt.Rows;
			foreach( DataRow row in rows )
			{
				try
				{
					String altCode = data.GetInt( row , "ALT_CODE" ).ToString();
					String deptCode = data.GetInt( row , "DEPT_CODE" ).ToString();
					if( !jobs.ContainsKey( altCode + " | " + deptCode ) ) jobs.Add( altCode + " | " + deptCode , data.GetString( row , "JOB_CODE" ) );
					logger.Debug("Job Codes From Hash:  " + altCode + " | " + deptCode + " | " + data.GetString( row , "JOB_CODE" ));
				}
				catch( Exception ex )
				{
					logger.Error( "Error parsing JOBLIST row: " + row, ex);
				}
			}

            if (PeriodLabor)
            {
                try
                {
                    LoadPeriodLabor(waivers);
                }
                catch (Exception ex)
                {
                    logger.Error("Error loading period labor", ex);
                }
            }
            else
            {
                LoadNormalLabor(waivers);
            }
		    logger.Debug( "XML:  " + this.GetXmlString() );
			this.SetWages();		
		}

	    private void LoadPeriodLabor(Hashtable waivers)
	    {
	        DateTime date = this.CurrWeek.AddDays( 0 );
	        for( int i=0; i<2; i++ )
	        {
                logger.Debug("Attempting to run ");
	            PosiControl.Run( "TARW","-R 21 2 " + i.ToString() );
	            logger.Debug( "looking for PR" + date.AddDays( 6.0 ).ToString( "MMddyy" ) + ".prn" );
	            logger.Debug( "OVT Rule: " + this.details.OvertimeRule );
	            ArrayList timeCards = PrnImport.GetTimeCards( this.details, date.AddDays( 6.0 ), drive + @":\OUTFILES\" , waivers);
	            //ArrayList timeCards = PrnImport.GetTimeCards( this.details.OvertimeRule, date.AddDays( 6.0 ), @"L:\OUTFILES\" );
	            if(timeCards!=null)
                    logger.Debug("Found " + timeCards.Count + " timecards in PRN");
	            else
	                logger.Debug("timeCards is null");
	            foreach( EmployeeTimeCard prnCard in timeCards )
	            {
	                AddPeriodTimecard(i, prnCard);
	            }
	            if( this.Details.CustomModuleList.IsActive( ClientCustomModule.BJS_TIMECARD_IMPORT ) )
                {
                    try
                    {
                        if (!Directory.Exists(drive + @":\hstmp"))
                        {
                            Directory.CreateDirectory(drive + @":\hstmp");
                        }
                        if (!Directory.Exists(drive + @":\hstmp\BACKUP"))
                        {
                            Directory.CreateDirectory(drive + @":\hstmp\BACKUP");
                        }
                        if (!Directory.Exists(drive + @":\hstmp\BACKUP\" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day))
                        {
                            Directory.CreateDirectory(drive + @":\hstmp\BACKUP\" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day);
                        }
                        File.Copy(drive + @":\OUTFILES\PR" + date.AddDays(6.0).ToString("MMddyy") + ".prn", drive + @":\hstmp\BACKUP\" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + @"\PR" + date.AddDays(6.0).ToString("MMddyy") + ".prn", true);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("error backing up file.", ex);
                    }
	            }
	            date = date.AddDays( -7.0 );
	        }
	    }

	    private void LoadNormalLabor(Hashtable waivers)
	    {
	        PosiControl.Run( "TARW", "-R 21 2 0" );
	        logger.Debug( "looking for PR" + this.CurrWeek.AddDays( 6.0 ).ToString( "MMddyy" ) + ".prn" );
	        logger.Debug( "OVT RULE: " + this.details.OvertimeRule );
	        ArrayList timeCards = PrnImport.GetTimeCards( this.details, this.CurrWeek.AddDays( 6.0 ), drive + @":\OUTFILES\" , waivers);
	        //ArrayList timeCards = PrnImport.GetTimeCards( this.details.OvertimeRule, this.currWeek.AddDays( 6.0 ), @"L:\OUTFILES\" );
	        logger.Debug( "is timeCards NULL: " + (timeCards == null).ToString() );
	        foreach( EmployeeTimeCard prnCard in timeCards )
	        {
	            AddNormalTimecard(prnCard);
	        }
	    }

	    private void AddNormalTimecard(EmployeeTimeCard prnCard)
	    {
	        TimeCard timeCard = new TimeCard();
	        timeCard.EmpPosId = Convert.ToInt32(GetEmpNumber(""+prnCard.EmpId));
					
	        int jobCode = Convert.ToInt32( (String) jobs[ prnCard.AltCode.ToString() + " | " + prnCard.JobDept.ToString() ] );
	        if( this.Details.Preferences.PrefExists( Preference.POSI_ALT_JOB ) ) jobCode = prnCard.AltCode;

					
	        timeCard.JobExtId = jobCode;
	        timeCard.RegHours = prnCard.RegHours;
	        timeCard.RegTotal = prnCard.RegDollars;
	        timeCard.OvtHours = prnCard.OtHours;
	        timeCard.OvtTotal = prnCard.OtDollars;
	        timeCard.SpcHours = prnCard.SpcHours;
	        timeCard.SpcTotal = prnCard.SpcDollars;
	        timeCard.BusinessDate = prnCard.ClockIn.Date;
	        timeCard.ClockIn = prnCard.ClockIn;
	        timeCard.ClockOut = prnCard.ClockOut;

            // Check if business date needs to be corrected
            TimeSpan clockIn = new TimeSpan(timeCard.ClockIn.Hour, timeCard.ClockIn.Minute, 0);
	        ClientShiftList cShiftList = Details.ShiftList;
            logger.Debug("Clock In: " + clockIn);
            TimeSpan midnight = new TimeSpan();
            ClientShift midnightShift = GetMidnightClientShift(midnight, cShiftList);
            logger.Debug("Midnight Shift: " + midnightShift.StartTime + " - " + midnightShift.EndTime);
	        TimeSpan midnightShiftEnd = midnightShift.EndTime;
            if (clockIn.TotalMinutes <= midnightShiftEnd.TotalMinutes)  // If timecard is in portion of client shift after midnight
            {
                logger.Debug("Adjusting incorrect timecard business date: " + timeCard.BusinessDate.ToShortDateString());
                timeCard.BusinessDate = new DateTime(timeCard.ClockIn.Year, timeCard.ClockIn.Month,
                                                     timeCard.ClockIn.Day - 1); // Set business date to previous day
                logger.Debug("\tadjusted to: " + timeCard.BusinessDate.ToShortDateString());
            }

	        timeCard.OvertimeMinutes = (int) Math.Round( prnCard.OtHours * 60 , 0 );
	        timeCard.PayType = prnCard.PayType;
	        int added = this.Add( timeCard );
	    }

	    private ClientShift GetMidnightClientShift(TimeSpan midnight, ClientShiftList cShiftList)
	    {
            if(_midnightClientShift==null)
                _midnightClientShift = GetClientShiftContainingTime(midnight, cShiftList);
	        return _midnightClientShift;
	    }

	    private ClientShift GetClientShiftContainingTime(TimeSpan time, ClientShiftList clientShifts)
	    {
	        logger.Debug("Finding client shift containing " + time + " within " + clientShifts.Count +
	                     " total client shifts");
            ClientShift result = null;
	        try
	        {
                if(time.Equals(new TimeSpan()))
                    time = new TimeSpan(1, 0, 0, 0);

	            foreach (ClientShift cShift in clientShifts)
	            {
	                TimeSpan startTime = cShift.StartTime;
	                TimeSpan endTime = cShift.EndTime;
                    if (endTime.CompareTo(startTime) <= 0)
                        endTime = endTime.Add(new TimeSpan(1, 0, 0, 0));
                    logger.Debug("Shift: " + startTime + " - " + endTime);
	                if (startTime.CompareTo(time) <= 0 && endTime.CompareTo(time) >= 0)
	                {
	                    result = cShift;
	                    break;
	                }
	            }
	            if (result == null)
	                logger.Error("Error finding client shift containing " + time);
	            else
	                logger.Debug("Found shift: " + result.StartTime + " - " + result.EndTime);
	        }
	        catch (Exception e)
	        {
                logger.Error("Error finding client shift containing " + time);
            }
            return result;
	    }

	    private void AddPeriodTimecard(int i, EmployeeTimeCard prnCard)
	    {
	        try
	        {
	            TimeCard timeCard = new TimeCard();
                timeCard.EmpPosId = Convert.ToInt32(GetEmpNumber("" + prnCard.EmpId));
	            int jobCode = Convert.ToInt32( (String) jobs[ prnCard.AltCode.ToString() + " | " + prnCard.JobDept.ToString() ] );
	            logger.Debug("PRNFILE DEBUG:  USING KEY - "+prnCard.AltCode + " | " +prnCard.JobDept + " - Got Job Code:  " + jobCode);
	            logger.Debug("IS STUFF NULL:  " + prnCard.RegHours + " , " + prnCard.OtHours );
	            if( this.Details.Preferences.PrefExists( Preference.POSI_ALT_JOB ) ) jobCode = prnCard.AltCode;
	            if( this.Details.Preferences.PrefExists( 1012 ) ) 
	            {
	                jobCode = prnCard.AltCode;//POSI_USE_REG_JOB
	            }
	            timeCard.JobExtId = jobCode;
	            timeCard.RegHours = prnCard.RegHours;
	            timeCard.RegTotal = prnCard.RegDollars;
	            timeCard.OvtHours = prnCard.OtHours;
	            timeCard.OvtTotal = prnCard.OtDollars;
	            timeCard.SpcHours = prnCard.SpcHours;
	            timeCard.SpcTotal = prnCard.SpcDollars;
	            timeCard.BusinessDate = prnCard.ClockIn.Date;
	            timeCard.ClockIn = prnCard.ClockIn;
                timeCard.ClockOut = prnCard.ClockOut;

                // Check if business date needs to be corrected
                TimeSpan clockIn = new TimeSpan(timeCard.ClockIn.Hour, timeCard.ClockIn.Minute, 0);
                ClientShiftList cShiftList = Details.ShiftList;
                logger.Debug("Clock In: " + clockIn);
                TimeSpan midnight = new TimeSpan();
                ClientShift midnightShift = GetClientShiftContainingTime(midnight, cShiftList);
                logger.Debug("Midnight Shift: " + midnightShift.StartTime + " - " + midnightShift.EndTime);
                TimeSpan midnightShiftEnd = midnightShift.EndTime;
                if (clockIn.TotalMinutes <= midnightShiftEnd.TotalMinutes)  // If timecard is in portion of client shift after midnight
                {
                    logger.Debug("Adjusting incorrect timecard business date: " + timeCard.BusinessDate.ToShortDateString());
                    timeCard.BusinessDate = new DateTime(timeCard.ClockIn.Year, timeCard.ClockIn.Month,
                                                         timeCard.ClockIn.Day - 1); // Set business date to previous day
                    logger.Debug("\tadjusted to: " + timeCard.BusinessDate.ToShortDateString());
                }

	            timeCard.OvertimeMinutes = (int) Math.Round( prnCard.OtHours * 60 , 0 );
	            timeCard.Adjusted = prnCard.Adjusted;
	            timeCard.PayType = prnCard.PayType;
	            int added = this.Add( timeCard );	
	        }
	        catch( Exception ex )
	        {
	            logger.Error( "Error in ["+i+"]: " + ex.ToString() );
	        }
	    }

	    private void GetWeekDates()
		{
			DataTableBuilder builder = new DataTableBuilder();
			DataTable tbl = builder.GetTableFromDBF(drive + @":\ALTDBF", @"C:", "SCHSETUP" );
			//DataTable tbl = builder.GetTableFromDBF( @"L:\SC", @"C:", "SCHSETUP" );
			DataRowCollection rows = tbl.Rows;
			foreach( DataRow row in rows )
			{
				try
				{
					this.lastWeek = GetDate( data.GetString( row , "LAST_WEEK" ) );
					this.CurrWeek = GetDate( data.GetString( row , "CURR_WEEK" ) );
					this.nextWeek = GetDate( data.GetString( row , "NEXT_WEEK" ) );
				}
				catch( Exception ex )
				{
				    logger.Error("Error getting week dates from SCHSETUP", ex);
					Main.Run.errorList.Add(ex);
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

		public override void DbUpdate(){}
		public override void DbInsert(){}
	}
}
