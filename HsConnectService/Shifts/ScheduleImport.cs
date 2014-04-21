using HsConnect.Shifts;
using HsConnect.Data;
using HsSharedObjects.Client;

using System;

namespace HsConnect.Shifts
{
	public interface ScheduleImport
	{
		HsDataConnection Cnx{ get;set; }
		ShiftList Shifts{ get;set; }
		bool AutoSync{ get;set; }
		ClientDetails Details{ get;set; }
		void Execute();
		void SetDataConnection( String str );

		// Posi only
		DateTime CurrentWeek{ get;set; }
		void GetWeekDates();

		//DD only
		String XmlString{ get;set; }
	}
}
