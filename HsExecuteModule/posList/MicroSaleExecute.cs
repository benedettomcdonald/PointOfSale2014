using System;
using System.Collections;
using System.Data.OleDb;
using System.Windows.Forms;
using HsSharedObjects;
using HsSharedObjects.Client.Preferences;
using HsSharedObjects.Main;

namespace HsExecuteModule.posList
{
	/// <summary>
	/// Summary description for MicroSaleExecute.
	/// </summary>
	[Serializable]
	public class MicroSaleExecute : BaseExecute
	{
		private OleDbConnection conn;
		private readonly String TRANSFER_DIR = Application.StartupPath + "\\transfer";
		private readonly String MICROSALE_DIR = Application.StartupPath + "\\MicroSale";
		private SysLog logger = new SysLog(typeof(MicroSaleExecute));


		public override void Execute(bool map)
		{
			logger.Debug("Executing MicroSaleExecute");
			RunCommand(null);
		}

		public override void RunCommand(Command cmd)
		{
			logger.Debug("Running MicroSaleExecute");
			GenerateCSVs();
		}

		private void GenerateCSVs()
		{
			try
			{
				EmployeesAndJobs();
				Timecards();
			}
			catch (Exception ex)
			{
				logger.Error("Error connecting to MicroSale Database", ex);
			}
		}

		private void EmployeesAndJobs()
		{
			try
			{
				logger.Debug("Creating Connection to Employee.mdb");
				conn = new OleDbConnection(ClientDetails.GetConnectionString().Replace("[filename]", "Employee.mdb"));
				conn.Open();

				Employees();
				Jobs();
				EmpJobs();
			}
			catch (Exception ex)
			{
				logger.Error("Error generating CSV files", ex);
			}
			finally
			{
				if(conn!=null)
					conn.Close();
			}
		}

		public void Timecards()
		{
			try
			{
				logger.Log("Generating Timecard.csv");
				MapFile empIdMap = new MapFile(MICROSALE_DIR + "\\EmployeeMap.csv", true);
				MapFile jobIdMap = new MapFile(MICROSALE_DIR + "\\JobMap.csv", true);
				CsvFile timecardCsv = new CsvFile();

				ArrayList headers = new ArrayList();
				headers.Add("TIMECARD_ID");
				headers.Add("EMP_ID");
				headers.Add("JOB_ID");
				headers.Add("BUSINESS_DATE");
				headers.Add("DATE_IN");
				headers.Add("TIME_IN");
				headers.Add("DATE_OUT");
				headers.Add("TIME_OUT");
				headers.Add("PAY_RATE");
				timecardCsv.AddRow(headers);

				ReadTimecardsFromFile("Time Records.mdb", timecardCsv, empIdMap, jobIdMap);

				if (ClientDetails.Preferences.PrefExists(Preference.PREV_WEEK_TIMECARDS))
				{
					try
					{
					    int numPrevWeeks = Convert.ToInt32(ClientDetails.Preferences.GetPreferenceById(Preference.PREV_WEEK_TIMECARDS).Val2);
					    logger.Debug("Reading labor from " + numPrevWeeks + " previous weeks");
                        for(int i=0; i<=numPrevWeeks && i<10; ++i)
                        {
                            logger.Debug("Reading labor from previous week #" + i);
                            DateTime weekStart = getWeekStart(DateTime.Today.AddDays(i * -7));
                            ReadTimecardsFromFile("Time Records Backup" + weekStart.ToString("M-d-yyyy") + ".mdb", timecardCsv, empIdMap, jobIdMap);
                        }
					}
					catch (Exception ex)
					{
						logger.Error("Error reading MicroSale timecards from previous week", ex);
					}
				}

				bool saved = timecardCsv.SaveAs(TRANSFER_DIR + "\\Timecard.csv");
                if (!saved)
                    logger.Error("Error saving Timecard.csv");
                else
                    logger.Log("Found " + timecardCsv.Values.Count + " timecards");
			}
			catch (Exception ex)
			{
				logger.Error("Error reading MicroSale timecard data", ex);
			}
		}

		private DateTime getWeekStart(DateTime time)
		{
			DateTime weekStart = time;
			while (weekStart.DayOfWeek != ClientDetails.DayOfWeekStart)
				weekStart = weekStart.AddDays(-1);
			return weekStart;
		}

