using System;
using System.IO;
using System.Threading;
using System.Runtime.Serialization;
using HsSharedObjects;
using HsSharedObjects.Client.Preferences;

namespace HsExecuteModule.posList
{
	/// <summary>
	/// Summary description for PosiExecute.
	/// </summary>
	[Serializable]
	public class PosiExecute : BaseExecute
	{
		private String drive;
		private bool mapped;

		public override void RunCommand(Command cmd)
		{
			Wait();
			System.Diagnostics.Process proc = new System.Diagnostics.Process();
			proc.EnableRaisingEvents=false;
            if (mapped)
                proc.StartInfo.WorkingDirectory = drive + @":\SC";
            else
                proc.StartInfo.WorkingDirectory = cmd.Dir.Split(':')[0] + @":\SC";
		    logger.Debug("Set working directory to " + proc.StartInfo.WorkingDirectory);
			//proc.StartInfo.WorkingDirectory = drive + @":\SC";
			proc.StartInfo.FileName=cmd.Cmd;
			proc.StartInfo.Arguments=cmd.Args;
			/*
					proc.StartInfo.WorkingDirectory = @"L:\SC\";
					*/
			//if(mapped)
		//	{
		//		logger.Log("is Mapped");
				if( File.Exists( @"C:\WINDOWS\system32\cmd.exe" ) )
				{
					proc.StartInfo.FileName=@"C:\WINDOWS\system32\cmd.exe";
				}
				else
				{
					proc.StartInfo.FileName=@"C:\WINNT\system32\cmd.exe";
				}
				proc.StartInfo.Arguments="/c \""+cmd.Cmd + " " + cmd.Args + "\"" ;
		//	}
			logger.Log("about to execute:  " + proc.StartInfo.WorkingDirectory + @"\ " + proc.StartInfo.FileName + " " + proc.StartInfo.Arguments);
			proc.Start();
			proc.WaitForExit();
		}
		
		public override void Execute(bool map)
		{
			mapped = map;
            drive = (map ? "S" : "C");

            if (map)
            {
                try
                {
                    if (HsCnxData.Details.Preferences.PrefExists(Preference.POSI_CUSTOM_DRIVE_MAPPING))
                    {
                        Preference pref = HsCnxData.Details.Preferences.GetPreferenceById(Preference.POSI_CUSTOM_DRIVE_MAPPING);
                        if (pref.Val2 != null)
                            drive = pref.Val2.Substring(0, 1);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error getting map drive from settings", ex);
                }
            } 
            logger.Debug("Setting drive to " + drive + ":/");
			base.Execute(map);
		}

		private void Wait()
		{
			while(true)
			{
				if(!File.Exists(drive + @":\SC\TA.OPN"))
				{
					return;
				}
				Thread.Sleep(3000);
			}
		}

	}
}
