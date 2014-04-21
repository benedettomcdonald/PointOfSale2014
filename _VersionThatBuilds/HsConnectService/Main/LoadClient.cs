//using HsSharedObjects.Client;
//using HsSharedObjects.Client.Module;
//using HsSharedObjects.Client.Field;
//using HsSharedObjects.Client.Shift;
//using HsSharedObjects.Client.Preferences;
//using HsSharedObjects.Client.CustomModule;
using HsConnect.Services;
using HsConnect.Services.Wss;
using HsConnect.Xml;
using HsConnect.Data;

using HsSharedObjects;
using HsSharedObjects.Client;
using HsSharedObjects.Client.Module;
using HsSharedObjects.Client.Field;
using HsSharedObjects.Client.Shift;
using HsSharedObjects.Client.Preferences;
using HsSharedObjects.Client.CustomModule;

using HsExecuteModule;
using HsFileTransfer;

using Nini.Config;

using System;
using System.Collections;
using System.Threading;
using System.Xml;
using System.Windows.Forms;

namespace HsConnect.Main
{
	/// <summary>
	/// Summary description for LoadClient.
	/// </summary>
	public class LoadClient
	{
		public LoadClient( int id )
		{
			logger = new SysLog( this.GetType() );
			this.clientId = id;
		}

		private int clientId;
		private SysLog logger;

		public ClientDetails GetClientDetails()
		{
			logger.Debug("in getClientDetails" );
			//HsCnxData.Details = null;
			lock( this )
			{
				logger.Debug("in getClientDetails:lock" );
				ClientSettingsWss settingsService = new ClientSettingsWss();
				logger.Debug( "getting client details" );
                String startup = Application.StartupPath;
                startup = startup.Replace(@"\files", "");
                String detailsAsXml = "";
                try
                {
                    IniConfigSource _source = new IniConfigSource(startup + "\\hs.ini");
                    String groupId = _source.Configs["Group"].GetString("ID");
                    detailsAsXml = settingsService.getClientDetails(clientId, Application.ProductVersion + "," + groupId);
                }
                catch (Exception e)
                {
                    logger.Error("ERROR: problem parsing hs.ini, file may be corrupt. Skipping adding groupId to clientDetails request", e);
                    detailsAsXml = settingsService.getClientDetails(clientId, Application.ProductVersion);
                }
             
             //   String detailsAsXml = settingsService.getClientDetails(clientId, Application.ProductVersion); 
				//logger.Debug( "got client details for(" + clientId +", "+ Application.ProductVersion+" )" );
				logger.Debug( "Details:"  +detailsAsXml );
				ClientDetails cdTmp = ParseXml( detailsAsXml );
				if(cdTmp.PosName.Equals("Posi"))
				{
					if(cdTmp.Dsn.Length > 0 && !Main.Run.Mapped)
						PosiControl.Drive = cdTmp.Dsn;
						
				}
				HsCnxData.Details = cdTmp;
			}
            return HsCnxData.Details; 
		}

