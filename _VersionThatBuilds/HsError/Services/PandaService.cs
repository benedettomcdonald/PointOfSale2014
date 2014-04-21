using System;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.Web.Services;
using HsProperties;

namespace HsError.Services
{

	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Web.Services.WebServiceBindingAttribute(Name="PandaServiceSoapBinding", Namespace="useMainURL" )]
    
	public class PandaService : Microsoft.Web.Services2.WebServicesClientProtocol 
	{
		public PandaService()
		{
            
            Boolean useSSL = Properties.UseSSL;
            if (useSSL)
            {
                this.Url = "https://" + Properties.BaseURL + "/hsws/services/PandaService";
            }
            else
            {
                this.Proxy = new System.Net.WebProxy(Properties.WebProxy);
                this.Url = "http://" + Properties.BaseURL + ":8020/hsws/services/PandaService";
            }
			this.RequestSoapContext.Security.Tokens.Add( HsSharedObjects.Main.HsSettings.HsToken );
			this.RequestSoapContext.Security.Timestamp.TtlInSeconds = 86400;
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="useMainURL", ResponseNamespace="useMainURL")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public int method1(String p1/*errors in xml format*/ , String p2/*service/method name*/, bool p3/*status*/)
		{
			object[] results = this.Invoke("method1", new object[] {p1,p2,p3});
			return ((int)(results[0]));
		}
	}

}
