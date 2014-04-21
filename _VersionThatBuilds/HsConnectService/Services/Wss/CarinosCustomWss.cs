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
	[System.Web.Services.WebServiceBindingAttribute(Name="ClientSalesServiceSoapBinding", Namespace="useMainURL" )]
    
	public class CarinosCustomWss : Microsoft.Web.Services2.WebServicesClientProtocol  
	{
		public CarinosCustomWss()
		{
            Boolean useSSL = Properties.UseSSL;
            if (useSSL)
            {
                this.Url = "https://" + Properties.BaseURL + "/hsws/services/CarinosCustomWss";
            }
            else
            {
                this.Proxy = new System.Net.WebProxy("http://" + Properties.BaseURL + ":8020");
                this.Url = "http://" + Properties.BaseURL + ":8020/hsws/services/CarinosCustomWss";
            }
			this.RequestSoapContext.Security.Tokens.Add( HsSettings.HsToken );
			this.RequestSoapContext.Security.Timestamp.TtlInSeconds = 3600;
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="useMainURL", ResponseNamespace="useMainURL")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public int insertGcItems( int clientId , String xmlString )
		{
			object[] results = this.Invoke("insertGcItems", new object[] {clientId , xmlString});
			return ((int)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="useMainURL", ResponseNamespace="useMainURL")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public String getWeekCounts( int clientId )
		{
			object[] results = this.Invoke("getWeekCounts", new object[] {clientId});
			return ((String)(results[0]));
		}
	}

}