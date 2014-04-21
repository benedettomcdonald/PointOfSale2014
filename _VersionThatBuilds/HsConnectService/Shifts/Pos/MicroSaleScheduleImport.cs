using System;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;
using HsSharedObjects;
using HsSharedObjects.Main;

namespace HsConnect.Shifts.Pos
{
    class MicroSaleScheduleImport : ScheduleImportImpl
    {
//        private OleDbConnection conn;
//        private static readonly int PREF_CUSTOM_IMPORT_DAYS = 1018;
        private readonly String MICROSALE_DIR = Application.StartupPath + "\\MicroSale";
//        private MapFile empIdMap;
//        private MapFile jobIdMap;
        private SysLog logger = new SysLog(typeof(MicroSaleScheduleImport));

        public override void Execute()
        {
            logger.Log("Executing MicroSale Schedule Import");
            try
            {
                MapFile empIdMap = new MapFile(MICROSALE_DIR + "\\EmployeeMap.csv", true);
                MapFile jobIdMap = new MapFile(MICROSALE_DIR + "\\JobMap.csv", true);
                Hashtable weekHash = getWeekHash();
                foreach(String weekKey in weekHash.Keys)
                {
                    logger.Debug("Starting sched-" + weekKey + ".csv");
                    try
                    {
                        CsvFile scheduleCsv = new CsvFile(',');
                        ArrayList weekShifts = (ArrayList)weekHash[weekKey];
                        foreach (Shift hsShift in weekShifts)
                        {
                            try
                            {
                                ArrayList shiftRow = new ArrayList();
                                shiftRow.Add(hsShift.ClockIn.ToString("M/d/yyyy"));
                                shiftRow.Add(hsShift.ClockIn.Hour);
                                shiftRow.Add(hsShift.ClockIn.Minute);
                                shiftRow.Add(hsShift.ClockOut.Hour);
                                shiftRow.Add(hsShift.ClockOut.Minute);
                                shiftRow.Add(empIdMap.GetKeyByValue(hsShift.PosEmpId + ""));
                                shiftRow.Add(jobIdMap.GetKeyByValue(hsShift.PosJobId + ""));
                                scheduleCsv.AddRow(shiftRow);
                            }
                            catch (Exception ex)
                            {
                                logger.Error("Error adding shift for employee [" + hsShift.PosEmpId + "] at time [" +
                                             hsShift.ClockIn + "]", ex);
                            }
                        }
                        bool saved = scheduleCsv.SaveAs("C:\\hotschedules\\sched-" + weekKey + ".csv");
                        if (saved)
                            logger.Debug("Saved schedule file C:\\hotschedules\\sched-" + weekKey + ".csv");
                        else
                            logger.Error("Error saving schedule file C:\\hotschedules\\sched-" + weekKey + ".csv");
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error creating schedule file C:\\hotschedules\\sched-" + weekKey + ".csv", ex);
                    }
                }
            }
            catch(Exception ex)
            {
                logger.Error("Error executing MicroSale Schedule Import", ex);
            }

            logger.Log("MicroSale Schedule Import Complete");

//            logger.Debug("Executing Squirrel Schedule Import");
//            try
//            {
//                empIdMap = new MapFile(MICROSALE_DIR + "\\EmployeeMap.csv", true);
//                jobIdMap = new MapFile(MICROSALE_DIR + "\\JobMap.csv", true);
//
//                conn = new OleDbConnection(Details.GetConnectionString().Replace("[filename]", "EmpSchedules.mdb"));
//                conn.Open();
//
//                ShiftList posShifts = GetPosShifts();
//                ShiftList hsShifts = Shifts;
//
//                ShiftList shiftsToInsert = new ShiftList();
//                ShiftList shiftsToUpdate = new ShiftList();
//                ShiftList shiftsToDelete = new ShiftList();
//
//                CompareShiftLists(hsShifts, posShifts, shiftsToUpdate, shiftsToInsert, shiftsToDelete);
//                SyncShiftLists(shiftsToInsert, shiftsToUpdate, shiftsToDelete);
//            }
//            catch (Exception ex)
//            {
//                logger.Error("Error executing Squirrel Schedule Import");
//                logger.Error(ex.ToString());
//            }
//            finally
//            {
//                conn.Close();
//            }
        }

