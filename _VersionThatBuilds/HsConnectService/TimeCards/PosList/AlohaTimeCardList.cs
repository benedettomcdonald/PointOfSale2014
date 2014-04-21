using HsConnect.Data;
using HsConnect.SalesItems;
using HsSharedObjects.Client.Preferences;

using System;
using System.Collections;
using System.Data;
using Microsoft.Data.Odbc;

namespace HsConnect.TimeCards.PosList
{
	public class AlohaTimeCardList : TimeCardListImpl
	{
		public AlohaTimeCardList() {}

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

		/*This method is called by the labor sync.
		 * It retrieves the time cards/labor items from the aloha pos machine and puts them in HS
		 * FUNCITONALITY ADDED 6/26/07: MFisher
		 * When the prefrerence UPDATE_TIMERS is present (i.e. the user wants the system to sync at the end of shifts)
		 * then the method will add today to the list of dates and will cause aloha to grind today's data 
		 */ 
		public override void DbLoad()
		{
			DateTime tempDate;
			//DropCards = 1;
			int numDays = 15;
			//if the preference is checked make the first day today, otherwise, make it yesterda
			if(this.Details.Preferences.PrefExists( Preference.UPDATE_TIMERS ))
			{
				tempDate = DateTime.Now;
				numDays++;
			}

			else
			{
				tempDate = DateTime.Now.AddDays( -1.0 );
			}
			String[] connStrings = new String[ numDays ];
			for( int i = 0; i < numDays ; i++ )
			{
				if(tempDate.Date.CompareTo(DateTime.Today.Date) == 0)
				{
					//"Data" is the directory for today's data
					//the rest of this runs the script to grind today's data
					connStrings[i] = @"\" + "Data";
					String pathName = this.Details.Dsn + @"\bin";
					logger.Log("need to execute grind:  " + pathName);
					System.Diagnostics.Process proc = new System.Diagnostics.Process();
					proc.EnableRaisingEvents=false;
					proc.StartInfo.WorkingDirectory = pathName;
					proc.StartInfo.FileName="grind.exe";
					proc.StartInfo.Arguments="/date Data";
					proc.Start();
					proc.WaitForExit();
					logger.Log("Executed Grind successfully");
				}
				else
				{
					connStrings[i] = @"\" + tempDate.ToString( "yyyy" ) + tempDate.ToString( "MM" ) + tempDate.ToString( "dd" ) + "";
				}
				tempDate = tempDate.Subtract( TimeSpan.FromDays( 1 ) );
			}
			ArrayList timeCardsLarge = new ArrayList();
			#region Loop through last 14 days
			for( int i = 0; i < numDays ; i++ )
			{
				String empCnxStr = this.cnx.ConnectionString + connStrings[i];
				OdbcConnection newConnection = this.cnx.GetCustomOdbc( empCnxStr );
				OdbcDataAdapter dataAdapter;
				DataSet dataSet;
				bool useADJTIME = false;
				try
				{
					/** Try the ADJTIMEX table first
					 **/
					dataSet = new DataSet();
					dataAdapter = new OdbcDataAdapter(
						"select * from ADJTIMEX WHERE INVALID = 'N'", newConnection );
					#region try ADJTIMEX
					try
					{
						/** ADJTIMEX exists, and all time cards for this date can
						 * be collected from this table
						**/
						dataAdapter.Fill( dataSet , "ADJTIMEX" );
						DataRowCollection rows = dataSet.Tables[0].Rows;
						foreach( DataRow row in rows )
						{
							#region Make Time Card
							try
							{
								TimeCard timeCard = new TimeCard();
								timeCard.BusinessDate = data.GetDate( row , "DATE" );
								timeCard.ExtId = Convert.ToInt64( timeCard.BusinessDate.ToString( "yy" ) + timeCard.BusinessDate.ToString( "MM" ) + timeCard.BusinessDate.ToString( "dd" ) + data.GetString( row , "SHIFT_ID" )  + data.GetString(row, "EMPLOYEE"));
								timeCard.EmpPosId = data.GetInt( row , "EMPLOYEE" );
								timeCard.JobExtId = data.GetInt( row , "JOBCODE" );
								timeCard.RegHours = data.GetFloat( row , "HOURS" );
								timeCard.RegTotal = data.GetFloat( row , "PAY" );
								timeCard.OvtHours = data.GetFloat( row , "OVERHRS" );
								timeCard.OvtTotal = data.GetFloat( row , "OVERPAY" );
								timeCard.RegWage = data.GetFloat( row , "RATE" );
                                timeCard.OvtWage = timeCard.RegWage + data.GetFloat(row, "OVERRATE");
                                timeCard.DeclaredTips = data.GetFloat(row, "DECTIPS");
                                timeCard.CcTips = data.GetFloat(row, "CCTIPS");

								// make clock in date
								timeCard.ClockIn = new DateTime( timeCard.BusinessDate.Year , 
									timeCard.BusinessDate.Month , 
									timeCard.BusinessDate.Day , 
									data.GetInt( row , "INHOUR" ) , 
									data.GetInt( row , "INMINUTE" ) , 0 );
								
								// make clock out date
								DateTime outDate = ( data.GetInt( row , "OUTHOUR" ) < data.GetInt( row , "INHOUR" ) )
									|| data.GetInt( row , "OUTHOUR" ) == 24 ? timeCard.BusinessDate.AddDays( 1.0 ) : timeCard.BusinessDate ;
								timeCard.ClockOut = new DateTime( outDate.Year , 
									outDate.Month , 
									outDate.Day , 
									data.GetInt( row , "OUTHOUR" ) == 24 ? 0 : data.GetInt( row , "OUTHOUR" ), 
									data.GetInt( row , "OUTMINUTE" ) , 0 );
                                int breakMins = data.GetInt(row, "UNPAIDBRK");
                                if (breakMins > 0)
                                {
                                    timeCard.UnpaidBreakMinutes = breakMins;
                                }		
								timeCard.OvertimeMinutes =  data.GetInt( row , "OVERMIN" );
								timeCardsLarge.Add( timeCard );
							}
							#endregion
							catch( Exception ex )
                            {
                                string rowStr = "";
                                foreach (object o in row.ItemArray)
                                    rowStr += o + "|";
                                logger.Error(rowStr, ex);
							}
						}
					}
					catch( Microsoft.Data.Odbc.OdbcException ex )
					{
						/** An exception will be thrown if the table doesn't exist
						**/
						useADJTIME = true;
					}
                    catch( Exception ex)
                    {
                        logger.Error("Error reading ADJTIMEX.DBF", ex);
                    }
					#endregion
					#region use ADJTIME
					if( useADJTIME )
					{
						/** Try the ADJTIME table first
						**/
						dataSet = new DataSet();
						dataAdapter = new OdbcDataAdapter(
							"select * from ADJTIME WHERE INVALID = 'N'", newConnection );
						#region try ADJTIMEX
						try
						{
							/** ADJTIMEX exists, and all time cards for this date can
							 * be collected from this table
							**/
							dataAdapter.Fill( dataSet , "ADJTIME" );
							DataRowCollection rows = dataSet.Tables[0].Rows;
							foreach( DataRow row in rows )
							{
								#region Make Time Card
								try
								{
									TimeCard timeCard = new TimeCard();
									timeCard.BusinessDate = data.GetDate( row , "DATE" );
                                    timeCard.ExtId = Convert.ToInt64(timeCard.BusinessDate.ToString("yy") + timeCard.BusinessDate.ToString("MM") + timeCard.BusinessDate.ToString("dd") + data.GetString(row, "SHIFT_ID") + data.GetString(row, "EMPLOYEE"));
									timeCard.EmpPosId = data.GetInt( row , "EMPLOYEE" );
									timeCard.RegHours = data.GetFloat( row , "HOURS" );
									timeCard.RegTotal = data.GetFloat( row , "PAY" );
									timeCard.OvtHours = data.GetFloat( row , "OVERHRS" );
									timeCard.OvtTotal = timeCard.RegTotal + data.GetFloat( row , "OVERPAY" );
									timeCard.OvertimeMinutes = data.GetInt( row , "OVERMIN" );
									timeCard.JobExtId = data.GetInt( row , "JOBCODE" );
									timeCard.RegWage = data.GetFloat( row , "RATE" );
									timeCard.OvtWage = timeCard.RegWage + data.GetFloat( row , "OVERRATE" );
                                    timeCard.DeclaredTips = data.GetFloat(row, "DECTIPS");
                                    timeCard.CcTips = data.GetFloat(row, "CCTIPS");

									// make clock in date
                                    logger.Debug("Making clock in date");
									timeCard.ClockIn = new DateTime( timeCard.BusinessDate.Year , 
										timeCard.BusinessDate.Month , 
										timeCard.BusinessDate.Day , 
										data.GetInt( row , "INHOUR" ) , 
										data.GetInt( row , "INMINUTE" ) , 0 );
								
									// make clock out date
								    logger.Debug("Making clock out date");
									DateTime outDate = ( data.GetInt( row , "OUTHOUR" ) < data.GetInt( row , "INHOUR" ) )
										|| data.GetInt( row , "OUTHOUR" ) == 24 ? timeCard.BusinessDate.AddDays( 1.0 ) : timeCard.BusinessDate ;
									timeCard.ClockOut = new DateTime( outDate.Year , 
										outDate.Month , 
										outDate.Day , 
										data.GetInt( row , "OUTHOUR" ) == 24 ? 0 : data.GetInt( row , "OUTHOUR" ), 
										data.GetInt( row , "OUTMINUTE" ) , 0 );		
									
									int breakMins = data.GetInt( row, "UNPAIDBRK" );
									if( breakMins > 0 )
									{
                                        timeCard.UnpaidBreakMinutes = breakMins;
									}									

									timeCardsLarge.Add( timeCard );
								}
									#endregion
								catch( Exception ex )
								{
								    string rowStr = "";
                                    foreach(object o in row.ItemArray)
                                        rowStr += o + "|";
									logger.Error( rowStr,ex );
								}
							}
						}
						catch( Exception ex )
						{
							logger.Error( "Error reading ADJTIME.DBF", ex );
						}
						#endregion
					}
					#endregion					
				}
				catch( Exception ex )
				{
					logger.Error( ex.ToString() );
					Main.Run.errorList.Add(ex);
				}
				finally
				{
					newConnection.Close();
					useADJTIME = false;
				}
			}
			#endregion 

			foreach( TimeCard card in timeCardsLarge )
			{
				logger.Debug("adding timecard to main list:  " + card);
				logger.Debug("Add Status:  " + this.Add( card ));
			}
			
		}
		public override void DbUpdate(){}
		public override void DbInsert(){}
	}
}
