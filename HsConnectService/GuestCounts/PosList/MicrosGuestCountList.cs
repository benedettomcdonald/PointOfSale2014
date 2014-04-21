using HsConnect.Data;
using System;
using System.Data;
using System.Collections;
using HsSharedObjects.Client.Preferences;
using Microsoft.Data.Odbc;

namespace HsConnect.GuestCounts.PosList
{
	public class MicrosGuestCountList : GuestCountListImpl
	{
		private HsData data = new HsData();
		private Hashtable _timePeriods;
		private Hashtable _coversByPeriod;

		private int _customQuery = -1;
		private const string DefaultQuery = 
			"select business_date, em_store_num, rvc_def.obj_num, rvc_def.name, time_prd_num, entree_cnt " +
			"from custom.rvc_tp_entree_ttl, micros.rvc_def, micros.time_period_def " +
			"where custom.rvc_tp_entree_ttl.rvc_seq=micros.rvc_def.rvc_seq " +
			"and custom.rvc_tp_entree_ttl.time_prd_num=micros.time_period_def.obj_num " +
			"and business_date = ? and time_prd_num between 151 and 198 and (rvc_tp_entree_ttl.rvc_seq<>6 and rvc_tp_entree_ttl.rvc_seq<>8)";

		private const int BravoBrioRestaurantGroup = 1;
		private const string BravoBrioQuery = 
			" SELECT base.business_date," +
			" 	base.store_number AS em_store_num," +
			" 	rvc.obj_num," +
			" 	rvc.name," +
			" 	base.time_period_number AS time_prd_num," +
			" 	sum(otttl.cov_cnt) AS entree_cnt" +
			" FROM MICROS.v_R_rvc_time_period_base AS base " +
			" LEFT OUTER JOIN MICROS.dly_rvc_fixed_prd_ot_ttl AS otttl" +
			" ON (base.business_date = otttl.business_date AND base.store_id = otttl.store_id " +
			" 	AND base.rvc_seq = otttl.rvc_seq AND " +
			" 		((base.end_fixed_period_seq >= base.start_fixed_period_seq " +
			" 			AND otttl.fixed_period_seq BETWEEN base.start_fixed_period_seq " +
			" 			AND base.end_fixed_period_seq)" +
			" 		OR (base.end_fixed_period_seq < base.start_fixed_period_seq" +
			" 			AND	(otttl.fixed_period_seq BETWEEN base.start_fixed_period_seq " +
			" 			AND 96 OR otttl.fixed_period_seq BETWEEN 1 AND base.end_fixed_period_seq)))) " +
			" LEFT OUTER JOIN MICROS.order_type_def AS odef " +
			" ON odef.order_type_seq = otttl.order_type_seq" +
			" LEFT OUTER JOIN MICROS.rvc_def rvc" +
			" ON base.rvc_seq = rvc.rvc_seq" +
			" WHERE base.business_date = ? AND otttl.order_type_seq IS NOT NULL" +
			" GROUP BY base.business_date," +
			" 	base.store_number," +
			" 	rvc.obj_num," +
			" 	rvc.name," +
			" 	base.time_period_number";

		public MicrosGuestCountList() 
		{
			useDateTimeHash = false;
		}

		public override void DbLoad()
		{
			Preference fondiPref = this.Details.Preferences.GetPreferenceById(Preference.RUI_FONDI_COVER_COUNT);
			logger.Debug("RUI_FONDI_COVER_COUNT = " + Preference.RUI_FONDI_COVER_COUNT);
			logger.Debug("PrefExists(RUI_FONDI_COVER_COUNT) = " +
				Details.Preferences.PrefExists(Preference.RUI_FONDI_COVER_COUNT));
			bool fondi = Details.Preferences.PrefExists(Preference.RUI_FONDI_COVER_COUNT);
			logger.Debug("This is " + (!fondi ? "NOT " : "") + "a fondi");

			Preference customQueryPref = Details.Preferences.GetPreferenceById(Preference.CustomGuestCountQuery);
			if (customQueryPref != null)
			{
				try
				{
					_customQuery = Convert.ToInt32(customQueryPref.Val2);
				}
				catch (Exception e)
				{
					logger.Error("Error parsing Val2 from preference " + Preference.CustomGuestCountQuery);
				}
			}
//			The following line is for testing the custom Bravo Brio Guest Count Sync
//			_customQuery = BravoBrioRestaurantGroup;

			OdbcConnection newConnection = this.cnx.GetOdbc();
			newConnection.Open();
			try
			{
				LoadTimePeriods();
				if (!fondi)
					PreProcessing(newConnection);
				_coversByPeriod = new Hashtable();
				DateTime start = DateTime.Today.AddDays(-7.0);
				while (start <= DateTime.Today)
				{
					if (fondi)
						FondiGetCountForDay(newConnection, start);
					else
						GetCountForDay(newConnection, start);

					int dayTotal = 0;
					foreach (Object o in _coversByPeriod.Values)
					{
						GuestCountItem gci = (GuestCountItem)o;
						this.Add(gci);
						dayTotal += gci.GuestCount;
					}
					logger.Debug("GC Total for " + start.ToShortDateString() + ": " + dayTotal);
					_coversByPeriod.Clear();
					start = start.AddDays(1);
				}

			}
			finally
			{
				newConnection.Close();
			}
			logger.Debug(this.GetXmlString());
		}

