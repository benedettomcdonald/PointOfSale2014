using System;
using HsConnect.SalesItems;

namespace HsConnect.HistoricalSales
{
    public class HistoricalSalesItem : SalesItem
    {
        private int employeeNum;
        private DateTime salesDate = new DateTime( 0 );
        private string histExtId;

        public int EmployeeNum
        {
            get { return this.employeeNum; }
            set { this.employeeNum = value; }
        }
        
        public DateTime SalesDate
        {
            get { return this.salesDate; }
            set { this.salesDate = value; }
        }

        public String HistExtId
        {
            get { return this.histExtId; }
            set { this.histExtId = value; }
        }
    }
}
