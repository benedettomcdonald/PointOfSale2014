using System;

namespace HsConnect.Employees
{
	public class PhoneNumber
	{
		public PhoneNumber(){}

		public PhoneNumber( String number )
		{			
			String nums = "";
			char[] chars = number.ToCharArray();
			foreach( char c in chars )
			{
				if( Char.IsDigit( c ) ) nums += c.ToString();
			}
			if( nums.Length >= 3 ) area = nums.Substring( 0 , 3 );
			if( nums.Length >= 7 ) prefix = nums.Substring( 3 , 3 );
			if( nums.Length >= 10 ) this.number = nums.Substring( 6 , 4 );
		}

		private string area = "111";
		private string prefix = "111";
		private string number = "1111";

		public string Area
		{
			get { return this.area; }
			set { this.area = value; }
		}

		public string Prefix
		{
			get { return this.prefix; }
			set { this.prefix = value; }
		}

		public string Number
		{
			get { return this.number; }
			set { this.number = value; }
		}

		public override String ToString()
		{
			return area.ToString() + prefix.ToString() + number.ToString();
		}
	}
}
