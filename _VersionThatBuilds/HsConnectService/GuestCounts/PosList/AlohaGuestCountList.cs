using HsConnect.Data;
using HsSharedObjects.Client.Preferences;
using HsConnect.SalesItems;
using HsConnect.Forms;

using HsSharedObjects;

using Nini.Ini;

using System;
using System.Data;
using System.Text;
using System.Collections;
using Microsoft.Data.Odbc;
using System.IO;


namespace HsConnect.GuestCounts.PosList
{
    public class AlohaGuestCountList : GuestCountListImpl
    {
        public AlohaGuestCountList() { }

        private HsData data = new HsData();
        private Hashtable _timePeriods;
        private Hashtable _minuteValues;
        private const int TYPEADD = 1;
        private const int TYPEIDADD = 2;
        private const int TYPESUBTRACT = 3;
        private const int TYPEIDSUBTRACT = 4;
        private const int SalesTimeTypeOpen = 0;
        private const int SalesTimeTypeOrder = 1;
        private const int SalesTimeTypeClose = 2;
        private HsSharedObjects.Client.ClientDetails details;
        private int salesTimeType = SalesTimeTypeClose;

        public HsSharedObjects.Client.ClientDetails Details
        {
            get { return this.details; }
            set { this.details = value; }
        }

        public override void DbLoad()
        {
            Preference salesTimePref = Details.Preferences.GetPreferenceById(Preference.SalesTimeType);
            if (salesTimePref != null)
            {
                try
                {
                    salesTimeType = Int32.Parse(salesTimePref.Val2);
                }
                catch{}
            }

            _minuteValues = new Hashtable();

            ArrayList dates = getSyncDates();

            dates = new ArrayList();

            //DateTime tdate = new DateTime(2008, 9, 9);
            //DateTime edate = new DateTime(2008, 11, 18);
            //while (tdate.Ticks < edate.Ticks)
            //{
                //dates.Add(new DateTime(tdate.Ticks));
                //tdate = tdate.AddDays(1.0);
           // }

            logger.Log("in DBLOAD" + dates.Count);

            foreach (DateTime startDate in dates)
            {
                logger.Log("in DBLOAD foreach");
                //String dt = startDate.ToString("MMdd");
                String dt = startDate.ToString( "yyyyMMdd" );
                //If today is in the list of dates, then we need to do some things different
                logger.Log("Testing Date:  " + dt);
                if (startDate.Date.CompareTo(DateTime.Today.Date) == 0)
                {
                    continue;
                }
                String empCnxStr = this.cnx.ConnectionString + @"\" + dt;
                //String empCnxStr = this.cnx.ConnectionString + @"\CRO25\" + dt;
                logger.Debug("Connection strng = " + empCnxStr);
                OdbcConnection newConnection = this.cnx.GetCustomOdbc(empCnxStr);
                int index = 0;
                int count = 0;
                try
                {
                    DataSet dataSet = new DataSet();
                    //StringBuilder typeSelect = new StringBuilder(" WHERE ");
                    //typeSelect.Append(" TYPE = 10 ");
                    //if the * is in the typeId lists or if they are both empty, we don't filter adds
                    OdbcDataAdapter dataAdapter = new OdbcDataAdapter(
                        "SELECT TYPE, TYPEID, AMOUNT , OPENHOUR , OPENMIN, ORDERHOUR, ORDERMIN, CLOSEHOUR, CLOSEMIN FROM GNDSALE WHERE TYPE = 10", newConnection);
                    dataAdapter.Fill(dataSet, "GNDSALE");
                    dataAdapter.Dispose();
                    DataRowCollection rows = dataSet.Tables[0].Rows;
                    logger.Log("found " + rows.Count + " rows.");
                    foreach (DataRow row in rows)
                    {
                        try
                        {
                            int hour = 0;
                            int minute = 0;
                            switch (salesTimeType)
                            {
                                case SalesTimeTypeOpen:
                                    hour = data.GetInt(row, "OPENHOUR");
                                    minute = data.GetInt(row, "OPENMIN");
                                    break;
                                case SalesTimeTypeOrder:
                                    hour = data.GetInt(row, "ORDERHOUR");
                                    minute = data.GetInt(row, "ORDERMIN");
                                    break;
                                case SalesTimeTypeClose:
                                    hour = data.GetInt(row, "CLOSEHOUR");
                                    minute = data.GetInt(row, "CLOSEMIN");
                                    break;
                                default:
                                    hour = data.GetInt(row, "CLOSEHOUR");
                                    minute = data.GetInt(row, "CLOSEMIN");
                                    break;
                            }

                            GuestCountItem gcItem = new GuestCountItem();
                            gcItem.Date = new DateTime(startDate.Year, startDate.Month, startDate.Day,
                                hour, minute, 0, 0);

                            int amnt = 0;
                            if (_minuteValues.Contains(gcItem.Date.ToString("MM/dd/yyyy HH:mm:ss")))
                            {
                                amnt = Convert.ToInt32(_minuteValues[gcItem.Date.ToString("MM/dd/yyyy HH:mm:ss")]);
                            }

                            amnt += data.GetInt(row, "AMOUNT");

                            gcItem.GuestCount = amnt;
                            this.Add(gcItem);

                            if (_minuteValues.Contains(gcItem.Date.ToString("MM/dd/yyyy HH:mm:ss")))
                            {
                                _minuteValues.Remove(gcItem.Date.ToString("MM/dd/yyyy HH:mm:ss"));
                                _minuteValues.Add(gcItem.Date.ToString("MM/dd/yyyy HH:mm:ss"), amnt + "");
                            }
                            else
                            {
                                _minuteValues.Add(gcItem.Date.ToString("MM/dd/yyyy HH:mm:ss"), amnt + "");
                            }

                            //count += data.GetInt( row, "COUNT" );
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Error adding aloha sales item in Load(): " + ex.ToString());
                        }
                        finally
                        {
                            index++;
                            //GuestCountItem totalGC = new GuestCountItem();
                            //totalGC.GuestCount = count;
                            //totalGC.Date = startDate.Date;
                            //this.Add(totalGC);

                        }
                    }
                    logger.Log("added " + this.Count + " rows.");
                }
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());
                }
                finally
                {
                    newConnection.Close();
                }
                logger.Debug("Loaded sales items for " + startDate.ToString() + " : " + index);
            }
        }

        public override void DbUpdate() { }
        public override void DbInsert() { }

        private void LoadTimePeriods()
        {
            _timePeriods = new Hashtable();
            int startMinute = 0;
            int interval = 100;

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
                            if (Convert.ToInt32(rows[0]["end_time"]) - Convert.ToInt32(rows[0]["start_time"]) == 100)
                            {
                                _timePeriods.Add(GetTime(startMinute).ToString(), rows[0]["time_period_seq"].ToString() + "");
                                logger.Debug(startMinute + " - " + (startMinute + interval) + " = " + rows[0]["time_period_seq"].ToString());
                            }
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
                //interval = interval == 30 ? 70 : 30;
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



        private ArrayList getSyncDates()
        {
            ArrayList aList = new ArrayList();

            //	DateTime startDate = DateTime.Now.AddDays(-1);
            //	DateTime endDate = DateTime.Now.AddDays(-7);

            //	if( !this.AutoSync )
            //{
            //SalesDateForm dateForm = new SalesDateForm();
            //dateForm.MaxDate = DateTime.Today;
            //dateForm.ShowDialog();
            //		startDate = HsCnxData.StartDate;//dateForm.GetSalesStartDate();
            //		endDate = HsCnxData.EndDate;//dateForm.GetSalesEndDate();

            //		while( startDate <= endDate )
            //			{
            //				aList.Add( startDate );
            //				logger.Log( "Form form: " + startDate.ToString() );
            //				startDate = new DateTime( startDate.Ticks ).AddDays( 1 );			
            //			}

            //			return aList;
            //}
            //else
            //{
            //SalesWeek salesWeek = new SalesWeek( this.Details.ClientId );
            //salesWeek.Load();

            GCWeek gcWeek = new GCWeek(this.Details.ClientId);
            gcWeek.Load();
            foreach (GCDay counts in gcWeek.DayAmounts)
            {
                if (counts.Count <= 0)
                {
                    aList.Add(counts.Date);
                    logger.Log("added date:  " + counts.Date);
                }
            }
            /*			while(endDate.CompareTo(startDate) < 1)
                        {
                            aList.Add(endDate.Date);
                            endDate = endDate.AddDays(1);
                            logger.Log(endDate.Date.ToString());
                        }
            */
            return aList;
            //}
        }
        private int GetSectionType(String s)
        {

            if (s.ToLower().Equals("typeadd"))
                return TYPEADD;

            if (s.ToLower().Equals("typeidadd"))
                return TYPEIDADD;

            if (s.ToLower().Equals("typesubtract"))
                return TYPESUBTRACT;

            if (s.ToLower().Equals("typeidsubtract"))
                return TYPEIDSUBTRACT;


            return 0;
        }

        public int Count
        {
            get { return (gcItemList == null ? -1 : gcItemList.Count); }
        }
    }
}
