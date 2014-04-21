using System;

namespace HsError
{
	/// <summary>
	/// </summary>

	public class HsError
	{
        private bool isFatal;
		private Exception error;

		public HsError()
		{
			error = new Exception();
			isFatal = false;
		}
		
		public HsError(Exception e, bool fatal)
		{
			error = e;
			isFatal = fatal;
		}

		public String Location
		{
			get{ return error.TargetSite.GetType().ToString(); }
		}

		public String ErrorString
		{
			get{ return error.ToString(); }
		}

		public String StackTrace
		{
			get{ return error.StackTrace; }
		}

		public bool IsFatal
		{
			get{ return isFatal; }
			set{ isFatal = value; }
		}

		public Exception Error
		{
			get{ return error; }
			set{ error = value; }
		}
	}
}
