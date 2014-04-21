using System;
using System.Data.SqlClient;
using HsExecuteModule.posList;
using HsSharedObjects.Client.Preferences;
using HsSharedObjects.Client.Shift;
using HsSharedObjects.Main;

namespace HsConnect.Shifts.Pos
{
	class SquirrelScheduleImport : ScheduleImportImpl
	{
		private static readonly int PREF_CUSTOM_IMPORT_DAYS = 1018;
		private SqlConnection conn;
		private SysLog logger = new SysLog(typeof(SquirrelScheduleImport));

		public override void Execute()
		{
            if (Details.Preferences.PrefExists(Preference.SQUIRREL_USE_BIG_JOB_ADJ))
            {
                SquirrelExecute.NEG_JOB_CODE_ADJ = 1000000;
            }
			logger.Debug("Executing Squirrel Schedule Import");
			try
			{
				conn = new SqlConnection(Details.GetConnectionString());
				conn.Open();

				SyncShiftLists(GetHsShifts(), GetPosShifts());
			}
			catch (Exception ex)
			{
				logger.Error("Error executing Squirrel Schedule Import");
				logger.Error(ex.ToString());
			}
			finally
			{
				conn.Close();
			}
		}

		private void SyncShiftLists(ShiftList hsShifts, ShiftList posShifts)
		{
            ShiftList shiftsToDelete = FindDeletes(hsShifts, posShifts);
			DeleteShiftsFromPos(shiftsToDelete);

            ShiftList shiftsToInsert = FindInserts(hsShifts, posShifts);
            InsertShiftsIntoPos(shiftsToInsert);
		}

	    private ShiftList FindDeletes(ShiftList hsShifts, ShiftList posShifts)
	    {
	        logger.Debug("Finding needed shift deletes");
            ShiftList shiftsToDelete = new ShiftList();
            foreach (Shift posShift in posShifts)
            {
                if (!hsShifts.ContainsShift(posShift) && posShift.PosEmpId > 0)
                {
                    logger.Debug("Will DELETE: " + posShift);
                    shiftsToDelete.Add(posShift);
                }
            }
	        return shiftsToDelete;
        }

        private void DeleteShiftsFromPos(ShiftList shiftsToDelete)
        {
            logger.Debug("Deleting " + shiftsToDelete.Count + " Squirrel Shifts");
            foreach (Shift shift in shiftsToDelete)
            {
                try
                {
                    String delete = DELETE_SHIFT.Replace("[ShiftNo]", shift.PosId.ToString());

                    SqlCommand cmd = new SqlCommand(delete, conn);
                    bool deleted = cmd.ExecuteNonQuery() > 0;
                    if (!deleted)
                        logger.Error("Error deleting shift [" + shift.PosId + "] for employee [" + shift.PosEmpId +
                            "] from Squirrel");
                }
                catch (Exception ex)
                {
                    logger.Error("Error deleting shift [" + shift.PosId + "] for employee [" + shift.PosEmpId +
                        "] from Squirrel");
                    logger.Error(ex.ToString());
                }
            }
        }

        private ShiftList FindInserts(ShiftList hsShifts, ShiftList posShifts)
        {
            ShiftList shiftsToInsert = new ShiftList();
            logger.Debug("Finding needed shift inserts");
            foreach (Shift hsShift in hsShifts)
            {
                if (!posShifts.ContainsShift(hsShift))
                {
                    logger.Debug("Will INSERT: " + hsShift.ToString());
                    shiftsToInsert.Add(hsShift);
                }
            }
            return shiftsToInsert;
        }

	    private void InsertShiftsIntoPos(ShiftList shiftsToInsert)
		{
			logger.Debug("Inserting " + shiftsToInsert.Count + " Squirrel Shifts");
			foreach(Shift shift in shiftsToInsert)
			{
				int shiftNo = -1;
				int positionNo = -1;
				try
				{
					SquirrelSchedule schedule = GetSquirrelSchedule(shift);
					shiftNo = GetNextShiftNo();
					positionNo = GetNextPositionNo(shift.ClockIn.Date);

					if (schedule!=null && shiftNo>0 && positionNo>0)
					{
						String insert = INSERT_SHIFT;
						insert = insert.Replace("[ShiftNo]", shiftNo.ToString());
						insert = insert.Replace("[ScheduleNo]", schedule.ScheduleNo.ToString());
						insert = insert.Replace("[DeptNo]", schedule.DeptNo.ToString());
						insert = insert.Replace("[PositionNo]", positionNo.ToString());
						insert = insert.Replace("[EmpID]", shift.PosEmpId.ToString());
						insert = insert.Replace("[JobNo]", shift.PosJobId.ToString());
						insert = insert.Replace("[ShiftDate]", shift.ClockIn.Date.ToString(SQL_DATETIME_FORMAT));
						insert = insert.Replace("[StartTime]", shift.ClockIn.ToString(SQL_DATETIME_FORMAT));
						insert = insert.Replace("[EndTime]", shift.ClockOut.ToString(SQL_DATETIME_FORMAT));

						SqlCommand cmd = new SqlCommand(insert, conn);
						bool inserted = cmd.ExecuteNonQuery() > 0;
						if (inserted)
							logger.Debug("Successfully inserted shift [" + shiftNo + "] for employee [" + shift.PosEmpId +
								"] into Squirrel");  
						else
							logger.Error("Error inserting shift [" + shiftNo + "] for employee [" + shift.PosEmpId +
								"] into Squirrel");  
					}
					else
						logger.Error("Unable to find Squirrel Schedule to insert shift with job [" + shift.PosJobId + "]");
				}
				catch (Exception ex)
				{
					logger.Error("Error inserting shift  for employee [" + shift.PosEmpId + "] for job [" + shift.PosJobId + "into Squirrel");
					logger.Error(ex.ToString());
				}
			}
		}

