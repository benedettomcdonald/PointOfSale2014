using System.Diagnostics;
using HsSharedObjects.Main;

using System;
using System.IO;
using System.Management;
using Microsoft.Win32;

namespace HsSharedObjects.Machine
{
	/// <summary>
	/// Summary description for Machine.
	/// </summary>
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
                bool breakOuter = false;
                logger.Debug("Begin HsSharedObjects MAC address discovery loop");
                while (macDiscoveryAttempts < 3)
                {
                    try
                    {
                        String path = System.Windows.Forms.Application.StartupPath + @"\hsconfig.ini";
                        if (File.Exists(path))
                        {
                            macAddress = GetMACFromIni();
                            break;
                        }
                        else
                        {
                            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                            ManagementObjectCollection moc = mc.GetInstances();
                            foreach (ManagementObject mo in moc)
                            {
                                if ((bool)mo["IPEnabled"] == true)
                                {
                                    macAddress = mo["MacAddress"].ToString();
                                    mo.Dispose();
                                    breakOuter = true;
                                    break;
                                }
                                mo.Dispose();
                            }
                            if (breakOuter)
                            {
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error getting MacAddress: " + ex.ToString());
                        macAddress = "XXXX";
                    }
                    macDiscoveryAttempts++;
                    logger.Debug("WARNING: Unable to obtain mac address, retrying after 5s");
                    System.Threading.Thread.Sleep(5000);
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

        public static RegistryIdentification RegistryId
        {
            get
            {
                try
                {
                    long concept = -1;
                    long unit = -1;
                    RegistryKey regKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\osi\\afaria");

                    if (regKey != null)
                    {
                        concept = Convert.ToInt64(regKey.GetValue("concept"));
                        unit = Convert.ToInt64(regKey.GetValue("unit"));
                        return new RegistryIdentification(concept, unit);
	}
}
                catch
                {
                    return null;
                }
                return null;
            }
        }

        public static long GetRegistryConcept()
        {
            RegistryIdentification regId = RegistryId;
            if (regId != null)
                return regId.Concept;
            return -1;
        }

        public static long GetRegistryUnit()
        {
            RegistryIdentification regId = RegistryId;
            if (regId != null)
                return regId.Unit;
            return -1;
        }
    }

    public class RegistryIdentification
    {
        private readonly long _concept;
        private readonly long _unit;

        public RegistryIdentification(long concept, long unit)
        {
            _concept = concept;
            _unit = unit;
        }

        public long Unit
        {
            get { return _unit; }
        }

        public long Concept
        {
            get { return _concept; }
        }
    }
}
