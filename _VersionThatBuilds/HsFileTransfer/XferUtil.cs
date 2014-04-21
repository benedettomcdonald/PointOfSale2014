using System;
using System.IO;
using System.Collections;
using System.Threading;
using HsSharedObjects.Main;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.GZip;
using System.Security.Cryptography;
using HsProperties;
using HsFileTransfer.HscFile;

namespace HsFileTransfer
{
	/// <summary>
	/// Summary description for XferUtil.
	/// </summary>
	public class XferUtil
	{
        private static SysLog logger = new SysLog(typeof(XferUtil));
        private static char[] sep = new char[] {'\\'};
		private static Boolean useSSL = true;
		public static void Process(FileForTransfer f, int clientId)
        {
            logger.Debug("XferUtil.Process()");
			try
			{
				useSSL = Properties.UseSSL;
			}
			catch(Exception ex)
			{
				logger.Debug("error getting UseSSL param from properties.xml.  Adding to debug, not error because this is not generally an issue");
			}
			String[] pathSplit = null;
			String newPath = "";
			if(f.FilePaths.Count > 0)
			{
				//create Zip File
				string zipPath = GetZipPathAndFiles(f);


			    string[] filenames = Directory.GetFiles(zipPath);
				pathSplit = f.FilePaths[0].ToString().Split(sep);
				f.FilePath = f.FilePaths[0].ToString();
				newPath = @"C:\hstemp\" + f.ProcRef + pathSplit[pathSplit.Length-1] + ".ZIP";
				ZipFiles(filenames, newPath);

			    Directory.Delete(zipPath, true);
			}
			else
			{
				return;
			}

			String filePath = newPath;
			SendZipFile(f, filePath, clientId);
		}

