using HsSharedObjects.Client;

using System;

namespace HsConnect.SalesItems
{
	public class SalesItemsManager
	{
		public SalesItemsManager( ClientDetails details )
		{
			this.details = details;
		}

		private ClientDetails details;

		public SalesItemList GetPosSalesItemList()
		{
			Type type = Type.GetType( "HsConnect.SalesItems.PosList." + details.PosName + "SalesItemList" );
			SalesItemList salesItemList = (SalesItemList) Activator.CreateInstance ( type );
			salesItemList.SetDataConnection( details.GetConnectionString() );
			return salesItemList;
		}
	}
}
