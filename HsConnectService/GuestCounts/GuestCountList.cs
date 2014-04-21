using HsConnect.Data;

using System;
using System.Collections;
using HsSharedObjects.Client;

namespace HsConnect.GuestCounts
{
	public interface GuestCountList
	{
		void DbLoad();
		void DbUpdate();
		void DbInsert();
		void Add( GuestCountItem item );
		void SetDataConnection( String str );
		String GetXmlString();
		IEnumerator GetEnumerator();
		DateTime EndDate{ get;set; }
        ClientDetails Details { get; set; }

	}
}
