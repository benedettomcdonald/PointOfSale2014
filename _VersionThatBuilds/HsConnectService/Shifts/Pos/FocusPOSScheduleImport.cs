using System;
using System.Text;
using HsSharedObjects.Main;
using HsConnect.Data;
using System.Collections;
using System.IO;


namespace HsConnect.Shifts.Pos
{
    /**
     * Inserts schedule data into EverServ POS
     */
    class FocusPOSScheduleImport : ScheduleImportImpl
    {
        //class members
        private SysLog logger;
        private HsData data = new HsData();
        private ArrayList settingNames = new ArrayList();

        //directories
        //this.Dsn: C:
        private string dropoffFolder = @"\Focus\";
        private string workingDir;
        private string workingPath;
        private string dropoffDir;
        private string dropoffPath;

        public FocusPOSScheduleImport()
        {
            this.logger = new SysLog(this.GetType());
        }

        /*
         * Execute method is entry point for FocusPOS schedule import
         * 1) build schedule.txt CSV file
         * 2) place file in C:/Focus/ directory
         * 3) ...
         * 4) profit!
         */
        public override void Execute()
        {
            logger.Debug("Begin FocusPOS Schedule Import for client id = " + this.Details.ClientId);

            configureFilePaths();

            bool builtSchedule = buildScheduleFile();

            if (!builtSchedule)
            {
                logger.Error("ERROR building FocusPOS schedule import file. Schedule Import bailing out now.");
                return;
            }

            bool copiedFile = copyScheduleFile();

            if (!copiedFile)
            {
                logger.Error("ERROR copying FocusPOS schedule import file to dropoff directory. Schedule Import bailing out now.");
                return;
            }

            logger.Debug("Successfully dropped schedule.txt in the dropoff directory. FocusPOS Schedule import process is complete.");
        }

        /*
         * Configures file paths for building and dropping off the schedule.txt file
         * Path root is configured in integrations settings as Dsn
         */
        private void configureFilePaths()
        {
            workingDir = this.Details.Dsn + @"\hstemp\";
            workingPath = workingDir + "schedule.txt";
            dropoffDir = this.Details.Dsn + dropoffFolder;
            dropoffPath = dropoffDir + "schedule.txt";

            logger.Debug(":::FocusPOS schedule import has configured file paths:::");
            logger.Debug("working directory: " + workingDir);
            logger.Debug("working filepath: " + workingPath);
            logger.Debug("dropoff directory: " + dropoffDir);
            logger.Debug("dropoff filepath: " + dropoffPath);
        }

        /*
         * builds schedule.txt in hstemp directory
         */
        private bool buildScheduleFile()
        {
            logger.Debug("Begin FocusPOS buildScheduleFile()");
            TextWriter writer = null;
            try
            {
                //check for existence of our copy of last run's schedule.txt file, and delete if found
                CheckFileExistence(workingDir, workingPath);

                writer = new StreamWriter(workingPath);
                logger.Debug("Writing " + this.shifts.Count + " schedule records to " + workingPath);
                bool wroteFirstRecord = false;
                int sequenceId = 0;
                string sep = ",";

                foreach (Shift shift in this.shifts)
                {
                    if (wroteFirstRecord)
                    {
                        writer.WriteLine();
                    }
                    else
                    {
                        wroteFirstRecord = true;
                    }

                    string record = sequenceId
                        + sep
                        + shift.PosEmpId
                        + sep
                        + shift.ClockIn.ToString("HH:mm MM/dd/yyyy")
                        + sep
                        + shift.ClockOut.ToString("HH:mm MM/dd/yyyy")
                        + sep
                        + shift.PosJobId;

                    writer.Write(record);
                    sequenceId++;
                }
            }
            catch (Exception e)
            {
                logger.Error("ERROR in FocusPOS buildScheduleFile(): " + e.ToString());
                return false;
            }
            finally
            {
                writer.Flush();
                writer.Close();
            }
            return true;
        }

        /*
         * Creates dir if not exists
         * Deletes file if exists
         */
        private void CheckFileExistence(string dir, string file)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (File.Exists(file))
                File.Delete(file);
        }

        /*
		 * Util method to copy the schedule.txt file into its target directory
		 */
        private bool copyScheduleFile()
        {
            HsFile hsFile = new HsFile();
            logger.Debug("this.Cnx.Dsn = " + this.Cnx.Dsn);
            bool copied = hsFile.Copy(workingDir, dropoffDir, "schedule.txt");

            if (copied)
            {
                logger.Debug("Successfully copied schedule.txt to dropoff directory");
            }
            else
            {
                logger.Debug("Error copying schedule.txt to dropoff directory");
            }

            return copied;
        }

        public override DateTime CurrentWeek { get { return new DateTime(0); } set { this.CurrentWeek = value; } }

        public override void GetWeekDates()
        {
            return;
        }
    }
}
