using System;
using System.Collections;
using HsSharedObjects.Client;

namespace HsExecuteModule
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public interface Execute
	{
		void RunCommand(Command cmd);
		void Execute(bool map);

		ArrayList Commands{ get;set; }
		String GetXmlString();
		int Count{ get;set; }
	    ClientDetails ClientDetails { get; set; }
	}
}
