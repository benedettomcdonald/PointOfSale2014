using HsConnect.Services;
using HsConnect.Services.Wss;
using HsConnect.Xml;

using System;
using System.Xml;
using System.Data;
using Microsoft.Data.Odbc;

namespace HsConnect.Employees
{
	/// <summary>
	/// Summary description for HsEmployeeList.
	/// </summary>
	public class HsEmployeeList : EmployeeListImpl
	{
		public HsEmployeeList( int clientId )
		{
			this.clientId = clientId;
		}

		private int clientId = -1;

        public override void DbInsert(GoHireEmpInsertList ls) { }

		public override void DbUpdate()
		{
			ClientEmployeesWss empService = new ClientEmployeesWss();
			logger.Debug( this.GetXmlString() );
			Console.WriteLine( "Updating " + this.Count );
			int rows = empService.syncEmployeeUpdates( this.clientId , this.GetXmlString() );
		}

		public override void DbInsert()
		{
			Console.WriteLine( "Inserting " + this.Count );
			ClientEmployeesWss empService = new ClientEmployeesWss();
			int rows = empService.syncEmployeeInserts( this.clientId , this.GetXmlString() );
		}

		public override void DbLoad()
		{
			this.DbLoad( false );
		}

		public override void DbLoad(  bool activeOnly  )
		{
			this.DbLoad(activeOnly, true );
		}

		public void DbLoad(  bool activeOnly, bool getAll )
		{
			this.DbLoad(activeOnly, getAll, "");
		}
		public void DbLoad(  bool activeOnly, bool getAll, string xml )
		{
			ClientEmployeesWss employeesService = new ClientEmployeesWss();
			String xmlString;
			if(getAll)
				xmlString = employeesService.getClientEmployees( clientId );
			else
				xmlString = employeesService.getAdjustEmployees( clientId , xml );
			HsXmlReader reader = new HsXmlReader();
			reader.LoadXml( xmlString );
			logger.Debug( xmlString );
			foreach( XmlNode user in reader.SelectNodes( "/hsconnect-client-employees/user" ) )
			{
				Employee emp = new Employee();
				emp.HsId = reader.GetInt( user , "id" , HsXmlReader.ATTRIBUTE );
				emp.PosId = reader.GetInt( user , "pos-id" , HsXmlReader.ATTRIBUTE );
				if( user.Attributes[ "alt-num" ] != null )
				{
					emp.AltNumber = reader.GetInt( user , "alt-num" , HsXmlReader.ATTRIBUTE );
				}
				emp.FirstName = reader.GetString( user , "first-name" , HsXmlReader.NODE );
				emp.LastName = reader.GetString( user , "last-name" , HsXmlReader.NODE );
				emp.NickName = reader.GetString( user , "nick-name" , HsXmlReader.NODE );
				emp.HsUserName = reader.GetString( user , "username" , HsXmlReader.NODE );
				emp.Email = reader.GetString( user , "email" , HsXmlReader.NODE );
				emp.Address1 = reader.GetString( user , "address/street-address-1" , HsXmlReader.NODE );
				emp.Address1 = reader.GetString( user , "address/street-address-1" , HsXmlReader.NODE );
				emp.Address2 = reader.GetString( user , "address/street-address-2" , HsXmlReader.NODE );
				emp.City = reader.GetString( user , "address/city" , HsXmlReader.NODE );
				emp.State = reader.GetString( user , "address/state" , HsXmlReader.NODE );
				emp.ZipCode = reader.GetString( user , "address/zip-code" , HsXmlReader.NODE );
				emp.Phone = new PhoneNumber();
				emp.Phone.Area = reader.GetString( user , "contact-number/area-code" , HsXmlReader.NODE );
				emp.Phone.Prefix = reader.GetString( user , "contact-number/prefix" , HsXmlReader.NODE );
				emp.Phone.Number = reader.GetString( user , "contact-number/number" , HsXmlReader.NODE );
				XmlNode smsNode = user.SelectSingleNode( "sms-messaging" );
				emp.Mobile = reader.GetString( smsNode , "number" , HsXmlReader.ATTRIBUTE );
				emp.Status = new EmployeeStatus();

				// add Emp status info
				if( reader.GetInt( user , "user-inactive/use-date-range" , HsXmlReader.NODE ) == 1 )
				{
					emp.Status.InactiveFrom = reader.GetDate( user , "user-inactive/startDate" , HsXmlReader.NODE );
					emp.Status.InactiveTo = reader.GetDate( user , "user-inactive/endDate" , HsXmlReader.NODE );
				}

				emp.Status.StatusCode = reader.GetInt( user , "user-status/status-id" , HsXmlReader.NODE );
				
				// add birth date
				foreach( XmlNode dateNode in user.SelectNodes( "date" ) )
				{
					if ( dateNode.InnerText.Equals( "birth-date" ) )
					{
						int day = reader.GetInt( dateNode , "day" , HsXmlReader.ATTRIBUTE );
						int month = 1 + reader.GetInt( dateNode , "month" , HsXmlReader.ATTRIBUTE );
						int year = reader.GetInt( dateNode , "year" , HsXmlReader.ATTRIBUTE );
						emp.BirthDate = new DateTime( year , month , day );
					}
				}
				emp.UpdateStatus = reader.GetInt( user.SelectSingleNode( "update-status" ) , "id" , HsXmlReader.ATTRIBUTE );
				this.Add( emp );
			}
		}

