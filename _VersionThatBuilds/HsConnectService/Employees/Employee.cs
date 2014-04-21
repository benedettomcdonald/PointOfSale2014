using HsConnect.EmpJobs;
using HsConnect.Main;

using System;
using System.Xml;
using System.Collections;

namespace HsConnect.Employees
{
	public class Employee
	{
		public Employee()
		{
		}

		private int hsId = -1;
		private int posId = -1;
		private int updateStatus = -1;
		private int altNumber = -1;
		private String fName = "";
		private String lName = "";
		private String nickName = "";
		private String address1 = "";
		private String address2 = "";
		private String zip = "";
		private String city = "";
		private String state = "";
		private PhoneNumber phone = new PhoneNumber( );
		private String mobile = "";
		private String email = "";
		private String hsUserName = "";
		private DateTime birthDate = new DateTime( 1971 , 1 , 1 );
		private EmployeeStatus status;		
		private EmpJobList empJobs;
		private int primaryJobCode;

        /*
         * Added hiredDate 07/15/2013
         * if it's set to anything other than 1/1/1971, we include it
         * with the Wss sync
         */
        private DateTime hiredDate = new DateTime(1971, 1, 1);
		
		private bool updated = false;
		private bool inserted = false;

		public static int HS_CURRENT = 2;
		public static int HS_UPDATED = 1;
		private static Hashtable stateListShort = new Hashtable();
		private static Hashtable stateListLong = new Hashtable();

		// required by POSitouch, for use with schedule importing
		private bool usePos = false;
		private int posUserType = 0;
		private String ssn = "";
		private int tableAccessCode = 0;
		private int tableAccess1 = 0;
		private int tableAccess2 = 0;
		private int tableAccess3 = 0;
		private int tableAccess4 = 0;
		private String magCardNum = "";
		private bool enforeSchedule = false;
		private bool punchOutWithOpen = false;
		private bool usePosWoutPunching = false;
		private String employeeType = "";
		private bool tipped = false;

		#region Posi Fields

		public String EmployeeType
		{
			get{ return this.employeeType; }
			set{ this.employeeType = value; }
		}

		public bool Tipped
		{
			get{ return this.tipped; }
			set{ this.tipped = value; }
		}

		public bool UsePosWoutPunching
		{
			get{ return this.usePosWoutPunching; }
			set{ this.usePosWoutPunching = value; }
		}

		public bool PunchOutWithOpen
		{
			get{ return this.punchOutWithOpen; }
			set{ this.punchOutWithOpen = value; }
		}

			public bool EnforceSchedule
		{
			get{ return this.enforeSchedule; }
			set{ this.enforeSchedule = value; }
		}

		public String MagCardNum
		{
			get{ return this.magCardNum; }
			set{ this.magCardNum = value; }
		}

		public int TableAccessCode1
		{
			get{ return this.tableAccess1; }
			set{ this.tableAccess1 = value; }
		}

		public int TableAccessCode2
		{
			get{ return this.tableAccess2; }
			set{ this.tableAccess2 = value; }
		}

		public int TableAccessCode3
		{
			get{ return this.tableAccess3; }
			set{ this.tableAccess3 = value; }
		}

		public int TableAccessCode4
		{
			get{ return this.tableAccess4; }
			set{ this.tableAccess4 = value; }
		}

		public int TableAccessCode
		{
			get{ return this.tableAccessCode; }
			set{ this.tableAccessCode = value; }
		}

		public String Ssn
		{
			get{ return this.ssn; }
			set{ this.ssn = value; }
		}

		public int PosUserType
		{
			get{ return this.posUserType; }
			set{ this.posUserType = value; }
		}

		public bool UsePos
		{
			get{ return this.usePos; }
			set{ this.usePos = value; }
		}

		#endregion

		public int HsId
		{
			get	{ return hsId; }
			set	{ this.hsId = value; }
		}

		public int PosId
		{
			get { return this.posId; }
			set	{ this.posId = value; }
		}

		public int AltNumber
		{
			get { return this.altNumber; }
			set	{ this.altNumber = value; }
		}

		public bool Updated
		{
			get { return this.updated; }
			set 
			{
				this.updated = value; 
			}
		}

		public bool Inserted
		{
			get { return this.inserted; }
			set 
			{
				this.inserted = value; 
			}
		}

		public int UpdateStatus
		{
			get { return this.updateStatus; }
			set { this.updateStatus = value; }
		}

		public String FirstName
		{
			get	{ return this.fName; }
			set	{ this.fName = value; }
		}

		public String LastName
		{
			get	{ return this.lName; }
			set	{ this.lName = value; }
		}

		public String NickName
		{
			get	{ return this.nickName; }
			set	{ this.nickName = value; }
		}

		public DateTime BirthDate
		{
			get	{ return this.birthDate; }
			set	{ this.birthDate = value; }
		}

        public DateTime HiredDate
        {
            get { return this.hiredDate; }
            set { this.hiredDate = value; }
        }

		public String Address1
		{
			get { return this.address1;	}
			set	{ this.address1 = value; }
		}

		public String Address2
		{
			get { return this.address2;	}
			set	{ this.address2 = value; }
		}

