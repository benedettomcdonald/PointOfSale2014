using System;

namespace HsConnect.EmpJobs
{
	public class EmpJob
	{
		public EmpJob()	{}

		private int posId = -1;
		private int hsId = -1;
		private float regWage = 0.0f;
		private float ovtWage = 0.0f;
		private int jobCode = -1;
		private int hsJobId = -1;
		private String jobName = "";
		bool primary = false;

		public int PosId
		{
			get { return this.posId; }
			set { this.posId = value; }
		}

		public int HsId
		{
			get { return this.hsId; }
			set { this.hsId = value; }
		}

		public int HsJobId
		{
			get { return this.hsJobId; }
			set { this.hsJobId = value; }
		}	

		public int JobCode
		{
			get { return this.jobCode; }
			set { this.jobCode = value; }
		}

		public float RegWage
		{
			get { return this.regWage; }
			set { this.regWage = value; }
		}

		public float OvtWage
		{
			get { return this.ovtWage; }
			set { this.ovtWage = value; }
		}

		public String JobName
		{
			get { return this.jobName; }
			set { this.jobName = value; }
		}

		public bool Primary
		{
			get { return primary; }
			set { this.primary = value; }
		}
	}
}
