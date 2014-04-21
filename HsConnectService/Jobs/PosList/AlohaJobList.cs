using HsConnect.Data;

using System;
using System.Data;
using Microsoft.Data.Odbc;

namespace HsConnect.Jobs.PosList
{
	public class AlohaJobList : JobListImpl
	{
		public AlohaJobList(){}

		private HsData data = new HsData();

		public override void DbInsert(){}
		public override void DbUpdate(){}

		public override void DbLoad()
		{
			HsFile hsFile = new HsFile();
			hsFile.Copy( this.Cnx.Dsn + @"\Newdata", this.Cnx.Dsn + @"\hstemp", "JOB.DBF" );
			String empCnxStr = this.cnx.ConnectionString + @"\hstemp";
			OdbcConnection newConnection = this.cnx.GetCustomOdbc( empCnxStr );
			try
			{
				DataSet dataSet = new DataSet();
				OdbcDataAdapter dataAdapter = new OdbcDataAdapter(
					"SELECT ID , SHORTNAME FROM JOB" , newConnection );
				dataAdapter.Fill( dataSet , "JOB" );
				dataAdapter.Dispose();
				DataRowCollection rows = dataSet.Tables[0].Rows;
				foreach( DataRow row in rows )
				{
					try
					{
						Job job = new Job();
						job.ExtId = data.GetInt( row , "ID" );
						job.Name = data.GetString( row , "SHORTNAME" );
						job.DefaultWage = 1.0;
						job.OvtWage = 1.0;
						this.Add( job );
					}
					catch( Exception ex ) 
					{
						logger.Error( ex.ToString() );
					//	Main.Run.errorList.Add(ex);
					}
				}
			}
			catch( Exception ex ) 
			{
				logger.Error( ex.ToString() );
				Main.Run.errorList.Add(ex);
			}
			finally
			{
				newConnection.Close();
			}
		}
	}
}
