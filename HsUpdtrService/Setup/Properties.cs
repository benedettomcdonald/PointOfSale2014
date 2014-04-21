using hsupdater.Setup;

using System;
using System.Xml;
using System.Collections;

namespace hsupdater.Setup
{
	public class Properties
	{
		public Properties( Settings settings )
		{
			_remoteFiles = new ArrayList();
			_remoteFileHash = new Hashtable();
			_settings = settings;
		}

		private ArrayList _remoteFiles;
		private Settings _settings;
		private Hashtable _remoteFileHash;

		public void Load( String _xmlString )
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml( _xmlString );
			foreach( XmlNode fileNode in doc.SelectNodes( "/properties/group[ @id = " + _settings.GroupId + " ]/file" ) )
			{
				HsFile file = new HsFile();
				file.Name = fileNode.SelectSingleNode( "name" ).InnerText;
				file.Version = (float) Convert.ToDouble( fileNode.SelectSingleNode( "version" ).InnerText );
				file.Path = ( fileNode.SelectSingleNode( "name" ).Attributes["path"].InnerText );
				_remoteFiles.Add( file );
				if( !_remoteFileHash.ContainsKey( file.Name ) )
				{
					_remoteFileHash.Add( file.Name , file );
				}
			}
		}

		public ArrayList RemoteFileList
		{
			get{ return this._remoteFiles; }
			set{ this._remoteFiles = value; }
		}

		public Hashtable RemoteFiles
		{
			get{ return this._remoteFileHash; }
			set{ this._remoteFileHash = value; }
		}
	}
}
