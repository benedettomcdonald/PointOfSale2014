using System;
using System.Text;
using HsConnect.Main;
using HsConnect.Data;
using System.Xml;
using System.Collections;
using System.Data.Odbc;
using HsSharedObjects.Client;
using System.Data;
using HsExecuteModule;
using System.Net;
using System.IO;
using HsConnect.Xml;

namespace HsConnect.Shifts.Pos
{
    /**
     * Inserts schedule data into EverServ POS
     */
    class EverServScheduleImport : ScheduleImportImpl
    {
        //class members
        private SysLog logger;
        private HsData data = new HsData();
        private static int refSuffix = 0;
        private ArrayList settingNames = new ArrayList();
        private static int clientShiftStartTime = 4;

        //query types
        private const int Q_LOCATION_NAME_AND_ID = 0;
        private const int Q_SETTING_DETAILS_BY_JOB = 1;
        private const int Q_SETTING_DETAILS_BY_SCHEDULE_NAME = 2;
        private const int Q_CENTER_DETAILS = 3;
        private const int Q_CENTER_DETAILS_BY_JOB = 4;
        private const int Q_JOB_NAME_BY_JOB_ID = 5;
        private const int Q_STAFF_NAME_BY_ID = 6;

        public EverServScheduleImport()
        {
            this.logger = new SysLog(this.GetType());
        }

        public override void Execute()
        {
            logger.Debug("Begin EverServ Schedule Import for client id = " + this.Details.ClientId);

            buildLocationList();

            logger.Debug("buildLocationList complete, found " + settingNames.Count + " settings");

            foreach (string setName in settingNames)
            {
                try
                {
                    logger.Debug("Execute(): evaluating shifts for " + setName);
                    ArrayList shiftsForSetting = new ArrayList();
                    foreach (Shift shift in this.shifts)
                    {
                        if (shift.SchedName.ToLower().Equals(setName.ToLower()))
                        {
                            shiftsForSetting.Add(shift);
                        }
                    }

                    logger.Debug("Execute(): found " + shiftsForSetting.Count + " shifts for " + setName);

                    //TODO: might need to use getSettingDetailsByJob...it all depends on how things
                    //look after partech gets back to us and fixes the lab setup.
                    //either way, they should be pretty much equivalent, just different ways
                    //of getting to the settingDetails
                    string[] settingInfo = getSettingDetailsByScheduleName(setName);

                    string scheduleXml = buildExportXml(settingInfo[1], settingInfo[0], shiftsForSetting);

                    string response = exportSchedule(scheduleXml);

                    handleResponse(response);
                }
                catch (Exception ex)
                {
                    logger.Error("Problem while attempting to insert schedule data for " + setName, ex);
                }
            }

        }

        private void handleResponse(string xml)
        {
            if (stringContains(xml, "InvalidMessage") || stringContains(xml, "Declined") || stringContains(xml, "rejected"))
            {
                logger.Debug("Schedule Insert FAIL: " + xml);
                logger.Error("Schedule Insert FAIL: " + xml);
            }
            else
            {
                logger.Debug("Successfully inserted schedule information: " + xml);
            }

            /*
             * the xml responses returned aren't valid format, so we can't really use the XmlReader. Leaving this here for now
             * in case ParTech fixes this someday
             *
             * 
            logger.Debug("Begin Parsing XML Response: " + xml);
            HsXmlReader reader = new HsXmlReader();
            reader.LoadXml(xml);
            foreach (XmlNode resp in reader.SelectNodes("/NewScheduleResponse"))
            {
                try
                {
                    String schedRefNumber = resp.SelectSingleNode("ScheduleRefNumber").InnerText;
                    String status = resp.SelectSingleNode("Status").InnerText;
                    String reason = resp.SelectSingleNode("Reason").InnerText;

                    if (reason.ToLower().Equals("declined"))
                    {
                        logger.Error("Schedule Insert FAIL: " + schedRefNumber + " ;; " + reason);
                    }
                    else
                    {
                        logger.Debug("Successfully inserted schedule information");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Problem parsing the response XML from EverServ", ex);
                }
            }//foreach
            */
        }//handleResponse()

