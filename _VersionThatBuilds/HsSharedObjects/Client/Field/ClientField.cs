using System;

namespace HsSharedObjects.Client.Field
{
	/// <summary>
	/// Summary description for ClientField.
	/// </summary>
	[Serializable]
	public class ClientField
	{
		public ClientField(){}

		private int id = -1;

		public static int FIRST_NAME = 101;
		public static int LAST_NAME = 102;
		public static int ADDRESS_1 = 103;
		public static int ADDRESS_2 = 104;
		public static int CITY = 105;
		public static int STATE = 106;
		public static int ZIP = 107;
		public static int PHONE = 108;
		public static int SMS = 109;
		public static int EMAIL = 110;
		public static int STATUS = 111;
		public static int BIRTH_DATE = 112;
		public static int NICK_NAME = 113;
		public static int ALT_NUMBER = 114;
		
		public int Id
		{
			get {return this.id;}
			set {this.id = value;}
		}

	}
}
