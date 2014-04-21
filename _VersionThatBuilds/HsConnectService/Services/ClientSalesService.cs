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
	[System.Web.Services.WebServiceBindingAttribute(Name="ClientSalesServiceSoapBinding", Namespace="http://soap.hotschedules.com:8020/hsws/services/ClientSalesService" )]
    
	public class ClientSalesService : System.Web.Services.Protocols.SoapHttpClientProtocol 
	{
		public ClientSalesService()
		{
			this.Proxy = new System.Net.WebProxy( HsSettings.PROXY_ADDRESS );
			this.Url =  "http://soap.hotschedules.com:8020/hsws/services/ClientSalesService";
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientSalesService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientSalesService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public String getSalesEndDate( int clientId )
		{
			object[] results = this.Invoke("getSalesEndDate", new object[] {clientId});
			return ((String)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientSalesService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientSalesService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public int insertSalesItems( int clientId , String xmlString )
		{
			object[] results = this.Invoke("insertSalesItems", new object[] {clientId , xmlString});
			return ((int)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientSalesService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientSalesService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public int compareSalesItems( int clientId , String xmlString )
		{
			object[] results = this.Invoke("compareSalesItems", new object[] {clientId , xmlString});
			return ((int)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientSalesService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientSalesService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public float getSalesTotal( int clientId , String dateRange )
		{
			object[] results = this.Invoke("getSalesTotal", new object[] {clientId, dateRange});
			return ((float)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientSalesService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/ClientSalesService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public String getWeekSales( int clientId )
		{
			object[] results = this.Invoke("getWeekSales", new object[] {clientId});
			return ((String)(results[0]));
		}
	}

}