        private String exportSchedule(String xml)
        {
            string ret = "";

            HttpWebRequest req = WebRequest.Create("http://localhost:8080/iSiva/Miniservlet?CMD=NewScheduleRequest&Miniservlet=SivaGateway") as HttpWebRequest;
            req.Proxy = GlobalProxySelection.GetEmptyWebProxy();
            req.Method = "POST";
            req.ContentType = "application/xml";
            req.Accept = "application/xml";

            byte[] byteStr = Encoding.UTF8.GetBytes(xml);
            req.ContentLength = byteStr.Length;

            using (Stream stream = req.GetRequestStream())
            {
                stream.Write(byteStr, 0, byteStr.Length);
            }


            //Parse the response
            using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
            {
                using (Stream respStream = resp.GetResponseStream())
                {
                    StreamReader myRsReader = new StreamReader(respStream);
                    ret = myRsReader.ReadToEnd();

                    myRsReader.Close();
                }

                //Business Error
                if (resp.StatusCode != HttpStatusCode.OK)
                {
                    logger.Error(string.Format("Error: response status code is{0}, at time:{1}", resp.StatusCode, DateTime.Now.ToString()));
                }
            }

            return ret;
        }

        private string buildExportXml(string settingName, string settingId, ArrayList shiftsForSetting)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode root = doc.CreateElement("NewScheduleRequest");

            //append version
            root.AppendChild(createNodeElement(doc, "Version", "1.0"));

            //append ref number
            String scheduleRequestRefNumber = generateScheduleRefNumber();
            root.AppendChild(createNodeElement(doc, "ScheduleRefNumber", scheduleRequestRefNumber));

            //append sched type
            root.AppendChild(createNodeElement(doc, "ScheduleType", "Schedule"));

            //append begin date
            root.AppendChild(createNodeElement(doc, "BeginDate", getBeginDate()));

            //TODO: a better version of this?
            //we may need to use settingName param here instead of this random number/name
            //append schedule name
            root.AppendChild(createNodeElement(doc, "ScheduleName", "sched" + scheduleRequestRefNumber));

            //append location name, ID, and extRef
            string[] locDetails = getLocationDetails();
            root.AppendChild(createNodeElement(doc, "LocationName", locDetails[0]));
            root.AppendChild(createNodeElement(doc, "LocationRefNumber", locDetails[2]));

            //append setting information
            root.AppendChild(createNodeElement(doc, "SettingName", settingName));
            root.AppendChild(createNodeElement(doc, "SettingID", "" + settingId));

            /*
             * Need to add the following 4 groups of elements. Even though they aren't required,
             * the elements must be present in the xml doc
             */
            for (int i = 1; i < 8; ++i)
            {
                root.AppendChild(createNodeElement(doc, "ForecastedSaleAmount" + i, ""));
            }
            for (int i = 1; i < 8; ++i)
            {
                root.AppendChild(createNodeElement(doc, "ForecastedLaborPercentage" + i, ""));
            }
            for (int i = 1; i < 8; ++i)
            {
                root.AppendChild(createNodeElement(doc, "AverageSalaryRate" + i, ""));
            }
            for (int i = 1; i < 8; ++i)
            {
                root.AppendChild(createNodeElement(doc, "TotalLaborHour" + i, ""));
            }

            //loop through the shifts and create the ScheduleDetail elements
            foreach (Object o in shiftsForSetting)
            {
                Shift myShift = (Shift)o;
                if (myShift.IsPosted)
                {
                    XmlNode sd = doc.CreateElement("ScheduleDetail");
                    XmlNode node = createScheduleDetailNode(doc, sd, myShift, settingName);
                    root.AppendChild(node);
                }
            }

            doc.AppendChild(root);

