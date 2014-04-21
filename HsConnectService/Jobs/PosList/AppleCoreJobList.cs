using HsConnect.Data;

using System;
using System.Data;
using System.Collections;
using System.IO;
using Microsoft.Data.Odbc;

namespace HsConnect.Jobs.PosList
{
	public class AppleCoreJobList : JobListImpl
	{
		public AppleCoreJobList(){}

		private HsData data = new HsData();

		public override void DbInsert(){}
		public override void DbUpdate(){}

		public override void DbLoad()
		{
			StreamReader text = null;
						
			try
			{
				long s = DateTime.Now.Millisecond;
					
				text = File.OpenText( System.Windows.Forms.Application.StartupPath + "\\PAYDPT28.CSV" );
				
				while( text.Peek() > 0 )
				{
					String inStr = text.ReadLine();
					String[] strArray = LineToArray( inStr );
					
					try
					{
						Job job = new Job();
						job.ExtId = Convert.ToInt32( strArray[0] );
						job.Name = strArray[2];
						this.Add( job );
					}
					catch( Exception ex )
					{
						logger.Error( ex.ToString() );
					}
					
				}
			}
			finally
			{
				text.Close();
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
	}
}
