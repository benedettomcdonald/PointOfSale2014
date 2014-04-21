using HsConnect.Main;

using System;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.Web.Services;
using HsProperties;

namespace HsConnect.Services.Wss
{

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name = "PunchRecordServiceSoapBinding", Namespace = "useMainURL")]

    public class PunchRecordService : Microsoft.Web.Services2.WebServicesClientProtocol //System.Web.Services.Protocols.SoapHttpClientProtocol 
    {
        public PunchRecordService()
        {
            Boolean useSSL = Properties.UseSSL;
            if (useSSL)
            {
                this.Url = "https://" + Properties.BaseURL + "/hsws/services/PunchRecordService";
            }
            else
            {
                this.Proxy = new System.Net.WebProxy("http://" + Properties.BaseURL + ":8020");
                this.Url = "http://" + Properties.BaseURL + ":8020/hsws/services/PunchRecordService";
            }
            this.RequestSoapContext.Security.Tokens.Add(HsSettings.HsToken);
            this.RequestSoapContext.Security.Timestamp.TtlInSeconds = 3600;
        }



        [System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace = "useMainURL", ResponseNamespace = "useMainURL")]
        [return: System.Xml.Serialization.SoapElementAttribute("return")]
        public int updatePunchRecords(int clientId, String xmlString)
        {
            object[] results = this.Invoke("updatePunchRecords", new object[] { clientId, xmlString });
            return ((int)(results[0]));
        }

        [System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace = "useMainURL", ResponseNamespace = "useMainURL")]
        [return: System.Xml.Serialization.SoapElementAttribute("return")]
        public int updateDropPunchRecords(int clientId, String xmlString, bool dropCards, bool createIds)
        {
            object[] results = this.Invoke("updateDropPunchRecords", new object[] { clientId, xmlString, dropCards, createIds });
            return ((int)(results[0]));
        }

        [System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace = "useMainURL", ResponseNamespace = "useMainURL")]
        [return: System.Xml.Serialization.SoapElementAttribute("return")]
        public String getPunchRecordsForEmp(int clientId, String xmlString)
        {
            object[] results = this.Invoke("getPunchRecordsForEmp", new object[] { clientId, xmlString });
            return ((string)(results[0]));
        }

        [System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace = "useMainURL", ResponseNamespace = "useMainURL")]
        [return: System.Xml.Serialization.SoapElementAttribute("return")]
        public String checkEmployeePunches(int clientId, String xmlString)
        {
            object[] results = this.Invoke("checkEmployeePunches", new object[] { clientId, xmlString });
            return ((string)(results[0]));
        }
        [System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace = "useMainURL", ResponseNamespace = "useMainURL")]
        [return: System.Xml.Serialization.SoapElementAttribute("return")]
        public String checkDropEmployeePunches(int clientId, String xmlString, int dropCards)
        {
            object[] results = this.Invoke("checkDropEmployeePunches", new object[] { clientId, xmlString, dropCards });
            return ((string)(results[0]));
        }

        [System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace = "useMainURL", ResponseNamespace = "useMainURL")]
        [return: System.Xml.Serialization.SoapElementAttribute("return")]
        public String getAdjEmpPunches(int clientId, String xmlString)
        {
            object[] results = this.Invoke("getAdjEmpPunches", new object[] { clientId, xmlString });
            return ((string)(results[0]));
        }

        [System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace = "useMainURL", ResponseNamespace = "useMainURL")]
        [return: System.Xml.Serialization.SoapElementAttribute("return")]
        public void verifyAdjEmpPunches(int clientId, String xmlString)
        {
            object[] results = this.Invoke("verifyAdjEmpPunches", new object[] { clientId, xmlString });
            return;
        }


    }

}