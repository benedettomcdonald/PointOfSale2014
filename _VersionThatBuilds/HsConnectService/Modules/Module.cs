using System;
using System.Timers;

namespace HsConnect.Modules
{
	public interface Module
	{
		bool Execute();
		bool AutoSync{ get;set; }
	}
}
