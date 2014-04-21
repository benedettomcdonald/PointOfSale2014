using HsConnect.Main;

using System;
using System.Xml;

namespace HsConnect.Xml
{
	public class HsXmlReader : XmlDocument
	{
		public HsXmlReader()
		{
			this.logger = new SysLog( this.GetType() );
		}

		private SysLog logger;

		public static int ATTRIBUTE = 1;
		public static int NODE = 2;

		public XmlNode getDateEle( DateTime date , String name )
		{
			XmlDocument doc = new XmlDocument();
			XmlNode root = doc.CreateElement( name );
			root.Attributes["day"].InnerText = date.Day.ToString();
			root.Attributes["month"].InnerText = date.Month.ToString();
			root.Attributes["year"].InnerText = date.Year.ToString();
			return root;
		}

		public String GetString( XmlNode node , String xpath , int type )
		{
			String str = "";
			try
			{
				switch( type )
				{
					case 1:
						return node.Attributes[ xpath ].InnerText;
					case 2:
						return node.SelectSingleNode( xpath ).InnerText;
				}
			}
			catch( Exception ex )
			{
				logger.Error( "Error in GetString()" );
			}
			return str;
		}

		public DateTime GetDate( XmlNode node , String xpath , int type )
		{
			DateTime date = new DateTime(1,1,1);
			try
			{
				switch( type )
				{
					case 1:
						return Convert.ToDateTime( node.Attributes[ xpath ].InnerText );
					case 2:
						return Convert.ToDateTime( node.SelectSingleNode( xpath ).InnerText );
				}
			}
			catch( Exception ex )
			{
				logger.Error( "Error in GetDate()" );
			}
			return date;
		}

		public int GetInt( XmlNode node , String xpath , int type )
		{
			int num = -1;
			try
			{
				switch( type )
				{
					case 1:
						return Convert.ToInt32( node.Attributes[ xpath ].InnerText );
					case 2:
						return Convert.ToInt32( node.SelectSingleNode( xpath ).InnerText );
				}
			}
			catch( Exception ex )
			{
				//logger.Error( "Error in GetInt(): " + xpath );
			}
			return num;
		}

		public double GetDouble( XmlNode node , String xpath , int type )
		{
			double num = 0.0;
			try
			{
				switch( type )
				{
					case 1:
						return Convert.ToDouble( node.Attributes[ xpath ].InnerText );
					case 2:
						return Convert.ToDouble( node.SelectSingleNode( xpath ).InnerText );
				}
			}
			catch( Exception ex )
			{
				logger.Error( "Error in GetDouble()" );
			}
			return num;
		}
	}
}
