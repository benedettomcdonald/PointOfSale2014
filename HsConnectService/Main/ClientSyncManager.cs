using HsConnect.Modules;
using HsSharedObjects.Client;
using HsConnect.Forms;
using HsConnect.Jobs;
using HsSharedObjects.Client.Module;
using HsSharedObjects.Client.CustomModule;

using System;
using System.Timers;
using System.Threading;

namespace HsConnect.Main
{
	/// <summary>
	/// The main control for HS Connect.
	/// </summary>
	public class ClientSyncManager
	{
		public ClientSyncManager( ClientDetails details )
		{
			this.clientDetails = details;
			logger = new SysLog( this.GetType() );
		}

		private ClientDetails clientDetails;
		private SysLog logger;
		
		public void Execute()
		{
			if( clientDetails != null )
			{
				if( clientDetails.Status == ClientDetails.ACTIVE )
				{
					if( clientDetails.UseWizard == ClientDetails.WIZARD_ON )
					{
						/** Run the sync wizard, which only links POS jobs and roles by ids,
						 * as well as employees by id ONLY - this is separate from data sync 
						 **/
						//logger.Debug( "executing Wizard" );
						//Wizard wizard = new Wizard( clientDetails );
						//wizard.Show();
					}
					else SetModules();
				} 
				else if ( clientDetails.Status == ClientDetails.INACTIVE )
				{
					logger.Error( "Client is inactive!" );
				}
			}
			else
			{
				logger.Error( "ClientDetails were NULL!" );
			}
		}

		private void SetModules()
		{	
			
		}

	}
}
