using System;
using System.Collections;

namespace HsSharedObjects.Client.Preferences
{
	/// <summary>
	/// Summary description for ClientModuleList.
	/// </summary>
	[Serializable]
	public class PreferenceList : ArrayList
	{
		public PreferenceList() {}
		
		public Preference GetPreferenceById( int id )
		{
			Preference prefer = new Preference();
			foreach( Object obj in this )
			{
				Preference pref = (Preference) obj;
				if( pref.Id == id ) return pref;
			}
			return prefer;
		}

		public bool PrefExists( int id )
		{
			Preference prefer = new Preference();
			foreach( Object obj in this )
			{
				Preference pref = (Preference) obj;
				if( pref.Id == id ) return true;
			}
			return false;
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
				Preference pref = (Preference) obj;
				if( pref.Id == id ) return true;
			}
			return isActive;
		}
	}
}
