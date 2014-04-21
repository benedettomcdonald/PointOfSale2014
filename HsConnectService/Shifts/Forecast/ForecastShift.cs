using System;

namespace HsConnect.Shifts.Forecast
{
	public class ForecastShift
	{
		public ForecastShift(){}

		private DateTime _inTime = new DateTime(0);
		private DateTime _outTime = new DateTime(0);
		private int _jobId = -1;

		public void SetInTime( int day, int month, int year )
		{
			_inTime = new DateTime( year, month, day );
		}

		public void SetOutTime( int day, int month, int year )
		{
			_outTime = new DateTime( year, month, day );
		}

		public DateTime InTime
		{
			get{ return _inTime; }
			set{ _inTime = value; }
		}

		public DateTime OutTime
		{
			get{ return _outTime; }
			set{ _outTime = value; }
		}

		public int JobId
		{
			get{ return _jobId; }
			set{ _jobId = value; }
		}

		public String ToPrintString()
		{
            return _inTime.ToString() + " - " + _outTime.ToString();
		}

	}
}
