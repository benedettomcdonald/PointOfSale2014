using System;
using System.Collections;
using HsConnect.Data;

namespace HsConnect.Jobs
{
	public interface JobList
	{
		void DbLoad();
		void DbInsert();
		void DbUpdate();
		void Add( Job job );
		void SetDataConnection( String str );

		String GetXmlString();
		HsDataConnection Cnx{ get;set; }
		bool UseAlt{ get;set; }
		Job GetJobByExtId( int id );
		Job GetJobByHsId( int id );
		Job GetJobByName( String name );
		IEnumerator GetEnumerator();
	}
}