		private ClientDetails ParseXml( String xmlString )
		{
			HsXmlReader reader = new HsXmlReader();
			reader.LoadXml( xmlString );
			ClientDetails details = new ClientDetails();
			details.ClientId = clientId;
			XmlNode node = reader.SelectSingleNode( "hsconnect-client-details/client-settings" );
			details.TransferRule = Convert.ToInt32( node.Attributes["transfer-rule"].InnerText );
			details.Status = Convert.ToInt32( node.Attributes["status"].InnerText );
			details.UseWizard = Convert.ToInt32( node.Attributes["use-wizard"].InnerText );
			details.PosName = node.SelectSingleNode( "pos-name" ).InnerText;
			details.Dsn = node.SelectSingleNode( "dsn" ).InnerText;
			details.DbUser = node.SelectSingleNode( "db-user" ).InnerText;
			details.DbPassword = node.SelectSingleNode( "db-password" ).InnerText;
			details.FieldList = LoadClientFields( xmlString );
			//rearanged the order of these a little
			details.Preferences = LoadPreferences( xmlString );
			details.ShiftList = LoadClientShifts ( xmlString );//now grabs client shifts
			details.ModuleList = LoadClientModules( xmlString, details );
			details.CustomModuleList = LoadClientCustomModules( xmlString );
			details.OvertimeRule = Convert.ToInt32( reader.SelectSingleNode( "hsconnect-client-details/overtime-rule" ).InnerText );
			//get Commands and Files
			details.Files = LoadFiles( xmlString );
			details.Commands = LoadCommands( xmlString );
			
			if( reader.SelectSingleNode( "hsconnect-client-details/work-week-start" ) != null )
			{
				details.WorkWeekStart = Convert.ToInt32( reader.SelectSingleNode( "hsconnect-client-details/work-week-start" ).InnerText );
			}

            UpdateHsIniGroup(reader.SelectSingleNode( "hsconnect-client-details/group" ).Attributes);

            //get Timers settings and apply to hs.ini
            if (reader.SelectNodes("hsconnect-client-details/client-timers/hsc-timer") != null)
 	 		{
                UpdateHsIniTimers(reader.SelectNodes("hsconnect-client-details/client-timers/hsc-timer"));
 	 		}

            if (reader.SelectSingleNode("hsconnect-client-details/store-ext-info") != null)
            {
                XmlNode infNode = reader.SelectSingleNode("hsconnect-client-details/store-ext-info");
                details.ClientExtRef = Convert.ToInt32(infNode.Attributes["client-ext-ref"].InnerText);
                details.ClientConceptExtRef = Convert.ToInt32(infNode.Attributes["client-concept-ext-ref"].InnerText);
                details.ClientCompanyExtRef = Convert.ToInt32(infNode.Attributes["client-company-ext-ref"].InnerText);
            }

			return details;
			}

	        private void UpdateHsIniTimers(XmlNodeList timers)
	 		{
				//retrieve new values
                int timerVersionCheck = 100;
                int timerHSCCheck = 50;
                int timerDeleteLogs = 720;

                foreach (XmlNode node in timers)
                {
                    switch (node.Attributes["name"].InnerText)
                    {
                        case "version-check":
                            timerVersionCheck = Convert.ToInt32(node.Attributes["value"].InnerText);
                            break;
                        case "hs-connect-check":
                            timerHSCCheck = Convert.ToInt32(node.Attributes["value"].InnerText);
                            break;
                        case "delete-logs":
                            timerDeleteLogs = Convert.ToInt32(node.Attributes["value"].InnerText);
                            break;
                        default:
                            logger.Log("ERROR: invalid attribute name when parsing timers");
                            break;
                    }
                }
 	 	
		 	 	//update HS.ini
		 	 	String startup = Application.StartupPath;
		 	 	startup = startup.Replace(@"\files", "");
                try
                {
                    IniConfigSource src = new IniConfigSource(startup + "\\hs.ini");
                    src.Configs["Timers"].Set("VersionCheck", timerVersionCheck);
                    src.Configs["Timers"].Set("HSConnectCheck", timerHSCCheck);
                    src.Configs["Timers"].Set("DeleteLogs", timerDeleteLogs);
                    src.Save();
                }
                catch (Exception e)
                {
                    logger.Error("ERROR: problem updating hs.ini with new timers settings. hs.ini will not be updated.", e);
                }
	 		}

        private void UpdateHsIniGroup(XmlAttributeCollection attributes)
        {
            int update = Int32.Parse(attributes["update"].InnerText.Trim());
            if (update == 1)
            {
                //do update of HS.ini
                String startup = Application.StartupPath;
                startup = startup.Replace(@"\files", "");
                try
                {
                    IniConfigSource source = new IniConfigSource(startup + "\\hs.ini");
                    source.Configs["Group"].Set("ID", attributes["id"].InnerText.Trim());
                    source.Configs["Group"].Set("UPDATE", 1);
                    source.Save();
                    //send back web service call to flag as updated
                    ClientSettingsWss settingsService = new ClientSettingsWss();
                    settingsService.markIniAsUpdated(clientId);
                }
                catch (Exception e)
                {
                    logger.Error("ERROR: problem updating groupId in hs.ini, hs.ini may be corrupted. Group value will not be updated.", e);
                }

            }
        }

