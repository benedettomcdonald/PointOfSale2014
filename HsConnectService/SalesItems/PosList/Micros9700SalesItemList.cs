using HsConnect.Data;
using HsConnect.SalesItems;

using System;
using System.Data;
using System.Collections;
using System.Globalization;
using Microsoft.Data.Odbc;

namespace HsConnect.SalesItems.PosList
{
	public class Micros9700SalesItemList : SalesItemListImpl
	{
		public Micros9700SalesItemList() {}

		private HsData data = new HsData();		
		//STATICS//
		private static String SALES_FILE = "clsd_chk_ttls";
		/*RevenueCenter, CHECK_NUM, DATE, SUB_TOTAL correspond to the columns in the 
		 * final sales item table.  This must be updated if the query is changed
		 */ 
		private static String RVC = "Row0";
		private static String CHECK_NUM = "Row1";
		private static String DATE = "Row2";
		private static String SUB_TOTAL = "Row3";
		private static String REOPENED_TO = "Row4";
		private static String XFER_TO = "Row5";
		private static String ORDER_TYPE = "Row6";

		/*public float GetSalesTotal
		 * 
		 * Currently a placeholder with a console.writeline
		 * 
		 */
		public override float GetSalesTotal()
		{
			Console.WriteLine("This is the 9700 GetSalesTotal method for SalesItemList");
			return 0;
		}

		/*public void DbLoad
		 * Called with Sales Sync.  This method will query the DB for the valid
		 * RVCs in the system, and from there, will dynamically create a .sql file
		 * to query for all of the sales items.  From there it will take the
		 * resulting CSVs and create a DataTable, which it will parse to create
		 * the SalesItems  
		 */
		public override void DbLoad()
		{
		//	Hashtable idHash = new Hashtable();
			ArrayList csvNums = new ArrayList();//will keep the valid RVCs' IDs

			//need to get correct RVCs and load sales items for them.
            //STEP 0: create a custom SQL file for querying valid RVCs within the correct PERIOD
            Micros9700Control.CreateSQLFile( Micros9700Control.RVC, csvNums );
			//STEP 1:  Query for valid RVCs (valid means they have a sales total > 0)
			Micros9700Control.Run( Micros9700Control.RVC_SYNC );
			//STEP 2:  create a custom SQL file based on the present RVCs
			Micros9700Control.CreateSQLFile(Micros9700Control.SALES, csvNums);
            //delete existing files
            Micros9700Control.ClearFiles(Micros9700Control.SALES, csvNums);
			//STEP 3:  run custom SQL file
			Micros9700Control.Run( Micros9700Control.SALES_SYNC );
            try
            {
                //STEP 4:  combine csv files into one big one
                Micros9700Control.CombineFiles(Micros9700Control.SALES, csvNums);

                //We have the data in one CSV, now we parse it and make SalesItems
                DataTableBuilder builder = new DataTableBuilder();
                DataTable dt = builder.GetTableFromCSV(Micros9700Control.PATH_NAME, Micros9700Control.SALES_FILE);
                DataRowCollection rows = dt.Rows;

                foreach (DataRow row in rows)//for each sales item in the table
                {
                    int reopenedTo = data.GetInt(row, REOPENED_TO);
                    int xferTo = data.GetInt(row, XFER_TO);
                    int orderType = data.GetInt(row, ORDER_TYPE);
                    if (reopenedTo == 0 && xferTo == 0 && orderType > 0)
                    {
                        SalesItem item = new SalesItem();

                        //Parse the date from the table and split it up
                        string date = row[DATE].ToString();
                        DateTimeFormatInfo myDTFI = new CultureInfo("en-US", false).DateTimeFormat;
                        DateTime nDate = DateTime.Parse(date);
                        item.DayOfMonth = nDate.Day;
                        item.Month = nDate.Month;
                        item.Year = nDate.Year;
                        item.Hour = nDate.Hour;
                        item.Minute = nDate.Minute;

                        item.Amount = data.GetDouble(row, SUB_TOTAL);
                        item.RVC = data.GetInt(row, RVC);

					//create a "unique" extID.  To do this, we use RevenueCenter number, Check number, minute and hour
                        long tempID = Convert.ToInt64(row[RVC].ToString() + row[CHECK_NUM].ToString() + item.Minute.ToString() + item.Hour.ToString());
                        item.ExtId = tempID;

                        //if the id is not already in the hash, add it to hash and list
                        //if( !idHash.ContainsKey( item.ExtId + "" ) )
                        //	{
                        this.Add(item);
                        //		idHash.Add( item.ExtId + "", item );
                        //	}
                        //	else
                        //		{
                        //			Console.WriteLine( "duplicate ID" );
                        //		}
                    }
                    else
                    {
                        Console.WriteLine(data.GetDouble(row, SUB_TOTAL) + " , " + reopenedTo + " , " + xferTo);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error parsing csv files for sales:  possibly no files");
                logger.Error(ex.ToString());
                logger.Error(ex.StackTrace);
            }
		}//end DbLoad

		#region unused methods
		/*public void DbUpdate
		 * 
		 * Currently a placeholder with a console.writeline
		 * 
		 */
		public override void DbUpdate()
		{
			Console.WriteLine("This is the 9700 DBUpdate method for SalesItemList");		
		}
		/*public void DbInsert
		 * 
		 * Currently a placeholder with a console.writeline
		 * 
		 */
		public override void DbInsert()
		{
			Console.WriteLine("This is the 9700 DBInsert method for SalesItemList");
		}
		#endregion
	}
}
