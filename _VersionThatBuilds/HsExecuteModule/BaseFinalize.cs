using System;
using System.Text;
using System.Runtime.Serialization;
using HsSharedObjects.Main;
using HsSharedObjects.Client;

namespace HsExecuteModule
{
    /// <summary>
    /// Summary description for BaseFinalize.
    /// </summary>
    [Serializable]
    class BaseFinalize : Finalize
    {
        protected SysLog logger;
        private ClientDetails clientDetails;

        public BaseFinalize()
		{
			logger = new SysLog( this.GetType() );
		}

        public virtual void PerformFinalize()
        {
            logger.Debug("In BaseFinalize -- noop.");
        }

        public String GetXmlString() { return ""; }

        public ClientDetails ClientDetails
        {
            get { return clientDetails; }
            set { clientDetails = value; }
        }
    }
}
