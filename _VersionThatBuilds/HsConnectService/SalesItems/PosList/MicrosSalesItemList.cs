using System.Collections;
using System.Diagnostics;
using System.Globalization;
using HsConnect.Data;
using HsConnect.SalesItems;

using System;
using System.Data;
using HsSharedObjects.Client.Preferences;
using Microsoft.Data.Odbc;

namespace HsConnect.SalesItems.PosList
{
	public class MicrosSalesItemList : SalesItemListImpl
	{
		public MicrosSalesItemList() {}

		private HsData data = new HsData();
		private const  int RUI = 0;
		private const  int COOPERS = 1;
		private const int FOUNDING_FARMERS = 2;
		private const int RUBY_TUESDAY_HAWAII = 3;
        private const int MAYBAR = 4;
        private const int CARA_IRISH_PUBS = 5;
		private ArrayList _rvcs;
		private ArrayList _salesCategories;

		public ArrayList RVCs
		{
			get
			{
				if (_rvcs == null)
					_rvcs = new ArrayList();
				return _rvcs;
			}
		}

		public ArrayList SalesCategories
		{
			get
			{
				if(_salesCategories==null)
					_salesCategories = new ArrayList();
				return _salesCategories;
			}
		}

		public override float GetSalesTotal()
		{
			bool grossSales = Details.Preferences.PrefExists(Preference.GROSS_SALES);
			if(grossSales)
				logger.Debug("Getting Gross Sales Total");

			float ttl = 0.0f;
			OdbcConnection newConnection = this.cnx.GetOdbc();
			try
			{
				String start = this.startDate.ToString( "yyyy-MM-dd HH:mm:ss" );
				String end = this.endDate.ToString( "yyyy-MM-dd HH:mm:ss" );
				logger.Debug( "start: " + start );
				logger.Debug( "end: " + end );
				DataSet dataSet = new DataSet();
				OdbcDataAdapter dataAdapter = new OdbcDataAdapter(
					"select SUM( b.net_sls_ttl " + (grossSales ? " - b.item_dsc_ttl - b.sttl_dsc_ttl" : "") + ") as AMOUNT " +
					"from micros.trans_dtl a , micros.sale_dtl b " +
					"where a.trans_seq = b.trans_seq and a.end_date_tm >= ? and a.end_date_tm <= ?", newConnection );
				dataAdapter.SelectCommand.Parameters.Add( "" , start );
				dataAdapter.SelectCommand.Parameters.Add( "" , end );
				dataAdapter.Fill( dataSet , "micros.trans_dtl" );
				dataAdapter.Dispose();
				DataRow row = dataSet.Tables[0].Rows[0];
				ttl = (float) data.GetDouble( row , "AMOUNT" );
			}
			catch( Exception ex )
			{
				logger.Error( ex.ToString() );
				Main.Run.errorList.Add(ex);
			}
			finally
			{
				newConnection.Close();
			}
			return ttl;
		}

