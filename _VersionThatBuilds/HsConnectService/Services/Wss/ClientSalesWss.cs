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
	[System.Web.Services.WebServiceBindingAttribute(Name="ClientSalesWssSoapBinding", Namespace="useMainURL" )]
    
	public class ClientSalesWss : Microsoft.Web.Services2.WebServicesClientProtocol 
	{
		public ClientSalesWss()
        {
            Boolean useSSL = Properties.UseSSL;
            if (useSSL)
            {
                this.Url = "https://" + Properties.BaseURL + "/hsws/services/ClientSalesWss";
            }
            else
            {
                this.Proxy = new System.Net.WebProxy("http://" + Properties.BaseURL + ":8020");
                this.Url = "http://" + Properties.BaseURL + ":8020/hsws/services/ClientSalesWss";
            }
			this.RequestSoapContext.Security.Tokens.Add( HsSettings.HsToken );
			this.RequestSoapContext.Security.Timestamp.TtlInSeconds = 86400;
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="useMainURL", ResponseNamespace="useMainURL")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public String getSalesEndDate( int clientId )
		{
			object[] results = this.Invoke("getSalesEndDate", new object[] {clientId});
			return ((String)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="useMainURL", ResponseNamespace="useMainURL")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public int insertSalesItems( int clientId , String xmlString )
		{
			object[] results = this.Invoke("insertSalesItems", new object[] {clientId , xmlString});
			return ((int)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="useMainURL", ResponseNamespace="useMainURL")]
        [return: System.Xml.Serialization.SoapElementAttribute( "return" )]
        public int insertSalesItemsWithMicrosRVC(int clientId, String xmlString)
        {
            object[] results = this.Invoke("insertSalesItemsWithMicrosRVC", new object[] { clientId, xmlString });
            return ((int)(results[0]));
        }

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="useMainURL", ResponseNamespace="useMainURL")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public int compareSalesItems( int clientId , String xmlString )
		{
			object[] results = this.Invoke("compareSalesItems", new object[] {clientId , xmlString});
			return ((int)(results[0]));
		}
        
        [System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace = "useMainURL", ResponseNamespace = "useMainURL")]
        [return: System.Xml.Serialization.SoapElementAttribute("return")]
        public int completeHistoricalSync(int clientId)
        {
            object[] results = this.Invoke("completeHistoricalSync", new object[] { clientId });
            return ((int)(results[0]));
        }

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="useMainURL", ResponseNamespace="useMainURL")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public float getSalesTotal( int clientId , String dateRange )
		{
			object[] results = this.Invoke("getSalesTotal", new object[] {clientId, dateRange});
			return ((float)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="useMainURL", ResponseNamespace="useMainURL")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public String getWeekSales( int clientId )
		{
			object[] results = this.Invoke("getWeekSales", new object[] {clientId});
			return ((String)(results[0]));
		}
	}

}