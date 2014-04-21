using HsConnect.Data;
using HsConnect.Employees.Workday;

using System;
using System.Data;
using System.Collections;
using Microsoft.Data.Odbc;
using System.Threading;
using HsConnect.Jobs;
using ALOHAMGRLib;
using System.Xml;
using HsConnect.Xml;

namespace HsConnect.Employees.PosList
{
	public class AlohaEmployeeList : EmployeeListImpl
	{
		public AlohaEmployeeList(){}

		private HsData data = new HsData();
		private ArrayList newEmps;
		private ArrayList updateEmps;
		private ArrayList currentIds;
		private int lastUsedId = 100;

		public override void DbLoad()
		{	
			this.DbLoad( false );
		}
		//tag names
		private readonly static string DB_UPDATE_TAG = "DBUPDATE";
		private readonly static string TYPE_TAG = "Type";
		private readonly static string TABLE_ID_TAG = "TableId";
		private readonly static string COLUMN_UPDATE_TAG = "ColumnUpdate";
		private readonly static string COLUMN_ID_TAG = "ColumnId";
		private readonly static string VALUE_TAG = "Value";
		private readonly static string RESULT_TAG = "RESULT";
		private readonly static string INFO_TAG = "Info";
		private readonly static string MSG_TAG = "Msg";

		//tag values
		private readonly static string TYPE_ADD_VALUE = "Add";
		private readonly static string SUCCESS_VALUE = "SUCCESS";

		//constants for column IDs
		private static readonly string EMP_DBF_TABLE_ID = "7";
		private static readonly string EMP_DBF_TABLE_COLUMN_ID = "0";
		private static readonly string EMP_DBF_TABLE_COLUMN_FIRST_NAME = "3";
		private static readonly string EMP_DBF_TABLE_COLUMN_LAST_NAME = "5";
		private static readonly string EMP_DBF_TABLE_COLUMN_ADDR_1 = "7";
		private static readonly string EMP_DBF_TABLE_COLUMN_ADDR_2 = "8";
		private static readonly string EMP_DBF_TABLE_COLUMN_CITY = "9";
		private static readonly string EMP_DBF_TABLE_COLUMN_STATE = "10";
		private static readonly string EMP_DBF_TABLE_COLUMN_ZIP = "11";
		private static readonly string EMP_DBF_TABLE_COLUMN_PHONE = "12";
        private static readonly string EMP_DBF_TABLE_COLUMN_BIRTHDATE = "13";
        private static readonly string EMP_DBF_TABLE_COLUMN_HIREDATE = "14";
        private static readonly string EMP_DBF_TABLE_COLUMN_SSN = "70";
		private static readonly string[] EMP_DBF_TABLE_COLUMNS_JOBS = {"27", "28", "29", "30", "31", "32", "33", "34", "35", "36"};
		private static readonly string[] EMP_DBF_TABLE_COLUMNS_RATES = {"37", "38", "39", "40", "41", "42", "43", "44", "45", "46"};

		//DB constants
		private readonly static string EMP_ID_QUERY_STRING = "SELECT ID FROM EMP ORDER BY ID";
		private readonly static string EMP_TABLE_NAME = "EMP";

		//for COM interop with AlohaMGR...do not change
		private static readonly Byte[] guidArray = {0xb7,0xca,0x67,0x8a,0x6f,0xab,0x8f,0x20};
		private static readonly Guid CLSID_DatabaseTXNProcessor = new Guid( 0x366c3c88,unchecked((short)0x9600), unchecked((short)0x4eb5),guidArray);

		/*
		 * Creates a new XmlElement object and returns it.  
		 * The element tagname is defined by the name parameter.
		 * The inner text of the xml element empty.  
		 */
		private XmlElement CreateElement(XmlDocument doc, string name)
		{
			return doc.CreateElement( name );
		}

		/*
		 * Creates a new XmlElement object and returns it.  The inner text of the 
		 * xml element is defined by the val parameter.  Then element tagname is defined
		 * by the name parameter
		 */
		private XmlElement CreateElement(XmlDocument doc, string name, string val)
		{
			XmlElement ele = CreateElement(doc, name);
			ele.InnerText = val;
			return ele;
		}

