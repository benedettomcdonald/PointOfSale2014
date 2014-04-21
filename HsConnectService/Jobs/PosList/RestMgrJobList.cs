using HsConnect.Data;

using System;
using System.Data;
using Microsoft.Data.Odbc;

namespace HsConnect.Jobs.PosList
{
	public class RestMgrJobList : JobListImpl
	{
		public RestMgrJobList(){}

		private HsData data = new HsData();

		public override void DbInsert(){}
		public override void DbUpdate(){}

		/**
		 * Called by the wizard when originally syncing jobs.  Restaurant Manager
		 * stores its job data in JOBCLASS.DBF.  This method simply retrieves the
		 * relevant data from that file and creates Job objects from each row
		 * that is returned.  It copies the file into the hsTemp directory
		 * before accessing it.
		 */ 
		public override void DbLoad()
		{
			HsFile hsFile = new HsFile();
			hsFile.Copy( this.Cnx.Dsn, this.Cnx.Dsn + @"\hstemp", "JOBCLASS.DBF" );
			String empCnxStr = this.Cnx.ConnectionString + @"\hstemp";
			OdbcConnection newConnection = this.cnx.GetCustomOdbc( empCnxStr );
			try
			{
				DataSet dataSet = new DataSet();
				OdbcDataAdapter dataAdapter = new OdbcDataAdapter(
					"SELECT JOB_NO, JOB_DESC, JOB_SCHED FROM JOBCLASS" , newConnection );
				dataAdapter.Fill( dataSet , "JOBCLASS" );
				dataAdapter.Dispose();
				DataRowCollection rows = dataSet.Tables[0].Rows;
				foreach( DataRow row in rows )
				{
					try
					{
						Job job = new Job();
						job.ExtId = data.GetInt( row , "JOB_NO" );
						job.Name = data.GetString( row , "JOB_DESC" );
						job.DefaultWage = 1.0;
						job.OvtWage = 1.0;
						this.Add( job );
					}
					catch( Exception ex ) 
					{
						logger.Error( ex.ToString() );
					}
				}
			}
			catch( Exception ex ) 
			{
				logger.Error( ex.ToString() );
			}
			finally
			{
				newConnection.Close();
			}
		}
	}
}