            //create xml declaration
            XmlDeclaration xmldec = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.InsertBefore(xmldec, root);

            String ret = doc.OuterXml;

            logger.Debug("generated schedule import xml: " + ret);
            return ret;
        }

        private XmlNode createScheduleDetailNode(XmlDocument doc, XmlNode parent, Shift shift, string settingName)
        {
            //schedule detail ref
            XmlNode schedDetRef = doc.CreateElement("ScheduleDetailRefNumber");
            schedDetRef.InnerText = generateScheduleRefNumber();
            parent.AppendChild(schedDetRef);

            //bus date
            XmlNode busDate = doc.CreateElement("BusinessDate");
            busDate.InnerText = calculateBusinessDate(shift.ClockIn);
            parent.AppendChild(busDate);

            //lookup staff name
            string[] staffName = lookupStaffNameById(shift.PosEmpId);

            //staff name
            XmlNode empName = doc.CreateElement("StaffName");
            empName.InnerText = staffName[0] + " " + staffName[1];
            parent.AppendChild(empName);

            //emp id
            XmlNode staffId = doc.CreateElement("StaffID");
            staffId.InnerText = "" + shift.PosEmpId;
            parent.AppendChild(staffId);

            //job name
            XmlNode jobName = doc.CreateElement("JobName");
            jobName.InnerText = "" + getJobNameByJobId(shift.PosJobId);
            parent.AppendChild(jobName);

            //job id
            XmlNode jobId = doc.CreateElement("JobID");
            jobId.InnerText = "" + shift.PosJobId;
            parent.AppendChild(jobId);

            //get RVC details for this shift
            //returns string[]{fldCenterID, fldName}
            string[] rvcDetails = getCenterDetailsByJob(shift.PosJobId);

            //center name
            XmlNode centerName = doc.CreateElement("CenterName");
            centerName.InnerText = "" + rvcDetails[1];
            parent.AppendChild(centerName);

            //center ID
            XmlNode centerId = doc.CreateElement("CenterID");
            centerId.InnerText = "" + rvcDetails[0];
            parent.AppendChild(centerId);

            //period name, should be null per partech
            XmlNode periodName = doc.CreateElement("PeriodName");
            periodName.InnerText = "null";
            parent.AppendChild(periodName);

            //period id, should be 0 per partech
            XmlNode periodId = doc.CreateElement("PeriodID");
            periodId.InnerText = "0";
            parent.AppendChild(periodId);

            //start
            XmlNode beginTime = doc.CreateElement("BeginTime");
            beginTime.InnerText = shift.ClockIn.ToString("HH:mm MM/dd/yyyy");
            parent.AppendChild(beginTime);

            //end
            XmlNode endTime = doc.CreateElement("EndTime");
            endTime.InnerText = shift.ClockOut.ToString("HH:mm MM/dd/yyyy");
            parent.AppendChild(endTime);

            //shift type - should be same as settingName
            XmlNode shiftType = doc.CreateElement("ShiftType");
            shiftType.InnerText = "" + settingName;
            parent.AppendChild(shiftType);

            //note
            XmlNode note = doc.CreateElement("Note");
            note.InnerText = "HotSchedules schedule import for employee " + shift.PosEmpId;
            parent.AppendChild(note);

            return parent;
        }

        private string[] getSettingDetailsByJob(string jobId, string jobName)
        {
            logger.Debug("EverServScheduleImport calling getSettingDetailsByJob(job-id:" + jobId + ")");
            OdbcConnection newConnection = new OdbcConnection(this.Details.GetConnectionString());
            try
            {
                DataSet dataSet = new DataSet();
                string queryStr = generateQueryString(Q_SETTING_DETAILS_BY_JOB, new Object[] { jobId, jobName });

                OdbcDataAdapter dataAdapter = new OdbcDataAdapter(queryStr, newConnection);
                dataAdapter.Fill(dataSet, "plnScheduleSetting");
                dataAdapter.Dispose();
                DataRowCollection rows = dataSet.Tables[0].Rows;

                string settingID = "";
                string settingName = "";

                BaseExecute exec = new BaseExecute();
                foreach (DataRow row in rows)
                {
                    settingID = exec.GetInt(row, "fldScheduleSettingID") + "";
                    settingName = exec.GetString(row, "fldName") + "";
                }
                exec = null;

                string[] ret = new string[2];
                ret[0] = settingID;
                ret[1] = settingName;
                return ret;
            }
            finally
            {
                newConnection.Close();
            }
        }

