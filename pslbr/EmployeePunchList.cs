using System;
using System.Collections;

namespace pslbr
{
	public class EmployeePunchList
	{
		public EmployeePunchList()
		{
			
		}

		private ArrayList punches = new ArrayList();

		public ArrayList Punches
		{
			get{ return this.punches; }
			set{ this.punches = value; }
		}

		public void Add( EmployeePunch punch )
		{
			this.punches.Add( punch );
		}
		
		public class PunchByDate : IComparer  
		{
			int IComparer.Compare( Object x, Object y )  
			{
				EmployeePunch tc1 = (EmployeePunch) x;
				EmployeePunch tc2 = (EmployeePunch) y;
				return ( tc1.ClockIn.CompareTo( tc2.ClockIn ) );
			}
		}

		public void SortByClockIn()
		{
			PunchByDate comp = new PunchByDate();
			punches.Sort( comp );
		}

	}
}
