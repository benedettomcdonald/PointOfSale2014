using HsSharedObjects.Client;
using HsSharedObjects.Client.Module;
using HsConnect.Main;
using HsConnect.Services;
using HsConnect.Services.Wss;
using HsConnect.GuestCounts;
using HsConnect.GuestCounts.PosList;

using System;

namespace HsConnect.Modules
{
	public class GuestCountModule : ModuleImpl
	{
		public GuestCountModule()
		{
			this.logger = new SysLog( this.GetType() );
		}

		private SysLog logger;

		public override bool Execute()
		{
			logger.Debug( "executing GuestCountModule" );
		//	bool run = Details.CustomModuleList.IsActive( ClientModule.GUEST_COUNT );
			//if( run )
		//	{
                GuestCountList gcList = GetPosList();
				gcList.SetDataConnection( this.Details.GetConnectionString() );
                gcList.Details = this.Details;
				gcList.DbLoad();
				logger.Debug( "loaded gc items" );
				CarinosCustomWss carinosService = new CarinosCustomWss();
                logger.Debug("XML:  " + gcList.GetXmlString());
                int cnt = carinosService.insertGcItems( this.Details.ClientId , gcList.GetXmlString() );
		//	}
			logger.Debug( "executed GuestCountModule" );
			return true;
		}

        public GuestCountList GetPosList()
        {
            logger.Debug("HsConnect.GuestCounts.PosList." + Details.PosName + "GuestCountList");
            Type type = Type.GetType("HsConnect.GuestCounts.PosList." + Details.PosName + "GuestCountList");
            GuestCountList gcList = (GuestCountList)Activator.CreateInstance(type);
            gcList.SetDataConnection(Details.GetConnectionString());
            return gcList;
        }
	}
}
