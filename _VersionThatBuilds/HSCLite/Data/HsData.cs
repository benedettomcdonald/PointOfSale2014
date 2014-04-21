using HsSharedObjects.Main;

using System;
using System.Data;

namespace HSCLite.Data
{
	public class HsData
	{
		public HsData()
		{
			logger = new SysLog( this.GetType() );
		}

		private SysLog logger;

		public String GetString( DataRow row , String column )
		{
			String str = "";
			try
			{
				return (String) row[ column ].ToString();
			}
			catch( Exception ex )
			{
				logger.Error( "Error converting data in [" + column + "] to string: " + ex.ToString() );
			}
			return str;
		}

		public DateTime GetDate( DataRow row , String column )
		{
			DateTime date = new DateTime( 1 , 1 , 1 );
			try
			{
				if( row[ column ].ToString().Length < 1 ) return date;
				return (DateTime) row[ column ];
			}
			catch( Exception ex )
			{
				logger.Error( "Error converting data in [" + column + "] to date: " + ex.ToString() );
			}
			return date;
		}

		public int GetInt( DataRow row , String column )
		{
			int num = -1;
			try
			{
				if( row[ column ].ToString().Equals("") ) return -1;
				return Convert.ToInt32( row[ column ] );
			}
			catch( Exception ex )
			{
				logger.Error( "Error converting data in [" + column + "] to int: " + ex.ToString() );
			}
			return num;
		}

		public double GetDouble( DataRow row , String column )
		{
			double num = 0.0;
			try
			{
				if( row[ column ].ToString().Equals("") ) return 0.0;
				return Convert.ToDouble( row[ column ] );
			}
			catch( Exception ex )
			{
				logger.Error( "Error converting data in [" + column + "] to dbl: " + ex.ToString() );
			}
			return num;
		}

		public float GetFloat( DataRow row , String column )
		{
			float num = 0.0f;
			try
			{
				if( row[ column ].ToString().Equals("") ) return 0.0f;
				return (float) Convert.ToDecimal( row[ column ] );
			}
			catch( Exception ex )
			{
				logger.Error( "Error converting data in [" + column + "] to dbl: " + ex.ToString() );
			}
			return num;
		}
	}
}
