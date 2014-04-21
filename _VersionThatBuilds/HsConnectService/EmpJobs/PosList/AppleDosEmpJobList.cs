using HsConnect.Data;
using HsConnect.Jobs;
using HsConnect.Services;
using HsSharedObjects.Client.Preferences;

using System;
using System.Data;
using System.IO;
using System.Threading;
using System.Collections;
using Microsoft.Data.Odbc;

namespace HsConnect.EmpJobs.PosList
{
    public class AppleDosEmpJobList : EmpJobImpl
    {
        public AppleDosEmpJobList() { }

        private HsData data = new HsData();
        private JobList hsJobList;
        private String SQL_INF_PATH = System.Windows.Forms.Application.StartupPath + "\\sql.inf";
        private String IMPORT_PATH = @"C:\network\touchit\DATA2\IMPORT\";

        public override void DbInsert() { }
        public override void DbUpdate() { }

        public override void SetHsJobList(JobList hsJobs)
        {
            this.hsJobList = hsJobs;
        }

        public override void DbLoad()
        {
            File.Copy(SQL_INF_PATH, IMPORT_PATH + "sql.inf", true);
            Thread.Sleep(2000);
            StreamReader text = null;
            if (!File.Exists(System.Windows.Forms.Application.StartupPath + "\\EMP28.CSV"))
            {
                File.Copy(@"C:\network\touchit\DATA2\EXPORT\EMP28.CSV", System.Windows.Forms.Application.StartupPath + "\\EMP28.CSV");
            }
            Hashtable empJobs = new Hashtable();
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
                            for (int i = 0; i < 30; i++)
                            {
                                if (Convert.ToInt32(strArray[0]) == 0)
                                    break;
                                EmpJob empJob = new EmpJob();
                                empJob.PosId = Convert.ToInt32(strArray[0]);

                                int jobCol = 58;
                                int rateCol = 28;

                                if (strArray.Length == 103)//some files have 104 cols, some 103
                                {
                                    jobCol = 57;
                                    rateCol = 27;
                                }

                                int rootCode = (int)Convert.ToDouble(strArray[(jobCol + i)]);

                            if (Details.Preferences.PrefExists(Preference.APPLE_DOS_EMP_JOBS_TRAILING_ZERO))
                            {
                                logger.Debug("Using pref 1020: \"Apple DOS employee jobs - remove trailing zero\"");
                                logger.Debug("Jobcode before pref: " + rootCode);
                                rootCode = rootCode / 10;
                                logger.Debug("Jobcode after pref: " + rootCode);
                            }
                            if (rootCode == 0)
                                continue;
                            empJob.JobCode = rootCode;
                            empJob.RegWage = (float)Convert.ToDouble(strArray[(rateCol + i)]);
                            if (hsJobList != null && hsJobList.GetJobByExtId(empJob.JobCode) != null)
                                empJob.HsJobId = hsJobList.GetJobByExtId(empJob.JobCode).HsId;
                            Add(empJob);
                            if (!empJobs.ContainsKey(empJob.PosId + " | " + empJob.JobCode))
                                empJobs.Add(empJob.PosId + " | " + empJob.JobCode, empJob);
                            }
                        }
                    catch (Exception ex)
                    {
                        logger.Error(ex.ToString());
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
    }
}
