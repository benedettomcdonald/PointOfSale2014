using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using HsConnect.HistoricalSales;
using HsConnect.Main;
using HsConnect.Services.Wss;
using HsFileTransfer;
using HsSharedObjects.Client.Module;
using System.Collections;
using HsConnect.SalesItems;

namespace HsConnect.Modules
{
    public class HistoricalSalesModule : ModuleImpl
    {
        private SysLog logger;
        public static bool _histSyncRunning = false;
        private readonly String dir = Application.StartupPath + "\\hist_sales_sync";
        private readonly int MULTICLIENT_SALES_FILE_PROC_ID = 208;

        public HistoricalSalesModule()
		{
			this.logger = new SysLog( this.GetType() );
		}

        public override bool Execute()
        {
            _histSyncRunning = true;
            //probably don't need this check since it's done in Timers.update_module_timers()
            if (Details.ModuleList.IsActive(ClientModule.HISTORICAL_SALES))
            {
                logger.Log("Executing Historical Sales Sync");

                ClientSalesWss salesService = new ClientSalesWss();

                HistoricalSalesManager salesManager = new HistoricalSalesManager(this.Details);
                HistoricalSalesItemList salesList = salesManager.GetPosHistSalesItemList();
                salesList.Details = this.Details;

                DateTime myStartDate = DateTime.Today.AddDays(-15);
                DateTime myEndDate = DateTime.Today;

                for (int i = 1; i <= 26; i++)
                {
                    logger.Debug("loading sales list for " + i + " weeks in the past");
                    myStartDate = DateTime.Today.AddDays(-15 * (i));
                    myEndDate = DateTime.Today.AddDays(-15 * (i - 1));

                    salesList.StartDate = myStartDate;
                    salesList.EndDate = myEndDate;

                    logger.Debug("sales start date: " + salesList.StartDate.ToString());
                    logger.Debug("sales end date: " + salesList.EndDate.ToString());

                    salesList.DbLoad();
                    logger.Debug("loaded sales list for " + i + " weeks in the past: " + salesList.Count);

                    logger.Debug("salesList.Count = " + salesList.Count);

                    if (salesList.Count > 0)
                    {
                        logger.Debug(salesList.GetXmlString());

                        String fileName = "HistSales_" + Details.ClientId + "_" + i + ".csv";

                        createFile(salesList, fileName);

                        FileForTransfer f = new FileForTransfer();
                        f.FilePaths = new ArrayList();
                        f.FilePaths.Add(dir + "\\" + fileName);
                        f.ProcRef = MULTICLIENT_SALES_FILE_PROC_ID; //configure to use toplevel multiclient.salesItemProcessor

                        //some retry logic in case there is a problem with the web connection
                        //while attempting a file send
                        bool success = false;
                        int remainingRetries = 3;
                        while (!success && remainingRetries > 0)
                        {
                            try
                            {
                                XferUtil.Process(f, Details.ClientCompanyExtRef);
                                success = true;
                            }
                            catch (System.Net.WebException e)
                            {
                                success = false;
                                remainingRetries--;
                                logger.Log("WARNING: problem sending historical sales file " + fileName + " ; " + remainingRetries + " retries remaining.");
                                logger.Log(e.StackTrace);
                            }
                        }

                        if (success == false)
                        {
                            logger.Error("ERROR: Problem sending historical sales file " + fileName + " ; this file has been skipped.");
                        }
                        else
                        {
                            logger.Debug("sent[" + salesList.Count + "] sales items for processing");
                        }
                        f = null;
                    }

                    salesList = null;
                    salesList = salesManager.GetPosHistSalesItemList();
                    salesList.Details = this.Details;
                    salesList.calculateClientShifts();

                }//for

                logger.Debug("executed HistoricalSalesModule");
                RemoteLogger.Log(Details.ClientId, RemoteLogger.HISTORICAL_SALES_SUCCESS);

                // Call ClientSalesWss.completeHistoricalSync() to disable the module
                salesService.completeHistoricalSync(Details.ClientId);
                //wait 20 minutes to ensure we don't duplicate before updated ClientDetails comes through
                logger.Log("Historical Sales Sync sleeping to allow server side activity to complete");
                Thread.Sleep(1200000);
                logger.Log("Historical Sales Sync waking up");
                
            }//if enabled
            //complete historical sync by turning off the flag
            _histSyncRunning = false;
            logger.Log("Historical Sales Sync Complete");
            return true;
        }//Execute()

        private void createFile(HistoricalSalesItemList ls, String fileName)
        {
            logger.Debug("Generating " + fileName);
            String file = dir + "\\" + fileName;

            checkFileExistence(file);

            writeHistImportFile(ls.GetSalesItemList(), file);
        }

        private void writeHistImportFile(ArrayList rows, String file)
        {
            TextWriter writer = null;
            try
            {
                writer = new StreamWriter(file);
                //headers - these actually cause a parse exception in server-side multiclient salesItemProcessor, so leave them out of the file
                //writer.WriteLine("CompanyNum|ConceptNum|StoreNum|DateofBusiness|Sales Date|Amount|Employee|RVC|Category");
                foreach (HistoricalSalesItem s in rows)
                {
                    writer.Write(Details.ClientCompanyExtRef + "|");
                    writer.Write(Details.ClientConceptExtRef + "|");
                    writer.Write(Details.ClientExtRef + "|");
                    writer.Write(s.BusinessDate.ToString("yyyy-MM-dd HH:mm:ss.fff") + "|");
                    writer.Write(s.SalesDate.ToString("yyyy-MM-dd HH:mm:ss.fff") + "|");
                    writer.Write(s.Amount + "|");
                    writer.Write(s.EmployeeNum + "|");
                    writer.Write(s.RVC + "|");
                    writer.Write(s.Category);
                    writer.WriteLine();
                }
            }
            catch (IOException e)
            {
                logger.Error(e.StackTrace);
            }
            finally
            {
                writer.Close();
            }
            logger.Debug("Successfully generated " + file);
        }

        private void checkFileExistence(string file)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (File.Exists(file))
                File.Delete(file);
        }
    }//class
}//namespace