		private SquirrelSchedule GetSquirrelSchedule(Shift shift)
		{
			SquirrelSchedule schedule = null;
			SqlDataReader reader = null;
			try
			{

				String query = SCHEDULE_BY_EMPJOB;
				query = query.Replace("[EmpID]", shift.PosEmpId.ToString());
				query = query.Replace("[JobNo]", shift.PosJobId.ToString());
				SqlCommand cmd = new SqlCommand(query, conn);

				reader = cmd.ExecuteReader();
				if(!reader.Read())
				{
					reader.Close();

					query = SCHEDULE_BY_JOB;
					query = query.Replace("[JobNo]", shift.PosJobId.ToString());
					cmd.CommandText = query;

					reader = cmd.ExecuteReader();
					reader.Read();
				}
				try
				{
					int scheduleNo = Int32.Parse(reader["ScheduleNo"].ToString());
					int deptNo = Int32.Parse(reader["DeptNo"].ToString());
					schedule = new SquirrelSchedule(scheduleNo, deptNo);
				}
				catch{}

				return schedule;
			}
			finally
			{
				if(reader!=null)
					reader.Close();
			}
		}

		private int GetNextShiftNo()
		{
			int nextShiftNo = 0;

			SqlDataReader reader = null;
			try
			{
				SqlCommand cmd = new SqlCommand(MAX_SHIFTNO, conn);
				reader = cmd.ExecuteReader();
				if (reader.Read())
				{
					String val = reader[0].ToString();
					if (val != null && !val.Equals("")) 
						nextShiftNo = Int32.Parse(val) + 1;
					else
						nextShiftNo = 1;
				}
				return nextShiftNo;
			}
			catch (Exception e)
			{
				logger.Error("Error getting next shift number from Squirrel");
				logger.Error(e.ToString());
				return -1;
			}
			finally
			{
				if (reader != null)
					reader.Close();
			}
		}

		private int GetNextPositionNo(DateTime date)
		{
			int nextPositionNo = 0;
			SqlDataReader reader = null;
			try
			{
				String query = MAX_POSITIONNO;
				query = query.Replace("[ShiftDate]", date.ToString(SQL_DATE_FORMAT));
				SqlCommand cmd = new SqlCommand(query, conn);
                
				reader = cmd.ExecuteReader();
				if (reader.Read())
				{
					String val = reader[0].ToString();
					if (val!=null && !val.Equals(""))
						nextPositionNo = Int32.Parse(val) + 1;
					else
						nextPositionNo = 1;
				}
				return nextPositionNo;
			}
			catch (Exception e)
			{
				logger.Error("Error getting next position number from Squirrel");
				logger.Error(e.ToString());
				return -1;
			}
			finally
			{
				if(reader!=null)
					reader.Close();
			}
		}

		private ShiftList GetPosShifts()
		{
			logger.Debug("Getting Squirrel POS Shifts");
			ShiftList posShifts = new ShiftList();

			int numberOfDays = GetNumberOfDays();
			SqlDataReader reader = null;
			try
			{
				String select = SELECT_SHIFTS;
				select = select.Replace("[StartDate]", DateTime.Today.ToString(SQL_DATETIME_FORMAT));
				select = select.Replace("[EndDate]", DateTime.Today.AddDays(numberOfDays).ToString(SQL_DATETIME_FORMAT));
				SqlCommand cmd = new SqlCommand(select, conn);
				reader = cmd.ExecuteReader();

				while(reader.Read())
				{
					try
					{
						Shift posShift = new Shift();

						posShift.PosId = Int32.Parse(reader["ShiftNo"].ToString());
						posShift.PosEmpId = Int32.Parse(reader["EmpID"].ToString());
						posShift.PosJobId = Int32.Parse(reader["JobNo"].ToString());
						posShift.ClockIn = DateTime.Parse(reader["StartTime"].ToString());
						posShift.ClockOut = DateTime.Parse(reader["EndTime"].ToString());

						posShifts.Add(posShift);
					}
					catch (Exception ex)
					{
						logger.Error("Error parsing Squirrel Shift");
						logger.Error(ex.ToString());
					}
				}
			}
			catch (Exception ex)
			{
				logger.Error("Error reading Squirrel Schedule Shifts");
				logger.Error(ex.ToString());
			}
			finally
			{
				if(reader!=null)
					reader.Close();
			}

			logger.Debug("Found " + posShifts.Count + " shifts in the Squirrel DB");
			return posShifts;
		}

