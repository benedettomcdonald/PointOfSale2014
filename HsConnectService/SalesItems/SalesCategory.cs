using System;

namespace HsConnect.SalesItems
{
    public class SalesCategory
    {
        private int _extId;
        private string _name;

        public SalesCategory(int extId, String name)
        {
            ExtId = extId;
            Name = name;
        }

        public int ExtId
        {
            get { return _extId; }
            set { _extId = value; }
        }
        public String Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}