using HsSharedObjects.Client;
using HsSharedObjects.Client.Module;
using HsConnect.Main;
using HsConnect.Services;
using HsConnect.Services.Wss;
using HsConnect.SalesItems;
using HsConnect.Data;

using System;
using HsSharedObjects.Client.Preferences;

namespace HsConnect.Modules
{
	public class NetSalesModule : ModuleImpl
	{
		public NetSalesModule()
		{
			this.logger = new SysLog( this.GetType() );
		}

		private SysLog logger;

		public override bool Execute()
		{
			if( Details.ModuleList.IsActive( ClientModule.NET_SALES ) )
			{
				logger.Debug( "executing NetSalesModule" );
				ClientSalesWss salesService = new ClientSalesWss();
				SalesStartDate lastSale = new SalesStartDate();
				lastSale.LoadFromXmlString( salesService.getSalesEndDate( Details.ClientId ) );
				
				SalesItemsManager salesManager = new SalesItemsManager( this.Details );
				SalesItemList salesList = salesManager.GetPosSalesItemList();
				salesList.Details = this.Details;
                bool adjustDateRange = Details.Preferences.PrefExists(Preference.ADJUST_SALES_DATE_RANGE);
				if( this.Details.PosName.Equals( "Micros" ) )
				{
                    int daysBack = -15;
                    if (adjustDateRange)
                    {
                        logger.Debug("Enter Micros Adjust Sales Date Range block.");
                        try
                        {
                            Preference pref = Details.Preferences.GetPreferenceById(Preference.ADJUST_SALES_DATE_RANGE);
                            logger.Debug("Going to parse pref.Val2 shortly. Is pref null? " + (pref == null));
                            logger.Debug("Val2 is the following: " + pref.Val2);
                            daysBack = (Math.Abs(Int32.Parse(pref.Val2))) * -1;
                            logger.Debug("Using custom date range.  Going " + daysBack +" days back.");
                        }
                        catch (Exception ex)
                        {
                            logger.Error("error reading date range preference, reverting to default");
                            logger.Error(ex.ToString());
                            daysBack = -15;
                        }
                    }
                    salesList.StartDate = DateTime.Today.AddDays(daysBack);
				    salesList.EndDate = DateTime.Now;


					logger.Debug( "got start date: " + salesList.StartDate.ToShortDateString() );
					logger.Debug( "got end date: " + salesList.EndDate.ToShortDateString() );
                    salesList.StartDate = salesList.StartDate.Date;
					salesList.Dob = DateTime.Now.Subtract( new TimeSpan( 1 , 0 , 0 , 0 , 0 ) );
				}
				logger.Debug( "loading sales list" );
				if( this.AutoSync ) salesList.AutoSync = true;
				if( this.Details.PosName.Equals( "Posi" ) )
				{
					if( lastSale.IsNull )
					{
						salesList.EndDate = DateTime.Now.AddDays( -1.0 );
					} else salesList.EndDate = lastSale.StartDate;
					logger.Debug( "sales end date: " + salesList.EndDate.ToString() );
				}
				//For Micros 9700, we need to set the query period, based on the start date
				if(this.Details.PosName.Equals( "Micros9700" ))
				{
					salesList.StartDate = lastSale.StartDate;
					Micros9700Control.setPeriod(salesList.StartDate, DateTime.Today, salesList.Details.WorkWeekStart-1);
					
				}
				salesList.DbLoad();
				logger.Debug( "loaded sales list: " + salesList.Count );
//				if( this.Details.PosName.Equals( "Micros" ) )
//				{
//					while( salesList.Count <= 0 && salesList.EndDate < DateTime.Now )
//					{
//						logger.Debug( "reloading sales list" );
//						salesList.EndDate = salesList.EndDate.AddDays( 1 );
//						salesList.DbLoad();
//					}		
//				}
				//For Micros 9700, if we queried for the period of last week, we need to get the week-to-date as well
				if(this.Details.PosName.Equals( "Micros9700" ) 
					&& Micros9700Control.PERIOD == Micros9700Control.PREV_WEEK)
				{
					Micros9700Control.setPeriod(Micros9700Control.WEEK_TO_DATE);
					salesList.DbLoad();
				}
				
				int rows = 0;
                logger.Debug("salesList.Count = " + salesList.Count);
				if( salesList.Count > 0 )
				{
					logger.Debug("SALESXML: " + salesList.GetXmlString() );

                    bool microsByRvc = Details.Preferences.PrefExists(Preference.MICROS_BY_RVC);
                    if (!microsByRvc)
					rows = salesService.insertSalesItems( Details.ClientId , salesList.GetXmlString() );
                    else
                        rows = salesService.insertSalesItemsWithMicrosRVC(Details.ClientId, salesList.GetXmlString());

					logger.Debug( "inserted["+rows+"] sales items" );
				}
                logger.Debug("executed NetSalesModule");
                RemoteLogger.Log(Details.ClientId, RemoteLogger.SALES_SUCCESS);
			}
				
			return true;
		}
	}
}
