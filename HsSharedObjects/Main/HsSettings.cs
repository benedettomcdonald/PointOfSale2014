using System;
using System.Windows.Forms;
using HsSharedObjects.Machine;
using Microsoft.Web.Services2;
using Microsoft.Web.Services2.Security;
using Microsoft.Web.Services2.Security.Tokens;

namespace HsSharedObjects.Main
{
	public class HsSettings
	{
		public HsSettings(){}

		public static String PROXY_ADDRESS = "http://bjs.hotschedules.com:8020";

		public static int GetMilliseconds( int minutes )
		{
			return minutes * 60000;
		}
		
		public static UsernameToken HsToken
		{
			get
			{
                HsProperties.Properties p = new HsProperties.Properties(Application.StartupPath + @"\properties.xml");
                if(p.UseRegistry)
                    return new UsernameToken("hsc0nnect" + Identification.GetRegistryConcept() + ":" + Identification.GetRegistryUnit(), "g4m3d4y", PasswordOption.SendHashed);
                else
				return new UsernameToken( "hsc0nnect" + Identification.MacAddress, "g4m3d4y", PasswordOption.SendHashed );
			}
			set{}
		}

	}
}
