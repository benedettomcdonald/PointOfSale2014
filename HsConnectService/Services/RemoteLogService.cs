using HsConnect.Main;

using System;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.Web.Services;
using HsProperties;

namespace HsConnect.Services
{

	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Web.Services.WebServiceBindingAttribute(Name="ClientSalesServiceSoapBinding", Namespace="useMainURL" )]
    
	public class RemoteLogService : System.Web.Services.Protocols.SoapHttpClientProtocol 
	{
		public RemoteLogService()
        {
            Boolean useSSL = Properties.UseSSL;
            if (useSSL)
            {
                this.Url = "https://" + Properties.BaseURL + "/hsws/services/RemoteLogService";
            }
            else
            {
                this.Proxy = new System.Net.WebProxy("http://" + Properties.BaseURL + ":8020");
                this.Url = "http://" + Properties.BaseURL + ":8020/hsws/services/RemoteLogService";
            }
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="useMainURL", ResponseNamespace="useMainURL")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public void remoteLogItem( int clientId , int logCode, String xmlString )
		{
			object[] results = this.Invoke("remoteLogItem", new object[] {clientId , logCode, xmlString});
		}
	}

}
