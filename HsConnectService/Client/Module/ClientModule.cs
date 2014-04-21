using System;

namespace HsConnect.Client.Module
{
	/// <summary>
	/// Summary description for Module.
	/// </summary>
	public class ClientModule
	{
		public ClientModule(){}

		private int moduleId = -1;
		private int syncType = 0;
		private int syncInterval = -1;
		private int syncHour = 0;
		private int syncMinute = 0;
		private int force = 0;

		public static int NET_SALES = 201;
		public static int LABOR_ITEMS = 202;
		public static int EMPLOYEE_DATA = 203;
		public static int SALES_VERIFICATION = 204;
		public static int PERIOD_LABOR = 205;
		public static int REG_CLK_IN = 206;
		public static int APPROVAL_ALERT = 207;
		public static int SCHED_DBF = 208;
        public static int HISTORICAL_SALES = 230;

		public static int FIXED_SYNC = 1;
		public static int TIMED_SYNC = 0;

		public int ModuleId
		{
			get { return this.moduleId; }
			set { this.moduleId = value ; }
		}

		public int SyncType
		{
			get { return this.syncType; }
			set { this.syncType = value ; }
		}

		public int SyncInterval
		{
			get { return this.syncInterval; }
			set { this.syncInterval = value ; }
		}

		public int SyncHour
		{
			get { return this.syncHour; }
			set { this.syncHour = value ; }
		}

		public int SyncMinute
		{
			get { return this.syncMinute; }
			set { this.syncMinute = value ; }
		}

		public int Force
		{
			get { return this.force; }
			set { this.force = value; }
		}


	}
}
