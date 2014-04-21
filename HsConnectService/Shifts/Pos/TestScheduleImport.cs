using HsConnect.Shifts;
using HsConnect.Main;
using HsConnect.Data;
using HsConnect.Services;
using HsConnect.Services.Wss;
using HsSharedObjects.Client.Preferences;

using System;
using System.Collections;
using System.ServiceProcess;
using System.Data;
using System.Threading;
using System.IO;
using System.Xml;
using Microsoft.Data.Odbc;

namespace HsConnect.Shifts.Pos
{
	/// <summary>
	/// Summary description for TestScheduleImport.
	/// </summary>
	public class TestScheduleImport : ScheduleImportImpl
	{
		public TestScheduleImport()
		{

			this.logger = new SysLog( this.GetType() );
		}

		private SysLog logger;
		private HsData data = new HsData();
		private String OSCAR_PATH = System.Windows.Forms.Application.StartupPath + "\\OscarSCH.dat";
		private String OSCAR_PATH2 = System.Windows.Forms.Application.StartupPath + "\\OscarSCH2.dat";
		private String SQL_INF_PATH = System.Windows.Forms.Application.StartupPath + "\\sql.inf";
		private String IMPORT_PATH = @"C:\network\touchit\DATA2\IMPORT\";
		private bool copied;

		/**
			 * In this method we need to:
			 * 1) copy all .pay files in /import to a temp dir
			 * 2) delete the .pay files
			 * 3) delete expbin.inf from /export
			 * 4) restart the SwitchBoard service
			 * 5) copy expbin.inf
			 */ 
		private void Repair()
		{
			String copyFolder = @"\PayFiles" + DateTime.Today.ToShortDateString();
			//STEP ONE and TWO
			DirectoryInfo di = new DirectoryInfo(IMPORT_PATH);
			FileInfo[] payFiles = di.GetFiles("*.pay");
			HsFile file = new HsFile();
			if(payFiles != null && payFiles.Length > 0)
			{
				foreach(FileInfo fi in payFiles)
				{
					file.Copy(fi.DirectoryName, this.Cnx.Dsn + @"\hstemp" + copyFolder, fi.Name);
					file.Delete(fi.FullName);
				}
			}
			//STEP THREE
			file.Delete(@"C:\network\touchit\DATA2\EXPORT\Expbin.inf");
			//STEP FOUR
			ServiceController sc = null;
			try
			{
				ServiceController[] listOfServices = ServiceController.GetServices();
				foreach( ServiceController serv in listOfServices )
				{
					if( serv.ServiceName.Equals( "SwitchBoard"))
					{
						sc = serv;
					}
				}
				Thread.Sleep( 2000 );
			}
			catch( Exception ex )
			{
				logger.Error( "Error looking for SwitchBoard service. " + ex.ToString() );
				throw ex;
			}
			try
			{
				if ( sc.Status == ServiceControllerStatus.Stopped ||  sc.Status == ServiceControllerStatus.StopPending )
				{
					sc.Start();
					Thread.Sleep( 5000 );
				}
			}
			catch(Exception ex)
			{
				logger.Error( "Error starting SwitchBoard service. " + ex.ToString() + " : " + ex.StackTrace );
				//throw ex;
			}
			//STEP FIVE
			File.Copy( @"C:\network\touchit\DATA2\Expbin.inf" , @"C:\network\touchit\DATA2\EXPORT\Expbin.inf" );
		}

