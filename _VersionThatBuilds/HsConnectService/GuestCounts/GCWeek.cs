using HsConnect.Services;
using HsConnect.Services.Wss;
using HsConnect.Main;
using HsConnect.Xml;

using System;
using System.Xml;
using System.Collections;

namespace HsConnect.GuestCounts
{
	public class GCWeek
	{
		public GCWeek( int clientId )
		{
			this._clientId = clientId;
			this._dayAmounts = new ArrayList();
			this._logger = new SysLog( this.GetType().ToString() );
		}

		private ArrayList _dayAmounts;
		private SysLog _logger;
		private int _clientId;

		public void Load()
		{
			//	ClientSalesWss salesService = new ClientSalesWss();
			CarinosCustomWss gcService = new CarinosCustomWss();
			//	String xmlString1 = salesService.getWeekSales( _clientId );
			String xmlString = gcService.getWeekCounts( _clientId );
			HsXmlReader reader = new HsXmlReader();
			reader.LoadXml( xmlString );
			foreach( XmlNode dob in reader.SelectNodes( "/week-count/guest-count" ) )
			{
				DateTime date = new DateTime( 
					reader.GetInt( dob , "year" , HsXmlReader.ATTRIBUTE ),
					reader.GetInt( dob , "month" , HsXmlReader.ATTRIBUTE ),
					reader.GetInt( dob , "day" , HsXmlReader.ATTRIBUTE )
				);

				GCDay day = new GCDay( date, (float) Convert.ToDouble( dob.InnerText ) );

				_logger.Log( day.Date.ToString() + ": " + day.Count );
				
				_dayAmounts.Add( day );
			}
		}

		public ArrayList DayAmounts
		{
			get{ return this._dayAmounts; }
			set{ this._dayAmounts = value; }
		}
	}

	public class GCDay
	{
		public GCDay( DateTime date, float count )
		{
			_date = date;
			_count = count;
		}

		private DateTime _date;
		private float _count;

		public DateTime Date
		{
			get{ return _date; }
			set{ _date = value; }
		}

		public float Count
		{
			get{ return _count; }
			set{ _count = value; }
		}
	}
}
