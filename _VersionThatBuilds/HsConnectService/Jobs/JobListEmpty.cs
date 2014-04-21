using System;

namespace HsConnect.Jobs
{
	public class JobListEmpty : JobListImpl
	{
		public JobListEmpty(){}

		public override void DbLoad(){}
		public override void DbInsert(){}
		public override void DbUpdate(){}

	}
}