		private void GetCountForDay(OdbcConnection newConnection, DateTime date)
		{
			DataSet dataSet = new DataSet();

			OdbcDataAdapter dataAdapter = null;
			switch (_customQuery)
			{
				case BravoBrioRestaurantGroup:
					logger.Debug("Using Bravo Brio Restaurant Group guest count query");
					dataAdapter = new OdbcDataAdapter(BravoBrioQuery, newConnection);
					break;
				default:
					logger.Debug("Using default guest count query");
					dataAdapter = new OdbcDataAdapter(DefaultQuery, newConnection);
					break;
			}
            
			dataAdapter.SelectCommand.Parameters.Add("", date.ToString("yyyy-MM-dd"));
			dataAdapter.Fill(dataSet, "custom.v_R_rvc_time_period");
			dataAdapter.Dispose();
			DataRowCollection rows = dataSet.Tables[0].Rows;
			logger.Debug("Query returned " + rows.Count + " rows");
			foreach (DataRow row in rows)
			{
				GuestCountItem gcItem = new GuestCountItem();
				DateTime busDate = data.GetDate(row, "BUSINESS_DATE");
				int rvc = data.GetInt(row, "OBJ_NUM");
				String rvcName = data.GetString(row, "NAME");
				int period = data.GetInt(row, "TIME_PRD_NUM");
				int coverCount = Convert.ToInt32(data.GetDouble(row, "ENTREE_CNT"));
				// if (coverCount < 0) coverCount = 0;

				if (_timePeriods[period] == null)
					continue;
				String dt = (String)_timePeriods[period];
				int hour = 0;
				int min = 0;
				if (dt.Length == 4)
				{
					hour = Int32.Parse(dt.Substring(0, 2));
					min = Int32.Parse(dt.Substring(2));
				}
				else if (dt.Length == 3)
				{
					hour = Int32.Parse(dt.Substring(0, 1));
					min = Int32.Parse(dt.Substring(1));
				}
				else if (dt.Length == 2)
				{
					hour = 0;
					min = Int32.Parse(dt);
				}
				else
				{
					min = Int32.Parse(dt);
				}
				busDate = busDate.AddHours(hour);
				busDate = busDate.AddMinutes(min);
				busDate = busDate.AddSeconds(0);
				gcItem.Date = busDate;
				gcItem.GuestCount = coverCount;
				//     _coversByPeriod.Add(period, gcItem);
				logger.Debug("first query for each row.  Adding: " + coverCount + " : " + gcItem.Date.ToString());
				if (_coversByPeriod[period] == null)
				{
					gcItem.addRvc(coverCount, rvc, rvcName);
					_coversByPeriod.Add(period, gcItem);
				}
				else
				{
					GuestCountItem gci = (GuestCountItem)_coversByPeriod[period];
					_coversByPeriod.Remove(period);
					gci.GuestCount += gcItem.GuestCount;
					gci.addRvc(coverCount, rvc, rvcName);
					_coversByPeriod.Add(period, gci);

				}
				//  this.Add(gcItem);
			}

			if (_customQuery<0)
			{
				//second query...for catering
				dataSet = new DataSet();
				dataAdapter = new OdbcDataAdapter(
					"select business_date, store_number, rvc_number, rvc_def.name, time_period_number, time_period_name, (isnull(cover_count,0)) as COVER_CNT " +
					"from custom.v_R_rvc_time_period " +
					"join micros.rvc_def " +
					"on custom.v_R_rvc_time_period.rvc_seq= micros.rvc_def.rvc_seq " +
					"where business_date = ? and time_period_number between 151 and 198 and (custom.v_R_rvc_time_period.rvc_seq=6 or custom.v_R_rvc_time_period.rvc_seq=8)", newConnection);
				dataAdapter.SelectCommand.Parameters.Add("", date.ToString("yyyy-MM-dd"));
				dataAdapter.Fill(dataSet, "custom.v_R_sys_time_period");
				dataAdapter.Dispose();
				rows = dataSet.Tables[0].Rows;
				foreach (DataRow row in rows)
				{
					logger.Debug("second query for each row");
					GuestCountItem gcItem = new GuestCountItem();
					DateTime busDate = data.GetDate(row, "BUSINESS_DATE");
					int rvc = data.GetInt(row, "RVC_NUMBER");
					String rvcName = data.GetString(row, "NAME");
					int period = data.GetInt(row, "TIME_PERIOD_NUMBER");
					int coverCount = 0;
					try
					{
						coverCount = Convert.ToInt32(data.GetDouble(row, "COVER_CNT"));
					}
					catch (Exception ex)
					{
						logger.Error("Null cover count.  you can ignore this");
					}
					//       if (coverCount < 0) coverCount = 0;

					if (_timePeriods[period] == null || coverCount <= 0)
						continue;
					String dt = (String)_timePeriods[period];
					int hour = 0;
					int min = 0;
					if (dt.Length == 4)
					{
						hour = Int32.Parse(dt.Substring(0, 2));
						min = Int32.Parse(dt.Substring(2));
					}
					else if (dt.Length == 3)
					{
						hour = Int32.Parse(dt.Substring(0, 1));
						min = Int32.Parse(dt.Substring(1));
					}
					else if (dt.Length == 2)
					{
						hour = 0;
						min = Int32.Parse(dt);
					}
					else
					{
						min = Int32.Parse(dt);
					}
					busDate.AddHours(hour);
					busDate.AddMinutes(min);
					busDate.AddSeconds(0);
					gcItem.Date = busDate;
					gcItem.GuestCount = coverCount;
					//_coversByPeriod.Add(period, gcItem);
					// this.Add(gcItem);
					if (_coversByPeriod[period] == null)
					{
						gcItem.addRvc(coverCount, rvc, rvcName);
						_coversByPeriod.Add(period, gcItem);
					}
					else
					{
						GuestCountItem gci = (GuestCountItem)_coversByPeriod[period];
						_coversByPeriod.Remove(period);
						gci.GuestCount += gcItem.GuestCount;
						gci.addRvc(coverCount, rvc, rvcName);
						_coversByPeriod.Add(period, gci);

					}

				} 
			}
		}


