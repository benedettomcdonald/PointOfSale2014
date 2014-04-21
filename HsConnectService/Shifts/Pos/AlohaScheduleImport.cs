using System.Data.SqlClient;
using HsConnect.Shifts;
using HsConnect.Main;
using HsConnect.Data;

using System;
using System.Collections;
using System.Data;
using System.IO;
using Microsoft.Data.Odbc;

//using HsConnect.Services;
using HsConnect.Services.Wss;
using HsSharedObjects.Client.Preferences;

using System.Xml;
using System.Threading;

namespace HsConnect.Shifts.Pos
{
    public class AlohaScheduleImport : ScheduleImportImpl
    {
        public AlohaScheduleImport()
        {
            this.logger = new SysLog(this.GetType());
        }

        private SysLog logger;
        private HsData data = new HsData();

        public override void Execute()
        {
            HsFile hsFile = new HsFile();
            hsFile.Copy(this.Cnx.Dsn + @"\NEWDATA", this.Cnx.Dsn + @"\hstmp", "Aloha.ini");
            String empCnxStr = this.cnx.ConnectionString + @"\hstmp";
            OdbcConnection newConnection = this.cnx.GetCustomOdbc(empCnxStr);
            StreamReader reader = null;
            bool copyFile = Details.Preferences.PrefExists(Preference.ALOHA_COPY_SCHEDULE_FILE);//aloha Lazy Dog extra copy of schedule files;
            string copyLocation = "";
            if (copyFile)
            {
                HsSharedObjects.Client.Preferences.Preference pref
                                  = this.Details.Preferences.GetPreferenceById(Preference.ALOHA_COPY_SCHEDULE_FILE);
                copyLocation = pref.Val2;
                if (copyLocation.EndsWith(@"\"))
                {
                    copyLocation = copyLocation.Substring(0, copyLocation.Length - 1);
                }
            }

            try
            {
                reader = new StreamReader(this.Cnx.Dsn + @"\hstmp\Aloha.ini");
                String fullFile = reader.ReadToEnd();
                int dayOfWeek = Convert.ToInt32(fullFile.Substring(fullFile.IndexOf("PAYPERIODDAY=") + 13, 1));
                DateTime tempDate = DateTime.Today;
                int numConnections = 0;
                if (dayOfWeek < 0) throw new Exception("Business start day of week error");
                while (String.Compare(GetDayOfWeek(dayOfWeek), tempDate.DayOfWeek.ToString(), true) != 0)
                {
                    tempDate = tempDate.AddDays(-1.0);
                }
                logger.Debug("opening " + empCnxStr);
                newConnection.Open();
                logger.Debug("connection opened");

                String tblName = createSCHFile(newConnection, tempDate.ToString("yyyyMMdd"));
                String copyTblName = createSCHFile(newConnection, "copy" + tempDate.ToString("MMdd"));
                ShiftList hsShifts = this.Shifts;
                Hashtable empHash = null;
                logger.Debug("need to map");
                if (Details.Preferences.PrefExists(Preference.USE_DL_EMP_ID))
                {
                    empHash = EmpIdHash();
                }
                tempDate = tempDate.AddDays(6);
                foreach (Shift shift in hsShifts)
                {
                    if (shift.ClockIn.Date.CompareTo(tempDate) > 0)
                    {
                        File.Copy(this.Cnx.Dsn + @"\hstmp\" + tblName + ".DBF", this.Cnx.Dsn + @"\Newdata\" + tblName + ".SCH", true);
                        if (copyFile)
                        {
                            try
                            {
                                File.Copy(this.Cnx.Dsn + @"\hstmp\" + copyTblName + ".DBF", copyLocation + "\\" + tblName + ".SCH", true);
                            }
                            catch (Exception ex)
                            {
                                logger.Error("error with Aloha Extra schedule file copy");
                                logger.Error(ex.ToString());
                            }
                        }
                        tempDate = tempDate.AddDays(1);
                        while (shift.ClockIn.Date.CompareTo(tempDate.AddDays(6)) > 0)
                        {
                            tempDate = tempDate.AddDays(7);
                        }
                        tblName = createSCHFile(newConnection, tempDate.ToString("yyyyMMdd"));
                        copyTblName = createSCHFile(newConnection, "copy" + tempDate.ToString("MMdd"));
                        tempDate = tempDate.AddDays(6);
                    }
                    int inMinutes = (shift.ClockIn.Hour * 60) + shift.ClockIn.Minute;
                    int outMinutes = 0;
                    if (new DateTime(shift.ClockOut.Year, shift.ClockOut.Month, shift.ClockOut.Day) > new DateTime(shift.ClockIn.Year, shift.ClockIn.Month, shift.ClockIn.Day))
                    {
                        outMinutes = ((shift.ClockOut.Hour + 24) * 60) + shift.ClockOut.Minute;
					}else outMinutes = (shift.ClockOut.Hour * 60) + shift.ClockOut.Minute;				

                    String empId = shift.PosEmpId.ToString();
                    if (Details.Preferences.PrefExists(Preference.USE_DL_EMP_ID) && empHash != null)
                    {
                        try
                        {
                            logger.Debug("using mapped DL ID");
                            empId = (empHash[shift.PosEmpId + ""].ToString());
                            logger.Debug("ID = " + empId);
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Employee " + shift.PosEmpId + " has an invalid mapping.");
                        }
                    }
                    if (shift.IsPosted)
                    {
                        String insertSch = "INSERT INTO " + tblName + " ( EMPLOYEE , JOBCODE , OWNERID , [DATE] , INMINUTE , OUTMINUTE ) " +
                            " VALUES( " + empId + " , " + shift.PosJobId.ToString() +
                            " , 0 , '" + shift.ClockIn.Date.ToString("yyyy/MM/dd") + "' , " + inMinutes.ToString() + " , " + outMinutes.ToString() + " )";
                        OdbcCommand insertCmd = new OdbcCommand(insertSch, newConnection);
                        int cnt = 0;
                        try
                        {
                            cnt = insertCmd.ExecuteNonQuery();
                        }
                        catch (OdbcException ex)
                        {
                            numConnections++;
                            logger.Error("in main file - error inserting row: " + insertSch);
                            newConnection.Close();
                            newConnection = new OdbcConnection(empCnxStr);
                            Thread.Sleep(3000);
                            newConnection.Open();
                            Thread.Sleep(3000);
                            logger.Debug("Num Connections(main):  " + numConnections);
                            logger.Debug("" + newConnection.State.ToString());
                            if (cnt <= 0)
                                insertCmd = new OdbcCommand(insertSch, newConnection);
                            cnt = insertCmd.ExecuteNonQuery();
                        }
                    }
                    if (copyFile)
                    {
                        String insertSch = "INSERT INTO " + copyTblName + " ( EMPLOYEE , JOBCODE , OWNERID , [DATE] , INMINUTE , OUTMINUTE ) " +
                               " VALUES( " + empId + " , " + shift.PosJobId.ToString() +
                               " , 0 , '" + shift.ClockIn.Date.ToString("yyyy/MM/dd") + "' , " + inMinutes.ToString() + " , " + outMinutes.ToString() + " )";
                        OdbcCommand insertCmd = new OdbcCommand(insertSch, newConnection);
                        int cnt = 0;
                        try
                        {
                            cnt = insertCmd.ExecuteNonQuery();
                        }
                        catch (OdbcException ex)
                        {
                            numConnections++;
                            logger.Error("in copy file - error inserting row: " + insertSch);
                            newConnection.Close();
                            newConnection = new OdbcConnection(empCnxStr);
                            Thread.Sleep(3000);
                            newConnection.Open();
                            logger.Debug("Num Connections(copy):  " + numConnections);
                            logger.Debug("" + newConnection.State.ToString());
                            Thread.Sleep(3000);
                            if (cnt <= 0)
                                insertCmd = new OdbcCommand(insertSch, newConnection);
                            cnt = insertCmd.ExecuteNonQuery();
                        }
                    }
                }

                File.Copy(this.Cnx.Dsn + @"\hstmp\" + tblName + ".DBF", this.Cnx.Dsn + @"\Newdata\" + tblName + ".SCH", true);
                if (copyFile)
                {
                    try
                    {
                        File.Copy(this.Cnx.Dsn + @"\hstmp\" + copyTblName + ".DBF", copyLocation + @"\" + tblName + ".SCH", true);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("error with Aloha Extra schedule file copy");
                        logger.Error(ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                Main.Run.errorList.Add(ex);
            }
            finally
            {
                reader.Close();
                newConnection.Close();
            }
        }


        private String GetDayOfWeek(int i)
        {
            switch (i)
            {
                case 0:
                    return "Sunday";
                case 1:
                    return "Monday";
                case 2:
                    return "Tuesday";
                case 3:
                    return "Wednesday";
                case 4:
                    return "Thursday";
                case 5:
                    return "Friday";
                case 6:
                    return "Saturday";
            }
            return "";
        }

        public override void GetWeekDates()
        {
            return;
        }

        public override DateTime CurrentWeek { get { return new DateTime(0); } set { this.CurrentWeek = value; } }

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
                String mapXml = service.getDLMappingXML(this.Details.ClientId);
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
            logger.Debug("done mapping");
            return hash;
        }

        private String createSCHFile(OdbcConnection newConnection, String tblName)
        {


            File.Delete(this.Cnx.Dsn + @"\hstmp\" + tblName + ".DBF");

            String createSch = "CREATE TABLE " + tblName + " ( EMPLOYEE number, JOBCODE integer , OWNERID integer , [DATE] date , INMINUTE integer , OUTMINUTE integer )";

            OdbcCommand createCmd = new OdbcCommand(createSch, newConnection);
            createCmd.ExecuteNonQuery();

            logger.Debug("File exists?: " + File.Exists(this.Cnx.Dsn + @"\hstmp\" + tblName + ".DBF"));

            return tblName;
        }
    }
}