		private void ReadTimecardsFromFile(string fileName, CsvFile timecardCsv, MapFile empIdMap, MapFile jobIdMap)
		{
			OleDbDataReader reader = null;
			try
			{
				logger.Debug("Creating Connection to \"" + fileName + "\"");
				conn = new OleDbConnection(ClientDetails.GetConnectionString().Replace("[filename]", fileName));
				conn.Open();
				OleDbCommand cmd = new OleDbCommand(TIMECARD, conn);
				reader = cmd.ExecuteReader();

			    int count = 0;
                while (reader.Read())
                {
                    ReadTimecardRow(reader, empIdMap, jobIdMap, timecardCsv);
                    ++count;
                }
			    logger.Log("Successfully loaded " + count + " timecards from \"" + fileName + "\"");
			}
			catch (Exception ex)
			{
				logger.Error("Error reading MicroSale timecard data from \"" + fileName + "\"", ex);
			}
			finally 
			{
				if(reader!=null)
					reader.Close();
				if (conn != null)
					conn.Close();
			}
		}

		private void ReadTimecardRow(OleDbDataReader reader, MapFile empIdMap, MapFile jobIdMap, CsvFile timecardCsv)
		{
			try
			{
				String empIdKey = reader["EMP_NAME"].ToString().Trim();
				String empId = empIdMap[empIdKey];

				String jobCode = reader["JOB_CODE"].ToString().Trim();
				String jobId = jobIdMap[jobCode];

				DateTime timeIn = DateTime.Parse(reader["DATE_IN"].ToString().Trim() + " " + reader["TIME_IN"].ToString().Trim());

				DateTime timeOut = DateTime.Now;
				String timeOutStr = reader["TIME_OUT"].ToString().Trim();
				if(!timeOutStr.Equals("") && !timeOutStr.Equals("0")) 
					timeOut = DateTime.Parse(reader["DATE_OUT"].ToString().Trim() + " " + timeOutStr);
				else
					logger.Debug("No out time, using NOW");

				if (timeOutStr.Equals(""))
					logger.Debug("Out time: " + timeOut.ToString("HHmm"));
                
				Double payRate = Double.Parse(reader["PAY_RATE"].ToString().Trim());

				ArrayList row = new ArrayList();

				row.Add("");
				row.Add(empId);
				row.Add(jobId);
				row.Add(timeIn.ToString("MM/dd/yyyy"));
				row.Add(timeIn.ToString("MM/dd/yyyy"));
				row.Add(timeIn.ToString("HHmm"));
				row.Add(timeOut.ToString("MM/dd/yyyy"));
				row.Add(timeOut.ToString("HHmm"));
				row.Add(payRate);

				timecardCsv.AddRow(row);
			}
			catch (Exception ex)
			{
				object[] values = new object[reader.FieldCount];
				reader.GetValues(values);
				String row = "";
				foreach (object o in values)
					row += o + "|";
				logger.Error("Error writing MicroSale Timecard row: " + row, ex);
			}
		}

		private void Employees()
		{
			MapFile empIdMap = new MapFile(MICROSALE_DIR + "\\EmployeeMap.csv", true);
			CsvFile employeeCsv = new CsvFile();
			OleDbDataReader reader = null;
			try
			{
				OleDbCommand cmd = new OleDbCommand(EMPLOYEE, conn);
				reader = cmd.ExecuteReader();

				ArrayList headers = new ArrayList();
				headers.Add("EmpId");
				headers.Add("FirstName");
				headers.Add("LastName");
				headers.Add("BirthDate");
				headers.Add("Address1");
				headers.Add("Address2");
				headers.Add("City");
				headers.Add("State");
				headers.Add("ZipCode");
				headers.Add("Phone");
				headers.Add("HireDate");
				headers.Add("TermDate");
				headers.Add("InactiveFrom");
				headers.Add("InactiveTo");
				headers.Add("Status");
				employeeCsv.AddRow(headers);

				while(reader.Read())
					ReadEmpRow(reader, empIdMap, employeeCsv);

				bool mapSaved = empIdMap.Save();
				if (!mapSaved)
					logger.Error("Error saving MicroSale Employee Map");

				bool saved = employeeCsv.SaveAs(TRANSFER_DIR + "\\Employee.csv");
				if(!saved)
					logger.Error("Error saving Employee.csv");
				else
				    logger.Log("Found " + employeeCsv.Values.Count + " employees");
			}
			catch (Exception ex)
			{
				logger.Error("Error writing MicroSale employee data", ex);
			}
			finally
			{
				if(reader!=null)
					reader.Close();
			}
		}

