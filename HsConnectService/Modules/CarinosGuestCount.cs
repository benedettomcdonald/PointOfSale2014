using HsSharedObjects.Client;
using HsSharedObjects.Client.CustomModule;
using HsConnect.Main;
using HsConnect.Services;
using HsConnect.Services.Wss;
using HsConnect.GuestCounts;
using HsConnect.GuestCounts.PosList;

using System;

namespace HsConnect.Modules
{
	public class CarinosGuestCount : ModuleImpl
	{
		public CarinosGuestCount()
		{
			this.logger = new SysLog( this.GetType() );
		}

		private SysLog logger;

		public override bool Execute()
		{
			logger.Debug( "executing CarinosGuestCount" );
			bool run = Details.CustomModuleList.IsActive( ClientCustomModule.CARINOS_GUEST_COUNT );
			if( run )
			{
				GuestCountList gcList = (GuestCountList) new CarinosCustomList();
				if( this.Details.PosName.Equals( "Aloha" ) )
				{
					logger.Log("THIS IS ALOHA");
					gcList = (GuestCountList) new AlohaGuestCountList();
					((AlohaGuestCountList)gcList).Details = this.Details;
					logger.Debug( "loaded gc items: " + ((AlohaGuestCountList)gcList).Count);
				}
				gcList.SetDataConnection( this.Details.GetConnectionString() );
				gcList.DbLoad();
				logger.Debug( "loaded gc items" );
				CarinosCustomWss carinosService = new CarinosCustomWss();
				if( this.Details.PosName.Equals( "Aloha" )  && ((AlohaGuestCountList)gcList).Count <= 0)
				{
						logger.Debug( "No items to add, returning ");
						return true;
				}
				int cnt = carinosService.insertGcItems( this.Details.ClientId , gcList.GetXmlString() );
			}
			logger.Debug( "executed CarinosGuestCount" );
			return true;
		}
	}
}
