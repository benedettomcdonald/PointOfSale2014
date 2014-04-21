using HsSharedObjects.Client;
using HsSharedObjects.Client.Module;
using HsConnect.Main;
using HsConnect.Services;
using HsConnect.Services.Wss;
using HsConnect.TimeCards;

using System;
using System.IO;
using System.Collections;

namespace HsConnect.Modules
{
	public class LaborItemModule : ModuleImpl
	{
		public LaborItemModule()
		{
			this.logger = new SysLog( this.GetType() );
		}

		private SysLog logger;

		public override bool Execute()
		{
			
			if( Details.ModuleList.IsActive( ClientModule.LABOR_ITEMS ) )
			{
				logger.Debug( "executing LaborItemModule" );
                if (Details.PosName.Equals("AppleDos") && Details.Preferences.PrefExists(1022))
                {
                    logger.Debug("Apple DOS expbin run");
                    System.Diagnostics.Process proc = new System.Diagnostics.Process();
                    proc.EnableRaisingEvents = false;
                    proc.StartInfo.WorkingDirectory = @"C:\network\touchit\data2\";
                    proc.StartInfo.FileName = @"expbin.exe";
                    proc.Start();
                    proc.WaitForExit();
                    logger.Debug("exited EXPBIN.exe process");
                }
				TimeCardManager timeCardManager = new TimeCardManager( this.Details );
				TimeCardList timeCardList = timeCardManager.GetPosTimeCardList();
				if( this.AutoSync ) timeCardList.AutoSync = true;
				logger.Debug( "Assign details" );
				timeCardList.Details = this.Details;
				logger.Debug( "OVT: " + timeCardList.Details.OvertimeRule );
				timeCardList.EndDate = DateTime.Now.Subtract( TimeSpan.FromHours( 48 ) );
				timeCardList.DbLoad();
				timeCardList.SortByDate();
				TimeCardWss timeCardService = new TimeCardWss();
			    String timecardListXml = timeCardList.GetXmlString();
				logger.Debug("Loaded["+timeCardList.Count+"] timecard items" );
                logger.Debug("POS Timecard XML: " + timecardListXml);
				if( this.Details.PosName.Equals( "Posi" ) )
				{
                    logger.Debug("Executing POSi Timecard Sync");
					ArrayList splitCards = timeCardList.GetDayCards();
					foreach( TimeCardListBlank list in splitCards )
					{
						Console.WriteLine( "In split list, trying " + list.Count.ToString() + " cards" );
						try
						{
							timeCardService.updateTimeCards( this.Details.ClientId , list.GetXmlString(), false, false, false );
						}
						catch( Exception ex ) 
						{
							logger.Error( ex.ToString() );
							Main.Run.errorList.Add(ex);
						}
					}
				}
                else if (this.Details.PosName.Equals("AppleDos") || this.Details.PosName.Equals("AppleOne")) 
				{
                    logger.Debug("Executing Apple Timecard Sync");
                    ArrayList splitCards = timeCardList.GetDayCards();
                    foreach (TimeCardListBlank list in splitCards)
                    {
                        Console.WriteLine("In split list, trying " + list.Count.ToString() + " cards");
                        try
                        {
                            timeCardService.updateDropTimeCards(this.Details.ClientId, list.GetXmlString(), true, true);
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex.ToString());
                            Main.Run.errorList.Add(ex);
                        }
                    }
				}
                else if  (this.Details.PosName.Equals("Micros"))
                {
                    if (this.Details.Preferences.PrefExists(HsSharedObjects.Client.Preferences.Preference.MICROS_CALCULATE_OVERTIME))
                    {
                        logger.Debug("Executing Micros Timecard Sync WITH Calculate Overtime preference");
                        timeCardService.updateTimeCards(this.Details.ClientId, timecardListXml, true, false, true);
                    }
                    else
                    {
                        logger.Debug("Executing Micros Timecard Sync WITHOUT Calculate Overtime preference");
                        timeCardService.updateDropTimeCards(this.Details.ClientId, timecardListXml, true, false);
                    }
                }
                else if (this.Details.PosName.Equals("Micros9700"))
                {
                    logger.Debug("Executing Micros9700 Timecard Sync");
                    //prevent duplicate timecards in 4.0.16 when using new timecard external id hash
                    timeCardService.updateTimeCards(this.Details.ClientId, timecardListXml, true, false, false);
                }
                else
                {
                    logger.Debug("Executing DEFAULT Timecard Sync");
                    timeCardService.updateTimeCards(this.Details.ClientId, timecardListXml, false, false, false);
                }
                if (Details.PosName.Equals("AppleDos") && Details.Preferences.PrefExists(1022))
                {
                    logger.Debug("deleting EXPORT files");
                    foreach (String f in Directory.GetFiles(@"C:\network\touchit\data2\export\", "*.*"))
                    {
                        File.Delete(f);
                    }
                }
                RemoteLogger.Log(Details.ClientId, RemoteLogger.TIME_CARD_SUCCESS);
                logger.Debug("executed LaborItemModule");

			}
			return true;
		}
	}
}
