using HsConnect.Data;
using HsConnect.Jobs;
using HsConnect.Services;

using System;
using System.Data;
using System.IO;
using System.Collections;
using Microsoft.Data.Odbc;

namespace HsConnect.EmpJobs.PosList
{
	public class AppleCoreEmpJobList : EmpJobImpl
	{
		public AppleCoreEmpJobList(){}

		private HsData data = new HsData();
		private JobList hsJobList;

		public override void DbInsert(){}
		public override void DbUpdate(){}

		public override void SetHsJobList(JobList hsJobs)
		{
			this.hsJobList = hsJobs;
		}

		public override void DbLoad()
		{
			StreamReader text = null;
			bool copied = false;
			//if( !File.Exists( System.Windows.Forms.Application.StartupPath + "\\EMP28.CSV" ) )
			//{
			try
			{
				File.Copy( @"C:\network\touchit\DATA2\EXPORT\EMP28.CSV" , System.Windows.Forms.Application.StartupPath + "\\EMP28.CSV" );
				copied = true;
			}
			catch(Exception ex)
			{
				logger.Log("could not copy EMP28.CSV, reading from export instead");
				copied = false;
			}
			//}
			Hashtable empJobs = new Hashtable();
			try
			{
				if(copied)
				{
					text = File.OpenText( System.Windows.Forms.Application.StartupPath + "\\EMP28.CSV" );
				}
				else
				{
					try
					{
						text = File.OpenText( @"C:\network\touchit\DATA2\EXPORT\EMP28.CSV" );
					}
					catch( Exception ex)
					{
						text = File.OpenText( System.Windows.Forms.Application.StartupPath + "\\EMP28.CSV" );
					}
				}
				int lineIndex = 0;
				while( text.Peek() > 0 )
				{
					String inStr = text.ReadLine();
					String[] strArray = LineToArray( inStr );

					try
					{
						if( lineIndex > 0 )
						{
							for(int i=0;i<30;i++)
							{
								EmpJob empJob = new EmpJob();
								empJob.PosId = Convert.ToInt32( strArray[0] );
								empJob.JobCode = Convert.ToInt32( strArray[(58 + i)] );
								empJob.RegWage = (float) Convert.ToDouble( strArray[(28 + i)] );	
								if( hsJobList != null && hsJobList.GetJobByExtId( empJob.JobCode ) != null ) empJob.HsJobId = hsJobList.GetJobByExtId( empJob.JobCode ).HsId;
								this.Add( empJob );
								if ( !empJobs.ContainsKey( empJob.PosId + " | " + empJob.JobCode ) ) empJobs.Add( empJob.PosId + " | " + empJob.JobCode , empJob );							
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
	}
}
