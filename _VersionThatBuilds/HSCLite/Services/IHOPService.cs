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
	[System.Web.Services.WebServiceBindingAttribute(Name="IHOPServiceSoapBinding", Namespace="http://soap.hotschedules.com:8020/hsws/services/IHOPService" )]
    
	public class IHOPService : Microsoft.Web.Services2.WebServicesClientProtocol 
	{
		public IHOPService()
        {
            this.Proxy = new System.Net.WebProxy(Properties.WebProxy);
			this.Url =  "http://soap.hotschedules.com:8020/hsws/services/IHOPService";
			this.RequestSoapContext.Security.Tokens.Add( new Microsoft.Web.Services2.Security.Tokens.UsernameToken( "hsc0nnect" + "hotschedIHOP", "g4m3d4y", Microsoft.Web.Services2.Security.Tokens.PasswordOption.SendHashed ));
			this.RequestSoapContext.Security.Timestamp.TtlInSeconds = 86400;
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/IHOPService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/IHOPService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public String getSchedule( int storeNumber )
		{
			object[] results = this.Invoke("getSchedule", new object[] {storeNumber});
			return ((String)results[0]);
		}
	}

}