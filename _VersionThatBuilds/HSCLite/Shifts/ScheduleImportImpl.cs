using HSCLite.Data;
using HSCLite.Shifts;
using HsSharedObjects.Client;

using System;

namespace HSCLite.Shifts
{
	public abstract class ScheduleImportImpl : ScheduleImport
	{
		public ScheduleImportImpl(){}

		protected ShiftList shifts;
		protected bool autoSync = false;

		public ShiftList Shifts
		{
			get{ return this.shifts; }
			set{ this.shifts = value; }
		}

		public abstract DateTime CurrentWeek{ get; set;}

		public bool AutoSync
		{
			get{ return this.autoSync; }
			set{ this.autoSync = value; }
		}

		public abstract void GetWeekDates();

		public abstract void Execute();

	}
}
