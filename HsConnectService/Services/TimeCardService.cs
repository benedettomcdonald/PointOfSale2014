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
	[System.Web.Services.WebServiceBindingAttribute(Name="TimeCardServiceSoapBinding", Namespace="http://soap.hotschedules.com:8020/hsws/services/TimeCardService" )]
    
	public class TimeCardService : System.Web.Services.Protocols.SoapHttpClientProtocol 
	{
		public TimeCardService()
		{
			this.Proxy = new System.Net.WebProxy( HsSettings.PROXY_ADDRESS );
			this.Url =  "http://soap.hotschedules.com:8020/hsws/services/TimeCardService";
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/TimeCardService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/TimeCardService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public int updateTimeCards( int clientId , String xmlString )
		{
			object[] results = this.Invoke("updateTimeCards", new object[] {clientId , xmlString});
			return ((int)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/TimeCardService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/TimeCardService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public int updateDropTimeCards( int clientId , String xmlString, bool dropCards, bool createIds )
		{
			object[] results = this.Invoke("updateDropTimeCards", new object[] {clientId , xmlString, dropCards, createIds});
			return ((int)(results[0]));
		}
	}

}
