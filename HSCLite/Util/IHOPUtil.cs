using System;

using HSCLite.Services;
using HSCLite.Shifts;
using HSCLite.Xml;
using HSCLite.Data;
using HsSharedObjects.Main;

using System.Data;
using Microsoft.Data.Odbc;


namespace HSCLite.Util
{
	/// <summary>
	/// Summary description for IHOPUtil.
	/// </summary>
	public class IHOPUtil : Util
	{
		public IHOPUtil()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		
		public bool RunSynch()
		{
			
			SysLog logger = new SysLog(this.GetType());
			HSCLite.Run.errorList.Clear(this.GetType().ToString());
			try
			{
				IHOPService serv = new IHOPService();
				DateTime startDate = DateTime.Today;

				// set up sched import
				ScheduleImportManager importMgr = new ScheduleImportManager();
				ScheduleImport schedImport = importMgr.GetPosScheduleImport();

				logger.Log( "Executing IHOPschedule import for store number " + Run.StoreNumber);
				String schedXml = serv.getSchedule(Run.StoreNumber);
				
				ShiftList shiftList = new ShiftList();
				shiftList.ImportScheduleXml( false, false, schedXml );
				logger.Debug( schedXml );
				logger.Log( "Downloaded " + shiftList.Count + " shifts." );

				// run sched import
				schedImport.AutoSync = true;
				schedImport.Shifts = shiftList;
				schedImport.Execute();
				logger.Log( "Imported schedule into the POS." );
			}
			catch(Exception ex)
			{
				logger.Error(ex.ToString());
				HSCLite.Run.errorList.Add(ex);
				HSCLite.Run.errorList.Send();
				return false;
			}

			return true;

		}

		public int GetStoreNumber()
		{
			int storenumber = -1;
			SysLog logger = new SysLog(this.GetType());
			OdbcConnection newConnection = new OdbcConnection( HSCLite.Run.ConnectionString );
			HsData data = new HsData();
			try
			{
				DataSet dataSet = new DataSet();
				OdbcDataAdapter dataAdapter = new OdbcDataAdapter(
					"SELECT obj_num FROM micros.rest_def" , newConnection );
				dataAdapter.Fill( dataSet , "micros.rest_def" );
				dataAdapter.Dispose();
				DataRowCollection rows = dataSet.Tables[0].Rows;
				foreach( DataRow row in rows )
				{
					try
					{
						storenumber = data.GetInt( row, "obj_num");
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
			return storenumber;
		}

		public bool NeedsStoreNumber
		{ 
			get { return true; }
		}

	}
}
