using System;
using System.Collections;
using System.Globalization;

namespace pslbr
{
	public class StandardOTManager : OTManager
	{

		/**
		 * This overtime time is figured using a federal min wage of
		 * 5.15, and calulates OT after 40 hours per week at the rate 
		 * of 1.5. However, if an employee has a reg. rate that is higher
		 * than 1.5 times the fed. minimum wage, OT is not greater.
		 */

		public StandardOTManager( EmployeeWeek week )
		{
			this.empWeek = week;
		}

		private EmployeeWeek empWeek;
		private float empWeekHours = 0.0f;

		public override EmployeeTimeCard GetTimeCard( EmployeePunch punch )
		{
			EmployeeJob empJob = punch.Job;
			DateTime inDate = punch.ClockIn;
			DateTime outDate = punch.ClockOut;
			
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
			float weekOtRate = empJob.Rate * 1.5f;
			
			//Console.WriteLine( "Emp - {0}, Job - {1}, In - {2}, Out - {3}, Duration - {4}", this.empWeek.EmployeeId, empJob.JobCode, inDate.ToString(), outDate.ToString(), (outDate-inDate).ToString() );
			
			if( empWeekHours >= 40 )
			{
				tCard.OtHours = durationFloat;
				tCard.OtDollars = durationFloat * weekOtRate;
			} 
			else if( empWeekHours < 40 && durationFloat + empWeekHours  > 40 )
			{
				// this shifts put emp into overtime
				tCard.RegHours = 40.0f - empWeekHours;
				tCard.RegDollars = tCard.RegHours * empJob.Rate;
				tCard.OtHours = durationFloat - tCard.RegHours;
				tCard.OtDollars = tCard.OtHours * weekOtRate;
			}
			else if( empWeekHours < 40 && durationFloat + empWeekHours <= 40 )
			{
				// no overtime
				tCard.RegHours = durationFloat;
				tCard.RegDollars = durationFloat * empJob.Rate;
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

	}
}