		private void ReadEmpRow(OleDbDataReader reader, MapFile empIdMap, CsvFile employeeCsv)
		{
			try
			{
				ArrayList row = new ArrayList();
				String firstName = reader["FirstName"].ToString().Trim();
				String lastName = reader["LastName"].ToString().Trim();
				String key = firstName + " " + lastName;
				if (!empIdMap.ContainsKey(key))
				{
					logger.Debug("Found new emp: " + key);
					empIdMap.AddNext(key);
				}
				String empId = empIdMap[key];
				int empStatus = reader["Terminated"].ToString().Equals("0") ? 1 : -1;

				row.Add(empId);
				row.Add(firstName);
				row.Add(lastName);
				row.Add(reader["DateOfBirth"]);
				row.Add(reader["Address"]);
				row.Add("");
				row.Add(reader["City"]);
				row.Add(reader["State"]);
				row.Add(reader["Zip"]);
				row.Add(reader["HomePhone"]);
				row.Add(reader["HireDate"]);
				row.Add("");
				row.Add("");
				row.Add("");
				row.Add(empStatus);

				employeeCsv.AddRow(row);
			}
			catch (Exception ex)
			{
				object[] values = new object[reader.FieldCount];
				reader.GetValues(values);
				String row = "";
				foreach (object o in values)
					row += o + "|";
				logger.Error("Error writing MicroSale Employee row: " + row, ex);
			}
		}

		private void Jobs()
		{
			MapFile jobMap = new MapFile(MICROSALE_DIR + "\\JobMap.csv", true);
			CsvFile jobCsv = new CsvFile();
			OleDbDataReader reader = null;
			try
			{
				OleDbCommand cmd = new OleDbCommand(JOB, conn);
				reader = cmd.ExecuteReader();

				ArrayList headers = new ArrayList();
				headers.Add("JobId");
				headers.Add("JobName");
				jobCsv.AddRow(headers);

				while (reader.Read())
					ReadJobRow(reader, jobMap, jobCsv);

				bool mapSaved = jobMap.Save();
				if(!mapSaved)
					logger.Error("Error saving MicroSale Job Map");

				bool csvSaved = jobCsv.SaveAs(TRANSFER_DIR + "\\Job.csv");
                if (!csvSaved)
                    logger.Error("Error saving Job.csv");
                else
                    logger.Log("Found " + jobCsv.Values.Count + " jobs");
			}
			catch (Exception ex)
			{
				logger.Error("Error writing MicroSale job data", ex);
			}
			finally
			{
				if (reader != null)
					reader.Close();
			}
		}

		private void ReadJobRow(OleDbDataReader reader, MapFile jobMap, CsvFile jobCsv)
		{
			try
			{
				String jobCode = reader["JobCode"].ToString().Trim();
				if (!jobMap.ContainsKey(jobCode))
					jobMap.AddNext(jobCode);

				String jobId = jobMap[jobCode];
				String jobName = jobCode.Substring(9);  // Todo: This is hard-coded to Main Event's usage.  Will need to be made a pref for other M$ clients.

				ArrayList row = new ArrayList();
				row.Add(jobId);
				row.Add(jobName);

				jobCsv.AddRow(row);
			}
			catch (Exception ex)
			{
				Object[] values = new object[reader.FieldCount];
				reader.GetValues(values);
				String row = "";
				foreach (object o in values)
					row += o + "|";
				logger.Error("Error reading MicroSale Job row: " + row, ex);
			}
		}

		private void EmpJobs()
		{
			CsvFile empJobCsv = new CsvFile();
			MapFile empIdMap = new MapFile(MICROSALE_DIR + "\\EmployeeMap.csv", true);
			MapFile jobMap = new MapFile(MICROSALE_DIR + "\\JobMap.csv", true);
			OleDbDataReader reader = null;
			try
			{
				OleDbCommand cmd = new OleDbCommand(EMPJOB, conn);
				reader = cmd.ExecuteReader();

				ArrayList headers = new ArrayList();
				headers.Add("EmpId");
				headers.Add("JobId");
				headers.Add("PayRate");
				headers.Add("OvtRate");
				headers.Add("OvtFactor");
				headers.Add("PrimaryJob");
				empJobCsv.AddRow(headers);

				while (reader.Read())
					ReadEmpJobRow(reader, empIdMap, jobMap, empJobCsv);

				bool csvSaved = empJobCsv.SaveAs(TRANSFER_DIR + "\\EmpJob.csv");
				if (!csvSaved)
					logger.Error("Error saving EmpJob.csv");
                else
				    logger.Log("Found " + empJobCsv.Values.Count + " employee jobs");
			}
			catch (Exception ex)
			{
				logger.Error("Error reading MicroSale employee job data", ex);
			}
			finally
			{
				if (reader != null)
					reader.Close();
			}
		}

