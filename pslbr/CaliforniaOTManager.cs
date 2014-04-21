using System;
using System.Collections;
using System.Globalization;
using HsSharedObjects.Main;

namespace pslbr
{
	public class CaliforniaOTManager : OTManager
	{

		/**
		 * This overtime time is figured using a federal min wage of
		 * 5.15, and calulates OT after 40 hours per week at the rate 
		 * of 1.5. However, if an employee has a reg. rate that is higher
		 * than 1.5 times the fed. minimum wage, OT is not greater.
		 */

		public CaliforniaOTManager( EmployeeWeek week, Hashtable waivers )
		{
			this.empWeek = week;
			this.waivers = waivers;
		}

		private float MINIMUM_WAGE = 6.75f;
		private EmployeeWeek empWeek;
		private Hashtable empDayHours = new Hashtable();
		private float empWeekHours = 0.0f;
		private DateTime lastDay = new DateTime(0);
		private int consecDays = 0;
		private DateTime lastOutTime = new DateTime(0); // used for split shift penalty
		private Hashtable waivers;
		SysLog logger = new SysLog("CaliforniaOTManager");

		public override EmployeeTimeCard GetTimeCard( EmployeePunch punch )
		{
			bool otCalculated = false;
			EmployeeJob empJob = punch.Job;
			DateTime inDate = punch.ClockIn;
			DateTime outDate = punch.ClockOut;
			if( lastDay.Date != inDate.Date )
			{
				if( (inDate.Day - lastDay.Day) == 1 ) consecDays++;
				lastDay = inDate;
				lastOutTime = new DateTime(0);
			}
			
			EmployeeTimeCard tCard = new EmployeeTimeCard( this.empWeek.EmployeeId );
			if( empJob == null )
			{
				Console.WriteLine( "Employee job was null" );
				return tCard;
			}
			
			tCard.AltCode = punch.Job.JobCode;
			tCard.JobDept = punch.Job.JobDept;
			tCard.ClockIn = punch.ClockIn;
			tCard.ClockOut = punch.ClockOut;
            tCard.Adjusted = punch.Adjusted;
		    tCard.PayType = punch.PayType;

			TimeSpan duration = outDate - inDate;
			float durationFloat = ( duration.Hours + (duration.Minutes / 60.0f) );
			float regHours = durationFloat;
			float regRate = consecDays < 6 ? empJob.Rate : empJob.Rate * 1.5f;
			float otRate = consecDays < 6 ? empJob.Rate * 1.5f : empJob.Rate * 2.0f;
			float dblRate = empJob.Rate * 2.0f;			
									
			String hrs = empDayHours[ inDate.Date.ToString() ] != null ? (String) empDayHours[ inDate.Date.ToString() ] : "0";
			float dayHours = (float) Convert.ToDecimal( hrs );

			if( dayHours >= 12 )
			{
				tCard.OtHours = durationFloat;
				tCard.OtDollars = durationFloat * dblRate;
				regHours = 0.0f;
				otCalculated = true;
			}	
			else if( dayHours < 12 && durationFloat + dayHours > 12 )
			{
				tCard.RegHours = (dayHours <= 8) ? 8.0f - dayHours : 0.0f;
				tCard.RegDollars = tCard.RegHours * regRate;
				tCard.OtHours = durationFloat - tCard.RegHours;
				tCard.OtDollars = (4.0f*otRate) + ( ((dayHours+durationFloat)-12) * dblRate);
				regHours = tCard.RegHours;
				otCalculated = true;
			}	
			else if( dayHours >= 8 )
			{
				tCard.OtHours = durationFloat;
				tCard.OtDollars = durationFloat * otRate;
				regHours = 0.0f;
				otCalculated = true;
			} 
			else if( dayHours < 8 && durationFloat + dayHours > 8 )
			{
				tCard.RegHours = 8.0f - dayHours;
				tCard.RegDollars = tCard.RegHours * regRate;
				tCard.OtHours = durationFloat - tCard.RegHours;
				tCard.OtDollars = tCard.OtHours * otRate;
				regHours = tCard.RegHours;
				otCalculated = true;
			}

			// add meal break
			bool hasWaiver = empHasWaiver(this.empWeek.EmployeeId);
			int maxDuration = (hasWaiver? 6:5);
			logger.Debug("MAX DURATION:  " + maxDuration);
			if( durationFloat > maxDuration )
			{
				logger.Debug("this one");
				tCard.SpcDollars += empJob.Rate;
			}

			// add meal break if less than 30 minutes was taken
			else if( dayHours > 0 && (inDate-lastOutTime) < new TimeSpan( 0, 30, 0) && (durationFloat + dayHours >= maxDuration) )
			{	
				logger.Debug("that one");
				tCard.SpcDollars += empJob.Rate;
			}

			// add split shift penalty
			if( dayHours > 0 && (inDate-lastOutTime) > new TimeSpan( 1, 0, 0) )
			{				
				logger.Debug("the other one");
				float premium = MINIMUM_WAGE - ( (empJob.Rate-MINIMUM_WAGE) * dayHours );
				tCard.SpcDollars += (premium > 0 ? premium : 0);
			}

			if( empDayHours.ContainsKey( inDate.Date.ToString() ) ) empDayHours.Remove( inDate.Date.ToString() );
			dayHours += durationFloat;
			empDayHours.Add( inDate.Date.ToString(), dayHours.ToString() );
			lastOutTime = tCard.ClockOut;

			otRate = empJob.Rate * 1.5f;// reset the otRate to 1.5, because > 8 hour day has been accounted for
			if( !otCalculated && empWeekHours >= 40 )
			{
				tCard.OtHours = durationFloat;
				tCard.OtDollars = durationFloat * otRate;
			} 
			else if( !otCalculated && empWeekHours < 40 && durationFloat + empWeekHours  > 40 )
			{
				// this shifts put emp into overtime
				tCard.RegHours = 40.0f - empWeekHours;
				tCard.RegDollars = tCard.RegHours * regRate;
				tCard.OtHours = durationFloat - tCard.RegHours;
				tCard.OtDollars = tCard.OtHours * otRate;
			}
			else if( !otCalculated && empWeekHours < 40 && durationFloat + empWeekHours <= 40 )
			{
				// no overtime
				tCard.RegHours = durationFloat;
				tCard.RegDollars = durationFloat * regRate;
			}
			empWeekHours += regHours;
			//Console.WriteLine( "\tReg Hours = {0}, Reg Dollars = {1}, Ot Hours = {2}, Ot Dollars = {3}\n", tCard.RegHours, tCard.RegDollars, tCard.OtHours, tCard.OtDollars );
			return tCard;
		}

		private string StripQuotes( String str )
		{
			return str.Substring( str.IndexOf('"')+1, str.Length - 2 ).Trim();
		}

		private int GetHours( String str )
		{
			int hours = Convert.ToInt32( str.Substring( str.IndexOf('"')+1, str.Length - 6 ) );
			if( string.Compare( str.Substring( str.Length - 2, 1 ), "p", true ) == 0 && hours != 12 ) hours += 12;
			if( string.Compare( str.Substring( str.Length - 2, 1 ), "a", true ) == 0 && hours == 12 ) hours = 0; 
			return hours;
		}

		private int GetMinutes( String str )
		{
			return Convert.ToInt32( str.Substring( str.IndexOf(':')+1, 2 ) );
		}
		private bool empHasWaiver(int empId)
		{
			bool has = waivers.ContainsKey(empId);
			
			logger.Debug("does " + empId + " have a waiver?  " + has);
			return has;
		}



	}
}
