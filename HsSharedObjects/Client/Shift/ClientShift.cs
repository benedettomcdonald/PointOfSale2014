using System;

namespace HsSharedObjects.Client.Shift
{
	/// <summary>
	/**Class:  ClientShift
	 * this class is an object representing one client shift(i.e. lunch, dinner, etc)
	 * Currently all it holds is an ID, start and end times and a string label
	 */
	/// </summary>
	[Serializable]
	public class ClientShift
	{
		public ClientShift(){
			startTime = new TimeSpan();
			endTime = new TimeSpan();
			label = "";
		}

		private int id = -1;
		private TimeSpan startTime;
		private TimeSpan endTime;
		private String label;


		public int Id
		{
			get {return this.id;}
			set {this.id = value;}
		}
		public TimeSpan StartTime
		{
			get{ return this.startTime; }
			set{ this.startTime = value; }
		}
		public TimeSpan EndTime
		{
			get{ return this.endTime; }
			set{ this.endTime = value; }
		}
		public String Label
		{
			get{ return this.label; }
			set{ this.label = value; }
		}

	}
}
