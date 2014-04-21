using System;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Web.Services.Protocols;
using System.ComponentModel;
using Microsoft.Web.Services2;
using HsSharedObjects.Main;
using HsFileTransfer.HscFile;
using HsProperties;

namespace HsFileTransfer
{
	/// <summary>
	/// Summary description for HscFileService.
	/// </summary>
	/// 
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Web.Services.WebServiceBindingAttribute(Name="HscFilesServiceSoapBinding", Namespace="http://adapter.ejb.hsconnect.hs.tdr3.com/" )]
	public class HscFileService : Microsoft.Web.Services2.WebServicesClientProtocol 
	{
		public HscFileService()
		{
			//this.Proxy = new System.Net.WebProxy( HsSettings.PROXY_ADDRESS );
            if (Properties.UseSSL)
            {
                this.Url = "https://soap.hotschedules.com/hscfilesService/hscfiles";
            }
            else
            {
                this.Url = "http://soap.hotschedules.com:8080/hscfilesService/hscfiles";
            }
			//this.RequestSoapContext.Security.Tokens.Add( HsSettings.HsToken );
			//this.RequestSoapContext.Security.Timestamp.TtlInSeconds = 86400;
		}

//		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://adapter.ejb.hsconnect.hs.tdr3.com/", ResponseNamespace="http://adapter.ejb.hsconnect.hs.tdr3.com/")]
//		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public Object processHSCFile( hscFile file )
		{

			object[] results = this.Invoke("processHSCFile", new object[] {file});
			return ((String)(results[0]));
		}
	}
}
