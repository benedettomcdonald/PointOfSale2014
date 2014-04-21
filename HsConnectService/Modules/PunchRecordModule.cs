using HsSharedObjects.Client;
using HsSharedObjects.Client.Module;
using HsSharedObjects.Client.CustomModule;
using HsConnect.Main;
using HsConnect.Services.Wss;
using HsConnect.TimeCards;

using System;
using System.IO;
using System.Collections;

namespace HsConnect.Modules
{
	public class PunchRecordModule : ModuleImpl
	{
		public PunchRecordModule()
		{
			this.logger = new SysLog( this.GetType() );
		}

		private SysLog logger;
        private static String drive = Data.PosiControl.Drive;

		public override bool Execute()
		{
			try
			{
				if( Details.ModuleList.IsActive( ClientModule.PUNCH_RECORDS ) )
				{
					logger.Debug( "executing PunchRecordModule" );
					Run.errorList.Clear(this.GetType().ToString());
					TimeCardManager timeCardManager = new TimeCardManager( this.Details );
					TimeCardList timeCardList = timeCardManager.GetPosTimeCardList();
					PunchRecordUtil util = new PunchRecordUtil();
					if( this.AutoSync ) timeCardList.AutoSync = true;
					logger.Debug( "Assign details" );
					timeCardList.Details = this.Details;
					logger.Debug( "OVT: " + timeCardList.Details.OvertimeRule );
					timeCardList.EndDate = DateTime.Now.Subtract( TimeSpan.FromDays(14) );
					timeCardList.PeriodLabor = true;
					timeCardList.DbLoad();
					timeCardList.SortByDate();
					util.PopulateEmpLists(timeCardList);
					PunchRecordService punchRecordService = new PunchRecordService();
					//CHECK WHICH EMPLOYEES NEED UPDATES
					int drop = timeCardList.DropCards;
                    String empXML = util.GetEmpXMLString();
                    logger.Debug("EMPXML:  " + empXML);
					String updateXML = punchRecordService.checkDropEmployeePunches(this.Details.ClientId, empXML, drop);
				    logger.Debug("UPDATEXML:  " + updateXML);
					Hashtable updateList = util.ParseUpdateXML(updateXML);

					//END CHECK

					logger.Debug( "loaded["+timeCardList.Count+"] timecard items" );
					logger.Debug( "overtime rule = " + Details.OvertimeRule );
					/*	if( this.Details.PosName.Equals( "Posi" ) )
						{
							ArrayList splitCards = timeCardList.GetDayCards();
							foreach( TimeCardListBlank list in splitCards )
							{
								Console.WriteLine( "In split list, trying " + list.Count.ToString() + " cards" );
								try
								{
									punchRecordService.updatePunchRecords( this.Details.ClientId , list.GetXmlString(updateList) );
								}
								catch( Exception ex ) 
								{
									logger.Error( ex.ToString() );
								}
							}
						}
						else
						*/
                    String finalXML = util.GetXmlString(updateList);
                    logger.Debug("FINALXML:  " + finalXML);
					punchRecordService.updatePunchRecords( this.Details.ClientId , finalXML );
					if( this.Details.PosName.Equals( "Posi" ) && this.Details.CustomModuleList.IsActive( ClientCustomModule.BJS_TIMECARD_IMPORT ))
					{
						DateTime date = ((TimeCards.PosList.PosiTimeCardList)timeCardList).CurrWeek;
						for(int x = 0; x<2; x++)
						{
							if(File.Exists(drive + @":\hstmp\BACKUP\"+DateTime.Now.Year + DateTime.Now.Month+DateTime.Now.Day+@"\PR" + date.AddDays( 6.0 ).ToString( "MMddyy" ) + ".prn"))
							{
                                FileInfo fi = new FileInfo(drive + @":\hstmp\BACKUP\" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + @"\PR" + date.AddDays(6.0).ToString("MMddyy") + ".prn");
								if(fi.LastWriteTime.CompareTo(DateTime.Now.AddMinutes(-10.0)) < 0)
									throw new Exception("The backup files were not created correctly.  Do not import punches into Posi");
							}
							else
							{
									throw new Exception("The backup files were not created correctly.  Do not import punches into Posi");
							}
							date.AddDays(-7.0);
						}
					}

				}logger.Debug( "executed PunchRecordModule" );

			}
			catch(Exception ex)
			{
				logger.Error("Punch Record Import Failed");
				logger.Error(ex.ToString());
				Run.errorList.Add(ex);
				RemoteLogger.Log( Details.ClientId, RemoteLogger.PUNCH_RECORD_SYNCH_FAIL );				
				return false;
			}
			finally
			{
				Run.errorList.Send();
			}
		PosiTimeCardImportModule posiMod = new PosiTimeCardImportModule();
		posiMod.Details = this.Details;
		SyncManager sm = new SyncManager();
		sm.SyncModule(posiMod);
		
		RemoteLogger.Log( Details.ClientId, RemoteLogger.PUNCH_RECORD_SYNCH_SUCCESS );				
		return true;
	}
	}
}
