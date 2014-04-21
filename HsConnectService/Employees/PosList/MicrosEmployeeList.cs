using HsConnect.Data;
using HsSharedObjects.Client.Preferences;

using System;
using System.Data;
using Microsoft.Data.Odbc;

namespace HsConnect.Employees.PosList
{
	public class MicrosEmployeeList : EmployeeListImpl
	{
		public MicrosEmployeeList()	{}
		
		private HsData data = new HsData();

        public override void DbInsert(GoHireEmpInsertList ls) { }

		public override void DbUpdate()
		{
			OdbcConnection newConnection = this.cnx.GetOdbc();
			foreach( Employee emp in this.empList )
			{
				if( emp.Updated )
				{
					try
					{
						newConnection.Open();
						OdbcCommand updateCmd =  new OdbcCommand( "UPDATE micros.emp_def " +
							" SET first_name = ? , long_first_name = ? , last_name = ? , long_last_name = ? , addr_ln_1 = ? , "+
							" addr_ln_2 = ? , city = ? , postal_code = ? , local_num_1 = ? , e_mail_addr = ? , date_of_birth = ? , other_local_num = ? " +
							" WHERE emp_seq = ? " , newConnection );

						#region Status Update
						OdbcCommand termCmd = new OdbcCommand( "UPDATE micros.emp_def SET termination_date = ? WHERE emp_seq = ? " , newConnection );
						OdbcCommand inactiveCmd = new OdbcCommand( "UPDATE micros.emp_def SET inactive_from_date = ? , inactive_to_date = NULL WHERE emp_seq = ? " , newConnection );
						OdbcCommand activeCmd = new OdbcCommand( "UPDATE micros.emp_def SET inactive_from_date = NULL , inactive_to_date = NULL , termination_date = NULL WHERE emp_seq = ? " , newConnection );
					
						if( emp.Status.Updated && ( emp.Status.StatusCode == EmployeeStatus.TERMINATED ) )
						{
							termCmd.Parameters.Add( new OdbcParameter( "date" , DateTime.Now.ToShortDateString() ) );
							termCmd.Parameters.Add( new OdbcParameter( "id" , emp.PosId ) );
							int rows = termCmd.ExecuteNonQuery();
						}

						if( emp.Status.Updated && ( emp.Status.StatusCode == EmployeeStatus.INACTIVE ) )
						{
							inactiveCmd.Parameters.Add( new OdbcParameter( "date" , DateTime.Now.ToShortDateString() ) );
							inactiveCmd.Parameters.Add( new OdbcParameter( "id" , emp.PosId ) );
							int rows = inactiveCmd.ExecuteNonQuery();
						}

						if( emp.Status.Updated && ( emp.Status.StatusCode == EmployeeStatus.ACTIVE ) )
						{
							activeCmd.Parameters.Add( new OdbcParameter( "id" , emp.PosId ) );
							int rows = activeCmd.ExecuteNonQuery();
							activeCmd.ExecuteNonQuery();
						}

						#endregion

						OdbcParameter[] parms = new OdbcParameter[13];
						parms[0]  = new OdbcParameter( "fName" , emp.FirstName );
						parms[1]  = new OdbcParameter( "longfName" , emp.FirstName );
						parms[2]  = new OdbcParameter( "lName" , emp.LastName );
						parms[3]  = new OdbcParameter( "longlName" , emp.LastName );
						parms[4]  = new OdbcParameter( "add1" , emp.Address1 );
						parms[5]  = new OdbcParameter( "add2" , emp.Address2 );
						parms[6]  = new OdbcParameter( "city" , emp.City );
						parms[7]  = new OdbcParameter( "zip" , emp.ZipCode );
						parms[8]  = new OdbcParameter( "phone" , emp.Phone.ToString() );
						parms[9]  = new OdbcParameter( "email" , emp.Email );
						parms[10]  = new OdbcParameter( "bday" , emp.BirthDate );
						parms[11]  = new OdbcParameter( "mobile" , emp.Mobile );
						parms[12] = new OdbcParameter( "empSeq" , emp.PosId );
				
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
		}

		public override void DbInsert()
		{
			OdbcConnection newConnection = this.cnx.GetOdbc();
			foreach( Employee emp in this.empList )
			{
				if( emp.Inserted )
				{
					String emp_seq = this.getNextObjNum();
					if( emp_seq.Length > 1 )
					{
						try
						{
							newConnection.Open();
							OdbcCommand insertCmd =  new OdbcCommand( "INSERT INTO micros.emp_def ( first_name , last_name , obj_num , " +
								"hire_date , date_of_birth , cntry_seq, chk_name , chk_name2 , effective_from , gender , " +
								"ob_re_hire , i9_id_cat, original_hire_date , last_updated_by , last_updated_date ) " +
								"VALUES( ? , ? , ? , ? , ? , '1' , ? , ? , ? , 'M' , 'T' , 'A' , ? , '1' , ? )" , newConnection );
							OdbcParameter[] parms = new OdbcParameter[10];
							parms[0]  = new OdbcParameter( "fName" , emp.FirstName );
							parms[1]  = new OdbcParameter( "lName" , emp.LastName );
							parms[2]  = new OdbcParameter( "objNum" , emp_seq );
							parms[3]  = new OdbcParameter( "hireDate" , DateTime.Now.ToShortDateString() );
							parms[4]  = new OdbcParameter( "bday" , emp.BirthDate.ToShortDateString() );
							parms[5]  = new OdbcParameter( "chkName1" , emp.FirstName + " " + emp.LastName.Substring( 0 , 1 ) );
							parms[6]  = new OdbcParameter( "chkName2" , emp.FirstName + " " + emp.LastName.Substring( 0 , 1 ) );
							parms[7]  = new OdbcParameter( "effective_from" , DateTime.Now.ToShortDateString() );
							parms[8]  = new OdbcParameter( "oHireDate" , DateTime.Now.ToShortDateString() );
							parms[9]  = new OdbcParameter( "lastUpdated" , DateTime.Now.ToShortDateString() );
							foreach( OdbcParameter parm in parms )
							{
								if( parm != null )
								{
									insertCmd.Parameters.Add( parm );
								} 
								else 
								{
									insertCmd.Parameters.Add( new OdbcParameter( "" , "" ) );
								}
							}
							int rows = insertCmd.ExecuteNonQuery();
						} 
						catch( Exception e )
						{
							logger.Error( "SQL Exception: " + e.ToString() );
						}
						finally
						{
							newConnection.Close();
						}
					}
				}
			}
		}


		public override void DbLoad()
		{
			this.DbLoad( false );
		}

		public override void DbLoad( bool activeOnly )
		{
			OdbcConnection newConnection = this.cnx.GetOdbc();
			try
			{
				DataSet dataSet = new DataSet();
				DateTime temp = DateTime.Now.Subtract( new TimeSpan( 30 , 0 , 0 , 0 ) );
				String daysAgo = temp.ToString( "yyyy-MM-dd HH:mm:ss" );
                bool addHrId = Details.Preferences.PrefExists(Preference.MICROS_SHARED_EMP_HR_ID);
                bool truncHrId = false;
                String hrIdSQL = "";
                if (addHrId)
                {
                    truncHrId = Details.Preferences.PrefExists(Preference.MICROS_TRUNC_HR_ID);
                    hrIdSQL = " a.payroll_id, ";
                }
				OdbcDataAdapter dataAdapter = new OdbcDataAdapter(
					"SELECT a.id, "+hrIdSQL+" first_name , last_name , chk_name, date_of_birth , emp_seq , termination_date, addr_ln_1 , addr_ln_2 , city , b.name as state, postal_code, local_num_1, other_local_num , e_mail_addr,"+
					"inactive_from_date , inactive_to_date FROM micros.emp_def a , micros.state_def b where ( termination_date > '" + daysAgo + "' OR termination_date is NULL )"+
					"and a.state_seq = b.state_seq and a.state_seq is not null "+
					"UNION "+
					"SELECT a.id, "+hrIdSQL+" first_name , last_name , chk_name, date_of_birth , emp_seq , termination_date, addr_ln_1 , addr_ln_2 , city , '' as state, postal_code, local_num_1, other_local_num , e_mail_addr,"+
					"inactive_from_date , inactive_to_date FROM micros.emp_def a , micros.state_def b where ( termination_date > '" + daysAgo + "' OR termination_date is NULL )"+
					"and a.state_seq is null" , newConnection );
				dataAdapter.Fill( dataSet , "micros.emp_def" );
				dataAdapter.Dispose();
				DataRowCollection rows = dataSet.Tables[0].Rows;
				if( rows.Count < 1 )
				{
					logger.Debug( "Returned 0 rows" );
					rows = getAltRows( newConnection, dataSet, daysAgo );		
					logger.Debug( "Alt rows = " + rows.Count );
				}
				else
				{
					logger.Debug( "Returned more than 0 rows..." );
				}
				foreach( DataRow row in rows )
				{
					try
					{
						Employee emp = new Employee();
						emp.PosId = data.GetInt( row , "emp_seq" );
						emp.FirstName = data.GetString( row , "first_name" );
						emp.LastName = data.GetString( row , "last_name" );
						emp.NickName = data.GetString( row , "chk_name" );
						emp.BirthDate = data.GetDate( row , "date_of_birth" );
						emp.Address1 = data.GetString( row , "addr_ln_1" );
						emp.Address2 = data.GetString( row , "addr_ln_2" );
						emp.City = data.GetString( row , "city" );
						emp.State = data.GetString( row , "state" );
						emp.ZipCode = data.GetString( row , "postal_code" );
						emp.Phone = new PhoneNumber( data.GetString( row , "local_num_1" ) );
						emp.Mobile = data.GetString( row , "other_local_num" );
						emp.Email = data.GetString( row , "e_mail_addr" );
                        if (addHrId)
                        {
                            emp.AltNumber = data.GetInt(row, "payroll_id");
                            //if truncHrId, chomp() until desired length
                            if (truncHrId)
                            {
                                String tmp = emp.AltNumber.ToString();
                                while(tmp.Length > 8){
                                    tmp = tmp.Substring(1);
                                }
                                emp.AltNumber = int.Parse(tmp);
                            }
                        }
						//logger.Log( emp.FirstName + " " + emp.LastName + " is disabled = " + data.GetString( row , "ob_account_disabled" ) );
						//bool isDisabled = String.Compare( data.GetString( row , "ob_account_disabled" ), "T" ) == 0;
						
						// load status
						emp.Status = new EmployeeStatus();
						emp.Status.TerminatedOn = data.GetDate( row , "termination_date" );
						emp.Status.InactiveFrom = data.GetDate( row , "inactive_from_date" );
						emp.Status.InactiveTo = data.GetDate( row , "inactive_to_date" );
						
						// set status code
						if( ( emp.Status.InactiveTo == new DateTime(1,1,1) ) && ( emp.Status.InactiveFrom == new DateTime(1,1,1) ) && ( emp.Status.TerminatedOn == new DateTime(1,1,1) ) ) 
						{
                            logger.Debug(emp.PosId + "emp setting status: ACTIVE");
							emp.Status.StatusCode = EmployeeStatus.ACTIVE;
						}
						else if( emp.Status.InactiveTo >= DateTime.Now || ( new DateTime(1,1,1) < emp.Status.InactiveFrom && emp.Status.InactiveFrom <= DateTime.Now && emp.Status.InactiveTo == new DateTime(1,1,1) ) ) 
						{
                            logger.Debug(emp.PosId + "emp setting status: INACTIVE");
							emp.Status.StatusCode = EmployeeStatus.INACTIVE;
						}
                        else if (emp.Status.TerminatedOn > new DateTime(1, 1, 1))
                        {
                            logger.Debug(emp.PosId + "emp setting status: TERMINATED");
                            emp.Status.StatusCode = EmployeeStatus.TERMINATED;
                        }
                        else
                        {
                            logger.Debug(emp.PosId + "emp setting default status: ACTIVE");
                            emp.Status.StatusCode = EmployeeStatus.ACTIVE;
                        }

						if( row["id"] != null ) logger.Debug( "id from row: " + data.GetString( row , "id" ) );
						if( row["id"] == null || String.Compare( data.GetString( row , "id" ), "null", true ) == 0  || data.GetString( row , "id" ).Length < 1 )
						{
                            logger.Debug(emp.PosId + "emp setting null id status: TERMINATED");
							emp.Status.StatusCode = EmployeeStatus.TERMINATED;
						}

                        if (emp.FirstName.Length > 0 && emp.LastName.Length > 0)
                        {
                            logger.Debug("adding " + emp.FirstName + " " + emp.LastName);
                            this.Add(emp);
                        }
                        else
                        {
                            logger.Debug(emp.PosId + "emp not added to sync list. reason: first or last name are blank");
                        }
					}
					catch( Exception ex )
					{
						logger.Error( "Error adding micros employee in Load(): " + ex.ToString() );
				//		Main.Run.errorList.Add(ex);
					}
				}
			}
			catch( Exception ex )
			{
				logger.Error( ex.ToString() );
				Main.Run.errorList.Add(ex);
			}
			finally
			{
				newConnection.Close();
			}
		}

		private String getNextObjNum()
		{
			String ob = "";
			OdbcConnection newConnection = this.cnx.GetOdbc();
			try
			{
				newConnection.Open();
				OdbcCommand cmd = new OdbcCommand("SELECT MAX(obj_num)+1 as NEXT_OBJ_NUM FROM micros.emp_def" , newConnection );
				OdbcDataReader reader = cmd.ExecuteReader();
				while(reader.Read()) 
				{
					ob = reader.GetString( 0 );
				}
				reader.Close();
			}
			catch( Exception ex )
			{
				logger.Error( ex.ToString() );
				
			}
			finally
			{
				newConnection.Close();
			}
			return ob;
		}
		private DataRowCollection getAltRows( OdbcConnection newConnection, DataSet dataSet, String daysAgo )
		{
			OdbcDataAdapter dataAdapter = new OdbcDataAdapter(
				"SELECT id, first_name , last_name , chk_name, date_of_birth , emp_seq , termination_date, addr_ln_1 , addr_ln_2 , city , '' as state, postal_code, local_num_1, other_local_num , e_mail_addr,"+
				"inactive_from_date , inactive_to_date FROM micros.emp_def where ( termination_date > '" + daysAgo + "' OR termination_date is NULL )" , newConnection );
			dataAdapter.Fill( dataSet , "micros.emp_def" );
			dataAdapter.Dispose();
			DataRowCollection rows = dataSet.Tables[0].Rows;
			return rows;
		}
	}
}
