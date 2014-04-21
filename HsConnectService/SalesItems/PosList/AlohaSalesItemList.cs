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

namespace HsConnect.SalesItems.PosList
{
	public class AlohaSalesItemList : SalesItemListImpl
	{
		public AlohaSalesItemList() {}

		private HsData data = new HsData();
		private const int TYPEADD = 1;
		private const int TYPEIDADD = 2;
		private const int TYPESUBTRACT = 3;
		private const int TYPEIDSUBTRACT = 4;
	    private const int SalesTimeTypeOpen = 0;
	    private const int SalesTimeTypeOrder = 1;
	    private const int SalesTimeTypeClose = 2;

		public override float GetSalesTotal()
		{
			float ttl = 0.0f;
			return ttl;
		}

		public override void DbLoad()
		{
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
				{	//we need to set the directory to "/Data" instead of the date string
					continue;
					/*
					logger.Log("Ok, it recognized the date as today");
					dt = "Data";
					//then we need to grind the current day's dat aon the aloha box, so that 
					//data is up-to-date for the queries
					String pathName = this.Details.Dsn + @"\bin";
					logger.Log("need to execute grind:  " + pathName);
					//run script to grind data
					System.Diagnostics.Process proc = new System.Diagnostics.Process();
					proc.EnableRaisingEvents=false;
					proc.StartInfo.WorkingDirectory = pathName;
					proc.StartInfo.FileName="grind.exe";
					proc.StartInfo.Arguments="/date Data";
					proc.Start();
					proc.WaitForExit();
					logger.Log("Executed Grind successfully");
					*/
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
						"SELECT TYPE, TYPEID, AMOUNT , CLOSEHOUR , CLOSEMIN , OPENHOUR , OPENMIN, ORDERHOUR, ORDERMIN FROM GNDSALE" + typeSelect.ToString() , newConnection );
					dataAdapter.Fill( dataSet , "GNDSALE" );
					dataAdapter.Dispose();
					DataRowCollection rows = dataSet.Tables[0].Rows;
					int index = 0;
					foreach( DataRow row in rows )
					{
						try
						{
							SalesItem item = new SalesItem();
							long tempId = Convert.ToInt64( startDate.ToString( "yyyy" ) + 
								startDate.ToString( "MM" ) + startDate.ToString( "dd" ) +
								index.ToString() );
							item.ExtId = tempId;
							item.Amount = data.GetDouble( row , "AMOUNT" );						
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

							DateTime punchDate = item.Hour < 6 ? new DateTime( startDate.Ticks ).AddDays(1) : new DateTime( startDate.Ticks );
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

		private ArrayList getSyncDates()
		{
			ArrayList aList = new ArrayList();

			DateTime startDate = DateTime.Now;
			DateTime endDate = DateTime.Now;

			if( !this.autoSync )
			{
				//SalesDateForm dateForm = new SalesDateForm();
				//dateForm.MaxDate = DateTime.Today;
				//dateForm.ShowDialog();
				startDate = HsCnxData.StartDate;//dateForm.GetSalesStartDate();
				endDate = HsCnxData.EndDate;//dateForm.GetSalesEndDate();
				
				while( startDate <= endDate )
				{
					aList.Add( startDate );
					logger.Log( "Form form: " + startDate.ToString() );
					startDate = new DateTime( startDate.Ticks ).AddDays( 1 );			
				}
					
				return aList;
			}
			else
			{
				SalesWeek salesWeek = new SalesWeek( this.Details.ClientId );
				salesWeek.Load();
				foreach( SalesDay sales in salesWeek.DayAmounts )
				{
					if( sales.Sales <= 0 )
					{
						aList.Add( sales.Date );
					}
				}
				//if the preference exists to update timers, then we need today on the list
				//of dates, because we are going to grind and query the most recent shift's data
				if(this.Details.Preferences.PrefExists( Preference.UPDATE_TIMERS ))
				{
					aList.Add( DateTime.Today.Date );
				}

				return aList;
			}
		}
	}
}
