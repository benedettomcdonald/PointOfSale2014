using System;
using System.Text;
using System.Collections;

namespace HsConnect.Employees
{
    public class GoHireEmpInsert
    {
        public string trxId = "";
        public int userNum = -1;
        public string firstName = "";
        public string lastName = "";
        public string addr1 = "";
        public string addr2 = "";
        public string city = "";
        public string state = "";
        public string zip = "";
        public string phoneNumber = "";
        public string ssn = "111111111";
        public DateTime birthDate = new DateTime(0);
        public DateTime hireDate = new DateTime(0);

        public int insertStatus = 0;
        public string insertReason = "";

        public ArrayList jobs = new ArrayList();

        public GoHireEmpInsert() { }

        //TODO: take this out later, or mask SSN somehow
        //it's only for testing purposes
        public String ToLogString()
        {
            String ret = "";

            ret += "trxId: " + trxId + " ;; ";
            ret += "userNum: " + userNum + " ;; ";
            ret += "firstName: " + firstName + " ;; ";
            ret += "lastName: " + lastName + " ;; ";
            ret += "addr1: " + addr1 + " ;; ";
            ret += "addr2: " + addr2 + " ;; ";
            ret += "city: " + city + " ;; ";
            ret += "state: " + state + " ;; ";
            ret += "zip: " + zip + " ;; ";
            ret += "phoneNumber: " + phoneNumber + " ;; ";
            ret += "ssn: " + ssn + " ;; ";
            ret += "birthDate: " + birthDate + " ;; ";
            ret += "hireDate: " + hireDate + " ;; ";

            return ret;
        }
    }
}