		public override void Execute()
		{
			
			/*unsure about this part being usable in the DOS version*/
			logger.Debug( "Copying Expbin.inf to EXPORT dir." );

			Hashtable empHash = EmpIdHash();

			try
			{
				File.Copy( @"C:\network\touchit\DATA2\Expbin.inf" , @"C:\network\touchit\DATA2\EXPORT\Expbin.inf" );
			}
			catch(Exception ex)
			{
				logger.Error(ex.ToString());
		//		Repair();
			}
			
			logger.Debug( "Copied, now sleeping." );
			System.Threading.Thread.Sleep( 5000 );
			
			Hashtable ssnHash = GetSSNHash();

			logger.Debug( "Created Hashtable:  "  + ssnHash.Count );	

			ShiftList hsShifts = this.Shifts;

			logger.Log( hsShifts.Count + " shifts in HS Shift list." );	

			if( File.Exists( OSCAR_PATH ) )
			{
				File.Delete( OSCAR_PATH );
				System.Threading.Thread.Sleep( 2000 );
			}

			if( File.Exists( OSCAR_PATH2 ) )
			{
				File.Delete( OSCAR_PATH2 );
				System.Threading.Thread.Sleep( 2000 );
			}

			BinaryWriter writer = null;
			BinaryWriter writer2 = null;

			try
			{
				FileStream fs = new FileStream(OSCAR_PATH, FileMode.CreateNew);
				writer = new BinaryWriter(fs);

				FileStream fs2 = new FileStream(OSCAR_PATH2, FileMode.CreateNew);
				writer2 = new BinaryWriter(fs2);

				foreach( Shift shift in hsShifts )
				{
					logger.Log( "is empHash[" + shift.PosEmpId.ToString() + "] null = " + (empHash[ shift.PosEmpId.ToString() ] == null).ToString() );
					if( empHash[ shift.PosEmpId.ToString() ] != null )
					{
						logger.Log( "is ssnHash[" + empHash[ shift.PosEmpId.ToString() ].ToString() + "] null = " + 
							(ssnHash[ empHash[ shift.PosEmpId.ToString() ].ToString() ] == null).ToString() );
					}
					if( empHash[ shift.PosEmpId.ToString() ] != null &&
						ssnHash[ empHash[ shift.PosEmpId.ToString() ].ToString() ] != null )
					{

						logger.Debug( shift.PosEmpId + " emp." );
						String ssn = ssnHash[ empHash[ shift.PosEmpId.ToString() ].ToString() ].ToString();
						logger.Debug( "SSN = " + ssn );
						logger.Debug( "In Date = " + shift.ClockIn.ToString( "yyyyMMdd" ) );
						logger.Debug( "In Time = " + shift.ClockIn.ToString( "HHmm" ) );
						logger.Debug( "Out Date = " + shift.ClockOut.ToString( "yyyyMMdd" ) );
						logger.Debug( "Out Time = " + shift.ClockOut.ToString( "HHmm" ) );
						logger.Debug( "Pay Rate = " + "0.0" );
						logger.Debug( "Job Code = " + shift.PosJobId.ToString() );
						String ln = ssn + "," + 
							shift.ClockIn.ToString( "yyyyMMdd" ) + "," + 
							shift.ClockIn.ToString( "HHmm" ) + "," + 
							shift.ClockOut.ToString( "yyyyMMdd" ) + "," + 
							shift.ClockOut.ToString( "HHmm" ) + "," + 
							"0.0" + "," + 
							shift.PosJobId.ToString();				
					
						try
						{
							writer.Write( ln );
							writer2.Write( System.Text.Encoding.UTF8.GetBytes(ln));
						}
						catch( Exception e )
						{
							logger.Error( e.ToString() );
						}
					}
					else
					{
						logger.Error( "Employee " + shift.PosEmpId + " has an invalid mapping." );
						logger.Error("DEBUG DUMP:");
						foreach(String key in ssnHash.Keys)
						{
							logger.Error(key);
						}
					}
				}
			}
			catch( Exception ex )
			{
				logger.Error( ex.ToString() );
			}
			finally
			{
				writer.Flush();
				writer.Close();
				writer2.Flush();
				writer2.Close();
				if(copied && File.Exists( System.Windows.Forms.Application.StartupPath + "\\EMP28.CSV" ) )
				{
					File.Delete( System.Windows.Forms.Application.StartupPath + "\\EMP28.CSV" );
				}
			}			

			// Copy the SCH file to an empty dir
			if( !Directory.Exists( System.Windows.Forms.Application.StartupPath + "\\_scheds" ) )
			{
				Directory.CreateDirectory( System.Windows.Forms.Application.StartupPath + "\\_scheds" );
			}
			File.Copy( OSCAR_PATH , System.Windows.Forms.Application.StartupPath + "\\_scheds\\" + 
				DateTime.Now.ToString( "MMddyyy" ) + ".csv" ,true );
			File.Copy( OSCAR_PATH , IMPORT_PATH + "OscarSCH.csv" , true );
			File.Copy( SQL_INF_PATH , IMPORT_PATH + "sql.inf" , true );
		}

