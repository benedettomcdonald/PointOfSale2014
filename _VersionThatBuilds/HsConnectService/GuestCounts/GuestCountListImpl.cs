using HsConnect.Main;
using HsConnect.Data;

using System;
using System.Xml;
using System.Collections;
using HsSharedObjects.Client;

namespace HsConnect.GuestCounts
{
	public abstract class GuestCountListImpl : GuestCountList
	{
		public GuestCountListImpl()
		{
			logger = new SysLog( this.GetType() );
			itemHash = new Hashtable();
		}

		protected DateTime endDate;
		protected ArrayList gcItemList;
		protected SysLog logger;
		protected HsDataConnection cnx;
        protected bool useDateTimeHash = true;
        protected ClientDetails details;

		public abstract void DbLoad();
		public abstract void DbUpdate();
		public abstract void DbInsert();
		private Hashtable itemHash;

		public void SetDataConnection( String cnxString )
		{
			this.cnx = new HsDataConnection( cnxString );
		}

		public DateTime EndDate
		{
			get { return this.endDate; }
			set { this.endDate = value; }
		}

		public void Add( GuestCountItem item )
		{
			if( gcItemList == null ) gcItemList = new ArrayList();
            if (useDateTimeHash)
            {
                if (item.Date.Year > 1 && !itemHash.ContainsKey(item.Date.ToString("MM/dd/yyyy HH:mm:ss")))
                {
                    itemHash.Add(item.Date.ToString("MM/dd/yyyy HH:mm:ss"), item);
                    gcItemList.Add(item);
                }
                else if (item.Date.Year > 1)
                {
                    gcItemList.Remove((GuestCountItem)itemHash[item.Date.ToString("MM/dd/yyyy HH:mm:ss")]);
                    gcItemList.Add(item);
                    itemHash[item.Date.ToString("MM/dd/yyyy HH:mm:ss")] = item;
                }
            }
            else
            {
                gcItemList.Add(item);
            }
		}

		public class DateComparer : IComparer  
		{
			int IComparer.Compare( Object x, Object y )  
			{
				GuestCountItem gc1 = (GuestCountItem) x;
				GuestCountItem gc2 = (GuestCountItem) y;
				int result = DateTime.Compare( gc1.Date, gc2.Date );
				return result;
			}
		}

		public void SortByDate()
		{
			DateComparer comp = new DateComparer();
			gcItemList.Sort( comp );
		}

		public String GetXmlString()
		{
			SortByDate();

			String xmlString = "";
			XmlDocument doc = new XmlDocument();
			XmlNode root = doc.CreateElement( "hsconnect-sales-items" );

			XmlElement dateRangeEle = doc.CreateElement( "date-range" );
			
			XmlElement startEle = doc.CreateElement( "start-date" );
			GuestCountItem startItem = (GuestCountItem) gcItemList[ 0 ];
			startEle.SetAttribute( "day" , startItem.Date.Day.ToString() );
			startEle.SetAttribute( "month" , startItem.Date.Month.ToString() );
			startEle.SetAttribute( "year" , startItem.Date.Year.ToString() );
			dateRangeEle.AppendChild( startEle );
			
			XmlElement endEle = doc.CreateElement( "end-date" );
			GuestCountItem endItem = (GuestCountItem) gcItemList[ gcItemList.Count - 1 ];
			endEle.SetAttribute( "day" , endItem.Date.Day.ToString() );
			endEle.SetAttribute( "month" , endItem.Date.Month.ToString() );
			endEle.SetAttribute( "year" , endItem.Date.Year.ToString() );
			dateRangeEle.AppendChild( endEle );

			root.AppendChild( dateRangeEle );
            logger.Debug(this.gcItemList.Count+"");
			foreach( GuestCountItem item in this.gcItemList )
			{
				XmlElement itemEle = doc.CreateElement( "gc-item" );

				XmlElement countEle = doc.CreateElement( "guest-count" );
				countEle.SetAttribute( "day" , item.Date.Day.ToString() );
				countEle.SetAttribute( "month" , item.Date.Month.ToString() );
				countEle.SetAttribute( "year" , item.Date.Year.ToString() );
				countEle.SetAttribute( "hour" , item.Date.Hour.ToString() );
				countEle.SetAttribute( "minute" , item.Date.Minute.ToString() );
				countEle.InnerText = item.GuestCount.ToString();
				itemEle.AppendChild( countEle );

                XmlElement rvcEle = doc.CreateElement("rvc-totals");
                foreach (Object o in item.RVCTotals.Keys)
                {
                    logger.Debug("rvc section");
                    int key = (int)o;
                    logger.Debug("rvc section key = " + key);
                    int total = (int)item.RVCTotals[key];
                    logger.Debug("rvc section total = " + total);
                    String name = (String)item.RVCNames[key];
                    logger.Debug("rvc section name = " + name);
                    XmlElement rvc = doc.CreateElement("rvc-total");
                    rvc.SetAttribute("id", "" + key);
                    rvc.SetAttribute("total", total+"");
                    rvc.SetAttribute("name", name);
                    rvcEle.AppendChild(rvc);
                }
                itemEle.AppendChild(rvcEle);

				root.AppendChild( itemEle );
			}
			doc.AppendChild( root );
			xmlString  = doc.OuterXml;
			return xmlString;
		}

		public IEnumerator GetEnumerator()
		{
			return gcItemList.GetEnumerator();
		}

        public ClientDetails Details
        {
            get { return this.details; }
            set { this.details = value; }
        }
	}
}