		public override void DbLoad()
		{
			bool grossSales = Details.Preferences.PrefExists(Preference.GROSS_SALES);
			//		    bool giftCards = true;  // Details.Preferences.PrefExists(Preference.GIFT_CARD);
			if (grossSales)
				logger.Debug("Getting Gross Sales");

			OdbcConnection newConnection = this.cnx.GetOdbc();
			try
			{
				String start = this.startDate.ToString( "yyyy-MM-dd HH:mm:ss" );
				String end = this.endDate.ToString( "yyyy-MM-dd HH:mm:ss" );
				logger.Debug( "start: " + start );
				logger.Debug( "end: " + end );
				DataSet dataSet = new DataSet();
				OdbcDataAdapter dataAdapter = null;

				bool isFFSales = false;
				int queryId = -1;
				bool microsByRvc = Details.Preferences.PrefExists(Preference.MICROS_BY_RVC);
				if (this.Details.Preferences.PrefExists(HsSharedObjects.Client.Preferences.Preference.MICROS_CUSTOM_SALES))//this is for RUI, it is a custom query that will most likely only be used for them.  It corresponds to the "RUI Custom Sales" prefrence
				{
					HsSharedObjects.Client.Preferences.Preference pref
						= this.Details.Preferences.GetPreferenceById(HsSharedObjects.Client.Preferences.Preference.MICROS_CUSTOM_SALES);
					try
					{
						queryId = Int32.Parse(pref.Val2);
					}
					catch (Exception ex)
					{
						logger.Error("Error choosing SQL query.  Please check your HSC Settings to make sure a query is selected for the 'Custom MIRCOS Sales Query' prefrence.");
						logger.Error(ex.ToString());
					}
					switch(queryId)
					{
						case RUI:
							RUISalesSync(newConnection);
							return;

						case COOPERS:
							logger.Debug("Coopers Sales");
							dataAdapter = new OdbcDataAdapter(
								"select (b.net_sls_ttl  - b.item_dsc_ttl - b.sttl_dsc_ttl) as AMOUNT, " +
								"a.trans_seq as EXT_ID , " +
								"DATEPART( dd , a.end_date_tm ) as DAY , " +
								"DATEPART( mm , a.end_date_tm ) as MONTH , " +
								"DATEPART( yy , a.end_date_tm ) as YEAR , " +
								"DATEPART( hh , a.end_date_tm ) as HOUR , " +
								"DATEPART( mi , a.end_date_tm ) as MINUTE " +
								"from micros.trans_dtl a , micros.sale_dtl b " +
								"where a.trans_seq = b.trans_seq " +
								"and a.end_date_tm >= ? and a.end_date_tm <= ?" +
								"order by a.end_date_tm", newConnection);
							break;
                        case MAYBAR:
                            logger.Debug("Maybar Custom Net Sales");
                            dataAdapter = new OdbcDataAdapter(
                                "select b.net_sls_ttl as AMOUNT, a.trans_seq as EXT_ID , " +
                                "DATEPART( dd , a.end_date_tm ) as DAY , " +
                                "DATEPART( mm , a.end_date_tm ) as MONTH , " +
                                "DATEPART( yy , a.end_date_tm ) as YEAR , " +
                                "DATEPART( hh , a.end_date_tm ) as HOUR , " +
                                "DATEPART( mi , a.end_date_tm ) as MINUTE , " +
                                "a.rvc_seq as RVC, " +
                                "b.sls_itmzr_01 as CAT1, " +
                                "b.sls_itmzr_02 as CAT2, " +
                                "b.sls_itmzr_03 as CAT3, " +
                                "b.sls_itmzr_04 as CAT4, " +
                                "b.sls_itmzr_05 as CAT5, " +
                                "b.sls_itmzr_06 as CAT6, " +
                                "b.sls_itmzr_07 as CAT7, " +
                                "b.sls_itmzr_08 as CAT8, " +
                                "a.business_date from micros.trans_dtl a , micros.sale_dtl b where a.trans_seq = b.trans_seq " +
                                "and a.business_date >= ? and a.business_date <= ? order by a.end_date_tm", newConnection);
                            break;
                        case CARA_IRISH_PUBS:
                            logger.Debug("Cara Irish Pubs Custom Net Sales");
                            dataAdapter = new OdbcDataAdapter(
                                "select b.Net_Sls_Ttl as AMOUNT, a.trans_seq as EXT_ID, " + 
                                "DATEPART( dd , a.end_date_tm ) as DAY, " + 
                                "DATEPART( mm , a.end_date_tm ) as MONTH , " + 
                                "DATEPART( yy , a.end_date_tm ) as YEAR , " + 
                                "DATEPART( hh , a.end_date_tm ) as HOUR , " + 
                                "DATEPART( mi , a.end_date_tm ) as MINUTE " + 
                                "FROM micros.trans_dtl a , micros.sale_dtl b " + 
                                "where a.trans_seq = b.trans_seq " + 
                                "and a.end_date_tm >= ? and a.end_date_tm <= ? " + 
                                "order by a.end_date_tm", newConnection);
                            break;
						case FOUNDING_FARMERS:
							isFFSales = true;
							logger.Debug("Founding Farmer's Sales");
							while (this.startDate <= this.endDate)
							{
								dataSet = new DataSet();
								String s = this.startDate.ToString("yyyy-MM-dd");
								logger.Debug("inside FF sales.  Date:  " + s);
								dataAdapter = new OdbcDataAdapter(
									"CALL custom.hs_trans(?)", newConnection);
								dataAdapter.SelectCommand.Parameters.Add("", s);
								dataAdapter.Fill(dataSet, "micros.trans_dtl");
								dataAdapter.Dispose();
								DataRowCollection rs = dataSet.Tables[0].Rows;
								foreach (DataRow row in rs)
								{
									try
									{
										SalesItem item = new SalesItem();
										item.ExtId = data.GetInt(row, "EXT_ID");
										item.Amount = data.GetDouble(row, "AMOUNT");
										item.DayOfMonth = data.GetInt(row, "DAY");
										item.Month = data.GetInt(row, "MONTH");
										item.Year = data.GetInt(row, "YEAR");
										item.Hour = data.GetInt(row, "HOUR");
										item.Minute = data.GetInt(row, "MINUTE");
										this.Add(item);
									}
									catch (Exception ex)
									{
										logger.Error("Error adding micros sales item in Load(): " + ex.ToString());
									}
								}
								this.StartDate = this.StartDate.AddDays(1);
							}
							break;
//						case RUBY_TUESDAY_HAWAII:
//							logger.Debug("Ruby Tuesday Sales");
//							dataAdapter = new OdbcDataAdapter(
//								"select (b.net_sls_ttl  - b.item_dsc_ttl - b.sttl_dsc_ttl - b.sls_itmzr_04) as AMOUNT, " +
//								"a.trans_seq as EXT_ID, " +
//								"DATEPART( dd , a.end_date_tm ) as DAY, " +
//								"DATEPART( mm , a.end_date_tm ) as MONTH, " +
//								"DATEPART( yy , a.end_date_tm ) as YEAR, " +
//								"DATEPART( hh , a.end_date_tm ) as HOUR, " +
//								"DATEPART( mi , a.end_date_tm ) as MINUTE, " +
//								"a.rvc_seq as RVC, " +
//								"b.sls_itmzr_01 as CAT1, " +
//								"b.sls_itmzr_02 as CAT2, " +
//								"b.sls_itmzr_03 as CAT3, " +
//								"b.sls_itmzr_04 as CAT4, " +
//								"b.sls_itmzr_05 as CAT5, " +
//								"b.sls_itmzr_06 as CAT6, " +
//								"b.sls_itmzr_07 as CAT7, " +
//								"b.sls_itmzr_08 as CAT8, " +
//								"a.business_date " +
//								"from micros.trans_dtl a , micros.sale_dtl b " +
//								"where a.trans_seq = b.trans_seq " +
//								"and a.end_date_tm >= ? and a.end_date_tm <= ? " +
//								"order by a.end_date_tm", newConnection);
//							break;
					}
				}
				else
				{
					logger.Debug("non-Custom Sales");
					if(microsByRvc)
					{
						logger.Debug("Using MicrosByRVC");
						dataAdapter = new OdbcDataAdapter(
							"select (b.net_sls_ttl" + (grossSales ? " - b.item_dsc_ttl - b.sttl_dsc_ttl" : "") + ") as AMOUNT, a.trans_seq as EXT_ID , " +
							"DATEPART( dd , a.end_date_tm ) as DAY ," +
							"DATEPART( mm , a.end_date_tm ) as MONTH ," +
							"DATEPART( yy , a.end_date_tm ) as YEAR , " +
							"DATEPART( hh , a.end_date_tm ) as HOUR ," +
							"DATEPART( mi , a.end_date_tm ) as MINUTE ," +
							"a.rvc_seq as RVC," +
							"b.sls_itmzr_01 as CAT1," +
							"b.sls_itmzr_02 as CAT2," +
							"b.sls_itmzr_03 as CAT3," +
							"b.sls_itmzr_04 as CAT4," +
							"b.sls_itmzr_05 as CAT5," +
							"b.sls_itmzr_06 as CAT6," +
							"b.sls_itmzr_07 as CAT7," +
							"b.sls_itmzr_08 as CAT8, " +
							"a.business_date from micros.trans_dtl a , micros.sale_dtl b where a.trans_seq = b.trans_seq " +
							"and a.business_date >= ? and a.business_date <= ? order by a.end_date_tm", newConnection);

						start = DateTime.Today.Subtract(new TimeSpan(7, 0, 0, 0)).ToString("yyyy-MM-dd HH:mm:ss");
						end = DateTime.Today.ToString("yyyy-MM-dd HH:mm:ss");
					}
					else
						dataAdapter = new OdbcDataAdapter(
							"select (b.net_sls_ttl" + (grossSales ? " - b.item_dsc_ttl - b.sttl_dsc_ttl" : "") + ") as AMOUNT, a.trans_seq as EXT_ID , " +
							"DATEPART( dd , a.end_date_tm ) as DAY ," +
							"DATEPART( mm , a.end_date_tm ) as MONTH ," +
							"DATEPART( yy , a.end_date_tm ) as YEAR , " +
							"DATEPART( hh , a.end_date_tm ) as HOUR ," +
							"DATEPART( mi , a.end_date_tm ) as MINUTE " +
							"from micros.trans_dtl a , micros.sale_dtl b where a.trans_seq = b.trans_seq " +
							"and a.end_date_tm >= ? and a.end_date_tm <= ? order by a.end_date_tm", newConnection);
				}

				if (!isFFSales)
				{
					dataAdapter.SelectCommand.Parameters.Add("", start);
					dataAdapter.SelectCommand.Parameters.Add("", end);
					dataAdapter.Fill(dataSet, "micros.trans_dtl");
					dataAdapter.Dispose();
					DataRowCollection rows = dataSet.Tables[0].Rows;
					foreach (DataRow row in rows)
					{
						try
						{
							if (!microsByRvc)
							{
								SalesItem item = new SalesItem();

								item.ExtId = data.GetInt(row, "EXT_ID");
								item.Amount = data.GetDouble(row, "AMOUNT");
								item.DayOfMonth = data.GetInt(row, "DAY");
								item.Month = data.GetInt(row, "MONTH");
								item.Year = data.GetInt(row, "YEAR");
								item.Hour = data.GetInt(row, "HOUR");
								item.Minute = data.GetInt(row, "MINUTE");

								Add(item);
							}
							else    // "Micros by RVC" (Proforma II)
							{
                                //skip adding RVC4 if MicrosByRVC && Ruby Tuesday Hawaii group
                                Boolean rth_flag = (queryId == RUBY_TUESDAY_HAWAII);
								for (int i = 1; i <= 8; ++i)    // Loop through each RVC
								{
									double catAmount = data.GetDouble(row, "CAT" + i);
                                    if (rth_flag && i == 4)
                                    {
                                        continue;
                                    }
									if (!catAmount.Equals(0.00))
									{
										SalesItem item = new SalesItem();

										item.Amount = catAmount;
										item.RVC = data.GetInt(row, "RVC");
										item.Category = i;

										item.ExtId = data.GetInt(row, "EXT_ID");
										item.DayOfMonth = data.GetInt(row, "DAY");
										item.Month = data.GetInt(row, "MONTH");
										item.Year = data.GetInt(row, "YEAR");
										item.Hour = data.GetInt(row, "HOUR");
										item.Minute = data.GetInt(row, "MINUTE");
										String businessDateStr = data.GetString(row, "BUSINESS_DATE");
										DateTime businessDate = DateTime.Parse(businessDateStr);
										item.BusinessDate = businessDate;

										Add(item);
									}
								}
							}

						}
						catch (Exception ex)
						{
							logger.Error("Error adding micros sales item in Load(): " + ex.ToString());
						}
					}
				}
				if (microsByRvc)
				{
					syncMicrosRVCs(newConnection);
					syncMicrosCategories(newConnection); 
				}
			}
			catch( Exception ex )
			{
				logger.Error( ex.ToString() );
				Main.Run.errorList.Add(ex);
			}
			finally
			{
				newConnection.Close();
			}
		}

