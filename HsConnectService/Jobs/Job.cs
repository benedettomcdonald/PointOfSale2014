using System;

namespace HsConnect.Jobs
{
	/// <summary>
	/// Summary description for Job.
	/// </summary>
	public class Job
	{
		public Job() {}

		private int extId = -1;
		private int hsId = -1;
		private int roleId = -1;
		private String name = "";
		private String roleName = "";
		private double defaultWage = 0.0;
		private double ovtWage = 0.0;
		private bool updated = false;

		public bool Updated
		{
			get { return this.updated; }
			set { this.updated = value; }
		}
		
		public int ExtId
		{
			get { return this.extId; }
			set { this.extId = value; }
		}

		public int RoleId
		{
			get { return this.roleId; }
			set { this.roleId = value; }
		}

		public int HsId
		{
			get { return this.hsId; }
			set { this.hsId = value; }
		}

		public String Name
		{
			get { return this.name; }
			set { this.name = value; }
		}

		public String RoleName
		{
			get { return this.roleName; }
			set { this.roleName = value; }
		}

		public double DefaultWage
		{
			get { return this.defaultWage; }
			set { this.defaultWage = value; }
		}

		public double OvtWage
		{
			get { return this.ovtWage; }
			set { this.ovtWage = value; }
		}


	}
}
