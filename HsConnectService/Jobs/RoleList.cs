using System;
using System.Collections;

namespace HsConnect.Jobs
{
	public class RoleList
	{
		public RoleList()
		{
			roleList = new ArrayList();
		}

		private ArrayList roleList;

		public void Load()
		{
			Role role1 = new Role();
			role1.Id = 1;
			role1.Name = "Server";

			Role role2 = new Role();
			role2.Id = 2;
			role2.Name = "Bar";

			roleList.Add( role1 );
			roleList.Add( role2 );
		}

		public System.Collections.IEnumerator GetEnumerator()
		{
			if( this.roleList == null ) return new ArrayList().GetEnumerator();
			return this.roleList.GetEnumerator();
		}
	}
}
