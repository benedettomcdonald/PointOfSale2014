using System;
using HsSharedObjects.Client;
using HsSharedObjects.Client.Module;
using HsSharedObjects.Client.CustomModule;

namespace HsSharedObjects
{

	/**
	 * HsSharedObjects
	 * HsCnxData:  This is a static class (never instantiated) that allows for 
	 * certain important pieces of data to be persisted and passed between
	 * the service and console processes
	 * 
	 * 
	 */ 
	public abstract class HsCnxData
	{

		private static bool loggedIn;
		private static String usrName, pwd;
		private static ClientDetails details;
		private static String macAddress;
		private static DateTime startDate;
		private static DateTime endDate;

		public static bool LoggedIn
		{
			get{ return loggedIn; }
			set{ loggedIn = value; }
		}

		public static String UsrName
		{
			get{ return usrName; }
			set{ usrName = value; }
		}

		public static String Pwd
		{
			get{ return pwd; }
			set{ pwd = value; }
		}

		public static ClientDetails Details
		{
			get{ return details; }
			set{ details = value; }
		}

		public static String MacAddress
		{
			get{ return macAddress; }
			set{ macAddress = value; }
		}

		public static DateTime StartDate
		{
			get{ return startDate; }
			set{ startDate = value; }
		}

		public static DateTime EndDate
		{
			get{ return endDate; }
			set{ endDate = value; }
		}
	}

	public abstract class HsCnxMethods : MarshalByRefObject
	{
		public bool LoggedIn
		{
			get{ return HsCnxData.LoggedIn; }
			set{ HsCnxData.LoggedIn = value; }
		}

		public void SetLoggedIn(bool val)
		{
			HsCnxData.LoggedIn = val;
		}

		public String UsrName
		{
			get{ return HsCnxData.UsrName; }
			set{ HsCnxData.UsrName = value; }
		}

		public String Pwd
		{
			get{ return HsCnxData.Pwd; }
			set{ HsCnxData.Pwd = value; }
		}

		public ClientDetails Details
		{		
			get{ return HsCnxData.Details; }
			set{ HsCnxData.Details = value; }
		}

		public String MacAddress
		{
			get{ return HsCnxData.MacAddress; }
			set{ HsCnxData.MacAddress = value; }
		}

		public DateTime StartDate
		{
			get{ return HsCnxData.StartDate; }
			set{ HsCnxData.StartDate = value; }
		}

		public DateTime EndDate
		{
			get{ return HsCnxData.EndDate; }
			set{ HsCnxData.EndDate = value; }
		}

		public abstract String login(String name, String pwd);
		public abstract bool login(int clientId, bool login);
		public abstract bool Sync(ClientModule mod);
		public abstract bool Sync(ClientCustomModule mod);
		public abstract bool PrintEES(String str);
		public abstract void ForceSync(int modId);
		public abstract bool UpdateDetails();
	}

	public abstract class AbstractBroadcastedMessageEventSink : 
		MarshalByRefObject
	{
		public void myCallback(string str)
		{
			internalcallback(str);
		}
		protected abstract void internalcallback (string str) ;
	}
}