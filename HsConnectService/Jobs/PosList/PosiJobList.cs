using HsConnect.Data;
using HsSharedObjects.Client.Preferences;

using System;
using System.Data;
using Microsoft.Data.Odbc;

namespace HsConnect.Jobs.PosList
{
	public class PosiJobList : JobListImpl
	{
		public PosiJobList(){}

		private HsData data = new HsData();
		private static String drive = Data.PosiControl.Drive;

		public override void DbInsert(){}
		public override void DbUpdate(){}

		public override void DbLoad()
		{
			try
			{
				DataTableBuilder builder = new DataTableBuilder();
				//DataTable dt = builder.GetTableFromDBF( @"C:\SC", @"C:\", "JOBLIST" );
				DataTable dt = builder.GetTableFromDBF(drive + @":\ALTDBF", @"C:\", "JOBLIST" );
				DataRowCollection rows = dt.Rows;
				foreach( DataRow row in rows )
				{
					try
					{
						Job job = new Job();
						int jobNumber = data.GetInt( row , "JOB_CODE" );
						int altCode = data.GetInt( row , "ALT_CODE" );
						job.ExtId = useAlt ? altCode : jobNumber;
						job.Name = data.GetString( row , "JOB_NAME" );
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
				Main.Run.errorList.Add(ex);
			}
		}
	}
}
