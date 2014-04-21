using System;
using System.Collections;

namespace HsConnect.TimeCards.PosList.AppleOneImport
{
	public class StandardRules : OTRules
	{
		public StandardRules()
		{
			_weekHours = new Hashtable();
		}

		private Hashtable _weekHours;

		public void TransferCards( TimeCardList timeCards, ArrayList appleCards )
		{
			foreach( AppleOneTimeCard card in appleCards )
			{
				TimeCard tCard = new TimeCard();
				tCard.EmpPosId = card.EmployeeId;
				tCard.JobExtId = card.JobId;
				tCard.ClockIn = card.ClockIn;
				tCard.ClockOut = card.ClockOut;
				tCard.BusinessDate = card.ClockIn;

				double weekHours = 0;
				if( _weekHours.ContainsKey( card.EmployeeId+"" ) )
				{
					weekHours = Convert.ToDouble( _weekHours[ card.EmployeeId+"" ] );
				}

				TimeSpan duration = card.ClockOut - card.ClockIn;
				float durationFloat = ( duration.Hours + (duration.Minutes / 60.0f) );
				float regHours = durationFloat;
				float weekOtRate = card.PayRate * 1.5f;

				if( weekHours >= 40 )
				{
					tCard.OvtHours = durationFloat;
					tCard.OvtTotal = durationFloat * weekOtRate;
				} 
				else if( weekHours < 40 && durationFloat + weekHours  > 40 )
				{
					tCard.RegHours = (float) (40.0 - weekHours);
					tCard.RegTotal = tCard.RegHours * card.PayRate;
					tCard.OvtHours = durationFloat - tCard.RegHours;
					tCard.OvtTotal = tCard.OvtHours * weekOtRate;
				}
				else if( weekHours < 40 && durationFloat + weekHours <= 40 )
				{
					tCard.RegHours = durationFloat;
					tCard.RegTotal = durationFloat * card.PayRate;
				}

				tCard.RegWage = tCard.RegHours == 0 ? 0 : (tCard.RegTotal/tCard.RegHours);
				tCard.OvtWage = tCard.OvtHours == 0 ? 0 : (tCard.OvtTotal/tCard.OvtHours);
                tCard.OvertimeMinutes = tCard.OvtHours == 0 ? 0 : ((int)(tCard.OvtHours * 60.0));
				weekHours += regHours;
				_weekHours[ card.EmployeeId+"" ] = weekHours + "";
				
				timeCards.Add( tCard );
			}
		}
	}
}
