using System;
using System.Collections;

namespace HsConnect.Client.Field
{
	/// <summary>
	/// Summary description for ClientFieldList.
	/// </summary>
	public class ClientFieldList : ArrayList
	{
		public ClientFieldList(){}

		public bool Allows( int id )
		{
			bool allow = false;
			foreach( ClientField f in this )
			{
				if( id == f.Id ) return true;
			}
			return allow;
		}


	}
}
