using System;
using System.Collections;
using System.Globalization;
using HsSharedObjects.Main;

namespace pslbr
{
	public class NevadaOTManager : OTManager
	{

		/**
		 * This overtime time is figured using a federal min wage of
		 * 5.15, and calulates OT after 40 hours per week at the rate 
		 * of 1.5. However, if an employee has a reg. rate that is higher
		 * than 1.5 times the fed. minimum wage, OT is not greater.
		 */

		public NevadaOTManager( EmployeeWeek week )
		{
			this.empWeek = week;
		}

		private EmployeeWeek empWeek;
		private Hashtable empDayHours = new Hashtable();
		private float empWeekHours = 0.0f;
	    private static readonly float NEVADA_DAY_OVT_RATE = 11.33f;
        private SysLog logger = new SysLog(typeof(NevadaOTManager));

		public override EmployeeTimeCard GetTimeCard( EmployeePunch punch )
		{
            logger.Debug("Analyzing employee punch: in=" + punch.ClockIn + " out=" + punch.ClockOut + " rate=" + punch.Job.Rate + " job=" + punch.Job.JobCode + " dept=" + punch.Job.JobDept);

			float otCalculated = 0.0f;
			EmployeeJob empJob = punch.Job;
			DateTime inDate = punch.ClockIn;
			DateTime outDate = punch.ClockOut;
			
			EmployeeTimeCard tCard = new EmployeeTimeCard( this.empWeek.EmployeeId );
			if( empJob == null )
			{
				logger.Error("Employee job was null" );
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
            float dayOtRate = empJob.Rate >= NEVADA_DAY_OVT_RATE ? empJob.Rate : (empJob.Rate * 1.5f);
            bool getsDayOt = empJob.Rate >= NEVADA_DAY_OVT_RATE ? false : true;

			String hrs = empDayHours[ inDate.Date.ToString() ] != null ? (String) empDayHours[ inDate.Date.ToString() ] : "0";
			float dayHours = (float) Convert.ToDecimal( hrs );

            logger.Debug("getsDayOt = " + getsDayOt + " dayHours=" + dayHours + " duration=" + durationFloat);
			if( getsDayOt && dayHours >= 8 )
			{
				tCard.OtHours = durationFloat;
				tCard.OtDollars = durationFloat * dayOtRate;
				regHours = 0.0f;
                otCalculated = tCard.OtHours;
			}
			else if( getsDayOt && dayHours < 8 && durationFloat + dayHours > 8 )
			{
				tCard.RegHours = 8.0f - dayHours;
				tCard.RegDollars = tCard.RegHours * empJob.Rate;
				tCard.OtHours = durationFloat - tCard.RegHours;
				tCard.OtDollars = tCard.OtHours * dayOtRate;
				regHours = tCard.RegHours;
                otCalculated = tCard.OtHours;
			}
		    logger.Debug("regHours=" + tCard.RegHours + " reg$=" + tCard.RegDollars + " otHours=" + tCard.OtHours + " ot$=" +
		                 tCard.OtDollars);

			if( empDayHours.ContainsKey( inDate.Date.ToString() ) ) empDayHours.Remove( inDate.Date.ToString() );
			dayHours += durationFloat;
			empDayHours.Add( inDate.Date.ToString(), dayHours.ToString() );

			//Console.WriteLine( "Emp - {0}, Job - {1}, In - {2}, Out - {3}, Duration - {4}", this.empWeek.EmployeeId, empJob.JobCode, inDate.ToString(), outDate.ToString(), (outDate-inDate).ToString() );
            float currentTcOt = tCard.OtHours;
                
            if ( empWeekHours >= 40 )
			{
                tCard.OtHours += durationFloat - currentTcOt;
                tCard.OtDollars += (durationFloat - currentTcOt) * weekOtRate;
			}
            else if (empWeekHours < 40 && (durationFloat - currentTcOt) + empWeekHours > 40)
			{
				// this shifts put emp into overtime
				tCard.RegHours = 40.0f - empWeekHours;
				tCard.RegDollars = tCard.RegHours * empJob.Rate;
                tCard.OtHours = (durationFloat - currentTcOt) - tCard.RegHours;
				tCard.OtDollars = tCard.OtHours * weekOtRate;
			}
            else if (empWeekHours < 40 && (durationFloat - currentTcOt) + empWeekHours <= 40)
			{
				// no overtime
                tCard.RegHours = (durationFloat - currentTcOt);
                tCard.RegDollars = (durationFloat - currentTcOt) * empJob.Rate;
			}
            empWeekHours += regHours;
            logger.Debug("regHours=" + tCard.RegHours + " reg$=" + tCard.RegDollars + " otHours=" + tCard.OtHours + " ot$=" +
                         tCard.OtDollars);
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
