using System.Globalization;
using HsConnect.Data;

using System;
using System.Data;
using System.Collections;
using System.Threading;
using System.IO;
using Microsoft.Data.Odbc;

namespace HsConnect.Employees.PosList
{
    public class AppleOneEmployeeList : EmployeeListImpl
    {
        public AppleOneEmployeeList() { }

        private HsData data = new HsData();
        private String SQL_INF_PATH = System.Windows.Forms.Application.StartupPath + "\\sql.inf";
        private String IMPORT_PATH = @"C:\network\touchit\DATA2\IMPORT\";

        public override void DbInsert(GoHireEmpInsertList ls) { }

        public override void DbLoad()
        {
            this.DbLoad(false);
        }

        public override void DbLoad(bool activeOnly)
        {
            try
            {
                File.Copy(@"C:\network\touchit\DATA2\Expbin.inf", @"C:\network\touchit\DATA2\EXPORT\Expbin.inf");
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            File.Copy(SQL_INF_PATH, IMPORT_PATH + "sql.inf", true);
            Thread.Sleep(2000);
            StreamReader text = null;
            int x = 30;
            while (x > 0)
            {
                try
                {
                    if (!File.Exists(System.Windows.Forms.Application.StartupPath + "\\EMP28.CSV"))
                    {
                        File.Copy(@"C:\network\touchit\DATA2\EXPORT\EMP28.CSV", System.Windows.Forms.Application.StartupPath + "\\EMP28.CSV");
                    }
                    break;
                }
                catch (IOException ioex)
                {
                    if (x == 1)
                    {
                        logger.Error(ioex.StackTrace);
                    }
                    x--;
                    Thread.Sleep(5000);
                }
            }
            try
            {
                text = File.OpenText(System.Windows.Forms.Application.StartupPath + "\\EMP28.CSV");
                int lineIndex = 0;
                while (text.Peek() > 0)
                {
                    String inStr = text.ReadLine();
                    String[] strArray = LineToArray(inStr);

                    try
                    {
                        try
                        {
                            int testForHeaderRow = Convert.ToInt32(strArray[0]);
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }

                            Employee emp = new Employee();
                            emp.PosId = Convert.ToInt32(strArray[0]);
                            emp.LastName = strArray[3];
                            emp.FirstName = strArray[2];
                            emp.Address1 = strArray[6];
                            emp.Address2 = strArray[7];
                            emp.City = strArray[8];
                            emp.State = strArray[9];
                            emp.ZipCode = strArray[10];

                            int primaryJobCol = strArray.Length == 104 ? 103 : 102; //some files have 104 cols, some 103

                            emp.PrimaryJob = Convert.ToInt32(strArray[primaryJobCol]);
                            emp.Status = new EmployeeStatus();

                        IFormatProvider frmt = new CultureInfo("en-US", true);
                            try
                            {
                                emp.BirthDate = DateTime.ParseExact(strArray[12], "yyyyMMdd", frmt);
                            }
                            catch (Exception ex)
                            {
                                logger.Debug("null birthday:  " + ex.ToString());
                                emp.BirthDate = new DateTime(1971, 1, 1);
                            }

                            if (strArray[14].Length < 2)
                            {
                                emp.Status.StatusCode = EmployeeStatus.ACTIVE;
                            }
                            else if (strArray[14].Length > 3)
                            {
                                try
                                {
                                    String year = strArray[14].Substring(0, 4);
                                    int y = Int32.Parse(year);
                                    if (y > DateTime.Today.Year)
                                    {
                                        emp.Status.StatusCode = EmployeeStatus.ACTIVE;
                                    }
                                    else
                                    {
                                        emp.Status.StatusCode = EmployeeStatus.TERMINATED;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Error("Error parsing term date for employee " + emp.PosId);
                                    logger.Error(ex.ToString());
                                    emp.Status.StatusCode = EmployeeStatus.TERMINATED;
                                }
                            }
                            else emp.Status.StatusCode = EmployeeStatus.TERMINATED;
                            if (!activeOnly || (emp.Status.StatusCode == EmployeeStatus.ACTIVE))
                            {
                            Add(emp);
                            }
                        }
                    catch (Exception ex)
                    {
                        logger.Error(ex.StackTrace);
                    }

                    lineIndex++;
                }
            }
            finally
            {
                text.Close();
            }

            if (File.Exists(System.Windows.Forms.Application.StartupPath + "\\EMP28.CSV"))
            {
                File.Delete(System.Windows.Forms.Application.StartupPath + "\\EMP28.CSV");
            }
        }

        private String[] LineToArray(String line)
        {
            int startIndex = 0;
            int length = 0;
            ArrayList strings = new ArrayList();
            while (startIndex < line.Length)
            {
                if (line.Substring(startIndex, 1).CompareTo("\"") == 0)
                {
                    length = line.IndexOf("\",", startIndex + 1) - (startIndex + 1);
                    length = length < 0 ? (line.Length - 1) - (startIndex + 1) : length;
                    String s = line.Substring(startIndex + 1, length);
                    strings.Add(s);
                    startIndex += (length + 3);
                }
                else if (line.Substring(startIndex, 1).CompareTo(",") == 0)
                {
                    String s = line.Substring(startIndex, length);
                    strings.Add("");
                    startIndex++;
                }
                else
                {
                    length = line.IndexOf(",", startIndex + 1) - startIndex;
                    length = length < 0 ? line.Length - startIndex : length;
                    String s = line.Substring(startIndex, length);
                    strings.Add(s);
                    startIndex += (length + 1);
                }
            }
            String[] strs = new String[strings.Count];
            int cnt = 0;
            foreach (String str in strings)
            {
                strs[cnt] = str.Trim();
                cnt++;
            }
            return strs;
        }

        public override void DbUpdate() { }
        public override void DbInsert() { }
    }
}
