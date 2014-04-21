using HsSharedObjects.Client;
using HsSharedObjects.Client.CustomModule;

using HsConnect.Main;
using HsConnect.Data;
using HsConnect.SalesItems;
using HsConnect.Services;
using HsConnect.Services.Wss;
using HsConnect.GuestCounts;
using HsConnect.GuestCounts.PosList;

using System;
using System.Data;
using System.Collections;

using Microsoft.Data.Odbc;

namespace HsConnect.Modules
{
	public class MicrosCoverCount : ModuleImpl
	{
		public MicrosCoverCount()
		{
			this.logger = new SysLog( this.GetType() );
		}

		private SysLog logger;
		private HsData data = new HsData();
		private Hashtable _timePeriods;

		public override bool Execute()
		{
			Hashtable gcHash = new Hashtable();
			logger.Log( "executing MicrosCoverCount" );
			bool run = Details.CustomModuleList.IsActive( ClientCustomModule.MICROS_CVR_CNT );
			if( run )
			{
				logger.Log( "running MicrosCoverCount" );
				
				/* Load the hash table so that is has fixed_period seq , TimeSpan() */
				LoadTimePeriods();

				GuestCountList gcList = (GuestCountList) new EmptyGuestCountList();
				
				OdbcConnection newConnection = new OdbcConnection( this.Details.GetConnectionString() );
			
				try
				{			
					String start = DateTime.Now.AddDays( -7 ).ToString( "yyyy-MM-dd" );
					DataSet dataSet = new DataSet();
					OdbcDataAdapter dataAdapter = new OdbcDataAdapter("select business_date, fixed_period_seq, cov_cnt from micros.dly_sys_fixed_prd_ot_ttl " + 
						" where business_date >= \'" + start + "\'", newConnection );
					dataAdapter.Fill( dataSet , "micros.dly_sys_fixed_prd_ot_ttl" );
					dataAdapter.Dispose();
					DataRowCollection rows = dataSet.Tables[0].Rows;
					try
					{
						foreach( DataRow row in rows )
						{
							try
							{
								TimeSpan time = (TimeSpan) _timePeriods[ data.GetString( row, "fixed_period_seq" ) ];
								DateTime date = time.Hours > 6 ? data.GetDate( row, "business_date" ) : data.GetDate( row, "business_date" ).AddDays( 1 );
								GuestCountItem gcItem = new GuestCountItem();
								gcItem.Date = new DateTime( date.Year , date.Month , date.Day , time.Hours , time.Minutes , 0 );
								int gc = 0;
								logger.Debug( "looking for " + gcItem.Date.ToString() );
								if( gcHash.ContainsKey( gcItem.Date.Ticks + "" ) )
								{
									logger.Debug( "found " + Convert.ToInt32( (String) gcHash[ gcItem.Date.Ticks + "" ] ) );
									gc = Convert.ToInt32( (String) gcHash[ gcItem.Date.Ticks + "" ] );
								}
								logger.Debug( "setting gc to " + (gc + data.GetInt( row , "cov_cnt" ) ) );
								gcItem.GuestCount = (gc + data.GetInt( row , "cov_cnt" ));
								if( !gcHash.ContainsKey( gcItem.Date.Ticks + "" ) )
								{
									logger.Debug( "adding " + gcItem.Date.Ticks + ", " + gcItem.GuestCount );
									gcHash.Add( gcItem.Date.Ticks + "", gcItem.GuestCount+"" );
								}
								else
								{
									gcHash[ gcItem.Date.Ticks + "" ] = (Convert.ToInt32( (String) gcHash[ gcItem.Date.Ticks + "" ] ) + gcItem.GuestCount) + "";
									logger.Debug( "gcHash[" + gcItem.Date.Ticks + "] = " + (Convert.ToInt32( (String) gcHash[ gcItem.Date.Ticks + "" ] ) + gcItem.GuestCount) );
								}
								logger.Debug( "business_date: " + date.ToShortDateString() + " " + time.ToString() );
								logger.Debug( "fixed_period_seq: " + data.GetString( row, "fixed_period_seq" ) );
								logger.Debug( "cov_cnt: " + data.GetString( row, "cov_cnt" ) );
								logger.Debug( "gc count: " + gcItem.GuestCount );
								logger.Debug( "" );
								gcList.Add( gcItem );
							}
							catch(Exception ex)
							{
								logger.Error( ex.ToString() );
							}
						}
					}
					catch( Exception ex )
					{
						logger.Error( ex.ToString() );
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

				CarinosCustomWss carinosService = new CarinosCustomWss();
				logger.Debug( gcList.GetXmlString() );
				int cnt = carinosService.insertGcItems( this.Details.ClientId , gcList.GetXmlString() );
			}

			logger.Log( "executed MicrosCoverCount" );
			return true;			
		}

		private void LoadTimePeriods()
		{
			_timePeriods = new Hashtable();
			OdbcConnection newConnection = new OdbcConnection( this.Details.GetConnectionString() );
			
			try
			{			
				DataSet dataSet = new DataSet();
				OdbcDataAdapter dataAdapter = new OdbcDataAdapter("select fixed_period_seq, start_time, end_time from micros.fixed_period_cfg", newConnection );
				dataAdapter.Fill( dataSet , "micros.fixed_period_cfg" );
				dataAdapter.Dispose();
				DataRowCollection rows = dataSet.Tables[0].Rows;
				try
				{
					foreach( DataRow row in rows )
					{
						try
						{
							if( !_timePeriods.ContainsKey( data.GetString( row, "fixed_period_seq" ) ) )
							{								
								logger.Debug( "Period: " + data.GetString( row, "fixed_period_seq" ) );
								logger.Debug( "Time: " + GetTime( data.GetString( row, "start_time" ) ).ToString() );
								logger.Debug( "" );
								_timePeriods.Add( data.GetString( row, "fixed_period_seq" ), GetTime( data.GetString( row, "start_time" ) ) );
							}
							logger.Debug( "" );
						}
						catch( Exception ex )
						{
							logger.Error( "Error adding fixed_period_cfg in LoadTimePeriods(): " + ex.ToString() );
						}
					}
				}
				catch( Exception ex )
				{
					logger.Error( "Error adding fixed_period_cfg in LoadTimePeriods(): " + ex.ToString() );
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

		private TimeSpan GetTime( String timeStr )
		{
			String s = "";
			switch( timeStr.Length )
			{
				case 1:
					s = "000" + timeStr;
					break;
				case 2:
					s = "00" + timeStr;
					break;
				case 3:
					s = "0" + timeStr;
					break;
				case 4:
					s = timeStr;
					break;
			}
			String str = s.Substring( 0, 2 ) + ":" + s.Substring( 2, 2 );
			return TimeSpan.Parse( str );
		}
	}
}
