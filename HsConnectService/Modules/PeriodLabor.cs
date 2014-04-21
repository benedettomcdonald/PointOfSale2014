using HsSharedObjects.Client;
using HsSharedObjects.Client.Module;
using HsConnect.Main;
using HsConnect.Services;
using HsConnect.Services.Wss;
using HsConnect.TimeCards;

using System;
using System.Collections;

namespace HsConnect.Modules
{
	public class PeriodLabor : ModuleImpl
	{
		public PeriodLabor()
		{
			this.logger = new SysLog( this.GetType() );
		}

		private SysLog logger;

		public override bool Execute()
		{
            int success = RemoteLogger.TIME_CARD_SUCCESS;
			
			if( Details.ModuleList.IsActive( ClientModule.PERIOD_LABOR ) )
			{
				logger.Debug( "executing Period Labor" );
				TimeCardManager timeCardManager = new TimeCardManager( this.Details );
				TimeCardList timeCardList = timeCardManager.GetPosTimeCardList();
				timeCardList.PeriodLabor = true;
				timeCardList.Details = this.Details;
				timeCardList.EndDate = DateTime.Now.Subtract( TimeSpan.FromDays( 14 ) );
				timeCardList.DbLoad();
				timeCardList.SortByDate();
				TimeCardWss timeCardService = new TimeCardWss();
				logger.Debug( "loaded["+timeCardList.Count+"] timecard items" );
                if (this.Details.PosName.Equals("Posi"))
                {
                    ArrayList splitCards = timeCardList.GetDayCards();
                    foreach (TimeCardListBlank list in splitCards)
                    {
                        Console.WriteLine("In split list, trying " + list.Count.ToString() + " cards");
                        try
                        {
                            logger.Debug(list.GetXmlString());
                            timeCardService.updateTimeCards(this.Details.ClientId, list.GetXmlString(), false, false, false);
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex.ToString());
                            Main.Run.errorList.Add(ex);
                            success = RemoteLogger.TIME_CARD_FAIL;

                        }
                    }
                }
                else if (this.Details.PosName.Equals("Micros"))
                {
                    ArrayList splitCards = timeCardList.GetDayCards();
                    foreach (TimeCardListBlank list in splitCards)
                    {
                        Console.WriteLine("In split list, trying " + list.Count.ToString() + " cards");
                        try
                        {
                            logger.Debug(list.GetXmlString());
                            if (this.Details.Preferences.PrefExists(HsSharedObjects.Client.Preferences.Preference.MICROS_CALCULATE_OVERTIME))
                            {
                                timeCardService.updateTimeCards(this.Details.ClientId, list.GetXmlString(), true, false, true);
                            }
                            else
                            {
                                timeCardService.updateTimeCards(this.Details.ClientId, list.GetXmlString(), true, false, false);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex.ToString());
                            Main.Run.errorList.Add(ex);
                            success = RemoteLogger.TIME_CARD_FAIL;

                        }
                    }
                }
                else if (this.Details.PosName.Equals("Micros9700"))
                {
                    ArrayList splitCards = timeCardList.GetDayCards();
                    foreach (TimeCardListBlank list in splitCards)
                    {
                        Console.WriteLine("In split list, trying " + list.Count.ToString() + " cards");
                        try
                        {
                            //prevent duplicate timecards in 4.0.16 when using new timecard external id hash
                            logger.Debug(list.GetXmlString());
                            timeCardService.updateTimeCards(this.Details.ClientId, list.GetXmlString(), true, false, false);
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex.ToString());
                            Main.Run.errorList.Add(ex);
                            success = RemoteLogger.TIME_CARD_FAIL;

                        }
                    }
                    
                }
                else
                {
                    logger.Debug(timeCardList.GetXmlString());
                    timeCardService.updateTimeCards(this.Details.ClientId, timeCardList.GetXmlString(), false, false, false);
                }
			}logger.Debug( "executed Period Labor" );
            RemoteLogger.Log(Details.ClientId, success);				
			return true;
		}
	}
}
