using HSCLite.Shifts;
using HSCLite.Data;
using HsSharedObjects.Client;

using System;

namespace HSCLite.Shifts
{
	public interface ScheduleImport
	{
		ShiftList Shifts{ get;set; }
		bool AutoSync{ get;set; }
		void Execute();

		// Posi only
		DateTime CurrentWeek{ get;set; }
		void GetWeekDates();
	}
}