		/*
		 * Takes in an XmlDocument object, a string representing the column ID (for emp.dbf table)
		 * and a string representing the value.
		 * 
		 * Final format of element returned:
		 * 	
				<ColumnUpdate>
					<ColumnId>col</ColumnId>
					<Value>val</Value>
				</ColumnUpdate>
		 */
		private XmlElement CreateColumnElement(XmlDocument doc, string col, string val)
		{
			
			XmlElement ele = CreateElement( doc, COLUMN_UPDATE_TAG );
			ele.AppendChild( CreateElement( doc, COLUMN_ID_TAG, col ) );
			ele.AppendChild( CreateElement( doc, VALUE_TAG, val ) );
			return ele;
		}

		/*
		 * Util method to generate the insert XML string for a single new GoHire employee.  
		 ********************* SEE BOTTOM OF CLASS FOR XML FORMAT*************************
		 */
		private string GetEmpInsertXmlString(GoHireEmpInsert emp)
		{
			XmlDocument doc = new XmlDocument();
			XmlNode root = CreateElement( doc, DB_UPDATE_TAG );
			//<Type>Add</Type>
			root.AppendChild( CreateElement(doc, TYPE_TAG, TYPE_ADD_VALUE) );
			//<TableId>7</TableId>
			root.AppendChild( CreateElement(doc, TABLE_ID_TAG, EMP_DBF_TABLE_ID) );
				
			//empId
			root.AppendChild( CreateColumnElement(doc, EMP_DBF_TABLE_COLUMN_ID, ""+ NextAvailableEmpID) );
			//first name
			root.AppendChild( CreateColumnElement(doc, EMP_DBF_TABLE_COLUMN_FIRST_NAME, emp.firstName) );
			//last name
			root.AppendChild( CreateColumnElement(doc, EMP_DBF_TABLE_COLUMN_LAST_NAME, emp.lastName) );
			//addr1
			root.AppendChild( CreateColumnElement(doc, EMP_DBF_TABLE_COLUMN_ADDR_1, emp.addr1) );
			//addr2
			root.AppendChild( CreateColumnElement(doc, EMP_DBF_TABLE_COLUMN_ADDR_2, emp.addr2) );
			//city
			root.AppendChild( CreateColumnElement(doc, EMP_DBF_TABLE_COLUMN_CITY, emp.city) );
			//state
			root.AppendChild( CreateColumnElement(doc, EMP_DBF_TABLE_COLUMN_STATE, emp.state) );
			//zip
			root.AppendChild( CreateColumnElement(doc, EMP_DBF_TABLE_COLUMN_ZIP, emp.zip) );
			//phone
			root.AppendChild( CreateColumnElement(doc, EMP_DBF_TABLE_COLUMN_PHONE, emp.phoneNumber) );
            //birth
            root.AppendChild(CreateColumnElement(doc, EMP_DBF_TABLE_COLUMN_BIRTHDATE, emp.birthDate.ToString("yyyyMMdd")));
            //hire
            root.AppendChild(CreateColumnElement(doc, EMP_DBF_TABLE_COLUMN_HIREDATE, emp.hireDate.ToString("yyyyMMdd")));
            //ssn
            root.AppendChild(CreateColumnElement(doc, EMP_DBF_TABLE_COLUMN_SSN, "111-11-1111"));

			for( int i = 0; i< emp.jobs.Count; i++ )//using old-style for loop because i need the counter
			{
				Job j = (Job)(emp.jobs[i]);
				string jobCol = EMP_DBF_TABLE_COLUMNS_JOBS[i];
				string rateCol = EMP_DBF_TABLE_COLUMNS_RATES[i];

				root.AppendChild( CreateColumnElement(doc, jobCol, "" + j.ExtId) );
                
                //ensure this is formatted properly for insert as a double
                string wageStr = "" + j.DefaultWage;
                if (wageStr.IndexOf('.') < 0)
                {
                    wageStr += ".00";
                }
				root.AppendChild( CreateColumnElement(doc, rateCol, wageStr) );
			}

			doc.AppendChild( root );
			string xmlString = doc.OuterXml;
			logger.Debug(xmlString);
			return xmlString;
		}

		/*
		 * Util method to query the Emp.dbf for currently used empIDs.  
		 * */
		private DataRowCollection GetIdRows()
		{
				CopyEmpFiles();
				DataSet dataSet = ExecuteQueryOnConnection(EMP_ID_QUERY_STRING, EMP_TABLE_NAME);
				return dataSet.Tables[0].Rows;
		}

		/*
		 * list of currently used ID.  Lazy loaded on first access.
		 */ 
		private ArrayList CurrentEmpIDs{
			get 
			{
				if(currentIds == null)
				{
					DataRowCollection rows = GetIdRows();
                    currentIds = new ArrayList();
					foreach (DataRow row in rows)
					{
						currentIds.Add(data.GetInt( row , "ID" ));
					}
				}
				return currentIds;
			}
		}