        private string[] lookupStaffNameById(int staffPosId)
        {
            logger.Debug("EverServScheduleImport calling lookupStaffNameById(pos-id:" + staffPosId + ")");
            OdbcConnection newConnection = new OdbcConnection(this.Details.GetConnectionString());
            try
            {
                DataSet dataSet = new DataSet();
                string queryStr = generateQueryString(Q_STAFF_NAME_BY_ID, new Object[] { staffPosId });

                OdbcDataAdapter dataAdapter = new OdbcDataAdapter(queryStr, newConnection);
                dataAdapter.Fill(dataSet, "locStaff");
                dataAdapter.Dispose();
                DataRowCollection rows = dataSet.Tables[0].Rows;

                string fName = "";
                string lName = "";

                BaseExecute exec = new BaseExecute();
                foreach (DataRow row in rows)
                {
                    fName = exec.GetString(row, "fldName_First");
                    lName = exec.GetString(row, "fldName_Last");
                }
                exec = null;

                string[] ret = new string[2];
                ret[0] = fName;
                ret[1] = lName;
                return ret;
            }
            finally
            {
                newConnection.Close();
            }
        }

        //returns string[]{fldCenterID, fldName}
        private string[] getCenterDetails(string locName)
        {
            logger.Debug("EverServScheduleImport calling getCenterDetails(locationName:" + locName + ")");
            OdbcConnection newConnection = new OdbcConnection(this.Details.GetConnectionString());
            try
            {
                DataSet dataSet = new DataSet();
                string queryStr = generateQueryString(Q_CENTER_DETAILS, new Object[] { locName.ToLower() });

                OdbcDataAdapter dataAdapter = new OdbcDataAdapter(queryStr, newConnection);
                dataAdapter.Fill(dataSet, "locCenter");
                dataAdapter.Dispose();
                DataRowCollection rows = dataSet.Tables[0].Rows;

                string fldCenterID = "";
                string fldName = "";

                BaseExecute exec = new BaseExecute();
                foreach (DataRow row in rows)
                {
                    fldCenterID = exec.GetInt(row, "fldCenterID") + "";
                    fldName = exec.GetString(row, "fldName") + "";
                }
                exec = null;

                string[] ret = new string[2];
                ret[0] = fldCenterID;
                ret[1] = fldName;
                return ret;
            }
            finally
            {
                newConnection.Close();
            }
        }

        //returns job name
        private string getJobNameByJobId(int jobId)
        {
            logger.Debug("EverServScheduleImport calling getJobNameByJobId(jobId:" + jobId + ")");
            OdbcConnection newConnection = new OdbcConnection(this.Details.GetConnectionString());
            try
            {
                DataSet dataSet = new DataSet();
                string queryStr = generateQueryString(Q_JOB_NAME_BY_JOB_ID, new Object[] { jobId });

                OdbcDataAdapter dataAdapter = new OdbcDataAdapter(queryStr, newConnection);
                dataAdapter.Fill(dataSet, "oprJob");
                dataAdapter.Dispose();
                DataRowCollection rows = dataSet.Tables[0].Rows;

                string fldName = "";

                BaseExecute exec = new BaseExecute();
                foreach (DataRow row in rows)
                {
                    fldName = exec.GetString(row, "fldName");
                }
                exec = null;

                return fldName;
            }
            finally
            {
                newConnection.Close();
            }
        }

