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
	[System.Web.Services.WebServiceBindingAttribute(Name="ScheduleServiceSoapBinding", Namespace="http://soap.hotschedules.com:8020/hsws/services/ScheduleService" )]
    
	public class ScheduleService : System.Web.Services.Protocols.SoapHttpClientProtocol 
	{
		public ScheduleService()
		{
			this.Proxy = new System.Net.WebProxy( HsSettings.PROXY_ADDRESS );
			this.Url =  "http://soap.hotschedules.com:8020/hsws/services/ScheduleService";
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/ScheduleService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/ScheduleService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public String getSchedulesXML( int clientId, String xmlStart, String xmlEnd )
		{
			object[] results = this.Invoke("getSchedulesXML", new object[] {clientId,xmlStart,xmlEnd});
			return ((String)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/ScheduleService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/ScheduleService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public int[] getShiftTradeCounts( int clientId )
		{
			object[] results = this.Invoke("getShiftTradeCounts", new object[] {clientId});
			return ((int[])(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/ScheduleService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/ScheduleService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public String getForecastXML( int clientId, String xmlStart )
		{
			object[] results = this.Invoke("getForecastXML", new object[] {clientId,xmlStart});
			return ((String)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/ScheduleService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/ScheduleService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public int applyForecast( int clientId, String xmlString )
		{
			object[] results = this.Invoke("applyForecast", new object[] {clientId,xmlString});
			return ((int)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/ScheduleService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/ScheduleService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public String getDLMappingXML( int clientId )
		{
			object[] results = this.Invoke("getDLMappingXML", new object[] {clientId});
			return ((String)(results[0]));
		}

	}

}
