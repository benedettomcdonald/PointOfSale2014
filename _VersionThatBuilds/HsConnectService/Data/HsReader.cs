using System;
using System.IO;
using System.Collections;
using HsConnect.Main;

namespace HsConnect.Data
{
	public class HsReader
	{
		public HsReader(){}

		private static String CONFIG_FILE = @"C:\HS\hscmd.hs";
		
		public static String[] Execute()
		{
			SysLog logger = new SysLog( "HsConnect.Data.HsReader" );
			ArrayList results = new ArrayList();
			logger.Debug( "Executing..." );
			try
			{
				logger.Debug( "Looking for " + CONFIG_FILE );
				if( File.Exists( CONFIG_FILE ) )
				{
					logger.Debug( "Found " + CONFIG_FILE );
					StreamReader reader = File.OpenText( CONFIG_FILE );
					try
					{
						while( reader.Peek() > 0 )
						{
							results.Add( reader.ReadLine() );
						}
					}
					catch( Exception ex )
					{
						Console.WriteLine( ex.ToString() );
					}
					finally
					{
						reader.Close();
					}
					logger.Debug( "Deleting file..." );
					File.Delete( CONFIG_FILE );			
					logger.Debug( "File should be deleted..." );
				} else logger.Debug( CONFIG_FILE + " doesn't exist." );
			}
			catch( Exception ex )
			{
				logger.Error( ex.ToString() );
			}

			String[] strArray = new String[ results.Count ];
			int ind = 0;
			foreach( String str in results )
			{
				strArray[ ind ] = str;
				ind++;
			}

			return strArray;
		}
	}
}
