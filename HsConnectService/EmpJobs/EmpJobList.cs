using HsConnect.Jobs;
using HsConnect.Data;
using HsSharedObjects.Client;
using System;
using System.Collections;
using System.Xml;

namespace HsConnect.EmpJobs
{
	public interface EmpJobList
	{
		void DbLoad();
		void DbUpdate();
		void DbInsert();

		void Add( EmpJob job );
		void SetDataConnection( String str );
		void SetHsJobList( JobList hsJobs );
		ClientDetails Details{ get;set; }
		HsDataConnection Cnx{ get;set; }
		String Dsn{ get;set; }
		String GetXmlString();
		XmlElement GetPosiXmlNode( XmlDocument doc );
		IEnumerator GetEnumerator();
        bool JobExistsForEmp(int empPosId);
	}
}
