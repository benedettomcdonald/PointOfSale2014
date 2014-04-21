using System;

namespace HsSharedObjects
{
	/// <summary>
	/// Summary description for SharedSchedules.
	/// </summary>
	[Serializable]
	public class SharedSchedule
	{
		public SharedSchedule()
		{
			name = "";
			hsId= -1;
		}

		private String name;
		private int hsId;

		public String Name
		{
			get{ return name; }
			set{ name = value; }
		}

		public int HsId
		{
			get{ return hsId; }
			set{ hsId = value; }
		}
	}
}