		private void FondiGetCountForDay(OdbcConnection newConnection, DateTime start)
		{
			logger.Debug("fondi - " + start.ToString());
			DataSet dataSet = new DataSet();
			OdbcDataAdapter dataAdapter = new OdbcDataAdapter(
				"select business_date, store_number, rvc_number, rvc_def.name, time_period_number, time_period_name,(isnull(cover_count,0)) as COVER_CNT " +
				"from custom.v_R_rvc_time_period " +
				"join  micros.rvc_def " +
				"on custom.v_R_rvc_time_period.rvc_seq= micros.rvc_def.rvc_seq " +
				"where business_date = ? and time_period_number  between 151 and 198", newConnection);
			dataAdapter.SelectCommand.Parameters.Add("", start.ToString("yyyy-MM-dd"));
			dataAdapter.Fill(dataSet, "custom.v_R_rvc_time_period");
			dataAdapter.Dispose();
			DataRowCollection rows = dataSet.Tables[0].Rows;
			foreach (DataRow row in rows)
			{
				GuestCountItem gcItem = new GuestCountItem();
				DateTime busDate = data.GetDate(row, "BUSINESS_DATE");
				int rvc = data.GetInt(row, "RVC_NUMBER");
				String rvcName = data.GetString(row, "NAME");
				int period = data.GetInt(row, "TIME_PERIOD_NUMBER");
				int coverCount = 0;
				try
				{
					coverCount =Convert.ToInt32(data.GetDouble(row, "COVER_CNT"));
				}
				catch (Exception ex)
				{
					logger.Error("Null cover count.  you can ignore this");
				}
				//       if (coverCount < 0) coverCount = 0;
                            
				if (_timePeriods[period] == null) 
					continue;

				String dt = (String)_timePeriods[period];
				int hour = 0;
				int min = 0;
				if (dt.Length == 4)
				{
					hour = Int32.Parse(dt.Substring(0, 2));
					min = Int32.Parse(dt.Substring(2));
				}
				else if (dt.Length == 3)
				{
					hour = Int32.Parse(dt.Substring(0, 1));
					min = Int32.Parse(dt.Substring(1));
				}
				else if (dt.Length == 2)
				{
					hour = 0;
					min = Int32.Parse(dt);
				}
				else
				{
					min = Int32.Parse(dt);
				}
				busDate = busDate.AddHours(hour);
				busDate = busDate.AddMinutes(min);
				busDate = busDate.AddSeconds(0);
				gcItem.Date = busDate;
				gcItem.GuestCount = coverCount;
				if (_coversByPeriod[period] == null)
				{
					gcItem.addRvc(coverCount, rvc, rvcName);
					_coversByPeriod.Add(period, gcItem);
				}
				else
				{
					GuestCountItem gci = (GuestCountItem)_coversByPeriod[period];
					_coversByPeriod.Remove(period);
					gci.GuestCount += gcItem.GuestCount;
					gci.addRvc(coverCount, rvc, rvcName);
					_coversByPeriod.Add(period, gci);

				}
				//    this.Add(gcItem);
			}
		}

