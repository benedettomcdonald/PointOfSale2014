using System;

namespace HsConnect.SalesItems
{
    public class RevenueCenter
    {
        private string _name;
        private int _extId;

        public RevenueCenter(int extId, String name)
        {
            ExtId = extId;
            Name = name;
        }

        public string Name
        {
            get { return _name; } 
            set { _name = value; }
        }
        public int ExtId
        {
            get { return _extId;}
            set { _extId = value; }
        }

    }
}