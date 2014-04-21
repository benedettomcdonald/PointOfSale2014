using HsConnect.Data;

using System;
using System.Data;
using Microsoft.Data.Odbc;

namespace HsConnect.Employees.PosList
{
	public class RestMgrEmployeeList : EmployeeListImpl
	{
		public RestMgrEmployeeList(){}

		private HsData data = new HsData();

		public override void DbLoad()
		{	
			this.DbLoad( false );
		}

        public override void DbInsert(GoHireEmpInsertList ls) { }

		/**
		 * Called by employee sync.  Restaurant Manager stores its employee data in 
		 * EMPLOYEE.DBF.  This method copies both the EMPLOYEE.DBF and EMPLOYEE.dbt
		 * files to the hsTemp directory.  It then grabs all employees who have a
		 * valid name (i.e. not blank).  It then parses the returned rows and
		 * creates employee objects, adding them to the employeeList.
		 */ 
		public override void DbLoad( bool activeOnly )
		{
			HsFile hsFile = new HsFile();
			logger.Debug( "this.Cnx.Dsn = " + this.Cnx.Dsn );
			hsFile.Copy( this.Cnx.Dsn, this.Cnx.Dsn + @"\hstemp", "EMPLOYEE.DBF" );
			hsFile.Copy( this.Cnx.Dsn, this.Cnx.Dsn + @"\hstemp", "EMPLOYEE.dbt" );
			String empCnxStr = this.cnx.ConnectionString + @"\hstemp";
			OdbcConnection newConnection = this.cnx.GetCustomOdbc( empCnxStr );
			try
			{
				logger.Log( "creating DataSet" );
				DataSet dataSet = new DataSet();
				logger.Log( "creating OdbcAdapter" );
				OdbcDataAdapter dataAdapter = new OdbcDataAdapter(
					"SELECT EMP_NO, EMP_NAME, EMP_STREET, EMP_CITY, EMP_STATE, EMP_ZIP, EMP_PHONE, LEAVE_DATE, EMP_ACTIVE FROM EMPLOYEE WHERE EMP_NAME <> ''" , newConnection );
				logger.Log( "filling adapter:  " + newConnection.ConnectionString);
				logger.Log( "empCnxStr:  " + empCnxStr);

				dataAdapter.Fill( dataSet , "EMPLOYEE" );
				logger.Log( "filled" );
				dataAdapter.Dispose();
				DataRowCollection rows = dataSet.Tables[0].Rows;
                //Char[] nameSplitter = {','};
				foreach( DataRow row in rows )
				{
					try
					{
						Employee emp = new Employee();
						emp.PosId = data.GetInt( row , "EMP_NO" );
						String[] names = getNames(data.GetString( row , "EMP_NAME" ));
						String first = names[1];
						String last = names[0];
						emp.FirstName = first;
						emp.LastName = last;
						emp.Address1 = data.GetString( row , "EMP_STREET" );
						emp.City = data.GetString( row , "EMP_CITY" );
						emp.State = data.GetString( row , "EMP_STATE" );
						emp.ZipCode = data.GetString( row , "EMP_ZIP" );
						emp.Phone = new PhoneNumber( data.GetString( row , "EMP_PHONE" ) );
						
						// load status
						emp.Status = new EmployeeStatus();
						DateTime leaveDate = data.GetDate( row , "LEAVE_DATE" );
						String active = data.GetString( row, "EMP_ACTIVE" );
						if(active.ToUpper().Equals("TRUE"))
						{
							emp.Status.StatusCode = EmployeeStatus.ACTIVE;
						}
						else
						{
							emp.Status.StatusCode = EmployeeStatus.TERMINATED;
						}

						this.Add( emp );
					}
					catch( Exception ex )
					{
						logger.Error( "Error adding RestMgr employee in Load(): " + ex.ToString() );
					}
				}
			}
			catch( Exception ex )
			{
				logger.Error( "Error in RestMgr Emp DBLoad: " + ex.ToString() );
			}
			finally
			{
				newConnection.Close();
			}
		}
		public override void DbUpdate(){}
		public override void DbInsert(){}

		private String[] getNames(String nameString)
		{
			Char[] comma = {','};
			Char[] space = {' '};
			String[] commaArray = nameString.Split(comma, 2);
			if(commaArray.Length < 2)
			{
				String[] spaceArray = nameString.Split(space , 2);
				System.Array.Reverse(spaceArray);
				return spaceArray;
			}
			return commaArray;
		}
	}
}
