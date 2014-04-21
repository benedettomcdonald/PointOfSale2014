using HsSharedObjects.Main;

using System;
using System.IO;

namespace HSCLite.Data
{
	public class HsFile
	{
		public HsFile()
		{
            logger = new SysLog( this.GetType() );			
		}

		private SysLog logger;
		public static String TMP_DIR = @"C:\Aloha\hstemp\";

		public bool Copy( String source, String destination )
		{
			try
			{
				if( !Directory.Exists( TMP_DIR ) ) Directory.CreateDirectory( TMP_DIR );
				File.Copy( source , TMP_DIR + destination , true );
				return true;
			}
			catch( Exception ex )
			{	
				logger.Error( ex.ToString() );
				return false;
			}			
		}

		public bool Copy( String sourcePath, String destPath, String fileName )
		{
			try
			{
				if( !Directory.Exists( destPath ) ) Directory.CreateDirectory( destPath );
				File.Copy( sourcePath+@"\"+fileName , destPath+@"\"+fileName , true );
				return true;
			}
			catch( Exception ex )
			{	
				logger.Error( ex.ToString() );
				return false;
			}
		}

		public bool Delete( String path )
		{
			try
			{
				if( File.Exists( path ) ) File.Delete( path );
				return true;
			}
			catch( Exception ex )
			{	
				logger.Error( ex.ToString() );
				return false;
			}
		}
	}
}
