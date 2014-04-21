using System;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Web.Services.Protocols;
using System.ComponentModel;
using HsProperties;
using Microsoft.Web.Services2;

namespace HSCLite.Services
{

	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Web.Services.WebServiceBindingAttribute(Name="CheckinServiceSoapBinding", Namespace="http://soap.hotschedules.com:8020/hsws/services/CheckinService" )]
    
	public class CheckinService : Microsoft.Web.Services2.WebServicesClientProtocol 
	{
		public CheckinService()
        {
            this.Proxy = new System.Net.WebProxy(Properties.WebProxy);
			this.Url =  "http://soap.hotschedules.com:8020/hsws/services/CheckinService";
			this.RequestSoapContext.Security.Tokens.Add( new Microsoft.Web.Services2.Security.Tokens.UsernameToken( "hsc0nnect" + "hotschedIHOP", "g4m3d4y", Microsoft.Web.Services2.Security.Tokens.PasswordOption.SendHashed ));
			this.RequestSoapContext.Security.Timestamp.TtlInSeconds = 86400;
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/CheckinService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/CheckinService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public void CheckIn( int storeNumber, long ticks, bool success )
		{
			object[] results = this.Invoke("checkIn", new object[] {storeNumber, ticks, success});
		}
	}

}