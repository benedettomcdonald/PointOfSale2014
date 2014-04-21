using HsConnect.Data;
using HsConnect.Forms;
using HsConnect.SalesItems;

using System;
using System.Data;
using System.Collections;
using Microsoft.Data.Odbc;
using pslbr;

namespace HsConnect.TimeCards.PosList
{
	public class Old_PosiTimeCardList : TimeCardListImpl
	{
		public Old_PosiTimeCardList() {}

		private HsData data = new HsData();
		private Hashtable jobs = new Hashtable();
		private DateTime dateToSync = new DateTime(0);

		public override bool PeriodLabor
		{
			get
			{
				return this.periodLabor;
			}
			set
			{
				this.periodLabor = value;
			}
		}

		public override void DbLoad()
		{
			try
			{
				if( !this.AutoSync )
				{
					SalesDateForm dateForm = new SalesDateForm();
					dateForm.MaxDate = DateTime.Today;
					dateForm.ShowDialog();
					if( dateForm.Cancel ) return;
					this.dateToSync = dateForm.GetSalesStartDate();
				}
				else this.dateToSync = DateTime.Now.AddHours( -2.0 );

				DataTableBuilder builder = new DataTableBuilder();
				// load current job list
				DataTable dt = builder.GetTableFromDBF( @"C:\SC", @"C:\", "JOBLIST" );
				DataRowCollection rows = dt.Rows;
				foreach( DataRow row in rows )
				{
					try
					{
						if( !jobs.ContainsKey(data.GetString( row , "ALT_CODE" )) ) jobs.Add( data.GetString( row , "ALT_CODE" ), data.GetString( row , "JOB_CODE" ) );
					}
					catch( Exception ex )
					{
						logger.Error( ex.ToString() );
					}
				}

				PosiControl.Run( "TARW","-R 52 4 " + dateToSync.ToString("yyMMdd") + " " + dateToSync.ToString("yyMMdd") );
				dt = builder.GetTableFromDBF( @"C:\DBF", @"C:\", "PAYRJOBD" );
				rows = dt.Rows;
				foreach( DataRow row in rows )
				{
					try
					{
						TimeCard timeCard = new TimeCard();
						timeCard.ExtId = GetExtId( data.GetInt( row , "EMPL_NUM" ),data.GetInt( row , "JOB" ),data.GetInt( row , "PAY_TYPE" ) ); 
						timeCard.EmpPosId = data.GetInt( row , "EMPL_NUM" );
						timeCard.JobName = data.GetString( row , "JOB" );
						timeCard.JobExtId = Convert.ToInt32( jobs[ data.GetString( row , "JOB" ) ] );
						timeCard.RegHours = GetHours( 1, data.GetInt( row , "PAY_TYPE" ), data.GetFloat( row , "HOURS" ) );
						timeCard.RegTotal = GetDollars( 1, data.GetInt( row , "PAY_TYPE" ), data.GetFloat( row , "PAY" ) );
						timeCard.OvtHours = GetHours( 3, data.GetInt( row , "PAY_TYPE" ), data.GetFloat( row , "HOURS" ) );
						timeCard.OvtTotal = GetDollars( 3, data.GetInt( row , "PAY_TYPE" ), data.GetFloat( row , "PAY" ) );
						timeCard.BusinessDate = dateToSync.Date;
						timeCard.ClockIn = GetStdClockIn();
						timeCard.ClockOut = GetStdClockIn().AddHours( data.GetFloat( row , "HOURS" ) );
						timeCard.OvertimeMinutes = (int) Math.Round( GetHours( 3, data.GetInt( row , "PAY_TYPE" ), data.GetFloat( row , "HOURS" ) ) * 60 , 0 );
						this.Add( timeCard );
					}
					catch( Exception ex )
					{
						logger.Error( "Error adding micros time card in Load(): " + ex.ToString() );
					}
				}
			}
			catch( Exception ex )
			{
				logger.Error( ex.ToString() );
			}
			this.SetWages();
		}

		private DateTime GetStdClockIn()
		{
			return new DateTime( dateToSync.Year, dateToSync.Month, dateToSync.Day, 10, 0, 0 );
		}

		private float GetHours( int outType, int inType, float data )
		{
			if( outType == inType ) return data;
			if( outType == 2 && inType == 3 ) return data;
			return 0.0f;
		}

		private float GetDollars( int outType, int inType, float data )
		{
			if( outType == inType ) return data;
			if( outType == 3 && inType == 2 ) return data;
			return 0.0f;
		}

		private long GetExtId( int empId, int jobId, int type )
		{
			String tmp = dateToSync.ToString( "MMddyyyy" );
			return Convert.ToInt64( tmp + empId.ToString() + jobId.ToString() + type.ToString() );
		}

		public override void DbUpdate(){}
		public override void DbInsert(){}
	}
}
