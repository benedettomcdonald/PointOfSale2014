using System;
using System.Collections;

namespace HsFileTransfer
{
	/// <summary>
	/// Summary description for FileForTransfer.
	/// </summary>
	
	[Serializable]
	public class FileForTransfer
	{
		private String filePath;
		private int procRef;
		private ArrayList filePaths;

		public FileForTransfer()
		{
		}
		public String FilePath
		{
			get{ return filePath; }
			set{ this.filePath = value; } 
		}
		public int ProcRef
		{
			get{ return procRef; }
			set{ this.procRef = value; }
		}
		public ArrayList FilePaths
		{
			get{ return filePaths; }
			set{ filePaths = value; }
		}
	}
}
