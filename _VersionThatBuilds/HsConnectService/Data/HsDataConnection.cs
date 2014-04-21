using System;

using Microsoft.Data.Odbc;

namespace HsConnect.Data
{
	/// <summary>
	/// Summary description for HsDataConnection.
	/// </summary>
	public class HsDataConnection
	{
		public HsDataConnection(  String cnxString  )
		{
			this.cnxString = cnxString;
		}

		private String cnxString = "";
		private String dsn = "";

		public String ConnectionString
		{
			get{ return this.cnxString; }
			set{ this.cnxString = value; }
		}

		public String Dsn
		{
			get{ return this.dsn; }
			set{ this.dsn = value; }
		}

		public OdbcConnection GetOdbc()
		{
			return new OdbcConnection( cnxString );
		}

		public OdbcConnection GetCustomOdbc( String str )
		{
			return new OdbcConnection( str );
		}
	}
}
