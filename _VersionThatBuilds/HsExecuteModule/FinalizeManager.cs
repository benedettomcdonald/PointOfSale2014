using System;
using System.Text;
using HsSharedObjects.Client;
using HsSharedObjects.Main;

namespace HsExecuteModule
{
    /// <summary>
    /// Summary description for FinalizeManager.
    /// </summary>
    [Serializable]
    public class FinalizeManager
    {
        private ClientDetails details;
        private SysLog logger;

        public FinalizeManager(ClientDetails details)
        {
            this.details = details;
            logger = new SysLog(this.GetType());
        }

        public Finalize GetPosFinalizeClass()
        {
            Finalize finalize = null;
            try
            {
                Type type = Type.GetType("HsExecuteModule.finalizers." + details.PosName + "Finalize");
                if (type != null)
                {
                    finalize = (Finalize)Activator.CreateInstance(type);
                    finalize.ClientDetails = details;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            return finalize == null ? new BaseFinalize() : finalize;
        }

    }
}