		private Hashtable GetSSNHash()
		{
			Hashtable ssnHash = new Hashtable();
			StreamReader text = null;
			try
			{
				File.Copy( @"C:\network\touchit\DATA2\EXPORT\EMP28.CSV", System.Windows.Forms.Application.StartupPath + "\\EMP28.CSV" );
				copied = true;
			}
			catch( Exception ex )
			{
				copied = false;
				logger.Error( "Could Not Copy File:  " + ex.StackTrace );
			}

			try
			{
				try
				{
					if( copied )
					{
						logger.Debug("Copied file, now opening");
						text = File.OpenText( System.Windows.Forms.Application.StartupPath + "\\EMP28.CSV" );
					}
					else
					{
						logger.Debug("Could not copy:  sleeping for 3 seconds and then opening original file");
						Thread.Sleep(3000);
						bool loop = true;
						int limit = 0;
						while(loop && limit < 60)
						{
							try
							{
								text= File.OpenText( @"C:\network\touchit\DATA2\EXPORT\EMP28.CSV" );
								logger.Debug("opened file, exiting loop");
								loop = false;
							}
							catch(Exception ex)
							{
								logger.Debug("Could not open file yet, keep looping");
								limit ++;
								Thread.Sleep(3000);
							}
						}
					}
				}
				catch(Exception ex)
				{
					logger.Debug("Error opening file:  here is the ERROR " + ex.ToString() + ", " +ex.StackTrace);
					//text= File.OpenText( @"C:\network\touchit\DATA2\EXPORT\EMP28.CSV" );

				}
				logger.Log( "text == null = " + (text == null).ToString() );
				while( text.Peek() > 0 )
				{
					String inStr = text.ReadLine();
					String[] strArray = inStr.Split( new char[] {','} );			
					
					try
					{
						if( !strArray[0].Substring( 0 , 1 ).Equals( "\"" ) && 
							!strArray[1].Substring( 0 , 1 ).Equals( "\"" ) )
						{
							logger.Log( strArray[0].Trim() + " , " + strArray[1] );
							if( !ssnHash.ContainsKey( strArray[0].Trim() ) ) ssnHash.Add( strArray[0].Trim() , strArray[1] );
						}		
					}
					catch( Exception ex )
					{
						logger.Error( ex.ToString() );
					}
				}
			}
			finally
			{
				text.Close();
			}

			return ssnHash;
		}

		public override void GetWeekDates()
		{
			return;
		}

		private Hashtable EmpIdHash()
		{
			Hashtable hash = new Hashtable();
			ScheduleWss service = new ScheduleWss();
			XmlDocument doc = new XmlDocument();
			logger.Debug( "client id = " + this.Details.ClientId );
			if( this.Details.Preferences.PrefExists( Preference.IGNORE_DL_ID ) )
			{
				foreach( Shift shift in this.shifts )
				{
					if( !hash.ContainsKey( shift.PosEmpId + "" ) )
					{
						logger.Log( shift.PosEmpId + "" +","+ shift.PosEmpId + "" );
						hash.Add( shift.PosEmpId + "" , shift.PosEmpId + "" );
					}
				}
			}
			else
			{
				String mapXml = service.getDLMappingXML( this.Details.ClientId );
				logger.Debug( mapXml );
				doc.LoadXml( mapXml );
				foreach( XmlNode node in doc.SelectNodes( "/employee-id-mapping/employee" ) )
				{
					if( !hash.ContainsKey( node.Attributes["dl-id"].InnerText ) )
					{
						logger.Log( node.Attributes["dl-id"].InnerText +","+ node.Attributes["pos-id"].InnerText );
						hash.Add( node.Attributes["dl-id"].InnerText , node.Attributes["pos-id"].InnerText );
					}
				}
			}
			return hash;
		}

		public override DateTime CurrentWeek{ get{ return new DateTime(0); } set{ this.CurrentWeek = value; }}

		

	}
}
