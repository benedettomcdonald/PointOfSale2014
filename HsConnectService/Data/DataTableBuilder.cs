using ConvertDBF;
using HsConnect.Main;

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;

namespace HsConnect.Data
{
	public class DataTableBuilder
	{
		public DataTableBuilder()
		{
			logger = new SysLog( this.GetType() );
		}

		private SysLog logger;
		private static String CSV_REGEX = ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))";
		private static String DBF_REGEX = "','";

		/**This method will take an unformatted dbf file and will create a csv from it.
		 * From here, it will create a DataTable, which is formatted for use in HSConnect.
		 **/
		public DataTable GetTableFromDBF( String srcPath, String destPath, String dbfFile )
		{
			if( !Directory.Exists( destPath+@"\hstmp" ) ) Directory.CreateDirectory( destPath+@"\hstmp" );
            String csvFile = destPath+@"\hstmp\"+dbfFile+".CSV";
			try
			{
				logger.Debug( "Trying " + csvFile );
				// create csv file
				DBFBuilder db = new DBFBuilder( srcPath+@"\"+dbfFile+".DBF" , csvFile );
				db.process();
				logger.Debug( "Built " + csvFile );				
			}
			catch( Exception ex )
			{
				logger.Debug( ex.ToString() );
			}

			StreamReader reader;
			bool currRec = false;
			DataTable dt = null;
			try
			{
				reader = new StreamReader( csvFile );
				String data = "";
				String[] dataArray;					

				while (0 < reader.Peek())
				{
					data = reader.ReadLine();
					dataArray = LineToArray(data);				

					if( !currRec )
					{
						currRec = true;
						dt = new DataTable( dbfFile );
						for (int i = 0; i < dataArray.Length; i++)
						{
							dt.Columns.Add( CleanString( dataArray[i].ToString() ) );
						}
					} 
					else
					{
						AddRow(dt, dataArray, false);
					}
				}
				reader.Close();
			}
			catch( Exception ex )
			{
				logger.Error( ex.ToString() );
			}
			return dt;
		}

		/*This method will take the name of a csv file and
		* will attempt to create a DataTable from it.  This
		* DataTable will be used to get data into the 
		* HSConnect objects associated with the csv file. 
		* */
		public DataTable GetTableFromCSV(String srcPath, String fileName)
		{	
			return GetTableFromCSV(srcPath, fileName, CSV_REGEX);//uses generic csv-parsing regex if one isnt provided
		}
		public DataTable GetTableFromCSV(String srcPath, String fileName, String pattern)
		{
			StreamReader reader;
			bool currRec = false;
			DataTable dt = null;
			String csvName = srcPath+fileName+".csv";
			try
			{
				reader = new StreamReader( csvName);
				StringBuilder data;
				String[] dataArray;					
				while (0 < reader.Peek())
				{
					data = new StringBuilder(reader.ReadLine());
					dataArray = LineToArray(data.ToString(), pattern);
					if( !currRec )//if this is line 1, the table's columns need to be created
					{
						currRec = true;
						dt = new DataTable(fileName);
						for (int i = 0; i < dataArray.Length; i++)
						{
							dt.Columns.Add("Row" + i.ToString());
						}
					} 
					//then we can add the row from the dataArray
					AddRow(dt, dataArray, true);
				}
				reader.Close();
			}
			catch( Exception ex )
			{
				logger.Error( ex.ToString() );
			}
			return dt;
		}
		
		/*Edited version of original LineToArray(String)
		 * This is now overloaded by LineToArray(String, String).
		 * This method maintains its original functionality
		 * by calling LineToArray(string, String) with a pattern
		 * string of "','".
		 * */
		private String[] LineToArray( String line )
		{
			String pattern = DBF_REGEX;
            return LineToArray(line, pattern);
		}
		/*This is a line to array method using the provided pattern as its
		 * regular expression
		 */
		private String[] LineToArray( String line, String pattern)
		{
			Regex r = new Regex(pattern); 
			string[] tmp = r.Split(line); 
			if( tmp[0].Length > 0 && tmp[0].IndexOf("'") == 0 )
			{
				tmp[0] = tmp[0].Substring( tmp[0].IndexOf("'")+1, tmp[0].Length-1 );
			}
			if( tmp[tmp.Length-1].Length > 0 && tmp[tmp.Length-1].LastIndexOf("'") == tmp[tmp.Length-1].Length-1 )
			{
				tmp[tmp.Length-1] = tmp[tmp.Length-1].Substring( 0, tmp[tmp.Length-1].LastIndexOf("'") );
			}
			return tmp;
		}

		/*
		 * this is an edited version of the old method, it now is overloaded and calls
		 * cleanstring(string, bool) with a bool value of false, maintaining its old
		 * functionality
		 * 
		 * */
		private String CleanString( String str )
		{
			return CleanString(str, false);
		}
		/*Overloaded version of the original CleanString(string)
		 * this version allows for a check on double quotes ("")
		 * as well as single quotes ('') 
		 * 
		 * a bool value of true causes a check for double quotes
		 * 
		 */ 
		private String CleanString( String str , bool doubleQuotes)
		{
			if( str.IndexOf("'") == 0 && str.LastIndexOf("'")+1 == str.Length )
			{
				return str.Substring( str.IndexOf("'")+1, str.LastIndexOf("'")-1 ).Trim();
			} 
			else if(doubleQuotes &&( str.IndexOf("\"") == 0 && str.LastIndexOf("\"")+1 == str.Length ))//this is an addition
			{
				return str.Substring( str.IndexOf("\"")+1, str.LastIndexOf("\"")-1 ).Trim();
			} 
			else
			{
				return str.Trim();
			}
		}
		/**This method adds a row to the given data table containing the data
		 * in the dataArray.  The boolean parameter tells CleanString if it
		 * needs to worry about double quotes ("") 
		 **/
		private void AddRow(DataTable dt, String[] dataArray, bool checkDoubleQuotes)
		{
			DataRow dr = dt.NewRow();
			for (int i = 0; i < dataArray.Length; i++)
			{
				if( i <= (dt.Columns.Count - 1) )
				{
					try
					{
						dr[i] = CleanString( dataArray[i], checkDoubleQuotes );
					}
					catch( Exception ex )
					{
						logger.Error( ex.ToString() );
					}
				}
			}
			dt.Rows.Add( dr );
		}
	}
}
