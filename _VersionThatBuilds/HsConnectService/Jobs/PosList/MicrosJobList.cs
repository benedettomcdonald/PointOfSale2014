using HsConnect.Data;

using System;
using System.Data;
using Microsoft.Data.Odbc;

namespace HsConnect.Jobs.PosList
{
	public class MicrosJobList : JobListImpl
	{
		public MicrosJobList(){}

		private HsData data = new HsData();

		public override void DbInsert(){}
		public override void DbUpdate(){}

		public override void DbLoad()
		{
			OdbcConnection newConnection = this.cnx.GetOdbc();
			try
			{
				DataSet dataSet = new DataSet();
				OdbcDataAdapter dataAdapter = new OdbcDataAdapter(
					"SELECT a.obj_num , a.deflt_reg_pay_rate , b.deflt_otm_pay_rate , a.name  FROM micros.job_def a , micros.job_otm_lvl_def b " +
					"WHERE a.job_seq = b.job_seq" , newConnection );
				dataAdapter.Fill( dataSet , "micros.job_def" );
				dataAdapter.Dispose();
				DataRowCollection rows = dataSet.Tables[0].Rows;
                if (rows.Count < 1)
                {
                    logger.Debug("Returned 0 rows");
                    rows = getAltRows(newConnection, dataSet);
                    logger.Debug("Alt rows = " + rows.Count);
                }
				foreach( DataRow row in rows )
				{
					try
					{
						Job job = new Job();
						job.ExtId = data.GetInt( row , "obj_num" );
						job.Name = data.GetString( row , "name" );
						job.DefaultWage = data.GetDouble( row , "deflt_reg_pay_rate" );
                        try
                        {
                            job.OvtWage = data.GetDouble(row, "deflt_otm_pay_rate");
                        }
                        catch (Exception ex)
                        {
                            job.OvtWage = 0.0f;
                        }
						this.Add( job );
					}
					catch( Exception ex ) 
					{
						logger.Error( ex.ToString() );
					//	Main.Run.errorList.Add(ex);
					}
				}
			}
			catch( Exception ex ) 
			{
				logger.Error( ex.ToString() );
				Main.Run.errorList.Add(ex);
			}
			finally
			{
				newConnection.Close();
			}
		}
        private DataRowCollection getAltRows(OdbcConnection newConnection, DataSet dataSet)
        {
            OdbcDataAdapter dataAdapter = new OdbcDataAdapter(
                                "SELECT obj_num , deflt_reg_pay_rate ,  0 as deflt_otm_pay_rate , name  FROM micros.job_def ", newConnection);
            dataAdapter.Fill(dataSet, "micros.emp_def");
            dataAdapter.Dispose();
            DataRowCollection rows = dataSet.Tables[0].Rows;
            return rows;
        }
	}
}
