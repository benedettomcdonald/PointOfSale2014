using System;
using System.Collections;
using HsSharedObjects.Main;
using System.Windows.Forms;
using System.IO;

namespace HsExecuteModule.posList
{
	/// <summary>
	/// Summary description for AlohaExecute.
	/// </summary>
	public class AlohaExecute : BaseExecute
	{
		private SysLog logger = new SysLog(typeof(AlohaExecute));

		//could combine FILE_NAME and FILE_EXT into one variable (they're rarely used separately I think?), but I wanted an easy way
		//to be able to add numbers to the FILE_NAME (so GNDSALE1.dbf, GNDSALE2.dbf, etc.). Hope it isn't too confusing.
		private static readonly String FILE_NAME = "GNDSALE";
		private static readonly String FILE_EXT = ".dbf";

		public override void Execute(bool map)
        {
            logger.Debug("Executing AlohaExecute");
            RunCommand(null);
        }

		public override void RunCommand(Command cmd)
		{
			logger.Debug("Running AlohaExecute");
			CopyDBFs();
		}

		private void CopyDBFs()
		{
			try
			{
				DateTime now = DateTime.Now;				
				for(int i = 1; i < 8; i++)
				{
					DateTime past = now.AddDays(-i); 
					String dateString = past.ToString("yyyyMMdd"); //get in the form year month day, without spaces (e.g., 20100224 for Feb 24 2010)
					String folderPath = this.ClientDetails.Dsn +  @"\" + dateString;
					String destination = this.ClientDetails.Dsn + @"\hstemp";
					if(!checkFileExistence(folderPath))
					{
						//file/directory doesn't exist?
						throw new Exception("Source Directory or File Could Not Be Found. Folder Path was = " + folderPath + " and file name was " 
							+ FILE_NAME + FILE_EXT);

					}


					if(!Directory.Exists(destination))
					{
						//destination directory doesn't exist?
						throw new Exception("Destination Directory Could Not Be Found. Destination = " + destination );
					}
					File.Copy(folderPath+ @"\" +FILE_NAME+FILE_EXT,destination+ @"\" + FILE_NAME+i+FILE_EXT,true);	
				}

			}

			catch(Exception ex)
			{
				logger.Error(ex.ToString());
			}
		}


		private bool checkFileExistence(string dir)
		{

			if(!Directory.Exists(dir))
			{
				return false;
			}
			
			if(!File.Exists(dir+ @"\" + FILE_NAME+FILE_EXT))
			{
				return false;
			}

			return true;
		}
	}
}
