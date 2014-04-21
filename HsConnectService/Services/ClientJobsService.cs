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
	[System.Web.Services.WebServiceBindingAttribute(Name="ClientJobsServiceSoapBinding", Namespace="http://soap.hotschedules.com:8020/hsws/services/ClientJobsService" )]
    
	public class ClientJobsService : System.Web.Services.Protocols.SoapHttpClientProtocol 
	{
		public ClientJobsService()
		{
			this.Proxy = new System.Net.WebProxy( HsSettings.PROXY_ADDRESS );
			this.Url =  "http://soap.hotschedules.com:8020/hsws/services/ClientJobsService";
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientJobsService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientJobsService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public String getClientJobs( int clientId )
		{
			object[] results = this.Invoke("getClientJobs", new object[] {clientId});
			return ((String)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientJobsService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientJobsService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public String getClientSchedules( int clientId )
		{
			object[] results = this.Invoke("getClientSchedules", new object[] {clientId});
			return ((String)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientJobsService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientJobsService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public int syncClientJobs( int clientId , String xmlString )
		{
			object[] results = this.Invoke("syncClientJobs", new object[] {clientId , xmlString});
			return ((int)(results[0]));
		}
	}

}
