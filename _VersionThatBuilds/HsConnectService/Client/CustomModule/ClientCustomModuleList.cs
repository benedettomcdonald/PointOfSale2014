using System;
using System.Collections;

namespace HsConnect.Client.CustomModule
{
	/// <summary>
	/// Summary description for ClientCustomModuleList.
	/// </summary>
	public class ClientCustomModuleList : ArrayList
	{
		public ClientCustomModuleList(){}
		
		public ClientCustomModule Get( int i )
		{
			return (ClientCustomModule) this[i];
		}

		public ClientCustomModule GetModuleById( int id )
		{
			ClientCustomModule module = new ClientCustomModule();
			foreach( Object obj in this )
			{
				ClientCustomModule mod = (ClientCustomModule) obj;
				if( mod.ModuleId == id ) return mod;
			}
			return module;
		}

		/* This method will return the sync interval, in minutes, of 
		 * the module with the id passed, or -1 if the module was not
		 * in the list.
		 * */
		public int GetInterval( int id )
		{
			int interval = -1;
			foreach( Object obj in this )
			{
				ClientCustomModule mod = (ClientCustomModule) obj;
				if( mod.ModuleId == id ) return mod.SyncInterval;
			}
			return interval;
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
				ClientCustomModule mod = (ClientCustomModule) obj;
				if( mod.ModuleId == id ) return true;
			}
			return isActive;
		}

	}
}
