using HsConnect.Shifts;
using HsConnect.Main;
using HsConnect.Data;

using System;
using System.Data;
using Microsoft.Data.Odbc;
using System.Collections;
using HsSharedObjects.Client.Preferences;

namespace HsConnect.Shifts.Pos
{
	public class MicrosScheduleImport : ScheduleImportImpl
	{
		public MicrosScheduleImport()
		{
			this.logger = new SysLog(this.GetType());
		}

		private SysLog logger;
		private HsData data = new HsData();
		private const int MAX_SEQ = 9999999;
        private bool mapEmpIDs = false;
        private Hashtable empIDS = null;

        private void getEmpMap()
        {
            OdbcConnection newConnection = this.cnx.GetOdbc();
            empIDS = new Hashtable();
            DataSet dataSet = new DataSet();
            DateTime temp = DateTime.Now.Subtract(new TimeSpan(30, 0, 0, 0));
            String daysAgo = temp.ToString("yyyy-MM-dd HH:mm:ss");
			OdbcDataAdapter dataAdapter = new OdbcDataAdapter(
					"select emp_seq , obj_num FROM micros.emp_def where ( termination_date > '" + daysAgo + "' OR termination_date is NULL )" , newConnection );
				dataAdapter.Fill( dataSet , "micros.emp_def" );
				dataAdapter.Dispose();
				DataRowCollection rows = dataSet.Tables[0].Rows;
                foreach (DataRow row in rows)
                {
                    try
                    {
                        int empSeq = data.GetInt(row, "emp_seq");
                        int objNum = data.GetInt(row, "obj_num");
                        empIDS.Add(objNum+ "", empSeq + "");
                    }
                    catch (Exception ex) {
                        logger.Error("error in emp_seq to obj_num mapping");
                        logger.Error(ex.ToString());
                    }
                }
        }

