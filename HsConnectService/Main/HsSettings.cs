using System.Windows.Forms;
using HsConnect.Machine;

using System;
using Microsoft.Web.Services2;
using Microsoft.Web.Services2.Security;
using Microsoft.Web.Services2.Security.Tokens;

namespace HsConnect.Main
{
	public class HsSettings
	{
		public HsSettings(){}

        public static String PROXY_ADDRESS = "http://soap.hotschedules.com:8020";

		public static int GetMilliseconds( int minutes )
		{
			return minutes * 60000;
		}
		
		public static UsernameToken HsToken
		{
			get
			{
                HsProperties.Properties p = new HsProperties.Properties(Application.StartupPath + @"\properties.xml");
                if (p.UseRegistry)
                {
                    HsSharedObjects.Machine.RegistryIdentification regId = HsSharedObjects.Machine.Identification.RegistryId;
                    return new UsernameToken("hsc0nnect" + regId.Concept + ":" +regId.Unit, "g4m3d4y", PasswordOption.SendHashed);
                }
                else
				return new UsernameToken( "hsc0nnect" + Identification.MacAddress, "g4m3d4y", PasswordOption.SendHashed );
			}
			set{}
		}

	}
}
