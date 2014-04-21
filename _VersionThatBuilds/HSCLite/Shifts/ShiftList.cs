using HsSharedObjects.Main;
using HSCLite.Xml;

using System;
using System.Xml;
using System.Collections;

namespace HSCLite.Shifts
{
	public class ShiftList : ArrayList
	{
		public ShiftList()
		{
			logger = new SysLog( this.GetType() );
		}

		private SysLog logger;

		public override bool Contains( object item )
		{
			Shift compShift = (Shift) item;
			foreach( Shift shift in this )
			{
				if( shift.PosEmpId == 860 && compShift.PosEmpId == 860 )
				{

					logger.Debug( "shift.PosEmpId = " + shift.PosEmpId );
					logger.Debug( "shift.PosJobId = " + shift.PosJobId );
					logger.Debug( "shift.ClientShift = " + shift.ClientShift );
					logger.Debug( "shift.ClockIn.Date = " + shift.ClockIn.Date.ToString() );

					logger.Debug( "compShift.PosEmpId = " + compShift.PosEmpId );
					logger.Debug( "compShift.PosJobId = " + compShift.PosJobId );
					logger.Debug( "compShift.ClientShift = " + compShift.ClientShift );
					logger.Debug( "compShift.ClockIn.Date = " + compShift.ClockIn.Date.ToString() );
				}

				if( shift.PosEmpId == compShift.PosEmpId && 
					shift.PosJobId == compShift.PosJobId && 
					shift.ClientShift == compShift.ClientShift &&
					shift.ClockIn.Date.Equals( compShift.ClockIn.Date ) ) return true;
			}
			return false;
		}

		public Shift Get( Shift compShift )
		{
			foreach( Shift shift in this )
			{
				if( shift.PosEmpId == compShift.PosEmpId && 
					shift.PosJobId == compShift.PosJobId && 
					shift.ClientShift == compShift.ClientShift &&
					shift.ClockIn.Date.Equals( compShift.ClockIn.Date ) ) return shift; 
			}
			return null;
		}

		public void ImportScheduleXml( bool incHouseShifts, bool extLabels, String scheduleXml )
		{
			HsXmlReader reader = new HsXmlReader();
			reader.LoadXml( scheduleXml );
			logger.Debug( scheduleXml );
			String nodes = incHouseShifts ? "*" : "user-shift";
			foreach( XmlNode shiftNode in reader.SelectNodes( "shift-list/" + nodes ) )
			{
				Shift shift = new Shift();
				shift.PosEmpId = reader.GetInt( shiftNode.SelectSingleNode( "owner-id" ) , "pos-id" , HsXmlReader.ATTRIBUTE );
				shift.PosJobId = reader.GetInt( shiftNode.SelectSingleNode( "job" ) , "pos-id" , HsXmlReader.ATTRIBUTE );
                        
				if( extLabels )
				{
					shift.SchedName = reader.GetString( shiftNode.SelectSingleNode( "sched" ) , "name" , HsXmlReader.ATTRIBUTE );
					shift.LocId = reader.GetInt( shiftNode.SelectSingleNode( "loc" ) , "id" , HsXmlReader.ATTRIBUTE );
					if( shift.LocId != -1 ) shift.LocName = reader.GetString( shiftNode.SelectSingleNode( "loc" ) , "name" , HsXmlReader.ATTRIBUTE );
				}

				int inDay = reader.GetInt( shiftNode.SelectSingleNode( "date[.='start-date']" ) , "day" , HsXmlReader.ATTRIBUTE );
				int inMonth = reader.GetInt( shiftNode.SelectSingleNode( "date[.='start-date']" ) , "month" , HsXmlReader.ATTRIBUTE );
				int inYear = reader.GetInt( shiftNode.SelectSingleNode( "date[.='start-date']" ) , "year" , HsXmlReader.ATTRIBUTE );
				int inHour = reader.GetInt( shiftNode.SelectSingleNode( "time[.='start-time']" ) , "hour" , HsXmlReader.ATTRIBUTE );
				int inMinute = reader.GetInt( shiftNode.SelectSingleNode( "time[.='start-time']" ) , "minute" , HsXmlReader.ATTRIBUTE );
				String iAmPm = shiftNode.SelectSingleNode( "time[.='start-time']" ).Attributes["ampm"].InnerText;
				if( String.Compare( iAmPm, "pm", true ) == 0  && inHour != 12 ) inHour += 12;
				if( String.Compare( iAmPm, "am", true ) == 0  && inHour == 12 ) inHour = 0;
				DateTime clkIn = new DateTime( inYear, inMonth+1, inDay, inHour, inMinute, 0 );
				shift.ClockIn = clkIn;

				int outDay = reader.GetInt( shiftNode.SelectSingleNode( "date[.='end-date']" ) , "day" , HsXmlReader.ATTRIBUTE );
				int outMonth = reader.GetInt( shiftNode.SelectSingleNode( "date[.='end-date']" ) , "month" , HsXmlReader.ATTRIBUTE );
				int outYear = reader.GetInt( shiftNode.SelectSingleNode( "date[.='end-date']" ) , "year" , HsXmlReader.ATTRIBUTE );
				int outHour = reader.GetInt( shiftNode.SelectSingleNode( "time[.='end-time']" ) , "hour" , HsXmlReader.ATTRIBUTE );
				int outMinute = reader.GetInt( shiftNode.SelectSingleNode( "time[.='end-time']" ) , "minute" , HsXmlReader.ATTRIBUTE );
				String oAmPm = shiftNode.SelectSingleNode( "time[.='end-time']" ).Attributes["ampm"].InnerText;
				if( String.Compare( oAmPm, "pm", true ) == 0 && outHour != 12 ) outHour += 12;
				if( String.Compare( oAmPm, "am", true ) == 0 && outHour == 12 ) outHour = 0;
				DateTime clkOut = new DateTime( outYear, outMonth+1, outDay, outHour, outMinute, 0 );
				shift.ClockOut = clkOut;

				if( shift.ClockIn.TimeOfDay >= new TimeSpan( 4, 0, 0 ) && shift.ClockIn.TimeOfDay <= new TimeSpan( 15, 30, 0 ) )
				{
					shift.ClientShift = 0;
				} 
				else shift.ClientShift = 1;

				logger.Debug( "         [ImportScheduleXml()] Adding: " + shift.PosEmpId + ", " + shift.ClockIn.ToString() );

				this.Add( shift );

			}

		}
        
	}
}