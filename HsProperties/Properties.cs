using System;
using System.Collections;
using System.Xml;


namespace HsProperties
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class Properties
	{
        private const String Path = "properties.xml";
		private static XmlDocument _xml;
		private static XmlNode _props;
	    private const String SoapWebProxy = "http://soap.hotschedules.com:8020";
	    private const String BetaWebProxy = "http://beta.hotschedules.com:8020";
        private static readonly Hashtable ProxyMap = new Hashtable();

	    private Properties()
		{
		    LoadProperties();
		}

		public Properties(String filePath)
		{
		    LoadProperties(filePath);
		}

        private static void LoadProperties()
        {
            LoadProperties(Path);
        }

	    private static void LoadProperties(string filePath)
	    {
	        _xml = new XmlDocument();
	        _xml.Load(filePath);
	        XmlNodeList list = _xml.GetElementsByTagName("properties");
	        _props = list.Item(0);
	    }

	    public static String GetProperty(String name)
		{
			try
			{
                if(_props==null)
                {
                    LoadProperties();
                }
				return _props.SelectSingleNode(name).InnerText;

			}
			catch(Exception ex)
			{
				throw ex;
			}
		}

		public String Computer
		{
			get{ return GetProperty("computer"); }
		}

		public String Port
		{
			get{ return GetProperty("port"); }
		}

		public String SharedPort
		{
			get{ return GetProperty("hsSharedPort"); }
		}

		public bool Remote
		{
			get{ return (GetProperty("location").ToLower().Equals("remote")); }
		}

		public static String GetAddress()
		{
			Properties p = new Properties();
			return p.Computer + ":" +  p.Port;
		}

		public static String GetAddress(String path)
		{
			Properties p = new Properties(path);
			return p.Computer + ":" +  p.Port;
		}

		public int GroupId
		{
			get
			{
				return int.Parse( _props.Attributes["group"].InnerText );
			}
		}

		public String Suffix
		{
			get{return _props.Attributes["suffix"].InnerText;}
		}

		public bool MapDrive
		{
			get{return _props.Attributes["map"].InnerText.Equals("1");}
		}

		public String PosiIP
		{
			get{ return GetProperty("posi-ip"); }
		}

	    public Boolean UseRegistry
	    {
	        get
	        {
				//testing
                try
                {
                    String useRegistry = GetProperty("useregistry");
                    if (useRegistry != null && !useRegistry.Equals(""))
                        return Convert.ToBoolean(useRegistry);
                    return false;
	}
                catch
                {
                    return false;
}
	        }
	    }

        public static Boolean UseRegistryMeth(String path)
        {
            Properties p = new Properties(path);
            return p.UseRegistry;
        }

		public static Boolean UseSSLMeth(String path)
		{
			Properties p = new Properties(path);
			return UseSSL;
		}

		public static String BaseURLMeth(String path)
		{
			Properties p = new Properties(path);
			return BaseURL;
		}

	    public static String SupportMessage
	    {
	        get
	        {
	            try
	            {
	                String val = GetProperty("supportMessage").Trim();
                    if (val != null && !"".Equals(val))
                        return val;
	            }
                catch(Exception e)
                {}

                return "HsConnect did not restart correctly.  " +
                        "Please contact HotSchedules Support if the problem persists.";
	        }
	    }

	    public static String WebProxy
	    {
	        get
	        {
	            try
                {
                    if (ProxyMap.Count == 0)
                        populateProxies();

	                String val = GetProperty("webproxy").Trim();
                    if (ProxyMap.ContainsKey(val))
                        return (string) ProxyMap[val];
                    if (val.ToLower().StartsWith("http"))
                        return val;
                    return SoapWebProxy;


//                    if (val.ToLower().Equals("soap"))
//                        return SoapWebProxy;
//                    if (val.ToLower().Equals("beta"))
//                        return BetaWebProxy;
//                    if (val.ToLower().StartsWith("http"))
//                        return val;
//                    return SoapWebProxy;
                }
	            catch (Exception e)
	            {
	                return SoapWebProxy;
	            }
	        }
	    }

        public static Boolean UseSSL
        {
            get
            {
                //testing
                try
                {
                    String useSSL = GetProperty("ssl").Trim();
                    if (useSSL != null && !useSSL.Equals(""))
                        return Convert.ToBoolean(useSSL);
                    return true;
                }
                catch
                {
                    return true;
                }
            }
        }

        public static String BaseURL
        {
            get
            {
                try
                {
                    String val = GetProperty("base-url").Trim();
                    if (val != null && !val.Equals(""))
                        return val;
                }
                catch (Exception e)
                {
                    //eating this because it is not an important error, just means the value is not in the XML.
                }
                return "soap.hotschedules.com";
            }
        }

	    private static void populateProxies()
	    {
	        ProxyMap.Add("soap", SoapWebProxy);
	        ProxyMap.Add("beta", BetaWebProxy);
	    }
	}
}
