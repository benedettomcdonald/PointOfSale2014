using System;
using System.Diagnostics;
using HsSharedObjects.Main;

namespace HsSharedObjects.Machine
{
	/// <summary>
	/// Summary description for Process.
	/// </summary>
	public class HsProcess
	{
	    private SysLog logger = new SysLog(typeof (HsProcess));

		public HsProcess( String name )
		{
			Process[] p = Process.GetProcessesByName( name );
			if ( p.Length > 1 ) running = true;
		}
		
		public HsProcess()
		{
			
		}

		private bool running = false;

		public bool Running
		{
			get { return running; }
			set { running = value; }
		}

		public bool ExecCmd( String dir, String process, String args )
		{
			System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.WorkingDirectory = dir;
            logger.Debug("Set working directory to " + proc.StartInfo.WorkingDirectory);
			proc.EnableRaisingEvents=false;
			proc.StartInfo.FileName=process;
			proc.StartInfo.Arguments=args;
			proc.Start();
			proc.WaitForExit();
			return true;
		}
	}
}
