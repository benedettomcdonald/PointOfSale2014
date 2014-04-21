using System;
using System.Xml;
using System.Text;
using System.Data;
using System.Collections;
using HsSharedObjects.Main;
using Microsoft.Data.Odbc;

using HsConnect.Xml;
using HsConnect.Data;
using HsConnect.Employees;

namespace HsConnect.TimeCards
{
	/// <summary>
	/// Summary description for PunchRecordUtil.
	/// </summary>
	public class PunchRecordUtil
	{
		public PunchRecordUtil()
		{
			//
			// TODO: Add constructor logic here
			//
			empLists = new Hashtable();
		}

		private Hashtable empLists;
		private DateTime start;
		private DateTime end;
		private Hashtable validDates;
        private static String drive =  Data.PosiControl.Drive;
        private static readonly SysLog logger = new SysLog(typeof(PunchRecordUtil));

		public String GetXmlString(Hashtable update)
		{
			if( empLists == null ) return "";
			String xmlString = "";
			XmlDocument doc = new XmlDocument();
			XmlNode root = doc.CreateElement( "hsconnect-punch-records" );

			XmlElement dateRangeEle = doc.CreateElement( "date-range" );
			
			XmlElement startEle = doc.CreateElement( "start-date" );
			startEle.SetAttribute( "day" , start.Day.ToString() );
			startEle.SetAttribute( "month" , start.Month.ToString() );
			startEle.SetAttribute( "year" , start.Year.ToString() );
			dateRangeEle.AppendChild( startEle );
			
			XmlElement endEle = doc.CreateElement( "end-date" );
			endEle.SetAttribute( "day" , end.Day.ToString() );
			endEle.SetAttribute( "month" , end.Month.ToString() );
			endEle.SetAttribute( "year" , end.Year.ToString() );
			dateRangeEle.AppendChild( endEle );
			Console.WriteLine( dateRangeEle.OuterXml );

			root.AppendChild( dateRangeEle );
			foreach(int emp in update.Keys)
			{
				ArrayList dates = (ArrayList)update[emp];
				ArrayList cards = (ArrayList)empLists[emp];

				foreach( TimeCard item in cards )
				{
					if(item.PayType!="S" && isValidDate(item.BusinessDate, dates))
					{
						XmlElement itemEle = doc.CreateElement( "punch-record" );
						itemEle.SetAttribute( "hs-id" , item.HsId.ToString() );
						itemEle.SetAttribute( "ext-id" , item.ExtId.ToString() );
						itemEle.SetAttribute( "emp-pos-id" , item.EmpPosId.ToString() );
                        itemEle.SetAttribute("adjusted", item.Adjusted.ToString());
				
						XmlElement jobEle = doc.CreateElement( "job" );
						jobEle.SetAttribute( "job-id" , item.JobExtId.ToString() );
						jobEle.SetAttribute( "reg-wage" , item.RegWage.ToString() );
						jobEle.SetAttribute( "reg-ttl" , item.RegTotal.ToString() );
						jobEle.SetAttribute( "ovt-wage" , item.OvtWage.ToString() );
						jobEle.SetAttribute( "ovt-ttl" , item.OvtTotal.ToString() );
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

						XmlElement spcHoursEle = doc.CreateElement( "reg-hours" );
						TimeSpan span = item.ClockOut.Subtract ( item.ClockIn );
						double minutes = span.TotalMinutes-item.OvertimeMinutes;
						spcHoursEle.InnerText = minutes.ToString();
						itemEle.AppendChild( spcHoursEle );

						XmlElement spcTtlEle = doc.CreateElement( "spc-pay" );
						spcTtlEle.InnerText = item.SpcTotal.ToString();
						itemEle.AppendChild( spcTtlEle );

						root.AppendChild( itemEle );
						//Console.WriteLine( itemEle.OuterXml );
					}
				}
			}
			doc.AppendChild( root );
			xmlString  = doc.OuterXml;
			return xmlString;
			
		}