        private ShiftList GetHsShifts()
        {
            ShiftList hsShifts = Shifts;
            foreach(Shift hsShift in hsShifts)
            {
                if (hsShift.PosJobId > SquirrelExecute.NEG_JOB_CODE_ADJ)
                {
                    int oldId = hsShift.PosJobId;
                    int newId = SquirrelExecute.NEG_JOB_CODE_ADJ - hsShift.PosJobId;
                    hsShift.PosJobId = newId;
                    logger.Debug("Converted incoming job code [" + oldId + "] to [" + newId + "]");
                }
            }
            return hsShifts;
        }

        private int GetNumberOfDays()
        {
            logger.Debug("Getting number of days to sync Squirrel Shifts");
            int numberOfDays = 7;
            if (Details.Preferences.PrefExists(PREF_CUSTOM_IMPORT_DAYS))
            {
                try
                {
                    Preference pref = Details.Preferences.GetPreferenceById(PREF_CUSTOM_IMPORT_DAYS);
                    if (pref != null)
                    {
                        int days = Int32.Parse(pref.Val2);
                        if (days > 0)
                            numberOfDays = days;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error getting number of days to sync schedules, using 6.");
                    logger.Error(ex.ToString());
                }
            }
            return numberOfDays;
        }

		private class SquirrelSchedule
		{
			public SquirrelSchedule(int scheduleNo, int deptNo)
			{
				ScheduleNo = scheduleNo;
				DeptNo = deptNo;
			}
			public int ScheduleNo;
			public int DeptNo;
		}

		public override DateTime CurrentWeek
		{
			get { return DateTime.Today; }
			set {}
		}

		public override void GetWeekDates()
		{
		}

		private static readonly String SELECT_SHIFTS =
			"SELECT * FROM Squirrel.dbo.K_ScheduleShift " +
			"WHERE ShiftDate>='[StartDate]' AND ShiftDate<='[EndDate]'";

		private static readonly String INSERT_SHIFT =
			"INSERT INTO Squirrel.dbo.K_ScheduleShift " +
			"(ShiftNo, ScheduleNo, DeptNo, PositionNo, EmpID, JobNo, IsTemplate, ShiftDate, StartTime, EndTime, Status) " +
			"VALUES ([ShiftNo], [ScheduleNo], [DeptNo], [PositionNo], [EmpID], [JobNo], 0, '[ShiftDate]', '[StartTime]', '[EndTime]', 0)";

		private static readonly String UPDATE_SHIFT =
			"UPDATE Squirrel.dbo.K_ScheduleShift " +
			"SET StartTime='[StartTime]', EndTime='[EndTime]'" +
			"WHERE ShiftNo=[ShiftNo]";

		private static readonly String DELETE_SHIFT =
			"DELETE FROM Squirrel.dbo.K_ScheduleShift WHERE ShiftNo=[ShiftNo]";

		private static readonly String MAX_POSITIONNO =
			"SELECT MAX(PositionNo) FROM Squirrel.dbo.K_ScheduleShift WHERE ShiftDate='[ShiftDate]'";

		private static readonly String MAX_SHIFTNO =
			"SELECT MAX(ShiftNo) FROM Squirrel.dbo.K_ScheduleShift";

		private static readonly String SCHEDULE_BY_EMPJOB =
            "SELECT s.* FROM Squirrel.dbo.K_Schedule AS s INNER JOIN Squirrel.dbo.K_ScheduleShift AS ss ON s.ScheduleNo=ss.ScheduleNo " +
			"WHERE ss.EmpID=[EmpID] AND s.JobNo=[JobNo] AND s.Status=0 ORDER BY s.JobNo DESC";

		private static readonly String SCHEDULE_BY_JOB =
			"SELECT * FROM Squirrel.dbo.K_Schedule WHERE JobNo=[JobNo] AND Status=0";

		private static readonly String SQL_DATETIME_FORMAT = "yyyy-MM-dd HH:mm:ss";
		private static readonly String SQL_DATE_FORMAT = "yyyy-MM-dd";
	}
}
