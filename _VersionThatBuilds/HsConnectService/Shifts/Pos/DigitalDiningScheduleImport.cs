using HsConnect.Shifts;
using HsConnect.Main;
using HsConnect.Data;
using HsConnect.Xml;

using System;
using System.Collections;
using System.Data;
using System.IO;
using Microsoft.Data.Odbc;

//using HsConnect.Services;
using HsConnect.Services.Wss;
using HsSharedObjects.Client.Preferences;

using System.Xml;

namespace HsConnect.Shifts.Pos
{
	public class DigitalDiningScheduleImport : ScheduleImportImpl
	{
		public DigitalDiningScheduleImport()
		{
			this.logger = new SysLog( this.GetType() );
		}

		private SysLog logger;
		private HsData data = new HsData();
		private static String FILE_PATH = System.Windows.Forms.Application.StartupPath + "\\Schedule.xml";

        public override void Execute()
        {
            logger.Debug("BEFORE:  " + this.XmlString);
            String adjusted = AdjustMonths(this.XmlString);
            logger.Debug("AFTER:  " + adjusted);
            CreateFile(adjusted);
        }

        private String AdjustMonths(String xml)
        {
            HsXmlReader reader = new HsXmlReader();
            reader.LoadXml(xml);
            logger.Debug(xml);
            foreach (XmlNode shiftNode in reader.SelectNodes("shift-list/user-shift/date"))
            {
                shiftNode.Attributes["month"].InnerText = (Int32.Parse(shiftNode.Attributes["month"].InnerText) + 1).ToString();
            }
            return reader.InnerXml;
        }
		private void CreateFile( String xml )
		{
			StreamWriter writer = null;
			try
			{
				if( File.Exists( FILE_PATH ) ) File.Delete( FILE_PATH );
				writer = File.CreateText( FILE_PATH );
				writer.Write( xml );
			}	
			catch( Exception ex )
			{
				logger.Error( ex.ToString() );
				Main.Run.errorList.Add(ex);
			}
			finally
			{
				writer.Flush();
				writer.Close();
			}
		}

		private String GetDayOfWeek( int i )
		{
			switch( i )
			{
				case 0:
					return "Sunday";
				case 1:
					return "Monday";
				case 2:
					return "Tuesday";
				case 3:
					return "Wednesday";
				case 4:
					return "Thursday";
				case 5:
					return "Friday";
				case 6:
					return "Saturday";
			}
			return "";
		}

		public override void GetWeekDates()
		{
			return;
		}

		public override DateTime CurrentWeek{ get{ return new DateTime(0); } set{ this.CurrentWeek = value; }}
	}
}
