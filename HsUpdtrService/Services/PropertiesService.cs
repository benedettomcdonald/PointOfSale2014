using System;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.Web.Services;

namespace hsupdater.Services
{

	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Web.Services.WebServiceBindingAttribute(Name="PropertiesServiceSoapBinding", Namespace="http://soap.hotschedules.com:8020/hsws/services/PropertiesService" )]
    
	public class PropertiesService : System.Web.Services.Protocols.SoapHttpClientProtocol 
	{
		public PropertiesService()
		{
			Boolean useSSL = Run.UseSSL;
			if (useSSL)
			{
				this.Url = "https://" + Run.BaseURL + "/hsws/services/PropertiesService";
			}
			else
			{
				this.Proxy = new System.Net.WebProxy("http://" + Run.BaseURL + ":8020");
				this.Url = "http://" + Run.BaseURL + ":8020/hsws/services/PropertiesService";
			}
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/PropertiesService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/PropertiesService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public String getProperties()
		{
			object[] results = this.Invoke("getProperties", new object[] {});
			return ((String)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/PropertiesService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/PropertiesService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public int setRunning( String macAddress , int running, bool restarted )
		{
			object[] results = this.Invoke("setRunning", new object[] {macAddress,running,restarted});
			return ((int)(results[0]));
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/PropertiesService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/PropertiesService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public void setUpdate( String macAddress , int updated )
		{
			object[] results = this.Invoke("setUpdate", new object[] {macAddress,updated});
		}

		[System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://soap.hotschedules.com:8020/hsws/services/PropertiesService", ResponseNamespace="http://soap.hotschedules.com:8020/hsws/services/PropertiesService")]
		[return: System.Xml.Serialization.SoapElementAttribute( "return" )]
		public String getFileByteString( String fileName , String pathName )
		{
			object[] results = this.Invoke("getFileByteString", new object[] {fileName,pathName});
			return ((String)(results[0]));
		}
		
	}

}
