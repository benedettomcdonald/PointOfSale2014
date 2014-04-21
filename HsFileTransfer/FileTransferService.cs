using System;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Web.Services.Protocols;
using System.ComponentModel;
using HsProperties;
using Microsoft.Web.Services2;
using HsSharedObjects.Main;

namespace HsFileTransfer
{

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name = "FileTransferServiceSoapBinding", Namespace = "useMainURL")]

    public class FileTransferService : Microsoft.Web.Services2.WebServicesClientProtocol
    {
        public FileTransferService()
        {
            Boolean useSSL = Properties.UseSSL;
            if (useSSL)
            {
                this.Url = "https://" + Properties.BaseURL + "/hsws/services/FileTransferService";
            }
            else
            {
                this.Proxy = new System.Net.WebProxy("http://" + Properties.BaseURL + ":8020");
                this.Url = "http://" + Properties.BaseURL + ":8020/hsws/services/FileTransferService";
            }
            //this.Url =  "useMainURL";
            this.RequestSoapContext.Security.Tokens.Add(HsSettings.HsToken);
            this.RequestSoapContext.Security.Timestamp.TtlInSeconds = 86400;
        }

        [System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace = "useMainURL", ResponseNamespace = "useMainURL")]
        [return: System.Xml.Serialization.SoapElementAttribute("return")]
        public int processHSCFile(int fileId, int clientId, String fileName, bool binComplete,
                int procRef, int seqNumber, String bytes)
        {
            object[] results = this.Invoke("processHSCFile", new object[] { fileId, clientId, fileName, binComplete, procRef, seqNumber, bytes });
            return ((int)(results[0]));
        }

        [System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace = "useMainURL", ResponseNamespace = "useMainURL")]
        [return: System.Xml.Serialization.SoapElementAttribute("return")]
        public String testConnection()
        {
            object[] results = this.Invoke("testConnection", new object[] { });
            return ((String)(results[0]));
        }

    }

}