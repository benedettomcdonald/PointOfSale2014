using System;
using HsSharedObjects.Main;
using System.IO;
using Microsoft.Data.Odbc;
using System.Collections;
using HsFileTransfer;
using System.Windows.Forms;

namespace HsExecuteModule.posList
{
    /// <summary>
    /// Summary description for TCPlusExecute.
    /// Copies EMPLLIST.DBF to a temporary location where it can
    /// be modified to have SSN scrubbed. We then change 
    /// ClientDetails.files.FilePath location for this file
    /// to the temporary location, so that fileXfer() will 
    /// send the scrubbed file back to HS. After file
    /// transfer is complete, the TCPlusFinalizer reverts 
    /// the FilePath to the original location so as not to
    /// disrupt other processes
    /// 
    /// WARNING: Microsoft.Data.Odbc is not compatible with x64 architecture. 
    /// It doesn't appear MS has any plans to release a 64bit Odbc driver
    /// </summary>
    class TCPlusExecute : BaseExecute
    {
        private SysLog logger = new SysLog(typeof(TCPlusExecute));
        OdbcConnection conn;
        OdbcCommand updateCmd;
        String workPath = Application.StartupPath + "\\TCPlusFiles";
        String tcpPath;
        String fileName = @"EMPLLIST.DBF";
            
        public override void Execute(bool map)
		{
            logger.Log("Executing TCPlusExecute");
			logger.Debug("Executing TCPlusExecute");
			RunCommand(null);
		}

        public override void RunCommand(Command cmd)
		{
			logger.Debug("Running TCPlusExecute");
			ScrubEmplList();
		}

        /*
         * Locates EMPLLIST.DBF, copies to a temp directory, and replaces SSN values with 111-11-1111
         */
        private void ScrubEmplList()
        {
            logger.Debug("Begin ScrubEmplList");

            tcpPath = ClientDetails.Dsn.ToString();

            String pathFq = tcpPath + "\\" + fileName;
            String path2Fq = workPath + "\\" + fileName;

            CheckFileExistence(path2Fq);

            try
            {
                // Copy EMPLLIST.DBF to temp directory
                File.Copy(pathFq, path2Fq);
                logger.Debug(pathFq + " copied to " + path2Fq);

                //establish connection to DBF file using Odbc
                //don't use ClientDetails.getConnectionString() because we want to 
                //connect to workPath to work on the temp file, not TCP.dsn where the original resides
                conn = new OdbcConnection("Driver={Microsoft dBASE Driver (*.dbf)};DriverID=277;Dbq=" + workPath + ";");

                //prepare the Update statement
                updateCmd = new OdbcCommand("UPDATE " + fileName + " AS F SET F.SSN = '111-11-1111';", conn);

                conn.Open();
                logger.Debug("Connection to DBF established");

                int recordsAffected = updateCmd.ExecuteNonQuery();
                logger.Debug("Num Records updated in data scrub: " + recordsAffected);

                ModifyFilePathContaining(fileName);

                logger.Debug("Finished TCPlusExecute");
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            finally
            {
                conn.Close();
            }
        }

        /*
         * --INTENTIONAL SIDE EFFECT--
         * Uses a shallow copy of ClientDetails.file.FilePaths to temporarily modify
         * the path of EMPLLIST.DBF to the temporary location where the edited file
         * resides.
         */
        private bool ModifyFilePathContaining(string containedString)
        {
            foreach (FileForTransfer file in ClientDetails.Files)
            {
                if (file.FilePath != null)
                    if (file.FilePath.ToLower().IndexOf(containedString.ToLower()) >= 0)
                    {
                        file.FilePath = file.FilePath.Replace(tcpPath, workPath);
                        return true;
                    }
                if (file.FilePaths != null && file.FilePaths.Count > 0)
                {
                    int index = 0;
                    ArrayList fpShallowCopy = file.FilePaths;
                    foreach (String filePath in file.FilePaths)
                    {
                        if (filePath.ToLower().IndexOf(containedString.ToLower()) >= 0)
                        {
                            fpShallowCopy[index] = workPath + "\\" + fileName;
                            return true;
                        }
                        index++;
                    }
                }
            }
            return false;
        }

        /*
         * Ensures the temporary directory exists, and deletes the last synced file if it is present there
         */
        private void CheckFileExistence(string file)
        {
            if (!Directory.Exists(workPath))
            {
                Directory.CreateDirectory(workPath);
                logger.Debug("Created directory at temp location, " + workPath);
            }

            if (File.Exists(file))
            {
                File.Delete(file);
                logger.Debug("Deleted file at temp location, " + file);
            }
        }
    }
}
