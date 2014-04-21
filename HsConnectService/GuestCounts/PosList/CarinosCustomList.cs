using HsConnect.Data;
using HsConnect.SalesItems;

using System;
using System.Data;
using System.Collections;
using Microsoft.Data.Odbc;

namespace HsConnect.GuestCounts.PosList
{
    public class CarinosCustomList : GuestCountListImpl
    {
        public CarinosCustomList() { }

        private HsData data = new HsData();
        private Hashtable _timePeriods;

        public override void DbLoad()
        {
            LoadTimePeriods();
            OdbcConnection newConnection = this.cnx.GetOdbc();
            DateTime start = DateTime.Now.AddDays(-3.0);
            while (start.TimeOfDay.Minutes != 30 && start.TimeOfDay.Minutes != 0)
            {
                start = start.AddMinutes(-1.0);
            }
            start = new DateTime(start.Year, start.Month, start.Day, start.Hour, start.Minute, 0, 0);
            while (start <= DateTime.Now)
            {
                GuestCountItem gcItem = new GuestCountItem();
                try
                {
                    if (_timePeriods[start.ToString("HH:mm:ss")] != null)
                    {
                        DataSet dataSet = new DataSet();
                        OdbcDataAdapter dataAdapter = new OdbcDataAdapter("select sum(cover_count) as guests from micros.v_R_sys_time_period " +
                            "where business_date = ?  and time_period_seq = ?", newConnection);
                        dataAdapter.SelectCommand.Parameters.Add("", start.ToString("yyyy-MM-dd"));
                        dataAdapter.SelectCommand.Parameters.Add("", _timePeriods[start.ToString("HH:mm:ss")]);
                        dataAdapter.Fill(dataSet, "micros.v_R_sys_time_period");
                        dataAdapter.Dispose();
                        DataRowCollection rows = dataSet.Tables[0].Rows;
                        foreach (DataRow row in rows)
                        {
                            try
                            {
                                gcItem.Date = new DateTime(start.Year, start.Month, start.Day, start.TimeOfDay.Hours, start.TimeOfDay.Minutes, 0);
                                gcItem.GuestCount = data.GetInt(row, "guests");
                                logger.Log("business_date = " + start.ToString("yyyy-MM-dd") + " and time_period_seq = " +
                                    _timePeriods[start.ToString("HH:mm:ss")] + ": " + gcItem.GuestCount);
                            }
                            catch (Exception ex)
                            {
                                logger.Error("Error adding micros sales item in Load(): " + ex.ToString());
                            }
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
                if (gcItem.GuestCount < 0) gcItem.GuestCount = 0;
                this.Add(gcItem);
                start = start.AddHours(.5);
            }
            logger.Debug(this.GetXmlString());
        }

        public override void DbUpdate() { }
        public override void DbInsert() { }

        private void LoadTimePeriods()
        {
            _timePeriods = new Hashtable();
            int startMinute = 0;
            int interval = 30;

            OdbcConnection newConnection = this.cnx.GetOdbc();

            while (startMinute <= 2330)
            {
                try
                {
                    DataSet dataSet = new DataSet();
                    OdbcDataAdapter dataAdapter = new OdbcDataAdapter("select b.time_period_seq, a.start_time, a.end_time " +
                        "from micros.period_def a, micros.time_period_def b where a.prd_seq = b.prd_seq and start_time = ? and end_time = ?", newConnection);
                    dataAdapter.SelectCommand.Parameters.Add("", startMinute);
                    int end = startMinute != 2330 ? (startMinute + interval) : 0;
                    dataAdapter.SelectCommand.Parameters.Add("", end);
                    dataAdapter.Fill(dataSet, "micros.period_def");
                    dataAdapter.Dispose();
                    DataRowCollection rows = dataSet.Tables[0].Rows;
                    try
                    {
                        if (rows.Count > 0 && rows[0]["time_period_seq"] != null)
                        {
                            _timePeriods.Add(GetTime(startMinute).ToString(), rows[0]["time_period_seq"].ToString() + "");
                            logger.Log(startMinute + " - " + (startMinute + interval) + " = " + rows[0]["time_period_seq"].ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error adding time_period_seq in LoadTimePeriods(): " + ex.ToString());
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
                startMinute += interval;
                interval = interval == 30 ? 70 : 30;
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
