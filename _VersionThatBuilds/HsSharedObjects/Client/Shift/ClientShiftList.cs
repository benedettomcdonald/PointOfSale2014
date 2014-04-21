using System;
using System.Collections;

namespace HsSharedObjects.Client.Shift
{
	/// <summary>
	/**Class:  ClientShiftList
	 * This class is basically an ArrayList that is intended
	 * to hold ClientShift objects. 
	 */ 
	/// </summary>
	[Serializable]
	public class ClientShiftList : ArrayList
	{
		public ClientShiftList(){}

		public bool Allows( int id )
		{
			bool allow = false;
			foreach( ClientShift f in this )
			{
				if( id == f.Id ) return true;
			}
			return allow;
		}

		/**This method will find the current shift given the list of shifts.
		 * The current shift is the one containing the current time (DateTime.Now); 
		 */
		public ClientShift CurrentShift()
		{
			TimeSpan rightNow = DateTime.Now.TimeOfDay;
			foreach(ClientShift shift in this)
			{
				if( rightNow.CompareTo( shift.StartTime ) > 0 && rightNow.CompareTo( shift.EndTime ) < 0 )
				{
					return shift;
				}
			}
			ClientShift tempShift = null;
			/*If the simple check didnt find it, that means its the last shift of the day,
			* and doesnt end until tomorrow morning.  So we find the shift with the 
			* earliest end time, and then check and make sure it is the correct shift
			*/
			foreach(ClientShift shift in this)
			{
				if(tempShift == null)
				{
					tempShift = shift;
					continue;
				}
				if(shift.EndTime.CompareTo(tempShift.EndTime) <= 0)
					if( rightNow.CompareTo(shift.StartTime) > 0 || rightNow.CompareTo(shift.EndTime) < 0 )
						tempShift = shift;
			}
			return tempShift;
		}


	}
}
