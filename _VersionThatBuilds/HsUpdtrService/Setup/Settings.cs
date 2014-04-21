using Nini.Config;

using hsupdater.Services;
using hsupdater.Logger;

using System;
using System.Collections;

namespace hsupdater.Setup
{
	public class Settings
	{
		public Settings( String startPath )
		{
			_localFiles = new ArrayList();
			_localFileHash = new Hashtable();
			_logger = new SysLog( this.GetType().ToString() );
			_source = new IniConfigSource( startPath + "\\hs.ini" );
		}

		private IniConfigSource _source = null;
		private int VERSION_INTERVAL = 0;
		private int HSC_INTERVAL = 0;
		private int DELETE_LOGS = 0;
		private int GROUP_ID = 0;
        private bool GROUP_UPDATED = false;
		private Properties _properties;
		private ArrayList _localFiles;
		private Hashtable _localFileHash;
		private SysLog _logger;
        

		public void Load( bool loadProperties )
		{
			try
			{
				VERSION_INTERVAL = _source.Configs["Timers"].GetInt("VersionCheck") * 1000 * 60;
				HSC_INTERVAL = _source.Configs["Timers"].GetInt("HSConnectCheck") * 1000 * 60;
				DELETE_LOGS = _source.Configs["Timers"].GetInt("DeleteLogs") * 1000 * 60;

                if (VERSION_INTERVAL <= 0)
                {
                    _logger.Log("WARNING: VERSION check timer interval " + VERSION_INTERVAL + " <= 0. Reverting to default value, 100");
                    VERSION_INTERVAL = 100 * 60000;
                }
                if (HSC_INTERVAL <= 0)
                {
                    _logger.Log("WARNING: HSC check timer interval " + HSC_INTERVAL + " <= 0. Reverting to default value, 50");
                    HSC_INTERVAL = 50 * 60000;
                }
                if (DELETE_LOGS <= 0)
                {
                    _logger.Log("WARNING: DELETE_LOGS check timer interval " + DELETE_LOGS + " <= 0. Reverting to default value, 720");
                    DELETE_LOGS = 720 * 60000;
                }

				GROUP_ID = _source.Configs["Group"].GetInt("ID");
                GROUP_UPDATED = _source.Configs["Group"].GetInt("UPDATE") == 1;
				if( loadProperties )
				{
					PropertiesService properties = new PropertiesService();

					_properties = new Properties( this );
					_properties.Load( properties.getProperties() );

					foreach( String key in _source.Configs["Files"].GetKeys() )
					{
						HsFile file = new HsFile();
						file.Name = key;
						file.Version = (float) Math.Round( _source.Configs["Files"].GetFloat( key ) , 2 );
						AddFile( file );
					}
				}
			}
			catch( Exception ex )
			{
				_logger.Error( "Error loading settings. " + ex.ToString() );
			}
		}

		public bool SaveFileSettings()
		{
			foreach( HsFile file in this._localFiles )
			{
                try  
                {  
            /*        if( !file.Update() ) return false;  
                    if( file.Status == HsFile.DELETE )  
                    {  
                        _source.Configs["Files"].Remove( file.Name );  
                    }  
                    else  
                    {  
                        _source.Configs["Files"].Set( file.Name, file.Version );  
                    }  
                }  
                catch( Exception ex )  
                {  
                    Console.WriteLine( ex.ToString() );  
                }  
                */
                    if (!GROUP_UPDATED && !file.Update()) return false;
                    if (file.Status == HsFile.DELETE)
                    {
                        _source.Configs["Files"].Remove(file.Name);
                    }
                    else
                    {
                        _source.Configs["Files"].Set(file.Name, file.Version);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            _source.Configs["Group"].Set("UPDATE", 0);
			_source.Save();

			return true;
		}

		public void AddFile( HsFile file )
		{
			this._localFiles.Add( file );
			if( !this._localFileHash.ContainsKey( file.Name ) )
			{
				this._localFileHash.Add( file.Name , _source.Configs["Files"].GetString( file.Name ) );
			}
		}

		public int VersionInterval
		{
			get{ return VERSION_INTERVAL; }
		}

		public int HsConnectInterval
		{
			get{ return HSC_INTERVAL; }
		}

		public int DeleteLogs
		{
			get{ return DELETE_LOGS; }
		}

		public int GroupId
		{
			get{ return GROUP_ID; }
		}

		public ArrayList LocalFileList
		{
			get{ return this._localFiles; }
			set{ this._localFiles = value; }
		}

		public Hashtable LocalFiles
		{
			get{ return this._localFileHash; }
			set{ this._localFileHash = value; }
		}

		public Properties Properties
		{
			get{ return this._properties; }
			set{ this._properties = value; }
		}

        public bool GroupUpdated
        {
            get { return GROUP_UPDATED; }
        }

	}
}
