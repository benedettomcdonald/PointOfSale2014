using HsSharedObjects.Client;

using System;

namespace HsConnect.Shifts
{
	public class ScheduleImportManager
	{
		public ScheduleImportManager( ClientDetails details )
		{
			this.details = details;
		}

		private ClientDetails details;

		public ScheduleImport GetPosScheduleImport()
		{
			Type type = Type.GetType( "HsConnect.Shifts.Pos." + details.PosName + "ScheduleImport" );
			ScheduleImport schedImport = (ScheduleImport) Activator.CreateInstance ( type );
			schedImport.SetDataConnection( details.GetConnectionString() );
			schedImport.Cnx.Dsn = details.Dsn;
			return schedImport;
		}
	}
}
