using System;
using System.Collections;
using HsSharedObjects.Client;

namespace HsConnect.HistoricalSales
{
    public interface HistoricalSalesItemList
    {
        void DbLoad();
        void DbUpdate();
        void DbInsert();
        void Add(HistoricalSalesItem item);
        void SetDataConnection(String str);
        String GetXmlString();
        IEnumerator GetEnumerator();
        int Count { get; set; }
        DateTime EndDate { get; set; }
        DateTime StartDate { get; set; }
        DateTime Dob { get; set; }
        bool Updated { get; set; }
        bool AutoSync { get; set; }
        ClientDetails Details { get; set; }
        ArrayList GetSalesItemList();
        void calculateClientShifts();
    }
}
