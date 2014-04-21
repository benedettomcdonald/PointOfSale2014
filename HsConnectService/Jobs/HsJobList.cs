using HsConnect.Services;
using HsConnect.Services.Wss;
using HsConnect.Xml;
using HsConnect.Main;

using System;
using System.Xml;

namespace HsConnect.Jobs
{
	public class HsJobList : JobListImpl
	{
		public HsJobList( int id )
		{
			this.clientId = id;
			logger.Debug( "loading["+this.clientId+"] hs jobs" );
		}

		private int clientId;
		public override void DbInsert(){}
		public override void DbUpdate(){}

		public override void DbLoad()
		{
			logger.Debug( "loading..." );
			ClientJobsWss jobsService = new ClientJobsWss();
			String xmlString = jobsService.getClientJobs( clientId );
			HsXmlReader reader = new HsXmlReader();
			reader.LoadXml( xmlString );
			foreach( XmlNode cJob in reader.SelectNodes( "/client-job-list/job" ) )
			{
				Job job = new Job();
				job.HsId = reader.GetInt( cJob , "id" , HsXmlReader.ATTRIBUTE );
				job.ExtId = reader.GetInt( cJob , "external-ref" , HsXmlReader.ATTRIBUTE );
				job.Name = reader.GetString( cJob , "job-name" , HsXmlReader.NODE );
				job.RoleId = reader.GetInt( cJob , "default-schedule-id" , HsXmlReader.ATTRIBUTE );
				job.DefaultWage = reader.GetDouble( cJob , "pay-rate" , HsXmlReader.ATTRIBUTE );
				this.Add( job );
			}
			logger.Debug( "loaded "+this.JobList.Count+" jobs" );
		}

	}
}
