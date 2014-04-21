using HsConnect.Data;
using HsConnect.SalesItems;
using HsSharedObjects.Client.Preferences;

using System;
using System.Collections;
using System.Data;
using Microsoft.Data.Odbc;

namespace HsConnect.TimeCards.PosList
{
	public class RestMgrTimeCardList : TimeCardListImpl
	{
		public RestMgrTimeCardList() {}

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

		/**
		 * This method is called by a labor sync.  The Restaurant Manager time card data
		 * is stored in a file called loginxx.dbf, where the xx is the last 2 digits of the
		 * current year (e.g. login07.dbf).  The code gets the correct year(s) and opens
		 * the corresponding login.dbf for each.  It then grabs the relevant info
		 * for the last 14 days and creates time cards for each row returned. 
		 * It copies the file(s) into the hsTemp directory before accessing it.
		 */ 
		public override void DbLoad()
		{
			try
			{
				ArrayList years = GetYears();
				foreach(DateTime year in years)
				{
					HsFile hsFile = new HsFile();
					String dt = year.ToString("yy");
					DateTime today = DateTime.Now;
					DataSet dataSet = new DataSet();
					logger.Debug("DSN Path: " + this.Details.Dsn);
					hsFile.Copy( this.Details.Dsn, this.Details.Dsn + @"\hstemp", "LOGIN"+dt+".DBF" );
					String empCnxStr = this.Cnx.ConnectionString + @"\hstemp";
					OdbcConnection newConnection = this.cnx.GetCustomOdbc( empCnxStr );
					OdbcCommand selectCmd =  new OdbcCommand("SELECT EMP_NO, IN_DATE, IN_TIME, OUT_DATE, "
						+"OUT_TIME, JCLASS, HRLY_RATE, OT_HOURS, OT_RATE, OT_FIXRATE, ACCT_POST, BREAKHOURS, "
						+"JOB_DESC FROM LOGIN"+dt+", JOBCLASS WHERE JCLASS = JOB_NO AND IN_DATE > ?",newConnection);
					selectCmd.Parameters.Add( "dt", today.AddDays(-14) );
					OdbcDataAdapter dataAdapter = new OdbcDataAdapter(selectCmd);
					dataAdapter.Fill(dataSet, "LOGIN"+dt);
					dataAdapter.Dispose();

					DataRowCollection rows = dataSet.Tables[0].Rows;
					foreach(DataRow row in rows)
					{
						try
						{
							DateTime tDate = data.GetDate( row , "IN_DATE" );
							TimeCard timeCard = new TimeCard();
							timeCard.BusinessDate = tDate;
							timeCard.EmpPosId = data.GetInt( row , "EMP_NO" );
							timeCard.JobExtId = data.GetInt( row , "JCLASS" );
							timeCard.JobName = data.GetString( row , "JOB_DESC" );
							timeCard.OvtHours = data.GetFloat( row , "OT_HOURS" );
							timeCard.OvertimeMinutes = (int)(timeCard.OvtHours * 60f);
							timeCard.RegWage = data.GetFloat( row , "HRLY_RATE" );
							float ovtFix = data.GetFloat( row , "OT_FIXRATE" );
							float ovtRate;
							if( ovtFix == 0)
								ovtRate = (timeCard.RegWage * 1.5f);
							else
								ovtRate = ovtFix;
							timeCard.OvtWage = ovtRate;
							String inTime = data.GetString( row , "IN_TIME" );
							String outTime = data.GetString( row , "OUT_TIME" );
							if(outTime.Length < 1)//if they haven't logged out, make right now their logout time
								outTime = today.ToString( "HH:mm:ss" );
							DateTime inDate = data.GetDate( row , "IN_DATE" );
							DateTime outDate = data.GetDate( row , "OUT_DATE" );
							if(outDate.Year == 1)
								outDate = today;//if they haven't logged out, make today their logout date
							DateTime it = new DateTime(inDate.Year, inDate.Month, inDate.Day, 
								Int32.Parse(inTime.Substring(0, 2)), Int32.Parse(inTime.Substring(3, 2)), 
								Int32.Parse(inTime.Substring(6, 2)));
							DateTime ot = new DateTime(outDate.Year, outDate.Month, outDate.Day, 
								Int32.Parse(outTime.Substring(0, 2)), Int32.Parse(outTime.Substring(3, 2)), 
								Int32.Parse(outTime.Substring(6, 2)));
							TimeSpan span = ot.Subtract ( it );
							timeCard.RegHours  = (float)span.TotalHours - data.GetFloat( row , "BREAKHOURS" );
							timeCard.ExtId = long.Parse(timeCard.EmpPosId + inDate.ToString("ddMMyy") 
								+ inTime.Substring(0, 2) + inTime.Substring( 3, 2));
							timeCard.RegTotal =  timeCard.RegWage * timeCard.RegHours;
							timeCard.ClockIn = it;
							timeCard.ClockOut = ot;
							this.Add(timeCard);
						}
						catch(Exception ex)
						{
							logger.Error(ex.ToString() + " : " + ex.StackTrace);
						}
					}
				}
			}
			catch(Exception ex)
			{
				logger.Error( ex.ToString() + " : " + ex.StackTrace );
			}
			#region Dish Hack Replaces whole method
			/*try
			{
				ArrayList years = GetYears();
				Hashtable jobs = getJobs();
				foreach(DateTime year in years)
				{
					HsFile hsFile = new HsFile();
					String dt = year.ToString("yy");
					DateTime today = DateTime.Now;
					DataSet dataSet = new DataSet();
					hsFile.Copy( this.Cnx.Dsn, this.Cnx.Dsn + @"\hstemp", "LOGIN"+dt+".DBF" );
					String empCnxStr = this.Cnx.ConnectionString + @"\hstemp";
					OdbcConnection newConnection = this.cnx.GetCustomOdbc( empCnxStr );
					//OdbcCommand selectCmd =  new OdbcCommand("SELECT EMP_NO, IN_DATE, IN_TIME, OUT_DATE, "
//						+"OUT_TIME, JCLASS, HRLY_RATE, OT_HOURS, OT_RATE, OT_FIXRATE, ACCT_POST, BREAKHOURS FROM LOGIN"+dt,newConnection);
					OdbcCommand selectCmd =  new OdbcCommand("SELECT EMP_NO, IN_DATE, IN_TIME, OUT_DATE, "
						+"OUT_TIME, JCLASS, HRLY_RATE, OT_HOURS, OT_RATE, OT_FIXRATE, ACCT_POST, BREAKHOURS "
						+"FROM LOGIN"+dt+"",newConnection);
					
					//selectCmd.Parameters.Add( "dt", today.AddDays(-14) );
					//Rest Mgr. Dish Hack
					for(int x = 0; x<2; x++)
					{
						OdbcDataAdapter dataAdapter = new OdbcDataAdapter(selectCmd);
						int start=0;
						int range=0;
						if(x==0)
						{
							start = 0;
							range = 206;
						}
						else
						{
							start = 208;
							range = 200000;
						}
						dataAdapter.Fill(dataSet, start, range, "LOGIN"+dt);
						dataAdapter.Dispose();

						DataRowCollection rows = dataSet.Tables[0].Rows;
						foreach(DataRow row in rows)
						{
							try
							{
								DateTime tDate = data.GetDate( row , "IN_DATE" );
								TimeCard timeCard = new TimeCard();
								timeCard.BusinessDate = tDate;
								timeCard.EmpPosId = data.GetInt( row , "EMP_NO" );
								timeCard.JobExtId = data.GetInt( row , "JCLASS" );
								//timeCard.JobName = data.GetString( row , "JOB_DESC" );
								timeCard.JobName = (string)jobs[timeCard.JobExtId];
								timeCard.OvtHours = data.GetFloat( row , "OT_HOURS" );
								timeCard.RegWage = data.GetFloat( row , "HRLY_RATE" );
								float ovtFix = data.GetFloat( row , "OT_FIXRATE" );
								float ovtRate;
								if( ovtFix == 0)
									ovtRate = (timeCard.RegWage * 1.5f);
								else
									ovtRate = ovtFix;
								timeCard.OvtWage = ovtRate;
								String inTime = data.GetString( row , "IN_TIME" );
								String outTime = data.GetString( row , "OUT_TIME" );
								if(outTime.Length < 1)//if they haven't logged out, make right now their logout time
									outTime = today.ToString( "HH:mm:ss" );
								DateTime inDate = data.GetDate( row , "IN_DATE" );
								DateTime outDate = data.GetDate( row , "OUT_DATE" );
								if(outDate.Year == 1)
									outDate = today;//if they haven't logged out, make today their logout date
								DateTime it = new DateTime(inDate.Year, inDate.Month, inDate.Day, 
									Int32.Parse(inTime.Substring(0, 2)), Int32.Parse(inTime.Substring(3, 2)), 
									Int32.Parse(inTime.Substring(6, 2)));
								if(it.Date.CompareTo(today.AddDays(-14).Date) <0)
									continue;
								DateTime ot = new DateTime(outDate.Year, outDate.Month, outDate.Day, 
									Int32.Parse(outTime.Substring(0, 2)), Int32.Parse(outTime.Substring(3, 2)), 
									Int32.Parse(outTime.Substring(6, 2)));
								TimeSpan span = ot.Subtract ( it );
								timeCard.RegHours  = (float)span.TotalHours - data.GetFloat( row , "BREAKHOURS" );
								timeCard.ExtId = long.Parse(timeCard.EmpPosId + inDate.ToString("ddMMyy") 
									+ inTime.Substring(0, 2) + inTime.Substring( 3, 2));
								timeCard.RegTotal =  timeCard.RegWage * timeCard.RegHours;
								timeCard.ClockIn = it;
								timeCard.ClockOut = ot;
								this.Add(timeCard);
							}
							catch(Exception ex)
							{
								logger.Error(ex.ToString() + " : " + ex.StackTrace);
							}
						}
					}
				}
			}
			catch(Exception ex)
			{
				logger.Error( ex.ToString() + " : " + ex.StackTrace );
			}*/
			#endregion
		}


