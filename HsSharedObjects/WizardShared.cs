using System;
using System.Collections;

namespace HsSharedObjects
{
	/// <summary>
	/// Summary description for WizardShared.
	/// The WizardSharedData class contains objects and data
	/// necessary on both sides of the connection (service and
	/// console).  it is used statically.
	/// </summary>
	public class WizardSharedData
	{
		private static Hashtable posEmployeeMap = new Hashtable();
		private static ArrayList posJobs = new ArrayList();
		private static ArrayList hsJobs = new ArrayList();
		private static ArrayList hsScheds = new ArrayList();
		private static ArrayList syncJobs = new ArrayList();
		private static Hashtable posEmployeeMapById = new Hashtable();
		private static Hashtable hsEmployeeMap = new Hashtable();
		private static Hashtable syncEmployeeMap = new Hashtable();
		private static Hashtable hsRoleMap = new Hashtable();

		public static Hashtable PosEmployeeMap
		{
			get{ return posEmployeeMap; }
			set{ posEmployeeMap = value; }
		}
		public static Hashtable PosEmployeeMapById
		{
			get{ return posEmployeeMapById; }
			set{ posEmployeeMapById = value; }
		}
		public static Hashtable HsEmployeeMap
		{
			get{ return hsEmployeeMap; }
			set{ hsEmployeeMap = value; }
		}
		public static Hashtable HsRoleMap
		{
			get{ return hsRoleMap; }
			set{ hsRoleMap = value; }
		}
		public static Hashtable SyncEmployeeMap
		{
			get{ return syncEmployeeMap; }
			set{ syncEmployeeMap = value; }
		}
		public static ArrayList PosJobs
		{
			get{ return posJobs; }
			set{ posJobs = value; }
		}
		public static ArrayList HsJobs
		{
			get{ return hsJobs; }
			set{ hsJobs = value; }
		}
		public static ArrayList HsScheds
		{
			get{ return hsScheds; }
			set{ hsScheds = value; }
		}
		public static ArrayList SyncJobs
		{
			get{ return syncJobs; }
			set{ syncJobs = value; }
		}
	}

	/// <summary>
	/// The WizardSharedMethods class is used for method calls and
	/// dtaa manipulation between the service and console, when the
	/// Wizard is executed
	/// </summary>

	public abstract class WizardSharedMethods : MarshalByRefObject
	{
		public Hashtable PosEmployeeMap
		{
			get{ return WizardSharedData.PosEmployeeMap; }
			set{ WizardSharedData.PosEmployeeMap = value; }
		}
		public Hashtable PosEmployeeMapById
		{
			get{ return WizardSharedData.PosEmployeeMapById; }
			set{ WizardSharedData.PosEmployeeMapById = value; }
		}
		public Hashtable HsEmployeeMap
		{
			get{ return WizardSharedData.HsEmployeeMap; }
			set{ WizardSharedData.HsEmployeeMap = value; }
		}
		public Hashtable HsRoleMap
		{
			get{ return WizardSharedData.HsRoleMap; }
			set{ WizardSharedData.HsRoleMap = value; }
		}
		public Hashtable SyncEmployeeMap
		{
			get{ return WizardSharedData.SyncEmployeeMap; }
			set{ WizardSharedData.SyncEmployeeMap = value; }
		}
		public ArrayList PosJobs
		{
			get{ return WizardSharedData.PosJobs; }
			set{ WizardSharedData.PosJobs = value; }
		}
		public ArrayList HsJobs
		{
			get{ return WizardSharedData.HsJobs; }
			set{ WizardSharedData.HsJobs = value; }
		}
		public ArrayList HsScheds
		{
			get{ return WizardSharedData.HsScheds; }
			set{ WizardSharedData.HsScheds = value; }
		}
		public ArrayList SyncJobs
		{
			get{ return WizardSharedData.SyncJobs; }
			set{ WizardSharedData.SyncJobs = value; }
		}

		public abstract void LoadPosData();
		public abstract bool LoadHsData();
		public abstract void LoadJobs();
		public abstract String AddPosEmployee(SharedEmployee emp, bool isNew);
		public abstract String AddHsEmployee(SharedEmployee emp, bool isNew);
		public abstract String AddSyncEmployee(SharedEmployee emp, bool isNew);
		public abstract String RemovePosEmployee(SharedEmployee emp);
		public abstract String RemoveHsEmployee(SharedEmployee emp);
		public abstract String RemoveSyncEmployee(SharedEmployee emp);
		public abstract int[] Synchronize();
		public abstract void AutoSync(bool btn1, bool btn2);
		public abstract void Button12(SharedJob posJob, int posIndex, int index);
		public abstract void AddJob(int posIndex, int hsIndex, SharedJob hsJobs, SharedJob posJob);
		public abstract void DoubleClickPosJob(SharedJob job, int index);
		public abstract void UpdateJobSched(int index, SharedSchedule sched);
		public abstract void Cancel();
		public abstract SharedEmployee ReAddPosEmployee(SharedEmployee emp);
	}
}