		private void RUISalesSync(OdbcConnection newConnection)
		{
			OdbcDataAdapter dataAdapter;
			DataSet dataSet;
			bool isFFSales;
			logger.Debug("RUI Sales");
			newConnection.Open();
			OdbcCommand cmd = new OdbcCommand("CALL micros.sp_post_trans_archive", newConnection);
			//OdbcDataAdapter dAdapter = new OdbcDataAdapter("CALL custom.sp_post_rvc_time_prd_entree_ttls", newConnection);
			cmd.ExecuteNonQuery();
			cmd.Dispose();
			logger.Debug("pre-processing query done");
			isFFSales = true;

			DateTime tempStartDate = new DateTime(startDate.Ticks);
			DateTime tempEndDate = new DateTime(endDate.Ticks);
			//	        int maxDateAdjust = 7;
			while (tempStartDate <= tempEndDate)
			{
				dataSet = new DataSet();
				String s = tempStartDate.ToString("yyyy-MM-dd");
				logger.Debug("inside RUI sales.  Date:  " + s);
				dataAdapter = new OdbcDataAdapter(
					"CALL custom.rui_hs_trans(?,?)", newConnection);

				dataAdapter.SelectCommand.Parameters.Add("", s);
				dataAdapter.SelectCommand.Parameters.Add("", "0");
				dataAdapter.Fill(dataSet, "micros.trans_dtl");
				dataAdapter.Dispose();
				DataRowCollection rs = dataSet.Tables[0].Rows;
				logger.Debug(rs.Count + " RUI sales found");
				//	            if(rs.Count<=0 && maxDateAdjust>0)  // To account for days with 0 sales
				//	            {
				//	                tempEndDate = tempEndDate.AddDays(1);
				//	                logger.Debug("Adjusted End Date: " + tempEndDate.ToString("yyyy-MM-dd"));
				//	                --maxDateAdjust;
				//	            }
				foreach (DataRow row in rs)
				{
					try
					{
						SalesItem item = new SalesItem();
						item.ExtId = data.GetInt(row, "EXT_ID");
						item.Amount = data.GetDouble(row, "AMOUNT");
						item.DayOfMonth = data.GetInt(row, "DAY");
						item.Month = data.GetInt(row, "MONTH");
						item.Year = data.GetInt(row, "YEAR");
						item.Hour = data.GetInt(row, "HOUR");
						item.Minute = data.GetInt(row, "MINUTE");
						this.Add(item);
					}
					catch (Exception ex)
					{
						logger.Error("Error adding micros sales item in Load(): " + ex.ToString());
					}
				}
				tempStartDate = tempStartDate.AddDays(1);
			}
		}

