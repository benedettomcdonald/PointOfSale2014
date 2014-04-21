using HsConnect.Services;
using HsConnect.Services.Wss;
using HsConnect.Xml;

using System;
using System.Xml;
using System.Collections;

namespace HsConnect.TimeCards
{
	/// <summary>
	/// Summary description for HsTimeCardList.
	/// </summary>
	public class HsTimeCardList : TimeCardListImpl
	{
		public HsTimeCardList()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		private Hashtable adjustedHash;
		private ArrayList adjustedCards;
		public override void DbInsert(){}
		public override void DbUpdate(){}

		public override void DbLoad()
		{
			
		}

		public void LoadFromXml(String xmlString)
		{
			
			//TimeCardWss timeCardService = new TimeCardWss();
			//String xmlString = timeCardService.getTimeCardsForEmp( this.Details.ClientId , empXml );
			AdjustedHash = new Hashtable();
			AdjustedCards = new ArrayList();
			timeCardList = new ArrayList();
			HsXmlReader reader = new HsXmlReader();
			bool useAlt = details.Preferences.PrefExists(HsSharedObjects.Client.Preferences.Preference.POSI_ALT_JOB);
			reader.LoadXml( xmlString );
			logger.Debug( xmlString );
			foreach( XmlNode card in reader.SelectNodes( "/hsconnect-adjust/punches/punch-record" ) )
			{
				//create a timecard from each and place it in the correct place(s)
				TimeCard tc = new TimeCard();
				int day = reader.GetInt( card , "business-date/day" , HsXmlReader.NODE );
				int month = reader.GetInt( card , "business-date/month" , HsXmlReader.NODE ) + 1;
				int year = reader.GetInt( card , "business-date/year" , HsXmlReader.NODE );
//			logger.Log("Business Date:  " + year + "/" + month + "/" + day);
				tc.BusinessDate = new DateTime(year, month, day);

				int inDay = reader.GetInt( card , "start-date/day" , HsXmlReader.NODE );
				int inMonth = reader.GetInt( card , "start-date/month" , HsXmlReader.NODE ) + 1;
				int inYear = reader.GetInt( card , "start-date/year" , HsXmlReader.NODE );
				int inHour = reader.GetInt( card , "start-date/hour" , HsXmlReader.NODE );
				int inMinute = reader.GetInt( card , "start-date/minute" , HsXmlReader.NODE );
				int inSecond = reader.GetInt( card , "start-date/second" , HsXmlReader.NODE );
//			logger.Log("In Date:  " + inYear + "/" + inMonth + "/" + inDay + "/" + inHour + "/" + inMinute + "/" + inSecond);
				tc.ClockIn = new DateTime( inYear, inMonth, inDay, inHour, inMinute, inSecond );

				int outDay = reader.GetInt( card , "end-date/day" , HsXmlReader.NODE );
				int outMonth = reader.GetInt( card , "end-date/month" , HsXmlReader.NODE ) + 1;
				int outYear = reader.GetInt( card , "end-date/year" , HsXmlReader.NODE );
				int outHour = reader.GetInt( card , "end-date/hour" , HsXmlReader.NODE );
				int outMinute = reader.GetInt( card , "end-date/minute" , HsXmlReader.NODE );
				int outSecond = reader.GetInt( card , "end-date/second" , HsXmlReader.NODE );
//			logger.Log("out Date:  " + outYear + "/" + outMonth + "/" + outDay + "/" + outHour + "/" + outMinute + "/" + outSecond);
				tc.ClockOut = new DateTime( outYear, outMonth, outDay, outHour, outMinute, outSecond );

				tc.EmpPosId = reader.GetInt( card , "emp-pos-id" , HsXmlReader.NODE );
				tc.ExtId = reader.GetInt( card , "ext-id" , HsXmlReader.ATTRIBUTE );
				tc.HsId = reader.GetInt( card , "hs-punch-id" , HsXmlReader.ATTRIBUTE );
				
				if(useAlt)
				{
					
					tc.JobExtId = PunchRecordUtil.GetJobCode(reader.GetInt( card , "job-id" , HsXmlReader.NODE ));
					logger.Log("useAlt TRUE:  " + tc.JobExtId);
				}
				else
					tc.JobExtId = reader.GetInt( card , "job-id" , HsXmlReader.NODE );
				//tc.RegWage = reader.GetInt( card , "job/reg-wage" , HsXmlReader.NODE ); 
				//tc.OvtWage = reader.GetInt( card , "job/ovt-wage" , HsXmlReader.NODE );
				tc.JobName = reader.GetString( card , "job-name" , HsXmlReader.NODE );//need to get job.innerText

				//tc.OvertimeMinutes = reader.GetInt( card , "overtime-minutes" , HsXmlReader.NODE );
				//tc.OvtHours = reader.GetString( card , "" , HsXmlReader.NODE );
				//tc.OvtTotal = reader.GetString( card , "" , HsXmlReader.NODE );
				//tc.RegHours = reader.GetString( card , "" , HsXmlReader.NODE );
				//tc.RegTotal = reader.GetString( card , "" , HsXmlReader.NODE );
				
				//tc.SpcHours = reader.GetString( card , "spc-hours" , HsXmlReader.NODE );// need to get spchoursele.innerText
				//tc.SpcTotal = reader.GetString( card , "" , HsXmlReader.NODE );//need to get spcttlele.innerText
				
				String changeType = reader.GetString( card , "change-type" , HsXmlReader.NODE );
				string ct = changeType == null ? "NULL" : changeType.ToString();
				logger.Log("LOADFROMXML: BEFORE -  " + ct  );
				if(!(changeType.ToLower().Equals("0")))
				{
					logger.Log("LOADFROMXML: INSIDE IF STATEMENT - " + tc.ExtId);
					if(!adjustedHash.ContainsKey(tc.EmpPosId+""))
					{
						adjustedHash.Add(tc.EmpPosId+"", tc);
						logger.Log("added hash" + tc.EmpPosId);
					}
					adjustedCards.Add(tc);
				}
				logger.Log("LOADFROMXML: AFTER");
				this.timeCardList.Add(tc);

				
			}
		}

		public ArrayList GetEmpTimecards(String emp)
		{
			ArrayList empTC = new ArrayList();
			foreach(TimeCard tc in this.timeCardList)
			{
				if(tc.EmpPosId == Int32.Parse(emp))
					empTC.Add(tc);
			}
			return empTC;
		}

		public ArrayList AdjustedCards
		{
			get{ return this.adjustedCards; }
			set{ this.adjustedCards = value; }
		}
		public Hashtable AdjustedHash
		{
			get{ return this.adjustedHash; }
			set{ this.adjustedHash = value; }
		}
		public override bool PeriodLabor
		{
			get{ return this.periodLabor; }
			set{ this.periodLabor = value; }
		}



	}
}
