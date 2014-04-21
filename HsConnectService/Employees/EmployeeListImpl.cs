using HsConnect.Main;
using HsConnect.Data;
using HsSharedObjects.Client;

using System;
using System.Collections;
using System.Xml;

namespace HsConnect.Employees
{
	public abstract class EmployeeListImpl : EmployeeList
	{
		public EmployeeListImpl()
		{
			logger = new SysLog( this.GetType() );
			fastAccessExtId = new Hashtable();
			fastAccessHsId = new Hashtable();
		}

		protected bool updated = false;
		protected ArrayList empList;
		protected SysLog logger;
		public HsDataConnection cnx;
		protected Hashtable fastAccessExtId;
		protected Hashtable fastAccessHsId;
		public String listName = "";
		private ClientDetails _details;

		public abstract void DbLoad();
		public abstract void DbLoad( bool activeOnly );
		public abstract void DbUpdate();
		public abstract void DbInsert();
        public abstract void DbInsert(GoHireEmpInsertList ls);

		public ArrayList EmpList
		{
			get
			{ 
				if( empList == null ) empList = new ArrayList(); 
				return empList;
			}
			set
			{
				this.empList = value;
			}
		}

		public void SetDataConnection( String cnxString )
		{
			this.cnx = new HsDataConnection( cnxString );
		}

		public HsDataConnection Cnx
		{
			get{ return this.cnx; }
			set{ this.cnx = value; }
		}

		public class EmployeeComparer : IComparer  
		{
			int IComparer.Compare( Object x, Object y )  
			{
				Employee emp1 = (Employee) x;
				Employee emp2 = (Employee) y;
				CaseInsensitiveComparer comparer = new CaseInsensitiveComparer();
				int result = comparer.Compare( emp1.LastName , emp2.LastName );
				return result;
			}
		}

		public void SortByLastName()
		{
			EmployeeComparer comp = new EmployeeComparer();
			empList.Sort( comp );
		}

		public bool Updated
		{
			get { return this.updated; }
			set { this.updated = value; }
		}

		public void Add( Employee emp )
		{
			if( empList == null ) empList = new ArrayList();
			if( emp.PosId != -1 && !fastAccessExtId.ContainsKey( emp.PosId ) ) fastAccessExtId.Add( emp.PosId , emp );
			if( emp.HsId != -1 && !fastAccessHsId.ContainsKey( emp.HsId ) ) fastAccessHsId.Add( emp.HsId , emp );
			empList.Add( emp );
		}

        public Boolean Remove(Employee emp)
        {
            try
            {
                if (empList == null) return true;
                if (emp.PosId != -1 && fastAccessExtId.ContainsKey(emp.PosId)) fastAccessExtId.Remove(emp.PosId);
                if (emp.HsId != -1 && fastAccessHsId.ContainsKey(emp.HsId)) fastAccessHsId.Remove(emp.HsId);
                empList.Remove(emp);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                return false;
            }
            return true;
        }

		public int Count
		{
			get 
			{
				if( this.empList == null ) return 0;
				return this.empList.Count; 
			}
			set { }
		}

		public String ListName
		{
			get 
			{
				return this.listName;
			}
			set { this.listName = value; }
		}

		public Employee GetEmployeeByExtId( int id )
		{
			Employee emp = null;
			if( fastAccessExtId.ContainsKey( id ) ) return (Employee) fastAccessExtId[ id ];
			return emp;
		}

		public Employee GetEmployeeByHsId( int id )
		{
			Employee emp = null;
			if( fastAccessHsId.ContainsKey( id ) ) return (Employee) fastAccessHsId[ id ];
			return emp;
		}

		/** This method tries to find a distinct match for the inEmp.  If the
		 * inEmp finds ONLY ONE match within the current list, it return the
		 * matching emp, otherwise it returns a NULL emp object. **/
		public Employee GetMatch( Employee inEmp )
		{
			Employee outEmp = null;
			int cntr = 0;
			foreach( Employee emp in this.EmpList )
			{
				if( emp.FirstName.Equals( inEmp.FirstName ) && emp.LastName.Equals( inEmp.LastName ) )
				{
					outEmp = emp;
					cntr++;
					if( cntr > 1 )
					{
						outEmp = null;
						break;
					}
				}
			}
			return outEmp;
		}

		/** This method tries to find a any match for the inEmp **/
		public bool ContainsHsEmp( Employee inEmp )
		{
			bool contains = false;
			foreach( Employee emp in this.empList )
			{
				if( emp.FirstName.Length > 0 && emp.LastName.Length > 0 )
				{
					if( inEmp.FirstName.StartsWith( emp.FirstName ) && inEmp.LastName.StartsWith( emp.LastName ) )
					{
						return true;
					}
				}
			}
			return contains;
		}

		public void UpdateExtId( int hsId , int posId )
		{
			foreach( Employee emp in empList )
			{
				if( emp.HsId == hsId ) 
				{
					emp.PosId = posId;
					emp.Updated = true;
				}
			}
		}

		public String GetShortXmlString()
		{
			String xmlString = "";
			XmlDocument doc = new XmlDocument();
			XmlNode root = doc.CreateElement( "hsconnect-sync-list" );
			foreach( Employee emp in this.EmpList )
			{
				XmlElement empEle = doc.CreateElement( "employee" );
				empEle.SetAttribute( "pos-id" , emp.PosId.ToString() );
				empEle.SetAttribute( "hs-id" , emp.HsId.ToString() );
				empEle.SetAttribute( "update-status" , emp.UpdateStatus.ToString() );
				root.AppendChild( empEle );
			}
			doc.AppendChild( root );
			xmlString = doc.OuterXml;
			return xmlString;
		}

