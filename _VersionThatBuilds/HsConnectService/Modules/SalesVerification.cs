using HsSharedObjects.Client;
using HsSharedObjects.Client.Module;
using HsConnect.Main;
using HsConnect.Xml;
using HsConnect.Services;
using HsConnect.Services.Wss;
using HsConnect.SalesItems;

using System;
using System.Xml;

namespace HsConnect.Modules
{
	public class SalesVerification : ModuleImpl
	{
		public SalesVerification()
		{
			this.logger = new SysLog( this.GetType() );
		}

		private SysLog logger;

		public override bool Execute()
		{
			if( Details.ModuleList.IsActive( ClientModule.SALES_VERIFICATION ) )
			{
				// retrieve total sales from previous DOB
				XmlDocument doc = new XmlDocument();
				XmlElement root = doc.CreateElement( "date-range" );
				XmlElement start = doc.CreateElement( "start" );
				start.SetAttribute( "day" , DateTime.Now.AddDays(-1).Day.ToString() );
				start.SetAttribute( "month" , DateTime.Now.AddDays(-1).Month.ToString() );
				start.SetAttribute( "year" , DateTime.Now.AddDays(-1).Year.ToString() );
				root.AppendChild(start);
				XmlElement end = doc.CreateElement( "end" );
				end.SetAttribute( "day" , DateTime.Now.Day.ToString() );
				end.SetAttribute( "month" , DateTime.Now.Month.ToString() );
				end.SetAttribute( "year" , DateTime.Now.Year.ToString() );
				root.AppendChild(end);
				doc.AppendChild(root);
				ClientSalesWss salesService = new ClientSalesWss();
				float hsTtl = salesService.getSalesTotal( this.Details.ClientId , doc.OuterXml );
				logger.Debug( "Total: " + hsTtl.ToString() );
				
				// compare sales total
				logger.Debug( "executing NetSalesModule" );
				SalesItemsManager salesManager = new SalesItemsManager( this.Details );
				SalesItemList salesList = salesManager.GetPosSalesItemList();
				salesList.Details = this.Details;

				DateTime tempStart = DateTime.Now.AddDays( -1 );
				DateTime tempEnd = DateTime.Now;

				salesList.StartDate = new DateTime( tempStart.Year , tempStart.Month , tempStart.Day );
				salesList.EndDate = new DateTime( tempEnd.Year , tempEnd.Month , tempEnd.Day );
				float posTtl = salesList.GetSalesTotal();
				
				// if the total differs, re-sync the days' sales items
				if( hsTtl != posTtl )
				{
					salesList.AutoSync = true;
					salesList.DbLoad();
					salesService.compareSalesItems( Details.ClientId , salesList.GetXmlString() );
				}

			}
			logger.Debug( "executed SalesVerification" );
			return true;
		}
	}
}
