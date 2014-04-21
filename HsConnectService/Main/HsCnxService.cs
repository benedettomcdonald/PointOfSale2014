using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.ServiceProcess;
using System.Threading;

using HsProperties;

using HsSharedObjects;
using HsSharedObjects.Client;
using HsSharedObjects.Client.Module;
using HsSharedObjects.Client.Preferences;
using HsSharedObjects.Client.CustomModule;

using HsConnect.Machine;
using HsConnect.Services;
using HsConnect.Services.Wss;
using HsConnect.Forms;
using HsConnect.Modules;
using HsConnect.Employees;
using HsConnect.EmpJobs;
using HsConnect.Jobs;
using HsConnect.Schedules;
using HsConnect.Data;

namespace HsConnect.Main
{
	/// <summary>
	/// Summary description for HsCnxService.
	/// </summary>
	public class _hscnx : System.ServiceProcess.ServiceBase
	{

		private static String Computer;
		private static String HsSharedPort;
		private static bool _mapped = false;	

	
		public _hscnx()
		{
			this.ServiceName = "_hscnx";
			this.CanStop = true;
			this.CanPauseAndContinue = false;
			this.AutoLog = false;
			InitializeComponent();
		}

		public static void Main()
		{
			System.ServiceProcess.ServiceBase.Run(new _hscnx());			
		}

