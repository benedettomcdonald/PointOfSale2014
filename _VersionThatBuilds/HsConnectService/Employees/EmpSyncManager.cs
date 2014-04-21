using HsConnect.Main;
using HsSharedObjects.Client;
using HsSharedObjects.Client.Field;
using HsConnect.Services;
using HsConnect.Services.Wss;
using HsSharedObjects.Client.Preferences;

using System;
using System.Collections;

namespace HsConnect.Employees
{
	public class EmpSyncManager
	{
		public EmpSyncManager( ClientDetails details )
		{
			logger = new SysLog( this.GetType() );
			this.details = details;
		}

		private SysLog logger;
		private ClientDetails details;
		private EmployeeList hsList;
		private EmployeeList posList;

		public EmployeeList HsList
		{
			get { return this.hsList; }
			set { this.hsList = value; }
		}

		public EmployeeList PosList
		{
			get { return this.posList; }
			set { this.posList = value; }
		}
				
		/** This method is only used for transfers from POS to HS and for 
		 * true sync.  It will add POS employees to the HS list, but NOT
		 * vice versa. **/
		public void SyncPosEmployees()
		{
			EmployeeList tempList = (EmployeeList) new EmployeeListEmpty(); 
			foreach( Employee posEmp in posList )
			{
				Employee posTemp = null;
				try
				{
					posTemp = posEmp;
					Employee hsEmp = hsList.GetEmployeeByExtId( posEmp.PosId );
					if( hsEmp == null )
					{
						//this is a fairly major change.  We are no longer going to attempt to "intelligently" 
						//decide if a manually entered emp matches a "new" POS emp, as it causes too many issues with POS
						//ID swapping, etc.
					/*	Employee matchEmp = hsList.GetMatch( posTemp );
						if( matchEmp != null ) 
						{
							if( matchEmp.HsId != -1 )
							{
								hsList.UpdateExtId( matchEmp.HsId , posTemp.PosId );
								hsList.Updated = true;
							}
						} 
						else
						{*/
							Employee hsTemp = new Employee();
							hsTemp.Status = new EmployeeStatus();
							hsTemp.Status.StatusCode = posTemp.Status.StatusCode;
							hsTemp.UpdateStatus = Employee.HS_CURRENT;
							Sync( ref posTemp , ref hsTemp );
							hsTemp.Inserted = true;
							hsTemp.Updated = false;
                            bool addHrId = posList.Details.Preferences.PrefExists(Preference.MICROS_SHARED_EMP_HR_ID);
                            bool truncHrId = posList.Details.Preferences.PrefExists(Preference.MICROS_TRUNC_HR_ID);
                            bool hasValidHrId = true;
                            if (addHrId && truncHrId)
                            {
                                hasValidHrId = posEmp.AltNumber > -1;
                                logger.Debug("in SyncPosEmployees.  HR ID:  " + posEmp.AltNumber + " , hasValidHrId?  " + hasValidHrId);
                            }
							if( hsTemp.FirstName.Length > 0 && hsTemp.LastName.Length > 0 && hasValidHrId) hsList.Add( hsTemp );
						//}
					} else Sync( ref posTemp , ref hsEmp );
					if( hsEmp != null && ( hsEmp.Updated || hsEmp.Inserted ) )
					{
						hsList.Updated = true;
					}
				}
				catch( Exception ex)
				{
					logger.Error( "Error in SyncPosEmployees(): " + ex.ToString() );
				}
				tempList.Add( posTemp );
			}
			posList = tempList;
		}
		

