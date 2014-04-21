using HsSharedObjects.Client;
using HsSharedObjects.Client.Module;
using HsConnect.Main;
using HsConnect.Services;
using HsConnect.Services.Wss;
using HsConnect.Shifts;

using System;
using System.Xml;
using System.IO;

namespace HsConnect.Modules
{
	public class RegClockInModule : ModuleImpl
	{
		public RegClockInModule()
		{
			this.logger = new SysLog( this.GetType() );
		}

		private SysLog logger;

		public override bool Execute()
		{
			if( Details.ModuleList.IsActive( ClientModule.REG_CLK_IN ) )
			{
                try
                {
                    logger.Log("Executing schedule import.");
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
                    ScheduleWss schedService = new ScheduleWss();

                    DateTime startDate = DateTime.Today;

                    // set up sched import
                    ScheduleImportManager importMgr = new ScheduleImportManager(this.Details);
                    ScheduleImport schedImport = importMgr.GetPosScheduleImport();
                    schedImport.Details = this.Details;

                    // posi needs this date
                    if (this.Details.PosName.Equals("Posi"))
                    {
                        schedImport.GetWeekDates();
                        startDate = schedImport.CurrentWeek;
                    }

                    XmlDocument doc = new XmlDocument();

                   

                    int outDays = 6;    // Changed default date range to import schedules
                    if (this.Details.PosName.Equals("Micros9700")) outDays = 2;
                    if (this.Details.PosName.Equals("Posi")) outDays = 6;
                    if (this.Details.PosName.Equals("Aloha")) outDays = 6;
                    if (this.Details.PosName.Equals("Micros")) outDays = 6;
                    if (this.Details.PosName.Equals("OnePOS")) outDays = 13;

                    if (Details.Preferences.PrefExists(1018))
                    {
                        try
                        {
                            HsSharedObjects.Client.Preferences.Preference pref
                                    = this.Details.Preferences.GetPreferenceById(1018);
                            if (pref != null)
                            {
                                int days = Int32.Parse(pref.Val2);
                                if (days > 0)
                                    outDays = days;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex.ToString());
                            outDays = 6;    // Changed default date range to import schedules
                            if (this.Details.PosName.Equals("Micros9700")) outDays = 2;
                            if (this.Details.PosName.Equals("Posi")) outDays = 6;
                            if (this.Details.PosName.Equals("Aloha")) outDays = 6;
                            if (this.Details.PosName.Equals("Micros")) outDays = 6;
                            if (this.Details.PosName.Equals("OnePOS")) outDays = 13;
                        }
                    }
                    bool alohaCopyScheduleFile = Details.Preferences.PrefExists(HsSharedObjects.Client.Preferences.Preference.ALOHA_COPY_SCHEDULE_FILE)
                        && this.Details.PosName.Equals("Aloha");//aloha Lazy Dog extra copy of schedule files
                    bool importUnpostedShifts = Details.Preferences.PrefExists(HsSharedObjects.Client.Preferences.Preference.SCHEDULE_IMPORT_INCLUDE_UNPOSTED_SHIFTS);
                    if (alohaCopyScheduleFile )
                    {
                        try
                        {
                            while (startDate.DayOfWeek != Details.DayOfWeekStart)
                            {
                                startDate = startDate.AddDays(-1);
                            }
                            /*Guarantee at least 2 weeks here, but if the configured
                             * value from pref_1018 is greater, use that instead
                             */
                            if (outDays < 13)
                            {
                                outDays = 13;//two weeks out
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex.ToString());
                            outDays = 6;    // Changed default date range to import schedules
                        }
                    }
                    
                    XmlElement start = doc.CreateElement("start-date");
                    start.SetAttribute("day", startDate.Day + "");
                    start.SetAttribute("month", startDate.Month + "");
                    start.SetAttribute("year", startDate.Year + "");

                    XmlElement end = doc.CreateElement("end-date");
                    end.SetAttribute("day", startDate.AddDays(outDays).Day + "");
                    end.SetAttribute("month", startDate.AddDays(outDays).Month + "");
                    end.SetAttribute("year", startDate.AddDays(outDays).Year + "");

                    logger.Log("Getting shifts [ " + startDate.ToString("MM/dd/yyyy") + " - " +
                        startDate.AddDays(outDays).ToString("MM/dd/yyyy") + " ] from HS.");
                    String schedXml = "";
                    if (alohaCopyScheduleFile || importUnpostedShifts)
                    {
                        schedXml = schedService.getFullSchedulesXML(this.Details.ClientId, start.OuterXml, end.OuterXml);
                    }
                    else
                    {
                        schedXml = schedService.getSchedulesXML(this.Details.ClientId, start.OuterXml, end.OuterXml);
                    }
                    
                    logger.Debug("ScheduleWss returned the following xml:\r\n" + schedXml);
                    if (this.Details.PosName.Equals("DigitalDining"))
                    {
                        schedImport.XmlString = schedXml;
                    }
                    else
                    {
                        ShiftList shiftList = new ShiftList();
                        if (Details.PosName.ToLower().Equals("everserv"))
                        {
                            shiftList.ImportScheduleXml(false, true, schedXml);
                        }
                        else
                        {
                            shiftList.ImportScheduleXml(false, false, schedXml);
                        }
                        logger.Debug(schedXml);
                        logger.Log("Downloaded " + shiftList.Count + " shifts.");
                        schedImport.Shifts = shiftList;
                    }
                    // run sched import
                    if (this.AutoSync) schedImport.AutoSync = true;

                    schedImport.Execute();
                    logger.Log("Imported schedule into the POS.");
                    if (Details.PosName.Equals("AppleDos") && Details.Preferences.PrefExists(1022))
                    {
                        logger.Debug("deleting EXPORT files");
                        foreach (String f in Directory.GetFiles(@"C:\network\touchit\data2\export\", "*.*"))
                        {
                            File.Delete(f);
                        }
                    }
                    RemoteLogger.Log(Details.ClientId, RemoteLogger.IMPORT_SCHEDULE_SUCCESS);
                }
                catch (Exception e)
                {
                    logger.Error("Error importing schedule", e);
                    throw;
                }		
			}
			return true;
		}
	}
}
