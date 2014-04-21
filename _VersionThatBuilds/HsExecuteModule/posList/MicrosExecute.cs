using System;
using HsSharedObjects.Main;
using System;
using System.Data;
using Microsoft.Data.Odbc;
using System.Collections;
using System.IO;
using System.Windows.Forms;

namespace HsExecuteModule.posList
{
    /// <summary>
    /// Summary description for MicrosExecute.
    /// </summary>
    public class MicrosExecute : BaseExecute
    {
        private SysLog logger = new SysLog(typeof(SquirrelExecute));
        private DateTime StartDate;
        private DateTime EndDate;
        private readonly String dir = Application.StartupPath + "\\MICROS_Files";

        public MicrosExecute(bool map)
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public override void Execute(bool map)
        {
            logger.Debug("Executing MicrosExecute");
            foreach (Command cmd in commands)
            {
                try
                {
                    if (cmd.Cmd.Equals("HistoricalSales"))
                    {
                        logger.Error("ERROR: attempting to run Micros HistoricalSales sync via FileTransfer. Use Historical Sales Module instead.");
                        return;
                    }
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
            logger.Debug("Running MicrosExecute");

            /*
             * TODO: when we want to run additional commands, 
             * add a switch statement here to perform the appropriate
             * "GenerateFiles" for the given command
             * 
             * For now leave as-is, as the only command being run
             * here should be PayControl export
             */
            GenerateFiles(cmd);
        }

        private void GenerateFiles(Command cmd)
        {
            TextWriter writer = null;
            try
            {
                ArrayList strings = DbLoad();
                //include clientId in the filename for support's sanity
                String file = dir + "\\" + "PUNCH_" + ClientDetails.ClientId + "_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                CheckFileExistence(file);
                writer = new StreamWriter(file);
                foreach (String s in strings)
                {
                    writer.Write(s);
                    writer.WriteLine();
                }
            }
            catch (Exception ex)
            {
                logger.Error("ERROR running command in MicrosExecute: " + cmd.Cmd + " / " + cmd.Args, ex);
            }
            finally
            {
                writer.Flush();
                writer.Close();
            }
        }

        private void CheckFileExistence(string file)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (File.Exists(file))
                File.Delete(file);
        }

        public ArrayList DbLoad()
        {
            OdbcConnection newConnection = new OdbcConnection(ClientDetails.GetConnectionString());
            ArrayList strings = new ArrayList();
            try
            {
                // first gather regular timecard info
                DataRowCollection rows = GatherTimecards(newConnection);

                //now gather tip info associated with these timecards
                DataRowCollection tipRows = GatherTips(newConnection);

                foreach (DataRow row in rows)
                {
                    try
                    {
                        //construct the export row and add it to the list
                        String st = constructPayControlRecord(row, tipRows);
                        strings.Add(st);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error adding micros time card in Load(): " + ex.ToString());
                    }
                }//foreach timecard
            }
            catch (Exception ex)
            {
                logger.Error("Error retrieving MICROS timecards in Load(): ");
                logger.Error(ex.ToString());
            }
            finally
            {
                newConnection.Close();
            }
            return strings;
        }

        public String constructPayControlRecord(DataRow row, DataRowCollection tipRows)
        {
            int ExtId = GetInt(row, "TIME_CARD_SEQ");
            int EmpPosId = GetInt(row, "EMP_SEQ");
            String JobName = GetString(row, "JOB_NAME");
            int JobExtId = GetInt(row, "job_number");
            float RegHours = GetFloat(row, "regular_hours");
            float RegTotal = GetFloat(row, "regular_ttl");
            float OvtHours = GetFloat(row, "overtime_hours");
            float OvtTotal = GetFloat(row, "overtime_ttl");
            DateTime BusinessDate = GetDate(row, "BUSINESS_DATE");
            DateTime ClockIn = GetDate(row, "clock_in_datetime");
            DateTime ClockOut = GetDate(row, "clock_out_datetime") > new DateTime(0) ? GetDate(row, "clock_out_datetime") : DateTime.Now;
            int OvertimeMinutes = (int)Math.Round(GetDouble(row, "overtime_hours") * 60, 0);
            float RegWage = 0.0f;
            float OvtWage = 0.0f;
            if (RegHours > 0 && RegTotal > 0)
                RegWage = (RegTotal / RegHours);
            if (OvtHours > 0 && OvtTotal > 0)
                OvtWage = (OvtTotal / OvtHours);

            //see if this employee has tips declared during this shift
            float tips = 0.0F;
            foreach (DataRow tipRow in tipRows)
            {
                if (!DBNull.Value.Equals(tipRow["trans_emp_seq"]) && GetInt(tipRow, "trans_emp_seq") == EmpPosId)
                {
                    if (!DBNull.Value.Equals(tipRow["start_date_tm"]))
                    {
                        DateTime transDate = GetDate(tipRow, "start_date_tm");
                        if (transDate > ClockIn && transDate < ClockOut)
                        {
                            //add this tip to this shift's total if value not null
                            if (!DBNull.Value.Equals(tipRow["TIPS"]))
                            {
                                tips += GetFloat(tipRow, "TIPS");
                            }//tip not null
                        }//if date
                    }//date not null
                }//if emp
            }//foreach tip

            String st = ClientDetails.ClientCompanyExtRef + "|" + ClientDetails.ClientConceptExtRef + "|" + ClientDetails.ClientExtRef
                + "|" + EmpPosId + "|" + JobExtId + "|" + BusinessDate.ToString("yyyy-MM-dd HH:mm:ss.SSS")
                + "|" + ClockIn.ToString("yyyy-MM-dd HH:mm:ss.SSS") + "|" + ClockOut.ToString("yyyy-MM-dd HH:mm:ss.SSS")
                + "|" + (RegHours * 60.0f) + "|" + (OvtHours * 60.0f) + "|" + (RegWage * RegHours) + "|" + (OvtWage * OvtHours)
                + "|" + "0.0" //special pay
                + "|" + tips //declared tips
                + "|" + "0.0" //break minutes
                + "|" + RegWage + "|" + OvtWage;

            return st;
        }

        public DataRowCollection GatherTimecards(OdbcConnection conn)
        {

            this.StartDate = DateTime.Now;

            this.EndDate = DateTime.Now.AddDays(-14);
            DataSet dataSet = new DataSet();
            OdbcDataAdapter dataAdapter = new OdbcDataAdapter(
                        "select BUSINESS_DATE , EMP_SEQ , TIME_CARD_SEQ , JOB_NAME , job_number ," +
                        "clock_in_datetime,clock_out_datetime,overtime_ttl,overtime_hours,regular_ttl,regular_hours, " +
                        "adjust_reason as ADJUST_REASON from micros.v_R_employee_time_card where BUSINESS_DATE > ? ", conn);
            dataAdapter.SelectCommand.Parameters.Add("", this.EndDate);
            dataAdapter.Fill(dataSet, "micros.v_R_employee_time_card");
            dataAdapter.Dispose();
            DataRowCollection rows = dataSet.Tables[0].Rows;

            logger.Debug("returned [" + dataSet.Tables[0].Rows.Count + "] rows from the database");

            return rows;
        }

        public DataRowCollection GatherTips(OdbcConnection conn)
        {

            DataSet tipDataSet = new DataSet();
            OdbcDataAdapter tipDataAdapter = new OdbcDataAdapter("select SUM(tips.decl_amt) as TIPS, q.trans_emp_seq, q.start_date_tm from micros.tip_declared_dtl tips, " +
                " (select trans_seq, trans_emp_seq, start_date_tm from micros.trans_dtl where BUSINESS_DATE > ? ) q " +
                " where tips.trans_seq = q.trans_seq " +
                " GROUP BY q.trans_emp_seq, q.start_date_tm " +
                " order by trans_emp_seq asc ", conn);
            tipDataAdapter.SelectCommand.Parameters.Add("", this.EndDate);
            tipDataAdapter.Fill(tipDataSet, "micros.tip_declared_dtl");
            tipDataAdapter.Dispose();
            DataRowCollection tipRows = tipDataSet.Tables[0].Rows;

            logger.Debug("returned [" + tipDataSet.Tables[0].Rows.Count + "] tip rows from the database");

            return tipRows;
        }



        /*
         * Expected columns, from multiclient.TimeCardProcessor
         * 		public static final int CompanyNum = 0;
	public static final int ConceptNum = 1;
	public static final int StoreNum = 2;
	public static final int EmpID = 3;
	public static final int JobNo = 4;
	public static final int BusinessDate = 5;
	public static final int PunchIn = 6;
	public static final int PunchOut = 7;
	public static final int WorkMinutes = 8;
	public static final int OvertimeMinutes = 9;
	public static final int RegularPay = 10;
	public static final int OvertimePay = 11;
	public static final int SpecialPay = 12;
	public static final int DeclaredTips = 13;
	public static final int BreakMinutes = 14;
	public static final int RegWage = 15;
	public static final int OvtWage = 16;
         */


        public String GetString(DataRow row, String column)
        {
            String str = "";
            try
            {
                return (String)row[column].ToString();
            }
            catch (Exception ex)
            {
                logger.Error("Error converting data in [" + column + "] to string: " + ex.ToString());
            }
            return str;
        }

        public DateTime GetDate(DataRow row, String column)
        {
            DateTime date = new DateTime(1, 1, 1);
            try
            {
                if (row[column].ToString().Length < 1) return date;
                return (DateTime)row[column];
            }
            catch (Exception ex)
            {
                logger.Error("Error converting data in [" + column + "] to date: " + ex.ToString());
            }
            return date;
        }

        public int GetInt(DataRow row, String column)
        {
            int num = -1;
            try
            {
                if (row[column].ToString().Equals("")) return -1;
                return Convert.ToInt32(row[column]);
            }
            catch (Exception ex)
            {
                logger.Error("Error converting data in [" + column + "] to int: " + ex.ToString());
            }
            return num;
        }

        public double GetDouble(DataRow row, String column)
        {
            double num = 0.0;
            try
            {
                if (row[column].ToString().Equals("")) return 0.0;
                return Convert.ToDouble(row[column]);
            }
            catch (Exception ex)
            {
                logger.Error("Error converting data in [" + column + "] to dbl: " + ex.ToString());
            }
            return num;
        }

        public float GetFloat(DataRow row, String column)
        {
            float num = 0.0f;
            try
            {
                if (row[column].ToString().Equals("")) return 0.0f;
                return (float)Convert.ToDecimal(row[column]);
            }
            catch (Exception ex)
            {
                logger.Error("Error converting data in [" + column + "] to dbl: " + ex.ToString());
            }
            return num;
        }
    }
}