		protected override void OnStart(string[] args)
		{
            String path = System.Windows.Forms.Application.StartupPath + @"\properties.xml";
            Properties p = new Properties(path);
            Computer = p.Computer;
            HsSharedPort = p.SharedPort;
            _mapped = p.MapDrive;
            TcpServerChannel channel = new TcpServerChannel(Int32.Parse(HsSharedPort));
            ChannelServices.RegisterChannel(channel);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(ServerImpl),
                "HsCnxMethods", WellKnownObjectMode.SingleCall);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(WizardImpl),
                "WizardSharedMethods", WellKnownObjectMode.SingleCall);
			HsConnect.Main.Run.Start( args );
			GC.Collect();
			GC.WaitForPendingFinalizers();
			
		}

		private void InitializeComponent()
		{

			this.ServiceName = "_hscnx";

		}

		protected override void OnStop()
		{
			HsConnect.Main.Run.Stop();
		}

	}

	#region Shared Objects
	/**UNDER HERE ARE THE SHARED CLASSES FOR THE SERVICE AND CONSOLE
	 * HsShared Objects contains the shared data and methods needed for the
	 * operation of the console, allowing it to talk to the service over a
	 * TCP connection.  the service and console do not need to be on the 
	 * same computer, as long as TCP connections are allowed on the configured port.
	 * */
	public class ServerImpl : HsCnxMethods
	{
		private static SyncManager syncManager = new SyncManager();
		private static SysLog logger = new SysLog("ServerImpl");

		public override String login(String name, String pwd)
		{
			ClientVerifyService verifyServ = new ClientVerifyService();
			String mac = Identification.MacAddress;
			String clientInfo = verifyServ.checkLogin2( name, pwd );
			if( clientInfo != null && clientInfo.Length > 0 ) // a client id was returned
			{
				//Run.Start( null );
				//HsCnxData.LoggedIn = true;
				return clientInfo;
			}
			else
				return "";
		}
		public override bool login(int clientId, bool login)
		{
			if(login)
			{
				ClientVerifyService verifyServ = new ClientVerifyService();
				verifyServ.saveMacAddress(clientId , Identification.MacAddress);
				Run.Start( null );
				HsCnxData.LoggedIn = true;
				return true;
			}
			return false;
		}
        /*
		public override bool login(String name, String pwd)
		{
			ClientVerifyService verifyServ = new ClientVerifyService();
			String mac = Identification.MacAddress;
			int clientId = verifyServ.checkLogin( name, pwd, mac );
			if( clientId > 0 ) // a client id was returned
			{
				Run.Start( null );
				HsCnxData.LoggedIn = true;
				return true;
			}
			else
				return false;
		}

        */
		public override bool PrintEES( String xmlString )
		{
			StreamWriter writer = null;
			String path = @"C:\sc\xml_in\EES.XML";
			try
			{
				if( File.Exists( path ) ) File.Delete( path );
				writer = File.CreateText( path );
				writer.Write( xmlString );
			}	
			catch( Exception ex )
			{
				logger.Error( ex.ToString() );
			}
			finally
			{
				writer.Flush();
				writer.Close();
			}

			return true;
		}
		

		public override bool Sync(ClientModule mod)
		{
			try
			{
				Module m = null;
				switch( mod.ModuleId )
				{
					case 201:
						m = new NetSalesModule();
						((NetSalesModule)m).Details = HsCnxData.Details;
					
						break;
					case 202:
						m = new LaborItemModule();
						((LaborItemModule)m).Details = HsCnxData.Details;
					
						break;
					case 203:
						m = new EmployeeDataModule();
						((EmployeeDataModule)m).Details = HsCnxData.Details;
					
						break;
					case 204:
						m = new SalesVerification();
						((SalesVerification)m).Details = HsCnxData.Details;
					
						break;
					case 205:
						m = new PeriodLabor();
						((PeriodLabor)m).Details = HsCnxData.Details;
					
						break;
					case 206:
						m = new RegClockInModule();
						((RegClockInModule)m).Details = HsCnxData.Details;
					
						break;
					case 207:
						m = new ApprovalAlertModule();
						((ApprovalAlertModule)m).Details = HsCnxData.Details;
					
						break;
					case 208:
						m = new ScheduleDbfModule();
						((ScheduleDbfModule)m).Details = HsCnxData.Details;
					
						break;
					case 209:
							m = new PunchRecordModule();
							((PunchRecordModule)m).Details = HsCnxData.Details;
					
							break;
					case 220:
						m = new FileTransferModule();
						((FileTransferModule)m).Details = HsCnxData.Details;
					
						break;
                    case 221:
                        m = new GuestCountModule();
                        ((GuestCountModule)m).Details = HsCnxData.Details;

                        break;
						
				}
				syncManager.SyncModule( m );

			}
			catch(Exception ex)
			{
				logger.Error(ex.StackTrace);
			}
			finally
			{
				logger.Log("Collected, total now in use:  " + GC.GetTotalMemory(false)/1000);
				logger.Log("Collected, total after GC:  " + GC.GetTotalMemory(true)/1000);
			}
			
			return true;
		}

		public override bool Sync(ClientCustomModule mod)
		{
			Module m = null;
			try
			{
				switch( mod.ModuleId )
				{
					case 301:
						m = new CarinosGuestCount();
						((CarinosGuestCount)m).Details = HsCnxData.Details;					
						break;
						/*
						case 303:
							m = new PosiTimeCardImportModule();
							((PosiTimeCardImportModule)m).Details = HsCnxData.Details;					
							break;
						*/
					case 304:
						m = new MicrosCoverCount();
						((MicrosCoverCount)m).Details = HsCnxData.Details;					
						break;
				}
				
				syncManager.SyncModule( m );

			}
			catch(Exception ex)
			{
			}
			return true;
		}
		//currently unused
		public override void ForceSync(int modId)
		{
		}

		//currently unused
		public override bool UpdateDetails()
		{
			LoadClient load = new LoadClient( HsCnxData.Details.ClientId );
			HsCnxData.Details = load.GetClientDetails();
			return true;

		}

	}


	/**
	 * WizardShared Object contains the data and methods needed for the Wizard to 
	 * be executed.  The console executes the wizard, but all of the syncing and 
	 * data retrieval occurs on the Service end of things. 
	 * */
	public class WizardImpl : WizardSharedMethods
	{
		private static SysLog logger = new SysLog("WizardSharedMethods");
		private static EmployeeManager empManager = new EmployeeManager( HsCnxData.Details );
		private static JobManager jobManager = new JobManager( HsCnxData.Details );
		private static EmployeeList empList;
		
		public override void LoadPosData()
		{
			logger.Log( "Generating POS class" );
			empList = empManager.GetPosEmployeeList();
			logger.Log( "Loading EMP list" );
			empList.DbLoad( true );
			logger.Log( "EMP list loaded" );
			empList.SortByLastName();
			try
			{
				foreach( Employee emp in empList )
				{
					PosEmployeeMapById.Add( emp.PosId , emp );
					AddPosEmployee( emp , true );
				}				
			}
			catch( Exception ex )
			{
				logger.Error( ex.ToString() );
			}
		}

		public override bool LoadHsData()
		{
			bool loaded = false;
			HsRoleMap = new Hashtable();
			try
			{
				logger.Debug( "getting hs emp list" );
				EmployeeList hsEmpList = empManager.GetHsEmployeeList();
				hsEmpList.DbLoad();
				foreach( Employee emp in hsEmpList )
				{
					//			if HS emp already has an extId , add them to the sync list and remove 
					//			from the Pos list.
					if( emp.PosId != -1 ) 
					{
						Employee posEmp = (Employee) PosEmployeeMapById[ emp.PosId ];
						// if the POS employee was located by id
						if( posEmp != null )
						{
							RemovePosEmployee( posEmp );
							AddSyncEmployee( emp , false );
						}
					} 
					else AddHsEmployee( emp , true );
				}
			}
			catch( Exception ex )
			{
				logger.Error( ex.ToString() );
			}
			return loaded;
		}
		public override void LoadJobs()
		{
			JobList hsJobList = jobManager.GetHsJobList();
			logger.Debug( "WIZARD -- loading HS job list" );
			hsJobList.DbLoad();

			PosJobs.Clear();
			HsJobs.Clear();
			HsScheds.Clear();
			SyncJobs.Clear();
	
			//load POS jobs
			JobList posJobList = jobManager.GetPosJobList();
			posJobList.UseAlt = HsCnxData.Details.Preferences.PrefExists( Preference.POSI_ALT_JOB );
			posJobList.DbLoad();
			int index = 0;
			foreach( Job job in posJobList )
			{
				PosJobs.Insert( index++ , GetSharedJob(job, SharedJob.POS) );
			}
			HsScheduleList scheds = new HsScheduleList( HsCnxData.Details.ClientId );
			scheds.DbLoad();
			HsScheds.Insert( 0 , new SharedSchedule() );
			index = 0;
			foreach( Schedule sched in scheds.ScheduleList )
			{
				logger.Log( "inserting " + index );
				HsScheds.Insert( index++ , GetSharedSchedule( sched ) );
			}
			foreach( Job job in hsJobList )
			{
				Job j = posJobList.GetJobByExtId( job.ExtId );
				j = j == null ? posJobList.GetJobByName( job.Name ) : j;
				if( j != null )
				{
					job.ExtId = j.ExtId;
					job.OvtWage =  j.OvtWage;
					job.DefaultWage =  j.DefaultWage;
					job.Name = j.Name;
					job.Updated = true;
					foreach(SharedJob sj in PosJobs)
					{
						if(sj.Name.Equals(job.Name))
						{
							PosJobs.Remove(sj);
							break;
						}
					}
					// add to sync list
					SyncJobs.Add( GetSharedJob(job, SharedJob.SYNC) );
				}
				else
				{
					String extId = job.ExtId == -1 ? "NOT LISTED" : job.ExtId.ToString();
					HsJobs.Add( GetSharedJob(job, SharedJob.HS) );
				}
			}
		}

		public String AddPosEmployee(Employee emp, bool isNew)
		{
			SharedEmployee sEmp = GetSharedEmployee(emp, isNew);
			sEmp.Type = SharedEmployee.POS;
			return AddPosEmployee(sEmp, isNew);
		}
		public override String AddPosEmployee(SharedEmployee emp, bool isNew)
		{
			String status = "";
			if( emp.Status == EmployeeStatus.TERMINATED ) status = " [T]";
			if( emp.Status == EmployeeStatus.INACTIVE ) status = " [IA]";
			String key = "[" + emp.PosId.ToString() + "] " + emp.LastName + " , " + emp.FirstName + status;
			// add employee to the POS emp hash
			if( !PosEmployeeMap.ContainsKey( key ) ) PosEmployeeMap.Add( key , emp );
			return key;
		}

		public String AddHsEmployee(Employee emp, bool isNew)
		{
			SharedEmployee sEmp = GetSharedEmployee(emp, isNew);
			sEmp.Type = SharedEmployee.HS;
			sEmp.HsUserName = emp.HsUserName;
			return AddHsEmployee(sEmp, isNew);
		}
		public override String AddHsEmployee(SharedEmployee emp, bool isNew)
		{
			String status = "";
			if( emp.Status == EmployeeStatus.TERMINATED ) status = " [T]";
			if( emp.Status == EmployeeStatus.INACTIVE ) status = " [IA]";
			String key = "[" + emp.HsUserName + "] " + emp.LastName + " , " + emp.FirstName + status;
			// add employee to the HS emp hash if the key doesn't exist
			if( !HsEmployeeMap.ContainsKey( key ) ) HsEmployeeMap.Add( key , emp );
			return key;
		}

		public String AddSyncEmployee(Employee emp, bool isNew)
		{
			SharedEmployee sEmp = GetSharedEmployee(emp, isNew);
			sEmp.Type = SharedEmployee.SYNC;
			sEmp.HsUserName = emp.HsUserName;
			return AddSyncEmployee(sEmp, isNew);
		}
		public override String AddSyncEmployee(SharedEmployee emp, bool isNew)
		{
			String status = "";
			if( emp.Status == EmployeeStatus.TERMINATED ) status = " [T]";
			if( emp.Status == EmployeeStatus.INACTIVE ) status = " [IA]";
			String key = "[" + emp.HsUserName + "] " + emp.LastName + " , " + emp.FirstName + status;
			if( !SyncEmployeeMap.ContainsKey( key ) ) SyncEmployeeMap.Add( key , emp );
			return key;
		}

		public String RemovePosEmployee(Employee emp)
		{
			SharedEmployee sEmp = GetSharedEmployee(emp, false);
			sEmp.Type = SharedEmployee.POS;
			return RemovePosEmployee(sEmp);
		}
		public override String RemovePosEmployee(SharedEmployee emp)
		{
			String status = "";
			if( emp.Status == EmployeeStatus.TERMINATED ) status = " [T]";
			if( emp.Status == EmployeeStatus.INACTIVE ) status = " [IA]";
			String key = "[" + emp.PosId.ToString() + "] " + emp.LastName + " , " + emp.FirstName + status;
			// add the employee to the POS emp list box
			// add employee to the POS emp hash
			if( PosEmployeeMap.ContainsKey( key ) ) PosEmployeeMap.Remove( key );
			return key;
		}

		public String RemoveHsEmployee(Employee emp)
		{
			SharedEmployee sEmp = GetSharedEmployee(emp, false);
			sEmp.Type = SharedEmployee.HS;
			sEmp.HsUserName = emp.HsUserName;
			return RemoveHsEmployee(sEmp);
		}
		public override String RemoveHsEmployee(SharedEmployee emp)
		{ 
			// this key will be the display for the emp and uniquely identify them in a Hashtable
			String status = "";
			if( emp.Status == EmployeeStatus.TERMINATED ) status = " [T]";
			if( emp.Status == EmployeeStatus.INACTIVE ) status = " [IA]";
			String key = "[" + emp.HsUserName + "] " + emp.LastName + " , " + emp.FirstName + status;
			// add the employee to the HS emp list box
			// add employee to the HS emp hash if the key doesn't exist
			if( HsEmployeeMap.ContainsKey( key ) ) HsEmployeeMap.Remove( key );
			return key;
		}

		public String RemoveSyncEmployee(Employee emp)
		{
			SharedEmployee sEmp = GetSharedEmployee(emp, false);
			sEmp.Type = SharedEmployee.SYNC;
			sEmp.HsUserName = emp.HsUserName;
			return RemoveSyncEmployee(sEmp);
		}
		public override String RemoveSyncEmployee(SharedEmployee emp)
		{
			String status = "";
			if( emp.Status == EmployeeStatus.TERMINATED ) status = " [T]";
			if( emp.Status == EmployeeStatus.INACTIVE ) status = " [IA]";
			// this key will be the display for the emp and uniquely identify them in a Hashtable
			String key = "[" + emp.HsUserName + "] " + emp.LastName + " , " + emp.FirstName + status;
			if( SyncEmployeeMap.ContainsKey( key ) ) SyncEmployeeMap.Remove( key );
			return key;
		}

		public override int[] Synchronize()
		{
			int empCount = 0;
			int jobCount = 0;
			IDictionaryEnumerator empEnumerator = SyncEmployeeMap.GetEnumerator();
			EmployeeList empList = (EmployeeList) new EmployeeListEmpty();
			while ( empEnumerator.MoveNext() )
			{
				SharedEmployee sEmp = (SharedEmployee) empEnumerator.Value;
				Employee emp = GetEmployee(sEmp);
				empList.Add( emp );
			}
			try
			{
				ClientEmployeesWss empService = new ClientEmployeesWss();
				empCount = empService.syncUserIds( HsCnxData.Details.ClientId , empList.GetShortXmlString() );
			}
			catch(Exception ex)
			{
				logger.Error( ex.ToString() );
			}
			// outgoing job list
			JobList jobList = new JobListEmpty();
			// add update hsJobs to outgoing list
			foreach ( SharedJob hsJob in SyncJobs )
			{
				if( hsJob.RoleId < 0 ) hsJob.RoleId = ((SharedSchedule) HsScheds[1]).HsId;
				Job tmpJob = GetJob(hsJob);
				jobList.Add( tmpJob );
			}
			//load POS jobs
			JobList posJobList = jobManager.GetPosJobList();
			posJobList.UseAlt = HsCnxData.Details.Preferences.PrefExists( Preference.POSI_ALT_JOB );
			posJobList.DbLoad();
			// if the POS job isn't in the list, add it
			foreach( Job posJob in posJobList )
			{
				if( posJob.RoleId < 0 ) posJob.RoleId = ((SharedSchedule) HsScheds[1]).HsId;
				Job j1 = jobList.GetJobByExtId( posJob.ExtId );
				if( j1 == null ) jobList.Add( posJob );
			}
			try
			{
				ClientJobsWss jobsService = new ClientJobsWss();
				jobCount = jobsService.syncClientJobs( HsCnxData.Details.ClientId , jobList.GetXmlString() );
			}
			catch( Exception ex )
			{
				logger.Error( ex.ToString() );
			}
			// update use wizard
			ClientSettingsWss clientService = new ClientSettingsWss();
			int rows = clientService.updateUseWizard( HsCnxData.Details.ClientId , ClientDetails.WIZARD_OFF );
			HsCnxData.Details.UseWizard = ClientDetails.WIZARD_OFF;
			int[] counts = {empCount, jobCount};
			return counts;
		}
		
		public override void AutoSync(bool btn1, bool btn2)
		{
			IDictionaryEnumerator posEnumerator = PosEmployeeMap.GetEnumerator();
			IDictionaryEnumerator hsEnumerator = HsEmployeeMap.GetEnumerator();
			ArrayList hsList = new ArrayList();
			ArrayList posList = new ArrayList();
			while ( hsEnumerator.MoveNext() )
			{
				SharedEmployee hsEmp = (SharedEmployee) hsEnumerator.Value;
				hsList.Add( hsEmp );
			}
			while ( posEnumerator.MoveNext() )
			{
				SharedEmployee posEmp = (SharedEmployee) posEnumerator.Value;
				posList.Add( posEmp );
			}
			for( int j=0 ; j<posList.Count;j++ )
			{
				SharedEmployee posEmp = (SharedEmployee) posList[j];
				for( int i=0 ; i<hsList.Count;i++ )
				{
					bool canAdd = true;
					SharedEmployee hsEmp = (SharedEmployee) hsList[i];
					if( btn1 && 
						// radio 1 compare first and last name only
						( hsEmp.FirstName.Equals( posEmp.FirstName ) && hsEmp.LastName.Equals( posEmp.LastName ) ) 
						)
					{
						hsEmp.PosId = posEmp.PosId;
						//check the sync map for an emp with this ExtId
						IDictionaryEnumerator empEnumerator = SyncEmployeeMap.GetEnumerator();
						while ( empEnumerator.MoveNext() )
						{
							SharedEmployee emp = (SharedEmployee) empEnumerator.Value;
							if( hsEmp.PosId == emp.PosId ) canAdd = false;
						}
						if( canAdd ) 
						{
							AddSyncEmployee( hsEmp , true );
							RemovePosEmployee( posEmp );
							RemoveHsEmployee( hsEmp );
						}
					} 
					else if( btn2 && 
						// radio 2 checks both names and birth dates
						(hsEmp.FirstName.Equals( posEmp.FirstName ) && hsEmp.LastName.Equals( posEmp.LastName )) 
						&& ( hsEmp.BirthDate == posEmp.BirthDate )
						)
					{
						hsEmp.PosId = posEmp.PosId;
						//check the sync map for an emp with this ExtId
						IDictionaryEnumerator empEnumerator = SyncEmployeeMap.GetEnumerator();
						while ( empEnumerator.MoveNext() )
						{
							SharedEmployee emp = (SharedEmployee) empEnumerator.Value;
							if( hsEmp.PosId == emp.PosId ) canAdd = false;
						}
						if( canAdd ) 
						{
							AddSyncEmployee( hsEmp , true );
							RemovePosEmployee( posEmp );
							RemoveHsEmployee( hsEmp );
						}
					}
				}
			}
		}

		public override void Button12(SharedJob posJob, int posIndex, int index)
		{
			PosJobs.RemoveAt(posIndex);
			SyncJobs.Insert(index, posJob);
		}

		public override void UpdateJobSched(int index, SharedSchedule sched)
		{
			SharedJob job = (SharedJob)SyncJobs[index];
			job.RoleId = sched.HsId;
		}
		public override void AddJob(int posIndex, int hsIndex, SharedJob hsJob, SharedJob posJob)
		{
			PosJobs.RemoveAt(posIndex);
			HsJobs.RemoveAt(hsIndex);
			hsJob.Name = posJob.Name;
			hsJob.ExtId = posJob.ExtId;
			SyncJobs.Add(hsJob);
		}
		public override void DoubleClickPosJob(SharedJob job, int index)
		{
			PosJobs.RemoveAt( index );
			SyncJobs.Add( job );
		}


		public override void Cancel()
		{
			PosEmployeeMapById = new Hashtable();
			PosEmployeeMap = new Hashtable();
			PosJobs = new ArrayList();
			HsJobs = new ArrayList();
			HsScheds = new ArrayList();
			SyncJobs = new ArrayList();
			HsEmployeeMap = new Hashtable();
			SyncEmployeeMap = new Hashtable();
		}
		public override SharedEmployee ReAddPosEmployee(SharedEmployee emp)
		{
			SharedEmployee newEmp = new SharedEmployee();
			Employee e = (Employee)PosEmployeeMapById[emp.PosId];
			newEmp = GetSharedEmployee(e, false);
			return newEmp;
		}

		private SharedEmployee GetSharedEmployee(Employee emp, bool isNew)
		{
			SharedEmployee sEmp = new SharedEmployee();
			sEmp.PosId = emp.PosId;
			sEmp.LastName = emp.LastName;
			sEmp.FirstName = emp.FirstName;
			sEmp.BirthDate = emp.BirthDate;
			sEmp.Status = emp.Status.StatusCode;
			sEmp.IsNew = isNew;
			return sEmp;
		}
		private SharedJob GetSharedJob( Job job, int type )
		{
			SharedJob j = new SharedJob();
			j.ExtId = job.ExtId;
			j.ExtIdString = (j.ExtId == -1? "NOT LISTED" : j.ExtId.ToString());
			j.Name = job.Name;
			j.Type = type;
			j.RoleId = job.RoleId;
			return j;
		}

		private SharedSchedule GetSharedSchedule( Schedule sched )
		{
			SharedSchedule ss = new SharedSchedule();
			ss.Name = sched.Name;
			ss.HsId = sched.HsId;
			return ss;
		}

		private Employee GetEmployee(SharedEmployee sEmp)
		{
			Employee emp = new Employee();
			emp.PosId = sEmp.PosId;
			emp.LastName = sEmp.LastName;
			emp.FirstName = sEmp.FirstName;
			if(sEmp.HsUserName != null)
				emp.HsUserName = sEmp.HsUserName;
			return emp;
		}

		private Job GetJob(SharedJob sJob)
		{
			Job job = new Job();
			job.Name = sJob.Name;
			job.RoleId = sJob.RoleId;
			job.ExtId = sJob.ExtId;
			return job;
		}
	}

	#endregion


}
