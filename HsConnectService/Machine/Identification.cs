using HsConnect.Main;

using System;
using System.IO;
using System.Management;

namespace HsConnect.Machine
{
	/// <summary>
	/// Summary description for Machine.
	/// </summary>
    [Obsolete("Use HsSharedObjects version, or check in updater.Run.cs")]
	public class Identification
	{
		public Identification()
		{}

		public static String MacAddress
		{
			get
			{
				SysLog logger = new SysLog( "HsConnect.Machine.Identification" );
				String macAddress = "";
                int macDiscoveryAttempts = 0;
                while (macDiscoveryAttempts < 5)
                {
                    try
                    {
                        String path = System.Windows.Forms.Application.StartupPath + @"\hsconfig.ini";
                        if (File.Exists(path))
                        {
                            macAddress = GetMACFromIni();
                        }
                        else
                        {
                            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                            ManagementObjectCollection moc = mc.GetInstances();
                            foreach (ManagementObject mo in moc)
                            {
                                if ((bool)mo["IPEnabled"] == true)
                                    macAddress = mo["MacAddress"].ToString();
                                mo.Dispose();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error getting MacAddress: " + ex.ToString());
                        macAddress = "XXXX";
                    }
                    macDiscoveryAttempts++;
                }//while

                if (string.Compare(macAddress, "XXXX", false) == 0 || string.Compare(macAddress, "", false) == 0)
				{
                    logger.Error("Error getting MacAddress, no MAC found for machine and no hsconfig.ini available. HSC will now close...");
                    throw new Exception("No MAC address");
				}
				return macAddress;
			}
		}

		private static String GetMACFromIni()
		{
			SysLog logger = new SysLog( "HsConnect.Machine.Identification" );
            String MAC = "";
			try
			{
				String path = System.Windows.Forms.Application.StartupPath + @"\hsconfig.ini";
				if( File.Exists( path ) )
				{
					StreamReader reader = File.OpenText( path );
					try
					{
						MAC = reader.ReadLine();
					} 
					finally
					{
						reader.Close();
					}				
				}
			}
			catch( Exception ex )
			{
				logger.Error( ex.ToString() );
			}
			return MAC;
		}

	}
}