		/*
		 * Property to get next available ID from the emp.dbf
		 */ 
		private int NextAvailableEmpID
		{
			get
			{
				int id = lastUsedId + 1;
				while( id < Int32.MaxValue &&  CurrentEmpIDs.Contains(id))
				{
					id++;
				}
				lastUsedId = id;
				logger.Debug("Next ID is:  " + lastUsedId);
				return lastUsedId;
			}
		}

		/*
		 * Util method to start the database batch call, send the input, 
		 * close the batch call and return the output XML.
		 */
		private string MakeInsertCall(IDatabaseTXNProcessor p, string xmlString)
		{
			string outStr = @"";
			p.StartDatabaseBatch();
			p.ProcessDatabaseTXN(xmlString, 1, 1, 1, out outStr);
			p.EndDatabaseBatch();
			return outStr;
		}

		/*
		 * Util method to parse the outstring to discover success
		 * or failure.  The returned xml will have a RESULT tag
		 * with a value of SUCCESS if the import worked.  All other 
		 * values of RESULT will return false
		 ************ SEE BOTTOM OF CLASS FOR XML FORMAT*************
		 */
		private bool IsSuccess(string outString)
		{
			HsXmlReader reader = new HsXmlReader();
			reader.LoadXml(outString);
		
			XmlNode root = reader.FirstChild;
			string result = root.SelectSingleNode(RESULT_TAG).InnerText;
			return (result != null && result.Equals(SUCCESS_VALUE));
		}

		/*
		 *Util method to parse the failure reason out of the result XML.
		  ************ SEE BOTTOM OF CLASS FOR XML FORMAT***************
		 */ 
		private string GetFailureReason(string outString)
		{
			HsXmlReader reader = new HsXmlReader();
			reader.LoadXml(outString);
		
			XmlNode root = reader.FirstChild;
			return root.SelectSingleNode(INFO_TAG).SelectSingleNode(MSG_TAG).InnerText;
		}

		/*
		 * Util method to set the employees insert status and reason based
		 * on the output string.
		 */
		private void CheckInsertStatus(GoHireEmpInsert emp, string outString)
		{
			if(IsSuccess(outString))
			{
				emp.insertStatus = 1;
				emp.insertReason = "success";
			}
			else
			{
				emp.insertStatus = -2;
				emp.insertReason = GetFailureReason(outString);
			}
		}

		/*
		 * DbInsert(GoHireEmpInsertList):  Takes in a list of employees
		 * from our GoHire web services, which need to be inserted into 
		 * the Aloha DB.  Makes a COM Interop connection to AlohaManager, 
		 * and makes one call per employee to the Aloha SDK, attmpting to
		 * insert the employee.
		 */
		public override void DbInsert(GoHireEmpInsertList ls)
		{
			logger.Debug("Begin GoHireEmpInsert DbInsert() in Aloha POS");
			logger.Debug("Using Aloha SDK");
			
			//connect to AlohaMGR	            
			IDatabaseTXNProcessor p;
			Type dbType = Type.GetTypeFromCLSID(CLSID_DatabaseTXNProcessor, true);
			p = (IDatabaseTXNProcessor)Activator.CreateInstance(dbType);

			foreach (GoHireEmpInsert emp in ls.getList())
			{
				if (emp.insertStatus != 0)
				{
					continue;
				}
				try
				{
					//create xml to work form
					string xmlString = GetEmpInsertXmlString( emp );
					//make calls to insert
					string outString = MakeInsertCall( p, xmlString );
					//check status
					CheckInsertStatus(emp, outString);
				}
				catch(Exception ex)
				{
					logger.Error("Error inserting employee:  " + emp.firstName + " " + emp.lastName, ex);
				}
			}
		}

