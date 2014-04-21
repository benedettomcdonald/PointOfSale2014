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
	[System.Web.Services.WebServiceBindingAttribute(Name="ScheduleWssSoapBinding", Namespace="useMainURL" )]
    
	public class ScheduleWss : Microsoft.Web.Services2.WebServicesClientProtocol
	{
		public ScheduleWss()
        {
            Boolean useSSL = Properties.UseSSL;
            if (useSSL)
            {
                this.Url = "https://" + Properties.BaseURL + "/hsws/services/ScheduleWss";
            }
            else
            {
                this.Proxy = new System.Net.WebProxy("http://" + Properties.BaseURL + ":8020");
                this.Url = "http://" + Properties.BaseURL + ":8020/hsws/services/ScheduleWss";
            }
			this.RequestSoapContext.Security.Tokens.Add( HsSettings.HsToken );
			this.RequestSoapContext.Security.Timestamp.TtlInSeconds = 86400;
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="useMainURL", ResponseNamespace="useMainURL")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public String getSchedulesXML( int clientId, String xmlStart, String xmlEnd )
		{
			object[] results = this.Invoke("getSchedulesXML", new object[] {clientId,xmlStart,xmlEnd});
			return ((String)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="useMainURL", ResponseNamespace="useMainURL")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public int[] getShiftTradeCounts( int clientId )
		{
			object[] results = this.Invoke("getShiftTradeCounts", new object[] {clientId});
			return ((int[])(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="useMainURL", ResponseNamespace="useMainURL")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public String getForecastXML( int clientId, String xmlStart )
		{
			object[] results = this.Invoke("getForecastXML", new object[] {clientId,xmlStart});
			return ((String)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="useMainURL", ResponseNamespace="useMainURL")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public int applyForecast( int clientId, String xmlString )
		{
			object[] results = this.Invoke("applyForecast", new object[] {clientId,xmlString});
			return ((int)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="useMainURL", ResponseNamespace="useMainURL")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public String getDLMappingXML( int clientId )
		{
			object[] results = this.Invoke("getDLMappingXML", new object[] {clientId});
			return ((String)(results[0]));
		}

        [System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace = "useMainURL", ResponseNamespace = "useMainURL")]
        [return: System.Xml.Serialization.SoapElementAttribute("return")]
        public String getFullSchedulesXML(int clientId, String xmlStart, String xmlEnd)
        {
            object[] results = this.Invoke("getFullSchedulesXML", new object[] { clientId, xmlStart, xmlEnd });
            return ((String)(results[0]));
        }

	}

}
