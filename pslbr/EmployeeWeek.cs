using System;
using System.Collections;
using HsSharedObjects.Client.Preferences;

namespace pslbr
{
	public class EmployeeWeek
	{
		public EmployeeWeek( LaborWeek week, int employeeId, Hashtable waivers )
		{
			this.employeeId = employeeId;
			switch( week.OvtType )
			{
				case 2:
					this.otManager = (OTManager) new NevadaOTManager( this );
					break;
				case 1:
                case 4:
					this.otManager = (OTManager) new CaliforniaOTManager( this, waivers );
					
					break;
				case 0:
					this.otManager = (OTManager) new StandardOTManager( this );
					break;
			}
		}
		
		private int employeeId = -1;
		private OTManager otManager;
		private ArrayList employeeTimeCards = new ArrayList();
		private ArrayList jobList = new ArrayList();
		private Hashtable jobTable = new Hashtable();
		private Hashtable empJobHours = new Hashtable();
		private EmployeePunchList punchList = new EmployeePunchList();
        private string payType = "";

		public ArrayList MakeTimeCards()
		{
			ArrayList aList = new ArrayList();
			foreach( EmployeePunch punch in this.punchList.Punches )
			{
				EmployeeTimeCard tCard = this.otManager.GetTimeCard( punch );
				aList.Add( tCard );
			}
			return aList;
		}

		public EmployeePunchList EmployeePunchList
		{
			get{ return this.punchList; }
			set{ this.punchList = value; }
		}

		public void AddJob( String altCode, String jobDept, String weekHours, String rate, String weekDlrs )
		{
			try
			{
				int code = Convert.ToInt32( altCode );
				//int code = Convert.ToInt32( altCode.Substring( altCode.IndexOf('"')+1, altCode.Length - 2 ) );
				EmployeeJob job = new EmployeeJob( this );
				job.JobCode = code;
				job.JobDept = Convert.ToInt32( jobDept );
				job.WeekHours = (float) Convert.ToDecimal( weekHours );
				job.Rate = (float) Convert.ToDecimal( rate );
				job.WeekDlrs = (float) Convert.ToDecimal( weekDlrs );
				UpdateJobHash( job );
			}
			catch( Exception ex )
			{
				Console.WriteLine( ex.ToString() );
			}
			//Console.WriteLine( "\tAdding job {0},{1},{2},{3},{4},{5}", this.employeeId, job.JobCode, job.JobDept, job.WeekHours, job.Rate, job.WeekDlrs );
		}

		public void AddPunch(string altCode, string jobDept, string date, string clkIn, string clkOut, string type, string payType)
		{
			try
			{
                int t = Convert.ToInt32( type );
                if (t != 2 && (t != 8 || !PrnImport.clientDetails.Preferences.PrefExists(Preference.POSI_LABOR_IGNORE_TYPE_8)))
				{
					Console.WriteLine( "'" +  altCode + "'" );
					int code = Convert.ToInt32( altCode );
					//int code = Convert.ToInt32( altCode.Substring( altCode.IndexOf('"')+1, altCode.Length - 2 ) );
					int dept = Convert.ToInt32( jobDept );
					ArrayList jobs = (ArrayList) jobTable[ code.ToString() + " | " + dept.ToString() ];
					if( jobs == null || jobs.Count == 0 )
					{
						Console.WriteLine( "No jobs " + code.ToString() + " | " + dept.ToString() + " were found for this employee["+this.employeeId.ToString()+"] " );
						return;
					}
				
					EmployeeJob empJob = null;
					foreach( EmployeeJob job in jobs )
					{
						float currentJobHours = (float) Convert.ToDecimal( (String) empJobHours[ code.ToString() + " | " + dept.ToString() ] );
						//Console.WriteLine( "In job[{0}], found {1} hours, {2} week hours", job.JobCode, currentJobHours, job.WeekHours );
						if( currentJobHours < job.WeekHours || job.WeekHours == 0 )
						{
							empJob = job;
							break;
						}
					}

                    EmployeePunch ePunch = new EmployeePunch(this, clkIn, clkOut, date, empJob, Convert.ToInt32(type), payType);
					//Console.WriteLine( "\t\tCreated time card emp - {0}, job - {1}, clkIn - {2}", tCard.EmpId, tCard.AltCode, tCard.ClockIn.ToString() );
					punchList.Add( ePunch );				
				}
			}
			catch( Exception ex )
			{
				Console.WriteLine( ex.ToString() );
			}
			//else Console.WriteLine( "\tDeletedPunch" );
		}

		public void UpdateJobHash( EmployeeJob job )
		{
			ArrayList jobs = (ArrayList) jobTable[ job.JobCode.ToString() + " | " + job.JobDept.ToString() ];
			if( jobs == null )
			{
				jobs = new ArrayList();
				job.SortOrder = 0;
				jobs.Add( job );
				//Console.WriteLine( "Adding '"+job.JobCode.ToString() + " | " + job.JobDept.ToString()+"' to job hash" );
				jobTable.Add( job.JobCode.ToString() + " | " + job.JobDept.ToString(), jobs );
			} 
			else 
			{
				job.SortOrder = jobs.Count - 1;
				jobs.Add( job );
				jobTable.Remove( job.JobCode.ToString() + " | " + job.JobDept.ToString() );
				jobTable.Add( job.JobCode.ToString() + " | " + job.JobDept.ToString(), jobs );
			}
		}

		public int EmployeeId
		{
			get{ return this.employeeId; }
			set{ this.employeeId = value; }
		}

        public string PayType
        {
            get { return this.payType; }
            set { this.payType = value; }
        }
	
	}
}