		public String City
		{
			get { return this.city;	}
			set	{ this.city = value; }
		}

		public String State
		{
			get { return this.state; }
			set { this.state = value; }
		}

		public String ZipCode
		{
			get	{ return this.zip; }
			set	{ this.zip = value;	}
		}

		public PhoneNumber Phone
		{
			get	{ return this.phone; }
			set
			{
				this.phone = value; 
			}
		}

		public String Mobile
		{
			get { return this.mobile; }
			set	{ this.mobile = value; }
		}

		public String Email
		{
			get { return this.email; }
			set	{ this.email = value; }
		}

		public String HsUserName
		{
			get	{ return this.hsUserName; }
			set	{ this.hsUserName = value; }
		}

		public EmployeeStatus Status
		{
			get { return this.status; }
			set	{ this.status = value; }
		}

		public EmpJobList EmployeeJobs
		{
			get{ return this.empJobs; }
			set{ this.empJobs = value; }
		}

		public int PrimaryJob
		{
			get{ return primaryJobCode; }
			set{ this.primaryJobCode = value; }
		}

		public String GetXmlString()
		{
			String xmlString = "";
			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement( "hsconnect-employee" );

			root.SetAttribute( "hs-id" , this.HsId.ToString() );
			root.SetAttribute( "pos-id" , this.PosId.ToString() );

			XmlElement fname = doc.CreateElement( "first-name" );
			fname.InnerText = this.FirstName;
			root.AppendChild( fname );

			XmlElement lname = doc.CreateElement( "last-name" );
			lname.InnerText = this.LastName;
			root.AppendChild( lname );

			XmlElement nname = doc.CreateElement( "nick-name" );
			nname.InnerText = this.NickName;
			root.AppendChild( nname );

			XmlElement addr1 = doc.CreateElement( "address1" );
			addr1.InnerText = this.Address1;
			root.AppendChild( addr1 );

			XmlElement addr2 = doc.CreateElement( "address2" );
			addr2.InnerText = this.Address2;
			root.AppendChild( addr2 );

			XmlElement city = doc.CreateElement( "city" );
			city.InnerText = this.City;
			root.AppendChild( city );

			XmlElement state = doc.CreateElement( "state" );
			state.InnerText = this.State;
			root.AppendChild( state );

			XmlElement zip = doc.CreateElement( "zip-code" );
			zip.InnerText = this.ZipCode;
			root.AppendChild( zip );

			XmlElement phone = doc.CreateElement( "phone-number" );
			
			XmlElement area = doc.CreateElement( "area-code" );
			area.InnerText = this.Phone.Area.ToString();
			phone.AppendChild( area );

			XmlElement prefix = doc.CreateElement( "prefix" );
			prefix.InnerText = this.Phone.Prefix.ToString();
			phone.AppendChild( prefix );

			XmlElement number = doc.CreateElement( "number" );
			number.InnerText = this.Phone.Number.ToString();
			phone.AppendChild( number );
			
			root.AppendChild( phone );

			XmlElement sms = doc.CreateElement( "sms" );
			sms.InnerText = this.Mobile;
			root.AppendChild( sms );

			XmlElement email = doc.CreateElement( "email" );
			email.InnerText = this.Email;
			root.AppendChild( email );

            /*
             * if hiredDate is set to anything other than 1/1/1971, include it
             */
            if (this.hiredDate.CompareTo(new DateTime(1971, 1, 1)) != 0){
                XmlElement hDate = doc.CreateElement("hired-on");
                hDate.SetAttribute("day", "" + this.hiredDate.Day);
                hDate.SetAttribute("month", "" + this.hiredDate.Month);
                hDate.SetAttribute("year", "" + this.hiredDate.Year);
            }

			XmlElement bday = doc.CreateElement( "birth-date" );
			bday.SetAttribute( "day" , this.BirthDate.Day.ToString() );
			bday.SetAttribute( "month" , this.BirthDate.Month.ToString() );
			bday.SetAttribute( "year" , this.BirthDate.Year.ToString() );
			root.AppendChild( bday );

			XmlElement primaryJob = doc.CreateElement( "primary-job-code" );
			primaryJob.InnerText = this.PrimaryJob.ToString();;
			root.AppendChild( primaryJob );

			XmlElement status = doc.CreateElement( "status" );
			
			XmlElement code = doc.CreateElement( "status-code" );
			code.InnerText = this.Status.StatusCode.ToString();
			status.AppendChild( code );

			XmlElement inactiveFrom = doc.CreateElement( "inactive-from" );
			inactiveFrom.InnerText = this.Status.InactiveFrom.ToShortDateString();
			status.AppendChild( inactiveFrom );

			XmlElement inactiveTo = doc.CreateElement( "inactive-to" );
			inactiveTo.InnerText = this.Status.InactiveTo.ToShortDateString();
			status.AppendChild( inactiveTo );

			XmlElement termOn = doc.CreateElement( "terminated-on" );
			termOn.InnerText = this.Status.TerminatedOn.ToShortDateString();
			status.AppendChild( termOn );
			
			root.AppendChild( status );

			doc.AppendChild( root );
			xmlString  = doc.OuterXml;
			return xmlString;
		}

