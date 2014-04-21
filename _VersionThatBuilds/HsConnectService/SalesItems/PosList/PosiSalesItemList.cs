using HsConnect.Data;
using HsConnect.SalesItems;
using HsConnect.Forms;

using System;
using System.Data;
using System.Collections;
using System.Globalization;
using Microsoft.Data.Odbc;

using HsSharedObjects.Client.Preferences;

namespace HsConnect.SalesItems.PosList
{
	public class PosiSalesItemList : SalesItemListImpl
	{
		public PosiSalesItemList() {}

		private HsData data = new HsData();
		private static String drive = Data.PosiControl.Drive;

		public override float GetSalesTotal()
		{
			String yest = DateTime.Today.AddDays( -1.0 ).ToString( "MM/dd/yy" );
			float ttl = 0.0f;
			try
			{
				PosiControl.Run( "POSIDBF","/ALT " + yest );
				DataTableBuilder builder = new DataTableBuilder();
				//DataTable dt = builder.GetTableFromDBF( @"C:\DBF", @"C:\", "HRSALES" );
				DataTable dt = builder.GetTableFromDBF(drive + @":\ALTDBF", @"C:\", "HRSALES" );
				DataRowCollection rows = dt.Rows;
				foreach( DataRow row in rows )
				{
					try
					{
						if( Convert.ToInt32( row["COST_CENTR"].ToString() ) == 0 )
						{
							SalesItem item = new SalesItem();

							int tempId = Convert.ToInt32( row["DATE"].ToString() + row["HOUR"].ToString() );
							item.ExtId = tempId;
							item.Amount = data.GetDouble( row , "TOT_SALES" );

							string date = row["DATE"].ToString();
							DateTimeFormatInfo myDTFI = new CultureInfo( "en-US", false ).DateTimeFormat;
							DateTime nDate = DateTime.ParseExact( date, "yyyyMMdd", myDTFI );
						
							if( nDate == DateTime.Today ) ttl += (float) item.Amount;
						}
					}
					catch( Exception ex )
					{
						logger.Error( "Error adding posi sales item in Load(): " + ex.ToString() );
					}
				}
			}
			catch( Exception ex )
			{
				logger.Error( ex.ToString() );
				Main.Run.errorList.Add(ex);
			}

			return ttl;
		}

		public override void DbLoad()
		{
			Hashtable idHash = new Hashtable();
			DateTime sDate = DateTime.Now.AddDays( -5.0 );
			for(int i=0; i<5; i++ )
			{
				DateTime syncDate = sDate.AddDays( i );
				String dateStr = syncDate.ToString( "MM/dd/yy" );
				try
				{
					PosiControl.Run( "POSIDBF","/ALT " + dateStr );
					DataTableBuilder builder = new DataTableBuilder();
					//BRANCHES HERE FOR M&E
					if(this.Details.Preferences.PrefExists(Preference.ALT_POSI_SALES))
					{
						DataTable dt = builder.GetTableFromDBF(drive + @":\ALTDBF", @"C:\", "JOURNAL" );
						//DataTable dt = builder.GetTableFromDBF( @"L:\DBF", @"C:\", "JOURNAL" );
						DataRowCollection rows = dt.Rows;
						SalesItem item = new SalesItem();
						
						foreach( DataRow row in rows )
						{
							try
							{
								string date = row["DATE"].ToString();
								DateTimeFormatInfo myDTFI = new CultureInfo( "en-US", false ).DateTimeFormat;
								DateTime nDate = DateTime.ParseExact( date, "yyyyMMdd", myDTFI );

								if((nDate.Date.CompareTo(syncDate.Date) == 0) && Convert.ToInt32( row["COST_CENTR"].ToString() ) == 0 )
								{
									int acctNum = Convert.ToInt32( row["ACCT_POSI"].ToString());
									switch(acctNum)
									{
										case 0://FOOD-SALES
											String str = row["CREDIT"].ToString();
											double amt = Convert.ToDouble(str);
											item.Amount += amt;
											break;
										case 1://LIQOUR-SALES
											item.Amount += Convert.ToDouble( row["CREDIT"].ToString());
											break;
										case 2://BEER
											item.Amount += Convert.ToDouble( row["CREDIT"].ToString());
											break;
										case 3://WINE
											item.Amount += Convert.ToDouble( row["CREDIT"].ToString());
											break;
										case 4://SODA
											item.Amount += Convert.ToDouble( row["CREDIT"].ToString());
											break;
										case 8://COUPONS
											item.Amount -= Convert.ToDouble( row["DEBIT"].ToString());
											break;
										case 9://BEV
											item.Amount += Convert.ToDouble( row["CREDIT"].ToString());
											break;
										case 150://S&P FOOD
											item.Amount -= Convert.ToDouble( row["DEBIT"].ToString());
											break;
										case 151://S&P BEV
											item.Amount -= Convert.ToDouble( row["DEBIT"].ToString());
											break;
										case 1301://REDEEM REWARDS
											item.Amount -= Convert.ToDouble( row["DEBIT"].ToString());
											break;
									}
								}
							}
							catch( Exception ex )
							{
								logger.Error( "Error adding posi sales item in Load(): " + ex.ToString() );
							}
						}
						int tempId = Convert.ToInt32( syncDate.ToString( "yyyyMMdd"));
						item.ExtId = tempId;
						
						item.DayOfMonth = syncDate.Day;
						item.Month = syncDate.Month;
						item.Year = syncDate.Year;
						item.Hour = 12;
						item.Minute = 0;
						if( !idHash.ContainsKey( item.ExtId + "" ) )
						{
							this.Add( item );
							idHash.Add( item.ExtId + "", item );
						}
					}
					else
					{
						DataTable dt = builder.GetTableFromDBF(drive + @":\ALTDBF", @"C:\", "HRSALES" );
						//DataTable dt = builder.GetTableFromDBF( @"L:\DBF", @"C:\", "HRSALES" );
						DataRowCollection rows = dt.Rows;
						foreach( DataRow row in rows )
						{
							try
							{
								if( Convert.ToInt32( row["COST_CENTR"].ToString() ) == 0 )
								{
									SalesItem item = new SalesItem();

									int tempId = Convert.ToInt32( row["DATE"].ToString() + row["HOUR"].ToString() );
									item.ExtId = tempId;
									item.Amount = data.GetDouble( row , "TOT_SALES" );

									string date = row["DATE"].ToString();
									DateTimeFormatInfo myDTFI = new CultureInfo( "en-US", false ).DateTimeFormat;
									DateTime nDate = DateTime.ParseExact( date, "yyyyMMdd", myDTFI );
									TimeSpan diff = DateTime.Now.Subtract(nDate);
									if(diff.Days > 14)
										continue;
									TimeSpan time = GetDateTime( Convert.ToInt32( row["HOUR"].ToString() ) );

									if( time.Hours == 24 || time.Hours < 6 ) nDate = nDate.AddDays( 1 );

									item.DayOfMonth = nDate.Day;
									item.Month = nDate.Month;
									item.Year = nDate.Year;

									item.Hour = time.Hours;
									item.Minute = time.Minutes;
								
									if( !idHash.ContainsKey( item.ExtId + "" ) )
									{
										this.Add( item );
										idHash.Add( item.ExtId + "", item );
									}
								}
							}
							catch( Exception ex )
							{
								logger.Error( "Error adding posi sales item in Load(): " + ex.ToString() );
							}
						}
					}
				}
				catch( Exception ex )
				{
					logger.Error( ex.ToString() );
					Main.Run.errorList.Add(ex);
				}
			}		
		}
		public override void DbUpdate(){}
		public override void DbInsert(){}

		private TimeSpan GetDateTime( int hr )
		{
			if( hr % 2 == 0 ) return new TimeSpan(  hr / 2, 0, 0 );
			return new TimeSpan( hr/2, 30, 0 );
		}
	}
}
