using System;
using HsSharedObjects.Client;
using HsSharedObjects.Main;
using System.Runtime.Serialization;

namespace HsExecuteModule
{
	/// <summary>
	/// Summary description for ExecuteManager.
	/// </summary>
	[Serializable]
	public class ExecuteManager
	{
		public ExecuteManager( ClientDetails details )
		{
			this.details = details;
			logger = new SysLog(this.GetType());
		}

		private ClientDetails details;
		private SysLog logger;

		public Execute GetPosExecuteClass()
		{
			Execute execute = null;
			try
			{
				Type type = Type.GetType( "HsExecuteModule.posList." + details.PosName + "Execute" );
                if (type != null)
                {
                    execute = (Execute) Activator.CreateInstance(type);
                    execute.ClientDetails = details;
                }
            }
			catch(Exception ex)
			{
				logger.Error(ex.ToString());
			}
			return execute == null ? new BaseExecute() : execute;
		}

	}
}
