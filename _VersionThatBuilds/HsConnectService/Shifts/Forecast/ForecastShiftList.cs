using System;
using System.Collections;

namespace HsConnect.Shifts.Forecast
{
	public class ForecastShiftList
	{
		public ForecastShiftList( String desc, String sched, DateTime weekStart )
		{
			_scheduleName = sched;
			_weekStart = weekStart;
			_description = desc;
			_shifts = new ArrayList();
		}

		private ArrayList _shifts = null;
		private String _scheduleName = "";
		private String _description = "";
		private int _schedId = -1;
		private int _tempId = -1;
		private DateTime _weekStart = new DateTime(0);
		private bool _applied = false;

		public void Add( ForecastShift shift )
		{
			_shifts.Add( shift );
		}

		public int SchedId
		{
			get{ return this._schedId; }
			set{ this._schedId = value; }
		}

		public int TempId
		{
			get{ return this._tempId; }
			set{ this._tempId = value; }
		}

		public ArrayList Shifts
		{
			get{ return this._shifts; }
			set{ this._shifts = value; }
		}

		public bool Applied
		{
			get{ return this._applied; }
			set{ this._applied = value; }
		}

		public String ScheduleName
		{
			get{ return this._scheduleName; }
			set{ this._scheduleName = value; }
		}

		public String Description
		{
			get{ return this._description; }
			set{ this._description = value; }
		}

		public DateTime WeekStart
		{
			get{ return this._weekStart; }
			set{ this._weekStart = value; }
		}

	}
}
