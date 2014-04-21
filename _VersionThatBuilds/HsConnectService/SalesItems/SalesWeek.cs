using HsConnect.Services;
using HsConnect.Services.Wss;
using HsConnect.Main;
using HsConnect.Xml;

using System;
using System.Xml;
using System.Collections;

namespace HsConnect.SalesItems
{
	public class SalesWeek
	{
		public SalesWeek( int clientId )
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
			ClientSalesWss salesService = new ClientSalesWss();
			String xmlString = salesService.getWeekSales( _clientId );

			HsXmlReader reader = new HsXmlReader();
			reader.LoadXml( xmlString );
			foreach( XmlNode dob in reader.SelectNodes( "/week-sales/sales-amount" ) )
			{
				DateTime date = new DateTime( 
					reader.GetInt( dob , "year" , HsXmlReader.ATTRIBUTE ),
					reader.GetInt( dob , "month" , HsXmlReader.ATTRIBUTE ),
					reader.GetInt( dob , "day" , HsXmlReader.ATTRIBUTE )
				);

				SalesDay day = new SalesDay( date, (float) Convert.ToDouble( dob.InnerText ) );

				_logger.Log( day.Date.ToString() + ": " + day.Sales );
				
				_dayAmounts.Add( day );
			}
		}

		public ArrayList DayAmounts
		{
			get{ return this._dayAmounts; }
			set{ this._dayAmounts = value; }
		}
	}

	public class SalesDay
	{
		public SalesDay( DateTime date, float sales )
		{
			_date = date;
			_sales = sales;
		}

		private DateTime _date;
		private float _sales;

		public DateTime Date
		{
			get{ return _date; }
			set{ _date = value; }
		}

		public float Sales
		{
			get{ return _sales; }
			set{ _sales = value; }
		}
	}
}
