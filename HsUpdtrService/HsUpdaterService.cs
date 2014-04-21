using System;
using hsupdater;
using hsupdater.Setup;
using hsupdater.Logger;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;



namespace hsupdater
{
	/// <summary>
	/// Summary description for HsUpdaterService.
	/// </summary>
	public class _hsupdtr : System.ServiceProcess.ServiceBase
	{
		public _hsupdtr()
		{
			this.ServiceName = "_hsupdtr";
			this.CanStop = true;
			this.CanPauseAndContinue = true;
			this.AutoLog = false;
			InitializeComponent();
		}

		protected override void OnStart(string[] args) 
		{
			hsupdater.Run.StartService( args );
		}

		private void InitializeComponent()
		{
			// _hsupdtr
			this.ServiceName = "_hsupdtr";
		}

		protected override void OnStop()
		{
			hsupdater.Run.StopService( );		
		}

		public static void Main() 
		{
			System.ServiceProcess.ServiceBase.Run(new _hsupdtr());
		}
	}
}
