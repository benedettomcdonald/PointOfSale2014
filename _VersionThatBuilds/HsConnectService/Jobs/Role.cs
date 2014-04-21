using System;

namespace HsConnect.Jobs
{
	public class Role
	{
		public Role() {}

		private int id = -1;
		private String name = "";

		public int Id
		{
			get{ return this.id; }
			set{ this.id = value; }
		}

		public String Name
		{
			get{ return this.name; }
			set{ this.name = value; }
		}
	}
}
