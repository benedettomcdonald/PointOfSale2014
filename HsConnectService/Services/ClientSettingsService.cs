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
	[System.Web.Services.WebServiceBindingAttribute(Name="ClientSettingsServiceSoapBinding", Namespace="http://soap.hotschedules.com:8020/hsws/services/ClientSettingsService" )]
    
	public class ClientSettingsService : System.Web.Services.Protocols.SoapHttpClientProtocol 
	{
		public ClientSettingsService()
		{
			this.Proxy = new System.Net.WebProxy( "http://soap.hotschedules.com:8020" );
			this.Url =  "http://soap.hotschedules.com:8020/hsws/services/ClientSettingsService";
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientSettingsService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientSettingsService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public int getClientId( String macAddress )
		{
			object[] results = this.Invoke("getClientId", new object[] {macAddress});
			return ((int)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientSettingsService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientSettingsService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public int checkLogin( String username , String password , String macAddress )
		{
			object[] results = this.Invoke("checkLogin", new object[] {
																		  username,
																		  password,
																		  macAddress});
			return ((int)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientSettingsService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientSettingsService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public String getClientDetails( int clientId , String version )
		{
			object[] results = this.Invoke("getClientDetails", new object[] {clientId,version});
			return ((String)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientSettingsService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientSettingsService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public void resetForce( int clientId , int modId )
		{
			object[] results = this.Invoke("resetForce", new object[] {clientId,modId});
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientSettingsService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientSettingsService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public int updateUseWizard( int clientId , int val )
		{
			object[] results = this.Invoke("updateUseWizard", new object[] {clientId , val});
			return ((int)(results[0]));
		}
	}

}