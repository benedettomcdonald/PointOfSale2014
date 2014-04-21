using HsConnect.Data;
using HsSharedObjects.Client.Preferences;

using System;
using System.Data;
using System.Collections;
using System.IO;
using Microsoft.Data.Odbc;
using System.Text;
using System.Threading;

namespace HsConnect.Jobs.PosList
{
    public class AppleDosJobList : JobListImpl
    {
        public AppleDosJobList() { }

        private HsData data = new HsData();
        private String SQL_INF_PATH = System.Windows.Forms.Application.StartupPath + "\\sql.inf";
        private String IMPORT_PATH = @"C:\network\touchit\DATA2\IMPORT\";

        public override void DbInsert() { }
        public override void DbUpdate() { }

        public override void DbLoad()
        {
            StreamReader text = null;

            try
            {
                File.Copy(SQL_INF_PATH, IMPORT_PATH + "sql.inf", true);
                Thread.Sleep(2000);
                long s = DateTime.Now.Millisecond;

                if (!File.Exists(System.Windows.Forms.Application.StartupPath + "\\PAYDPT28.CSV"))
                {
                    File.Copy(@"C:\network\touchit\DATA2\EXPORT\PAYDPT28.CSV", System.Windows.Forms.Application.StartupPath + "\\PAYDPT28.CSV");
                }

                text = File.OpenText(System.Windows.Forms.Application.StartupPath + "\\PAYDPT28.CSV");

                while (text.Peek() > 1)
                {
                    String inStr = text.ReadLine();
                    String[] strArray = LineToArray(inStr);

                    try
                    {
                        /*
                        edited by MFisher to add the "3 jobs per entry" logic
                        if id = 3, 30 = normal, 31 = trainer, 32 = trainee
                         */
                        int id;
                        String jobName = "";
                        if (Details.Preferences.PrefExists(Preference.A_GOLD_TRAINING_JOBS))
                        {
                            logger.Debug("Create Training Job Codes Preference is ON");
                            StringBuilder idStr = new StringBuilder(strArray[0]);
                            idStr.Append("0");

                            id = Convert.ToInt32(idStr.ToString());
                            jobName = strArray[2];
                            logger.Debug("jobName=" + jobName);

                            Job job = new Job();
                            job.ExtId = id;
                            job.Name = jobName;
                            this.Add(job);
                            logger.Debug(jobName + " added");

                            Job trainer = new Job();
                            trainer.ExtId = id + 1;
                            trainer.Name = jobName + " Trainer";
                            this.Add(trainer);
                            logger.Debug(trainer.Name + " added");

                            Job trainee = new Job();
                            trainee.ExtId = id + 2;
                            trainee.Name = jobName + " Trainee";
                            this.Add(trainee);
                            logger.Debug(trainee.Name + " added");
                        }
                        else
                        {
                            logger.Debug("Create Training Job Codes Preference is OFF");
                            id = Convert.ToInt32(strArray[0]);
                            jobName = strArray[2];
                            logger.Debug("jobName=" + jobName);

                            if (jobName.Length == 1 && (jobName.CompareTo("0") == 0 || jobName.CompareTo("1") == 0))
                            {
                                jobName = strArray[1];
                                logger.Debug("Now jobName=" + jobName);
                            }

                            Job job = new Job();
                            job.ExtId = id;
                            job.Name = jobName;
                            this.Add(job);
                            logger.Debug(jobName + " added");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.ToString());
                    }

                }
            }
            finally
            {
                text.Close();
            }
            if (File.Exists(System.Windows.Forms.Application.StartupPath + "\\PAYDPT28.CSV"))
            {
                File.Delete(System.Windows.Forms.Application.StartupPath + "\\PAYDPT28.CSV");
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