		public String GetEmpXMLString()
		{
			StringBuilder xmlString = new StringBuilder();
			xmlString.Append("<employee-time-totals>");
			
			foreach(int key in empLists.Keys)
			{
				ArrayList tcList = (ArrayList)empLists[key];
				if(tcList.Count <= 0)
					continue;
				xmlString.Append("<employee id='"+ key+ "'>");
				Hashtable dateTotals = new Hashtable();
				Hashtable ovtTotals = new Hashtable();
				Hashtable startTimes = new Hashtable();
				foreach(TimeCard tc in tcList)
				{
						String date = tc.BusinessDate.ToShortDateString();
					TimeSpan span = tc.ClockOut.Subtract ( tc.ClockIn );
					double minutes = span.TotalMinutes-tc.OvertimeMinutes;
					double ovtMinutes = tc.OvertimeMinutes;
					if(ovtMinutes == 0 && tc.OvtTotal > 0)
						ovtMinutes = tc.OvtHours * 60;
					//regular minutes
					if(dateTotals.ContainsKey(date))
					{
						double subTotal = ((double)dateTotals[date]);
						subTotal += minutes;
						dateTotals[date] = subTotal;
					}
					else 
					{
						dateTotals.Add(date, minutes);
					}
					//overtime minutes
					if(ovtTotals.ContainsKey(date))
					{
						double ovtSubTotal = ((double)ovtTotals[date]);
						ovtSubTotal += ovtMinutes;
						ovtTotals[date] = ovtSubTotal;
					}
					else
					{
						ovtTotals.Add(date, ovtMinutes);
					}
					//start times
					ArrayList temp;
					if(startTimes.ContainsKey(date))
					{
						temp = (ArrayList)startTimes[date];
						temp.Add(tc.ClockIn);
						startTimes[date] = temp;
					}
					else
					{
						temp = new ArrayList();
						temp.Add(tc.ClockIn);
						startTimes.Add(date, temp);
					}
					
				}
				foreach( String k in dateTotals.Keys)
				{
					double ttl = (double)dateTotals[k];
					double ovtTtl = 0;
					try
					{
						ovtTtl = (double)ovtTotals[k];
					}
					catch(Exception ex)
					{
						Console.WriteLine("no overtime minutes");
					}
					ArrayList times = (ArrayList)startTimes[k];
					int count = ( times == null? 0 : times.Count );
					xmlString.Append("<day>");
					xmlString.Append("<date>" + k + "</date>");
					xmlString.Append("<regular-total>" + ttl + "</regular-total>");
					xmlString.Append("<overtime-total>" + ovtTtl + "</overtime-total>");
					xmlString.Append("<start-times count='"+count+"'>");
					//ADD START TIMES
					if(count > 0)
					{
						foreach(DateTime d in times)
						{
							xmlString.Append("<start-time>"+ d.TimeOfDay +"</start-time>");
						}
					}
					xmlString.Append("</start-times>");
					xmlString.Append("</day>");
				}
				xmlString.Append("</employee>");
			}
			xmlString.Append("</employee-time-totals>");
			return xmlString.ToString();
		}

		public Hashtable ParseUpdateXML(String update)
		{
			Hashtable hash = new Hashtable();
			HsXmlReader reader = new HsXmlReader();
			reader.LoadXml( update );
			foreach( XmlNode emp in reader.SelectNodes( "/employee-update-times/employee" ) )
			{
				ArrayList days = new ArrayList();
				foreach(XmlNode tc in emp.ChildNodes)
				{
					String date = tc.InnerText;
					char[] div = {'/'};
					String[] split = date.Split(div, 3);
					DateTime dt = new DateTime(Int32.Parse(split[2]), Int32.Parse(split[0]), Int32.Parse(split[1]));
					days.Add(dt);
				}
				hash.Add(Int32.Parse(emp.Attributes["id"].InnerText), days);
					
			}
			return hash;
		}

		public void PopulateEmpLists(TimeCardList tc)
		{
			
			foreach(TimeCard t in tc)
			{
			    logger.Debug("Salary Type: " + t.PayType);
                if (t.PayType!="S")
                {
                    int emp = t.EmpPosId;
                    if (empLists.ContainsKey(emp))
                    {
                        ((ArrayList)empLists[emp]).Add(t);
                    }
                    else
                    {
                        ArrayList al = new ArrayList();
                        al.Add(t);
                        empLists.Add(emp, al);
                    } 
                }
			}
			this.start = ((TimeCard)tc.GetList()[0]).BusinessDate.Date;
			this.end = ((TimeCard)tc.GetList()[tc.Count - 1]).BusinessDate.Date;

		}

		private bool isValidDate(DateTime date, ArrayList dates)
		{
			if(validDates == null)
				validDates = new Hashtable();

			if(validDates.ContainsKey(date.ToShortDateString()))
				return true;
			foreach(DateTime d in dates)
			{
				if(date.Date.CompareTo(d.Date) == 0)
				{	
					validDates.Add(date.ToShortDateString(), date);
					return true;
				}
			}
			return false;
		}

		public static string GetPosEmpPunches( HsSharedObjects.Client.ClientDetails details, bool AutoSync)
		{
			TimeCardManager timeCardManager = new TimeCardManager( details );
			TimeCardList timeCardList = timeCardManager.GetPosTimeCardList();
			PunchRecordUtil util = new PunchRecordUtil();
			if( AutoSync ) timeCardList.AutoSync = true;
			timeCardList.Details = details;
			timeCardList.EndDate = DateTime.Now.Subtract( TimeSpan.FromDays(30) );
			timeCardList.PeriodLabor = true;
			timeCardList.DbLoad();
			timeCardList.SortByDate();
			util.PopulateEmpLists(timeCardList);
			return util.GetEmpXMLString();
		}
		