	    private static void ZipFiles(string[] filenames, string zipPath)
	    {
	        logger.Debug("ZipFiles()");
	        ZipOutputStream z = new ZipOutputStream(File.Create(zipPath));

            try
            {
                byte[] buffer = new byte[4096];

                logger.Debug("Filenames size:  " + filenames.Length);
                int x = 0;
                foreach (string zfile in filenames)
                {
                    if (zfile == null)
                    {
                        logger.Debug("zfile was null: " + x);
                    }
                    else
                    {
                        logger.Debug("File name:  " + zfile + " : " + x);
                    }
                    x++;
                    System.IO.FileInfo fi = new System.IO.FileInfo(zfile);
                    ZipEntry entry = new
                        ZipEntry(Path.GetFileName(zfile));

                    entry.DateTime = DateTime.Now;
                    entry.Size = fi.Length;
                    z.PutNextEntry(entry);

                    using (FileStream fs = File.OpenRead(zfile))
                    {
                        int sourceBytes = 0;
                        do
                        {
                            sourceBytes = fs.Read(buffer, 0,
                                                  buffer.Length);
                            z.Write(buffer, 0, sourceBytes);
                        } while (sourceBytes > 0);

                        fs.Close();
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("Error creating zip " + zipPath, e);
                throw;
            }
            finally
            {
                if (z != null)
                {
                    z.Finish();
                    z.Close();
                }
            }
	    }

	    private static string GetZipPathAndFiles(FileForTransfer fileForTransfer)
        {
            logger.Debug("GetZipPathAndFiles()");
	        string[] pathSplit;
	        String zipPath = @"C:\hstemp\zipFiles";
	        if(Directory.Exists(zipPath))
	            Directory.Delete(zipPath, true);
	        Directory.CreateDirectory(zipPath);
	        int i = 1;
	        foreach(String fileName in fileForTransfer.FilePaths)
	            CopyFile(zipPath, fileName, i);
	        return zipPath;
	    }

	    private static void CopyFile(string zipPath, string fileName, int i)
        {
            logger.Debug("CopyFile()");
	        logger.Debug("Looking for " + fileName);
	        string[] pathSplit;
	        if(File.Exists(fileName))
	        {
	            try
	            {
	                pathSplit = fileName.Split(sep);
	                String zipFilePath = zipPath + "\\" + pathSplit[pathSplit.Length-1];

	                //could possibly refactor the following "if" to a "while", which would get rid of the inner while loop I think.
	                //Kept it this way since I wanted to keep zipFilePath the same until it found a "correct" path (why I use alternatePath below), but
	                //could get around that pretty easy I suppose (could just pull alternatePath up I guess).
	                if(File.Exists(zipFilePath)) //already have a file with this name. Don't overwrite it, just "uniquify" this new file
	                {
	                    //try to append a number to the end of the file name to make it unique
	                    int index = zipFilePath.LastIndexOf('.'); //using the '.' to get us right before the file extension at the end (assumes file doesn't have multiple '.' characters)
	                    string alternatePath = zipFilePath.Insert(index, "(" + i+ ")");
	                    while(File.Exists(alternatePath)) //if for some reason there is already a file with this name too, keep trying
	                    {
	                        i++;
	                        alternatePath = zipFilePath.Insert(index, "(" + i+ ")");
	                    }
	                    zipFilePath = alternatePath;
	                    i++;
	                }
							
	                File.Copy(fileName, zipFilePath);
	                logger.Debug("Successfully copied " + fileName + " to " + zipFilePath);
	            }
	            catch(Exception ex)
	            {
	                logger.Error("Could not copy " + fileName + " to hstemp", ex);
	            }
	        }
            else
                logger.Error("Could not find " + fileName);
        }

        private static void SendZipFile(FileForTransfer fileForTransfer, string filePath, int clientId)
        {
            logger.Debug("SendZipFile()");
            FileInfo zipInfo = new FileInfo(filePath);
            long bytesLeft = zipInfo.Length;
            int proc = fileForTransfer.ProcRef;

            FileStream reader = new FileStream(filePath, System.IO.FileMode.Open);
            try
            {
                int counter = 0;
                hscFile file = new hscFile();
                file.guid = Guid.NewGuid().ToString("N");
                file.clientID = clientId;
            
                String[] split = filePath.Split(sep);
                file.fileName = split[split.Length - 1];
                file.processingRef = proc;
                double count = (double)(Math.Ceiling((double)reader.Length / (1024.0 * 64.0)));
                while (counter < count)
                {
                    byte[] a = new byte[(bytesLeft >= (1024 * 64)) ? (1024 * 64) : bytesLeft];
                    bytesLeft -= (1024 * 64);
                    ReadChunk(reader, a);
                    file.hsp = new hscPacket[1];
                    hscPacket packet = new hscPacket();
                    packet.seqNumber = counter;
                    packet.binData = System.Convert.ToBase64String(a);
                    string checksum = GetChecksum(a);
                    file.hsp[0] = packet;

                    file.id = processHSCFile(file);
                    
                   
                    counter++;
                }
                //send empty packet as end
                file.hsp = new hscPacket[1];
                hscPacket p = new hscPacket();
                p.seqNumber = -1;
                file.hsp[0] = p;
                processHSCFile(file);
                //get file data and create packets
            }
            catch(Exception e)
            {
                logger.Error("Error sending zip file " + filePath, e);
                throw;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

	    private static int ReadChunk (Stream stream, byte[] data)
		{
			int offset=0;
			int remaining = data.Length;
			int read = stream.Read(data, offset, remaining);

			return offset;
		}

        private static int processHscFile(int fileId, int clientId, String fileName, bool binComplete,
                int procRef, int seqNumber, String bytes)
        {
            FileTransferService serv = new FileTransferService();
            int fId = (int)(serv.processHSCFile(fileId, clientId, fileName, binComplete, procRef, seqNumber, bytes));
            return fId;
        }

		private static long processHSCFile(hscFile file)
		{
            file.id = processHscFile((int)file.id, file.clientID, file.fileName, file.binComplete, file.processingRef, file.hsp[0].seqNumber, file.hsp[0].binData);
            return file.id;
		}
		
		private static string GetChecksum(byte[] bytes)
		{	
			SHA256Managed sha = new SHA256Managed();
			byte[] checksum = sha.ComputeHash(bytes);
			return BitConverter.ToString(checksum).Replace("-", String.Empty);		
		}
	}
}