		/** This method is only used for transfers from HS to POS and for 
		 * true sync.  It will add HS employees to the POS list, but NOT
		 * vice versa. **/
		public void SyncHsEmployees()
		{
			EmployeeList tempList = (EmployeeList) new EmployeeListEmpty(); 
			foreach( Employee hsEmp in hsList )
			{
				Employee hsTemp = null;
				try
				{
					hsTemp = hsEmp;
					Employee posEmp = posList.GetEmployeeByExtId( hsEmp.PosId );
					if( posEmp == null ) 
					{
						Employee match = posList.GetMatch( hsEmp );
						if( match == null && !posList.ContainsHsEmp( hsEmp ) )
						{
							posEmp = new Employee();
							posEmp.Status = new EmployeeStatus();
							posEmp.Status.StatusCode = hsTemp.Status.StatusCode;
							posEmp.Inserted = true;
							posEmp.Updated = false;
							posEmp.UpdateStatus = Employee.HS_UPDATED;
							Sync( ref posEmp , ref hsTemp );
							if( posEmp.FirstName.Length > 0 && posEmp.LastName.Length > 0 ) posList.Add( posEmp );
						}
					}
					if( posEmp != null ) Sync( ref posEmp , ref hsTemp );
					if( posEmp != null && ( posEmp.Updated || posEmp.Inserted ) )
					{
						posList.Updated = true;
					}
				}
				catch( Exception ex)
				{
					logger.Error( "Error in SyncHsEmployees(): " + ex.ToString() );
				}
				tempList.Add( hsTemp );
			}
			hsList = tempList;
		}
		
		
		/** This is a long method!  It takes the two employees, and syncronizes each field
		 * based on: transfer rule and fields allowed, as well the update staus from HS (if
		 * the employee has been recently updated in HS) **/
		private void Sync( ref Employee posEmp , ref Employee hsEmp )
		{	
			posEmp.HsId = hsEmp.HsId;
			hsEmp.PosId = posEmp.PosId;
			
			# region First Name
			if( details.FieldList.Allows( ClientField.FIRST_NAME ) &&
				!posEmp.FirstName.Equals( hsEmp.FirstName ) )
			{
				if( details.TransferRule == ClientDetails.POS_TO_HS )
				{
					hsEmp.FirstName = posEmp.FirstName;
					hsEmp.Updated = true;
				}
				if( details.TransferRule == ClientDetails.HS_TO_POS )
				{
					posEmp.FirstName = hsEmp.FirstName;
					posEmp.Updated = true;
				}
				if( details.TransferRule == ClientDetails.SYNC )
				{
					if( hsEmp.UpdateStatus == Employee.HS_UPDATED )
					{
						posEmp.FirstName = hsEmp.FirstName;
						posEmp.Updated = true;
					} 
					else
					{
						hsEmp.FirstName = posEmp.FirstName;
						hsEmp.Updated = true;
					}
				}				
			}
			#endregion

			# region Last Name
			if( details.FieldList.Allows( ClientField.LAST_NAME ) &&
				!posEmp.LastName.Equals( hsEmp.LastName ) )
			{
				if( details.TransferRule == ClientDetails.POS_TO_HS )
				{
					hsEmp.LastName = posEmp.LastName;
					hsEmp.Updated = true;
				}
				if( details.TransferRule == ClientDetails.HS_TO_POS )
				{
					posEmp.LastName = hsEmp.LastName;
					posEmp.Updated = true;
				}
				if( details.TransferRule == ClientDetails.SYNC )
				{
					if( hsEmp.UpdateStatus == Employee.HS_UPDATED )
					{
						posEmp.LastName = hsEmp.LastName;
						posEmp.Updated = true;
					} else
					{
						hsEmp.LastName = posEmp.LastName;
						hsEmp.Updated = true;
					}
				}				
			}
			#endregion

			# region Nick Name
			if( details.FieldList.Allows( ClientField.NICK_NAME ) &&
				!posEmp.NickName.Equals( hsEmp.NickName ) )
			{
				if( details.TransferRule == ClientDetails.POS_TO_HS )
				{
					hsEmp.NickName = posEmp.NickName;
					hsEmp.Updated = true;
				}
				if( details.TransferRule == ClientDetails.HS_TO_POS )
				{
					posEmp.NickName = hsEmp.NickName;
					posEmp.Updated = true;
				}
				if( details.TransferRule == ClientDetails.SYNC )
				{
					if( hsEmp.UpdateStatus == Employee.HS_UPDATED )
					{
						posEmp.NickName = hsEmp.NickName;
						posEmp.Updated = true;
					} 
					else
					{
						hsEmp.NickName = posEmp.NickName;
						hsEmp.Updated = true;
					}
				}				
			}
			#endregion

			# region Address 1
			if( details.FieldList.Allows( ClientField.ADDRESS_1 ) &&
				!posEmp.Address1.Equals( hsEmp.Address1 ) )
				
			{
				if( details.TransferRule == ClientDetails.POS_TO_HS )
				{
					hsEmp.Address1 = posEmp.Address1;
					hsEmp.Updated = true;
				}
				if( details.TransferRule == ClientDetails.HS_TO_POS )
				{
					posEmp.Address1 = hsEmp.Address1;
					posEmp.Updated = true;
				}
				if( details.TransferRule == ClientDetails.SYNC )
				{
					if( hsEmp.UpdateStatus == Employee.HS_UPDATED )
					{
						posEmp.Address1 = hsEmp.Address1;
						posEmp.Updated = true;
					} 
					else 
					{
						hsEmp.Address1 = posEmp.Address1;
						hsEmp.Updated = true;
					}					
				}				
			}
			#endregion

			# region Address 2
			if( details.FieldList.Allows( ClientField.ADDRESS_2 ) &&
				!posEmp.Address2.Equals( hsEmp.Address2 ) )
			{
				if( details.TransferRule == ClientDetails.POS_TO_HS )
				{
					hsEmp.Address2 = posEmp.Address2;
					hsEmp.Updated = true;
				}
				if( details.TransferRule == ClientDetails.HS_TO_POS )
				{
					posEmp.Address2 = hsEmp.Address2;
					posEmp.Updated = true;
				}
				if( details.TransferRule == ClientDetails.SYNC )
				{
					if( hsEmp.UpdateStatus == Employee.HS_UPDATED )
					{
						posEmp.Address2 = hsEmp.Address2;
						posEmp.Updated = true;
					} 
					else
					{
						hsEmp.Address2 = posEmp.Address2;
						hsEmp.Updated = true;
					}
				}				
			}
			#endregion

			# region City
			if( details.FieldList.Allows( ClientField.CITY ) &&
				!posEmp.City.Equals( hsEmp.City ) )
			{
				if( details.TransferRule == ClientDetails.POS_TO_HS )
				{
					hsEmp.City = posEmp.City;
					hsEmp.Updated = true;
				}
				if( details.TransferRule == ClientDetails.HS_TO_POS )
				{
					posEmp.City = hsEmp.City;
					posEmp.Updated = true;
				}
				if( details.TransferRule == ClientDetails.SYNC )
				{
					if( hsEmp.UpdateStatus == Employee.HS_UPDATED )
					{
						posEmp.City = hsEmp.City;
						posEmp.Updated = true;
					} 
					else
					{
						hsEmp.City = posEmp.City;
						hsEmp.Updated = true;
					}
				}				
			}
			#endregion

			# region State
			if( details.FieldList.Allows( ClientField.STATE ) &&
				!posEmp.State.Equals( hsEmp.State ) && !details.PosName.Equals( "Micros" ) )
			{
				if( details.TransferRule == ClientDetails.POS_TO_HS )
				{
					hsEmp.State = posEmp.State;
					hsEmp.Updated = true;
				}
				if( details.TransferRule == ClientDetails.HS_TO_POS && !details.PosName.Equals( "Micros" ) )
				{
					posEmp.State = hsEmp.State;
					posEmp.Updated = true;
				}
				if( details.TransferRule == ClientDetails.SYNC && !details.PosName.Equals( "Micros" ) )
				{
					if( hsEmp.UpdateStatus == Employee.HS_UPDATED )
					{
						posEmp.State = hsEmp.State;
						posEmp.Updated = true;
					} 
					else
					{
						hsEmp.State = posEmp.State;
						hsEmp.Updated = true;
					}
				}				
			}
			#endregion

			# region Zip Code
			if( details.FieldList.Allows( ClientField.ZIP ) &&
				!posEmp.ZipCode.Equals( hsEmp.ZipCode ) )
			{
				if( details.TransferRule == ClientDetails.POS_TO_HS )
				{
					hsEmp.ZipCode = posEmp.ZipCode;
					hsEmp.Updated = true;
				}
				if( details.TransferRule == ClientDetails.HS_TO_POS )
				{
					posEmp.ZipCode = hsEmp.ZipCode;
					posEmp.Updated = true;
				}
				if( details.TransferRule == ClientDetails.SYNC )
				{
					if( hsEmp.UpdateStatus == Employee.HS_UPDATED )
					{
						posEmp.ZipCode = hsEmp.ZipCode;
						posEmp.Updated = true;
					} 
					else
					{
						hsEmp.ZipCode = posEmp.ZipCode;
						hsEmp.Updated = true;
					}
				}				
			}
			#endregion

			# region Phone
			if( details.FieldList.Allows( ClientField.PHONE ) &&
				!posEmp.Phone.ToString().Equals( hsEmp.Phone.ToString() ) )
			{
				if( details.TransferRule == ClientDetails.POS_TO_HS )
				{
					hsEmp.Phone = posEmp.Phone;
					hsEmp.Updated = true;
				}
				if( details.TransferRule == ClientDetails.HS_TO_POS )
				{
					posEmp.Phone = hsEmp.Phone;
					posEmp.Updated = true;
				}
				if( details.TransferRule == ClientDetails.SYNC )
				{
					if( hsEmp.UpdateStatus == Employee.HS_UPDATED )
					{
						posEmp.Phone = hsEmp.Phone;
						posEmp.Updated = true;
					} 
					else
					{
						hsEmp.Phone = posEmp.Phone;
						hsEmp.Updated = true;
					}
				}				
			}
			#endregion

			# region Sms
			if( details.FieldList.Allows( ClientField.SMS ) &&
				!posEmp.Mobile.Equals( hsEmp.Mobile ) )
			{
				if( details.TransferRule == ClientDetails.POS_TO_HS )
				{
					hsEmp.Mobile = posEmp.Mobile;
					hsEmp.Updated = true;
				}
				if( details.TransferRule == ClientDetails.HS_TO_POS )
				{
					posEmp.Mobile = hsEmp.Mobile;
					posEmp.Updated = true;
				}
				if( details.TransferRule == ClientDetails.SYNC )
				{
					if( hsEmp.UpdateStatus == Employee.HS_UPDATED )
					{
						posEmp.Mobile = hsEmp.Mobile;
						posEmp.Updated = true;
					} 
					else
					{
						hsEmp.Mobile = posEmp.Mobile;
						hsEmp.Updated = true;
					}
				}				
			}
			#endregion

			# region Email
			if( details.FieldList.Allows( ClientField.EMAIL ) &&
				!posEmp.Email.Equals( hsEmp.Email ) )
			{
				if( details.TransferRule == ClientDetails.POS_TO_HS )
				{
					hsEmp.Email = posEmp.Email;
					hsEmp.Updated = true;
				}
				if( details.TransferRule == ClientDetails.HS_TO_POS )
				{
					posEmp.Email = hsEmp.Email;
					posEmp.Updated = true;
				}
				if( details.TransferRule == ClientDetails.SYNC )
				{
					if( hsEmp.UpdateStatus == Employee.HS_UPDATED )
					{
						posEmp.Email = hsEmp.Email;
						posEmp.Updated = true;
					} 
					else
					{
						hsEmp.Email = posEmp.Email;
						hsEmp.Updated = true;
					}
				}				
			}
			#endregion

			# region BirthDate
			if( details.FieldList.Allows( ClientField.BIRTH_DATE ) &&
				posEmp.BirthDate != hsEmp.BirthDate )
			{
				if( details.TransferRule == ClientDetails.POS_TO_HS )
				{
					hsEmp.BirthDate = posEmp.BirthDate;
					hsEmp.Updated = true;
				}
				if( details.TransferRule == ClientDetails.HS_TO_POS )
				{
					posEmp.BirthDate = hsEmp.BirthDate;
					posEmp.Updated = true;
				}
				if( details.TransferRule == ClientDetails.SYNC )
				{
					if( hsEmp.UpdateStatus == Employee.HS_UPDATED )
					{
						posEmp.BirthDate = hsEmp.BirthDate;
						posEmp.Updated = true;
					} 
					else
					{
						hsEmp.BirthDate = posEmp.BirthDate;
						hsEmp.Updated = true;
					}
				}				
			}
			#endregion

			# region Status
			if( details.FieldList.Allows( ClientField.STATUS ) &&
				posEmp.Status.StatusCode != hsEmp.Status.StatusCode )
			{
				if( details.TransferRule == ClientDetails.POS_TO_HS )
				{
					hsEmp.Status = posEmp.Status;
					hsEmp.Status.Updated = true;
					hsEmp.Updated = true;
				}
				if( details.TransferRule == ClientDetails.HS_TO_POS )
				{
					posEmp.Status = hsEmp.Status;
					posEmp.Status.Updated = true;
					posEmp.Updated = true;
				}
				if( details.TransferRule == ClientDetails.SYNC )
				{
					if( hsEmp.UpdateStatus == Employee.HS_UPDATED )
					{
						posEmp.Status = hsEmp.Status;
						posEmp.Status.Updated = true;
						posEmp.Updated = true;
					} 
					else
					{
						hsEmp.Status = posEmp.Status;
						hsEmp.Status.Updated = true;
						hsEmp.Updated = true;
					}
				}
			}
			#endregion

			# region Alt Number
			if( details.FieldList.Allows( ClientField.ALT_NUMBER ) &&
				posEmp.AltNumber != hsEmp.AltNumber )
			{
				if( details.TransferRule == ClientDetails.POS_TO_HS )
				{
					Console.WriteLine( "HS Alt Number: " + hsEmp.AltNumber );
					Console.WriteLine( "POS Alt Number: " + posEmp.AltNumber );
					Console.WriteLine( "" );
					hsEmp.AltNumber = posEmp.AltNumber;
					hsEmp.Updated = true;
				}				
			}
			#endregion

		}
		

