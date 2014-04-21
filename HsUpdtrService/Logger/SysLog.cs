using System;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace hsupdater.Logger
{
	/// <summary>
	/// This class handles debugging and error logging.
	/// </summary>
	public class SysLog
	{
		public SysLog()
		{
			if( !Directory.Exists( Application.StartupPath + "\\_sys" ) )
			{
				Directory.CreateDirectory( Application.StartupPath + "\\_sys" );
			}
		}

		public SysLog( String type )
		{
			if( !Directory.Exists( Application.StartupPath + "\\_sys" ) )
			{
				Directory.CreateDirectory( Application.StartupPath + "\\_sys" );
			}
			this.objType = type;
		}

		private String logExt = ".hs";
		private String rootPath = Application.StartupPath + "\\_sys\\";
		private String objType = "";
		private String logPath = "";

        private void LogMessage(String path, String msg, Exception ex)
        {
            Console.WriteLine(msg);
            logPath = path;
            try
            {
                StringBuilder sb = new StringBuilder(msg);

                String fileSoFar = "";

                if (ex != null)
                {
                    sb.Append(ex.Message + Environment.NewLine);
                    sb.Append(ex.StackTrace + Environment.NewLine);
                }

                String newData = DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss") + "	" + sb;
                if (File.Exists(logPath))
                {
                    StreamReader reader = File.OpenText(logPath);
                    try
                    {
                        fileSoFar = reader.ReadToEnd();
                    }
                    finally
                    {
                        reader.Close();
                    }

                }
                StreamWriter writer = File.CreateText(logPath);
                try
                {
                    writer.WriteLine(fileSoFar + newData);
                }
                finally
                {
                    writer.Flush();
                    writer.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
		
		public void Log(String msg)
		{
			LogMessage(getLogPath(), msg, null);
		}

		public void Error(String msg)
		{
		    LogMessage(getErrorLogPath(), msg, null);
		}

        public void Error(String msg, Exception ex)
        {
            LogMessage(getErrorLogPath(), msg, ex);
        }

		internal void DeleteLogs( )
		{
			try
			{
				FileInfo[] files = Directory.CreateDirectory( Application.StartupPath + "\\_sys" ).GetFiles();
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
								File.Delete( rootPath + file.Name );
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

        public String getLogPath()
        {
            String day = DateTime.Now.Day < 10 ? "0" + DateTime.Now.Day.ToString() : DateTime.Now.Day.ToString();
            String month = DateTime.Now.Month < 10 ? "0" + DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString();
            return rootPath + DateTime.Now.Year + "-" + month + "-" + day + logExt;
        }

        public String getErrorLogPath()
        {
            String day = DateTime.Now.Day < 10 ? "0" + DateTime.Now.Day.ToString() : DateTime.Now.Day.ToString();
            String month = DateTime.Now.Month < 10 ? "0" + DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString();
            return rootPath + DateTime.Now.Year + "-" + month + "-" + day + "_error" + logExt;
        }
	}
}