        //returns string[]{fldCenterID, fldName}
        private string[] getCenterDetailsByJob(int jobPosId)
        {
            logger.Debug("EverServScheduleImport calling getCenterDetailsByJob(jobPosId:" + jobPosId + ")");

            OdbcConnection newConnection = new OdbcConnection(this.Details.GetConnectionString());
            try
            {
                DataSet dataSet = new DataSet();
                string queryStr = generateQueryString(Q_CENTER_DETAILS_BY_JOB, new Object[] { jobPosId });

                OdbcDataAdapter dataAdapter = new OdbcDataAdapter(queryStr, newConnection);
                dataAdapter.Fill(dataSet, "locCenterAndJob");
                dataAdapter.Dispose();
                DataRowCollection rows = dataSet.Tables[0].Rows;

                string fldCenterID = "";
                string fldName = "";

                BaseExecute exec = new BaseExecute();
                foreach (DataRow row in rows)
                {
                    fldCenterID = exec.GetInt(row, "fldCenterID") + "";
                    fldName = exec.GetString(row, "fldName");
                }
                exec = null;

                string[] ret = new string[2];
                ret[0] = fldCenterID;
                ret[1] = fldName;
                return ret;
            }
            finally
            {
                newConnection.Close();
            }
        }

        private string[] getSettingDetailsByScheduleName(string schedName)
        {
            logger.Debug("EverServScheduleImport calling getSettingDetailsByScheduleName(sched-name:" + schedName + ")");
            OdbcConnection newConnection = new OdbcConnection(this.Details.GetConnectionString());
            try
            {
                DataSet dataSet = new DataSet();
                string queryStr = generateQueryString(Q_SETTING_DETAILS_BY_SCHEDULE_NAME, new Object[] { schedName.ToLower() });
                logger.Debug("getSettingDetailsByScheduleName query: " + queryStr);

                OdbcDataAdapter dataAdapter = new OdbcDataAdapter(queryStr, newConnection);
                dataAdapter.Fill(dataSet, "plnScheduleSetting");
                dataAdapter.Dispose();
                DataRowCollection rows = dataSet.Tables[0].Rows;

                string settingID = "";
                string settingName = "";

                BaseExecute exec = new BaseExecute();
                foreach (DataRow row in rows)
                {
                    settingID = exec.GetInt(row, "fldScheduleSettingID") + "";
                    settingName = exec.GetString(row, "fldName") + "";
                }
                exec = null;

                string[] ret = new string[2];
                ret[0] = settingID;
                ret[1] = settingName;
                return ret;
            }
            finally
            {
                newConnection.Close();
            }
        }

        /*
         * Returns: String[]{fldName, fldLocationID, fldReferenceNumber}
         */
        private string[] getLocationDetails()
        {
            OdbcConnection newConnection = new OdbcConnection(this.Details.GetConnectionString());
            try
            {
                DataSet dataSet = new DataSet();
                string queryStr = generateQueryString(Q_LOCATION_NAME_AND_ID, new Object[] { Details.ClientExtRef });

                OdbcDataAdapter dataAdapter = new OdbcDataAdapter(queryStr, newConnection);
                dataAdapter.Fill(dataSet, "locLocation");
                dataAdapter.Dispose();
                DataRowCollection rows = dataSet.Tables[0].Rows;

                string[] ret = new string[3];

                BaseExecute exec = new BaseExecute();
                foreach (DataRow row in rows)
                {
                    ret[0] = exec.GetString(row, "fldName");
                    ret[1] = exec.GetInt(row, "fldLocationID") + "";
                    ret[2] = exec.GetString(row, "fldReferenceNumber");
                }
                exec = null;

                return ret;
            }
            finally
            {
                newConnection.Close();
            }
        }

