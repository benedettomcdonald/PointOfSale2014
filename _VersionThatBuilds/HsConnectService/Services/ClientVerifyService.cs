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
	[System.Web.Services.WebServiceBindingAttribute(Name="ClientSettingsServiceSoapBinding", Namespace="useMainURL" )]
    
	public class ClientVerifyService : System.Web.Services.Protocols.SoapHttpClientProtocol 
	{
		public ClientVerifyService()
        {
            Boolean useSSL = Properties.UseSSL;
            if (useSSL)
            {
                this.Url = "https://" + Properties.BaseURL + "/hsws/services/ClientVerifyService";
            }
            else
            {
                this.Proxy = new System.Net.WebProxy("http://" + Properties.BaseURL + ":8020");
                this.Url = "http://" + Properties.BaseURL + ":8020/hsws/services/ClientVerifyService";
            }
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="useMainURL", ResponseNamespace="useMainURL")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public int getClientId( String macAddress )
		{
			object[] results = this.Invoke("getClientId", new object[] {macAddress});
			return ((int)(results[0]));
		}

        [SoapRpcMethodAttribute("", RequestNamespace = "useMainURL", ResponseNamespace = "useMainURL")]
        [return: SoapElementAttribute("return")]
        public int getClientId2(long concept, long unit)
        {
            object[] results = Invoke("getClientId2", new object[] {concept, unit});
            return (int) results[0];
        }

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="useMainURL", ResponseNamespace="useMainURL")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public int checkLogin( String username , String password , String macAddress )
		{
			object[] results = this.Invoke("checkLogin", new object[] {
																		  username,
																		  password,
																		  macAddress});
			return ((int)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="useMainURL", ResponseNamespace="useMainURL")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public String checkLogin2( String username , String password )
		{
			object[] results = this.Invoke("checkLogin2", new object[] {
																		   username,
																		   password});
			return ((String)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="useMainURL", ResponseNamespace="useMainURL")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public int saveMacAddress( int clientId , String macAddress )
		{
			object[] results = this.Invoke("saveMacAddress", new object[] {
																			  clientId,
																			  macAddress});
			return ((int)(results[0]));
		}
	}

}