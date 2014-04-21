using HsSharedObjects.Client;
using HsSharedObjects.Client.CustomModule;
using HsConnect.Main;
using HsConnect.Services.Wss;
using HsConnect.Shifts;
using HsConnect.TimeCards;
using HsConnect.TimeCards.TimeCardImport;
using HsConnect.Employees;

using System;
using System.Xml;
namespace HsConnect.Modules
{
    /// <summary>
    /// Summary description for PosiTimeCardImportModule.
    /// </summary>
    public class PosiTimeCardImportModule : ModuleImpl
    {
        public PosiTimeCardImportModule()
        {
            this.logger = new SysLog(this.GetType());
        }

        private SysLog logger;

        public override bool Execute()
        {
            if (Details.CustomModuleList.IsActive(ClientCustomModule.BJS_TIMECARD_IMPORT))
            {
                if (Details.PosName.Equals("Posi"))
                {
                    PosiTimeCardImport timeCardImport = new PosiTimeCardImport();
                    string empPunchXml = PunchRecordUtil.GetPosEmpPunches(this.Details, this.AutoSync);

                    PunchRecordService prService = new PunchRecordService();
                    String punchXml = prService.getAdjEmpPunches(this.Details.ClientId, empPunchXml);
                    /*	<hsconnect-adjust>
                     *		<punches>
                     *			<punch>
                     *			...
                     *			</punch>
                     *		</punches>
                     *		<employees>
                     *			<employee>
                     *			...
                     *			</employee>
                     *		</employees>
                     *	</hsconnect-adjust>
                     * /
                     * 
                    /*HsEmployeeList hsEmpList = new HsEmployeeList(this.Details.ClientId);
                    hsEmpList.DbLoad(false, false, empPunchXml);
                    */
                    //	HsTimeCardList timeCardList = new HsTimeCardList();
                    //		timeCardList.Details = this.Details;
                    //		timeCardList.LoadFromXml(punchXml);

                    //		timeCardImport.EmpList = PunchRecordUtil.GetEmpList(this.Details, punchXml);
                    //		timeCardImport.TCList = timeCardList;
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(punchXml);
                    timeCardImport.PunchXml = doc;
                    timeCardImport.Execute();
                    prService.verifyAdjEmpPunches(this.Details.ClientId, punchXml);
                }
                else if (Details.PosName.Equals("Aloha"))
                {
                    logger.Debug("Running aloha time card import");
                    AlohaTimeCardImport timeCardImport = new AlohaTimeCardImport();
                    timeCardImport.Details = this.Details;
                    string empPunchXml = PunchRecordUtil.GetPosEmpPunches(this.Details, this.AutoSync);

                    PunchRecordService prService = new PunchRecordService();
                    String punchXml = prService.getAdjEmpPunches(this.Details.ClientId, empPunchXml);
                    logger.Debug(punchXml);
                    /*	<hsconnect-adjust>
                     *		<punches>
                     *			<punch>
                     *			...
                     *			</punch>
                     *		</punches>
                     *		<employees>
                     *			<employee>
                     *			...
                     *			</employee>
                     *		</employees>
                     *	</hsconnect-adjust>
                     * /
                     * 
                    /*HsEmployeeList hsEmpList = new HsEmployeeList(this.Details.ClientId);
                    hsEmpList.DbLoad(false, false, empPunchXml);
                    */
                    //	HsTimeCardList timeCardList = new HsTimeCardList();
                    //		timeCardList.Details = this.Details;
                    //		timeCardList.LoadFromXml(punchXml);

                    //		timeCardImport.EmpList = PunchRecordUtil.GetEmpList(this.Details, punchXml);
                    //		timeCardImport.TCList = timeCardList;
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(punchXml);
                    timeCardImport.PunchXml = doc;
                    logger.Debug("BEFORE:  " + timeCardImport.PunchXml.OuterXml);
                    timeCardImport.Execute();
                    logger.Debug("AFTER:  " + timeCardImport.PunchXml.OuterXml);
                    prService.verifyAdjEmpPunches(this.Details.ClientId, timeCardImport.PunchXml.OuterXml);
                }

            }
            return false;
        }
    }
}
