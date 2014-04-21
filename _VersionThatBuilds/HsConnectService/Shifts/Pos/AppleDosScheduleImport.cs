using HsConnect.Shifts;
using HsConnect.Main;
using HsConnect.Data;
using HsConnect.Services;
using HsConnect.Services.Wss;
using HsSharedObjects.Client.Preferences;

using System;
using System.Collections;
using System.ServiceProcess;
using System.Data;
using System.Threading;
using System.IO;
using System.Xml;
using System.Text;
using Microsoft.Data.Odbc;

namespace HsConnect.Shifts.Pos
{
    public class AppleDosScheduleImport : ScheduleImportImpl
    {
        public AppleDosScheduleImport()
        {
            this.logger = new SysLog(this.GetType());
        }

        private SysLog logger;
        private HsData data = new HsData();
        private String OSCAR_PATH = System.Windows.Forms.Application.StartupPath + "\\OscarSCH.dat";
        private String SQL_INF_PATH = System.Windows.Forms.Application.StartupPath + "\\sql.inf";
        private String IMPORT_PATH = @"C:\network\touchit\DATA2\";
        private bool copied;

        public override void Execute()
        {
            Hashtable empHash = EmpIdHash();
            Hashtable ssnHash = GetSSNHash();

            logger.Debug("Created Hashtable.");

            ShiftList hsShifts = this.Shifts;

            logger.Log(hsShifts.Count + " shifts in shifts.");

            if (File.Exists(OSCAR_PATH))
            {
                File.Delete(OSCAR_PATH);
                System.Threading.Thread.Sleep(2000);
            }

            try
            {

                BinaryWriter binWriter =
                new BinaryWriter(File.Open(OSCAR_PATH, FileMode.Create), Encoding.ASCII);
                foreach (Shift shift in hsShifts)
                {
                    if (empHash[shift.PosEmpId.ToString()] != null)
                    {
                        logger.Log("is ssnHash[" + empHash[shift.PosEmpId.ToString()].ToString() + "] null = " +
                            (ssnHash[empHash[shift.PosEmpId.ToString()].ToString()] == null).ToString());
                    }
                    if (empHash[shift.PosEmpId.ToString()] != null &&
                        ssnHash[empHash[shift.PosEmpId.ToString()].ToString()] != null)
                    {
                        int ssn = int.Parse(ssnHash[empHash[shift.PosEmpId.ToString()].ToString()].ToString());
                        int dateIn = 11112233;
                        int timein = 1122;
                        int dateOut = 22223344;
                        int timeOut = 2233;
                        int payRate = 0;
                        int payDept = shift.PosJobId;
                        DateTime clkIn = shift.ClockIn;
                        String inDateStr = clkIn.ToString("yyyyMMdd");
                        String inTimeStr = clkIn.ToString("HHmm");
                        dateIn = int.Parse(inDateStr);
                        timein = int.Parse(inTimeStr);
                        DateTime clkOut = shift.ClockOut;
                        String outDateStr = clkOut.ToString("yyyyMMdd");
                        String outTimeStr = clkOut.ToString("HHmm");
                        dateOut = int.Parse(outDateStr);
                        timeOut = int.Parse(outTimeStr); 
                        
                        logger.Debug("Writing to OscarSCH.dat: " + ssn + "|" + dateIn + "|" + timein + "|" + dateOut + "|" +
                                     timeOut + "|" + payRate + "|" + payDept);
                        binWriter.Write(ssn);
                        binWriter.Write(dateIn);
                        binWriter.Write(timein);
                        binWriter.Write(dateOut);
                        binWriter.Write(timeOut);
                        binWriter.Write(payRate);
                        binWriter.Write(payDept);
                    }

                }
                binWriter.Close();
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            finally
            {
                if (copied && File.Exists(System.Windows.Forms.Application.StartupPath + "\\EMP28.CSV"))
                {
                    File.Delete(System.Windows.Forms.Application.StartupPath + "\\EMP28.CSV");
                }
            }
            logger.Debug("oscarsch.dat should be created in /files now.  copying to import folder");
            // Copy the SCH file to an empty dir
            if (!Directory.Exists(System.Windows.Forms.Application.StartupPath + "\\_scheds"))
            {
                Directory.CreateDirectory(System.Windows.Forms.Application.StartupPath + "\\_scheds");
            }
            File.Copy(OSCAR_PATH, System.Windows.Forms.Application.StartupPath + "\\_scheds\\" +
                DateTime.Now.ToString("MMddyyy") + ".dat", true);
            File.Copy(OSCAR_PATH, IMPORT_PATH + "OscarSCH.dat", true);
            logger.Debug(@"OscarSCH.dat should be in C:\network\touchit\DATA2\  now");
            //File.Copy( SQL_INF_PATH , IMPORT_PATH + "sql.inf" , true );
        }

        private Hashtable GetSSNHash()
        {
            Hashtable ssnHash = new Hashtable();
            StreamReader text = null;
            try
            {
                File.Copy(@"C:\network\touchit\DATA2\EXPORT\EMP28.CSV", System.Windows.Forms.Application.StartupPath + "\\EMP28.CSV");
                copied = true;
            }
            catch (Exception ex)
            {
                copied = false;
                logger.Error("Could Not Copy File:  " + ex.StackTrace);
            }

            try
            {
                try
                {
                    if (copied)
                    {
                        logger.Debug("Copied file, now opening");
                        text = File.OpenText(System.Windows.Forms.Application.StartupPath + "\\EMP28.CSV");
                    }
                    else
                    {
                        logger.Debug("Could not copy:  sleeping for 3 seconds and then opening original file");
                        Thread.Sleep(3000);
                        bool loop = true;
                        int limit = 0;
                        while (loop && limit < 60)
                        {
                            try
                            {
                                text = File.OpenText(@"C:\network\touchit\DATA2\EXPORT\EMP28.CSV");
                                logger.Debug("opened file, exiting loop");
                                loop = false;
                            }
                            catch (Exception ex)
                            {
                                logger.Debug("Could not open file yet, keep looping");
                                limit++;
                                Thread.Sleep(3000);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug("Error opening file:  here is the ERROR " + ex.ToString() + ", " + ex.StackTrace);
                    //text= File.OpenText( @"C:\network\touchit\DATA2\EXPORT\EMP28.CSV" );

                }
                logger.Log("text == null = " + (text == null).ToString());
                while (text.Peek() > 0)
                {
                    String inStr = text.ReadLine();
                    String[] strArray = inStr.Split(new char[] { ',' });

                    try
                    {
                        if (!strArray[0].Substring(0, 1).Equals("\"") &&
                            !strArray[1].Substring(0, 1).Equals("\""))
                        {
                            logger.Log(strArray[0].Trim() + " , " + strArray[1]);
                            if (!ssnHash.ContainsKey(strArray[0].Trim())) ssnHash.Add(strArray[0].Trim(), strArray[1]);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.ToString());
                    }
                }
            }
            finally
            {
                text.Close();
            }

            return ssnHash;
        }

        public override void GetWeekDates()
        {
            return;
        }

        private Hashtable EmpIdHash()
        {
            Hashtable hash = new Hashtable();
            ScheduleWss service = new ScheduleWss();
            XmlDocument doc = new XmlDocument();
            logger.Debug("client id = " + this.Details.ClientId);
            if (this.Details.Preferences.PrefExists(Preference.IGNORE_DL_ID))
            {
                foreach (Shift shift in this.shifts)
                {
                    if (!hash.ContainsKey(shift.PosEmpId + ""))
                    {
                        logger.Log(shift.PosEmpId + "" + "," + shift.PosEmpId + "");
                        hash.Add(shift.PosEmpId + "", shift.PosEmpId + "");
                    }
                }
            }
            else
            {
                String mapXml = service.getDLMappingXML(this.Details.ClientId);//INT ID HERE
                logger.Debug(mapXml);
                doc.LoadXml(mapXml);
                foreach (XmlNode node in doc.SelectNodes("/employee-id-mapping/employee"))
                {
                    if (!hash.ContainsKey(node.Attributes["dl-id"].InnerText))
                    {
                        logger.Log(node.Attributes["dl-id"].InnerText + "," + node.Attributes["pos-id"].InnerText);
                        hash.Add(node.Attributes["dl-id"].InnerText, node.Attributes["pos-id"].InnerText);
                    }
                }
            }
            return hash;
        }

        public override DateTime CurrentWeek { get { return new DateTime(0); } set { this.CurrentWeek = value; } }

    }
}
