using HsConnect.Data;
using HsSharedObjects.Client;

using System;
using System.Collections;

namespace HsConnect.TimeCards
{
	public interface TimeCardList
	{
		void DbLoad();
		void DbUpdate();
		void DbInsert();
		void SortByDate();
		int Add( TimeCard card );
		void SetDataConnection( String str );
		String GetXmlString();
		ArrayList GetDayCards();
		DateTime StartDate{ get;set; }
		DateTime EndDate{ get;set; }
		bool AutoSync{ get;set; }
		bool PeriodLabor{ get;set; }
		int DropCards{ get;set; }
		IEnumerator GetEnumerator();
		int Count{ get;set; }
		ClientDetails Details{ get;set; }
		ArrayList GetList();
	}
}