        private string generateQueryString(int qType, Object[] qParams)
        {
            string ret = "";

            switch (qType)
            {
                case Q_SETTING_DETAILS_BY_JOB:
                    ret = "select * from plnScheduleSetting pss, (select * from plnScheduleSettingDetail where fldJobID = " + qParams[0] + " ) q " +
                    " where pss.fldScheduleSettingID = q.fldScheduleSettingID " +
                    " and pss.fldName like '%" + qParams[1] + "%' ";
                    break;

                case Q_LOCATION_NAME_AND_ID:
                    ret = "select fldName, fldLocationID, fldReferenceNumber from dbo.locLocation where locLocation.fldReferenceNumber LIKE '%" + qParams[0] + "%' ";
                    break;

                case Q_SETTING_DETAILS_BY_SCHEDULE_NAME:
                    ret = "select * from plnScheduleSetting where fldScheduleSettingID IN (select top 1 fldScheduleSettingID from plnSchedule where LOWER(fldName) = '" + qParams[0] + "') ";
                    break;

                case Q_CENTER_DETAILS:
                    ret = "select fldCenterID, fldName from dbo.locCenter where LOWER(fldName) LIKE '%" + qParams[0] + "%' ";
                    break;

                case Q_CENTER_DETAILS_BY_JOB:
                    ret = "select lcaj.fldCenterID, lc.fldName from dbo.locCenterAndJob lcaj, dbo.locCenter lc where lcaj.fldCenterID = lc.fldCenterID and lcaj.fldJobID = " + qParams[0] + " ";
                    break;

                case Q_JOB_NAME_BY_JOB_ID:
                    ret = "select fldName from dbo.oprJob where fldJobID = " + qParams[0] + " ";
                    break;

                case Q_STAFF_NAME_BY_ID:
                    ret = "select fldName_First, fldName_Last from dbo.locStaff where fldStaffID = " + qParams[0] + " ";
                    break;

                default:
                    ret = "";
                    break;
            }

            return ret;
        }

        private int lookupSettingId(string setName)
        {
            //TODO: this
            return 0;
        }

        //TODO: probably need to filter empty names here so we dont' end up querying for them
        private void buildLocationList()
        {
            logger.Debug("Beging buildLocationList()");
            foreach (Shift shift in this.shifts)
            {
                logger.Debug("SchedName" + shift.SchedName);
                logger.Debug("LocName" + shift.LocName);
                string locName = shift.SchedName;
                if (!settingNames.Contains(locName))
                {
                    logger.Debug("buildLocationList: adding " + locName);
                    settingNames.Add(locName);
                }
            }
        }

        private String calculateBusinessDate(DateTime clock)
        {
            if (clock.Hour < clientShiftStartTime)
            {
                clock.AddDays(-1);
            }

            return clock.ToString("MM/dd/yyyy");
        }

        private XmlNode createNodeElement(XmlDocument doc, String name, String text)
        {
            XmlNode ret = doc.CreateElement(name);
            ret.InnerText = text;
            return ret;
        }

        private String generateScheduleRefNumber()
        {
            refSuffix++;
            return DateTime.Today.ToString("1yyMMdd") + "" + refSuffix;
        }

        private String getBeginDate()
        {
            DateTime earliestDate = DateTime.Now;

            foreach (Shift sh in this.shifts)
            {
                if (sh.IsPosted)
                {
                    DateTime myDate = sh.ClockIn;
                    if (myDate.Hour < clientShiftStartTime)
                    {
                        myDate.AddDays(-1);
                    }
                    if (myDate.CompareTo(earliestDate) <= 0)
                    {
                        earliestDate = myDate;
                    }
                }
            }

            return earliestDate.ToString("MM/dd/yyyy");
        }

        public override DateTime CurrentWeek { get { return new DateTime(0); } set { this.CurrentWeek = value; } }

        public override void GetWeekDates()
        {
            return;
        }

        public bool stringContains(string sourceStr, string searchStr)
        {
            if (sourceStr.IndexOf(searchStr) > -1)
            {
                return true;
            }
            return false;
        }
    }
}
