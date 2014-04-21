using System;
using System.Collections;
using System.Data;
using System.Globalization;
using HsConnect.Data;
using HsSharedObjects.Client.Preferences;

namespace HsConnect.HistoricalSales.PosList
{
    class PosiHistSalesItems : HistoricalSalesItemListImpl
    {
        public PosiHistSalesItems() { }

		protected HsData data = new HsData();
        protected static String drive = Data.PosiControl.Drive;
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

        public override void DbLoad()
        {
            logger.Debug("Begin PosiHistSalesItems DbLoad()");
            Hashtable idHash = new Hashtable();
            //key off of StartDate and EndDate for processed date range
            DateTime syncDate = StartDate;
            //backdate 1 day for the increment to work correctly in while loop
            syncDate = syncDate.AddDays(-1);

            while (syncDate < EndDate)
            {
                syncDate = syncDate.AddDays(1);
                
                if (syncDate.Date.CompareTo(DateTime.Today.Date) == 0)
                {
                    //skip today
                    continue;
                }
                
                String dateStr = syncDate.ToString("MM/dd/yy");
                try
                {
                    PosiControl.Run("POSIDBF", "/ALT " + dateStr);
                    DataTableBuilder builder = new DataTableBuilder();

                    DataTable dt = builder.GetTableFromDBF(drive + @":\ALTDBF", @"C:\", "CHKITEMS");
                    DataRowCollection rows = dt.Rows;
                    foreach (DataRow row in rows)
                    {
                        try
                        {
                            if (this.Details.Preferences.PrefExists(Preference.FILTER_ITEM_TYPE))
                            {
                                Preference pref = this.Details.Preferences.GetPreferenceById(Preference.FILTER_ITEM_TYPE);
                                int itemNum = data.GetInt(row, "ITEM_NUM");
                                string[] filters = (pref.Val3).Split(',');
                                for (int i = 0; i < filters.Length; i++)
                                {
                                    if (itemNum == Convert.ToInt32(filters[i]))
                                    {
                                        continue;
                                    }
                                }
                            }

                            HistoricalSalesItem item = new HistoricalSalesItem();

                            DateTimeFormatInfo myDTFI = new CultureInfo("en-US", false).DateTimeFormat;
                            item.BusinessDate = syncDate;

                            string time = row["TIME"].ToString();
                            string hourStr = "";
                            string minStr = "";
                            if (time.Length < 4)
                            {
                                hourStr = "0" + time.Substring(0, 1);
                                minStr = time.Substring(1, 3);
                            }
                            else
                            {
                                hourStr = time.Substring(0, 2);
                                minStr = time.Substring(2, 4);
                            }

                            string salesDate = item.BusinessDate.ToString("MM/dd/yyyy") + " " + hourStr + ":" + minStr + ":" + "00.000";
                            DateTime mySalesDate = DateTime.ParseExact(salesDate, "MM/dd/yyyy HH:mm:ss.fff", myDTFI);
                            item.SalesDate = mySalesDate;

                            item.Amount = data.GetDouble(row, "SALES_AMT");

                            item.EmployeeNum = Convert.ToInt32((string)GetEmpNumber("" + data.GetInt(row, "USER_NUM")));

                            item.RVC = data.GetInt(row, "COST_CENTR");

                            item.Category = data.GetInt(row, "SALES_CAT");

                            item.HistExtId = "" + data.GetInt(row, "ITEM_NUM") + data.GetInt(row, "TABLE") + data.GetInt(row, "TIME") + Convert.ToInt32((string)GetEmpNumber("" + data.GetInt(row, "USER_NUM")));

                            if (!idHash.ContainsKey(item.HistExtId))
                            {
                                this.Add(item);
                                idHash.Add(item.HistExtId, item);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Error adding posi sales item in Load(): " + ex.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());
                    Main.Run.errorList.Add(ex);
                }
            }
            logger.Debug("Finish PosiHistSalesItems DbLoad()");
        }
		public override void DbUpdate(){}
		public override void DbInsert(){}

		protected TimeSpan GetDateTime( int hr )
		{
			if( hr % 2 == 0 ) return new TimeSpan(  hr / 2, 0, 0 );
			return new TimeSpan( hr/2, 30, 0 );
		}
	}
}