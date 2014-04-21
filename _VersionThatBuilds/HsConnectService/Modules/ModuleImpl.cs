using HsConnect.Main;
using HsSharedObjects.Client;
using HsSharedObjects.Client.Module;

using System;
using System.Timers;

namespace HsConnect.Modules
{
	public abstract class ModuleImpl : Module
	{
		public ModuleImpl()
		{
			this.logger = new SysLog( this.GetType() );
		}

		private ClientDetails details;
		private SysLog logger;
		private bool autoSync = false;

		public abstract bool Execute();

		public ClientDetails Details
		{
			get { return this.details; }
			set { this.details = value; }
		}

		public bool AutoSync
		{
			get{ return this.autoSync; }
			set{ this.autoSync = value; }
		}

	}
}
