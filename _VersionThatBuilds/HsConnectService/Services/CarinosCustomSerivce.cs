using HsConnect.Main;

using System;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.Web.Services;

namespace HsConnect.Services
{

	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Web.Services.WebServiceBindingAttribute(Name="ClientSalesServiceSoapBinding", Namespace="http://soap.hotschedules.com:8020/hsws/services/CarinosCustomService" )]
    
	public class CarinosCustomService : System.Web.Services.Protocols.SoapHttpClientProtocol 
	{
		public CarinosCustomService()
		{
			this.Proxy = new System.Net.WebProxy( HsSettings.PROXY_ADDRESS );
			this.Url =  "http://soap.hotschedules.com:8020/hsws/services/CarinosCustomService";
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/CarinosCustomService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/CarinosCustomService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public int insertGcItems( int clientId , String xmlString )
		{
			object[] results = this.Invoke("insertGcItems", new object[] {clientId , xmlString});
			return ((int)(results[0]));
		}
	}

}
