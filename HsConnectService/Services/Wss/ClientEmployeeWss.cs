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
	[System.Web.Services.WebServiceBindingAttribute(Name="ClientEmployeesWssSoapBinding", Namespace="useMainURL" )]
    
	public class ClientEmployeesWss : Microsoft.Web.Services2.WebServicesClientProtocol 
	{
		public ClientEmployeesWss()
        {
            Boolean useSSL = Properties.UseSSL;
            if (useSSL)
            {
                this.Url = "https://" + Properties.BaseURL + "/hsws/services/ClientEmployeesWss";
            }
            else
            {
                this.Proxy = new System.Net.WebProxy("http://" + Properties.BaseURL + ":8020");
                this.Url = "http://" + Properties.BaseURL + ":8020/hsws/services/ClientEmployeesWss";
            }
			this.RequestSoapContext.Security.Tokens.Add( HsSettings.HsToken );
			this.RequestSoapContext.Security.Timestamp.TtlInSeconds = 86400;
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="useMainURL", ResponseNamespace="useMainURL")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public String getClientEmployees( int clientId )
		{
			object[] results = this.Invoke("getClientEmployees", new object[] {clientId});
			return ((String)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="useMainURL", ResponseNamespace="useMainURL")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public int syncUserIds( int clientId , String xmlString )
		{
			object[] results = this.Invoke("syncUserIds", new object[] {clientId , xmlString});
			return ((int)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="useMainURL", ResponseNamespace="useMainURL")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public int syncUserUpdateStatus( int clientId , String xmlString )
		{
			object[] results = this.Invoke("syncUserUpdateStatus", new object[] {clientId , xmlString});
			return ((int)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="useMainURL", ResponseNamespace="useMainURL")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public int syncUserJobs( int clientId , String xmlString )
		{
			object[] results = this.Invoke("syncUserJobs", new object[] {clientId , xmlString});
			return ((int)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="useMainURL", ResponseNamespace="useMainURL")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public int syncEmployeeUpdates( int clientId , String xmlString )
		{
			object[] results = this.Invoke("syncEmployeeUpdates", new object[] {clientId , xmlString});
			return ((int)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="useMainURL", ResponseNamespace="useMainURL")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public int syncEmployeeInserts( int clientId , String xmlString )
		{
			object[] results = this.Invoke("syncEmployeeInserts", new object[] {clientId , xmlString});
			return ((int)(results[0]));
		}

        [System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace = "useMainURL", ResponseNamespace = "useMainURL")]
        [return: System.Xml.Serialization.SoapElementAttribute("return")]
        public String getAdjustEmployees(int clientId, string xmlString)
        {
            object[] results = this.Invoke("getAdjustEmployees", new object[] { clientId, xmlString });
            return ((String)(results[0]));
        }

        [System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace = "useMainURL", ResponseNamespace = "useMainURL")]
        [return: System.Xml.Serialization.SoapElementAttribute("return")]
        public String getClientEmpWaivers(int clientId)
        {
            object[] results = this.Invoke("getClientEmpWaivers", new object[] { clientId });
            return ((String)(results[0]));
        }

        [System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace = "useMainURL", ResponseNamespace = "useMainURL")]
        [return: System.Xml.Serialization.SoapElementAttribute("return")]
        public String syncGoHireEmployees(int clientId)
        {
            object[] results = this.Invoke("syncGoHireEmployees", new object[] { clientId });
            return ((String)(results[0]));
        }

        [System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace = "useMainURL", ResponseNamespace = "useMainURL")]
        [return: System.Xml.Serialization.SoapElementAttribute("return")]
        public int checkGoHireEmployeeInsertStatuses(int clientId, string xmlString)
        {
            object[] results = this.Invoke("checkGoHireEmployeeInsertStatuses", new object[] { clientId, xmlString });
            return ((int)(results[0]));
        }
	}

}