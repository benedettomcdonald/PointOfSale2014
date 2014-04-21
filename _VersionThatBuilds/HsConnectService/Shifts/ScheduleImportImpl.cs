using HsConnect.Data;
using HsConnect.Shifts;
using HsSharedObjects.Client;

using System;

namespace HsConnect.Shifts
{
	public abstract class ScheduleImportImpl : ScheduleImport
	{
		public ScheduleImportImpl(){}

		public HsDataConnection cnx;
		protected ShiftList shifts;
		protected bool autoSync = false;
		protected ClientDetails details;
		protected String xmlString;

		public ShiftList Shifts
		{
			get{ return this.shifts; }
			set{ this.shifts = value; }
		}

		public HsDataConnection Cnx
		{
			get{ return this.cnx; }
			set{ this.cnx = value; }
		}

		public ClientDetails Details
		{
			get{ return this.details; }
			set{ this.details = value; }
		}

		public abstract DateTime CurrentWeek{ get; set;}

		public bool AutoSync
		{
			get{ return this.autoSync; }
			set{ this.autoSync = value; }
		}

		public void SetDataConnection( String cnxString )
		{
			this.cnx = new HsDataConnection( cnxString );
		}

		public abstract void GetWeekDates();

		public abstract void Execute();

		public String XmlString
		{
			get{ return this.xmlString; }
			set{ this.xmlString = value; }
		}

	}
}
