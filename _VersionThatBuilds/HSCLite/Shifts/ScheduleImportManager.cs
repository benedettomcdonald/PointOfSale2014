using HsSharedObjects.Client;

using System;

namespace HSCLite.Shifts
{
	public class ScheduleImportManager
	{
		public ScheduleImportManager()
		{
			 
		}

		public ScheduleImport GetPosScheduleImport()
		{
			Type type = Type.GetType( "HSCLite.Shifts.Pos." + HSCLite.Run.ClientName + "ScheduleImport" );
			ScheduleImport schedImport = (ScheduleImport) Activator.CreateInstance ( type );
			return schedImport;
		}
	}
}
