using System;
using System.Text;
using HsConnect.Main;
using HsConnect.Data;
using HsSharedObjects;
using System.IO;

namespace HsConnect.Shifts.Pos
{
    class OnePOSScheduleImport : ScheduleImportImpl
    {
        private SysLog logger;
        private HsData data = new HsData();
        private static String drive = (Main.Run.Mapped ? "S" : "C");

        //indexes of relevant fields in the output csv used for schedule import by OnePOS
        private const int empId = 0;
        private const int empSsn = 1;
        private const int dateIn = 2;
        private const int timeIn = 3;
        private const int dateOut = 4;
        private const int timeOut = 5;
        private const int payCategory = 6;

        public OnePOSScheduleImport()
        {
            this.logger = new SysLog(this.GetType());
        }

        public override void Execute()
        {
            String tempPath = Drive + ":\\hstemp\\";

            String fileName = "EmpSchedule.csv";

            writeScheduleExport(tempPath + fileName);

            String importPath = Drive + ":\\onepos\\imports\\";

            copyFileToImportLocation(tempPath, importPath, fileName);
        }

        private void copyFileToImportLocation(String tempPath, String importPath, String fileName)
        {
            try
            {
                logger.Debug("Copying OnePOS schedule export file, " + tempPath + fileName + ", to export location, " + importPath + fileName);
                HsFile hsFile = new HsFile();
                hsFile.Copy(tempPath, importPath, fileName);
                logger.Debug("Copy complete");
            }
            catch (Exception ex)
            {
                logger.Error(ex.StackTrace);
            }
        }

        private void writeScheduleExport(String file){
            TextWriter writer = null;
            try
            {
                logger.Debug("Generating OnePOS Schedule Export File: " + file);
                writer = new StreamWriter(file);

                foreach (Shift shift in this.shifts)
                {
                    if (shift.IsPosted)
                    {
                        writer.Write(shift.PosEmpId + ",");
                        writer.Write("0,");//since we're using posId for the emp identification, leave SSN as 0
                        writer.Write(shift.ClockIn.ToString("yyyyMMdd") + ",");
                        writer.Write(shift.ClockIn.ToString("HHmm") + ",");
                        writer.Write(shift.ClockOut.ToString("yyyyMMdd") + ",");
                        writer.Write(shift.ClockOut.ToString("HHmm") + ",");
                        //add the last column with a CR/LF
                        if (shift.LocId < 0)
                        {
                            writer.WriteLine("0");
                        }
                        else
                        {
                            writer.WriteLine(shift.LocId);
                        }
                    }
                }
            }
            catch (IOException e)
            {
                logger.Error(e.StackTrace);
            }
            finally
            {
                writer.Flush();
                writer.Close();
            }
            logger.Debug("Successfully generated " + file);
        }

        public override DateTime CurrentWeek { get { return new DateTime(0); } set { this.CurrentWeek = value; } }

        public override void GetWeekDates()
        {
            return;
        }

        public static String Drive
        {
            get
            {
                if (HsCnxData.Details.Dsn == null || HsCnxData.Details.Dsn.Length <= 0)
                    return drive;
                else
                    return HsCnxData.Details.Dsn.Substring(0, 1);
            }
            set { drive = value; }
        }
    }
}
