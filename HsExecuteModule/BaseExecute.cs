using System;
using System.Collections;
using System.Threading;
using HsSharedObjects.Client;
using HsSharedObjects.Main;
using System.Runtime.Serialization;
using System.Data;

namespace HsExecuteModule
{
	/// <summary>
	/// Summary description for BaseExecute.
	/// </summary>
	[Serializable]
	public class BaseExecute : Execute
	{
		public BaseExecute()
		{
			logger = new SysLog( this.GetType() );
		}

		protected SysLog logger;
		protected ArrayList commands;
		int count;
	    private ClientDetails clientDetails;

		public virtual void RunCommand(Command cmd)
		{
			System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.WorkingDirectory = cmd.Dir;
			proc.StartInfo.Arguments = cmd.Args;
			proc.StartInfo.FileName = cmd.Cmd;
            logger.Log("Attempting to execute \"" + cmd.Cmd + " " + cmd.Args + "\" in directory \"" + cmd.Dir + "\"" );
			proc.Start();
			proc.WaitForExit();
		}

		public virtual void Execute( bool map )
		{
			foreach(Command cmd in commands)
			{
				try
				{
					RunCommand(cmd);
				}	
				catch( Exception ex )
				{
					logger.Error( ex.ToString() );
				}
			}
		}

		public String GetXmlString(){return "";}

		public ArrayList Commands{ 
			get{ return this.commands; }
			set{ this.commands = value; }
		}

		public int Count{ 
			get{ return this.count; }
			set{this.count = value; } 
		}							  

	    public ClientDetails ClientDetails
	    {
	        get { return clientDetails; }
	        set { clientDetails = value; }
    	}

        /* data getters */

        public String GetString(DataRow row, String column)
        {
            String str = "";
            try
            {
                return (String)row[column].ToString();
            }
            catch (Exception ex)
            {
                logger.Error("Error converting data in [" + column + "] to string: " + ex.ToString());
            }
            return str;
        }

        public DateTime GetDate(DataRow row, String column)
        {
            DateTime date = new DateTime(1, 1, 1);
            try
            {
                if (row[column].ToString().Length < 1) return date;
                return (DateTime)row[column];
            }
            catch (Exception ex)
            {
                logger.Error("Error converting data in [" + column + "] to date: " + ex.ToString());
            }
            return date;
        }

        public int GetInt(DataRow row, String column)
        {
            int num = -1;
            try
            {
                if (row[column].ToString().Equals("")) return -1;
                return Convert.ToInt32(row[column]);
            }
            catch (Exception ex)
            {
                logger.Error("Error converting data in [" + column + "] to int: " + ex.ToString());
            }
            return num;
        }

        public double GetDouble(DataRow row, String column)
        {
            double num = 0.0;
            try
            {
                if (row[column].ToString().Equals("")) return 0.0;
                return Convert.ToDouble(row[column]);
            }
            catch (Exception ex)
            {
                logger.Error("Error converting data in [" + column + "] to dbl: " + ex.ToString());
            }
            return num;
        }

        public float GetFloat(DataRow row, String column)
        {
            float num = 0.0f;
            try
            {
                if (row[column].ToString().Equals("")) return 0.0f;
                return (float)Convert.ToDecimal(row[column]);
            }
            catch (Exception ex)
            {
                logger.Error("Error converting data in [" + column + "] to dbl: " + ex.ToString());
            }
            return num;
        }
}
}
