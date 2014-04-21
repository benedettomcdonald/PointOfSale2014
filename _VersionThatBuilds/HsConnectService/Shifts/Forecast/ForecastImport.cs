using HsConnect.Xml;
using HsConnect.Main;

using System;
using System.Xml;
using System.Collections;

namespace HsConnect.Shifts.Forecast
{
	public class ForecastImport
	{
		public ForecastImport()
		{
			_forecastLists = new ArrayList();
			logger = new SysLog( this.GetType() );
		}

		private String _xmlString = "";
        private ArrayList _forecastLists;
		private Hashtable _altHash;
		private SysLog logger;

		public String XmlString
		{
			get{ return _xmlString; }
			set{ _xmlString = value; }
		}

			public Hashtable AltJobHash
		{
			get{ return _altHash; }
			set{ this._altHash = value; }
		}

		public ArrayList ForecastLists
		{
			get{ return _forecastLists; }
			set{ this._forecastLists = value; }
		}

		public void Load()
		{
			HsXmlReader reader = new HsXmlReader();
			reader.LoadXml( _xmlString );
			foreach( XmlNode listNode in reader.SelectNodes( "forecast-lists/forecast-shift-list" ) )
			{
				//if( reader.GetInt( listNode.SelectSingleNode( "schedule" ) , "id", HsXmlReader.ATTRIBUTE ) == 158967635 &&
				//	reader.GetInt( listNode.SelectSingleNode( "week-start" ), "day", HsXmlReader.ATTRIBUTE ) == 16
				//	)
				//{
					String desc = reader.GetString( listNode , "description", HsXmlReader.NODE ); 
					String sched = reader.GetString( listNode , "schedule", HsXmlReader.NODE );
					int descId = reader.GetInt( listNode.SelectSingleNode( "description" ) , "id", HsXmlReader.ATTRIBUTE ); 
					int schedId = reader.GetInt( listNode.SelectSingleNode( "schedule" ) , "id", HsXmlReader.ATTRIBUTE ); 				
					bool applied = string.Compare( reader.GetString( listNode, "applied", HsXmlReader.ATTRIBUTE ) ,
						"true", true ) == 0;
					DateTime start = new DateTime( reader.GetInt( listNode.SelectSingleNode( "week-start" ), "year", HsXmlReader.ATTRIBUTE ),
						reader.GetInt( listNode.SelectSingleNode( "week-start" ), "month", HsXmlReader.ATTRIBUTE ) + 1,
						reader.GetInt( listNode.SelectSingleNode( "week-start" ), "day", HsXmlReader.ATTRIBUTE ) );
					ForecastShiftList shiftList = new ForecastShiftList( desc, sched, start );
					shiftList.SchedId = schedId;
					shiftList.TempId = descId;
					shiftList.Applied = applied;
					if( reader.GetInt( listNode, "shift-count", HsXmlReader.ATTRIBUTE ) > 0 )
					{
						foreach( XmlNode shiftNode in listNode.SelectNodes( "shift" ) )
						{
							try
							{
								ForecastShift shift = new ForecastShift();
				
								DateTime inTime = new DateTime(
									reader.GetInt( shiftNode.SelectSingleNode( "in" ), "y", HsXmlReader.ATTRIBUTE ),
									reader.GetInt( shiftNode.SelectSingleNode( "in" ), "m", HsXmlReader.ATTRIBUTE ) + 1,
									reader.GetInt( shiftNode.SelectSingleNode( "in" ), "d", HsXmlReader.ATTRIBUTE ),
									reader.GetInt( shiftNode.SelectSingleNode( "in" ), "h", HsXmlReader.ATTRIBUTE ),
									reader.GetInt( shiftNode.SelectSingleNode( "in" ), "mi", HsXmlReader.ATTRIBUTE ),
									0 );
							
								DateTime outTime = new DateTime(
									reader.GetInt( shiftNode.SelectSingleNode( "out" ), "y", HsXmlReader.ATTRIBUTE ),
									reader.GetInt( shiftNode.SelectSingleNode( "out" ), "m", HsXmlReader.ATTRIBUTE ) + 1,
									reader.GetInt( shiftNode.SelectSingleNode( "out" ), "d", HsXmlReader.ATTRIBUTE ),
									reader.GetInt( shiftNode.SelectSingleNode( "out" ), "h", HsXmlReader.ATTRIBUTE ),
									reader.GetInt( shiftNode.SelectSingleNode( "out" ), "mi", HsXmlReader.ATTRIBUTE ),
									0 );

								shift.InTime = inTime;
								shift.OutTime = outTime;
								int jobCode = reader.GetInt( shiftNode.SelectSingleNode( "job" ), "id", HsXmlReader.ATTRIBUTE );
								shift.JobId = _altHash.ContainsKey( jobCode+"" ) ? 
									Convert.ToInt32( (String) _altHash[ jobCode+"" ] ): -1;

								shiftList.Add( shift );
							}
							catch( Exception ex )
							{
								logger.Error( "Error adding: " + shiftNode.OuterXml );
							}
						}
						logger.Debug( shiftList.Shifts.Count + " shifts added to [" + desc + "] - " + sched + " - " + start.ToShortDateString() );
					}
					else logger.Debug( "There are no shifts in [" + desc + "] - " + sched + " - " + start.ToShortDateString() );

					_forecastLists.Add( shiftList );
				//}
			}
		}

	}
}
