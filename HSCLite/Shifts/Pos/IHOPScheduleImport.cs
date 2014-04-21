using HSCLite.Shifts;
using HsSharedObjects.Main;
using HSCLite.Data;

using System;
using System.Data;
using Microsoft.Data.Odbc;

namespace HSCLite.Shifts.Pos
{
	public class IHOPScheduleImport : ScheduleImportImpl
	{
		public IHOPScheduleImport()
		{
			this.logger = new SysLog( this.GetType() );
		}

		private SysLog logger;
		private HsData data = new HsData();

		public override void Execute()
		{
			// load shifts used for comparison
			ShiftList posShifts = GetIHOPSchedules();
			ShiftList hsShifts = this.Shifts;

			// create lists for insert,delete,and update
			ShiftList addShifts = new ShiftList();
			ShiftList deleteShifts = new ShiftList();
			ShiftList updateShifts = new ShiftList();

			foreach( Shift shift in hsShifts )
			{
				if( posShifts.Contains( shift ) )
				{
					logger.Debug( "found match!" );
					Shift posShift = posShifts.Get( shift );
					if( shift.ClockIn.Hour != posShift.ClockIn.Hour ||
						shift.ClockIn.Minute != posShift.ClockIn.Minute ||
						shift.ClockOut.Hour != posShift.ClockOut.Hour ||
						shift.ClockOut.Minute != posShift.ClockOut.Minute )
					{
						logger.Debug( "adding to update list" );
						shift.PosId = posShift.PosId;
						if( shift.PosEmpId > 0 ) updateShifts.Add( shift );
					}
				} 
				else
				{
					addShifts.Add( shift );
				}
			}

			foreach( Shift shift in posShifts )
			{
				if( !hsShifts.Contains( shift ) )
				{
					if( shift.PosEmpId > 0 ) deleteShifts.Add( shift );
				}
			}

			AddShiftsToIHOP( addShifts );
			DeleteShiftsFromIHOP( deleteShifts );
			UpdateShiftsInIHOP( updateShifts );
		}

		private ShiftList GetIHOPSchedules()
		{
			ShiftList posShifts = new ShiftList();
			OdbcConnection newConnection = new OdbcConnection( HSCLite.Run.ConnectionString );                 
			try
			{
				DataSet dataSet = new DataSet();
				OdbcDataAdapter dataAdapter = new OdbcDataAdapter(
					"select tm_clk_sched_seq, emp_seq , job.obj_num , clk_in_date_tm , clk_out_date_tm from micros.time_clock_sched_def sched, micros.job_def job " +
					"where sched.job_seq = job.job_seq and clk_in_date_tm > ? and clk_in_date_tm < ?", newConnection );
				dataAdapter.SelectCommand.Parameters.Add( "" , DateTime.Today );
				dataAdapter.SelectCommand.Parameters.Add( "" , DateTime.Today.AddDays( 8 ) );
				dataAdapter.Fill( dataSet , "micros.v_R_employee_time_card" );
				dataAdapter.Dispose();
				DataRowCollection rows = dataSet.Tables[0].Rows;
				foreach( DataRow row in rows )
				{
					try
					{
						Shift shift = new Shift();
						shift.PosId = data.GetInt( row , "tm_clk_sched_seq" );
						shift.PosEmpId = data.GetInt( row , "emp_seq" );
						shift.PosJobId = data.GetInt( row , "obj_num" );
						shift.ClockIn = data.GetDate( row , "clk_in_date_tm" );
						shift.ClockOut = data.GetDate( row , "clk_out_date_tm" );
                  
						logger.Debug( "         [GetIHOPSchedules()] Adding: " + shift.PosEmpId + ", " + shift.ClockIn.ToString() );
                                    
						// client shift hack
						if( shift.ClockIn.TimeOfDay >= new TimeSpan( 4, 0, 0 ) && shift.ClockIn.TimeOfDay <= new TimeSpan( 15, 30, 0 ) )
						{
							shift.ClientShift = 0;
						} 
						else shift.ClientShift = 1;

						posShifts.Add( shift );
					}
					catch( Exception ex )
					{
						logger.Error( "Error adding micros schedules in Execute(): " + ex.ToString() );
					}
				}
			}
			catch( Exception ex )
			{
				logger.Error( ex.ToString() );
			}
			finally
			{
				newConnection.Close();
			}
			return posShifts;
		}

