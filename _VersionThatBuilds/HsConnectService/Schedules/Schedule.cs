using System;

namespace HsConnect.Schedules
{
	public class Schedule
	{
		public Schedule(){}

		private String name = "";
		private String category = "";
		private int hsId = -1;

		public String Name
		{
			get{ return this.name; }
			set{ this.name = value; }
		}

		public String Category
		{
			get{ return this.category; }
			set{ this.category = value; }
		}

		public int HsId
		{
			get{ return this.hsId; }
			set{ this.hsId = value; }
		}

	}
}
