using System;
using System.Text;
using HsSharedObjects.Main;
using HsFileTransfer;
using System.Windows.Forms;
using System.Collections;

namespace HsExecuteModule.finalizers
{
    /// <summary>
    /// Summary description for TCPlusFinalize.
    /// Reverts ClientDetails.files.FilePath for EMPLLIST.DBF to
    /// its original value
    /// </summary>
    [Serializable]
    class TCPlusFinalize : BaseFinalize
    {
        private SysLog logger = new SysLog(typeof(TCPlusFinalize));
        String workPath = Application.StartupPath + "\\TCPlusFiles";
        String tcpPath;
        String fileName = @"EMPLLIST.DBF";

        public override void PerformFinalize()
        {
            base.PerformFinalize();
            logger.Log("Executing TCPlusFinalize");
            logger.Debug("Now in TCPlusFinalize.");

            tcpPath = ClientDetails.Dsn.ToString();

            // The modified file has been transferred from its temp location. Time to revert its filepath to the original file's location
            RevertFilePathContaining(fileName);
        }
        
        private bool RevertFilePathContaining(string containedString)
        {
            foreach (FileForTransfer file in ClientDetails.Files)
            {
                if (file.FilePath != null)
                    if (file.FilePath.ToLower().IndexOf(containedString.ToLower()) >= 0)
                    {
                        file.FilePath = file.FilePath.Replace(workPath, tcpPath);
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
                            fpShallowCopy[index] = tcpPath + "\\" + fileName;
                            return true;
                        }
                        index++;
                    }
                }
            }
            return false;
        }
    }
}
