using HsSharedObjects.Client.Module;
using HsSharedObjects.Client.CustomModule;
using HsSharedObjects.Client.Field;
using HsSharedObjects.Client.Shift;
using HsSharedObjects.Client.Preferences;

using System;
using System.Runtime.Serialization;
using System.Collections;

namespace HsSharedObjects.Client
{

	[Serializable]
	public class ClientDetails
	{
		public ClientDetails(){}

		private int clientId;
		private int transferRule;
		private int status;
		private int useWizard;
		private int overtimeRule;
		private int workWeekStart = 4;
		private String posName = "";
		private String dsn = "";
		private String dbUser = "";
		private String dbPassword = "";
		private ClientModuleList moduleList;
		private ClientFieldList fieldList;
		private ClientShiftList shiftList;//MFisher 6/26/08:  added to hold list of clientShifts
		private ClientCustomModuleList customModuleList;
		private PreferenceList preferences;
		private ArrayList commands;
		private ArrayList files;
        private int clientExtRef;
        private int clientConceptExtRef;
        private int clientCompanyExtRef;


		public static int POS_TO_HS = 1;
		public static int HS_TO_POS = 2;
		public static int SYNC = 3;

		public static int ACTIVE = 1;
		public static int INACTIVE = 0;

		public static int WIZARD_ON = 1;
		public static int WIZARD_OFF = 0;

		public int ClientId
		{
			get { return this.clientId; }
			set { this.clientId = value; }
		}

		public int TransferRule
		{
			get { return this.transferRule; }
			set { this.transferRule = value; }
		}

		public int Status
		{
			get { return this.status; }
			set { this.status = value; }
		}

		public int UseWizard
		{
			get { return this.useWizard; }
			set { this.useWizard = value; }
		}

		public String PosName
		{
			get { return this.posName; }
			set { this.posName = value; }
		}

		public String Dsn
		{
			get { return this.dsn; }
			set { this.dsn = value; }
		}

		public String DbUser
		{
			get { return this.dbUser; }
			set { this.dbUser = value; }
		}

		public String DbPassword
		{
			get { return this.dbPassword; }
			set { this.dbPassword = value; }
		}

		public ClientModuleList ModuleList
		{
			get { return this.moduleList; }
			set { this.moduleList = value; }
		}

		public ClientFieldList FieldList
		{
			get { return this.fieldList; }
			set { this.fieldList = value; }
		}

		public ClientShiftList ShiftList
		{
			get {return this.shiftList; }
			set {this.shiftList = value; }
		}
		public ClientCustomModuleList CustomModuleList
		{
			get { return this.customModuleList; }
			set { this.customModuleList = value; }
		}

		public PreferenceList Preferences
		{
			get { return this.preferences; }
			set { this.preferences = value; }
		}

		public int OvertimeRule
		{
			get { return this.overtimeRule; }
			set { this.overtimeRule = value; }
		}

        public int ClientExtRef
        {
            get { return this.clientExtRef; }
            set { this.clientExtRef = value; }
        }

        public int ClientConceptExtRef
        {
            get { return this.clientConceptExtRef; }
            set { this.clientConceptExtRef = value; }
        }

        public int ClientCompanyExtRef
        {
            get { return this.clientCompanyExtRef; }
            set { this.clientCompanyExtRef = value; }
        }

		public int WorkWeekStart
		{
			get { return this.workWeekStart; }
			set { this.workWeekStart = value; }
		}

		public String WeekStartDay
		{
			get 
			{  
				switch( this.workWeekStart )
				{
					case 1:
						return "Sunday";
					case 2:
						return "Monday";
					case 3:
						return "Tuesday";
					case 4:
						return "Wednesday";
					case 5:
						return "Thursday";
					case 6:
						return "Friday";
					case 7:
						return "Saturday";
				}
				return "";
			}
			set {  }
        }

        public DayOfWeek DayOfWeekStart
        {
            get
            {
                switch (this.workWeekStart)
                {
                    case 1:
                        return DayOfWeek.Sunday;
                    case 2:
                        return DayOfWeek.Monday;
                    case 3:
                        return DayOfWeek.Tuesday;
                    case 4:
                        return DayOfWeek.Wednesday;
                    case 5:
                        return DayOfWeek.Thursday;
                    case 6:
                        return DayOfWeek.Friday;
                    case 7:
                        return DayOfWeek.Saturday;
                }
                return DayOfWeek.Sunday;
            }
            set { }
        }

		public ArrayList Commands
		{
			get{  return commands; }
			set{ commands = value; }
		}

		public ArrayList Files
		{
			get{  return files; }
			set{ files = value; }
		}


		public String GetConnectionString()
		{
			String dummy = "";
			switch( this.posName )
			{
                case "EverServ":
                    return @"Driver={SQL Server};Server="+this.dsn+@"\ISIVA;Database=Enterprise;Trusted_Connection=yes;";
				case "Micros":
					return	@"DSN="+this.dsn+";PWD="+this.dbPassword+";UID="+this.dbUser+"";
				case "Aloha":
					return @"Driver={Microsoft dBASE Driver (*.dbf)};DriverID=277;Dbq=" + this.dsn + "";
				case "Posi":
					return @"Driver={Microsoft dBASE Driver (*.dbf)};DriverID=277;";
				case "TCPlus":
					return @"Driver={Microsoft dBASE Driver (*.dbf)};DriverID=277;Dbq=" + this.dsn + "";
				case "RestMgr":
					return @"Driver={Microsoft dBASE Driver (*.dbf)};DriverID=277;Dbq=" + this.dsn + "";
                case "Squirrel":
			        return @"Server=localhost; Database=" + dsn + "; UID=" + dbUser + "; Password=" + dbPassword;
                case "MicroSale":
			        string datasource = dsn;
                    if (!datasource.EndsWith("\\"))
                        datasource += "\\";
			        return @"Provider=Microsoft.Jet.OLEDB.4.0; Data Source=" + dsn + "[filename]; User Id=" + dbUser + "; Password=" + dbPassword;
			}
			return dummy;
		}

	}
}
