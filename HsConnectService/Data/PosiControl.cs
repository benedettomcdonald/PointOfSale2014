using HsConnect.Main;
using HsProperties;
using System;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using System.Net;
using HsSharedObjects;
using System.Collections;
using System.Data;
using HsConnect.Services.Wss;
using System.Security.Cryptography;
using HsSharedObjects.Client.Preferences;

namespace HsConnect.Data
{
    public class PosiControl
    {
        public PosiControl() { }

        public static int TAW_EXPORT = 0;
        public static int POSIDBF_TODAY = 1;
        public static int TARW_TODAY = 2;
        public static int TAXML = 3;
        public static int TAXML_NO_ISC = 4;
        private static String drive = (Main.Run.Mapped ? "S" : "C");
        private static SysLog logger = new SysLog("HsConnect.Data.PosiControl");

        public static void Run(int cmdCode)
        {

            Run(cmdCode, "", "");
        }

        public static void Run(String cmd, String args)
        {
            Run(99, cmd, args);
        }

        public static void Run(int cmdCode, String command, String arg)
        {
            //SysLog logger = new SysLog( "HsConnect.Data.PosiControl" );
            String cmd = "";
            String args = "";
            try
            {
                switch (cmdCode)
                {
                    case 0:
                        cmd = "TAW";
                        args = "EXPORTALL /ALT";
                        break;
                    case 1:
                        String today = DateTime.Now.ToString("MM/dd/yy");
                        cmd = "POSIDBF";
                        args = "/ALT " + today;
                        break;
                    case 2:
                        cmd = "TARW";
                        args = "-R 52 1 0";
                        break;
                    case 3:
                        cmd = "TAXML";
                        args = "ADDEMPLXML";
                        break;
                    case 4:
                        cmd = "TAXML";
                        args = "ADDEMPLXML NO_IMMED";
                        break;
                    case 99:
                        cmd = command;
                        args = arg;
                        break;
                }

                try
                {
                    //report taxml chksum
                    if (cmd.ToLower().StartsWith("taxml"))
                    {
                        logger.Debug("calculating taxml chksum");
                        ClientSettingsWss settingsService = new ClientSettingsWss();

                        //replace with taxml
                        string chkfile = Drive + @":\SC" + @"\TAXML.EXE";
                        String result = GetChecksum(chkfile);
                        logger.Debug("found chksum for " + chkfile + " : " + result);

                        settingsService.reportTaxmlChecksum(HsCnxData.Details.ClientId, result);
                    }
                }
                catch (Exception e)
                {
                    //eat the exception here so as not to impact posi command execution
                    logger.Error("Error when attempting to find taxml chksum", e);
                }

                if (!Application.ProductName.Equals("DEBUG"))
                {
                    //writeToPos( cmd, args );
                    logger.Debug("wait in line");
                    waitInLine();
                    logger.Debug("waited in line");
                    System.Diagnostics.Process proc = new System.Diagnostics.Process();
                    proc.EnableRaisingEvents = false;
                    proc.StartInfo.WorkingDirectory = Drive + @":\SC";
                    logger.Debug("Set working directory to " + proc.StartInfo.WorkingDirectory);
                    proc.StartInfo.FileName = cmd;
                    proc.StartInfo.Arguments = args;
                    /*
                    proc.StartInfo.WorkingDirectory = @"L:\SC\";
                    */
                    logger.Debug("is mapped?");
                    if (Main.Run.Mapped)
                    {
                        logger.Debug("its mapped");
                        if (File.Exists(@"C:\WINDOWS\system32\cmd.exe"))
                        {
                            logger.Debug("WINDOWS");
                            proc.StartInfo.FileName = @"C:\WINDOWS\system32\cmd.exe";
                        }
                        else
                        {
                            logger.Debug("WINNT");
                            proc.StartInfo.FileName = @"C:\WINNT\system32\cmd.exe";
                        }
                        logger.Debug("setting to " + "/c \"" + cmd + " " + args + "\"");
                        proc.StartInfo.Arguments = "/c \"" + cmd + " " + args + "\"";
                    }
                    logger.Debug("Running: \"" + proc.StartInfo.WorkingDirectory + " " + proc.StartInfo.FileName + " " + proc.StartInfo.Arguments);
                    proc.Start();
                    logger.Debug("ran start");
                    proc.WaitForExit();
                    logger.Debug("exited process");

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }

        }

        private static String GetChecksum(string filePath)
        {
            using (BufferedStream stream = new BufferedStream(File.OpenRead(filePath), 1200000))
            {
                SHA256Managed sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
        }

        /*
         * @Param - reverseLookup 
         * True - map from alt_num -> emp_num
         * False - map from emp_num -> alt_num
         */
        public static Hashtable MapEmployees(bool reverseLookup)
        {
            Hashtable emps = new Hashtable();
            try
            {
                //Run employee data export and send DBF files to alt folder
                Run(TAW_EXPORT);

                HsData data = new HsData();
                DataTableBuilder builder = new DataTableBuilder();
                DataTable dt = builder.GetTableFromDBF(Drive + @":\ALTDBF", @"C:\", "EMPFILE");
                DataRowCollection rows = dt.Rows;
                foreach (DataRow row in rows)
                {
                    try
                    {
                        int empNum = data.GetInt(row, "EMP_NUMBER");
                        int altNum = data.GetInt(row, "ALT_NUM");
                        if (reverseLookup)
                        {
                            if (!emps.ContainsKey(altNum))
                            {
                                //reverse lookup: map from altNum -> empNum
                                emps.Add(altNum + "", empNum + "");
                            }
                        }
                        else
                        {
                            if (!emps.ContainsKey(empNum))
                            {
                                //standard lookup: map from empNum -> altNum
                                emps.Add(empNum + "", altNum + "");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.ToString());
                        Main.Run.errorList.Add(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                Main.Run.errorList.Add(ex);
            }
            return emps;
        }

        public static void AdvanceSchedule()
        {
            //SysLog logger = new SysLog( "HsConnect.Data.PosiControl" );
            try
            {
                if (!Application.ProductName.Equals("DEBUG"))
                {
                    bool existed = File.Exists(@"L:\SC\TA.OPN");

                    System.Diagnostics.Process proc = new System.Diagnostics.Process();
                    proc.EnableRaisingEvents = false;
                    proc.StartInfo.WorkingDirectory = @"L:\SC";
                    proc.StartInfo.FileName = "TAW";
                    proc.StartInfo.Arguments = "";
                    proc.Start();
                    Thread.Sleep(15000);

                    // kill process
                    System.Diagnostics.Process kill = new System.Diagnostics.Process();
                    kill.EnableRaisingEvents = false;
                    kill.StartInfo.WorkingDirectory = @"L:\SC";
                    kill.StartInfo.FileName = "PSKILL";
                    kill.StartInfo.Arguments = "TAW";
                    kill.Start();
                    if (!existed) File.Delete(@"L:\SC\TA.OPN");

                    proc = new System.Diagnostics.Process();
                    proc.EnableRaisingEvents = false;
                    proc.StartInfo.WorkingDirectory = @"L:\SC";
                    proc.StartInfo.FileName = "TAW";
                    proc.StartInfo.Arguments = "EXPORT";
                    proc.Start();
                    proc.WaitForExit();

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        private static void waitInLine()
        {
            int numwaits = 0;
            while (File.Exists(Drive + @":\SC\TA.OPN"))
            {
                if (numwaits % 10 == 0)
                {
                    logger.Log("TA.OPN present.  Waiting for TAXML to close.  Waited " + (numwaits * 30) + "seconds.");
                }
                numwaits++;
                Thread.Sleep(30000);
            }
            if (numwaits > 0)
            {
                logger.Log("Finished waiting for TAXML.  TA.OPN no longer present.  Waited " + (numwaits * 30) + "seconds");
            }
        }

        // this will write to hscmd on the POSDriver as CMD%ARGS
        private static void writeToPos(String cmd, String args)
        {
            StreamWriter writer = null;
            try
            {
                if (!Directory.Exists(Drive + @":\HS")) Directory.CreateDirectory(Drive + @":\HS");
                if (File.Exists(Drive + @":\HS\hscmd.hs")) File.Delete(Drive + @":\HS\hscmd.hs");
                writer = File.CreateText(Drive + @":\HS\hscmd.hs");
                //if( !Directory.Exists( @"L:\HS" ) ) Directory.CreateDirectory( @"L:\HS" );
                //if( File.Exists( @"L:\HS\hscmd.hs" ) ) File.Delete( @"L:\HS\hscmd.hs" );
                //writer = File.CreateText( @"L:\HS\hscmd.hs" );
                writer.WriteLine(cmd + "%" + args);
            }
            catch (Exception ex)
            {

            }
            finally
            {
                writer.Flush();
                writer.Close();
            }
        }

        public static String Drive
        {
            get
            {
                if (Main.Run.Mapped)
                {
                    if (HsCnxData.Details.Preferences.PrefExists(Preference.POSI_CUSTOM_DRIVE_MAPPING))
                    {
                        Preference pref = HsCnxData.Details.Preferences.GetPreferenceById(Preference.POSI_CUSTOM_DRIVE_MAPPING);
                        if(pref.Val2 != null)
                            drive = pref.Val2.Substring(0,1);
                    }
                    return drive;
                }
                if (HsCnxData.Details.Dsn == null || HsCnxData.Details.Dsn.Length <= 0)
                {
                    return drive;
                }
                else
                    return HsCnxData.Details.Dsn.Substring(0, 1);
            }
            set { drive = value; }
        }

        #region OSI
        public static bool MapDrive()
        {
            String path = System.Windows.Forms.Application.StartupPath + @"\properties.xml";
            Properties p = new Properties(path);
            SysLog log = new SysLog("PosiControl");
            String ip = "";
            if (p.Suffix.IndexOf(".") > 0)
            {
                ip = p.Suffix;//if suffix is x.x.x.x, then use full string as IP
            }
            else
            {
                ip = getDynamicIP(p.Suffix); // otherwise, treat as suffix
            }
            log.Log("connecting to: " + ip);

            String usr = HsCnxData.Details.DbUser;
            String pwd = HsCnxData.Details.DbPassword;
            String remoteDrive = @"c";
            if (HsCnxData.Details.Preferences.PrefExists(1057))
            {
                log.Debug("setting remote drive");
                Preference pref = HsCnxData.Details.Preferences.GetPreferenceById(1057);
                if (pref.Val2 != null)
                    remoteDrive = pref.Val2;
            }

            log.Debug(@"use  " + Drive + @": \\" + ip + @"\" + remoteDrive + @" " + pwd + @" /user:" + usr + @" /persistent:no");
            System.Diagnostics.Process.Start("net.exe", @"use  " + Drive + @": \\" + ip + @"\"+remoteDrive + @" " + pwd + @" /user:" + usr + @" /persistent:no");

            Thread.Sleep(15000);
            if (System.IO.Directory.Exists(Drive + @":\"))
            {
                log.Log("Mapping succeeded");
                return true;
            }
            else
            {
                log.Error("Mapping failed");
                RemoteLogger.Log(HsCnxData.Details.ClientId, RemoteLogger.DRIVE_MAP_FAIL);
                return false;
            }
        }

        public static void CloseMap()
        {
            System.Diagnostics.Process.Start("net.exe", @"use  " + Drive + @": /delete");
            Thread.Sleep(3000);
        }

        private static String getDynamicIP(String suffix)
        {
            //CCF SPECIFIC CODE TO DYNAMICALLY CREATE IP OF REMOTE MACHINE
            String dynamicIP = "";
            String[] split = new String[4];
            try
            {
                String strHostName = "";
                strHostName = Dns.GetHostName();
                Console.WriteLine("Local Machine's Host Name: " + strHostName);

                IPHostEntry ipEntry = Dns.GetHostByName(strHostName);
                IPAddress[] addr = ipEntry.AddressList;
                char[] splitter = { '.' };
                split = addr[0].ToString().Split(splitter, 4);
                dynamicIP = split[0] + "." + split[1] + "." + split[2] + "." + suffix;
            }
            catch (Exception ex)
            {
            }
            //END CCF SPECIFIC CODE
            return dynamicIP;
        }
        #endregion

    }
}
