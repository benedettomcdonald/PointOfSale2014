using System;

namespace HsConnect.Shifts.Forecast
{
	public class ForecastDataRow
	{
		public ForecastDataRow(){}

		private int _jobCode = -1;
		private String _groupName = "";
		private String _tempName = "";
		private DateTime _weekStart = new DateTime(0);
		private bool _add = false;
		private int _schedId = -1;
		private int _tempId = -1;

		private ForecastShift _wedShift;
		private ForecastShift _thursShift;
		private ForecastShift _friShift;
		private ForecastShift _satShift;
		private ForecastShift _sunShift;
		private ForecastShift _monShift;
		private ForecastShift _tuesShift;

		public bool Add
		{
			get{ return _add; }
			set{ _add = value; }
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

		public int JobCode
		{
			get{ return _jobCode; }
			set{ _jobCode = value; }
		}

		public String GroupName
		{
			get{ return _groupName; }
			set{ _groupName = value; }
		}

		public String TempName
		{
			get{ return _tempName; }
			set{ _tempName = value; }
		}

		public DateTime WeekStart
		{
			get{ return _weekStart; }
			set{ _weekStart = value; }
		}

		public bool AddShift( ForecastShift shift )
		{
			switch( (int) shift.InTime.DayOfWeek )
			{
				case 0:
					if( _sunShift != null ) return false;
					_sunShift = shift;
					break;
				case 1:
					if( _monShift != null ) return false;
					_monShift = shift;
					break;
				case 2:
					if( _tuesShift != null ) return false;
					_tuesShift = shift;
					break;
				case 3:
					if( _wedShift != null ) return false;
					_wedShift = shift;
					break;
				case 4:
					if( _thursShift != null ) return false;
					_thursShift = shift;
					break;
				case 5:
					if( _friShift != null ) return false;
					_friShift = shift;
					break;
				case 6:
					if( _satShift != null ) return false;
					_satShift = shift;
					break;
			}
			return true;
		}


		public ForecastShift Wed
		{
			get{ return _wedShift; }
			set{ _wedShift = value; }
		}

		public ForecastShift Thurs
		{
			get{ return _thursShift; }
			set{ _thursShift = value; }
		}

		public ForecastShift Fri
		{
			get{ return _friShift; }
			set{ _friShift = value; }
		}

		public ForecastShift Sat
		{
			get{ return _satShift; }
			set{ _satShift = value; }
		}

		public ForecastShift Sun
		{
			get{ return _sunShift; }
			set{ _sunShift = value; }
		}

		public ForecastShift Mon
		{
			get{ return _monShift; }
			set{ _monShift = value; }
		}

		public ForecastShift Tues
		{
			get{ return _tuesShift; }
			set{ _tuesShift = value; }
		}

		public String toPrintString()
		{
			return (_wedShift != null ? "X" : " ") + " | " + (_thursShift != null ? "X" : " ") + " | " + (_friShift != null ? "X" : " ") + " | " +
				(_satShift != null ? "X" : " ") + " | " + (_sunShift != null ? "X" : " ") + " | " + (_monShift != null ? "X" : " ") + " | " +
				(_tuesShift != null ? "X" : " ");
		}

	}
}