		public static void populateStateLists()
		{
			if( stateListShort.Count < 1)
			{
				stateListShort.Add("AL", 1);
				stateListLong.Add("Alabama", 1);
				stateListShort.Add("AK", 2);
				stateListLong.Add("Alaska", 2);
				stateListShort.Add("AZ", 3);
				stateListLong.Add("Arizona", 3);
				stateListShort.Add("AR", 4);
				stateListLong.Add("Arkansas", 4);
				stateListShort.Add("CA", 5);
				stateListLong.Add("California", 5);
				stateListShort.Add("CO", 6);
				stateListLong.Add("Colorado", 6);
				stateListShort.Add("CT", 7);
				stateListLong.Add("Connecticut", 7);
				stateListShort.Add("DE", 8);
				stateListLong.Add("Delaware", 8);
				stateListShort.Add("FL", 9);
				stateListLong.Add("Florida", 9);
				stateListShort.Add("GA", 10);
				stateListLong.Add("Georgia", 10);
				stateListShort.Add("HI", 11);
				stateListLong.Add("Hawaii", 11);
				stateListShort.Add("ID", 12);
				stateListLong.Add("Idaho", 12);
				stateListShort.Add("IL", 13);
				stateListLong.Add("Illinois", 13);
				stateListShort.Add("IN", 14);
				stateListLong.Add("Indiana", 14);
				stateListShort.Add("IA", 15);
				stateListLong.Add("Iowa", 15);
				stateListShort.Add("KS", 16);
				stateListLong.Add("Kansas", 16);
				stateListShort.Add("KY", 17);
				stateListLong.Add("Kentucky", 17);
				stateListShort.Add("LA", 18);
				stateListLong.Add("Louisiana", 18);
				stateListShort.Add("ME", 19);
				stateListLong.Add("Maine", 19);
				stateListShort.Add("MD", 20);
				stateListLong.Add("Maryland", 20);
				stateListShort.Add("MA", 21);
				stateListLong.Add("Massachusetts", 21);
				stateListShort.Add("MI", 22);
				stateListLong.Add("Michigan", 22);
				stateListShort.Add("MN", 23);
				stateListLong.Add("Minnesota", 23);
				stateListShort.Add("MS", 24);
				stateListLong.Add("Mississippi", 24);
				stateListShort.Add("MO", 25);
				stateListLong.Add("Missouri", 25);
				stateListShort.Add("MT", 26);
				stateListLong.Add("Montana", 26);
				stateListShort.Add("NE", 27);
				stateListLong.Add("Nebraska", 27);
				stateListShort.Add("NV", 28);
				stateListLong.Add("Nevada", 28);
				stateListShort.Add("NH", 29);
				stateListLong.Add("New Hampshire", 29);
				stateListShort.Add("NJ", 30);
				stateListLong.Add("New Jersey", 30);
				stateListShort.Add("NM", 31);
				stateListLong.Add("New Mexico", 31);
				stateListShort.Add("NY", 32);
				stateListLong.Add("New York", 32);
				stateListShort.Add("NC", 33);
				stateListLong.Add("North Carolina", 33);
				stateListShort.Add("ND", 34);
				stateListLong.Add("North Dakota", 34);
				stateListShort.Add("OH", 35);
				stateListLong.Add("Ohio", 35);
				stateListShort.Add("OK", 36);
				stateListLong.Add("Oklahoma", 36);
				stateListShort.Add("OR", 37);
				stateListLong.Add("Oregon", 37);
				stateListShort.Add("PA", 38);
				stateListLong.Add("Pennsylvania", 38);
				stateListShort.Add("RI", 39);
				stateListLong.Add("Rhode Island", 39);
				stateListShort.Add("SC", 40);
				stateListLong.Add("South Carolina", 40);
				stateListShort.Add("SD", 41);
				stateListLong.Add("South Dakota", 41);
				stateListShort.Add("TN", 42);
				stateListLong.Add("Tennessee", 42);
				stateListShort.Add("TX", 43);
				stateListLong.Add("Texas", 43);
				stateListShort.Add("UT", 44);
				stateListLong.Add("Utah", 44);
				stateListShort.Add("VT", 45);
				stateListLong.Add("Vermont", 45);
				stateListShort.Add("VA", 46);
				stateListLong.Add("Virginia", 46);
				stateListShort.Add("WA", 47);
				stateListLong.Add("Washington", 47);
				stateListShort.Add("WV",48);
				stateListLong.Add("West Virginia", 48);
				stateListShort.Add("WI", 49);
				stateListLong.Add("Wisconsin", 49);
				stateListShort.Add("WY", 50);
				stateListLong.Add("Wyoming", 50);
			}


		}
		public static int getStateId(String str)
		{	
			SysLog logger = new SysLog("Employee");
			logger.Debug("Inside getStateId():  "+str);
			if(stateListShort.ContainsKey(str))
				return Convert.ToInt32(stateListShort[str]);
			if(stateListLong.ContainsKey(str))
				return Convert.ToInt32(stateListLong[str]);
			return -1;
		}
	}
}