		private void AddShiftsToIHOP( ShiftList shifts )
		{
			logger.Debug( "Adding shifts to IHOP" );
			OdbcConnection newConnection = new OdbcConnection( HSCLite.Run.ConnectionString );
			foreach( Shift shift in shifts )
			{
				try
				{
					newConnection.Open();
					OdbcCommand insertCmd =  new OdbcCommand( "INSERT INTO micros.time_clock_sched_def ( emp_seq, " +
						"tm_clk_sched_seq, job_seq, clk_in_date_tm, clk_out_date_tm ) " +
						"VALUES( ?, ?, (select job_seq from micros.job_def where obj_num = ?), ?, ? )" , newConnection );
					OdbcParameter[] parms = new OdbcParameter[5];
					parms[0]  = new OdbcParameter( "empSeq" , shift.PosEmpId );
					parms[1]  = new OdbcParameter( "tcSeq" , GetNextSeq() );
					parms[2]  = new OdbcParameter( "jobSeq" , shift.PosJobId );
					parms[3]  = new OdbcParameter( "clkIn" , shift.ClockIn );
					parms[4]  = new OdbcParameter( "clkOut" , shift.ClockOut );
					foreach( OdbcParameter parm in parms )
					{
						if( parm != null )
						{
							insertCmd.Parameters.Add( parm );
						} 
						else 
						{
							insertCmd.Parameters.Add( new OdbcParameter( "" , "" ) );
						}
					}
					logger.Debug( "INSERT INTO micros.time_clock_sched_def ( emp_seq, " +
						"tm_clk_sched_seq, job_seq, clk_in_date_tm, clk_out_date_tm ) " +
						"VALUES( "+shift.PosEmpId+", [seq], (select job_seq from micros.job_def where obj_num = "+shift.PosJobId+"), "+shift.ClockIn.ToString()+", "+shift.ClockOut.ToString()+" )" );
					int rows = insertCmd.ExecuteNonQuery();
				}
				catch( Exception ex )
				{
					logger.Error( ex.ToString() );
				}
				finally
				{
					newConnection.Close();
				}
			}
		}

		private void DeleteShiftsFromIHOP( ShiftList shifts )
		{
			OdbcConnection newConnection = new OdbcConnection( HSCLite.Run.ConnectionString );
			foreach( Shift shift in shifts )
			{
				logger.Debug( "Deleting shift[" + shift.PosId.ToString() + "]" );
				try
				{
					newConnection.Open();
					OdbcCommand delCmd =  new OdbcCommand( "DELETE FROM micros.time_clock_sched_def WHERE tm_clk_sched_seq = ?" , newConnection );
					OdbcParameter[] parms = new OdbcParameter[1];
					parms[0]  = new OdbcParameter( "seq" , shift.PosId );
					foreach( OdbcParameter parm in parms )
					{
						if( parm != null )
						{
							delCmd.Parameters.Add( parm );
						} 
						else 
						{
							delCmd.Parameters.Add( new OdbcParameter( "" , "" ) );
						}
					}
					int rows = delCmd.ExecuteNonQuery();
				}
				catch( Exception ex )
				{
					logger.Debug("Could not delete shift [" + shift.PosId + "] for employee +[" + shift.PosEmpId +"]" );
					logger.Error( ex.ToString() );
				}
				finally
				{
					newConnection.Close();
				}
			}
		}

		private void UpdateShiftsInIHOP( ShiftList shifts )
		{
			OdbcConnection newConnection = new OdbcConnection( HSCLite.Run.ConnectionString );
			foreach( Shift shift in shifts )
			{
				try
				{
					newConnection.Open();
					OdbcCommand updateCmd =  new OdbcCommand( "UPDATE micros.time_clock_sched_def " +
						"SET clk_in_date_tm = ?, clk_out_date_tm = ? WHERE tm_clk_sched_seq = ?" , newConnection );
					OdbcParameter[] parms = new OdbcParameter[3];
					parms[0]  = new OdbcParameter( "clkIn" , shift.ClockIn );
					parms[1]  = new OdbcParameter( "clkOut" , shift.ClockOut );
					parms[2]  = new OdbcParameter( "seq" , shift.PosId );
					foreach( OdbcParameter parm in parms )
					{
						if( parm != null )
						{
							updateCmd.Parameters.Add( parm );
						} 
						else 
						{
							updateCmd.Parameters.Add( new OdbcParameter( "" , "" ) );
						}
					}
					int rows = updateCmd.ExecuteNonQuery();
				}
				catch( Exception ex )
				{
					logger.Error( ex.ToString() );
				}
				finally
				{
					newConnection.Close();
				}
			}
		}

		private int GetNextSeq()
		{
			OdbcConnection newConnection = new OdbcConnection( HSCLite.Run.ConnectionString );
			try
			{
				DataSet dataSet = new DataSet();
				OdbcDataAdapter dataAdapter = new OdbcDataAdapter( "SELECT ISNULL(MAX(tm_clk_sched_seq),0) as current_seq FROM micros.time_clock_sched_def", newConnection );
				dataAdapter.Fill( dataSet , "micros.time_clock_sched_def" );
				dataAdapter.Dispose();
				DataRowCollection rows = dataSet.Tables[0].Rows;
				return data.GetInt( rows[0] , "current_seq" ) + 1;
			}
			catch( Exception ex )
			{
				logger.Error( "Error in " +  ex.ToString() );
			}
			finally
			{
				newConnection.Close();
			}
			return -1;
		}

		public override void GetWeekDates()
		{
			return;
		}

		public override DateTime CurrentWeek{ get{ return new DateTime(0); } set{ this.CurrentWeek = value; }}
	}
}