using HsConnect.Data;

using System;
using System.Data;
using System.IO;
using System.Threading;
using Microsoft.Data.Odbc;
using HsSharedObjects.Client.Preferences;

namespace HsConnect.Employees.PosList
{
	public class PosiEmployeeList : EmployeeListImpl
	{
		public PosiEmployeeList(){}

		private HsData data = new HsData();
		private static String drive = Data.PosiControl.Drive;

		public override void DbLoad()
		{
			this.DbLoad( false );
		}

        public override void DbInsert(GoHireEmpInsertList ls) { }

		public override void DbLoad( bool activeOnly )
		{
			try
			{
				logger.Debug( "Running TAW EXPORTALL" );
				PosiControl.Run( PosiControl.TAW_EXPORT );
				logger.Debug( "TAW EXPORTALL completed" );
				DataTableBuilder builder = new DataTableBuilder();
				logger.Debug( "Building Table" );
				//File.Copy(@"L:\SC\EMPFILE.DBF", @"C:\hstmp\EMPFILE.DBF",true);//For OSI : DO NOT USE
				DataTable dt = builder.GetTableFromDBF( drive + @":\ALTDBF", @"C:\", "EMPFILE" );// Non OSI
				//DataTable dt = builder.GetTableFromDBF( @"L:\SC", @"C:\", "EMPFILE" );//For OSI
				logger.Debug( "Table built" );
				DataRowCollection rows = dt.Rows;
				foreach( DataRow row in rows )
				{
					try
					{
						Employee emp = new Employee();
                        if (this.Details.Preferences.PrefExists(Preference.USE_ALTID_NOT_EXTID))
                        {
                            emp.PosId = data.GetInt(row, "ALT_NUM");
                        }
                        else
                        {
                            emp.PosId = data.GetInt(row, "EMP_NUMBER");
                        }
						emp.AltNumber = data.GetInt( row , "ALT_NUM" );
						emp.LastName = data.GetString( row , "LAST_NAME" );
						emp.FirstName = data.GetString( row , "FIRST_NAME" );
						emp.Address1 = data.GetString( row , "ADDRESS_1" );
						emp.MagCardNum = data.GetString( row , "CARD_NUM" );
						emp.PosUserType = data.GetInt( row , "POS_TYPE" );
						emp.PunchOutWithOpen = false;
						emp.Ssn = data.GetString( row , "SOC_SEC" );
						emp.EmployeeType = data.GetString( row , "EMP_TYPE" );
						emp.Phone = new PhoneNumber( data.GetString( row , "PHONE" ) ) ;


                        emp.HiredDate = getHiredDate(row, emp.PosId);
                        

						emp.Status = new EmployeeStatus();
						if( string.Compare( data.GetString( row , "EMP_STATUS" ), "F" , true) == 0 )
						{
							emp.Status.StatusCode = EmployeeStatus.ACTIVE;
						} 
						else emp.Status.StatusCode = EmployeeStatus.TERMINATED;
						if( !activeOnly || (emp.Status.StatusCode == EmployeeStatus.ACTIVE) )
						{
							this.Add( emp );
						}						
					}
					catch( Exception ex )
					{
						logger.Error( "Error adding POSi employee in Load(): " + ex.ToString() );
					}
				}
				logger.Debug( this.GetXmlString() );
			}
			catch( Exception ex )
			{
				logger.Error( "Error in POSi Emp DBLoad: " + ex.ToString() );
				Main.Run.errorList.Add(ex);
			}
		}

        private DateTime getHiredDate(DataRow row, int empPosId)
        {
            /*
            * DATE_HIRED format: yyyyMMdd
            * 
            * attempt normal cast/parse approach first
            * then use custom date parse
            */
            DateTime ret = new DateTime(1, 1, 1);
            bool hireDateSuccess = false;
            try
            {
                ret = data.CastDate(row, "DATE_HIRED");
                hireDateSuccess = true;
                logger.Debug("Posi GetDate(DATE_HIRED) succeeded!");
            }
            catch (Exception ex)
            {
                try
                {
                    ret = data.ParseDate(row, "DATE_HIRED");
                    hireDateSuccess = true;
                    logger.Debug("Posi ParseDate(DATE_HIRED) succeeded!");
                }
                catch (Exception ex2)
                {
                    try
                    {
                        DateTime date = new DateTime(1, 1, 1);
                        if (row["DATE_HIRED"].ToString().Length < 8)
                        {
                            ret = date;
                        }
                        String dateStr = row["DATE_HIRED"].ToString();
                        String yearStr = dateStr.Substring(0, 4);
                        String monthStr = dateStr.Substring(4, 2);
                        String dayStr = dateStr.Substring(6, 2);

                        if (Int32.Parse(yearStr) < 1900)
                        {
                            DateTime today = new DateTime();
                            yearStr = "" + today.Year;
                        }

                        ret = new DateTime(Int32.Parse(yearStr), Int32.Parse(monthStr), Int32.Parse(dayStr));
                        hireDateSuccess = true;
                        logger.Debug("Posi DetermineDate(DATE_HIRED) succeeded!");
                    }
                    catch (Exception ex3)
                    {
                        if (!hireDateSuccess)
                        {
                            logger.Error("ERROR parsing DATE_HIRED for EMP_NUM: " + empPosId + " . DATE_HIRED will be left default and not synced to HS servers", ex3);
                            ret = new DateTime(1971, 1, 1);
                        }
                    }
                    if (!hireDateSuccess)
                    {
                        logger.Error("ERROR parsing DATE_HIRED for EMP_NUM: " + empPosId + " . DATE_HIRED will be left default and not synced to HS servers", ex2);
                        ret = new DateTime(1971, 1, 1);
                    }
                }
                if (!hireDateSuccess)
                {
                    logger.Error("ERROR parsing DATE_HIRED for EMP_NUM: " + empPosId + " . DATE_HIRED will be left default and not synced to HS servers", ex);
                    ret = new DateTime(1971, 1, 1);
                }
            }
            return ret;
        }

		private void CreateFile( String xml )
		{
			StreamWriter writer = null;
			String path = drive + @":\SC\EMPLOYEE.XML";
			//String path = @"L:\SC\EMPLOYEE.XML";
			try
			{
				if( File.Exists( path ) ) File.Delete( path );
				writer = File.CreateText( path );
				writer.Write( xml );
			}	
			catch( Exception ex )
			{
				logger.Error( ex.ToString() );
			}
			finally
			{
				writer.Flush();
				writer.Close();
			}
		}

		public override void DbUpdate(){}
		public override void DbInsert(){}
	}
}
