using System;

namespace pslbr
{
	public abstract class OTManager
	{
		public OTManager() { }

		public abstract EmployeeTimeCard GetTimeCard( EmployeePunch punch );

	}
}
