using System;
using System.Collections;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using HsConnect.Data;
using HsSharedObjects.Client.Preferences;
using Microsoft.Data.Odbc;
using Nini.Ini;

namespace HsConnect.HistoricalSales.PosList
{
    public class AlohaHistSalesItems : HistoricalSalesItemListImpl 
    {
        public AlohaHistSalesItems() { }

		private HsData data = new HsData();
		private const int TYPEADD = 1;
		private const int TYPEIDADD = 2;
		private const int TYPESUBTRACT = 3;
		private const int TYPEIDSUBTRACT = 4;
	    private const int SalesTimeTypeOpen = 0;
	    private const int SalesTimeTypeOrder = 1;
	    private const int SalesTimeTypeClose = 2;

		public override void DbLoad()
		{
            logger.Debug("Begin AlohaHistSalesItems DbLoad()");
			double netSales = 0.0;
            ArrayList dates = getSyncDates();

		    int salesTimeType = SalesTimeTypeClose;  // Default to close time
		    Preference salesTimePref = Details.Preferences.GetPreferenceById(Preference.SalesTimeType);
            if(salesTimePref!=null)
                try
                {
                    salesTimeType = Int32.Parse(salesTimePref.Val2);
                }
                catch{}

			foreach( DateTime startDate in dates )
			{
				String dt = startDate.ToString( "yyyyMMdd" );
				//If today is in the list of dates, then we need to do some things different
				logger.Log("Testing Date:  " + dt);
				if(startDate.Date.CompareTo( DateTime.Today.Date ) == 0)
				{
					continue;
				}
				String empCnxStr = this.cnx.ConnectionString + @"\" + dt;
				logger.Debug( "Connection strng = " + empCnxStr );
				OdbcConnection newConnection = this.cnx.GetCustomOdbc( empCnxStr );			
				try
				{


					DataSet dataSet = new DataSet();
					#region type selection
					bool useLists = false;
					bool filterAdds = false;
					ArrayList typeAdd = new ArrayList();
					ArrayList typeIDAdd = new ArrayList();
					ArrayList typeSubtract = new ArrayList();
					ArrayList typeIDSubtract = new ArrayList();
					try
					{//Attempt to read in the alohaSales.ini file.  If it is there,
						//then we parse it and use it to populate the 4 arrayLists
						IniReader ini;
						
						
						if(File.Exists(System.Windows.Forms.Application.StartupPath + @"\alohaSales.ini"))
							ini = new IniReader(System.Windows.Forms.Application.StartupPath + @"\alohaSales.ini");
						else
						{
							logger.Log("No alohaSales.ini file in " + System.Windows.Forms.Application.StartupPath);
							logger.Log(@"Now looking in C:\Program Files\HotSchedules\HS Connect\files");
							ini = new IniReader(@"C:\Program Files\HotSchedules\HS Connect\files\alohaSales.ini");
						}
						int type = TYPEADD;
						while(ini.Read())
						{
							switch(ini.Type)
							{
								case IniType.Empty://empty line
									break;
								case IniType.Section://new section
									type = GetSectionType(ini.Name);
									break;
								case IniType.Key://value
								switch(type)
								{
									case TYPEADD://typeadd value
										typeAdd.Add(ini.Value);
										break;
									case TYPEIDADD://typeidadd value
										typeIDAdd.Add(ini.Value);
										break;
									case TYPESUBTRACT://typesubtract value
										typeSubtract.Add(ini.Value);
										break;
									case TYPEIDSUBTRACT://typeidsubtract value
										typeIDSubtract.Add(ini.Value);
										break;
								}
									useLists = true;//Lists are not empty, need to use them
									filterAdds = true;//typeId lists need to be used to filter adds
									break;
							}
						}
					}
					catch(Exception ex)
					{
						logger.Log("No alohaSales.ini file, using defaults");
					}
					StringBuilder typeSelect = new StringBuilder(" WHERE ");
					if(useLists)
					{
						if((typeAdd.Count > 0 && typeAdd[0].ToString().Equals("*"))
							|| (typeAdd.Count == 0 && typeSubtract.Count == 0))
						{
							typeSelect = new StringBuilder();//no WHERE statement
						}
						else//else create WHERE statement from lists.  We filter on types, not typeIds
						{
							foreach(String x in typeAdd){ typeSelect.Append(" TYPE = "+x+" or"); }
							foreach(String x in typeSubtract){ typeSelect.Append(" TYPE = "+x+" or"); }
							typeSelect.Remove(typeSelect.Length-2, 2);
						}
					}
					else
					{
						typeSelect.Append(" TYPE = 1 or TYPE = 6 or TYPE = 5 ");
					}
					//if the * is in the typeId lists or if they are both empty, we don't filter adds
					if(typeIDAdd.Count > 0 && typeIDAdd[0].ToString().Equals("*") 
						|| (typeIDAdd.Count ==0 && typeIDSubtract.Count ==0))
						filterAdds = false;
					#endregion

					OdbcDataAdapter dataAdapter = new OdbcDataAdapter(
						"SELECT EMPLOYEE, TYPE, TYPEID, AMOUNT , CLOSEHOUR , CLOSEMIN , OPENHOUR , OPENMIN, ORDERHOUR, ORDERMIN, REVENUE FROM GNDSALE" + typeSelect.ToString() , newConnection );
					dataAdapter.Fill( dataSet , "GNDSALE" );
					dataAdapter.Dispose();
					DataRowCollection rows = dataSet.Tables[0].Rows;
					int index = 0;
					foreach( DataRow row in rows )
					{
						try
						{
							HistoricalSalesItem item = new HistoricalSalesItem();
							long tempId = Convert.ToInt64( startDate.ToString( "yyyy" ) + 
								startDate.ToString( "MM" ) + startDate.ToString( "dd" ) +
								index.ToString() );
							item.ExtId = tempId;
                            item.EmployeeNum = data.GetInt(row, "EMPLOYEE");
							item.Amount = data.GetDouble( row , "AMOUNT" );
                            item.RVC = data.GetInt(row, "REVENUE");
                            item.Category = data.GetInt(row, "TYPEID");
							//If the subtract lists are not empty, then anything that matches
							//a type or typeid from the lists gets negated before it gets added
							//If the lists are empty, then just use the default (types 5 and 6)
							if(useLists)
							{
								foreach(String s in typeSubtract)
								{
									if(data.GetInt( row, "TYPE" ) == Int32.Parse(s))
										item.Amount = item.Amount * -1;
								}

								foreach(String s in typeIDSubtract)
								{
									if(data.GetInt( row, "TYPEId" ) == Int32.Parse(s))
										item.Amount = item.Amount = 0;
								}
							}
							else
							{
								if( data.GetInt( row, "TYPE" ) == 5 || data.GetInt( row, "TYPE" ) == 6 ) item.Amount = item.Amount * -1;						
							}

                            switch(salesTimeType)
                            {
                                case SalesTimeTypeOpen:
                                    item.Hour = data.GetInt(row, "OPENHOUR");
                                    item.Minute = data.GetInt(row, "OPENMIN");
                                    break;
                                case SalesTimeTypeOrder:
                                    item.Hour = data.GetInt(row, "ORDERHOUR");
                                    item.Minute = data.GetInt(row, "ORDERMIN");
                                    break;
                                case SalesTimeTypeClose:
                                    item.Hour = data.GetInt(row, "CLOSEHOUR");
                                    item.Minute = data.GetInt(row, "CLOSEMIN");
                                    break;
                                default:
                                    item.Hour = data.GetInt(row, "CLOSEHOUR");
                                    item.Minute = data.GetInt(row, "CLOSEMIN");
                                    break;
                            }

                            //set business and sales dates
                            DateTime punchDate = item.Hour < this.clientShiftStartTime.Hours ? new DateTime(startDate.Ticks).AddDays(-1) : new DateTime(startDate.Ticks);
                            item.BusinessDate = punchDate;
                            item.SalesDate = createSalesDate(startDate, item.Hour, item.Minute);


                            item.DayOfMonth = punchDate.Day;
							item.Month = punchDate.Month;
							item.Year = punchDate.Year;

                            double amount = item.Amount;
							
                            if( this.Details.Preferences.PrefExists( Preference.REMOVE_ALC_TAX ) )
							{
								Preference pref = this.Details.Preferences.GetPreferenceById( Preference.REMOVE_ALC_TAX );
								if( data.GetInt( row , "TYPEID" ) == Convert.ToInt32( pref.Val2 ) ||
									data.GetInt( row , "TYPEID" ) == Convert.ToInt32( pref.Val3 ) ||
									data.GetInt( row , "TYPEID" ) == Convert.ToInt32( pref.Val4 )
									)
								{
									amount = amount - (amount * .14);
									item.Amount = amount;
								}

							}
							//If the typeId lists are not empty then use them to filter
							//what gets added.  Only items who's typeId is in the lists
							//get added.  If these lists are empty, or if the * is present
							//in the add list, then add everything
							bool added = true;
							if(filterAdds)
							{
								added = false;
								foreach(String s in typeIDAdd)
								{
									if(data.GetInt( row, "TYPEId" ) == Int32.Parse(s))
										added = true;	
								}
								foreach(String s in typeIDSubtract)
								{
									if(data.GetInt( row, "TYPEId" ) == Int32.Parse(s))
										added = true;	
								}
							}
							if(added)
							{
								netSales += amount;
								this.Add( item );
							}

						}
						catch( Exception ex )
						{
							logger.Error( "Error adding aloha sales item in Load(): " + ex.ToString() );
						}
						finally
						{
							index++;
						}
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
				logger.Debug( "Loaded sales items for " + startDate.ToString() );
                logger.Debug("Finish AlohaHistSalesItems DbLoad()");
			}
		}
		public override void DbUpdate(){}
		public override void DbInsert(){}

		private int GetSectionType(String s)
		{
                  
			if(s.ToLower().Equals("typeadd"))
				return TYPEADD;
                              
			if(s.ToLower().Equals("typeidadd"))
				return TYPEIDADD;
                              
			if(s.ToLower().Equals("typesubtract"))
				return TYPESUBTRACT;
                              
			if(s.ToLower().Equals("typeidsubtract"))
				return TYPEIDSUBTRACT;
                              
                  
			return 0;
		}

        /*
         * Generate sync date list by keying off of Start and EndDate
         */
		private ArrayList getSyncDates()
		{
			ArrayList aList = new ArrayList();

            DateTime startDate = StartDate;
            DateTime endDate = EndDate;

			while( startDate <= endDate )
			{
				aList.Add( startDate );
				logger.Log( "Form form: " + startDate.ToString() );
				startDate = new DateTime( startDate.Ticks ).AddDays( 1 );			
			}

            logger.Debug("Historical date range generated for this run. Processing " + startDate.ToShortDateString() + " to " + endDate.ToShortDateString());
					
			return aList;
		}

        /*
         * Generate the sales date using hour and minute
         */
        private DateTime createSalesDate(DateTime startDate, int hour, int minute){
            String dateStr = "";
            String hourStr = "" + hour;
            String minStr = "" + minute;
            if (hour < 10)
            {
                hourStr = "0" + hour;
            }
            if (minute < 10)
            {
                minStr = "0" + minute;
            }
            
            dateStr += startDate.ToString("yyyy-MM-dd");
            dateStr += " " + hourStr + ":" + minStr + ":00.000";

            DateTimeFormatInfo myDTFI = new CultureInfo("en-US", false).DateTimeFormat;
            DateTime ret = DateTime.ParseExact(dateStr, "yyyy-MM-dd HH:mm:ss.fff", myDTFI);

            return ret;
        }
	}
}