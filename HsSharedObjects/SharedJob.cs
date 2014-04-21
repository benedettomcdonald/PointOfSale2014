using System;

namespace HsSharedObjects
{
	/// <summary>
	/// Summary description for SharedJob.
	/// </summary>
	[Serializable]
	public class SharedJob
	{
		public SharedJob()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		private String name;
		private int extId;
		private String extIdString;
		private int roleId;
		private int type;

		public static int POS = 1;
		public static int HS = 2;
		public static int SYNC = 3;

		public String Name 
		{
			get{return name;}
			set{ name = value;}
		}

		public String ExtIdString 
		{
			get{return extIdString;}
			set{ extIdString = value;}
		}
		public int ExtId
		{
			get{return extId;}
			set{ extId = value;}
		}
		public int RoleId
		{
			get{ return roleId; }
			set{ roleId = value; }
		}
		public int Type
		{
			get{return type;}
			set{ type = value;}
		}
	}
}
