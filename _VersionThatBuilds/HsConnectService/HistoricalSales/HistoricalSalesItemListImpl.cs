using System;
using System.Collections;
using System.Xml;
using HsConnect.Data;
using HsConnect.Main;
using HsSharedObjects.Client;
using HsSharedObjects.Client.Preferences;
using HsSharedObjects.Client.Shift;

namespace HsConnect.HistoricalSales
{
    public abstract class HistoricalSalesItemListImpl : HistoricalSalesItemList
    {
        public HistoricalSalesItemListImpl()
		{
			logger = new SysLog( this.GetType() );
		}

		protected DateTime endDate;
		protected DateTime startDate;
		protected bool updated = false;
		protected bool autoSync = false;
		protected ArrayList salesItemList;
		protected SysLog logger;
		protected HsDataConnection cnx;
		protected DateTime DOB = new DateTime( 0 );
		protected ClientDetails _details;
        protected TimeSpan clientShiftStartTime;
        protected TimeSpan clientShiftEndTime;

		//public abstract float GetSalesTotal();
		public abstract void DbLoad();
		public abstract void DbUpdate();
		public abstract void DbInsert();

        /*
         * Effectively sets the timespan covering the DAY client shift
         * to match what comes across with clientDetails, if the start
         * time is before noon.
         */
        public void calculateClientShifts()
        {
            TimeSpan startTime = new TimeSpan(6, 0, 0);
            TimeSpan endTime = new TimeSpan(18, 0, 0);
            ClientShiftList ls = _details.ShiftList;
            foreach (ClientShift s in ls)
            {
                if (s.StartTime.Hours < 12)
                {
                    startTime = s.StartTime;
                    endTime = s.EndTime;
                }
            }
        }

		public void SetDataConnection( String cnxString )
		{
			this.cnx = new HsDataConnection( cnxString );
		}

		public bool Updated
		{
			get { return this.updated; }
			set { this.updated = value; }
		}


		public ClientDetails Details
		{
			get { return this._details; }
			set { this._details = value; }
		}

		public bool AutoSync
		{
			get { return this.autoSync; }
			set { this.autoSync = value; }
		}

		public DateTime EndDate
		{
			get { return this.endDate; }
			set { this.endDate = value; }
		}

		public DateTime StartDate
		{
			get { return this.startDate; }
			set { this.startDate = value; }
		}

		public DateTime Dob
		{
			get { return this.DOB; }
			set { this.DOB = value; }
		}

		public void Add( HistoricalSalesItem item )
		{
			if( salesItemList == null ) salesItemList = new ArrayList();
			salesItemList.Add( item );
		}

		public int Count
		{
			get 
			{
				if( this.salesItemList == null ) return 0;
				return this.salesItemList.Count; 
			}
			set { }
		}

		public HsDataConnection Cnx
		{
			get{ return this.cnx; }
			set{ this.cnx = value; }
		}


		public String GetXmlString()
		{
			String xmlString = "";
			if( salesItemList == null ) return "";
	
			SalesComparer comp = new SalesComparer();
			salesItemList.Sort( comp );

			XmlDocument doc = new XmlDocument();
			XmlNode root = doc.CreateElement( "hsconnect-sales-items" );

			XmlElement dateRangeEle = doc.CreateElement( "date-range" );
			
			XmlElement startEle = doc.CreateElement( "start-date" );
			HistoricalSalesItem startItem = (HistoricalSalesItem) salesItemList[ 0 ];
			startEle.SetAttribute( "day" , startItem.DayOfMonth.ToString() );
			startEle.SetAttribute( "month" , startItem.Month.ToString() );
			startEle.SetAttribute( "year" , startItem.Year.ToString() );
			dateRangeEle.AppendChild( startEle );
			
			XmlElement endEle = doc.CreateElement( "end-date" );
            HistoricalSalesItem endItem = (HistoricalSalesItem)salesItemList[salesItemList.Count - 1];
			endEle.SetAttribute( "day" , endItem.DayOfMonth.ToString() );
			endEle.SetAttribute( "month" , endItem.Month.ToString() );
			endEle.SetAttribute( "year" , endItem.Year.ToString() );
			dateRangeEle.AppendChild( endEle );

			root.AppendChild( dateRangeEle );
		    bool microsByRvc = Details.Preferences.PrefExists(Preference.MICROS_BY_RVC);
			foreach( HistoricalSalesItem item in this.salesItemList )
			{
				XmlElement itemEle = doc.CreateElement( "sales-item" );
				itemEle.SetAttribute( "hs-id" , item.HsId.ToString() );
				itemEle.SetAttribute( "ext-id" , item.ExtId.ToString() );

				XmlElement amountEle = doc.CreateElement( "amount" );
				amountEle.InnerText = item.Amount.ToString();
				itemEle.AppendChild( amountEle );

				XmlElement dateEle = doc.CreateElement( "date-time" );
				dateEle.SetAttribute( "day" , item.DayOfMonth.ToString() );
				dateEle.SetAttribute( "month" , item.Month.ToString() );
				dateEle.SetAttribute( "year" , item.Year.ToString() );
				dateEle.SetAttribute( "hour" , item.Hour.ToString() );
				dateEle.SetAttribute( "minute" , item.Minute.ToString() );
				itemEle.AppendChild( dateEle );



                if (microsByRvc)
                {
                    XmlElement rvcEle = doc.CreateElement("rvc-data");
                    rvcEle.SetAttribute("rvc", item.RVC.ToString());
                    rvcEle.SetAttribute("category", item.Category.ToString());
                    itemEle.AppendChild(rvcEle);

                    XmlElement bdateEle = doc.CreateElement("business-date");
                    bdateEle.SetAttribute("day", item.BusinessDate.Day.ToString());
                    bdateEle.SetAttribute("month", item.BusinessDate.Month.ToString());
                    bdateEle.SetAttribute("year", item.BusinessDate.Year.ToString());
                    bdateEle.SetAttribute("hour", "0");
                    bdateEle.SetAttribute("minute", "0");
                    itemEle.AppendChild(bdateEle);
                }

				root.AppendChild( itemEle );
			}

			doc.AppendChild( root );
			xmlString  = doc.OuterXml;
			return xmlString;
		}

		public IEnumerator GetEnumerator()
		{
			return salesItemList.GetEnumerator();
		}

		public class SalesComparer : IComparer  
		{
			int IComparer.Compare( Object x, Object y )  
			{
                HistoricalSalesItem item1 = (HistoricalSalesItem)x;
                HistoricalSalesItem item2 = (HistoricalSalesItem)y;
				DateTime date1 = new DateTime( item1.Year, item1.Month, item1.DayOfMonth );
				DateTime date2 = new DateTime( item2.Year, item2.Month, item2.DayOfMonth );
				return ( date1.CompareTo( date2 ) );
			}
		}

        public ArrayList GetSalesItemList()
        {
            return this.salesItemList;
        }
    }
}
