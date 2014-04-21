using System;
using System.Collections;
using System.Management;
using System.Xml;

using HsError.Services;

namespace HsError
{
	/// <summary>
	/// Summary description for HsErrorList.
	/// </summary>
	public class HsErrorList
	{	
		
		private ArrayList errors;
		private String location;
		private int clientId;
		private String macAddress;

		public HsErrorList()
		{
			errors = new ArrayList();
			clientId = -1;
			macAddress = "";
			location = "";
		}

		public HsErrorList(String loc)
		{
			errors = new ArrayList();
			location = loc;
			clientId = -1;
			macAddress = "";
		}

		public void Clear()
		{
			errors.Clear();
		}

		public void Clear(String loc)
		{
			errors.Clear();
			this.location = loc;
		}

		public int Send()
		{
			if(errors.Count > 0)
			{
				PandaService ps = new PandaService();
				try
				{
					return ps.method1(this.getXmlString(), this.location, true);
					this.Clear();
				}						
				catch(Exception ex)
				{
					HsSharedObjects.Main.SysLog logger = new HsSharedObjects.Main.SysLog(typeof(HsErrorList));
				    logger.Error("Error List:", ex);
					return -1;}
			}
			return 0;
		}
		public void Add(HsError e)
		{
			errors.Add(e);
		}

		public void Add(Exception e)
		{
			HsError error = new HsError(e, false);
			Add(error);
		}

		public HsError Get(int index)
		{
			if(index < errors.Count)
				return (HsError)errors[index];	
			else
				return null;
		}

		public String getXmlString()
		{
			try
			{
				XmlDocument doc = new XmlDocument();
				XmlNode root = doc.CreateElement( "hsconnect-errors" );
				((XmlElement)root).SetAttribute( "error-count" , errors.Count.ToString() );
				((XmlElement)root).SetAttribute( "timestamp", DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() );
				if(clientId != -1)
				{
					((XmlElement)root).SetAttribute( "client-id", clientId.ToString() );
				}
				else
				{
					((XmlElement)root).SetAttribute( "mac-address", macAddress );
				}


				foreach( HsError e in errors )
				{
					XmlElement itemEle = doc.CreateElement( "error-item" );
					itemEle.SetAttribute( "fatal" , e.IsFatal.ToString() );
					itemEle.SetAttribute( "class" , this.location );

					XmlElement locationEle = doc.CreateElement( "location" );
					locationEle.InnerText = e.Location;
					itemEle.AppendChild( locationEle );

					XmlElement exEle = doc.CreateElement( "exception-type" );
					exEle.InnerText = e.Error.GetType().ToString();
					itemEle.AppendChild( exEle );

					XmlElement estrEle = doc.CreateElement( "error-string" );
					estrEle.InnerText = e.ErrorString;
					itemEle.AppendChild( estrEle );

					XmlElement stackEle = doc.CreateElement( "stack-trace" );
					stackEle.InnerText = e.StackTrace;
					itemEle.AppendChild( stackEle );

					root.AppendChild( itemEle );
				}
				doc.AppendChild( root );
				String xmlString;
				xmlString  = doc.OuterXml;
				return xmlString;
			}
			catch(Exception ex)
			{
				HsSharedObjects.Main.SysLog logger = new HsSharedObjects.Main.SysLog("HsErrorList");
				logger.Error(ex.ToString());
				logger.Error(ex.StackTrace);
			}
			return "";
		}

		public ArrayList Errors
		{
			get{ return errors; }
		}

		public String Location
		{
			get{ return location; }
			set{ location = value; }
		}

		public int ClientId
		{
			get{ return clientId; }
			set{ clientId = value; }
		}

		public String MAC
		{
			get{ return macAddress; }
			set{ macAddress = value; }
		}

		public static String MacAddress
		{
			get
			{
				String macAddress = "";
				try
				{
					ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
					ManagementObjectCollection moc = mc.GetInstances();
					foreach(ManagementObject mo in moc)
					{
						if( ( bool ) mo["IPEnabled"] == true )
							macAddress = mo["MacAddress"].ToString();
						mo.Dispose();
					}
				}
				catch( Exception ex )
				{

				}
				return macAddress;
			}
		}
	}
}