		private void ReadEmpJobRow(OleDbDataReader reader, MapFile empIdMap, MapFile jobMap, CsvFile empJobCsv)
		{
			try
			{
				String firstName = reader["FirstName"].ToString().Trim();
				String lastName = reader["LastName"].ToString().Trim();
				String empIdKey = firstName + " " + lastName;
				String empId = empIdMap[empIdKey];

				String jobCode = reader["JobCode"].ToString().Trim();
				String jobId = jobMap[jobCode];

				if (empId!=null && !empId.Equals("") && jobId!=null && !jobId.Equals(""))
				{
					ArrayList row = new ArrayList();
					row.Add(empId);
					row.Add(jobId);
					row.Add(reader["Rate"]);
					row.Add("");
					row.Add("");

					String empJobNo = reader["Pos"].ToString();
					if (empJobNo.Equals("1"))
						row.Add(true);
					else
						row.Add(false);

					empJobCsv.AddRow(row);
				}
			}
			catch (Exception ex)
			{
				object[] values = new object[reader.FieldCount];
				reader.GetValues(values);
				String row = "";
				foreach (object o in values)
					row += o + "|";
				logger.Error("Error writing MicroSale EmpJob row: " + row, ex);
			}
		}


		//Queries
		private static readonly String EMPLOYEE =
			"SELECT prd.`First Name` AS FirstName, prd.`Last Name` AS LastName, prd.`Hire Date` AS HireDate, prd.`Date of Birth` AS DateOfBirth, " +
			"gd.Address AS Address, `gd`.`City` AS City, gd.State AS State, gd.Zip as Zip, gd.`Home Phone` AS HomePhone, gd.Terminated as Terminated " +
			"FROM `Pay Roll Data` AS prd, `General Data` AS gd " +
			"WHERE prd.`First Name`=gd.`First Name` AND prd.`Last Name`=gd.`Last Name`";

		private static readonly String JOB =
			"SELECT DISTINCT `Job Code` AS `JobCode` FROM `Job Codes` WHERE `Job Code` <> ''";

		private static readonly String EMPJOB =
			"SELECT `First Name` AS FirstName, `Last Name` AS LastName, `Job Code` AS JobCode, Rate, Pos " +
            "FROM `Job Codes` WHERE `First Name` <> '' AND `Last Name` <> '' ORDER BY `Last Name`, Pos";

        private static readonly String TIMECARD =
            "SELECT TOP 10000 `Employee Name` AS EMP_NAME, `Department Name` AS JOB_CODE, `Shift Date` AS DATE_IN, `Time In` AS TIME_IN, `Time Out` AS TIME_OUT, " +
            "`Punch Out Date` AS DATE_OUT, `Rate of Pay` AS PAY_RATE " +
            "FROM `Employee Records`";

//        Previously, we queried only a given date range from the timecard file.  
//        But since each timecard file only contains a week, we may as well get all the data from the file.
//        
//		private static readonly String TIMECARD =
//			"SELECT `Employee Name` AS EMP_NAME, `Department Name` AS JOB_CODE, `Shift Date` AS DATE_IN, `Time In` AS TIME_IN, `Time Out` AS TIME_OUT, " +
//			"`Punch Out Date` AS DATE_OUT, `Rate of Pay` AS PAY_RATE " +
//			"FROM `Employee Records` " +
//			"WHERE cdate(`Shift Date`) BETWEEN '[Start Date]' AND '[End Date]'";

        //      These (Proforma2) queries aren't used for now.  May be at some point in the future.
        //
		//        private static readonly String RVC =
		//            "SELECT * " +
		//            "FROM Squirrel.dbo.K_Department";
		//
		//        private static readonly String SALESCAT =
		//            "SELECT * " +
		//            "FROM Squirrel.dbo.K_Category";
		//
		//	    private static readonly String SALESITEM =
		//	        "SELECT ci.ItemID, ci.CheckID, ci.EmpID, ci.MenuID, ci.SeatNo, ci.DeptNo, ci.SaleTime, ci.Quantity, " +
		//	        "ci.GrossPrice, ci.Journal, ci.List, ci.Status AS CheckItemStatus, ci.OriginalPrice, " +
		//	        "ch.Active, ch.TransactionDate, ch.IsCurrent, ch.Status AS CheckHeaderStatus, m.CatID " +
		//	        "FROM Squirrel.dbo.X_CheckItem ci, Squirrel.dbo.X_CheckHeader ch, Squirrel.dbo.K_Menu m " +
		//	        "WHERE ci.CheckID=ch.CheckID AND ci.MenuID=m.MenuID AND SaleTime>'[SaleTime]'";
		//
		//	    private static readonly String SQL_DATE_FORMAT = "yyyy-MM-dd";
	}
}