		public static HsEmployeeList GetEmpList( HsSharedObjects.Client.ClientDetails details, String xmlString)
		{
			HsSharedObjects.Main.SysLog logger = new HsSharedObjects.Main.SysLog("PunchRecordUtil");
			HsEmployeeList empList = new HsEmployeeList(details.ClientId);
			HsXmlReader reader = new HsXmlReader();
			reader.LoadXml( xmlString );
			logger.Debug( xmlString );
			foreach( XmlNode user in reader.SelectNodes( "/hsconnect-adjust/employees/user" ) )
			{
				//create a timecard from each and place it in the correct place(s)
				Employee emp = new Employee();
				emp.HsId = reader.GetInt( user , "id" , HsXmlReader.ATTRIBUTE );
				emp.PosId = reader.GetInt( user , "pos-id" , HsXmlReader.ATTRIBUTE );
				if( user.Attributes[ "alt-num" ] != null )
				{
					emp.AltNumber = reader.GetInt( user , "alt-num" , HsXmlReader.ATTRIBUTE );
				}
				emp.FirstName = reader.GetString( user , "first-name" , HsXmlReader.NODE );
				emp.LastName = reader.GetString( user , "last-name" , HsXmlReader.NODE );
				emp.NickName = reader.GetString( user , "nick-name" , HsXmlReader.NODE );
				emp.HsUserName = reader.GetString( user , "username" , HsXmlReader.NODE );
				emp.Email = reader.GetString( user , "email" , HsXmlReader.NODE );
				emp.Address1 = reader.GetString( user , "address/street-address-1" , HsXmlReader.NODE );
				emp.Address1 = reader.GetString( user , "address/street-address-1" , HsXmlReader.NODE );
				emp.Address2 = reader.GetString( user , "address/street-address-2" , HsXmlReader.NODE );
				emp.City = reader.GetString( user , "address/city" , HsXmlReader.NODE );
				emp.State = reader.GetString( user , "address/state" , HsXmlReader.NODE );
				emp.ZipCode = reader.GetString( user , "address/zip-code" , HsXmlReader.NODE );
				emp.Phone = new PhoneNumber();
				emp.Phone.Area = reader.GetString( user , "contact-number/area-code" , HsXmlReader.NODE );
				emp.Phone.Prefix = reader.GetString( user , "contact-number/prefix" , HsXmlReader.NODE );
				emp.Phone.Number = reader.GetString( user , "contact-number/number" , HsXmlReader.NODE );
				XmlNode smsNode = user.SelectSingleNode( "sms-messaging" );
				emp.Mobile = reader.GetString( smsNode , "number" , HsXmlReader.ATTRIBUTE );
				emp.Status = new EmployeeStatus();

				// add Emp status info
				if( reader.GetInt( user , "user-inactive/use-date-range" , HsXmlReader.NODE ) == 1 )
				{
					emp.Status.InactiveFrom = reader.GetDate( user , "user-inactive/startDate" , HsXmlReader.NODE );
					emp.Status.InactiveTo = reader.GetDate( user , "user-inactive/endDate" , HsXmlReader.NODE );
				}

				emp.Status.StatusCode = reader.GetInt( user , "user-status/status-id" , HsXmlReader.NODE );
				
				// add birth date
				foreach( XmlNode dateNode in user.SelectNodes( "date" ) )
				{
					if ( dateNode.InnerText.Equals( "birth-date" ) )
					{
						int day = reader.GetInt( dateNode , "day" , HsXmlReader.ATTRIBUTE );
						int month = 1 + reader.GetInt( dateNode , "month" , HsXmlReader.ATTRIBUTE );
						int year = reader.GetInt( dateNode , "year" , HsXmlReader.ATTRIBUTE );
						emp.BirthDate = new DateTime( year , month , day );
					}
				}
				emp.UpdateStatus = reader.GetInt( user.SelectSingleNode( "update-status" ) , "id" , HsXmlReader.ATTRIBUTE );
				empList.Add( emp );
			}
			return empList;
		}

		public static int GetJobCode(int id)
		{
			HsSharedObjects.Main.SysLog  logger = new HsSharedObjects.Main.SysLog("PunchRecordUtil");
			HsData data = new HsData();
			DataTableBuilder builder = new DataTableBuilder();
			// load current job list
			DataTable dt = builder.GetTableFromDBF(drive + @":\SC", @"C:\", "JOBLIST" );
			DataRowCollection rows = dt.Rows;
			foreach( DataRow row in rows )
			{
				try
				{
					int altCode = data.GetInt( row , "ALT_CODE" );
					int jobCode = data.GetInt( row , "JOB_CODE" );
					if(altCode == id)
						return jobCode;
				}
				catch( Exception ex )
				{
					logger.Error( ex.ToString() );
				}
			}
			return id;
		}
	}
}