		public String GetXmlString()
		{
			String xmlString = "";
			XmlDocument doc = new XmlDocument();
			XmlNode root = doc.CreateElement( "hsconnect-employees" );

			foreach( Employee emp in this.EmpList )
			{
				XmlElement empEle = doc.CreateElement( "employee" );
				empEle.SetAttribute( "hs-id" , emp.HsId.ToString() );
				empEle.SetAttribute( "pos-id" , emp.PosId.ToString() );
				empEle.SetAttribute( "alt-num" , emp.AltNumber.ToString() );

				XmlElement fname = doc.CreateElement( "first-name" );
				fname.InnerText = emp.FirstName;
				empEle.AppendChild( fname );

				XmlElement lname = doc.CreateElement( "last-name" );
				lname.InnerText = emp.LastName;
				empEle.AppendChild( lname );

				XmlElement nname = doc.CreateElement( "nick-name" );
				nname.InnerText = emp.NickName;
				empEle.AppendChild( nname );

				XmlElement addr1 = doc.CreateElement( "address1" );
				addr1.InnerText = emp.Address1;
				empEle.AppendChild( addr1 );

				XmlElement addr2 = doc.CreateElement( "address2" );
				addr2.InnerText = emp.Address2;
				empEle.AppendChild( addr2 );

				XmlElement city = doc.CreateElement( "city" );
				city.InnerText = emp.City;
				empEle.AppendChild( city );

				XmlElement state = doc.CreateElement( "state" );
				state.InnerText = emp.State;
				empEle.AppendChild( state );

				XmlElement zip = doc.CreateElement( "zip-code" );
				zip.InnerText = emp.ZipCode;
				empEle.AppendChild( zip );

				XmlElement phone = doc.CreateElement( "phone-number" );
				
				XmlElement area = doc.CreateElement( "area-code" );
				area.InnerText = emp.Phone.Area.ToString();
				phone.AppendChild( area );

				XmlElement prefix = doc.CreateElement( "prefix" );
				prefix.InnerText = emp.Phone.Prefix.ToString();
				phone.AppendChild( prefix );

				XmlElement number = doc.CreateElement( "number" );
				number.InnerText = emp.Phone.Number.ToString();
				phone.AppendChild( number );
				
				empEle.AppendChild( phone );

				XmlElement sms = doc.CreateElement( "sms" );
				sms.InnerText = emp.Mobile;
				empEle.AppendChild( sms );

				XmlElement email = doc.CreateElement( "email" );
				email.InnerText = emp.Email;
				empEle.AppendChild( email );

				XmlElement bday = doc.CreateElement( "birth-date" );
				bday.SetAttribute( "day" , emp.BirthDate.Day.ToString() );
				bday.SetAttribute( "month" , emp.BirthDate.Month.ToString() );
				bday.SetAttribute( "year" , emp.BirthDate.Year.ToString() );
				empEle.AppendChild( bday );

                //add hire date if it is present, to match ClientEmployeesWss parser
                if (emp.HiredDate.CompareTo(new DateTime(1971, 1, 1)) != 0)
                {
                    XmlElement hiredOn = doc.CreateElement("hired-on");
                    hiredOn.SetAttribute("day", emp.HiredDate.Day.ToString());
                    hiredOn.SetAttribute("month", emp.HiredDate.Month.ToString());
                    hiredOn.SetAttribute("year", emp.HiredDate.Year.ToString());
                    empEle.AppendChild(hiredOn);
                }

				XmlElement status = doc.CreateElement( "status" );
				
				XmlElement code = doc.CreateElement( "status-code" );
				code.InnerText = emp.Status.StatusCode.ToString();
				status.AppendChild( code );

				XmlElement inactiveFrom = doc.CreateElement( "inactive-from" );
				inactiveFrom.InnerText = emp.Status.InactiveFrom.ToShortDateString();
				status.AppendChild( inactiveFrom );

				XmlElement inactiveTo = doc.CreateElement( "inactive-to" );
				inactiveTo.InnerText = emp.Status.InactiveTo.ToShortDateString();
				status.AppendChild( inactiveTo );

				XmlElement termOn = doc.CreateElement( "terminated-on" );
				termOn.InnerText = emp.Status.TerminatedOn.ToShortDateString();
				status.AppendChild( termOn );
				
				empEle.AppendChild( status );

				root.AppendChild( empEle );
			}
			doc.AppendChild( root );
			xmlString  = doc.OuterXml;
			return xmlString;
		}

		public String GetPosiXmlString()
		{
			String xmlString = "";
			XmlDocument doc = new XmlDocument();
			XmlNode root = doc.CreateElement( "Employees" );
			foreach( Employee emp in this.EmpList )
			{
				try
				{
					XmlElement empEle = doc.CreateElement( "UpdateEmployee" );
					XmlElement eId = doc.CreateElement( "EmployeeNumber" );
					eId.InnerText = emp.PosId.ToString();
					empEle.AppendChild( eId );
								
					root.AppendChild( empEle );
				}
				catch( Exception ex )
				{
					logger.Error( ex.ToString() );
				}
			}
			doc.AppendChild( root );
			xmlString  = doc.OuterXml;
			return xmlString;
		}

		public IEnumerator GetEnumerator()
		{
			return empList.GetEnumerator();
		}

		public ClientDetails Details
		{
			get { return this._details; }
			set { this._details = value; }
		}

        public Employee[] ToArray()
        {
            Employee[] ret = new Employee[this.EmpList.Count];
            int i = 0; 
            foreach (Employee e in this.EmpList)
            {
                ret[i] = e;
                i++;
            }
            return ret;
        }

	}
}
