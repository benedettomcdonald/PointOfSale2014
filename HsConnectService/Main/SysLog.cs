using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Collections;

namespace HsConnect.Main
{
	/// <summary>
	/// This class handles debugging and error logging.
	/// TESTING TESTING 1 2 3
	/// </summary>
	public class SysLog
	{
		public SysLog( String typeName )
		{
			this.typeName = typeName;
		}

		public SysLog( Type type )
		{
			this.objType = type;
		}

		private String logExt = ".hs";
		private String debugPath = Application.StartupPath + "\\_debug\\";
		private String errorPath = Application.StartupPath + "\\_error\\";
		private String loggerPath = Application.StartupPath + "\\_log\\";
		private Type objType;
		private String typeName = "";
		private String logPath = "";
		private ArrayList debugClasses = new ArrayList();

		private String TypeName
		{
			get
			{
				if( objType == null ) return typeName;
				return objType.ToString();
			}
			set { value = ""; }	
		}

		public String getLogPath( String rootPath )
		{
			String day = DateTime.Now.Day < 10 ? "0" + DateTime.Now.Day.ToString() : DateTime.Now.Day.ToString();
			String month = DateTime.Now.Month < 10 ? "0" + DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString();
			if( !Directory.Exists( rootPath ) )
			{
				Directory.CreateDirectory( rootPath );
			}
			return rootPath + DateTime.Now.Year + "-" + month + "-" + day + logExt;
		}


		public void Debug( String msg )
		{
			//Console.WriteLine( msg );
			if( debugClasses.Count == 0 )
			{
				loadDebugProperties();
			}
			if( displayMsg( TypeName ) )
			{
				logMessage( debugPath, msg, null );
			}
		}

		public void Error( String msg )
		{
			logMessage( errorPath, msg , null);
		}

        public void Error(String msg, Exception ex)
        {
            logMessage(errorPath, msg, ex);
        }

		public void Log( String msg )
		{
			logMessage( loggerPath, msg, null );
		}

		private void logMessage( String root, String msg, Exception ex )
		{
			logPath = getLogPath( root );	
			try
            {
                string fileSoFar = "";

                if (ex != null)
                    msg += "\r\n\t" + ex.ToString().Replace("\n", "\r\n\t");

                msg.Replace("\n", "\r\n\t");

                string logTime = DateTime.Now.TimeOfDay.ToString();
                string logClass = objType != null ? objType.ToString() : typeName;
                string logMethod = new StackFrame(2).GetMethod().Name;
                string log = logTime + " [" + logClass + "." + logMethod + "()]\t" + msg;

				if( File.Exists( logPath ) )
				{
					StreamReader reader = File.OpenText( logPath );
					try
					{
						fileSoFar = reader.ReadToEnd();
					} 
					finally
					{
						reader.Close();
					}
				
				}
				StreamWriter writer = File.CreateText( logPath );
				try
				{
					writer.WriteLine( fileSoFar + log );
				}
				finally
				{
					writer.Flush();
					writer.Close();
				}
			}
			catch(Exception e)
			{
				Console.WriteLine( e.ToString() );
			}
		}


		internal void DeleteLogs( String logPath )
		{
			try
			{
				FileInfo[] files = Directory.CreateDirectory( Application.StartupPath + "\\" + logPath + "\\" ).GetFiles();
				if( files.Length > 0 )
				{

					foreach( FileInfo file in files )
					{
						if( file.Name.Substring( 0 , 1 ).Equals("2") )
						{
							int year = Convert.ToInt32( file.Name.Substring( 0 , 4 ) );
							int month = Convert.ToInt32( file.Name.Substring( 5 , 2 ) );
							int day = Convert.ToInt32( file.Name.Substring( 8 , 2 ) );
							DateTime date = new DateTime( year , month , day );
							TimeSpan diff = DateTime.Now - date;
							if( diff.Days > 6 )
							{
								File.Delete( Application.StartupPath + "\\" + logPath + "\\" + file.Name );
							}
						}
					}
				}
			}
			catch( Exception ex )
			{
				Console.WriteLine( ex.ToString() );
			}
			
		}
	

		public void loadDebugProperties()
		{
			String file = Application.StartupPath + "\\debug.properties";
			try
			{
				if( File.Exists( file ) )
				{
					StreamReader reader = File.OpenText( file );
					try
					{
						while( reader.Peek() > 0 )
						{
							debugClasses.Add( reader.ReadLine() );
						}
					} 
					finally
					{
						reader.Close();
					}			
				}
			}
			catch(Exception e)
			{
				Console.WriteLine( e.ToString() );
			}
		}

		private bool displayMsg( String classPath )
		{
			foreach( String str in debugClasses )
			{
				int len = str.Length;
				String lastChar = str.Substring( len-1, 1 );
				if( String.Compare( lastChar, "*" ) == 0 )
				{
					String path = str.Substring( 0, len-2 );
					int subLen = path.Length;
					if( classPath.Length >= subLen 
						&& String.Compare( classPath.Substring( 0, subLen ), path ) == 0 )
					{
						return true;					
					}
				}
				else if( String.Compare( str, classPath ) == 0 )
				{
					return true;
				}
			}
			return false;
		}
	}
}
