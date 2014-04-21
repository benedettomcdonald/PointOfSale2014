using HsConnect.Main;

using System;
using System.Xml;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.Web.Services;
using Microsoft.Web.Services2;

namespace HsConnect.Services
{

	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Web.Services.WebServiceBindingAttribute(Name="ClientGuestCountServiceSoapBinding", Namespace="http://soap.hotschedules.com:8020/hsws/services/ClientGuestCountService" )]
    
	public class ClientGuestCountService : Microsoft.Web.Services2.WebServicesClientProtocol 
	{
		public ClientGuestCountService()
		{
			this.Proxy = new System.Net.WebProxy( HsSettings.PROXY_ADDRESS );
			this.Url =  "http://soap.hotschedules.com:8020/hsws/services/ClientGuestCountService";
			this.RequestSoapContext.Security.Tokens.Add( HsSettings.HsToken );			
		}

		private SysLog logger = new SysLog( "HsConnect.Services.ClientGuestCountService" );

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientGuestCountService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientGuestCountService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public String guestCountVerify( int clientId )
		{
			object[] results = this.Invoke("guestCountVerify", new object[] {clientId});
			return ((String)(results[0]));
		}
	}

}
