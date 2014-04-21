using System.Text;
using HsConnect.Main;
using HsConnect.Data;
using HsSharedObjects.Client;

using System;
using System.Xml;
using System.Collections;

namespace HsConnect.TimeCards
{
	public abstract class TimeCardListImpl : TimeCardList
	{
		public TimeCardListImpl()
		{
			logger = new SysLog( this.GetType() );
		}

		protected DateTime startDate;
		protected DateTime endDate;
		protected bool updated = false;
		protected ArrayList timeCardList;
		protected SysLog logger;
		protected HsDataConnection cnx;
		protected bool autoSync;
		protected bool periodLabor;
		protected int dropCards = 0;//0 = false, 1 = true
		protected ClientDetails details;
		private Hashtable dayCards = new Hashtable();

		public abstract void DbLoad();
		public abstract void DbUpdate();
		public abstract void DbInsert();

		public abstract bool PeriodLabor{get;set;}
		
		public int DropCards
		{
			get{return dropCards;}
			set{dropCards = value;}
		}

		public void SetDataConnection( String cnxString )
		{
			this.cnx = new HsDataConnection( cnxString );
		}

		public int Add( TimeCard item )
		{
		    logger.Debug("Adding timecard: " + item.EmpPosId + " " + item.BusinessDate);
			if( timeCardList == null ) timeCardList = new ArrayList();
			if( !this.GetType().ToString().Equals("HsConnect.TimeCards.TimeCardListBlank") && dayCards.ContainsKey( item.BusinessDate.Date.ToString() ) )
			{
				TimeCardListBlank list = (TimeCardListBlank) dayCards[item.BusinessDate.Date.ToString()];
				list.Add( item );
			} 
			else if( !this.GetType().ToString().Equals("HsConnect.TimeCards.TimeCardListBlank") )
			{
				TimeCardListBlank list = new TimeCardListBlank();
				list.Add( item );
				dayCards.Add( item.BusinessDate.Date.ToString(), list );
			}
			return timeCardList.Add( item );
		}

		public ArrayList GetDayCards()
		{
			ArrayList aList = new ArrayList();
			ICollection tCards = this.dayCards.Values;
			foreach( TimeCardListBlank list in tCards )
			{
				aList.Add( list );
			}	
			return aList;
		}

		public void SetWages()
		{
			try
			{
                logger.Debug("Setting Wages");
			    logger.Debug("for " + timeCardList.Count + " cards");
				foreach( TimeCard tCard in this.timeCardList )
                {
                    logger.Debug("Regular hours: " + tCard.RegHours);
                    logger.Debug("Overtime hours: " + tCard.OvtHours);
					if( tCard.RegHours > 0 && tCard.RegTotal > 0 )tCard.RegWage = (tCard.RegTotal/tCard.RegHours);
					if( tCard.OvtHours > 0 && tCard.OvtTotal > 0 ) tCard.OvtWage = (tCard.OvtTotal/tCard.OvtHours);
				}
			}
			catch(Exception ex)
			{
				logger.Error("Error setting wages for timecards", ex);
			}
		}

		public DateTime StartDate
		{
			get{ return this.startDate; }
			set{ this.startDate = value; }
		}
		
		public DateTime EndDate
		{
			get{ return this.endDate; }
			set{ this.endDate = value; }
		}

		public bool AutoSync
		{
			get{ return this.autoSync; }
			set{ this.autoSync = value; }
		}

		public ClientDetails Details
		{
			get{ return this.details; }
			set{ this.details = value; }
		}

		public class TimeCardComparer : IComparer  
		{
			int IComparer.Compare( Object x, Object y )  
			{
				TimeCard tc1 = (TimeCard) x;
				TimeCard tc2 = (TimeCard) y;
				return ( tc1.BusinessDate.CompareTo( tc2.BusinessDate ) );
			}
		}

		public class TimeCardComparerByPunch : IComparer  
		{
			int IComparer.Compare( Object x, Object y )  
			{
				TimeCard tc1 = (TimeCard) x;
				TimeCard tc2 = (TimeCard) y;
				return ( tc1.ClockIn.CompareTo( tc2.ClockOut ) );
			}
		}

		public void SortByDate()
		{
			if( timeCardList == null ) return;
			TimeCardComparer comp = new TimeCardComparer();
			timeCardList.Sort( comp );
		}

		public void SortByClockIn()
		{
			if( timeCardList == null ) return;
			TimeCardComparerByPunch comp = new TimeCardComparerByPunch();
			timeCardList.Sort( comp );
		}

