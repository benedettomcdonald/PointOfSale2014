using System;
using System.Collections;

namespace HsConnect.Client.Module
{
	/// <summary>
	/// Summary description for ClientModuleList.
	/// </summary>
	public class ClientModuleList : ArrayList
	{
		public ClientModuleList() {}
		
		public ClientModule GetModuleById( int id )
		{
			ClientModule module = new ClientModule();
			foreach( Object obj in this )
			{
				ClientModule mod = (ClientModule) obj;
				if( mod.ModuleId == id ) return mod;
			}
			return module;
		}

		/* This method will return the sync interval, in minutes, of 
		 * the module with the id passed, or -1 if the module was not
		 * in the list.
		 * */
		public bool IsActive( int id )
		{
			bool isActive = false;
			foreach( Object obj in this )
			{
				ClientModule mod = (ClientModule) obj;
				if( mod.ModuleId == id ) return true;
			}
			return isActive;
		}
	}
}
