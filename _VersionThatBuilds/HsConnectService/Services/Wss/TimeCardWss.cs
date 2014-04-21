using HsConnect.Main;

using System;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.Web.Services;
using HsProperties;
using Microsoft.Web.Services2;

namespace HsConnect.Services.Wss
{

	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Web.Services.WebServiceBindingAttribute(Name="TimeCardWssSoapBinding", Namespace="useMainURL" )]
    
	public class TimeCardWss : Microsoft.Web.Services2.WebServicesClientProtocol 
	{
		public TimeCardWss()
        {
            Boolean useSSL = Properties.UseSSL;
            if (useSSL)
            {
                this.Url = "https://" + Properties.BaseURL + "/hsws/services/TimeCardWss";
            }
            else
            {
                this.Proxy = new System.Net.WebProxy("http://" + Properties.BaseURL + ":8020");
                this.Url = "http://" + Properties.BaseURL + ":8020/hsws/services/TimeCardWss";
            }
			this.RequestSoapContext.Security.Tokens.Add( HsSettings.HsToken );
			this.RequestSoapContext.Security.Timestamp.TtlInSeconds = 86400;
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="useMainURL", ResponseNamespace="useMainURL")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public int updateDropTimeCards( int clientId , String xmlString, bool dropCards, bool createIds )
		{
			object[] results = this.Invoke("updateDropTimeCards", new object[] {clientId , xmlString, dropCards, createIds});
			return ((int)(results[0]));
		}

        [System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace = "useMainURL", ResponseNamespace = "useMainURL")]
        [return: System.Xml.Serialization.SoapElementAttribute("return")]
        public int updateTimeCards( int clientId, String xmlString, bool dropCards, bool createIds, bool calculateOvertime )
        {
            object[] results = this.Invoke("updateTimeCards", new object[] { clientId, xmlString, dropCards, createIds, calculateOvertime });
            return ((int)(results[0]));
        }
	}

}
