using System;
using System.Collections;
using System.Xml;
using HsConnect.Services.Wss;
using HsConnect.Xml;

namespace HsConnect.TimeCards
{
    /// <summary>
    /// Summary description for ClientEmployeeWaiverList.
    /// </summary>
    public class ClientEmployeeWaiverList
    {
        private ArrayList empWaivers;
        private Hashtable empWaiverMap;
        private int clientId;

        public ClientEmployeeWaiverList(int cid)
        {
            clientId = cid;
            empWaivers = new ArrayList();
            empWaiverMap = new Hashtable();
            init();
        }

        private void init()
        {
            ClientEmployeesWss wss = new ClientEmployeesWss();
            String xmlString = wss.getClientEmpWaivers(clientId);
            parseXML(xmlString);
        }

        private void parseXML(String xml)
        {
            HsXmlReader reader = new HsXmlReader();
            reader.LoadXml(xml);
            foreach (XmlNode user in reader.SelectNodes("/client-employee-waiver-list/employee-waiver"))
            {
                String posId = user.Attributes["emp-pos-id"].Value;
                empWaiverMap.Add(Int32.Parse(posId), posId);
            }
        }

        public bool empHasWaiver(int empId)
        {
            String id = empWaiverMap[empId].ToString();
            return id != null;
        }

        public Hashtable EmpWaiverMap
        {
            get { return empWaiverMap; }
        }

    }
}
