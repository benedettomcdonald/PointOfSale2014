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
	public class AppleOneTimeCardList : TimeCardListImpl
	{
		public AppleOneTimeCardList() {}

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
            try
            {
                File.Copy(@"C:\network\touchit\DATA2\Expbin.inf", @"C:\network\touchit\DATA2\EXPORT\Expbin.inf");
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            int attempts = 20;
            while (attempts > 0)
            {

                Thread.Sleep(2000);
                try
                {
                    if(File.Exists(System.Windows.Forms.Application.StartupPath + "\\TM_CLOCK.CSV"))
                    {
                        logger.Log("File exists");
                        File.Delete(System.Windows.Forms.Application.StartupPath + "\\TM_CLOCK.CSV");
                    }

                    File.Copy(@"C:\network\touchit\DATA2\EXPORT\TM_CLOCK.CSV", System.Windows.Forms.Application.StartupPath + "\\TM_CLOCK.CSV", false);
                    attempts = 0;
                }
                catch (Exception ex)
                {
                    attempts--;
                    logger.Error("error copying file:");
                    logger.Error(ex.ToString());
                }
                
            }
            logger.Debug("finished copying files");
            CsvImport import = new CsvImport(System.Windows.Forms.Application.StartupPath + "\\TM_CLOCK.CSV", details);
            try
            {
                import.Init();
            }
            catch (Exception ex)
            {
                logger.Error("error in CSVImport init");
                logger.Error(ex.ToString());
            }

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
