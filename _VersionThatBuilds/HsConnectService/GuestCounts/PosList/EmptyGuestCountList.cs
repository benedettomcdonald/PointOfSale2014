using System;

namespace HsConnect.GuestCounts
{
	/// <summary>
	/// Summary description for EmptyguestCountList.
	/// </summary>
	public class EmptyGuestCountList : GuestCountListImpl
	{
		public EmptyGuestCountList()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public override void DbLoad(){}
		public override void DbInsert(){}
		public override void DbUpdate(){}
	}
}
