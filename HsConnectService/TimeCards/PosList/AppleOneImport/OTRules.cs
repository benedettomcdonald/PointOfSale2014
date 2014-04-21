using System;
using System.Collections;

namespace HsConnect.TimeCards.PosList.AppleOneImport
{
	public interface OTRules
	{
		void TransferCards( TimeCardList timeCards, ArrayList appleCards );
	}
}
