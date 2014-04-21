using HsConnect.Shifts;
using HsConnect.Main;
using HsConnect.Data;
using HsConnect.Employees;
using HsConnect.Employees.PosList;
using HsConnect.EmpJobs;
using HsConnect.EmpJobs.PosList;
using HsConnect.Jobs;
using HsConnect.Jobs.PosList;
using HsSharedObjects.Client.Preferences;

using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Xml;
using Microsoft.Data.Odbc;
using HsConnect.Xml;
using HsSharedObjects.Client;

namespace HsConnect.TimeCards.TimeCardImport
{
    /// <summary>
    /// Summary description for PosiTimeCardImport.
    /// </summary>
    public class AlohaTimeCardImport
    {
        public AlohaTimeCardImport()
        {
            this.logger = new SysLog(this.GetType());
        }

        private SysLog logger;
        private HsData data = new HsData();
        private XmlDocument punchXml;
        private DateTime lastWeek = new DateTime(0);
        private DateTime currWeek = new DateTime(0);
        private DateTime nextWeek = new DateTime(0);
        private HsEmployeeList empList;
        private HsTimeCardList tcList;
        private ClientDetails details;

        public ClientDetails Details
        {
            get { return this.details; }
            set { this.details = value; }
        }

        public DateTime CurrentWeek
        {
            get
            {
                if (DateTime.Now >= this.nextWeek) return this.nextWeek;
                return this.currWeek;
            }
            set { this.currWeek = value; }
        }

        public void Execute()
        {
            try
            {
                logger.Debug("Inside aloha time card import: 1");
                XmlDocument doc = punchXml;
                punchXml = ParseFile(doc);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                Run.errorList.Add(ex);
            }
        }

