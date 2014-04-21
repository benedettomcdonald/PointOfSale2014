using HsSharedObjects.Client;

using System;

namespace HsConnect.TimeCards
{
	public class TimeCardManager
	{
		public TimeCardManager( ClientDetails details )
		{
			this.details = details;
		}

		private ClientDetails details;

		public TimeCardList GetPosTimeCardList()
		{
			Type type = Type.GetType( "HsConnect.TimeCards.PosList." + details.PosName + "TimeCardList" );
			TimeCardList timeCardList = (TimeCardList) Activator.CreateInstance ( type );
			timeCardList.SetDataConnection( details.GetConnectionString() );
			return timeCardList;
		}
	}
}
