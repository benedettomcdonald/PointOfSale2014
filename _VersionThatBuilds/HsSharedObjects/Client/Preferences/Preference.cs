using System;

namespace HsSharedObjects.Client.Preferences
{
	/// <summary>
	/// Summary description for ClientField.
	/// </summary>
	[Serializable]
	public class Preference
	{
		public Preference(){}

		private int id = -1;
		private String value1 = "";
		private String value2 = "";
		private String value3 = "";
		private String value4 = "";
		private String value5 = "";

		public const int POSI_ALT_JOB = 1001;
		public const int REMOVE_ALC_TAX = 1003;
		public const int IGNORE_DL_ID = 1004;
		public const int USE_SPECIFIC_TYPE = 1006;
		public const int CARINOS_EMP_FOOD = 1007;
		public const int UPDATE_TIMERS = 1008;
		public const int IMPORT_ADDRESSES = 1009;
		public const int USE_DL_EMP_ID = 1010;
		public const int ALT_POSI_SALES = 1011;
		public const int POSI_USE_REG_JOB = 1012;
		public const int MICROS_CUSTOM_SALES = 1013;
		public const int SCHED_IORT_TIMEFRAME = 1014;
		public const int MICROS9700_TERM_CODE = 1015;
		public const int PSLBR_WAIVERS = 1016;
		public const int BJS_COPY_EMPLOYEE_XML = 1017;  
		public const int SCHED_IMPORT_CUSTOM_LENGTH = 1018;  
		public const int A_GOLD_TRAINING_JOBS = 1019;  
		public const int APPLE_DOS_EMP_JOBS_TRAILING_ZERO = 1020;
		public const int RUI_FONDI_COVER_COUNT = 1021;  
		public const int MICROS_BY_RVC = 1023;
		public const int PREV_WEEK_TIMECARDS = 1025;
		public const int COMPARE_TIMECARD_BUSINESS_DATES = 1026;
		public const int MICROS_SHARED_EMP_HR_ID = 1027;
		public const int GROSS_SALES = 1028;
		public const int POSI_REGULAR_WEEK = 1029;
		public const int REPLACE_DL_ID = 1030;
		public const int NET_SALES = 1031;
		public const int POSI_LABOR_IGNORE_TYPE_8 = 1032;
		public const int MICROS_CALCULATE_OVERTIME = 1033;
		public const int ADJUST_SALES_DATE_RANGE = 1034;
		public const int ALOHA_COPY_SCHEDULE_FILE = 1035;
		public const int SalesTimeType = 1036;
		public static int SCHEDULE_IMPORT_INCLUDE_UNPOSTED_SHIFTS = 1037;
		public const int CustomGuestCountQuery = 1038;
        //1039 seems to be reserved for PUNCH_RECORD_EXCLUDE_JOB_NAME @ClientPreference.java
        public const int MICROS_TRUNC_HR_ID = 1040;
        public const int USE_ALTID_NOT_EXTID = 1041;
        public const int FILTER_ITEM_TYPE = 1024; // Aloha File Processing sales (replace sales.ini for server side) -- overloaded for Posi as well
        public const int ALOHA_USE_SSN_AS_HR_ID = 1043;
        public const int ENABLE_GOHIRE_EMPLOYEE_INSERTS = 1045;
        public const int FILE_XFER_WITH_COMPANY_EXTREF = 1047;
        public const int SYNC_ONLY_EMPS_WITH_JOBCODES = 1049;
        public const int POSI_CUSTOM_DRIVE_MAPPING = 1054;
        public const int EVERSERV_SYNC_NONACTIVE_EMPLOYEES = 1050;
        public const int DELETE_ALL_SHIFTS_POSI_SCHEDULES = 1059;
        public const int SQUIRREL_USE_BIG_JOB_ADJ = 1060;
        public const int LSF_EVERSERV_CUSTOM_NET_SALES = 1061; 
        public const int MICROS9700_OVERRIDE_WORK_DIR = 1062;
        public const int POSI_CUSTOM_DATE_FORMAT = 1063;

		public int Id
		{
			get {return this.id;}
			set {this.id = value;}
		}

		public String Val1
		{
			get { return this.value1; }
			set { this.value1 = ""; }
		}

		public String Val2
		{
			get { return this.value2; }
			set { this.value2= value; }
		}

		public String Val3
		{
			get { return this.value3; }
			set { this.value3 = value; }
		}

		public String Val4
		{
			get { return this.value4; }
			set { this.value4 = value; }
		}

		public String Val5
		{
			get { return this.value5; }
			set { this.value5 = value; }
		}


	}
}

