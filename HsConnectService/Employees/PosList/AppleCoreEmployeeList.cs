using HsConnect.Data;

using System;
using System.Data;
using System.Collections;
using System.Threading;
using System.IO;
using Microsoft.Data.Odbc;

namespace HsConnect.Employees.PosList
{
	public class AppleCoreEmployeeList : EmployeeListImpl
	{
		public AppleCoreEmployeeList(){}

		private HsData data = new HsData();

        public override void DbInsert(GoHireEmpInsertList ls) { }

		public override void DbLoad()
		{
			this.DbLoad( false );
		}

		public override void DbLoad( bool activeOnly )
		{
			StreamReader text = null;
			if( !File.Exists( System.Windows.Forms.Application.StartupPath + "\\EMP28.CSV" ) )
			{
				File.Copy( @"C:\network\touchit\DATA2\EXPORT\EMP28.CSV" , System.Windows.Forms.Application.StartupPath + "\\EMP28.CSV" );
			}
			
			try
			{
				text = File.OpenText( System.Windows.Forms.Application.StartupPath + "\\EMP28.CSV" );
				int lineIndex = 0;
				while( text.Peek() > 0 )
				{
					String inStr = text.ReadLine();
					String[] strArray = LineToArray( inStr );

					try
					{
						if( lineIndex > 0 )
						{
							Employee emp = new Employee();
							emp.PosId = Convert.ToInt32( strArray[0] );
							emp.LastName = strArray[3];
							emp.FirstName = strArray[2];
							emp.Address1 = strArray[6];
							emp.Address2 = strArray[7];
							emp.City = strArray[8];
							emp.State = strArray[9];
							emp.ZipCode = strArray[10];
							emp.Status = new EmployeeStatus();

							System.IFormatProvider frmt = new System.Globalization.CultureInfo("en-US", true);
							emp.BirthDate = DateTime.ParseExact( strArray[12],"yyyyMMdd", frmt);

							if( strArray[14].Length < 2 )
							{
								emp.Status.StatusCode = EmployeeStatus.ACTIVE;
							} 
							else emp.Status.StatusCode = EmployeeStatus.TERMINATED;
							if( !activeOnly || (emp.Status.StatusCode == EmployeeStatus.ACTIVE) )
							{
								this.Add( emp );
							}
						}
					}
					catch( Exception ex )
					{
						logger.Error( ex.ToString() );
					}

					lineIndex++;
				}
			}
			finally
			{
				text.Close();
			}

			if( File.Exists( System.Windows.Forms.Application.StartupPath + "\\EMP28.CSV" ) )
			{
				File.Delete( System.Windows.Forms.Application.StartupPath + "\\EMP28.CSV" );
			}
		}

		private String[] LineToArray( String line )
		{
			int startIndex = 0;
			int length = 0;
			ArrayList strings = new ArrayList();
			while( startIndex < line.Length )
			{
				if( line.Substring( startIndex, 1 ).CompareTo( "\"" ) == 0 )
				{
					length = line.IndexOf( "\"," ,startIndex+1 ) - (startIndex+1);
					length = length < 0 ? (line.Length-1) - (startIndex+1) : length;
					String s = line.Substring( startIndex+1, length );
					strings.Add( s );
					startIndex += (length + 3);
				}
				else
				{
					if(line.Substring(startIndex, 1).CompareTo(",")==0)
					{
						strings.Add("");
						startIndex++;
						continue;
					}
					length = line.IndexOf( "," ,startIndex+1 ) - startIndex;
					length = length < 0 ? line.Length - startIndex : length;
					String s = line.Substring( startIndex, length );
					strings.Add( s );
					startIndex += (length + 1);
				}
			}
			String[] strs = new String[strings.Count];			
			int cnt = 0;
			foreach( String str in strings )
			{
				strs[cnt] = str.Trim();
				cnt++;	
			}
			return strs;
		}

		public override void DbUpdate(){}
		public override void DbInsert(){}
	}
}
