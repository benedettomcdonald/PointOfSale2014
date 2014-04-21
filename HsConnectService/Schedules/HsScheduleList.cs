using HsConnect.Services;
using HsConnect.Services.Wss;
using HsConnect.Main;
using HsConnect.Xml;

using System;
using System.Xml;
using System.Collections;

namespace HsConnect.Schedules
{
	public class HsScheduleList
	{
		public HsScheduleList( int clientId )
		{
			this.logger = new SysLog( this.GetType() );
			this.clientId = clientId;
		}

		private int clientId = -1;
		private ArrayList scheduleList;
		private SysLog logger;

		public ArrayList ScheduleList
		{
			get
			{
				if( this.scheduleList == null ) return new ArrayList(); 
				return this.scheduleList;
			}
			set{ this.scheduleList = value; }
		}

		public void Add( Schedule sched )
		{
			if( this.scheduleList == null ) this.scheduleList = new ArrayList(); 
			this.scheduleList.Add( sched );
		}

		public void DbLoad()
		{
			ClientJobsWss jobService = new ClientJobsWss();
			String xmlString = jobService.getClientSchedules( this.clientId );
			HsXmlReader reader = new HsXmlReader();
			reader.LoadXml( xmlString );
			foreach( XmlNode cSched in reader.SelectNodes( "/client-schedule-list/schedule" ) )
			{
				Schedule sched = new Schedule();
				sched.HsId = reader.GetInt( cSched , "id" , HsXmlReader.ATTRIBUTE );
				sched.Name = reader.GetString( cSched , "name" , HsXmlReader.NODE );
				logger.Debug( sched.Name );
				
				sched.Category = reader.GetString( cSched , "category" , HsXmlReader.NODE );
				logger.Debug( sched.Category );
				
				this.Add( sched );
			}
			logger.Debug( "loaded "+this.ScheduleList.Count+" schedules" );
		}
	}
}