		public override void DbLoad( bool activeOnly )
		{
			CopyEmpFiles();
			OdbcConnection newConnection = GetConnectionString();
            bool useSSNFieldAsHRID = this.Details.Preferences.PrefExists(HsSharedObjects.Client.Preferences.Preference.ALOHA_USE_SSN_AS_HR_ID);
			try
			{
				logger.Debug( "creating DataSet" );
				DataSet dataSet = new DataSet();
				DateTime temp = DateTime.Now.Subtract( new TimeSpan( 30 , 0 , 0 , 0 ) );
				String daysAgo = temp.ToShortDateString();
				logger.Debug( "creating OdbcAdapter" );
				OdbcDataAdapter dataAdapter = new OdbcDataAdapter(
					"SELECT * FROM EMP" , newConnection );
				logger.Debug( "filling adapter" );
				dataAdapter.Fill( dataSet , "EMP" );
				logger.Debug( "filled" );
				dataAdapter.Dispose();
				DataRowCollection rows = dataSet.Tables[0].Rows;
				foreach( DataRow row in rows )
				{
					try
					{
						Employee emp = new Employee();
						emp.PosId = data.GetInt( row , "ID" );
						emp.FirstName = data.GetString( row , "FIRSTNAME" );
						emp.LastName = data.GetString( row , "LASTNAME" );
						emp.Address1 = data.GetString( row , "ADDRESS1" );
						emp.Address2 = data.GetString( row , "ADDRESS2" );
						emp.City = data.GetString( row , "CITY" );
						emp.State = data.GetString( row , "STATE" );
						emp.ZipCode = data.GetString( row , "ZIPCODE" );
						emp.Phone = new PhoneNumber( data.GetString( row , "PHONE" ) );
						emp.BirthDate = data.GetDate( row , "BIRTHDAY" );
                        emp.NickName = data.GetString(row, "NICKNAME");
						
						// load status
						emp.Status = new EmployeeStatus();
						emp.Status.InactiveTo = data.GetDate( row , "RTNDAY" );

						if(emp.LastName.Equals("Appleby"))
						{
							Console.Write("apple");
						}

						// set status code
						if( data.GetString( row , "TERMINATED" ).Equals( "N" ) ) 
						{
							emp.Status.StatusCode = EmployeeStatus.ACTIVE;
						}
						else if( data.GetString( row , "TERMINATED" ).Equals( "Y" ) && emp.Status.InactiveTo > new DateTime(1,1,1) && emp.Status.InactiveTo > DateTime.Now ) 
						{
							emp.Status.StatusCode = EmployeeStatus.INACTIVE;
						} 
						else if( data.GetString( row , "TERMINATED" ).Equals( "Y" ) && emp.Status.InactiveTo > new DateTime(1,1,1) && emp.Status.InactiveTo <= DateTime.Now ) 
						{
							emp.Status.StatusCode = EmployeeStatus.TERMINATED;
						} 
						else if( data.GetString( row , "TERMINATED" ).Equals( "Y" ) && emp.Status.InactiveTo == new DateTime(1,1,1) ) 
						{
							emp.Status.StatusCode = EmployeeStatus.TERMINATED;
						}
						else emp.Status.StatusCode = EmployeeStatus.ACTIVE;

                        //add HR ID if setting is there
                        if (useSSNFieldAsHRID)
                        {
                            emp.AltNumber = data.GetInt(row, "SSN");
                        }

						this.Add( emp );
					}
					catch( Exception ex )
					{
						logger.Error( "Error adding Aloha employee in Load(): " + ex.ToString() );
				//		Main.Run.errorList.Add(ex);
					}
				}
			}
			catch( Exception ex )
			{
				logger.Error( "Error in Aloha Emp DBLoad: " + ex.ToString() );
				Main.Run.errorList.Add(ex);
			}
			finally
			{
				newConnection.Close();
			}
		}
		public override void DbUpdate(){
			foreach(Employee e in updateEmps)
			{
				HsFile hsFile = new HsFile();
				//hsFile.Copy( this.Cnx.Dsn+@"\NEWDATA", this.Cnx.Dsn+@"\hstmp", "Aloha.ini" );
				String empCnxStr = this.cnx.ConnectionString + @"\NEWDATA";
				OdbcConnection newConnection = this.cnx.GetCustomOdbc( empCnxStr );
				String tblName = "EMP";
				String updateEmp = "UPDATE " + tblName + " SET FIRSTNAME= '"+e.FirstName+"' , LASTNAME=  '"+e.LastName+"' WHERE ID = "+e.PosId ;
				logger.Log(updateEmp);
				newConnection.Open();
				OdbcCommand updateCmd = new OdbcCommand( updateEmp , newConnection );
				int rows = updateCmd.ExecuteNonQuery();
			}
		}
		public override void DbInsert(){
			foreach(Employee e in newEmps)
			{
				HsFile hsFile = new HsFile();
				//hsFile.Copy( this.Cnx.Dsn+@"\NEWDATA", this.Cnx.Dsn+@"\hstmp", "Aloha.ini" );
				String empCnxStr = this.cnx.ConnectionString + @"\NEWDATA";
				OdbcConnection newConnection = this.cnx.GetCustomOdbc( empCnxStr );
				String tblName = "EMP";
				String addEmp = "INSERT INTO " + tblName + " (ID, FIRSTNAME, LASTNAME, JOBCODE1, PAYRATE1, ACCESS1, TERMINATED) VALUES ("+e.PosId +",'"+e.FirstName+"','"+e.LastName+"',"+ 101 +","+2.25+","+1+","+"'N'" + ")" ;
				logger.Log(addEmp);
				newConnection.Open();
				OdbcCommand addCmd = new OdbcCommand( addEmp , newConnection );
				int rows = addCmd.ExecuteNonQuery();
			}
		}