		private ClientFieldList LoadClientFields( String xml )
		{
			ClientFieldList fields = new ClientFieldList();
			HsXmlReader reader = new HsXmlReader();
			reader.LoadXml( xml );
			XmlNodeList nodeList = reader.SelectNodes( "hsconnect-client-details/hsconnect-client-field-list/field" );
			foreach( XmlNode node in nodeList )
			{
				ClientField field = new ClientField();
				field.Id = Convert.ToInt32( node.Attributes["id"].InnerText );
				fields.Add( field );
			}
			return fields;
		}

		/**This method retrieves the client shifts from the xml and populates
		 * the ClientShiftList. 
		 */ 
		private ClientShiftList LoadClientShifts( String xml )
		{
			ClientShiftList shifts = new ClientShiftList();
			HsXmlReader reader = new HsXmlReader();
			reader.LoadXml( xml );
			XmlNodeList nodeList = reader.SelectNodes( "hsconnect-client-details/client-shifts/client-shift" );
			foreach( XmlNode node in nodeList )
			{
				ClientShift shift = new ClientShift();
				shift.Id = Convert.ToInt32( node.Attributes["id"].InnerText);
				shift.Label = node.ChildNodes[0].InnerText;
				int startHour = Convert.ToInt32( node.ChildNodes[1].Attributes["hour"].InnerText );
				int startMin = Convert.ToInt32( node.ChildNodes[1].Attributes["minute"].InnerText );
				shift.StartTime = new TimeSpan( startHour, startMin, 0 );
				int endHour = Convert.ToInt32( node.ChildNodes[2].Attributes["hour"].InnerText );
				int endMin = Convert.ToInt32( node.ChildNodes[2].Attributes["minute"].InnerText );
				shift.EndTime = new TimeSpan( endHour, endMin, 0 );

				shifts.Add( shift );
			}
			return shifts;
		}
		
		private PreferenceList LoadPreferences( String xml )
		{
			PreferenceList prefs = new PreferenceList();
			HsXmlReader reader = new HsXmlReader();
			reader.LoadXml( xml );
			XmlNodeList nodeList = reader.SelectNodes( "hsconnect-client-details/client-preference-list/client-preference" );
			foreach( XmlNode node in nodeList )
			{
				Preference pref = new Preference();
				pref.Id = Convert.ToInt32( node.Attributes["pref-id"].InnerText );
				pref.Val1 = node.SelectSingleNode("value-1").InnerText;
				pref.Val2 = node.SelectSingleNode("value-2").InnerXml;
				pref.Val3 = node.SelectSingleNode("value-3").InnerText;
				pref.Val4 = node.SelectSingleNode("value-4").InnerText;
				pref.Val5 = node.SelectSingleNode("value-5").InnerText;
				prefs.Add( pref );
			}
			return prefs;
		}

		//MFisher 6/26/07:  added a parameter the this method, because i needed access
		//to the details, needed to check preferences for UPDATE_TIMERS
		private ClientModuleList LoadClientModules( String xml, ClientDetails details )
		{
			ClientModuleList modules = new ClientModuleList();
			HsXmlReader reader = new HsXmlReader();
			reader.LoadXml( xml );
			XmlNodeList nodeList = reader.SelectNodes( "hsconnect-client-details/hsconnect-client-module-list/module" );
			foreach( XmlNode node in nodeList )
			{
				ClientModule mod = new ClientModule();
				mod.ModuleId = Convert.ToInt32( node.Attributes["module-id"].InnerText );
				mod.SyncType = Convert.ToInt32( node.SelectSingleNode( "sync-type" ).InnerText );
				mod.SyncInterval = Convert.ToInt32( node.SelectSingleNode( "sync-interval" ).InnerText );
				mod.SyncHour = Convert.ToInt32( node.SelectSingleNode( "sync-hour" ).InnerText );
				mod.SyncMinute = Convert.ToInt32( node.SelectSingleNode( "sync-minute" ).InnerText );
				if( node.SelectSingleNode( "force" ) != null ) mod.Force = Convert.ToInt32( node.SelectSingleNode( "force" ).InnerText );
				if(mod.ModuleId == ClientModule.NET_SALES || mod.ModuleId == ClientModule.LABOR_ITEMS )
				{
					if( details.Preferences.PrefExists( Preference.UPDATE_TIMERS ) )
					{
						logger.Log( "The preference exists, need to update sync time" );
						TimeSpan nextSync = details.ShiftList.CurrentShift().EndTime;
						logger.Log( "Next Sync for " + mod.ModuleId + " Will Occur At:  " + nextSync.ToString() );
						mod.SyncType = ClientModule.FIXED_SYNC;
						mod.SyncHour = nextSync.Hours;
						mod.SyncMinute = nextSync.Minutes;
						logger.Log( mod.ModuleId + " sync time is updated" );
					}
				}

				modules.Add( mod );
			}
			return modules;
		}