        private XmlDocument ParseFile(XmlDocument doc)
        {
         /*   HsXmlReader reader = new HsXmlReader();
            reader.LoadXml(doc.InnerXml);
            XmlNode adj = reader.SelectSingleNode("/Adjustments");
            AlohaEmpJobList empJobs = new AlohaEmpJobList();
            empJobs.Dsn = details.Dsn;
            empJobs.SetDataConnection(details.GetConnectionString());
            empJobs.Details = details;
            empJobs.DbLoad();
            foreach (XmlNode adjust in adj.ChildNodes)
            {
                try
                {
                    logger.Debug("Inside aloha time card import: 4 - looping" + adjust.Name);
                    int empId = reader.GetInt(adjust, "EmployeeNumber", HsXmlReader.NODE);
                    String extId = reader.GetString(adjust, "extId", HsXmlReader.ATTRIBUTE);
                    int shiftId = -1;
                    if (extId.Length > 2)
                    {
                        String extShiftId = extId.Substring(6);

                        shiftId = Int32.Parse(extShiftId.Replace("" + empId, ""));
                    }



                    EDTPUNCHLib.IBohEdtPunches boh = new EDTPUNCHLib.BohEdtPunches();
                    if (adjust.Name.Equals("AdjustDelete"))
                    {
                        string oldInDateStr = reader.GetString(adjust, "OldInDate", HsXmlReader.NODE);
                        string oldInTimeStr = reader.GetString(adjust, "OldInTime", HsXmlReader.NODE);
                        string oldOutTimeStr = reader.GetString(adjust, "OldOutTime", HsXmlReader.NODE);
                        int oldJob = reader.GetInt(adjust, "NewAltJobNumber", HsXmlReader.NODE);
                        String[] oldInDateStrArray = oldInDateStr.Split(new char[] { '/' });
                        DateTime oldInDate = new DateTime(2000 + Int32.Parse(oldInDateStrArray[0]), Int32.Parse(oldInDateStrArray[1]), Int32.Parse(oldInDateStrArray[2]));
                        logger.Debug("AdjustDelete");
                        logger.Debug("IN DATE " + oldInDate.ToString());
                        logger.Debug("SHIFT ID " + shiftId);
                        logger.Debug("EMP ID " + empId);
                        boh.DeleteShift(oldInDate,
                                        "",
                                        shiftId,
                                        empId,
                                        -1//NEED STORE ID
                                        );
                    }
                    else
                    {
                        int reason = reader.GetInt(adjust, "Reason", HsXmlReader.NODE);
                        string mgrId = reader.GetString(adjust, "ManagerNumber", HsXmlReader.NODE);
                        string newInDateStr = reader.GetString(adjust, "NewInDate", HsXmlReader.NODE);
                        string newInTimeStr = reader.GetString(adjust, "NewInTime", HsXmlReader.NODE);
                        string newOutTimeStr = reader.GetString(adjust, "NewOutTime", HsXmlReader.NODE);
                        int newJob = reader.GetInt(adjust, "NewAltJobNumber", HsXmlReader.NODE);
                        String[] newInDateStrArray = newInDateStr.Split(new char[] { '/' });
                        String[] newInTimeStrArray = newInTimeStr.Split(new char[] { ':' });
                        String[] newOutTimeStrArray = newOutTimeStr.Split(new char[] { ':' });
                        DateTime newInDate = new DateTime(2000 + Int32.Parse(newInDateStrArray[0]), Int32.Parse(newInDateStrArray[1]), Int32.Parse(newInDateStrArray[2]));
                        DateTime newTimeIn = new DateTime(DateTime.Now.Year,
                                                           DateTime.Now.Month,
                                                           DateTime.Now.Day,
                                                           Int32.Parse(newInTimeStrArray[0]),
                                                           Int32.Parse(newInTimeStrArray[1]),
                                                            0, 0);
                        DateTime newTimeOut = new DateTime(DateTime.Now.Year,
                                                       DateTime.Now.Month,
                                                       DateTime.Now.Day,
                                                           Int32.Parse(newOutTimeStrArray[0]),
                                                           Int32.Parse(newOutTimeStrArray[1]),
                                                        0, 0);
                        EmpJob empJob = empJobs.GetEmpJobByIds(empId, newJob);
                        float pay = 0;
                        if (empJob == null)
                        {
                            logger.Error("user does not have this job in the POS.  emp = " + empId + "  job = " + newJob);
                        }
                        else
                        {
                            pay = empJob.RegWage;
                        }


                        if (adjust.Name.Equals("AdjustAdd"))//ADD SHIFT
                        {
                            // call Aloha COM method
                            logger.Debug("AdjustAdd");
                            logger.Debug("IN DATE " + newInDate.ToString());
                            logger.Debug("MGR ID " + mgrId);
                            logger.Debug("SHIFT ID " + shiftId);
                            logger.Debug("EMP ID " + empId);
                            logger.Debug("NEW IN TIME " + newTimeIn.ToString());
                            logger.Debug("NEW OUT TIME " + newTimeOut.ToString());
                            logger.Debug("NEW JOB " + newJob);
                            logger.Debug("PAY " + pay);
                            logger.Debug("REASON " + reason);

                            int shftid = boh.AddShift(newInDate,
                                            mgrId,
                                            empId,
                                            newTimeIn,
                                            newTimeOut,
                                            newJob,
                                            pay,
                                            0,
                                            reason,
                                            -1,//need to get store ID
                                            true);
                            if (shftid > -1)
                            {
                                String newExtId = newInDate.ToString("yy") + newInDate.ToString("MM") + newInDate.ToString("dd") + shftid + empId;
                                logger.Debug(newExtId);
                                adjust.Attributes["extId"].Value = newExtId;
                            }
                            else
                            {
                                XmlAttribute att = reader.CreateAttribute("Error");
                                att.Value = "error";
                                adjust.Attributes.Append(att);
                            }
                            //logger.Debug("NEW SHIFT ID in XML 3:  " + doc.InnerXml);

                        }
                        else if (adjust.Name.Equals("AdjustChange"))//EDIT SHIFT
                        {
                            logger.Debug("AdjustChange");
                            logger.Debug("IN DATE " + newInDate.ToString());
                            logger.Debug("MGR ID " + mgrId);
                            logger.Debug("SHIFT ID " + shiftId);
                            logger.Debug("EMP ID " + empId);
                            logger.Debug("NEW IN TIME " + newTimeIn.ToString());
                            logger.Debug("NEW OUT TIME " + newTimeOut.ToString());
                            logger.Debug("NEW JOB " + newJob);
                            logger.Debug("PAY " + pay);
                            logger.Debug("REASON " + reason);
                            boh.EditShift(newInDate,
                                  mgrId,
                                  shiftId,
                                  empId,
                                  newTimeIn,
                                  newTimeOut,
                                  newJob,
                                  pay,
                                  0,
                                  reason,
                                  -1,
                                  true);
                            
                        }
                    }
                }
                catch (Exception ex) 
                {
                    logger.Error("Error importing Punch Record adjustment into Aloha:  " + ex.ToString());
                    logger.Error("for punch record:  " + adjust.OuterXml);
                    XmlAttribute att = reader.CreateAttribute("Error");
                    att.Value = "error";
                    adjust.Attributes.Append(att);

                }
            }
            XmlDocument doc2 = new XmlDocument();
            doc2.LoadXml(reader.OuterXml);
            return doc2;*/
            return null;
        }

        private void CreateFile(String xml)
        {
            StreamWriter writer = null;
            String path = @"C:\SC\EMPLOYEE.XML";
            String path2 = @"C:\hstmp\EMPLOYEE.XML";
            try
            {
                if (File.Exists(path)) File.Delete(path);
                writer = File.CreateText(path);
                writer.Write(xml);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            finally
            {
                writer.Flush();
                writer.Close();
            }

            try
            {
                if (File.Exists(path2)) File.Delete(path2);
                writer = File.CreateText(path2);
                writer.Write(xml);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            finally
            {
                writer.Flush();
                writer.Close();
            }
        }

        public HsEmployeeList EmpList
        {
            get { return this.empList; }
            set { this.empList = value; }
        }
        public HsTimeCardList TCList
        {
            get { return this.tcList; }
            set { this.tcList = value; }
        }

        public XmlDocument PunchXml
        {
            get { return this.punchXml; }
            set { this.punchXml = value; }
        }

    }
}
