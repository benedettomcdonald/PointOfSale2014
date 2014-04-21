using HsConnect.Main;
using HsConnect.Xml;

using System;
using System.Xml;

namespace HsConnect.SalesItems
{
	public class SalesStartDate
	{
		public SalesStartDate()
		{
			this.logger = new SysLog( this.GetType() );
		}

		private DateTime startDate;
		private bool isNull = false;
		private SysLog logger;

		public DateTime StartDate
		{
			get{ return this.startDate; }
			set{ this.startDate = value; }
		}

		public bool IsNull
		{
			get{ return this.isNull; }
			set{ this.isNull = value; }
		}

		public void LoadFromXmlString( String xmlString )
		{
			HsXmlReader reader = new HsXmlReader();
			int containsValue = -1;
			XmlDocument doc = new XmlDocument();
			doc.LoadXml( xmlString );
			XmlNode node = doc.SelectSingleNode( "/sales-end-date" );
			containsValue = reader.GetInt( node , "contains-value" , HsXmlReader.ATTRIBUTE );
			if( containsValue > 0 )
			{
				XmlNode dateNode = doc.SelectSingleNode( "/sales-end-date/date" );
				int day = reader.GetInt( dateNode , "day" , HsXmlReader.ATTRIBUTE );
				int month = reader.GetInt( dateNode , "month" , HsXmlReader.ATTRIBUTE );
				int year = reader.GetInt( dateNode , "year" , HsXmlReader.ATTRIBUTE );
				int hours = reader.GetInt( dateNode , "hours" , HsXmlReader.ATTRIBUTE );
				int minutes = reader.GetInt( dateNode , "minutes" , HsXmlReader.ATTRIBUTE );
				StartDate = new DateTime( year , month + 1 , day , hours , minutes , 0 );
			} else this.isNull = true;
		}
	}
}