		private ClientCustomModuleList LoadClientCustomModules( String xml )
		{
			ClientCustomModuleList modules = new ClientCustomModuleList();
			HsXmlReader reader = new HsXmlReader();
			reader.LoadXml( xml );
			XmlNodeList nodeList = reader.SelectNodes( "hsconnect-client-details/hsconnect-client-custom-module-list/custom-module" );
			foreach( XmlNode node in nodeList )
			{
				ClientCustomModule mod = new ClientCustomModule();
				mod.ModuleId = Convert.ToInt32( node.Attributes["module-id"].InnerText );
				mod.SyncType = Convert.ToInt32( node.SelectSingleNode( "sync-type" ).InnerText );
				mod.SyncInterval = Convert.ToInt32( node.SelectSingleNode( "sync-interval" ).InnerText );
				mod.SyncHour = Convert.ToInt32( node.SelectSingleNode( "sync-hour" ).InnerText );
				mod.SyncMinute = Convert.ToInt32( node.SelectSingleNode( "sync-minute" ).InnerText );
				modules.Add( mod );
			}
			return modules;
		}

		private ArrayList LoadFiles(String xml)
		{
			ArrayList list = new ArrayList();
			HsXmlReader reader = new HsXmlReader();
			reader.LoadXml( xml );
			XmlNodeList nodeList = reader.SelectNodes( "hsconnect-client-details/hsconnect-files/hsconnect-file-group" );
			foreach( XmlNode node in nodeList )
			{
				//String path = node.InnerText;
				int proc = Int32.Parse(node.Attributes["proc-ref"].InnerText);
				FileForTransfer fft = new FileForTransfer();
				//fft.FilePath = path;
				fft.ProcRef = proc;
				fft.FilePaths = new ArrayList();
				foreach(XmlNode fileNode in node.ChildNodes)
				{
					fft.FilePaths.Add(fileNode.InnerText);
				}
				list.Add(fft);

			}
		/*	if(list.Count == 0)
			{
				String f = @"S:\ALTDBF\PAYRPUNC.DBF";
				int pr = 0;
				FileForTransfer ff = new FileForTransfer();
				ff.FilePath = f;
				ff.ProcRef = pr;
				list.Add(ff);
			}*/
			return list;
		}

		private ArrayList LoadCommands( String xml )
		{
			ArrayList list = new ArrayList();
			HsXmlReader reader = new HsXmlReader();
			reader.LoadXml( xml );
			XmlNodeList nodeList = reader.SelectNodes( "hsconnect-client-details/hsconnect-commands/hsconnect-command" );
			foreach( XmlNode node in nodeList )
			{
				String cmdString = node.SelectSingleNode("file-name").InnerText;
				String argsString = node.SelectSingleNode("args").InnerText;
				String dirString = node.SelectSingleNode("file-path").InnerText;
				Command cmd = new Command(dirString, cmdString, argsString);
				list.Add(cmd);
			}
			/*if(list.Count == 0)
			{
				Command c = new Command(@"C:\SC", @"TARW.EXE", @"-R 93 3 0 /ALT");
				list.Add(c);
			}*/
			return list;
		}

	}
}
