using System;

namespace HsConnect.TimeCards.PosList.AppleOneImport
{
	public class AppleOneTimeCard
	{
		public AppleOneTimeCard()
		{
			
		}

		private int _employeeId = -1;
		private DateTime _clockIn = new DateTime(0);
		private DateTime _clockOut = new DateTime(0);
		private int _jobId = -1;
		private float _payRate = 0.0f;

		public int EmployeeId
		{
			set{ _employeeId = value; }
			get{ return _employeeId; }
		}

		public int JobId
		{
			set{ _jobId = value; }
			get{ return _jobId; }
		}

		public float PayRate
		{
			set{ _payRate = value; }
			get{ return _payRate; }
		}

		public DateTime ClockIn
		{
			set{ _clockIn = value; }
			get{ return _clockIn; }
		}

		public DateTime ClockOut
		{
			set{ _clockOut = value; }
			get{ return _clockOut; }
		}

	}
}
