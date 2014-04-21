using System;
using HsSharedObjects.Client;

namespace HsConnect.HistoricalSales
{
    public class HistoricalSalesManager
    {
        public HistoricalSalesManager(ClientDetails details)
		{
			this.details = details;
		}

		private ClientDetails details;

		public HistoricalSalesItemList GetPosHistSalesItemList()
		{
			Type type = Type.GetType( "HsConnect.HistoricalSales.PosList." + details.PosName + "HistSalesItems" );
            HistoricalSalesItemList histSalesItemList = (HistoricalSalesItemList) Activator.CreateInstance(type);
            histSalesItemList.SetDataConnection(details.GetConnectionString());
            return histSalesItemList;
		}
    }
}
