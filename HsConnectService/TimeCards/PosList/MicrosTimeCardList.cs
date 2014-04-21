using HsConnect.Data;
using HsConnect.SalesItems;

using System;
using System.Data;
using Microsoft.Data.Odbc;

namespace HsConnect.TimeCards.PosList
{
	public class MicrosTimeCardList : TimeCardListImpl
	{
		public MicrosTimeCardList() {}

		private HsData data = new HsData();

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
			OdbcConnection newConnection = this.cnx.GetOdbc();
			try
			{
                bool thirtyDays = this.Details.Preferences.PrefExists(9999);
                if (thirtyDays) this.EndDate = this.EndDate.AddDays(-30);
				DataSet dataSet = new DataSet();
				OdbcDataAdapter dataAdapter = new OdbcDataAdapter(
							"select BUSINESS_DATE , EMP_SEQ , TIME_CARD_SEQ , JOB_NAME , job_number ," +
							"clock_in_datetime,clock_out_datetime,overtime_ttl,overtime_hours,regular_ttl,regular_hours, " +
							"adjust_reason as ADJUST_REASON from micros.v_R_employee_time_card where BUSINESS_DATE > ? ", newConnection );
				dataAdapter.SelectCommand.Parameters.Add( "" , this.EndDate );
				dataAdapter.Fill( dataSet , "micros.v_R_employee_time_card" );
                try//not sure if this is valid debug
                {
                    logger.Debug("For the query:  " + dataAdapter.SelectCommand.CommandText);
                }
                catch (Exception ex)
                {
                    logger.Error("that debug statement didnt work...");
                }
				dataAdapter.Dispose();
				DataRowCollection rows = dataSet.Tables[0].Rows;
                
                logger.Debug("returned [" + dataSet.Tables[0].Rows.Count + "] rows from the database"); 
				foreach( DataRow row in rows )
				{
					try
					{
                        logger.Debug("parsing time card");
                        
						TimeCard timeCard = new TimeCard();

                        //build tcExtId as follows:
                        //123456789hhmmMMddyy
                        //(empid_timein_month_day_year)
                        string tmpTcExtId = "";
                        tmpTcExtId += data.GetInt(row, "EMP_SEQ");
                        tmpTcExtId += data.GetInt(row, "TIME_CARD_SEQ");
                        tmpTcExtId += (data.GetDate( row, "clock_in_datetime")).ToString("HmmMMddyy");

                        //tc.ExtId = data.GetInt( row , SEQ_NUM );
                        //timeCard.ExtId = data.GetInt(row, "TIME_CARD_SEQ");
                        timeCard.ExtId = long.Parse(tmpTcExtId);

						timeCard.EmpPosId = data.GetInt( row , "EMP_SEQ" );
						timeCard.JobName = data.GetString( row , "JOB_NAME" );
						timeCard.JobExtId = data.GetInt( row , "job_number" );
						timeCard.RegHours = data.GetFloat( row , "regular_hours" );
						timeCard.RegTotal = data.GetFloat( row , "regular_ttl" );
						timeCard.OvtHours = data.GetFloat( row , "overtime_hours" );
						timeCard.OvtTotal = data.GetFloat( row , "overtime_ttl" );
						timeCard.BusinessDate = data.GetDate( row , "BUSINESS_DATE" );
						timeCard.ClockIn = data.GetDate( row , "clock_in_datetime" );
						timeCard.ClockOut = data.GetDate( row , "clock_out_datetime" ) > new DateTime(0) ? data.GetDate( row , "clock_out_datetime" ) : DateTime.Now;
						timeCard.OvertimeMinutes = (int) Math.Round( data.GetDouble( row , "overtime_hours" ) * 60 , 0 );
						this.Add( timeCard );
                        logger.Debug("added time card:  " + timeCard.ClockIn.ToString());
					}
					catch( Exception ex )
					{
						logger.Error( "Error adding micros time card in Load(): " + ex.ToString() );
					}
				}
			}
			catch( Exception ex )
			{
                logger.Error("Error retrieving MICROS timecards in Load(): ");
				logger.Error( ex.ToString() );
				Main.Run.errorList.Add(ex);
			}
			finally
			{
				newConnection.Close();
			}
            logger.Debug("is timecardList NULL?  " + (timeCardList == null));
			this.SetWages();
		}
		public override void DbUpdate(){}
		public override void DbInsert(){}
	}
}
