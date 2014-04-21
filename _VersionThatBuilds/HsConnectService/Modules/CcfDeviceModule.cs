using HsSharedObjects.Client;
using HsSharedObjects.Client.CustomModule;
using HsConnect.Data;
using HsConnect.Services;
using HsConnect.Shifts;
using HsConnect.Shifts.Forecast;
using HsConnect.Main;

using System;
using System.Xml;
using System.Data;
using System.Collections;
using System.IO;

using Microsoft.Data.Odbc;

namespace HsConnect.Modules
{
	public class CcfDeviceModule : ModuleImpl
	{
		public CcfDeviceModule()
		{
			this.logger = new SysLog( this.GetType() );
		}

		private SysLog logger;

		public override bool Execute()
		{
			if( Details.CustomModuleList.IsActive( ClientCustomModule.CCF_DEVICE_MAPPING ) )
			{		
				logger.Log( "Running device mapping module." );

				/*
				PosiControl.Run( "POSIDBF","/DBF " + DateTime.Today.ToString( "MM/dd/yy" ) );

				DataTableBuilder builder = new DataTableBuilder();
				DataTable nameTermDt = builder.GetTableFromDBF( @"C:\DBF", @"C:\", "NAMETERM" );
				DataTable namePrtrDt = builder.GetTableFromDBF( @"C:\DBF", @"C:\", "NAMEPRTR" );

				DataRowCollection termRows = nameTermDt.Rows;
				int max = 0;
				String printerName = "";
				foreach( DataRow row in termRows )
				{
					try
					{
						if( Convert.ToInt32( row["CODE"].ToString() ) > max )
						{
							max = Convert.ToInt32( row["CODE"].ToString() );
							printerName = row["NAME"].ToString();
						}
					}
					catch( Exception ex )
					{
						logger.Error( "Error in CcfDeviceModule: " + ex.ToString() );
					}
				}

				logger.Log( max + " is the max." );

				int addCode = 0;

				foreach( DataRow row in namePrtrDt.Rows )
				{
					try
					{
						if( String.Compare( row["NAME"].ToString(), "HSTimeclockPrtr" ) == 0 )
						{
							addCode = Convert.ToInt32( row["CODE"].ToString() );
							logger.Log( addCode + " is the addCode." );
						}
					}
					catch( Exception ex )
					{
						logger.Error( "Error in CcfDeviceModule: " + ex.ToString() );
					}
				}

				ClientSettingsService settingsService = new ClientSettingsService();
				settingsService.updateCcfDevice( this.Details.ClientId, (max + addCode) );
				*/
				
			}
			return true;
		}
		
	}
}
