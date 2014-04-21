using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;

namespace HSCLite
{
	public class HSCLiteService : System.ServiceProcess.ServiceBase
	{
		private System.ComponentModel.Container components = null;

		public HSCLiteService()
		{
			this.AutoLog = false;
			this.CanStop = true;
			this.CanPauseAndContinue = true;
			InitializeComponent();
		}

		// The main entry point for the process
		static void Main()
		{
			System.ServiceProcess.ServiceBase.Run(new HSCLiteService() );
		}

		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			this.ServiceName = "HSCLite";
		}

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		protected override void OnStart(string[] args)
		{
			HSCLite.Run.StartService( args );
		}
 
		protected override void OnStop()
		{
			this.Dispose(true);
			HSCLite.Run.StopService();
		}
	}
}
