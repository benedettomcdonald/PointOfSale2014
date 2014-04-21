using System;

namespace HsConnect.Client.CustomModule
{
	/// <summary>
	/// Summary description for ClientCustomModule.
	/// </summary>
	public class ClientCustomModule
	{
		public ClientCustomModule(){}

		private int moduleId = -1;
		private int syncType = 0;
		private int syncInterval = -1;
		private int syncHour = 0;
		private int syncMinute = 0;

		public static int CARINOS_GUEST_COUNT = 301;
		public static int CCF_DEVICE_MAPPING = 302;
		public static int MICROS_COV_CNT = 304;

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
	}
}
