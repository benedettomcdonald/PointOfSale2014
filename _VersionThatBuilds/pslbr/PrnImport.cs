using System;
using System.IO;
using System.Collections;
using HsSharedObjects.Main;
using HsSharedObjects.Client;

namespace pslbr
{
	public class PrnImport
	{
		public PrnImport(){}

		private static Hashtable employeeWeeks;
	    private static readonly SysLog logger = new SysLog(typeof(PrnImport));

        private static int ROW_TYPE_COL = 1;

        private static int EMP_ROW = 0;
        private static int JOB_ROW = 1;
        private static int TIME_CARD_ROW = 2;

        private static int JOB_ROW_TYPE_NORMAL = 1;
        private static int JOB_ROW_TYPE_ADJUSTMENT = 7;
        private static int JOB_ROW_ENTRY_TYPE = 4;

        private static int JOB_ROW_ALT_CODE = 2;
        private static int JOB_ROW_JOB_DEPT = 3;
        private static int JOB_ROW_WEEK_HOURS = 5;
        private static int JOB_ROW_RATE = 6;
        private static int JOB_ROW_WEEK_DLRS = 7;

        private static int EMP_ID = 0;
        private static int EMP_ROW_PAY_TYPE_COL = 12;

        private static int TIME_CARD_ROW_ALT_CODE = 2;
        private static int TIME_CARD_ROW_JOB_DEPT = 3;
        private static int TIME_CARD_ROW_DATE = 4;
        private static int TIME_CARD_ROW_CLOCK_IN = 5;
        private static int TIME_CARD_ROW_CLOCK_OUT = 6;
        private static int TIME_CARD_ROW_TYPE = 7;

        public static ClientDetails clientDetails;

		public static ArrayList GetTimeCards( ClientDetails details, DateTime weekEnd, String path, Hashtable waivers )
		{
			logger.Debug("In PrnImport GetTimeCards");
            clientDetails = details;
            int ovtType = details.OvertimeRule;
			ArrayList timeCards = new ArrayList();
			employeeWeeks = new Hashtable();
			LaborWeek laborWeek = new LaborWeek( ovtType, weekEnd.AddDays(-6) );
			try
			{
                String file = path + "PR" + weekEnd.ToString( "MMddyy" ) + ".prn";
                logger.Debug("Reading from " + file);
				StreamReader text = File.OpenText( file );
				while( text.Peek() > 0 )
				{
					String inStr = text.ReadLine();
                    logger.Debug("Parsing: " + inStr);
					String[] strArray = inStr.Split( new char[] {','} );
					if( Convert.ToInt32(strArray[ROW_TYPE_COL]) == EMP_ROW )
					{
						EmployeeWeek empWeek = new EmployeeWeek( laborWeek, Convert.ToInt32( strArray[EMP_ID] ), waivers );

					    String payType = strArray[EMP_ROW_PAY_TYPE_COL];
                        try
                        {
                            Double.Parse(payType);
                            payType = strArray[EMP_ROW_PAY_TYPE_COL + 1];   // If the above line parses, we're in the wrong column
                        }
                        catch (FormatException) {}
					    payType = payType.Replace("\"", "");
					    logger.Debug("Final Pay Type: " + payType);

                        empWeek.PayType = payType;
						if( !employeeWeeks.Contains( strArray[EMP_ID] ) ) employeeWeeks.Add( strArray[EMP_ID], empWeek );
					} 
					else if( Convert.ToInt32(strArray[ROW_TYPE_COL]) == JOB_ROW )//&& ((Convert.ToInt32(strArray[JOB_ROW_ENTRY_TYPE]) == JOB_ROW_TYPE_NORMAL ) || (Convert.ToInt32(strArray[JOB_ROW_ENTRY_TYPE]) == JOB_ROW_TYPE_ADJUSTMENT)))//for second value, 1 = normal hours, 7 = previous period adjustment (needed for punch adjusts)
					{
//						logger.Debug( "Creating emp job" );
						EmployeeWeek empWeek = (EmployeeWeek) employeeWeeks[ strArray[EMP_ID] ];

                        if (empWeek != null)
                        {
                            empWeek.AddJob(getIntStr(strArray[JOB_ROW_ALT_CODE]),
                                getIntStr(strArray[JOB_ROW_JOB_DEPT]), strArray[JOB_ROW_WEEK_HOURS],
                                strArray[JOB_ROW_RATE], strArray[JOB_ROW_WEEK_DLRS]);
                        }
					}
					else if( Convert.ToInt32(strArray[ROW_TYPE_COL]) == TIME_CARD_ROW )
					{
//						logger.Debug( "Creating emp time card" );
						try
						{
							EmployeeWeek empWeek = (EmployeeWeek) employeeWeeks[ strArray[EMP_ID] ];
							strArray[2].Replace( "\"", "" );
							strArray[2].Replace( "'", "" );

                            if (empWeek != null) empWeek.AddPunch(getIntStr(strArray[TIME_CARD_ROW_ALT_CODE]), 
                                getIntStr(strArray[TIME_CARD_ROW_JOB_DEPT]), strArray[TIME_CARD_ROW_DATE], 
                                strArray[TIME_CARD_ROW_CLOCK_IN], strArray[TIME_CARD_ROW_CLOCK_OUT], 
                                strArray[TIME_CARD_ROW_TYPE], empWeek.PayType );
						}
						catch( Exception ex )
						{
							string fileSoFar = "";
							if( File.Exists( @"pslb-error.hs" ) )
							{
								StreamReader reader = File.OpenText( @"pslb-error.hs" );
								try
								{
									fileSoFar = reader.ReadToEnd();
								} 
								finally
								{
									reader.Close();
								}
							}
							StreamWriter writer = File.CreateText( @"pslb-error.hs" );
							try
							{
								writer.WriteLine( fileSoFar + ex.ToString() );
							}
							finally
							{
								writer.Flush();
								writer.Close();
							}
						}
					}
				}
				text.Close();
				
				IDictionaryEnumerator empEnum = employeeWeeks.GetEnumerator();
				int cnt = 0;
				while( empEnum.MoveNext() )
				{
					cnt++;
					EmployeeWeek weekVal = (EmployeeWeek) empEnum.Value;
					weekVal.EmployeePunchList.SortByClockIn();
					ArrayList tCards = weekVal.MakeTimeCards();
						foreach( EmployeeTimeCard tCard in tCards )
						{
							timeCards.Add( tCard );
							//Console.WriteLine( "Reg Hours = {0}, Reg Dollars = {1}, Ot Hours = {2}, Ot Dollars = {3}", tCard.RegHours, tCard.RegDollars, tCard.OtHours, tCard.OtDollars );
						}
						//Console.WriteLine( "Reg hours = {0}, Reg dollars = {1}, Ot hours = {2}, Ot dollars = {3}", regHours, regDlrs, otHours, otDlrs );
				}
				return timeCards;
			}
			catch( Exception ex )
			{
				logger.Error("Error getting timecards from Prn", ex);
				Console.WriteLine( ex.ToString() );
			}
			return new ArrayList();
		}

		private static String getIntStr( String str )
		{
			String newStr = "";
			foreach( char ch in str.ToCharArray() )
			{
				if( System.Char.IsDigit( ch ) )
				{
					newStr += ch.ToString();
				}
			}
			return newStr;
		}

	}
}
