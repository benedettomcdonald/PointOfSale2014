using HsSharedObjects.Client;
using HsSharedObjects.Client.Preferences;

using System;
using System.Reflection;

namespace HsConnect.Jobs
{
	public class JobManager
	{
		public JobManager( ClientDetails details )
		{
			this.details = details;
		}

		private ClientDetails details;

		public JobList GetPosJobList()
		{
			Type type = Type.GetType( "HsConnect.Jobs.PosList." + details.PosName + "JobList" );
			JobList jobList = (JobList) Activator.CreateInstance ( type );
			jobList.SetDataConnection( details.GetConnectionString() );
			jobList.Cnx.Dsn = details.Dsn;
			jobList.UseAlt = details.Preferences.PrefExists( Preference.POSI_ALT_JOB );
			return jobList;
		}

		public JobList GetHsJobList()
		{
			JobList jobList = (JobList) new HsJobList( details.ClientId );
			return jobList;
		}
	}
}
