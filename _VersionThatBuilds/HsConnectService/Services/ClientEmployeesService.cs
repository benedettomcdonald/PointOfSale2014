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
	[System.Web.Services.WebServiceBindingAttribute(Name="ClientEmployeesServiceSoapBinding", Namespace="http://soap.hotschedules.com:8020/hsws/services/ClientEmployeesService" )]
    
	public class ClientEmployeesService : System.Web.Services.Protocols.SoapHttpClientProtocol 
	{
		public ClientEmployeesService()
		{
			this.Proxy = new System.Net.WebProxy( HsSettings.PROXY_ADDRESS );
			this.Url =  "http://soap.hotschedules.com:8020/hsws/services/ClientEmployeesService";
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientEmployeesService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientEmployeesService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public String getClientEmployees( int clientId )
		{
			object[] results = this.Invoke("getClientEmployees", new object[] {clientId});
			return ((String)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientEmployeesService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientEmployeesService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public int syncUserIds( int clientId , String xmlString )
		{
			object[] results = this.Invoke("syncUserIds", new object[] {clientId , xmlString});
			return ((int)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientEmployeesService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientEmployeesService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public int syncUserUpdateStatus( int clientId , String xmlString )
		{
			object[] results = this.Invoke("syncUserUpdateStatus", new object[] {clientId , xmlString});
			return ((int)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientEmployeesService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientEmployeesService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public int syncUserJobs( int clientId , String xmlString )
		{
			object[] results = this.Invoke("syncUserJobs", new object[] {clientId , xmlString});
			return ((int)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientEmployeesService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientEmployeesService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public int syncEmployeeUpdates( int clientId , String xmlString )
		{
			object[] results = this.Invoke("syncEmployeeUpdates", new object[] {clientId , xmlString});
			return ((int)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientEmployeesService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientEmployeesService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public int syncEmployeeInserts( int clientId , String xmlString )
		{
			object[] results = this.Invoke("syncEmployeeInserts", new object[] {clientId , xmlString});
			return ((int)(results[0]));
		}
	}

}
