using HsConnect.Main;

using System;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Web.Services.Protocols;
using System.ComponentModel;
using HsProperties;
using Microsoft.Web.Services2;

namespace HsConnect.Services.Wss
{

	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Web.Services.WebServiceBindingAttribute(Name="ClientSettingsServiceSoapBinding", Namespace="useMainURL" )]
    
	public class ClientSettingsWss : Microsoft.Web.Services2.WebServicesClientProtocol 
	{
		public ClientSettingsWss()
        {
            Boolean useSSL = Properties.UseSSL;
            if (useSSL)
            {
                this.Url = "https://" + Properties.BaseURL + "/hsws/services/ClientSettingsWss";
            }
            else
            {
                this.Proxy = new System.Net.WebProxy("http://" + Properties.BaseURL + ":8020");
                this.Url = "http://" + Properties.BaseURL + ":8020/hsws/services/ClientSettingsWss";
            }
			//this.Url =  "useMainURL";
			this.RequestSoapContext.Security.Tokens.Add( HsSettings.HsToken );
			this.RequestSoapContext.Security.Timestamp.TtlInSeconds = 86400;
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="useMainURL", ResponseNamespace="useMainURL")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public String getClientDetails( int clientId , String version )
		{
			object[] results = this.Invoke("getClientDetails", new object[] {clientId,version});
			return ((String)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="useMainURL", ResponseNamespace="useMainURL")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public void resetForce( int clientId , int modId )
		{
			object[] results = this.Invoke("resetForce", new object[] {clientId,modId});
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="useMainURL", ResponseNamespace="useMainURL")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public int updateUseWizard( int clientId , int val )
		{
			object[] results = this.Invoke("updateUseWizard", new object[] {clientId , val});
			return ((int)(results[0]));
		}

        [System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace = "useMainURL", ResponseNamespace = "useMainURL")]
        [return: System.Xml.Serialization.SoapElementAttribute("return")]
        public void markIniAsUpdated(int clientId)
        {
            object[] results = this.Invoke("markIniAsUpdated", new object[] {clientId});
        }

        [System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace = "useMainURL", ResponseNamespace = "useMainURL")]
        [return: System.Xml.Serialization.SoapElementAttribute("return")]
        public bool reportTaxmlChecksum(int clientId, String chksum)
        {
            object[] results = this.Invoke("reportTaxmlChecksum", new object[] { clientId, chksum });
            return ((bool)(results[0]));
        }
	}

}