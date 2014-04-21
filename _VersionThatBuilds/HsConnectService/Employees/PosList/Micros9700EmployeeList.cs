using HsConnect.Data;

using System;
using System.Data;
using Microsoft.Data.Odbc;

namespace HsConnect.Employees.PosList
{
    public class Micros9700EmployeeList : EmployeeListImpl
    {
        public Micros9700EmployeeList() { }

        private HsData data = new HsData();

        //STATICS
        private static String TERMINATED_STRING = "0000000000";

        /*LAST_NAME, FIRST_NAME, ID, POS_ID, EMPLOYEE_TYPE, NICK_NAME
         * correspond to the columns of the DataTable.  These must
         * be edited if changes are made to the DataTable structure
         * or the SQL query
         **/
        private static String LAST_NAME = "Row0";
        private static String FIRST_NAME = "Row1";
        private static String ID = "Row2";
        private static String POS_ID = "Row3";
        private static String EMPLOYEE_TYPE = "Row4";
        private static String NICK_NAME = "Row5";

        #region unused methods
        public override void DbUpdate()
        {
            Console.WriteLine("This is the 9700 DBUpdate method for EmployeeList");
        }
        public override void DbInsert(GoHireEmpInsertList ls) { }
        public override void DbInsert()
        {
            Console.WriteLine("This is the 9700 DBInsert method for EmployeeList");
        }
        #endregion

        public override void DbLoad()
        {
            this.DbLoad(false);

        }

        /**Used with Employee Sync.  this method queries the 9700 database
         * and creates csv files containing the employee information used
         * to create the Employee object.  These are added to the employee list
         */
        public override void DbLoad(bool activeOnly)
        {
            Micros9700Control.Run(Micros9700Control.EMPLOYEE_SYNC);//runs the batch file
            DataTableBuilder builder = new DataTableBuilder();
            DataTable dt = builder.GetTableFromCSV(Micros9700Control.PATH_NAME, "empl_def_filtered");
            DataRowCollection rows = dt.Rows;
            //finished creating table from CSV, now creating Employee objects
            foreach (DataRow row in rows)
            {
                try
                {
                    Employee emp = new Employee();
                    emp.PosId = data.GetInt(row, POS_ID);
                    emp.LastName = data.GetString(row, LAST_NAME);
                    emp.FirstName = data.GetString(row, FIRST_NAME);
                    emp.EmployeeType = data.GetString(row, EMPLOYEE_TYPE);
                    logger.Debug("Employee Type for emp # " + emp.PosId + " = " + emp.EmployeeType);
                    emp.NickName = data.GetString(row, NICK_NAME);
                    emp.Status = new EmployeeStatus();
                    //use the ID string to check for status.  
                    //A value of "0000000000" means terminated
                    if (data.GetString(row, ID).Equals(TERMINATED_STRING))
                    {
                        emp.Status.StatusCode = EmployeeStatus.TERMINATED;
                    }
                    else if (this.Details.Preferences.PrefExists(HsSharedObjects.Client.Preferences.Preference.MICROS9700_TERM_CODE))
                    {
                        /*
                        * Ensure we mark them active first, 
                        * if Preference.MICROS9700_TERM_CODE is enabled
                        */
                        emp.Status.StatusCode = EmployeeStatus.ACTIVE;

                        logger.Debug("PREF 1015 Exists.  emp type = " + emp.EmployeeType.Trim());
                        HsSharedObjects.Client.Preferences.Preference pref
                            = this.Details.Preferences.GetPreferenceById(HsSharedObjects.Client.Preferences.Preference.MICROS9700_TERM_CODE);
                        String vals = pref.Val2;
                        char[] splitter = { ',' };
                        String[] splitVals = vals.Split(splitter);
                        if (splitVals.Length > 0)
                        {
                            foreach (String val in splitVals)
                            {
                                if (emp.EmployeeType.Trim().Equals(val.Trim()))
                                {
                                    logger.Debug("Employee " + emp.PosId + " should be marked as TERMINATED.  TERM code was " + val);
                                    emp.Status.StatusCode = EmployeeStatus.TERMINATED;
                                }
                            }
                        }
                    }
                    else
                    {
                        emp.Status.StatusCode = EmployeeStatus.ACTIVE;
                    }
                    this.Add(emp);
                }
                catch (Exception ex)
                {
                    logger.Error("Error adding 9700 employee in Load(): " + ex.ToString());
                }
            }
            try
            {
                logger.Debug(this.GetXmlString());
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }//end DbLoad
    }

}