		/*utility method for RM execute.  the loginxx.dbf files are seperated by year.
		 * All this method does is return the current year, plus the previous year if
		 * today's date is within the first week of a new year (i.e. Jan 1-6)
		 */ 
		private ArrayList GetYears()
		{
			ArrayList years = new ArrayList(2);
			DateTime today = DateTime.Today;
			if(today.Month == 1 && today.Day < 14)
				years.Add(today.AddYears(-1));
			years.Add(today);
			return years;

		}
	
		private Hashtable getJobs()
		{
			Hashtable jobs = new Hashtable();
			HsFile hsFile = new HsFile();
			hsFile.Copy( this.Details.Dsn, this.Details.Dsn + @"\hstemp", "JOBCLASS.DBF" );
			DataSet dataSet = new DataSet();
			String empCnxStr = this.Cnx.ConnectionString + @"\hstemp";
			OdbcConnection newConnection = this.cnx.GetCustomOdbc( empCnxStr );
			OdbcCommand selectCmd =  new OdbcCommand("SELECT JOB_NO, JOB_DESC FROM JOBCLASS",newConnection);

			OdbcDataAdapter dataAdapter = new OdbcDataAdapter(selectCmd);

			dataAdapter.Fill(dataSet, "JOBCLASS");
			dataAdapter.Dispose();

			DataRowCollection rows = dataSet.Tables[0].Rows;
			foreach(DataRow row in rows)
			{
				try
				{
					int id = data.GetInt(row, "JOB_NO");
					string name = data.GetString(row, "JOB_DESC");
					jobs.Add(id, name);
				}
				catch(Exception ex)
				{
					logger.Error("Error getting job names" + ex.ToString());
				}
			}
			return jobs;
		}


		#region unused overrides
		public override void DbUpdate(){}
		public override void DbInsert(){}
		#endregion
	}
}
