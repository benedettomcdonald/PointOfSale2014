using System;
using System.Collections;

namespace HsConnect.Data.Punch2
{
	public class Tokens : IEnumerable
	{
		private string[] elements;

		public Tokens(string source, char[] delimiters)
		{
			elements = source.Split(delimiters);
		}

		public IEnumerator GetEnumerator()
		{
			return new TokenEnumerator(this);
		}

		private class TokenEnumerator : IEnumerator
		{
			private int position = -1;
			private Tokens t;

			public TokenEnumerator(Tokens t)
			{
				this.t = t;
			}

			public bool MoveNext()
			{
				if (position < t.elements.Length - 1)
				{
					position++;
					return true;
				}
				else
				{
					return false;
				}
			}

			public void Reset()
			{
				position = -1;
			}

			public object Current
			{
				get
				{
					return t.elements[position];
				}
			}
		}
	}
}
