using System;
using System.Collections;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Runtime.Serialization;
using System.Windows.Forms;
using HsFileTransfer;
using HsSharedObjects.Client.Preferences;
using HsSharedObjects.Main;
using HsSharedObjects.Client;

namespace HsExecuteModule.posList
{
    /// <summary>
    /// Summary description for SquirrelExecute.
    /// </summary>
    [Serializable]
    public class SquirrelExecute : BaseExecute
    {
        private SqlConnection conn;
        private readonly String dir = Application.StartupPath + "\\Squirrel Files";
        public static int NEG_JOB_CODE_ADJ = 100000;
        private ArrayList files = new ArrayList();
        private SysLog logger = new SysLog(typeof(SquirrelExecute));

        public override void Execute(bool map)
        {
            if (this.ClientDetails.Preferences.PrefExists(Preference.SQUIRREL_USE_BIG_JOB_ADJ))
            {
                NEG_JOB_CODE_ADJ = 1000000;
            }
            logger.Debug("Executing SquirrelExecute");
            foreach (Command cmd in commands)
            {
                RunCommand(cmd);
            }
            if (commands.Count == 0)
            {
                logger.Debug("WARN: older, unconfigured squirrel site setup without commands. Defaulting to standard sync using normal timecard data.");
                GenerateCsvs(false);
            }
        }

        public override void RunCommand(Command cmd)
        {
            logger.Debug("Running SquirrelExecute, with command: " + cmd.Cmd);
            switch (cmd.Cmd)
            {
                case "standard_timecard":
                    GenerateCsvs(false);
                    break;
                case "time_and_attendance":
                    logger.Error("ERROR: time_and_attendance not yet supported for SquirrelPOS. Defaulting to standard timecard sync");
                    GenerateCsvs(false);
                    break;
                default:
                    GenerateCsvs(false);
                    break;
            }
        }