		public int Count
		{
			get 
			{
				if( this.timeCardList == null ) return 0;
				return this.timeCardList.Count; 
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
			if( timeCardList == null ) return "";
			logger.Debug("in getXmlString");
			String xmlString = "";
			XmlDocument doc = new XmlDocument();
			XmlNode root = doc.CreateElement( "hsconnect-time-cards" );

			XmlElement dateRangeEle = doc.CreateElement( "date-range" );
			
			XmlElement startEle = doc.CreateElement( "start-date" );
			TimeCard startItem = (TimeCard) timeCardList[ 0 ];
			startEle.SetAttribute( "day" , startItem.BusinessDate.Day.ToString() );
			startEle.SetAttribute( "month" , startItem.BusinessDate.Month.ToString() );
			startEle.SetAttribute( "year" , startItem.BusinessDate.Year.ToString() );
			dateRangeEle.AppendChild( startEle );
			
			XmlElement endEle = doc.CreateElement( "end-date" );
			TimeCard endItem = (TimeCard) timeCardList[ timeCardList.Count - 1 ];
			endEle.SetAttribute( "day" , endItem.BusinessDate.Day.ToString() );
			endEle.SetAttribute( "month" , endItem.BusinessDate.Month.ToString() );
			endEle.SetAttribute( "year" , endItem.BusinessDate.Year.ToString() );
			dateRangeEle.AppendChild( endEle );
			Console.WriteLine( dateRangeEle.OuterXml );

			root.AppendChild( dateRangeEle );
			logger.Debug("in getXmlString:  starting for loop");
			foreach( TimeCard item in this.timeCardList )
			{
				XmlElement itemEle = doc.CreateElement( "time-card" );
				itemEle.SetAttribute( "hs-id" , item.HsId.ToString() );
				itemEle.SetAttribute( "ext-id" , item.ExtId.ToString() );
				itemEle.SetAttribute( "emp-pos-id" , item.EmpPosId.ToString() );
				
				XmlElement jobEle = doc.CreateElement( "job" );
				jobEle.SetAttribute( "job-id" , item.JobExtId.ToString() );
				jobEle.SetAttribute( "reg-wage" , item.RegWage.ToString() );
				jobEle.SetAttribute( "ovt-wage" , item.OvtWage.ToString() );
				jobEle.InnerText = item.JobName;
				itemEle.AppendChild( jobEle );

				XmlElement busDate = doc.CreateElement( "business-date" );
				busDate.SetAttribute( "day" , item.BusinessDate.Day.ToString() );
				busDate.SetAttribute( "month" , item.BusinessDate.Month.ToString() );
				busDate.SetAttribute( "year" , item.BusinessDate.Year.ToString() );
				itemEle.AppendChild( busDate );

				XmlElement inEle = doc.CreateElement( "clock-in" );
				inEle.SetAttribute( "day" , item.ClockIn.Day.ToString() );
				inEle.SetAttribute( "month" , item.ClockIn.Month.ToString() );
				inEle.SetAttribute( "year" , item.ClockIn.Year.ToString() );
				inEle.SetAttribute( "hour" , item.ClockIn.Hour.ToString() );
				inEle.SetAttribute( "minute" , item.ClockIn.Minute.ToString() );
				inEle.SetAttribute( "second" , item.ClockIn.Second.ToString() );
				itemEle.AppendChild( inEle );
				
				XmlElement outEle = doc.CreateElement( "clock-out" );
				outEle.SetAttribute( "day" , item.ClockOut.Day.ToString() );
				outEle.SetAttribute( "month" , item.ClockOut.Month.ToString() );
				outEle.SetAttribute( "year" , item.ClockOut.Year.ToString() );
				outEle.SetAttribute( "hour" , item.ClockOut.Hour.ToString() );
				outEle.SetAttribute( "minute" , item.ClockOut.Minute.ToString() );
				outEle.SetAttribute( "second" , item.ClockOut.Second.ToString() );
				itemEle.AppendChild( outEle );

				XmlElement otEle = doc.CreateElement( "overtime-minutes" );
				otEle.InnerText = item.OvertimeMinutes.ToString();
				itemEle.AppendChild( otEle );

				XmlElement spcHoursEle = doc.CreateElement( "spc-hours" );
				spcHoursEle.InnerText = item.SpcHours.ToString();
				itemEle.AppendChild( spcHoursEle );

				XmlElement spcTtlEle = doc.CreateElement( "spc-pay" );
				spcTtlEle.InnerText = item.SpcTotal.ToString();
				itemEle.AppendChild( spcTtlEle );

                XmlElement unpaidBrkEle = doc.CreateElement("unpaid-break-minutes");
                unpaidBrkEle.InnerText = item.UnpaidBreakMinutes.ToString();
                itemEle.AppendChild(unpaidBrkEle);

                XmlElement declaredTipEle = doc.CreateElement("tips");//not declared-tips to match server-side
                declaredTipEle.InnerText = item.DeclaredTips.ToString();
                itemEle.AppendChild(declaredTipEle);

                XmlElement ccTipEle = doc.CreateElement("cc-tips");//not declared-tips to match server-side
                ccTipEle.InnerText = item.CcTips.ToString();
                itemEle.AppendChild(ccTipEle);

				root.AppendChild( itemEle );
				//Console.WriteLine( itemEle.OuterXml );
			}
			doc.AppendChild( root );
			xmlString  = doc.OuterXml;
			return xmlString;
		}

		public ArrayList GetList()
		{
			return this.timeCardList;
		}
		
		public IEnumerator GetEnumerator()
		{
			return timeCardList.GetEnumerator();
		}

        public override string ToString()
        {
            if (timeCardList == null)
                return "";

            StringBuilder stringBuilder = new StringBuilder("");
            foreach (TimeCard card in timeCardList)
                stringBuilder.Append(card.ToString() + "\r\n");
            return stringBuilder.ToString();
        }
	}
}
