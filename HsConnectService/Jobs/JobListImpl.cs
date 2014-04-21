using HsConnect.Main;
using HsConnect.Data;
using HsSharedObjects.Client;

using System;
using System.Xml;
using System.Collections;

namespace HsConnect.Jobs
{
	public abstract class JobListImpl : JobList
	{
		public JobListImpl()
		{
			logger = new SysLog( this.GetType() );
		}

		private Hashtable fastAccessByExtID = new Hashtable();
		private Hashtable fastAccessByName = new Hashtable();
		
		protected ArrayList jobList;
		protected SysLog logger;
		protected bool useAlt;
		public HsDataConnection cnx;
        private ClientDetails details;

		public abstract void DbLoad();
		public abstract void DbInsert();
		public abstract void DbUpdate();

		public ArrayList JobList
		{
			get
			{
				if( jobList == null ) return new ArrayList();
				return jobList;
			}
			set{}
		}

        public ClientDetails Details  
        {  
          get { return details; }  
          set { this.details = value; }  
        }  
		public HsDataConnection Cnx
		{
			get{ return this.cnx; }
			set{ this.cnx = value; }
		}

		public bool UseAlt
		{
			get{ return this.useAlt; }
			set{ this.useAlt = value; }
		}

		public void Add(Job job)
		{
			if( this.jobList == null ) this.jobList = new ArrayList();
			if( !fastAccessByExtID.Contains( job.ExtId ) ) fastAccessByExtID.Add( job.ExtId , job );
			if( !fastAccessByName.Contains( job.Name ) ) fastAccessByName.Add( job.Name , job );
			this.jobList.Add( job );
		}

		public void SetDataConnection( String cnxString )
		{
			this.cnx = new HsDataConnection( cnxString );
		}

		public String GetXmlString()
		{
			if( this.jobList == null ) this.jobList = new ArrayList();
			String xmlString = "";
			XmlDocument doc = new XmlDocument();
			XmlNode root = doc.CreateElement( "hsconnect-jobs" );

			foreach( Job job in this.jobList )
			{
				XmlElement jobEle = doc.CreateElement( "job" );
				jobEle.SetAttribute( "hs-id" , job.HsId.ToString() );
				jobEle.SetAttribute( "ext-id" , job.ExtId.ToString() );
				jobEle.SetAttribute( "schedule-id" , job.RoleId.ToString() );
				jobEle.SetAttribute( "default-wage" , job.DefaultWage.ToString() );
				jobEle.SetAttribute( "ovt-wage" , job.OvtWage.ToString() );
				jobEle.InnerText = job.Name;
				root.AppendChild( jobEle );
			}
			doc.AppendChild( root );
			xmlString  = doc.OuterXml;
			return xmlString;
		}

		public System.Collections.IEnumerator GetEnumerator()
		{
			if( this.jobList == null ) return new ArrayList().GetEnumerator();
			return this.jobList.GetEnumerator();
		}

		public Job GetJobByExtId( int id )
		{
			Job outJob = null;
			if( fastAccessByExtID.Contains( id ) ) return (Job) fastAccessByExtID[ id ];
			return outJob;
		}

		public Job GetJobByName( String name )
		{
			Job outJob = null;
			if( fastAccessByName.Contains( name ) ) return (Job) fastAccessByName[ name ];
			return outJob;
		}

		public Job GetJobByHsId( int id )
		{
			Job outJob = null;
			foreach( Job job in this.jobList )
			{
				if( job.HsId == id ) return job;
			}
			return outJob;
		}
	}
}
