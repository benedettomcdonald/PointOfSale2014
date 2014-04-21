using HsConnect.Services;

using System;
using System.Xml;

namespace HsConnect.Main
{
	/// <summary>
	/// Summary description for RemoteLogger.
	/// </summary>
	public class RemoteLogger
	{
		public RemoteLogger()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public static void Log( int clientId, int logCode )
		{
			XmlDocument doc = new XmlDocument();

			XmlElement dateEle = doc.CreateElement( "date-time" );
			dateEle.SetAttribute( "day" , DateTime.Now.Day.ToString() );
			dateEle.SetAttribute( "month" , DateTime.Now.Month.ToString() );
			dateEle.SetAttribute( "year" , DateTime.Now.Year.ToString() );
			dateEle.SetAttribute( "hour" , DateTime.Now.Hour.ToString() );
			dateEle.SetAttribute( "minute" , DateTime.Now.Minute.ToString() );

			doc.AppendChild( dateEle );
			String xmlString  = doc.OuterXml;

			RemoteLogService logService = new RemoteLogService();
			logService.remoteLogItem( clientId, logCode, xmlString);
		}

		public static int IMPORT_SCHEDULE_SUCCESS = 1;
		public static int IMPORT_SCHEDULE_FAIL = -1;
  
		public static int IMPORT_EMPLOYEE_SUCCESS = 2;
		public static int IMPORT_EMPLOYEE_FAIL = -2;

		public static int DRIVE_MAP_FAIL = -3;
		public static int THREAD_POOL_FAIL = -4;

		public static int PUNCH_RECORD_SYNCH_SUCCESS = 5;
		public static int PUNCH_RECORD_SYNCH_FAIL = -5;

		public static int PUNCH_RECORD_IMPORT_SUCCESS = 6;
		public static int PUNCH_RECORD_IMPORT_FAIL = -6;

        public static int SALES_SUCCESS = 7;
        public static int SALES_FAIL = -7;

        public static int TIME_CARD_SUCCESS = 8;
        public static int TIME_CARD_FAIL = -8;

        public static int FILE_SUCCESS = 9;
        public static int FILE_FAIL = -9;

        public static int HISTORICAL_SALES_SUCCESS = 10;
        public static int HISTORICAL_SALES_FAIL = -10;



	}
}
