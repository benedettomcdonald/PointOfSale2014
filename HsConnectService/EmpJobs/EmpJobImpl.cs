using HsConnect.Main;
using HsConnect.Data;
using HsConnect.Jobs;
using HsSharedObjects.Client;

using System;
using System.Xml;
using System.Collections;

namespace HsConnect.EmpJobs
{
	public abstract class EmpJobImpl : EmpJobList
	{
		public EmpJobImpl()
		{
			logger = new SysLog( this.GetType() );
		}
		
		protected ArrayList empJobList;
		protected SysLog logger;
		protected String dsn;
		protected ClientDetails details;
		public HsDataConnection cnx;

		public abstract void DbLoad();
		public abstract void DbInsert();
		public abstract void DbUpdate();
		public abstract void SetHsJobList( JobList hsJobs );

		public void Add(EmpJob empJob)
		{
			if( empJobList == null ) empJobList = new ArrayList();
			empJobList.Add( empJob );
		}

		public void SetDataConnection( String cnxString )
		{
			this.cnx = new HsDataConnection( cnxString );
		}

		public HsDataConnection Cnx
		{
			get{ return this.cnx; }
			set{ this.cnx = value; }
		}
		
		public ClientDetails Details
		{
			get{ return this.details; }
			set{ this.details = value; }
		}
		
		public String Dsn
		{
			get{ return this.dsn; }
			set{ this.dsn = value; }
		}

		public String GetXmlString()
		{
			String xmlString = "";
			XmlDocument doc = new XmlDocument();
			XmlNode root = doc.CreateElement( "hsconnect-emp-jobs" );

			foreach( EmpJob job in this.empJobList )
			{
				XmlElement jobEle = doc.CreateElement( "emp-job" );
				jobEle.SetAttribute( "pos-id" , job.PosId.ToString() );
				jobEle.SetAttribute( "hs-id" , job.HsId.ToString() );
				jobEle.SetAttribute( "ext-job-code" , job.JobCode.ToString() );
				jobEle.SetAttribute( "hs-job-id" , job.HsJobId.ToString() );
				jobEle.SetAttribute( "reg-wage" , job.RegWage.ToString() );
				jobEle.SetAttribute( "ovt-wage" , job.OvtWage.ToString() );
				jobEle.SetAttribute( "is-primary" , job.Primary.ToString() );
				root.AppendChild( jobEle );
			}
			doc.AppendChild( root );
			xmlString  = doc.OuterXml;
			return xmlString;
		}

		public XmlElement GetPosiXmlNode( XmlDocument doc )
		{
			XmlElement root = doc.CreateElement( "Jobs" );

			foreach( EmpJob job in this.empJobList )
			{
				XmlElement jobEle = doc.CreateElement( "JobRecord" );

				XmlElement number = doc.CreateElement( "JobNumber" );
				number.InnerText = job.JobCode.ToString();
				jobEle.AppendChild( number );
				
				XmlElement name = doc.CreateElement( "JobName" );
				name.InnerText = job.JobName;
				jobEle.AppendChild( name );

				XmlElement rate = doc.CreateElement( "Rate" );
				rate.InnerText = job.RegWage.ToString();
				jobEle.AppendChild( rate );

				root.AppendChild( jobEle );
			}
			return root;
		}

		public System.Collections.IEnumerator GetEnumerator()
		{
			if( this.empJobList == null ) return new ArrayList().GetEnumerator();
			return this.empJobList.GetEnumerator();
		}

        public bool JobExistsForEmp(int empPosId)
        {
            foreach (Object o in empJobList)
            {
                EmpJob j = (EmpJob)o;
                if (j.PosId == empPosId)
                {
                    return true;
                }
            }
            return false;
        }
	}
}

