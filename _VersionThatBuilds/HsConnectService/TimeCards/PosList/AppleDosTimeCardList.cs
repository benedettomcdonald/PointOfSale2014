using HsConnect.Data;
using HsConnect.SalesItems;
using HsConnect.TimeCards.PosList.AppleOneImport;

using System;
using System.Collections;
using System.Data;
using System.IO;
using Microsoft.Data.Odbc;
using System.Threading;

namespace HsConnect.TimeCards.PosList
{
	public class AppleDosTimeCardList : TimeCardListImpl
	{
		public AppleDosTimeCardList() {}

		private HsData data = new HsData();
		private String SQL_INF_PATH = System.Windows.Forms.Application.StartupPath + "\\sql.inf";

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
			//File.Copy( SQL_INF_PATH , IMPORT_PATH + "sql.inf" , true );
			updateFiles();
			Thread.Sleep(2000);
                File.Copy( @"C:\network\touchit\DATA2\EXPORT\TM_CLOCK.CSV" , System.Windows.Forms.Application.StartupPath + "\\TM_CLOCK.CSV" ,true );

			CsvImport import = new CsvImport( System.Windows.Forms.Application.StartupPath + "\\TM_CLOCK.CSV", details );
			import.Init();

			foreach( ArrayList list in import.GetWeekCards() )
			{
				AppleCardComparer comp = new AppleCardComparer();
				list.Sort( comp );

				import.MakeTimeCards( this, list );	
			}

			this.SortByDate();
		}

		public class AppleCardComparer : IComparer  
		{
			int IComparer.Compare( Object x, Object y )  
			{
				AppleOneTimeCard tc1 = (AppleOneTimeCard) x;
				AppleOneTimeCard tc2 = (AppleOneTimeCard) y;
				return ( tc1.ClockIn.CompareTo( tc2.ClockIn ) );
			}
		}
		
		public override void DbUpdate(){}
		
		public override void DbInsert(){}

		private void updateFiles()
		{
			try
			{
				System.Diagnostics.Process proc = new System.Diagnostics.Process();
				proc.EnableRaisingEvents=false;
				proc.StartInfo.WorkingDirectory = @"C:\network\TOUCHIT\DATA2";
				proc.StartInfo.FileName="EXPBIN.EXE";
				proc.Start();
				proc.WaitForExit();
			}
			catch(Exception ex)
			{
				logger.Error("Error running expbin.exe:  " + ex.ToString());
			}
		}
	}
}
