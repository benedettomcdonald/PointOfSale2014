using HsConnect.Data;
using HsConnect.SalesItems;
using HsConnect.EmpJobs.PosList;

using System;
using System.Data;
using Microsoft.Data.Odbc;
using System.Collections;

namespace HsConnect.TimeCards.PosList
{
	public class Micros9700TimeCardList : TimeCardListImpl
	{
		public Micros9700TimeCardList() {}

		private HsData data = new HsData();

		//STATICS
		/*These statics correspond to the rows in the time_card dataTable, and need
		 * to be updated if the dataTable or query for timecards is ever changed 
		 */ 
		private static String EMPL = "Row0";
		private static String SEQ_NUM = "Row1";
		private static String RATE = "Row2";
		private static String RVC = "Row3";
		private static String CLOCK_IN = "Row4";
		private static String CLOCK_OUT = "Row5";
		private static String REG_SECS = "Row6";
		private static String OVT_SECS = "Row7";



		/*PeriodLabor procedure*/
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

		/*public void DbLoad
		 * This method is called by the labor sync.  It grabs the time card data, as well as
		 * the employee job data, from the database.  Using this info, it creates and populates
		 * all timecards within the given timeframe (default is yesterday) 
		 */
		public override void DbLoad()
		{
			//we need to get the employee job files
			Micros9700Control.Run( Micros9700Control.EMPLOYEE_JOB_SYNC );
			DataTableBuilder builder = new DataTableBuilder();
			Micros9700Control.CombineFiles(Micros9700Control.JOBS);
			DataTable empJobsDt = builder.GetTableFromCSV( Micros9700Control.PATH_NAME,  Micros9700Control.JOB_FILE );
			DataRowCollection jobRows = empJobsDt.Rows;
			
			//then we need the actual time card files
			Micros9700Control.Run( Micros9700Control.TIME_CARD_SYNC );
			DataTable timeDt = builder.GetTableFromCSV( Micros9700Control.PATH_NAME , Micros9700Control.TIME_FILE );
            DataRowCollection timeRows = timeDt.Rows;
			int ct = 0;
			foreach( DataRow row in timeRows )
			{
				try
				{
					//things that can be retrieved from the time_cards table
					Console.WriteLine( ct++ );
					TimeCard tc = new TimeCard();
					String ind = row[CLOCK_IN].ToString();
					String outd = row[CLOCK_OUT].ToString();
					DateTime inDate = DateTime.Parse(ind);
					DateTime outDate;
					try
					{
						outDate = DateTime.Parse(outd);
					}catch(Exception dateEx)
					{
						logger.Log( "date is null, they never clocked out:  " + dateEx );
                        //outDate = new DateTime(1,1,1);
                        continue;
					}
					tc.EmpPosId = data.GetInt( row , EMPL );
					tc.BusinessDate = inDate.Date;
					tc.ClockIn = inDate;
					tc.ClockOut = outDate;
					int regSeconds = data.GetInt( row , REG_SECS );
					int ovtSeconds = data.GetInt( row , OVT_SECS );
					tc.OvtHours = ovtSeconds / 3600;
					tc.OvertimeMinutes = ovtSeconds / 60;
					tc.RegHours = regSeconds / 3600;
					
					//now we have to find the job info for this time card
					int rate = data.GetInt( row , RATE );
					String eval = Micros9700EmpJobList.EMP_NUMBER + " = '" + tc.EmpPosId + "' AND " + Micros9700EmpJobList.RATE_NUM + " = '" + rate + "'";
					DataRow[] jobs = empJobsDt.Select( eval );
                    DataRow job = jobs[0];
					tc.JobExtId = data.GetInt( job , Micros9700EmpJobList.JOB_CODE );
					tc.JobName = data.GetString ( job , Micros9700EmpJobList.JOB_NAME );
					tc.RegWage = data.GetFloat( job , Micros9700EmpJobList.REG_RATE );
					tc.OvtWage = data.GetFloat( job , Micros9700EmpJobList.OVT_RATE );

					tc.RegTotal = tc.RegHours * tc.RegWage;
					tc.OvtTotal = tc.OvtHours * tc.OvtWage;

                    //build tcExtId as follows:
                    //123456789hhmmMMddyy
                    //(empid_timein_month_day_year)
                    string tmpTcExtId = "";
                    tmpTcExtId += data.GetInt(row, EMPL);
                    tmpTcExtId += inDate.ToString("HmmMMddyy");
                    
					//tc.ExtId = data.GetInt( row , SEQ_NUM );
                    tc.ExtId = long.Parse(tmpTcExtId);

					this.Add(tc);
				}
				catch(Exception ex)
				{
					logger.Error("Error in Time Card Creation:  " + ex);
										Console.WriteLine("Error in Time Card Creation:  " + ex);
				}
			}
		}
		#region unused methods
		/*public float DbUpdate
		 * 
		 * Currently a placeholder with a console.writeline
		 * 
		 */
		public override void DbUpdate()
		{
			Console.WriteLine("This is the 9700 DbUpdate method for TimeCardList");
		}

		/*public float DbInsert
		 * 
		 * Currently a placeholder with a console.writeline
		 * 
		 */
		public override void DbInsert()
		{
			Console.WriteLine("This is the 9700 DbInsert method for TimeCardList");
		}
		#endregion
	}
}