		public ArrayList NewEmps
		{
			get{return newEmps;}
			set{this.newEmps = value;}
		}
		public ArrayList UpdateEmps
		{
			get{return updateEmps;}
			set{this.updateEmps = value;}
		}
	
		/*
		 * Util method to copy the emp DBF and index files into our temp DIR
		 */ 
		private void CopyEmpFiles()
		{
			HsFile hsFile = new HsFile();
			logger.Debug( "this.Cnx.Dsn = " + this.Cnx.Dsn );
			hsFile.Copy( this.Cnx.Dsn + @"\Newdata", this.Cnx.Dsn + @"\hstemp", "EMP.DBF" );
			hsFile.Copy( this.Cnx.Dsn + @"\Newdata", this.Cnx.Dsn + @"\hstemp", "EMP.CDX" );
		}

		/*
		 * Util method to get a connection string for connecting to the DBFs
		 */		
		private OdbcConnection GetConnectionString()
		{
			String empCnxStr = this.cnx.ConnectionString + @"\hstemp";
			return this.cnx.GetCustomOdbc( empCnxStr );
		}

		/*
		 * Util method for executing a DB query on the table supplied
		 */ 
		private DataSet ExecuteQueryOnConnection(string queryString, string tableName)
		{
			OdbcConnection newConnection = null;
			DataSet dataSet = new DataSet();
			try
			{
				newConnection = GetConnectionString();
				newConnection.Open();
				
				OdbcDataAdapter dataAdapter = new OdbcDataAdapter(queryString, newConnection);
				dataAdapter.Fill(dataSet, tableName);
				dataAdapter.Dispose();
				
			}
			catch(Exception ex)
			{
				logger.Error("error loading IDs for employees.", ex);
			}
			finally
			{
				newConnection.Close();
			}
			return dataSet;
		}

	}
}

/* INPUT XML
 * 
		<DBUPDATE>
			<Type>Add</Type>
			<TableId>7</TableId>
			<ColumnUpdate>
				<ColumnId>0</ColumnId>
				<Value>166</Value>
			</ColumnUpdate>
			<ColumnUpdate>
				<ColumnId>3</ColumnId>
				<Value>John</Value>
			</ColumnUpdate>
			<ColumnUpdate>
				<ColumnId>5</ColumnId>
				<Value>DB_Up_ADD</Value>
			</ColumnUpdate>
			<ColumnUpdate>
				<ColumnId>12</ColumnId>
				<Value>817-123-1234</Value>
			</ColumnUpdate>
			<ColumnUpdate>
				<ColumnId>27</ColumnId>
				<Value>200</Value>
			</ColumnUpdate>
			<ColumnUpdate>
				<ColumnId>37</ColumnId>
				<Value>6.66</Value>
			</ColumnUpdate>
			<ColumnUpdate>
				<ColumnId>47</ColumnId>
				<Value>20</Value>
			</ColumnUpdate>
			<ColumnUpdate>
				<ColumnId>70</ColumnId>
				<Value>333-22-4444</Value>
			</ColumnUpdate>
		</DBUPDATE>
*/

/*
 * RESULT XML - SUCCESS
 * 
 *	<DB_UPDATE_RESULTS>
 *		<RESULT>SUCCESS</RESULT>
 *	</DB_UPDATE_RESULTS>
 * 
 */


/*
 * RESULT XML - FAILURE
 * 
 *	<DB_UPDATE_RESULTS>
 *		<RESULT>FAILURE</RESULT>
 *		<Info>
 *			<MsgCode>17</MsgCode>
 *			<Msg>Cannot find jobcode record id 20</Msg>
 *		</Info>
 *	</DB_UPDATE_RESULTS>
 * 
 * 
 * */