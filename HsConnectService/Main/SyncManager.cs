using HsConnect.Modules;
using HsConnect.Data;
using HsConnect.Main;

using System;
using System.Net;

namespace HsConnect.Main
{
	public class SyncManager
	{
		public SyncManager()
		{
			logger = new SysLog( this.GetType() );
		}

		private SysLog logger;

		public void SyncModule( object mod )
		{
            logger.Debug("SyncManager preparing to lock");
			lock( this )
			{
                logger.Debug("SyncManager lock acquired");
				try
				{
                    logger.Debug("SyncManager, attempt to map drive?: " + Run.Mapped);
					if(Run.Mapped)
					{
                        logger.Debug("SyncManager attempting to map drive.");
						if(!PosiControl.MapDrive())
						{//for OSI
                            logger.Debug("SyncManager !PosiControl.MapDrive() ;; preparing to return now, sync module will not execute");
							return;
						}
                        logger.Debug("SyncManager PosiControl.MapDrive() successful");
					}
					logger.Log( "excecuting " + mod.GetType().ToString() );
					Main.Run.errorList.Clear(mod.GetType().ToString());
					Module module = ( Module ) mod;
					logger.Log( "excecuted " + module.Execute() );
					
				}
				catch(WebException wex)
				{
                    logger.Debug("SyncManager WebException occurred during Posi drive mapping: " + wex.ToString());
					logger.Error( wex.ToString());
				}
				catch( Exception ex )
				{
                    logger.Debug("SyncManager Exception occurred during Posi drive mapping: " + ex.ToString());
					logger.Error( ex.ToString() );
					Main.Run.errorList.Add(ex);
				}
				finally
				{
                    logger.Debug("SyncManager Finally block. Run.Mapped ? : " + Run.Mapped);
                    if (Run.Mapped)
                    {
                        logger.Debug("SyncManager calling PosiControl.CloseMap()");
                        PosiControl.CloseMap();//for OSI
                        logger.Debug("SyncManager finish PosiControl.CloseMap()");
                    }
					Main.Run.errorList.Send();
				}
			}
		}
	}
}
