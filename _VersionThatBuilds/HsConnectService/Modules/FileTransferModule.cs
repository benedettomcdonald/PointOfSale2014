using System.Threading;
using HsConnect.Main;

using System;
using System.Collections;
using System.Data;
using System.IO;

using HsExecuteModule;
using HsFileTransfer;

namespace HsConnect.Modules
{
	/// <summary>
	/// Summary description for FileTransferModule.
	/// </summary>
	public class FileTransferModule : ModuleImpl
	{
		private SysLog logger = new SysLog(typeof(FileTransferModule));

		public override bool Execute()
		{
			logger.Log("Starting File Xfer Sync");

            ExecuteManager manager;
            Execute exec;
            FinalizeManager finalMgr;
            Finalize fin;

			ArrayList commands = Details.Commands;
			ArrayList files = Details.Files;
		    logger.Debug("Creating " + files.Count + " zip files");

			manager = new ExecuteManager( Details );
			exec = manager.GetPosExecuteClass();
		    logger.Debug("exec " + (exec == null ? "IS" : "IS NOT") + " null");
			exec.Commands = commands;
			bool doExec = true;
			if(Main.Run.Mapped)
				doExec = HsConnect.Data.PosiControl.MapDrive();
            logger.Debug("doExec = " + doExec);

			if(doExec)
				exec.Execute(Main.Run.Mapped);
			//finished executing commands
			exec = null;
			manager = null;
			
			//now for the files
			if(doExec)
			{
				foreach(FileForTransfer f in files)
				    XferFile(f);
			}
			//end file xfer
			if(Main.Run.Mapped)
				Data.PosiControl.CloseMap();

            //Run finalizer if applicable
            finalMgr = new FinalizeManager(Details);
            fin = finalMgr.GetPosFinalizeClass();
            logger.Debug("finalizer " + (fin == null ? "IS" : "IS NOT") + " null");
            if (fin != null)
            {
                fin.PerformFinalize();
            }


            RemoteLogger.Log(Details.ClientId, RemoteLogger.FILE_SUCCESS);
			return true;
		}

	    private void XferFile(FileForTransfer fileForTransfer)
	    {
            logger.Debug("XferFile()");
	        logger.Debug("Trying to send zip containing " + fileForTransfer.FilePaths.Count + " files");
	        int attempts = 0;
	        bool completed = false;
	        while (attempts < 6 && !completed)
	        {
	            try
	            {
                    if(Details.Preferences.PrefExists(HsSharedObjects.Client.Preferences.Preference.FILE_XFER_WITH_COMPANY_EXTREF))
                    {
                        HsFileTransfer.XferUtil.Process(fileForTransfer, Details.ClientCompanyExtRef);
                    }
                    else
                    {
                        HsFileTransfer.XferUtil.Process(fileForTransfer, Details.ClientId);
                    }
	                completed = true;
	            }
	            catch(IOException ioe)
	            {
	                logger.Error("Access error zipping file(s), trying again in 30s\n\t" + ioe);
	                ++attempts;
	                Thread.Sleep(30000);
	            }
	        }
	        if(!completed)
	        {
	            logger.Error("Failed zipping file, file transfer will not complete");
	        }
	    }
	}
}
