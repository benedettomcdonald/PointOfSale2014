using HsConnect.Data;
using HsSharedObjects.Client;
using HsSharedObjects.Client.Preferences;
using System;
using System.Data;
using System.Collections;
using Microsoft.Data.Odbc;

namespace HsConnect.SalesItems.PosList
{
	public class RestMgrSalesItemList : SalesItemListImpl
	{
		public RestMgrSalesItemList() {}

		private HsData data = new HsData();

		public override float GetSalesTotal()
		{
			float ttl = 0.0f;
			return ttl;
		}

		/**
		 * Called by sales sync. Restaurant Manager's sales items are
		 * stored in SLSxxxx.dbf where the xxxx is the month and year
		 * of the sales items (e.g. SLS0707.DBF for july '07).
		 *  To sync sales, the code finds the correct month(s) to look in
		 * and then grabs the relevant data for the last 7 days.  Each row 
		 * returned is then used to create a SalesItem object which is
		 * then added to the list.
		 * It copies the file(s) into the hsTemp directory before accessing it.
		 */ 
		public override void DbLoad()
		{
			//int ct =0 ;
			bool grossSales = Details.Preferences.PrefExists(Preference.GROSS_SALES);
			if(grossSales)
				logger.Debug("Using gross sales");
			HsFile hsFile = new HsFile();
			double netSales = 0.0;
			ArrayList months = getSyncMonths();
			DateTime today =DateTime.Today;
			foreach( DateTime month in months )
			{
				String dt = month.ToString( "MMyy" );
				String empCnxStr = this.Cnx.ConnectionString;
				logger.Debug( "Connection strng = " + empCnxStr );
				hsFile.Copy( this.Details.Dsn, this.Details.Dsn + @"\hstemp", "SLS"+dt+".DBF" );
				hsFile.Copy( this.Details.Dsn, this.Details.Dsn + @"\hstemp", "SDET"+dt+".DBF" );
				OdbcConnection newConnection = this.Cnx.GetCustomOdbc( empCnxStr );			
				try
				{
					DataSet dataSet = new DataSet();
					OdbcCommand selectCmd =  new OdbcCommand("SELECT a.BILL_NO, a.OPEN_TIME, a.BILL_TIME, "
						+"a.BILL_DATE, a.TOTAL, a.DISCOUNT FROM SLS"+dt+" a "
						+" WHERE a.DATE > ? AND a.PAY_TYPE <> 4 AND a.PAY_TYPE <> 5 " , newConnection );

					selectCmd.Parameters.Add( "dt", today.AddDays(-7) );
					OdbcDataAdapter dataAdapter = new OdbcDataAdapter(selectCmd);
					dataAdapter.Fill( dataSet , "SLS"+dt );

					#region old hack save just in case

					//				String cmdTxt = "SELECT BILL_NO FROM sls"+dt;
					//				OdbcCommand selectCmd =  new OdbcCommand();
					//				selectCmd.Connection = newConnection;
					//				selectCmd.CommandText = cmdTxt.Trim();
					//selectCmd.Parameters.Add( "dt", today.AddDays(-14) );
					//				OdbcDataAdapter dataAdapter = new OdbcDataAdapter(selectCmd);
					//				OdbcDataReader rrr ;
				
					//				try
					//				{
					//	dataAdapter.Fill(dataSet,2225, 9999, "sls"+dt);
					//				}
					//				catch(Exception ex)
					//				{
					//					newConnection.Open();
					//					
					//					rrr = selectCmd.ExecuteReader();
					//					while(rrr.Read())
					//					{
					//						
					//						ct++;
					//						if(ct == 134)
					//						{
					//							Console.WriteLine("134");
					//						}
					//						Object ooo = rrr.GetValue(0);
					//						Console.WriteLine("reading");
					//					}
					//					

					//				}

					#endregion

					dataAdapter.Dispose();
					DataRowCollection rows = dataSet.Tables[0].Rows;
					foreach( DataRow row in rows )
					{
						try
						{
								DateTime salesDate = data.GetDate( row , "BILL_DATE" );
							TimeSpan ts = today.Subtract(salesDate);
							SalesItem item = new SalesItem();
							//use BILL_TIME for smaller total, OPEN_TIME for all
							String salesTime = data.GetString( row , "BILL_TIME" );
							long tempId = Convert.ToInt64("1"+data.GetInt(row, "BILL_NO"));
							item.ExtId = tempId;
							item.Amount = data.GetDouble(row, "TOTAL");
							if(!grossSales)
								item.Amount += data.GetDouble(row, "DISCOUNT");						
							item.Hour = Int32.Parse(salesTime.Substring(0, 2));
							item.Minute = Int32.Parse(salesTime.Substring(3, 2));
							item.DayOfMonth = salesDate.Day;
							item.Month = salesDate.Month;
							item.Year = salesDate.Year;
							double amount = item.Amount;
							netSales += amount;
							this.Add( item );
						}
						catch( Exception ex )
						{
							logger.Error( "Error adding RestMgr sales item in Load(): " + ex.ToString() );
						}
					}

					if (!grossSales)
					{
						dataSet = new DataSet();
						selectCmd = new OdbcCommand("SELECT a.BILL_NO, a.OPEN_TIME, a.BILL_TIME, "
							+ "a.BILL_DATE, a.TOTAL, a.DISCOUNT, b.ITEM_ADJ FROM SLS" + dt + " a, SDET" + dt + " b WHERE a.BILL_NO = b.BILL_NO AND EXISTS "
							+ " ( SELECT BILL_NO FROM SDET" + dt + " b WHERE a.BILL_NO = b.BILL_NO) AND b.ITEM_ADJ <> NULL AND a.BILL_DATE > ? AND PAY_TYPE <> 4 AND PAY_TYPE <>5", newConnection);
						selectCmd.Parameters.Add("dt", today.AddDays(-7));
						dataAdapter = new OdbcDataAdapter(selectCmd);
						dataAdapter.Fill(dataSet, "SLS" + dt);

						dataAdapter.Dispose();
						rows = dataSet.Tables[0].Rows;
						long tempBill = 0;
						int ct = 0;
						foreach (DataRow row in rows)
						{
							try
							{
								DateTime salesDate = data.GetDate(row, "BILL_DATE");
								TimeSpan ts = today.Subtract(salesDate);
								SalesItem item = new SalesItem();
								//use BILL_TIME for smaller total, OPEN_TIME for all
								if (tempBill != data.GetInt(row, "BILL_NO"))
								{
									ct = 0;
									tempBill = data.GetInt(row, "BILL_NO");
								}
								else
									ct++;

								String salesTime = data.GetString(row, "BILL_TIME");
								long tempId = Convert.ToInt64("2" + ct.ToString() + tempBill);
								item.ExtId = tempId;
								item.Amount = (-1.0 * data.GetDouble(row, "ITEM_ADJ"));
								item.Hour = Int32.Parse(salesTime.Substring(0, 2));
								item.Minute = Int32.Parse(salesTime.Substring(3, 2));
								item.DayOfMonth = salesDate.Day;
								item.Month = salesDate.Month;
								item.Year = salesDate.Year;
								double amount = item.Amount;
								netSales += amount;
								this.Add(item);
							}
							catch (Exception ex)
							{
								logger.Error("Error adding RestMgr sales item in Load(): " + ex.ToString());
							}
						} 
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
				logger.Log( "Loaded sales items for " + startDate.ToString() + " : " + netSales );
			}
		}

		/**
		 * Utility method for DbLoad.  This gets the current month and also the
		 * previous month, if today's date is within a week of the beginning
		 * of the month.
		 */ 
		private ArrayList getSyncMonths()
		{
			ArrayList aList = new ArrayList();
			DateTime today = DateTime.Today;
			if(today.Day < 7)
				aList.Add(today.AddMonths(-1));
			aList.Add(today);
			return aList;
		}
		#region unused overrides
		public override void DbUpdate(){}
		public override void DbInsert(){}
		#endregion
	}
}
