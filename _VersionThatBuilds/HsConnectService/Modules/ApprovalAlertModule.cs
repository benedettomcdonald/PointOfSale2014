using HsSharedObjects.Client;
using HsSharedObjects.Client.Module;
using HsConnect.Main;
using HsConnect.Services;
using HsConnect.Services.Wss;
using HsConnect.Forms;

using System;
using System.Xml;

namespace HsConnect.Modules
{
	public class ApprovalAlertModule : ModuleImpl
	{
		public ApprovalAlertModule()
		{
			this.logger = new SysLog( this.GetType() );
		}

		private SysLog logger;

		public override bool Execute()
		{
			if( Details.ModuleList.IsActive( ClientModule.APPROVAL_ALERT ) )
			{
				logger.Debug( "executing ApprovalAlertModule" );
				ScheduleWss schedService = new ScheduleWss();
				int[] tradeCounts = schedService.getShiftTradeCounts( this.Details.ClientId );
				int simpleTrades = tradeCounts[0];
				int shiftSwaps = tradeCounts[1];
				if( simpleTrades > 0 || shiftSwaps > 0 )
				{
					ApprovalForm appForm = new ApprovalForm();
                    appForm.SimpleTrades = simpleTrades;
					appForm.ShiftSwaps = shiftSwaps;					
					appForm.Display();
				}
			}
			logger.Debug( "executed ApprovalAlertModule" );
			return true;
		}
	}
}
