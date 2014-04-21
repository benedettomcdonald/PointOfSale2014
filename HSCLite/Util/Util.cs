using System;

namespace HSCLite.Util
{
	/// <summary>
	/// interface used for different clients for the HSCLite synch
	/// </summary>
	public interface Util
	{
		bool NeedsStoreNumber{ get; }
		bool RunSynch();
		int GetStoreNumber();
	}
}
