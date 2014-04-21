using System;

namespace HsConnect.TimeCards
{
	public class TimeCardListBlank : TimeCardListImpl
	{
		public TimeCardListBlank()
		{
			
		}

		public override bool PeriodLabor
		{
			get
			{
				return this.periodLabor;
			}
			set
			{
				this.periodLabor = value;
			}
		}

		public override void DbUpdate(){}
		public override void DbInsert(){}
		public override void DbLoad(){}
	}
}
