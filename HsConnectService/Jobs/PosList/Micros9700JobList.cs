using HsConnect.Data;

using System;
using System.Data;
using Microsoft.Data.Odbc;

namespace HsConnect.Jobs.PosList
{
	public class Micros9700JobList : JobListImpl
	{
		public Micros9700JobList(){}
		
		private HsData data = new HsData();
		
		//STATICS		
		private static String JOB_FILE = "job_code_def";
		//JOB_CODE, JOB_NAME correspond to the columns of the data table. 
		//They must be changed if the structure of the data table or query are edited
		private static String JOB_CODE = "Row0";
		private static String JOB_NAME = "Row1";

		#region unused methods
		/*public void DbInsert
		 * 
		 * Currently a placeholder with a console.writeline
		 * 
		 */
		public override void DbInsert()
		{
			Console.WriteLine("This is the 9700 DBInsert method for JobList");
		}

		/*public void DbUpdate
		 * 
		 * Currently a placeholder with a console.writeline
		 * 
		 */
		public override void DbUpdate()
		{
			Console.WriteLine("This is the 9700 DBUpdate method for JobList");
		}
		#endregion

		/*public void DbLoad
		 * Used with Wizard.  This method will query the 9700 database and create csv files
		 * containing the PoS job information needed by HotSchedules.
		 * It will then create a DataTable with this info for use in
		 * populating the JobList with Jobs 
		 */
		public override void DbLoad()
		{		
			try
			{
				Micros9700Control.Run( Micros9700Control.JOB_SYNC );//runs the job batch
				DataTableBuilder builder = new DataTableBuilder();
				DataTable dt = builder.GetTableFromCSV( Micros9700Control.PATH_NAME, JOB_FILE );
				DataRowCollection rows = dt.Rows;
				foreach( DataRow row in rows )
				{
					try
					{
						Job job = new Job();
						job.ExtId = data.GetInt( row , JOB_CODE );
						job.Name = data.GetString( row , JOB_NAME );
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
		}//end DbLoad
	}
}
