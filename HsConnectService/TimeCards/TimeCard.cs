using System;

namespace HsConnect.TimeCards
{
    public class TimeCard
    {
        public TimeCard() { }

        private int hsId = -1;
        private long extId = -1;
        private int empPosId = -1;
        private int jobExtId = -1;
        private String jobName = "";
        private float reg_wage = 0.0f;
        private float ovt_wage = 0.0f;
        private float reg_ttl = 0.0f;
        private float ovt_ttl = 0.0f;
        private float reg_hrs = 0.0f;
        private float ovt_hrs = 0.0f;
        private float spc_hrs = 0.0f;
        private float spc_ttl = 0.0f;
        private int ovtMins = 0;
        private int unpaidBrkMins = 0;
        private DateTime businessDate = new DateTime(0);
        private DateTime clockIn = new DateTime(0);
        private DateTime clockOut = new DateTime(0);
        private bool adjusted = false;
        private string payType;
        private float declaredTips = 0.0f;
        private float ccTips = 0.0f;

        public bool Adjusted
        {
            get { return this.adjusted; }
            set { this.adjusted = value; }
        }

        public int HsId
        {
            get { return this.hsId; }
            set { this.hsId = value; }
        }

        public long ExtId
        {
            get { return this.extId; }
            set { this.extId = value; }
        }

        public int EmpPosId
        {
            get { return this.empPosId; }
            set { this.empPosId = value; }
        }

        public int JobExtId
        {
            get { return this.jobExtId; }
            set { this.jobExtId = value; }
        }

        public String JobName
        {
            get { return this.jobName; }
            set { this.jobName = value; }
        }

        public float RegWage
        {
            get { return this.reg_wage; }
            set { this.reg_wage = value; }
        }

        public float OvtWage
        {
            get { return this.ovt_wage; }
            set { this.ovt_wage = value; }
        }

        public float RegHours
        {
            get { return this.reg_hrs; }
            set { this.reg_hrs = value; }
        }

        public float OvtHours
        {
            get { return this.ovt_hrs; }
            set { this.ovt_hrs = value; }
        }

        public float RegTotal
        {
            get { return this.reg_ttl; }
            set { this.reg_ttl = value; }
        }

        public float OvtTotal
        {
            get { return this.ovt_ttl; }
            set { this.ovt_ttl = value; }
        }

        public float SpcHours
        {
            get { return this.spc_hrs; }
            set { this.spc_hrs = value; }
        }

        public float SpcTotal
        {
            get { return this.spc_ttl; }
            set { this.spc_ttl = value; }
        }

        public int OvertimeMinutes
        {
            get { return this.ovtMins; }
            set { this.ovtMins = value; }
        }

        public int UnpaidBreakMinutes
        {
            get { return this.unpaidBrkMins; }
            set { this.unpaidBrkMins = value; }
        }

        public DateTime ClockIn
        {
            get { return this.clockIn; }
            set { this.clockIn = value; }
        }

        public DateTime ClockOut
        {
            get { return this.clockOut; }
            set { this.clockOut = value; }
        }

		public DateTime BusinessDate
		{
			get{ return this.businessDate; }
			set{ this.businessDate = value; }
		}

        public string PayType
        {
            get { return payType; }
            set { payType = value; }
        }

        public float DeclaredTips
        {
            get { return declaredTips; }
            set { declaredTips = value; }
        }

        public float CcTips
        {
            get { return ccTips; }
            set { ccTips = value; }
        }

        public override string ToString()
        {
            return EmpPosId + " " + BusinessDate.ToShortDateString() + " " + ClockIn.ToShortTimeString() + " - " +
                   ClockOut.ToShortTimeString();
        }
	}

}