		private void PreProcessing(OdbcConnection newConnection)
		{
			try
			{
				logger.Debug("pre-processing query");
				DataSet dSet = new DataSet();
				OdbcCommand cmd = new OdbcCommand("CALL custom.sp_post_rvc_time_prd_entree_ttls", newConnection);
				//OdbcDataAdapter dAdapter = new OdbcDataAdapter("CALL custom.sp_post_rvc_time_prd_entree_ttls", newConnection);
				cmd.ExecuteNonQuery();
				cmd.Dispose();
				logger.Debug("pre-processing query done");
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
			}
		}

		public override void DbUpdate() { }
		public override void DbInsert() { }

		private void LoadTimePeriods()
		{
			_timePeriods = new Hashtable();
			// int startMinute = 0;
			int startInterval = 151;   // default
			int endInterval = 198;  // default

			switch(_customQuery)
			{
				case BravoBrioRestaurantGroup:
					startInterval = 0;
					endInterval = 48;
					break;
				default:
					startInterval = 151;
					endInterval = 198;
					break;
			}
            
			OdbcConnection newConnection = this.cnx.GetOdbc();

			try
			{
				DataSet dataSet = new DataSet();
				OdbcDataAdapter dataAdapter = new OdbcDataAdapter("select b.obj_num, a.start_time, a.end_time " +
					"from micros.period_def a, micros.time_period_def b where a.prd_seq = b.prd_seq and b.obj_num > ? and b.obj_num < ?", newConnection);
				dataAdapter.SelectCommand.Parameters.Add("", startInterval-1);
				dataAdapter.SelectCommand.Parameters.Add("", endInterval + 1);
				dataAdapter.Fill(dataSet, "micros.period_def");
				dataAdapter.Dispose();
				DataRowCollection rows = dataSet.Tables[0].Rows;
				foreach (DataRow row in rows)
				{
					try
					{
						int currentInterval = data.GetInt(row, "obj_num");
						String dt = data.GetString(row, "START_TIME");

						_timePeriods.Add(currentInterval, dt);
						logger.Log(currentInterval + " - " + dt);
                            
					}
					catch (Exception ex)
					{
						logger.Error("Error adding time_period_seq in LoadTimePeriods(): " + ex.ToString());
					}
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
			}
			finally
			{
				newConnection.Close();
			}
		}

		private TimeSpan GetTime(int minutes)
		{
			String mins = minutes.ToString();
			if ((minutes == 0) || (minutes == 30))
			{
				return new TimeSpan(0, minutes, 0);
			}
			else if (mins.Length == 3)
			{
				return new TimeSpan(
					Convert.ToInt32(mins.Substring(0, 1)),
					Convert.ToInt32(mins.Substring(1, 2)), 0);
			}
			else
			{
				return new TimeSpan(
					Convert.ToInt32(mins.Substring(0, 2)),
					Convert.ToInt32(mins.Substring(2, 2)), 0);
			}
		}
	}
}
