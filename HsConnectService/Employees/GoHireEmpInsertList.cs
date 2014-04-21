using System;
using System.Text;
using System.Collections;
using HsConnect.Xml;
using System.Xml;
using HsConnect.Main;
using HsConnect.Jobs;

namespace HsConnect.Employees
{
    public class GoHireEmpInsertList
    {
        private int clientId = -1;
        private ArrayList ls = new ArrayList();

        public GoHireEmpInsertList(int clientId)
        {
            this.clientId = clientId;
            ls = new ArrayList();
            this.logger = new SysLog(this.GetType());
        }

        private SysLog logger;

        public ArrayList getList()
        {
            return ls;
        }

        public void parseXml(String xml)
        {
            logger.Debug("Begin GoHireEmpInsertList.parseXml()");
            HsXmlReader reader = new HsXmlReader();
            reader.LoadXml(xml);
            logger.Debug("Begin GoHireEmpInsertList.parseXml() foreach loop");
            foreach (XmlNode emp in reader.SelectNodes("/go-hire-employees/employee"))
            {
                GoHireEmpInsert myEmp = new GoHireEmpInsert();
                try
                {
                    myEmp.trxId = emp.Attributes["trx-id"].Value;
                    if (emp.Attributes["user-num"].Value == null || (emp.Attributes["user-num"].Value).Length == 0)
                    {
                        logger.Debug("WARN: user-num is null. Defaulting it to 0");
                        myEmp.userNum = 0;
                    }
                    else
                    {
                        myEmp.userNum = Int32.Parse(emp.Attributes["user-num"].Value);
                    }
                    myEmp.firstName = emp.SelectSingleNode("first-name").InnerText;
                    myEmp.lastName = emp.SelectSingleNode("last-name").InnerText;
                    myEmp.addr1 = emp.SelectSingleNode("address1").InnerText;
                    myEmp.addr2 = emp.SelectSingleNode("address2").InnerText;
                    myEmp.city = emp.SelectSingleNode("city").InnerText;
                    myEmp.state = emp.SelectSingleNode("state").InnerText;
                    myEmp.zip = emp.SelectSingleNode("zip-code").InnerText;
                    myEmp.phoneNumber = emp.SelectSingleNode("phone-number").SelectSingleNode("area-code").InnerText
                        + emp.SelectSingleNode("phone-number").SelectSingleNode("prefix").InnerText
                        + emp.SelectSingleNode("phone-number").SelectSingleNode("number").InnerText;
                    myEmp.ssn = emp.SelectSingleNode("ssn").InnerText;
                    logger.Debug("Begin foreach job");
                    foreach (XmlNode job in emp.SelectNodes("jobs/emp-job"))
                    {
                        if (job.Attributes["jobcode"].Value == null || (job.Attributes["jobcode"].Value).Length == 0)
                        {
                            logger.Debug("WARN: jobcode is null. Skipping this job");
                            continue;
                        }
                        else
                        {
                            Job j = new Job();

                            int jobCode = Int32.Parse(job.Attributes["jobcode"].Value);
                            if (jobCode < 0)
                            {
                                continue;
                            }
                            else
                            {
                                j.ExtId = jobCode;
                            }

                            string jobName = job.Attributes["job-name"].Value;
                            if (jobName == null || jobName.Length == 0)
                            {
                                //noop
                            }
                            else
                            {
                                j.Name = jobName;
                            }

                            Double jobPayRate = Double.Parse(job.Attributes["pay-rate"].Value);
                            if ( jobPayRate < 0.0)
                            {
                                jobPayRate = 0.0;
                            }

                            j.DefaultWage = jobPayRate;
                            
                            myEmp.jobs.Add(j);
                        }
                    }//foreach emp-job
                    //birthdate
                    foreach (XmlNode bd in emp.SelectNodes("birthdate/date-time"))
                    {
                        string ltime = bd.Attributes["long-time"].Value;
                        if (bd.SelectSingleNode("year") != null && bd.SelectSingleNode("month") != null && bd.SelectSingleNode("day") != null)
                        {
                            myEmp.birthDate = new DateTime(Int32.Parse(bd.SelectSingleNode("year").InnerText), Int32.Parse(bd.SelectSingleNode("month").InnerText) + 1, Int32.Parse(bd.SelectSingleNode("day").InnerText));
                            logger.Debug("xmltime parse attempt. Parsed GoHire Emp's Birthday: " + myEmp.birthDate.ToString() + " ;; formatted: " + myEmp.birthDate.ToString("yyyyMMdd"));
                        }
                        else
                        {
                            logger.Debug("ERROR: birth-date not present for emp " + myEmp.trxId + " ;; Birthday will hold default value.");
                        }
                    }
                    //hiredate
                    foreach (XmlNode sd in emp.SelectNodes("startdate/date-time"))
                    {
                        string ltime = sd.Attributes["long-time"].Value;
                        if (sd.SelectSingleNode("year") != null && sd.SelectSingleNode("month") != null && sd.SelectSingleNode("day") != null)
                        {
                            myEmp.hireDate = new DateTime(Int32.Parse(sd.SelectSingleNode("year").InnerText), Int32.Parse(sd.SelectSingleNode("month").InnerText) + 1, Int32.Parse(sd.SelectSingleNode("day").InnerText));
                            logger.Debug("xmltime parse attempt. Parsed GoHire Emp's HireDate: " + myEmp.hireDate.ToString() + " ;; formatted: " + myEmp.hireDate.ToString("yyyyMMdd"));
                        }
                        else
                        {
                            logger.Debug("ERROR: start-date not present for emp " + myEmp.trxId + " ;; StartDate will hold default value.");
                        }
                    }
                    logger.Debug("Added a GoHire Emp for Insert: " + myEmp.ToLogString());
                    logger.Debug("Finish foreach job, try to add to list");
                }
                catch (Exception ex)
                {
                    logger.Error("Problem parsing employee data for insert, this employee will be skipped and have an error status applied: " + myEmp.trxId, ex);
                    myEmp.insertStatus = -1;
                    myEmp.insertReason = "Unable to parse XML when performing insert";
                }
                ls.Add(myEmp);
            }
            logger.Debug("End of loop foreach emp");
        }

        public String getInsertStatuses()
        {
            logger.Debug("Begin getInsertStatuses()");
            String xmlString = "";
            XmlDocument doc = new XmlDocument();
            XmlNode root = doc.CreateElement("emp-status-list");

            foreach (GoHireEmpInsert emp in this.ls)
            {
                if (emp.trxId.Length == 0)
                {
                    continue;
                }
                XmlElement empStatus = doc.CreateElement("emp-status");
                empStatus.SetAttribute("trx-id", emp.trxId);
                XmlElement statusEle = doc.CreateElement("status");
                statusEle.SetAttribute("id", "" + emp.insertStatus);
                XmlElement reasonEle = doc.CreateElement("reason");
                reasonEle.InnerText = emp.insertReason;
                statusEle.AppendChild(reasonEle);
                empStatus.AppendChild(statusEle);
                root.AppendChild(empStatus);
            }
            doc.AppendChild(root);
            xmlString = doc.OuterXml;
            logger.Debug("End getInsertStatuses()");
            return xmlString;
        }
    }
}