		private void syncMicrosRVCs(OdbcConnection connection)
		{
			logger.Debug("Micros Revenue Centers");
			OdbcDataAdapter dataAdapter = new OdbcDataAdapter("SELECT rvc_seq AS EXT_ID, name as NAME from micros.rvc_def", connection);
			DataSet dataSet = new DataSet();
			dataAdapter.Fill(dataSet);
			dataAdapter.Dispose();
			RVCs.Clear();
			foreach(DataRow row in dataSet.Tables[0].Rows)
			{
				int extId = data.GetInt(row, "EXT_ID");
				String name = data.GetString(row, "NAME");
				RVCs.Add(new RevenueCenter(extId, name));
			}
		}

		private void syncMicrosCategories(OdbcConnection connection)
		{
			logger.Debug("Micros Sales Categories");
			OdbcDataAdapter dataAdapter = new OdbcDataAdapter("SELECT sls_itmzr_seq AS EXT_ID, name as NAME from micros.sales_itmzr_def", connection);
			DataSet dataSet = new DataSet();
			dataAdapter.Fill(dataSet);
			dataAdapter.Dispose();
			SalesCategories.Clear();
			foreach (DataRow row in dataSet.Tables[0].Rows)
			{
				int extId = data.GetInt(row, "EXT_ID");
				String name = data.GetString(row, "NAME");
				SalesCategories.Add(new SalesCategory(extId, name));
			}
		}

		public override void DbUpdate(){}
		public override void DbInsert(){}
	}
}