        public Hashtable getWeekHash()
        {
            Hashtable result = new Hashtable();
            try
            {
                ShiftList hsShifts = Shifts;
                logger.Debug(hsShifts.Count + " shifts found");
                foreach (Shift hsShift in hsShifts)
                {
                    try
                    {
                        String weekStart = getWeekStart(hsShift.ClockIn);
                        if (result[weekStart] == null)
                            result[weekStart] = new ArrayList();
                        ((ArrayList)result[weekStart]).Add(hsShift);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(
                            "Error sorting shift for employee [" + hsShift.PosEmpId + "] at time [" + hsShift.ClockIn + "]", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error sorting shifts by week", ex);
            }
            logger.Debug(result.Count + " weeks found");
            return result;
        }

        private string getWeekStart(DateTime time)
        {
            String result = time.ToString("yyyyMMdd");
            DateTime weekStart = time;
            while (weekStart.DayOfWeek != Details.DayOfWeekStart)
                weekStart = weekStart.AddDays(-1);
            result = weekStart.ToString("yyyyMMdd");
            logger.Debug("Week start is: " + result);
            return result;
        }

        public override DateTime CurrentWeek
        {
            get { return DateTime.Today; }
            set { }
        }

        public override void GetWeekDates()
        {
        }
//
//        private ShiftList GetPosShifts()
//        {
//            logger.Debug("Getting MicroSale POS Shifts");
//            ShiftList posShifts = new ShiftList();
//
//            int numberOfDays = GetNumberOfDays();
//            OleDbDataReader reader = null;
//            try
//            {
//                String select = SELECT_SHIFTS;
//                select = select.Replace("[StartDate]", DateTime.Today.ToString(SQL_DATETIME_FORMAT));
//                select = select.Replace("[EndDate]", DateTime.Today.AddDays(numberOfDays).ToString(SQL_DATETIME_FORMAT));
//                OleDbCommand cmd = new OleDbCommand(select, conn);
//                reader = cmd.ExecuteReader();
//
//                while (reader.Read())
//                {
//                    try
//                    {
//                        DateTime date = DateTime.Parse(reader["SchDate"].ToString());
//                        String jobCode = reader["Position"].ToString();
//                        int shiftId = Int32.Parse(reader["RowNum"].ToString());
//                        String empName = reader["EmpName"].ToString();
//                        int startTime = Int32.Parse(reader["StartTime"].ToString());
//                        int endTime = Int32.Parse(reader["EndTime"].ToString());
//
//                        int empId = Int32.Parse(empIdMap[empName]);
//                        int jobId = Int32.Parse(jobIdMap[jobCode]);
//
//                        DateTime inTime = date.AddMinutes(30*startTime);
//                        DateTime outTime = date.AddMinutes(30*endTime);
//
//                        Shift posShift = new Shift();
//                        posShift.PosId = shiftId;
//                        posShift.PosEmpId = empId;
//                        posShift.PosJobId = jobId;
//                        posShift.ClockIn = inTime;
//                        posShift.ClockOut = outTime;
//
//                        posShifts.Add(posShift);
//                    }
//                    catch (Exception ex)
//                    {
//                        logger.Error("Error parsing MicroSale Shift");
//                        logger.Error(ex.ToString());
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                logger.Error("Error reading MicroSale Schedule Shifts");
//                logger.Error(ex.ToString());
//            }
//            finally
//            {
//                if (reader != null)
//                    reader.Close();
//            }
//
//            return posShifts;
//        }
//
//        private int GetNumberOfDays()
//        {
//            logger.Debug("Getting number of days to sync MicroSale Shifts");
//            int numberOfDays = 7;
//            if (Details.Preferences.PrefExists(PREF_CUSTOM_IMPORT_DAYS))
//            {
//                try
//                {
//                    Preference pref = Details.Preferences.GetPreferenceById(PREF_CUSTOM_IMPORT_DAYS);
//                    if (pref != null)
//                    {
//                        int days = Int32.Parse(pref.Val2);
//                        if (days > 0)
//                            numberOfDays = days;
//                    }
//                }
//                catch (Exception ex)
//                {
//                    logger.Error("Error getting number of days to sync schedules, using 6.");
//                    logger.Error(ex.ToString());
//                }
//            }
//            return numberOfDays;
//        }
//
//        private void CompareShiftLists(ShiftList hsShifts, ShiftList posShifts, ShiftList shiftsToUpdate, ShiftList shiftsToInsert, ShiftList shiftsToDelete)
//        {
//            logger.Debug("Comparing Squirrel Shift Lists");
//            foreach (Shift hsShift in hsShifts)
//            {
//                if (posShifts.Contains(hsShift))
//                {
//                    Shift posShift = posShifts.Get(hsShift);
//                    if (hsShift.ClockIn.Hour != posShift.ClockIn.Hour ||
//                        hsShift.ClockIn.Minute != posShift.ClockIn.Minute ||
//                        hsShift.ClockOut.Hour != posShift.ClockOut.Hour ||
//                        hsShift.ClockOut.Minute != posShift.ClockOut.Minute)
//                    {
//                        hsShift.PosId = posShift.PosId;
//                        if (hsShift.PosEmpId > 0)
//                            shiftsToUpdate.Add(hsShift);
//                    }
//                }
//                else
//                {
//                    shiftsToInsert.Add(hsShift);
//                }
//            }
//
//            foreach (Shift posShift in posShifts)
//            {
//                if (!hsShifts.Contains(posShift))
//                    if (posShift.PosEmpId > 0)
//                        shiftsToDelete.Add(posShift);
//            }
//        }
//
//        private void SyncShiftLists(ShiftList shiftsToInsert, ShiftList shiftsToUpdate, ShiftList shiftsToDelete)
//        {
//            InsertShiftsIntoPOS(shiftsToInsert);
//            UpdateShiftsInPOS(shiftsToUpdate);
//            DeleteShiftsFromPOS(shiftsToDelete);
//        }
//
//        private void InsertShiftsIntoPOS(ShiftList shiftsToInsert)
//        {
//            logger.Debug("Inserting " + shifts.Count + " MicroSale Shifts");
//            foreach(Shift shift in shiftsToInsert)
//            {
//                int rowNum = -1;
//                try
//                {
//                    rowNum = GetNextRowNum();
//
//                    if (rowNum>0)
//                    {
//                        String insert = INSERT_SHIFT;
//                        insert = insert.Replace("[SchDate]", shift.ClockIn.ToString(MS_DATE_FORMAT));
//                        insert = insert.Replace("[Position]", jobIdMap.GetKeyByValue(shift.PosJobId + ""));
//                        insert = insert.Replace("[RowNum]", rowNum+"");
//                        insert = insert.Replace("[EmpName]", empIdMap.GetKeyByValue(shift.PosEmpId + ""));
//                        insert = insert.Replace("[StartTime]", 2 * shift.ClockIn.TimeOfDay.Hours+"");
//                        insert = insert.Replace("[EndTime]", 2 * shift.ClockOut.TimeOfDay.Hours + "");
//
//                        OleDbCommand cmd = new OleDbCommand(insert, conn);
//                        bool inserted = cmd.ExecuteNonQuery() > 0;
//                        if (inserted)
//                            logger.Debug("Successfully inserted shift [" + rowNum + "] for employee [" + shift.PosEmpId +
//                                         "] into MicroSale");
//                        else
//                            logger.Error("Error inserting shift [" + rowNum + "] for employee [" + shift.PosEmpId +
//                                         "] into MicroSale");   
//                    }
//                    else
//                        logger.Error("Unable to get next row number");
//                }
//                catch (Exception ex)
//                {
//                    logger.Error("Error inserting shift [" + rowNum + "] for employee [" + shift.PosEmpId +
//                                 "] into MicroSale");
//                    logger.Error(ex.ToString());
//                }
//            }
//        }
//
//        private int GetNextRowNum()
//        {
//            logger.Debug("Counting MicroSale shifts");
//            int nextPositionNo = 0;
//            OleDbDataReader reader = null;
//            try
//            {
//                OleDbCommand cmd = new OleDbCommand(MAX_ROWNUM, conn);
//                reader = cmd.ExecuteReader();
//                if (reader.Read())
//                {
//                    String val = reader[0].ToString();
//                    logger.Debug("Current max row number is: " + val);
//                    if (val != null && !val.Equals(""))
//                        nextPositionNo = Int32.Parse(val) + 1;
//                    else
//                        nextPositionNo = 1;
//                }
//                return nextPositionNo;
//            }
//            catch (Exception e)
//            {
//                logger.Error("Error getting next row number from MicroSale");
//                logger.Error(e.ToString());
//                return -1;
//            }
//            finally
//            {
//                if (reader != null)
//                    reader.Close();
//            }
//        }
//
//        private void UpdateShiftsInPOS(ShiftList shiftsToUpdate)
//        {
//            logger.Debug("Updating " + shifts.Count + " MicroSale Shifts");
//            foreach (Shift shift in shiftsToUpdate)
//            {
//                try
//                {
//                    String startTime = (2 * shift.ClockIn.Hour).ToString();
//                    String endTime = (2 * shift.ClockOut.Hour).ToString();
//
//                    String update = UPDATE_SHIFT;
//                    update = update.Replace("[StartTime]", startTime);
//                    update = update.Replace("[EndTime]", endTime);
//                    update = update.Replace("[RowNum]", shift.PosId.ToString());
//
//                    OleDbCommand cmd = new OleDbCommand(update, conn);
//                    bool updated = cmd.ExecuteNonQuery() > 0;
//                    if (!updated)
//                        logger.Error("Error updating shift [" + shift.PosId + "] for employee [" + shift.PosEmpId +
//                                     "] in MicroSale");
//                }
//                catch (Exception ex)
//                {
//                    logger.Error("Error updating shift [" + shift.PosId + "] for employee [" + shift.PosEmpId + "] in MicroSale");
//                    logger.Error(ex.ToString());
//                }
//            }
//        }
//
//        private void DeleteShiftsFromPOS(ShiftList shiftsToDelete)
//        {
//            logger.Debug("Deleting " + shifts.Count + " MicroSale Shifts");
//            foreach (Shift shift in shiftsToDelete)
//            {
//                try
//                {
//                    String delete = DELETE_SHIFT.Replace("[RowNum]", shift.PosId.ToString());
//
//                    OleDbCommand cmd = new OleDbCommand(delete, conn);
//                    bool deleted = cmd.ExecuteNonQuery() > 0;
//                    if (!deleted)
//                        logger.Error("Error deleting shift [" + shift.PosId + "] for employee [" + shift.PosEmpId +
//                                     "] from MicroSale");
//                }
//                catch (Exception ex)
//                {
//                    logger.Error("Error deleting shift [" + shift.PosId + "] for employee [" + shift.PosEmpId +
//                                 "] from MicroSale");
//                    logger.Error(ex.ToString());
//                }
//            }
//        }
//
//        private static readonly String SQL_DATETIME_FORMAT = "yyyy-MM-dd HH:mm";
//        private static readonly String MS_DATE_FORMAT = "MM/dd/yy HH:mm tt";
//
//        private static readonly String SELECT_SHIFTS = "SELECT * FROM MasterSchedule " +
//                                                       " WHERE CDATE(SchDate)>=CDATE('[StartDate]') AND CDATE(SchDate) <=CDATE('[EndDate]')";
//
//        private static readonly String INSERT_SHIFT = "INSERT INTO MasterSchedule " +
//                                                      "(SchDate, Position, WorkArea, RowNum, EmpName, StartTime, EndTime) " +
//                                                      "VALUES ('[SchDate]', '[Position]', '', [RowNum], '[EmpName]', [StartTime], [EndTime])";
//
//        private static readonly String MAX_ROWNUM = "SELECT MAX(RowNum) FROM MasterSchedule";
//
//        private static readonly String UPDATE_SHIFT = "UPDATE MasterSchedule SET StartTime=[StartTime], EndTime=[EndTime] WHERE RowNum=[RowNum]";
//
//        private static readonly String DELETE_SHIFT = "DELETE FROM MasterSchedule WHERE RowNum=[RowNum]";
    }
}
