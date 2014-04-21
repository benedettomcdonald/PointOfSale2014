using System;

namespace HsSharedObjects
{
	/// <summary>
	/// Summary description for SharedEmployee.
	/// </summary>
	[Serializable]
	public class SharedEmployee
	{
		public SharedEmployee()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		private int posId;
		private String first;
		private String last;
		private String userName;
		private DateTime birthDate;
		private int status;
		private int type;
		private bool isNew;

		public static int POS = 1;
		public static int HS = 2;
		public static int SYNC = 3;

		public int PosId
		{
			get{ return posId; }
			set{ posId = value; }
		}

		public String FirstName
		{
			get{ return first; }
			set{ first = value; }
		}

		public String LastName
		{
			get{ return last; }
			set{ last = value; }
		}

		public String HsUserName
		{
			get{ return userName; }
			set{ userName = value; }
		}

		public DateTime BirthDate
		{
			get{ return birthDate; }
			set{ birthDate = value; }
		}

		public int Status
		{
			get{ return status; }
			set{ status = value; }
		}
		public int Type
		{
			get{ return type; }
			set{ type = value; }
		}

		public bool IsNew
		{
			get{ return isNew; }
			set{ isNew = value; }
		}
	}
}