		public void UpdatePosEmployees()
		{
			int added = 0;
			int updated = 0;
			EmployeeManager mgr1 = new EmployeeManager( this.details );			
			EmployeeList updateList = mgr1.GetPosEmployeeList();
			foreach( Employee emp in posList )
			{
				if( emp.Updated ) 
				{
					updated++;
					emp.UpdateStatus = Employee.HS_CURRENT;
					updateList.Add( emp );
				}
			}
			if( updateList.Count > 0 ) 
			{
				/** Update the Pos Employee information
				 * */
				updateList.DbUpdate();
			
				/** Update HotSchedules Update Status to 2, designating that the user
				 * has been updated in the POS and everything is current
				 * */
				ClientEmployeesWss empService = new ClientEmployeesWss();
				int rows = empService.syncUserUpdateStatus( details.ClientId , updateList.GetShortXmlString() );
			}

			EmployeeManager mgr2 = new EmployeeManager( this.details );
			EmployeeList insertList = mgr2.GetPosEmployeeList();
			foreach( Employee emp in posList )
			{
				if( emp.Inserted ) 
				{
					added++;
					insertList.Add( emp );
				}
			}
			if( insertList.Count > 0 ) insertList.DbInsert();
		}


		public void UpdateHsEmployees()
		{
			int updated = 0;
			EmployeeList updateList = new HsEmployeeList( details.ClientId );
			foreach( Employee emp in hsList )
			{
				if( emp.Updated ) 
				{
					updateList.Add( emp );
					updated++;
				}
			}
			if( updateList.Count > 0 ) updateList.DbUpdate();
			
			int added = 0;
			EmployeeList insertList = new HsEmployeeList( details.ClientId );
			foreach( Employee emp in hsList )
			{
                bool addHrId = details.Preferences.PrefExists(Preference.MICROS_SHARED_EMP_HR_ID);
                bool truncHrId = details.Preferences.PrefExists(Preference.MICROS_TRUNC_HR_ID);
                bool hasValidHrId = true;
                if (addHrId && truncHrId)
                {
                    hasValidHrId = emp.AltNumber > -1;
                    logger.Debug("in UpdateHSEmployees.  HR ID:  " + emp.AltNumber + " , hasValidHrId?  " + hasValidHrId);
                }
				if( emp.Inserted && emp.Status.StatusCode != EmployeeStatus.TERMINATED && hasValidHrId) 
				{
					insertList.Add( emp );
					added++;
				}
			}
			if( insertList.Count > 0 ) insertList.DbInsert();
			
		}
	}
}
