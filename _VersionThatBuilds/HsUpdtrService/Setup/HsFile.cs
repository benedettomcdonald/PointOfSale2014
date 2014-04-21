using hsupdater.Logger;
using hsupdater.Services;

using System;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace hsupdater.Setup
{
	public class HsFile
	{
		public HsFile()
		{
			_logger = new SysLog();
		}

		private String _name = "";
		private float _version = 0.0f;
		private int _status = -1;
		private String _path = "";
		private SysLog _logger;
		private static String FILE_PATH = System.Windows.Forms.Application.StartupPath + "\\files";

		public static int ADD = 0;
		public static int UPDATE = 1;
		public static int DELETE = 2;

		public bool Update()
		{
			if( _status == ADD || _status == UPDATE )
			{
				_logger.Log( "should AddFile()" );
				if( !AddFile( FILE_PATH+"\\NEWFILES" , _name ) ) return false;
			}
			if( _status == DELETE || _status == UPDATE )
			{
				if( !DeleteFile( FILE_PATH, _name ) ) return false;
				if(!CopyFile(FILE_PATH, _name ) )
				{
					RevertFile(FILE_PATH, _name);
					return false;
				}
			}
			if(_status == ADD)
			{
				CopyFile(FILE_PATH, _name);
			}
			return true;
		}

		private bool AddFile( String root, String fileName )
		{
			bool success = false;
			_logger.Log( "HsFile.AddFile( "+root+","+fileName+" )" );
			try
			{
				if( !Directory.Exists( root ) )
				{
					Directory.CreateDirectory( root );
				}
				PropertiesService properties = new PropertiesService();
				String byteString = properties.getFileByteString( fileName , _path );
				byte[] ba = new byte[ byteString.Length / 3 ];
				int index = 0;
				while( index < byteString.Length / 3 )
				{
					ba[index] = Convert.ToByte( byteString.Substring( index*3, 3 ) );
					index++;
				}			
			
				FileStream fs = new FileStream( 
					root + "\\" + fileName,
					FileMode.OpenOrCreate,
					FileAccess.Write);
				fs.Write(ba, 0, System.Convert.ToInt32(ba.Length));
				fs.Close();
				success = true;
				Thread.Sleep( 10000 );
			}
			catch( Exception ex )
			{
				_logger.Error( ex.ToString() );
				success = false;
			}
			
			_logger.Log( "file should have been added." );
			

			return success;
		}

		private bool DeleteFile( String path , String name )
		{
			bool success = false;
			String srcPath = path + "//" + name;
			String destPath = srcPath + "." + DateTime.Today.ToString("yyyyMMdd");
			if( File.Exists( srcPath ) )
			{
				try
				{
					while( Process.GetProcessesByName( "_hscnx" ).Length > 0 )
					{
						Process.GetProcessesByName( "_hscnx" )[0].Kill();
						Thread.Sleep( 2000 );
					}
					File.Copy(srcPath, destPath, true);
					File.Delete(srcPath);
					Thread.Sleep( 2000 );
					success = true;
				}
				catch( Exception ex )
				{
					_logger.Error( ex.ToString() );
				}
			}
			else
			{
				_logger.Error( path + " was set to be deleted or updated, but no file was found." );
			}
			return success;
		}

		private bool CopyFile(String path, String name)
		{
				bool success = false;
			try
			{
				String srcPath = path + "//NEWFILES//" + name;
				String destPath = path + "//" + name;
				File.Move(srcPath, destPath);
				success = true;
			}
			catch(Exception ex)
			{
				_logger.Error("failed to copy file from NEWFILES:  " + ex.ToString());
			}
			return success;

		}

		private bool RevertFile(String path, String name)
		{
			bool success = false;
			try
			{
				String destPath = path + "//" + name;
				String srcPath = destPath + "." + DateTime.Today.ToString("yyyyMMdd");
				File.Move(srcPath, destPath);
				success = true;
			}
			catch(Exception ex)
			{
				_logger.Error("failed to revert file.  This is BAD:  " + ex.ToString());
			}
			return success;

		}

		public String Name
		{
			get{ return _name; }
			set{ this._name = value; }
		}

		public float Version
		{
			get{ return _version; }
			set{ this._version = value; }
		}

		public int Status
		{
			get{ return _status; }
			set{ this._status = value; }
		}

		public String Path
		{
			get{ return _path; }
			set{ this._path = value; }
		}

	}
}
