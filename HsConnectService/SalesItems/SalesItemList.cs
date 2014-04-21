using HsConnect.Data;
using HsSharedObjects.Client;

using System;
using System.Collections;

namespace HsConnect.SalesItems
{
	public interface SalesItemList
	{
		float GetSalesTotal();
		void DbLoad();
		void DbUpdate();
		void DbInsert();
		void Add( SalesItem item );
		void SetDataConnection( String str );
		String GetXmlString();
		IEnumerator GetEnumerator();
		int Count{ get;set; }
		DateTime EndDate{ get;set; }
		DateTime StartDate{ get;set; }
		DateTime Dob{ get;set; }
		bool Updated{ get;set; }
		bool AutoSync{ get;set; }
		ClientDetails Details{ get;set; }
	}
}
