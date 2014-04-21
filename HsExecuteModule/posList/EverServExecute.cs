using System;
using System.Text;
using HsSharedObjects.Main;
using System.Collections;
using System.Data.Odbc;
using System.Data;
using System.Windows.Forms;
using System.IO;
using HsSharedObjects.Client.Preferences;

namespace HsExecuteModule.posList
{
    class EverServExecute :BaseExecute
    {

        private string delim = "|";
        private int NUM_WEEKS = 4;
        private readonly String dir = @"C:\hstemp\everserv_files";
        private int locationId = -1;

        public override void Execute(bool map)
        {
            //Cleanup the export directory before processing commands
            System.IO.DirectoryInfo di = new DirectoryInfo(dir);
            if (di.Exists)
            {

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dinfo in di.GetDirectories())
                {
                    dinfo.Delete(true);
                }

            }

            foreach (Command cmd in commands)
            {
                logger.Debug("Preparing to execute commands on EverServ using the following connection string: " + ClientDetails.GetConnectionString());
                try
                {
                    RunCommand(cmd);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());
                }
            }
        }

        public override void RunCommand(Command cmd)
        {
            logger.Debug("EverServExecute running the following command / args: " + cmd.Cmd + " / " + cmd.Args);
            TextWriter writer = null;
            try
            {
                locationId = getLocationId();
                ProcessArgs(cmd.Args);
                ArrayList strings = DbLoad(cmd);
                String file = dir + "\\" + cmd.Cmd + "_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                logger.Debug("Command " + cmd.Cmd + " execution is complete, preparing to write to file: " + file);
                CheckFileExistence(file);
                writer = new StreamWriter(file);
                logger.Debug("writing " + strings.Count + " lines to " + file);
                foreach (String s in strings)
                {
                    writer.Write(s);
                    writer.WriteLine();
                }
            }
            catch (Exception ex)
            {
                logger.Error("ERROR running command in EverServExecute" + cmd.Cmd + " / " + cmd.Args, ex);
            }
            finally
            {
                writer.Flush();
                writer.Close();
            }
        }

        private void ProcessArgs(String args)
        {
            logger.Debug("EverServExecute processing the following args: " + args);
            try
            {
                if (args == null || args.Length == 0)
                {
                    return;
                }

                char[] stringSeparators = { ';' };
                String[] myArgs = args.Split(stringSeparators);

                foreach (String s in myArgs)
                {
                    if (s.IndexOf("num_weeks") > -1)
                    {
                        String val = s.Substring(s.IndexOf('=') + 1);
                        int numWks = Int32.Parse(val);
                        this.NUM_WEEKS = numWks;
                        logger.Debug("Set NUM_WEEKS for this command to " + NUM_WEEKS);
                    }
                    //handle more args here...
                }
            }
            catch (Exception e)
            {
                logger.Error("ERROR parsing Args for command in EverServExecute: " + args, e);
            }
        }

        private int getLocationId()
        {
            logger.Debug("EverServExecute calling getLocationId(ClientExtRef:" + ClientDetails.ClientExtRef + ")");
            OdbcConnection newConnection = new OdbcConnection(ClientDetails.GetConnectionString());

            int locationId = -1;
            string referenceNumber = "";

            try
            {
                DataSet dataSet = new DataSet();
                string queryStr = "select fldLocationID, fldReferenceNumber from dbo.locLocation";

                OdbcDataAdapter dataAdapter = new OdbcDataAdapter(queryStr, newConnection);
                dataAdapter.Fill(dataSet, "locLocation");
                dataAdapter.Dispose();
                DataRowCollection rows = dataSet.Tables[0].Rows;

                

                BaseExecute exec = new BaseExecute();
                foreach (DataRow row in rows)
                {
                    locationId = exec.GetInt(row, "fldLocationID");
                    referenceNumber = exec.GetString(row, "fldReferenceNumber");
                    if (referenceNumber == null || referenceNumber.Equals(""))
                    {
                        continue;
                    }

                    int refNum = Int32.Parse(referenceNumber);
                    if (refNum == ClientDetails.ClientExtRef)
                    {
                        break;
                    }
                }
                exec = null;
            }
            finally
            {
                newConnection.Close();
            }
            logger.Debug("GetLocationId() found locationId:" + locationId + " for client with extRef:" + ClientDetails.ClientExtRef);
            return locationId;
        }

        private void CheckFileExistence(string file)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (File.Exists(file))
                File.Delete(file);
        }

        public ArrayList DbLoad(Command cmd)
        {
           
            ArrayList strings = new ArrayList();
            try
            {
                DataRowCollection rows = GenerateDataRows(cmd);
                foreach (DataRow row in rows)
                {
                    try
                    {
                        String st = GenerateDataStringFromRow(row, cmd);
                        strings.Add(st);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error adding item in Load(): " + ex.ToString(), ex);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error retrieving EverServ data in Load(): ", ex);
            }
            return strings;
        }

        private DataRowCollection GenerateDataRows(Command cmd)
        {
            logger.Debug("EverServExecute calling GenerateDataRows(" + cmd.Cmd + ")");
            OdbcConnection newConnection = new OdbcConnection(ClientDetails.GetConnectionString());
            try
            {
                DataSet dataSet = new DataSet();
                logger.Debug("Executing the following query: " + GetQueryByCommand(cmd));
                OdbcDataAdapter dataAdapter = new OdbcDataAdapter(GetQueryByCommand(cmd), newConnection);
                //dataAdapter.SelectCommand.Parameters.Add(, null);
                AddParametersForCommand(cmd, dataAdapter);
                dataAdapter.Fill(dataSet, GetQueryTableByCommand(cmd));
                dataAdapter.Dispose();
                DataRowCollection rows = dataSet.Tables[0].Rows;
                logger.Debug("returned [" + rows.Count + "] rows from the database");
                return rows;
            }
            catch (Exception ex)
            {
                logger.Error("Error retrieving EverServ data in Load(): " + ex.ToString(), ex);
            }
            finally
            {
                newConnection.Close();
            }
            return null;
        }

        private String GetQueryByCommand(Command cmd)
        {
            logger.Debug("EverServExecute calling GetQueryByCommand(" + cmd.Cmd + ")");
            //for time-constraint based queries
            DateTime start = DateTime.Now.AddDays(NUM_WEEKS * -7);
            DateTime end = DateTime.Now.AddDays(1);

            switch (cmd.Cmd)
            {
                //ASK ABOUT HRID
                case "empl_master":
                    return "select staff.fldStaffID as pos_id, staff.fldName_First as first_name, staff.fldName_Last as last_name, "
                        + "staff.fldContact_Phone as telephone, staff.fldContact_Phone as sms, staff.fldContact_Email as email, "
                        + "staff.fldAddress_Street as address_1, staff.fldAddress_Suite as address_2, staff.fldAddress_City as city, "
                        + "staff.fldAddress_State as state, staff.fldAddress_ZipCode as zip, staff.fldStatus as status, staff.fldEndDate as term_date "
                        + "from dbo.locStaff staff, dbo.locLocationAndStaff locStaff "
                        + "where locStaff.fldStaffID = staff.fldStaffID " 
                        + (this.ClientDetails.Preferences.PrefExists(Preference.EVERSERV_SYNC_NONACTIVE_EMPLOYEES) ? "" : "and staff.fldStatus = 1 ") 
                        + "and locStaff.fldLocationID = " + locationId + " ";

                case "empl_job":
                    return "select locStaff.fldStaffID as emp_pos_id, oprJob.fldJobId as job_pos_id, locStaffandJob.fldSalaryRate as reg_rate, lclLaborConfig.fldOvtRateList as ovt_rate, "
                            + "locStaffandJob.fldIsPrimaryJob as is_primary "
                            + "from locStaffAndJob "
                            + "inner join locStaff on locStaffAndJob.fldStaffID=locStaff.fldStaffID "
                            + "inner join oprJob on oprJob.fldJobID=locStaffAndJob.fldJobID "
                            + "inner join locLocationAndStaff on locLocationAndStaff.fldStaffID = locStaffAndJob.fldStaffID "
                            + "inner join locLocation on locLocationAndStaff.fldLocationID = locLocation.fldLocationID "
                            + "inner join lclLaborConfig on locLocation.fldLaborConfigID = lclLaborConfig.fldLaborConfigID "
                            + "where locLocation.fldLocationID = " + locationId + " and locStaff.fldStatus = 1 ";


                case "job_client":
                    return "select distinct locLocation.fldLocationID as 'Unique_LocationID', locLocation.fldName as 'Location_Name', "
                            + "oprJob.fldJobID as 'job_pos_id', oprJob.fldName as 'job_name' "
                            + "from locLocation "
                            + "inner join locLocationAndStaff on locLocationAndStaff.fldLocationID=locLocation.fldLocationID "
                            + "inner join locStaff on locLocationandstaff.fldStaffID=locStaff.fldStaffID "
                            + "inner join locStaffAndJob on locStaffAndJob.fldStaffID=locStaff.fldStaffID "
                            + "inner join oprJob on oprJob.fldJobID=locStaffAndJob.fldJobID "
                            + "where locLocation.fldLocationID = " + locationId + " ";

                case "Timecard":
                    return "select jorClock.fldStaffID as emp_id, jorClock.fldJobID as job_id, jorJournal.fldBusinessDate as bus_date, "
                            + "jorClock.fldInTime as clock_in, jorClock.fldOutTime as clock_out, jorClock.fldRegularDuration as reg_hours, jorClock.fldOvertimeDuration as ovt_hours, "
                            + "jorClock.fldRegularPayAmount as reg_pay, jorClock.fldOvertimePayAmount as ovt_pay, '0' as sp_pay "
                            + "from jorClock "
                            + "inner join jorJournal on jorJournal.fldJournalId=jorClock.fldJournalID "
                            + "inner join locLocation on jorJournal.fldLocationID = locLocation.fldLocationID "
                            + "where jorJournal.fldBusinessDate > '" + start.ToString("yyyy-MM-dd") + "' AND jorJournal.fldBusinessDate < '" + end.ToString("yyyy-MM-dd") + "' "
                            + "and locLocation.fldLocationID = " + locationId + " ";

                case "Sales":
                    return (this.ClientDetails.Preferences.PrefExists(Preference.LSF_EVERSERV_CUSTOM_NET_SALES)
                        ?
                        "select jorJournal.fldBusinessDate as bus_date, jorCheck.fldCloseTime as time, "
                            + "(jorCheckDetail.fldAmount - jorCheckDetail.fldAdjustmentAmount) as amount, "
                            + "jorCheck.fldOwnedByStaffID as emp_id, "
                            + "jorCheck.fldCenterID as rev_code, "
                            + "jorCheckMerchandise.fldCategoryID as sales_cat "
                            + "from jorCheck "
                            + "inner join jorJournal on jorCheck.fldJournalID=jorJournal.fldJournalID "
                            + "inner join jorCheckDetail on jorCheck.fldCheckID = jorCheckDetail.fldCheckID "
                            + "inner join jorCheckMerchandise on jorCheckDetail.fldCheckDetailID = jorCheckMerchandise.fldCheckDetailID "
                            + "inner join locLocation on locLocation.fldLocationID = jorJournal.fldLocationID "
                            + "where jorJournal.fldBusinessDate > '" + start.ToString("yyyy-MM-dd") + "' AND jorJournal.fldBusinessDate < '" + end.ToString("yyyy-MM-dd") + "' "
                            + "and locLocation.fldLocationID = " + locationId + " "
                            + "and jorCheckDetail.fldVoidID = 0 "
                            + "and jorCheckDetail.fldAmount <> 0 "
                        :
                        "select jorJournal.fldBusinessDate as bus_date, jorCheck.fldCloseTime as time, jorCheckDetail.fldAmount as amount, jorCheck.fldOwnedByStaffID as emp_id, "
                            + "jorCheck.fldCenterID as rev_code, jorCheckMerchandise.fldCategoryID as sales_cat "
                            + "from jorCheck "
                            + "inner join jorJournal on jorCheck.fldJournalID=jorJournal.fldJournalID "
                            + "inner join jorCheckDetail on jorCheck.fldCheckID = jorCheckDetail.fldCheckID "
                            + "inner join jorCheckMerchandise on jorCheckDetail.fldCheckDetailID = jorCheckMerchandise.fldCheckDetailID "
                            + "inner join locLocation on locLocation.fldLocationID = jorJournal.fldLocationID "
                            + "where jorJournal.fldBusinessDate > '" + start.ToString("yyyy-MM-dd") + "' AND jorJournal.fldBusinessDate < '" + end.ToString("yyyy-MM-dd") + "' "
                            + "and locLocation.fldLocationID = " + locationId + " "
                            + "and jorCheckDetail.fldAmount <> 0 ");

                case "RVC":
                    return "select locCenter.fldCenterID as rvc, locCenter.fldName as name from locCenter ";

                case "GuestCounts":
                    return "select jorJournal.fldBusinessDate as bus_date, jorCheck.fldCloseTime as time, jorCheck.fldGuestCount as guests, jorCheck.fldCenterID as rvc "
                            + "from jorCheck "
                            + "inner join jorJournal on jorJournal.fldJournalID=jorCheck.fldJournalID "
                            + "inner join locLocation on locLocation.fldLocationID = jorJournal.fldLocationID "
                            + "where jorJournal.fldBusinessDate > '" + start.ToString("yyyy-MM-dd") + "' AND jorJournal.fldBusinessDate < '" + end.ToString("yyyy-MM-dd") + "' "
                            + "and locLocation.fldLocationID = " + locationId + " ";

                case "cat":
                    return "select cptCategory.fldCategoryID as catId, cptCategory.fldName as name from cptCategory ";

                default:
                    return "";

            }
        }

        private String GetQueryTableByCommand(Command cmd)
        {
            switch (cmd.Cmd)
            {
                case "empl_master":
                    return "locStaff";

                case "empl_job":
                    return "locStaffAndJob";

                case "job_client":
                    return "locLocation";

                case "Timecard":
                    return "jorClock";

                case "Sales":
                    return "jorCheck";

                case "RVC":
                    return "locCenter";

                case "GuestCounts":
                    return "jorCheck";

                case "cat":
                    return "cptCategory";

                default:
                    return "";

            }
        }

        private String GenerateDataStringFromRow(DataRow row, Command cmd)
        {
            switch (cmd.Cmd)
            {
                case "empl_master":
                    return GetEmployeeString(row);

                case "empl_job":
                    return GetEmpJobString(row);

                case "job_client":
                    return GetJobString(row);

                case "Timecard":
                    return GetTimecardString(row);

                case "Sales":
                    return GetSalesString(row);

                case "RVC":
                    return GetRvcString(row);

                case "GuestCounts":
                    return GetGcString(row);

                case "cat":
                    return GetSalesCatString(row);

                default:
                    return "";
            }
        }

        private void AddParametersForCommand(Command cmd, OdbcDataAdapter dataAdapter)
        {
        }

        private String GetEmployeeString(DataRow row)
        {
            int companyId = ClientDetails.ClientCompanyExtRef;
            int conceptId = ClientDetails.ClientConceptExtRef;
            int storeNum = ClientDetails.ClientExtRef;
            int PosId = GetInt(row, "pos_id");
            String FirstName = GetString(row, "first_name");
            String LastName = GetString(row, "last_name");
            String Address1 = GetString(row, "address_1");
            String Address2 = GetString(row, "address_2");
            String City = GetString(row, "city");
            String State = GetString(row, "state");
            String ZipCode = GetString(row, "zip");
            String Phone = GetString(row, "telephone");
            String Mobile = GetString(row, "sms");
            String Email = GetString(row, "email");
            
            /*ask about HRID*/

            int status = GetInt(row, "status");
            DateTime TermDate = GetDate(row, "term_date");
            if (status <= 0 && TermDate.CompareTo(null) == 0)
            {
                TermDate = DateTime.Today;
            }

            StringBuilder ret = new StringBuilder();
            ret.Append(companyId);
            ret.Append(delim);

            ret.Append(conceptId);
            ret.Append(delim);

            ret.Append(storeNum);
            ret.Append(delim);

            ret.Append(PosId);
            ret.Append(delim);

            ret.Append(FirstName);
            ret.Append(delim);

            ret.Append(LastName);
            ret.Append(delim);

            ret.Append(Phone);
            ret.Append(delim);

            ret.Append(Mobile);
            ret.Append(delim);

            ret.Append(Email);
            ret.Append(delim);

            ret.Append(Address1 + " " + Address2);
            ret.Append(delim);

            ret.Append(City);
            ret.Append(delim);

            ret.Append(State);
            ret.Append(delim);

            ret.Append(ZipCode);
            ret.Append(delim);

            if (status == 1)
            {
                ret.Append("");
            }
            else
            {
                ret.Append(TermDate.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            }


            return ret.ToString();
        }

        private String GetTimecardString(DataRow row)
        {
            int companyId = ClientDetails.ClientCompanyExtRef;
            int conceptId = ClientDetails.ClientConceptExtRef;
            int storeNum = ClientDetails.ClientExtRef;
            int empId = GetInt(row, "emp_id");
            int jobNum = GetInt(row, "job_id");
            DateTime busDate = GetDate(row, "bus_date");
            DateTime punchIn = GetDate(row, "clock_in");
            DateTime punchOut = GetDate(row, "clock_out");

            //convert to minutes
            Double workHours = GetDouble(row, "reg_hours");
            Double ovtHours = GetDouble(row, "ovt_hours");
            int workMinutes = Convert.ToInt32(workHours * 60);
            int ovtMinutes = Convert.ToInt32(ovtHours * 60);
            float regularPay = GetFloat(row, "reg_pay");
            float ovtPay = GetFloat(row, "ovt_pay");
            float specialPay = GetFloat(row, "sp_pay");
            float declaredTips = 0.0f;
            int breakMinutes = 0;

            //allow these to be auto-calculated server-side
            String regWage = "";
            String ovtWage = "";

            StringBuilder ret = new StringBuilder();
            ret.Append(companyId);
            ret.Append(delim);
            
            ret.Append(conceptId);
            ret.Append(delim);
            
            ret.Append(storeNum);
            ret.Append(delim);
            
            ret.Append(empId);
            ret.Append(delim);

            ret.Append(jobNum);
            ret.Append(delim);

            ret.Append(busDate.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            ret.Append(delim);

            ret.Append(punchIn.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            ret.Append(delim);

            ret.Append(punchOut.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            ret.Append(delim);

            ret.Append(workMinutes);
            ret.Append(delim);

            ret.Append(ovtMinutes);
            ret.Append(delim);

            ret.Append(regularPay);
            ret.Append(delim);

            ret.Append(ovtPay);
            ret.Append(delim);

            ret.Append(specialPay);
            ret.Append(delim);

            ret.Append(declaredTips);
            ret.Append(delim);

            ret.Append(breakMinutes);
            ret.Append(delim);

            ret.Append(regWage);
            ret.Append(delim);

            ret.Append(ovtWage);

            return ret.ToString();
        }

        private String GetSalesString(DataRow row)
        {
            int companyId = ClientDetails.ClientCompanyExtRef;
            int conceptId = ClientDetails.ClientConceptExtRef;
            int storeNum = ClientDetails.ClientExtRef;
            DateTime busDate = GetDate(row, "bus_date");
            DateTime transDate = GetDate(row, "time");
            float amount = GetFloat(row, "amount");
            int empId = GetInt(row, "emp_id");
            int revCenter = GetInt(row, "rev_code");
            int salesCat = GetInt(row, "sales_cat");

            //TODO: figure out what we can reference for orderId
            String orderId = "-1";

            StringBuilder ret = new StringBuilder();
            ret.Append(companyId);
            ret.Append(delim);

            ret.Append(conceptId);
            ret.Append(delim);

            ret.Append(storeNum);
            ret.Append(delim);

            ret.Append(busDate.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            ret.Append(delim);

            ret.Append(transDate.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            ret.Append(delim);

            ret.Append(amount);
            ret.Append(delim);

            ret.Append(empId);
            ret.Append(delim);

            ret.Append(revCenter);
            ret.Append(delim);

            ret.Append(salesCat);
            ret.Append(delim);

            ret.Append(orderId);

            return ret.ToString();
        }

        private String GetEmpJobString(DataRow row)
        {
            int companyId = ClientDetails.ClientCompanyExtRef;
            int conceptId = ClientDetails.ClientConceptExtRef;
            int storeNum = ClientDetails.ClientExtRef;
            int empPosId = GetInt(row, "emp_pos_id");
            int jobNum = GetInt(row, "job_pos_id");
            double regRate = GetDouble(row, "reg_rate");
            String ovtValues = GetString(row, "ovt_rate");
            String myOvt = parseOvtValue(ovtValues);
            double ovtRate = Double.Parse(myOvt);
            int isPrimary = GetInt(row, "is_primary");

            StringBuilder ret = new StringBuilder();
            ret.Append(companyId);
            ret.Append(delim);

            ret.Append(conceptId);
            ret.Append(delim);

            ret.Append(storeNum);
            ret.Append(delim);

            ret.Append(empPosId);
            ret.Append(delim);

            ret.Append(jobNum);
            ret.Append(delim);

            ret.Append(regRate);
            ret.Append(delim);

            ret.Append(ovtRate);
            ret.Append(delim);

            ret.Append(isPrimary);

            return ret.ToString();
        }

        private String GetJobString(DataRow row)
        {
            int companyId = ClientDetails.ClientCompanyExtRef;
            int conceptId = ClientDetails.ClientConceptExtRef;
            int storeNum = ClientDetails.ClientExtRef;
            int jobNum = GetInt(row, "job_pos_id");
            String jobName = GetString(row, "job_name");
            double regRate = 0.0;
            double ovtRate = 0.0;

            StringBuilder ret = new StringBuilder();
            ret.Append(companyId);
            ret.Append(delim);

            ret.Append(conceptId);
            ret.Append(delim);

            ret.Append(storeNum);
            ret.Append(delim);

            ret.Append(jobNum);
            ret.Append(delim);

            ret.Append(jobName);
            ret.Append(delim);

            ret.Append(regRate);
            ret.Append(delim);

            ret.Append(ovtRate);

            return ret.ToString();
        }

        private String GetRvcString(DataRow row)
        {
            int companyId = ClientDetails.ClientCompanyExtRef;
            int conceptId = ClientDetails.ClientConceptExtRef;
            int storeNum = ClientDetails.ClientExtRef;
            int rvc = GetInt(row, "rvc");
            string name = GetString(row, "name");

            StringBuilder ret = new StringBuilder();
            ret.Append(companyId);
            ret.Append(delim);

            ret.Append(conceptId);
            ret.Append(delim);

            ret.Append(storeNum);
            ret.Append(delim);

            ret.Append(rvc);
            ret.Append(delim);

            ret.Append(name);

            return ret.ToString();
        }

        private String GetGcString(DataRow row)
        {
            int companyId = ClientDetails.ClientCompanyExtRef;
            int conceptId = ClientDetails.ClientConceptExtRef;
            int storeNum = ClientDetails.ClientExtRef;
            DateTime busDate = GetDate(row, "bus_date");
            DateTime time = GetDate(row, "time");
            int guests = GetInt(row, "guests");
            int rvc = GetInt(row, "rvc");

            StringBuilder ret = new StringBuilder();
            ret.Append(companyId);
            ret.Append(delim);

            ret.Append(conceptId);
            ret.Append(delim);

            ret.Append(storeNum);
            ret.Append(delim);

            ret.Append(busDate.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            ret.Append(delim);

            ret.Append(time.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            ret.Append(delim);

            ret.Append(guests);
            ret.Append(delim);

            ret.Append(rvc);

            return ret.ToString();
        }

        private String GetSalesCatString(DataRow row)
        {
            int companyId = ClientDetails.ClientCompanyExtRef;
            int conceptId = ClientDetails.ClientConceptExtRef;
            int storeNum = ClientDetails.ClientExtRef;
            int catId = GetInt(row, "catId");
            string name = GetString(row, "name");

            StringBuilder ret = new StringBuilder();
            ret.Append(companyId);
            ret.Append(delim);

            ret.Append(conceptId);
            ret.Append(delim);

            ret.Append(storeNum);
            ret.Append(delim);

            ret.Append(catId);
            ret.Append(delim);

            ret.Append(name);

            return ret.ToString();
        }

        private String parseOvtValue(String ovt)
        {
            if (ovt == null || ovt.Length == 0)
            {
                return "1.5";
            }
            string next = ovt.Substring(ovt.IndexOf(',') + 1);
            string ret = next.Substring(0, next.IndexOf(','));
            return ret;
        }
    }
}
