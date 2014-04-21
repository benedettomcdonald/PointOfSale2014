using System;
using System.Runtime.Serialization;

namespace HsExecuteModule
{
	/// <summary>
	/// Summary description for Command.
	/// </summary>
	[Serializable]
	public class Command
	{
		public Command(String d, String c, String a)
		{
			dir = d;
			cmd = c;
			args = a;
		}

		private String dir;
		private String cmd;
		private String args;

		public String Dir
		{
			get{return this.dir;}
			set{this.dir = value;}
		}
		public String Cmd
		{
			get{return cmd;}
			set{cmd = value;}
		}
		public String Args
		{
			get{return args;}
			set{args = value;}
		}

	}
}
