using System;
using System.Collections;
using HsConnect.Data;
using HsSharedObjects.Client;

namespace HsConnect.Employees
{
	public interface EmployeeList
	{
		void DbLoad();
		void DbLoad( bool activeOnly );
		void DbUpdate();
		void DbInsert();
        void DbInsert(GoHireEmpInsertList ls);
		void Add( Employee emp );
		void SetDataConnection( String str );
		void SortByLastName();
		void UpdateExtId( int hsId , int posId );

		HsDataConnection Cnx{ get;set; }
		String ListName{ get;set; }
		String GetXmlString();
		String GetShortXmlString();
		IEnumerator GetEnumerator();
		Employee GetMatch( Employee emp );
		bool ContainsHsEmp( Employee emp );
		Employee GetEmployeeByExtId( int id );
		Employee GetEmployeeByHsId( int id );
		int Count{ get;set; }
		bool Updated{ get;set; }
		ClientDetails Details{ get;set; }
        Employee[] ToArray();
        Boolean Remove(Employee emp);
	}
}