        private void replaceEmpIDs(ShiftList shifts)
        {
            foreach (Shift shift in shifts)
            {
                try
                {
                    int empSeq = Convert.ToInt32((String)empIDS[shift.PosEmpId+""]);
                    if (empSeq > -1)
                    {
                        shift.PosEmpId = empSeq;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("emp mapping doesnt exist:  " + shift.PosEmpId);
                    logger.Error(ex.ToString());

                }
            }
        }

		public override void Execute()
		{
			// load shifts used for comparison
			ShiftList posShifts = GetMicrosSchedules();
			ShiftList hsShifts = this.Shifts;

			// create lists for insert,delete,and update
			ShiftList addShifts = new ShiftList();
			ShiftList deleteShifts = new ShiftList();
			ShiftList updateShifts = new ShiftList();
            mapEmpIDs = Details.Preferences.PrefExists(Preference.REPLACE_DL_ID);
            if (mapEmpIDs)
            {
                getEmpMap();
                replaceEmpIDs(hsShifts);
            }
			foreach (Shift shift in hsShifts)
			{

				if (posShifts.Contains(shift))
				{
					logger.Debug("found match!");
					Shift posShift = posShifts.Get(shift);
					if (shift.ClockIn.Hour != posShift.ClockIn.Hour ||
						shift.ClockIn.Minute != posShift.ClockIn.Minute ||
						shift.ClockOut.Hour != posShift.ClockOut.Hour ||
						shift.ClockOut.Minute != posShift.ClockOut.Minute)
					{
						logger.Debug("adding to update list");
						shift.PosId = posShift.PosId;
						if (shift.PosEmpId > 0) updateShifts.Add(shift);
					}
				}
				else
				{
					addShifts.Add(shift);
				}
			}

			foreach (Shift shift in posShifts)
			{
				if (!hsShifts.Contains(shift))
				{
					if (shift.PosEmpId > 0) deleteShifts.Add(shift);
				}
			}

			DeleteShiftsFromMicros(deleteShifts);
			UpdateShiftsInMicros(updateShifts);
			AddShiftsToMicros(addShifts);
		}

		private ShiftList GetMicrosSchedules()
		{
			int numDays = 6;
			if (Details.Preferences.PrefExists(1018))
			{
				try
				{
					HsSharedObjects.Client.Preferences.Preference pref
						= this.Details.Preferences.GetPreferenceById(1018);
					if (pref != null)
					{
						int days = Int32.Parse(pref.Val2);
						if (days > 0)
							numDays = days;
					}
				}
				catch (Exception ex)
				{
					logger.Error("error setting numDays from details.  using default of 2");
				}
			}
			ShiftList posShifts = new ShiftList();
			OdbcConnection newConnection = this.cnx.GetOdbc();
			try
			{
				DataSet dataSet = new DataSet();
				OdbcDataAdapter dataAdapter = new OdbcDataAdapter(
					"select tm_clk_sched_seq, emp_seq , job_seq , clk_in_date_tm , clk_out_date_tm from micros.time_clock_sched_def " +
					"where clk_in_date_tm > ? and clk_in_date_tm < ?", newConnection);
				dataAdapter.SelectCommand.Parameters.Add("", DateTime.Today);
				dataAdapter.SelectCommand.Parameters.Add("", DateTime.Today.AddDays(numDays));
				dataAdapter.Fill(dataSet, "micros.v_R_employee_time_card");
				dataAdapter.Dispose();
				DataRowCollection rows = dataSet.Tables[0].Rows;
				foreach (DataRow row in rows)
				{
					try
					{
						Shift shift = new Shift();
						shift.PosId = data.GetInt(row, "tm_clk_sched_seq");
						shift.PosEmpId = data.GetInt(row, "emp_seq");
						shift.PosJobId = data.GetInt(row, "job_seq");
						shift.ClockIn = data.GetDate(row, "clk_in_date_tm");
						shift.ClockOut = data.GetDate(row, "clk_out_date_tm");

						// client shift hack
						if (shift.ClockIn.TimeOfDay >= new TimeSpan(4, 0, 0) && shift.ClockIn.TimeOfDay <= new TimeSpan(15, 30, 0))
						{
							shift.ClientShift = 0;
						}
						else shift.ClientShift = 1;

						posShifts.Add(shift);
					}
					catch (Exception ex)
					{
						logger.Error("Error adding micros schedules in Execute(): " + ex.ToString());
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
				newConnection.Close();
			}
			return posShifts;
		}

		private void AddShiftsToMicros(ShiftList shifts)
		{
			logger.Debug("Adding shifts to Micros");

			using (OdbcConnection newConnection = this.cnx.GetOdbc())
			{
				newConnection.Open();
				Random random = new Random();
				foreach (Shift shift in shifts)
				{
					int seq = -1;
					try
					{
						OdbcCommand insertCmd = new OdbcCommand("INSERT INTO micros.time_clock_sched_def ( emp_seq, " +
							"tm_clk_sched_seq, job_seq, clk_in_date_tm, clk_out_date_tm ) " +
							"VALUES( ?, ?, (select job_seq from micros.job_def where obj_num = ?), ?, ? )", newConnection);
						seq = GetNextSeq(random, newConnection);
						OdbcParameter[] parms = new OdbcParameter[5];
						parms[0] = new OdbcParameter("empSeq", shift.PosEmpId);
						parms[1] = new OdbcParameter("tcSeq", seq);
						parms[2] = new OdbcParameter("jobSeq", shift.PosJobId);
						parms[3] = new OdbcParameter("clkIn", shift.ClockIn);
						parms[4] = new OdbcParameter("clkOut", shift.ClockOut);
						foreach (OdbcParameter parm in parms)
						{
							if (parm != null)
							{
								insertCmd.Parameters.Add(parm);
							}
							else
							{
								insertCmd.Parameters.Add(new OdbcParameter("", ""));
							}
						}
						logger.Debug("INSERT INTO micros.time_clock_sched_def ( emp_seq, " +
							"tm_clk_sched_seq, job_seq, clk_in_date_tm, clk_out_date_tm ) " +
							"VALUES( " + shift.PosEmpId + ", " + seq + ", (select job_seq from micros.job_def where obj_num = " + shift.PosJobId + "), " + shift.ClockIn + ", " + shift.ClockOut + " )");
						int rows = insertCmd.ExecuteNonQuery();
					}
					catch (Exception ex)
					{
						logger.Error("Error inserting shift [" + seq + "] to Micros: " + shift, ex);
					}
				}
			}
		}

		private void DeleteShiftsFromMicros(ShiftList shifts)
		{
			OdbcConnection newConnection = this.cnx.GetOdbc();
			foreach (Shift shift in shifts)
			{
				logger.Debug("Deleting shift[" + shift.PosId.ToString() + "]");
				try
				{
					newConnection.Open();
					OdbcCommand delCmd = new OdbcCommand(
						"DELETE FROM micros.time_clock_sched_def " +
						"WHERE tm_clk_sched_seq = ? " +
						"AND NOT EXISTS(SELECT * FROM micros.time_card_dtl WHERE tm_clk_sched_seq=?)", newConnection);
					OdbcParameter[] parms = new OdbcParameter[2];
					parms[0] = new OdbcParameter("seq", shift.PosId);
					parms[1] = new OdbcParameter("seq", shift.PosId);
					foreach (OdbcParameter parm in parms)
					{
						if (parm != null)
						{
							delCmd.Parameters.Add(parm);
						}
						else
						{
							delCmd.Parameters.Add(new OdbcParameter("", ""));
						}
					}
					int rows = delCmd.ExecuteNonQuery();
				}
				catch (Exception ex)
				{
					logger.Error("Could not delete shift [" + shift.PosId + "] for employee [" + shift.PosEmpId + "]", ex);
				}
				finally
				{
					newConnection.Close();
				}
			}
		}

		private void UpdateShiftsInMicros(ShiftList shifts)
		{
			OdbcConnection newConnection = this.cnx.GetOdbc();
			foreach (Shift shift in shifts)
			{
				try
				{
					newConnection.Open();
					OdbcCommand updateCmd = new OdbcCommand("UPDATE micros.time_clock_sched_def " +
						"SET clk_in_date_tm = ?, clk_out_date_tm = ? WHERE tm_clk_sched_seq = ?", newConnection);
					OdbcParameter[] parms = new OdbcParameter[3];
					parms[0] = new OdbcParameter("clkIn", shift.ClockIn);
					parms[1] = new OdbcParameter("clkOut", shift.ClockOut);
					parms[2] = new OdbcParameter("seq", shift.PosId);
					foreach (OdbcParameter parm in parms)
					{
						if (parm != null)
						{
							updateCmd.Parameters.Add(parm);
						}
						else
						{
							updateCmd.Parameters.Add(new OdbcParameter("", ""));
						}
					}
					int rows = updateCmd.ExecuteNonQuery();
				}
				catch (Exception ex)
				{
					logger.Error(ex.ToString());
				}
				finally
				{
					newConnection.Close();
				}
			}
		}

		private int GetNextSeq(Random random, OdbcConnection connection)
		{
			try
			{
				int nextSeq = random.Next(MAX_SEQ);
				while (SeqIsUsed(nextSeq, connection))
				{
					logger.Debug("Shift seq [" + nextSeq + "] is not unique.  Regenerating.");
					nextSeq = random.Next(MAX_SEQ);
				}
				return nextSeq;
			}
			catch (Exception ex)
			{
				logger.Error("Error generating new Shift ID", ex);
				return -1;
			}
		}

		private bool SeqIsUsed(int seq, OdbcConnection connection)
		{
			try
			{
				OdbcCommand cmd = new OdbcCommand("SELECT tm_clk_sched_seq FROM micros.time_clock_sched_def WHERE tm_clk_sched_seq=" + seq, connection);
				Object result = cmd.ExecuteScalar();
				return result != null;
			}
			catch (Exception ex)
			{
				logger.Error("Error checking new shift ID", ex);
				return false;
			}
		}

		public override void GetWeekDates()
		{
			return;
		}

		public override DateTime CurrentWeek { get { return new DateTime(0); } set { this.CurrentWeek = value; } }
	}
}
