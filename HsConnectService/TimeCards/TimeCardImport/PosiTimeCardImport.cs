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

namespace HsConnect.TimeCards.TimeCardImport
{
    /// <summary>
    /// Summary description for PosiTimeCardImport.
    /// </summary>
    public class PosiTimeCardImport
    {
        public PosiTimeCardImport()
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
        private static String drive = Data.PosiControl.Drive;

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
                XmlDocument doc = punchXml;

                logger.Debug(doc.InnerXml);
                XmlNode mainNode = doc.GetElementById("Adjustments");
                CreateFile(doc.DocumentElement.OuterXml);
                PosiControl.Run(PosiControl.TAXML_NO_ISC);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                Run.errorList.Add(ex);
            }
        }

        private void CreateFile(String xml)
        {
            StreamWriter writer = null;
            String path = drive + @":\SC\EMPLOYEE.XML";
            String path2 = drive + @":\hstmp\EMPLOYEE.XML";
            logger.Log(path);
            try
            {
                if (File.Exists(path)) File.Delete(path);
                writer = File.CreateText(path);
                writer.Write(xml);
                logger.Log( "" +File.Exists(path));
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