		public void AddressUpdate(EmployeeList posList)
		{
			Employee.populateStateLists();
			logger.Debug("debug : 1");
			OdbcConnection newConnection = this.cnx.GetOdbc();
			foreach( Employee emp in this.empList )
			{
					
				try
				{
					Employee posEmp = posList.GetEmployeeByExtId(emp.PosId);
					bool posNotNull = posEmp != null && emp.PosId != -1;
					bool allNull = emp.Address1 == null && emp.Address2 == null && emp.City == null && emp.State == null && emp.ZipCode == null;
					logger.Debug("debug : 2a - " + emp.PosId + " : " + posNotNull);
					if(!posNotNull || allNull)
					{
						logger.Debug("continuing");
						continue;
					}
					bool add1 = (emp.Address1 != null && posEmp.Address1 == null ) || (emp.Address1 != null && emp.Address1.CompareTo(posEmp.Address1) != 0);
					logger.Debug("	debug : 2a - " + add1 + " : " + add1);
					bool add2 = (emp.Address2 != null && posEmp.Address2 == null ) || (emp.Address2 != null && emp.Address2.CompareTo(posEmp.Address2) != 0);
					logger.Debug("	debug : 2a - " + add2 + " : " + add2);
					bool city = (emp.City != null && posEmp.City == null ) || (emp.City != null && emp.City.CompareTo(posEmp.City) != 0);
					logger.Debug("	debug : 2a - " + city + " : " + city);
					bool state = (emp.State != null && emp.State.Length > 0) &&((posEmp.State == null ) || (Employee.getStateId(emp.State)!= Employee.getStateId(posEmp.State)));
					logger.Debug("	debug : 2a - " + state + " : " + state + " , " + emp.State + ":"+ Employee.getStateId(emp.State) + " , " +posEmp.State+":"+ Employee.getStateId(posEmp.State));
					bool zip = (emp.ZipCode != null && posEmp.ZipCode == null ) || (emp.ZipCode != null && emp.ZipCode.CompareTo(posEmp.ZipCode) != 0);
					logger.Debug("	debug : 2a - " + zip + " : " + zip);
					
					bool needsUpdate = (posNotNull && (add1 || add2 || city || state || zip));
					if(needsUpdate)
					{
						try
						{
							newConnection.Open();
							OdbcCommand updateCmd =  new OdbcCommand( "UPDATE micros.emp_def " +
								" SET addr_ln_1 = ? , "+
								" addr_ln_2 = ? , city = ? , postal_code = ? " + " , state_seq = ?" +  
								" WHERE emp_seq = ? " , newConnection );
							OdbcParameter[] parms = new OdbcParameter[6];
							parms[0]  = new OdbcParameter( "add1" , emp.Address1 );
							parms[1]  = new OdbcParameter( "add2" , emp.Address2 );
							parms[2]  = new OdbcParameter( "city" , emp.City );
							parms[3]  = new OdbcParameter( "zip" , emp.ZipCode );
							parms[4] = new OdbcParameter( "stateSeq" , Employee.getStateId(emp.State) );
							parms[5] = new OdbcParameter( "empSeq" , emp.PosId );
				
							foreach( OdbcParameter parm in parms )
							{
								if( parm != null )
								{
									updateCmd.Parameters.Add( parm );
								} 
								else 
								{
									updateCmd.Parameters.Add( new OdbcParameter( "" , "" ) );
								}
							}
							int rowCnt = updateCmd.ExecuteNonQuery();
						} 
						catch( Exception e )
						{
							logger.Error( "Error in Micros DbUpdate() : " + e.ToString() );
						}
						finally
						{
							newConnection.Close();
						}
					}
				}
				catch(Exception ex)
				{
					logger.Error("Error comparing lists: " + ex.ToString());
				}
				
			}
			logger.Debug("debug : 3 - Done");
		}

	}
}
