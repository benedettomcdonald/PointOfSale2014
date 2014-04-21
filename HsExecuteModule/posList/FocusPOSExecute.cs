using System;
using System.Text;
using System.IO;

namespace HsExecuteModule.posList
{
    class FocusPOSExecute : BaseExecute
    {
        private String dropoffDir = @"\Focus\Status\";
        private String pickupDir = @"\Focus\HotSchedules\";
        private bool generateExport = false;

        public override void Execute(bool map)
        {
            logger.Debug("Begin FocusPOSExecute.Execute()");
            ConfigureDirectories();

            foreach (Command cmd in commands)
            {
                logger.Debug("Preparing to execute commands on FocusPOS using the following directories: dropoffDir:" + dropoffDir + "  ;; pickupDir:" + pickupDir);
                try
                {
                    RunCommand(cmd);
                    WaitForFile();
                }
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());
                }
            }
            logger.Debug("Finish FocusPOSExecute.Execute()");
        }

        public override void RunCommand(Command cmd)
        {
            logger.Debug("FocusPosExecute running the following command / args: " + cmd.Cmd + " / " + cmd.Args);
            TextWriter writer = null;
            try
            {
                ProcessArgs(cmd.Args);

                if (generateExport)
                {
                    //if we're generating, clean out folders first
                    CleanupFolders();

                    logger.Debug("Generating FocusPos export before sending file to HS");
                    String file = dropoffDir + "HsManualUpload";
                    writer = new StreamWriter(file);
                    writer.Write("<T>2</T>");
                    writer.WriteLine();
                    writer.Write("<C>" + DateTime.Now.AddDays(-1).ToString("MMddyyyy") + "</C>");
                    writer.WriteLine();
                }
                else
                {
                    //if we're picking up, don't clean out folders as we would delete what they left there for us
                    logger.Debug("FocusPOS configured to pickup standard daily export, no generation will commence.");
                }
            }
            catch (Exception ex)
            {
                logger.Error("ERROR running command in FocusPosExecute" + cmd.Cmd + " / " + cmd.Args, ex);
            }
            finally
            {
                if (writer != null)
                {
                    try
                    {
                        writer.Flush();
                    }
                    catch (Exception wfe)
                    {
                        logger.Error("ERROR Flushing file writer, FocusPOS export may not generate properly");
                    }
                    try
                    {
                        writer.Close();
                    }
                    catch (Exception wce)
                    {
                        logger.Error("ERROR Closing file writer, FocusPOS export may not generate properly");
                    }
                }
            }
        }

        private void ConfigureDirectories()
        {
            dropoffDir = this.ClientDetails.Dsn + dropoffDir;
            pickupDir = this.ClientDetails.Dsn + pickupDir;
            logger.Debug("Configured directories. dropoffDir: " + dropoffDir + " ;; pickupDir: " + pickupDir);
        }

        private bool WaitForFile()
        {
            //bail out if the file is not found before x attempts
            int countFileNotFound = 0;
            int MAX_RETRIES = 5;

            bool readyToTransfer = false;

            while (!readyToTransfer && countFileNotFound < MAX_RETRIES)
            {
                //wait 3 seconds before continuing
                System.Threading.Thread.Sleep(3000);

                //check the pickup directory to see if the file is there with the correct .FQDN extension
                System.IO.DirectoryInfo di = new DirectoryInfo(pickupDir);
                if (di.Exists)
                {
                    //loop the files
                    foreach (FileInfo file in di.GetFiles())
                    {
                        //want to take a substring to check suffix, ensure string is large enough to do this before attempting
                        String fileName = file.FullName.ToLower();
                        if (fileName.Length < 4)
                        {
                            continue;
                        }

                        //check what suffix the export file has on it at this time. if tmp, wait 3s before checking again
                        String suffix = fileName.Substring(fileName.Length - 4, 4).ToLower();
                        if(suffix.Equals(".tmp")){
                            continue;
                        }
                        if (suffix.Equals("ftpq"))
                        {
                            readyToTransfer = true;
                            continue;
                        }

                        //didn't find a file with either suffix. increment file not found and try again
                        countFileNotFound++;
                        if (countFileNotFound > MAX_RETRIES)
                        {
                            logger.Debug("ERROR: waited more than 15s for export file to generate but it did not. Bailing out");

                        }
                    }//foreach file
                }//if di.exists
                else
                {
                    logger.Error("ERROR running FocusPOS Sync Command, pickup directory - " + pickupDir + " - does not exist!");
                    return false;
                }
            }//while 
            return true;
        }

        private void ProcessArgs(String args)
        {
            logger.Debug("FocusPOSExecute processing the following args: " + args);
            try
            {
                if (args == null || args.Length == 0)
                {
                    generateExport = false;
                    return;
                }
                else
                {
                    generateExport = true;
                    return;
                }
            }
            catch (Exception e)
            {
                logger.Error("ERROR parsing Args for command in FocusPOSExecute: " + args, e);
            }
        }

        private void CleanupFolders()
        {
            logger.Debug("Begin CleanupFolders()");
            //Cleanup the export directory before processing commands
            System.IO.DirectoryInfo di = new DirectoryInfo(dropoffDir);
            if (di.Exists)
            {

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dinfo in di.GetDirectories())
                {
                    dinfo.Delete(true);
                }

            }

            //Cleanup the import directory before processing commands
            di = new DirectoryInfo(pickupDir);
            if (di.Exists)
            {

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dinfo in di.GetDirectories())
                {
                    dinfo.Delete(true);
                }

            }
            logger.Debug("Finish CleanupFolders()");
        }//cleanupFolder()
    }
}