        private void GenerateCsvs(bool useTimeAndAttendance)
        {
            try
            {
                logger.Debug("Creating Connection to Squirrel DB");
                conn = new SqlConnection(ClientDetails.GetConnectionString());
                conn.Open();

                QueryToCsv(Employee, "Employee");
                QueryToCsv(Job, "Job");
                QueryToCsv(Empjob, "EmpJob");

                if (useTimeAndAttendance)
                {
                    //new T&A query
                    logger.Error("ERROR: time_and_attendance not yet supported for SquirrelPOS. This code should not have executed, please see a dev.");
                }
                else
                {
                    //standard timecard query
                    String transactionDate = DateTime.Today.AddDays(-14).ToShortDateString();
                    QueryToCsv(Timecard.Replace("[TransactionDate]", transactionDate), "Timecard");
                }

                QueryToCsv(RevenueCenters, "RVC");
                QueryToCsv(SalesCats, "SalesCat");

                String startDate = DateTime.Today.AddDays(-7).ToShortDateString();
                String endDate = DateTime.Today.ToShortDateString();
                bool netSales = ClientDetails.Preferences.PrefExists(Preference.NET_SALES);
                if (netSales)
                    QueryToCsv(NetSales.Replace("[StartDate]", startDate).Replace("[EndDate]", endDate), "SalesItem");
                else
                    QueryToCsv(GrossSales.Replace("[StartDate]", startDate).Replace("[EndDate]", endDate), "SalesItem");

                if (TransferHistFilesContaining("sale"))
                {
                    for (int m = 1; m <= 24; m++)  // 24 files containing sales for 2 years over 1 month periods
                    {
                        startDate = DateTime.Today.AddMonths(-m).ToShortDateString();
                        endDate = DateTime.Today.AddMonths(-m + 1).ToShortDateString();

                        string fileName = "SalesHistorical_" + m;
                        if (netSales)
                            QueryToCsv(NetSales.Replace("[StartDate]", startDate).Replace("[EndDate]", endDate), fileName);
                        else
                            QueryToCsv(GrossSales.Replace("[StartDate]", startDate).Replace("[EndDate]", endDate), fileName);
                    }
                }

                startDate = DateTime.Today.AddDays(-14).ToShortDateString();
                endDate = DateTime.Today.ToShortDateString();
                String guestCounts = GuestCounts.Replace("[StartDate]", startDate).Replace("[EndDate]", endDate);
                QueryToCsv(guestCounts, "GuestCounts");

                if (TransferHistFilesContaining("guest"))
                {
                    for (int m = 1; m <= 24; m++)  // 24 files containing guest counts for 2 years over 1 month periods
                    {
                        startDate = DateTime.Today.AddMonths(-m).ToShortDateString();
                        endDate = DateTime.Today.AddMonths(-m + 1).ToShortDateString();

                        string fileName = "GuestCountsHistorical_" + m;
                        QueryToCsv(GuestCounts.Replace("[StartDate]", startDate).Replace("[EndDate]", endDate), fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            finally
            {
                conn.Close();
            }
        }

        private bool TransferHistFilesContaining(string containedString)
        {
            foreach (FileForTransfer file in ClientDetails.Files)
            {
                if (file.FilePath != null)
                    if (file.FilePath.ToLower().IndexOf("hist") >= 0 && file.FilePath.ToLower().IndexOf(containedString) >= 0)
                        return true;
                if (file.FilePaths != null && file.FilePaths.Count > 0)
                {
                    foreach (String filePath in file.FilePaths)
                    {
                        if (filePath.ToLower().IndexOf("hist") >= 0 && filePath.ToLower().IndexOf(containedString) >= 0)
                            return true;
                    }
                }
            }
            return false;
        }

        private void QueryToCsv(String query, String fileName)
        {
            SqlDataReader reader = null;
            TextWriter writer = null;
            try
            {
                logger.Debug("Generating " + fileName + ".csv");
                String file = dir + "\\" + fileName + ".csv";

                CheckFileExistence(file);

                logger.Debug("Executing Query: " + query);

                SqlCommand cmd = new SqlCommand(query, conn);
                reader = cmd.ExecuteReader();

                writer = new StreamWriter(file);

                // Headers
                for (int i = 0; i < reader.FieldCount; ++i)
                    writer.Write(reader.GetName(i) + "|");
                writer.WriteLine();

                // Values
                while (reader.Read())
                {
                    try
                    {
                        for (int i = 0; i < reader.FieldCount; ++i)
                        {
                            String val = reader[i].ToString();
                            if (reader.GetName(i).Equals("JobNo"))  // Hack to avoid negative job codes, adding 100,000 to it's absolute value instead
                            {
                                int jobNo = Convert.ToInt32(val);
                                if (jobNo < 0)
                                {
                                    String oldVal = val;
                                    val = "" + (NEG_JOB_CODE_ADJ - jobNo);
                                    logger.Debug("Converted outgoing job code [" + oldVal + "] to [" + val + "]");
                                }
                            }
                            writer.Write(val + "|");
                        }
                        writer.WriteLine();
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error reading row", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error generating " + fileName + ".csv", ex);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
                if (reader != null)
                    reader.Close();
            }
        }

        private void CheckFileExistence(string file)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (File.Exists(file))
                File.Delete(file);
        }

        //Queries
        private const String Employee =
            "SELECT emp.EmpID, emp.EmpNumber, emp.LastName, emp.FirstName, emp.BirthDate, emp.Address1, emp.Address2, emp.City, emp.Province_State, emp.PostalCode, emp.PhoneNo, emp.HireDate, " +
            "emp.FireDate, emp.Remarks, emp.SubAcct, emp.LeftHanded, emp.Status, emp.CrossRef, badge.Name as NickName " + "FROM Squirrel.dbo.K_Employee emp LEFT OUTER JOIN Squirrel.dbo.K_EmpBadge badge ON emp.EmpId = badge.EmpId";

        private const String Job =
            "SELECT JobNo, Name, JobCatNo, Hourly, PayFrequency, JAutoBreak, RuleSetNo, AnlCombined, AnlDetail, OverTimeAdd, OverTimeFactor, " +
            "SubRaise, SubAdd, SubFactor, Status, BreakNo, CrossRef " + "FROM Squirrel.dbo.K_Job";

        private const String Empjob =
            "SELECT ejr.EmpID, ejr.Generation, ejr.JobNo, ejr.PayRate, ejr.EmpJobNo, ejr.Status, ejr.PunchIn, ejr.PunchOut, j.OverTimeFactor " +
            "FROM Squirrel.dbo.C_EmpJobRelation AS ejr INNER JOIN Squirrel.dbo.K_Job AS j " + "ON ejr.JobNo = j.JobNo";

        private const String Timecard =
            "SELECT XactID, EmpID, PunchIn, PunchOut, InOrigin, OutOrigin, Rate, OverRate, AutoBreak, RateCodeNo, JobNo, DeptNo, NDeptNo, " +
            "OverTimeDataID, Active, TransactionDate, PayPeriodID, RuleSetNo, Status, HourlyJob, HourlyRate, LabourDeptNo, LabourNDeptNo, DispMethod " +
            "FROM Squirrel.dbo.X_PunchIn " + "WHERE Active=1 AND TransactionDate>'[TransactionDate]'" + "ORDER BY PunchIn";

        /*
         * TODO: coordinate with folks at SquirrelPOS to determine how to successfully query for the following values, especially the last two:
         * WorkMinutes, OvertimeMinutes, RegularPay, OvertimePay, SpecialPay, __DeclaredTips__, __BreakMinutes__
         */
        private const String Punch = "SELECT [companyXr] as COMPANY_XR, -1 as CONCEPT_XR, [clientXr] as CLIENT_XR, EmpID, JobNo, TransactionDate, PunchIn, PunchOut, '' as WorkMinutes, " +
            "'' as OvertimeMinutes, '' as RegularPay, '' as OvertimePay, '' as SpecialPay, '' as DeclaredTips, '' as BreakMinutes, Rate as RegWage, OverRate as OvtWage";

        private const String RevenueCenters = "SELECT * " + "FROM Squirrel.dbo.K_Department";

        private const String SalesCats = "SELECT * " + "FROM Squirrel.dbo.K_Category";

        private const String GrossSales =
            "SELECT ci.ItemID, ci.CheckID, ci.EmpID, ci.MenuID, ci.SeatNo, ci.DeptNo, ci.SaleTime, ci.Quantity, " +
            "ci.GrossPrice, ci.Journal, ci.List, ci.Status AS CheckItemStatus, ci.OriginalPrice, " +
            "ch.Active, ch.TransactionDate, ch.IsCurrent, ch.Status AS CheckHeaderStatus, m.CatID " +
            "FROM Squirrel.dbo.X_CheckItem ci, Squirrel.dbo.X_CheckHeader ch, Squirrel.dbo.K_Menu m " +
            "WHERE ci.CheckID=ch.CheckID AND ci.MenuID=m.MenuID AND TransactionDate>='[StartDate]' AND TransactionDate<='[EndDate]'" +
            "ORDER BY SaleTime";

        private const String NetSales =
            "(SELECT     ci.ItemID, ci.CheckID, ci.EmpID, ci.MenuID, ci.SeatNo, ci.DeptNo, ci.SaleTime, ci.Quantity, ci.GrossPrice, ci.Journal, ci.List,  " +
            "					ci.Status AS CheckItemStatus, ci.OriginalPrice, ch.Active, ch.TransactionDate, ch.IsCurrent, ch.Status AS CheckHeaderStatus, m.CatID " +
            "FROM         Squirrel.dbo.X_CheckItem ci INNER JOIN " +
            "					Squirrel.dbo.X_CheckHeader ch ON ci.CheckID = ch.CheckID INNER JOIN " +
            "					Squirrel.dbo.K_Menu m ON ci.MenuID = m.MenuID " +
            "WHERE     TransactionDate >= '[StartDate]' AND TransactionDate <= '[EndDate]' AND isCurrent = 1 " + "UNION " +
            "SELECT     ci.ItemID, ci.CheckID, ci.EmpID, ci.MenuID, ci.SeatNo, ci.DeptNo, ci.SaleTime, ci.Quantity, - cp.PromoAmt AS GrossPrice, ci.Journal, ci.List,  " +
            "				   ci.Status AS CheckItemStatus, ci.OriginalPrice, ch.Active, ch.TransactionDate, ch.IsCurrent, ch.Status AS CheckHeaderStatus, m.CatID " +
            "FROM         Squirrel.dbo.X_CheckItem ci INNER JOIN " +
            "				   Squirrel.dbo.X_CheckHeader ch ON ci.CheckID = ch.CheckID INNER JOIN " +
            "				   Squirrel.dbo.K_Menu m ON ci.MenuID = m.MenuID INNER JOIN " +
            "				   Squirrel.dbo.X_CheckPromo cp ON cp.ItemID = ci.ItemID " +
            "WHERE     TransactionDate >= '[StartDate]' AND TransactionDate <= '[EndDate]' AND isCurrent = 1 " + "UNION " +
            "SELECT     ci.ItemID, ci.CheckID, ci.EmpID, ci.MenuID, ci.SeatNo, ci.DeptNo, ci.SaleTime, ci.Quantity, - cd.DiscountAmt GrossPrice, ci.Journal, ci.List,  " +
            "				   ci.Status AS CheckItemStatus, ci.OriginalPrice, ch.Active, ch.TransactionDate, ch.IsCurrent, ch.Status AS CheckHeaderStatus, m.CatID " +
            "FROM         Squirrel.dbo.X_CheckItem ci INNER JOIN " +
            "				   Squirrel.dbo.X_CheckHeader ch ON ci.CheckID = ch.CheckID INNER JOIN " +
            "				   Squirrel.dbo.K_Menu m ON ci.MenuID = m.MenuID INNER JOIN " +
            "				   Squirrel.dbo.X_Checkdiscount cd ON cd.ItemID = ci.ItemID " +
            "WHERE     TransactionDate >= '[StartDate]' AND TransactionDate <= '[EndDate]' AND isCurrent = 1 " +
            ")ORDER BY SaleTime";

        private const String GuestCounts =
            "DECLARE @StartDate datetime \r\n" +
            "DECLARE @EndDate datetime \r\n" +
            "SET @StartDate = '[StartDate]' \r\n" +
            "SET @EndDate = '[EndDate]' \r\n" +
            "DROP TABLE #T_Covers \r\n" +
            "DROP TABLE #T_Coverphase2 \r\n" +
            "DROP TABLE #T_DeptType \r\n" +
            "SELECT	TxDate=XCH.TransactionDate,DeptNo=XCH.SetDept,XCH.CheckID,XCI.ItemID,XCI.MenuID,XCI.Quantity,Covers=IsNULL(XCH.Covers,0), \r\n" +
            "	 	XCI.SeatNo, XCH.CloseDate,XCH.CheckNo, \r\n" +
            " 		CoverType=(SELECT f.data from Squirrel.dbo.c_flagdata f where f.generation=1 and f.deptno=xci.deptno and f.flagnameid=8), \r\n" +
            "		CoverItem=ISNULL((SELECT 1 from Squirrel.dbo.v_coveritem v where v.MenuID=xci.MenuID and v.DeptNo=xci.DeptNo and v.Generation=1 and \r\n" +
            "			XCI.ItemID not in (SELECT XCP.ItemID FROM Squirrel.dbo.X_CheckPromo XCP, Squirrel.dbo.C_Promo cp Where XCP.ItemID=XCI.ItemID and XCP.PromoID=cp.PromoID and cp.Generation=1 and cp.XCludeCovers=1)),0) \r\n" +
            "INTO		#T_Covers \r\n" +
            "FROM \r\n" +
            "         Squirrel.dbo.X_CheckHeader XCH , Squirrel.dbo.X_CheckItem XCI \r\n" +
            "WHERE \r\n" +
            "         XCH.IsCurrent=1 AND XCH.Active=0 AND \r\n" +
            "         XCH.TransactionDate BETWEEN @StartDate AND @EndDate AND \r\n" +
            "         XCI.CheckID=XCH.CheckID \r\n" +
            "\r\n" +
            "SELECT DISTINCT c.TxDate,C.CloseDate,c.CheckNo,c.CheckID,c.DeptNo, Covers=c.Covers,SeatNo=0,iType=0 \r\n" +
            " into #T_Coverphase2 \r\n" +
            "FROM #T_Covers c \r\n" +
            "WHERE c.CoverType=2 \r\n" +
            "UNION \r\n" +
            "SELECT  c.TxDate,C.CloseDate,c.CheckNo,c.CheckID,c.DeptNo, Covers=SUM(c.CoverItem*Quantity),SeatNo=0 ,iType=1 \r\n" +
            "FROM #T_Covers c  \r\n" +
            "WHERE c.CoverType=1  \r\n" +
            "GROUP BY c.TxDate,C.CloseDate,CheckID,c.CheckNo,DeptNo \r\n" +
            "UNION \r\n" +
            "SELECT DISTINCT  c.TxDate,C.CloseDate,c.CheckNo,c.CheckID,c.DeptNo, Covers=Case when c.CoverItem > 0 Then 1 Else 0 End,SeatNo,iType=1 \r\n" +
            " FROM #T_Covers c  \r\n" +
            "WHERE c.CoverType=0 and C.SeatNo <> 0  \r\n" +
            "GROUP BY c.TxDate,C.CloseDate,CheckID,c.CheckNo,DeptNo,SeatNo,CoverItem \r\n" +
            "UPDATE	#T_Coverphase2 \r\n" +
            "SET	Covers=Covers - ISNULL((SELECT SUM(c1.covers) FROM #T_Coverphase2 C1 WHERE C1.iType <>0  AND C1.CheckID=C.CheckID),0) \r\n" +
            "FROM	#T_Coverphase2 C \r\n" +
            "WHERE C.iType=0 \r\n" +
            "--Bar \r\n" +
            "select DeptNo \r\n" +
            "Into	#T_DeptType \r\n" +
            "from Squirrel.dbo.C_flagdata  \r\n" +
            "where flagnameID=180 and generation=1 and Data=1 \r\n" +
            "-- \r\n" +
            "Select 	TxDate,CloseDate, \r\n" +
            "	CheckNoTable=Case when C.DeptNo in (Select DeptNo From #T_DeptType ) Then (SELECT Name From Squirrel.dbo.X_CheckTable XCT where XCT.CheckID=C.CheckID) \r\n" +
            "			Else Convert(varchar(20),C.CheckNo) End, \r\n" +
            "DeptNo,Covers=SUM(Covers)  \r\n" +
            "FROM #T_Coverphase2 C \r\n" +
            "Group by TxDate,CloseDate,C.CheckID,C.CheckNo,DeptNo \r\n" +
            "Order By TxDate,CloseDate,CheckNoTable";
    }
}
