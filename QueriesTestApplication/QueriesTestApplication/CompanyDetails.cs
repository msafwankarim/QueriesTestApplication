using Alachisoft.NCache.Runtime.Caching;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueriesTestApplication
{
    [QueryIndexable]
    [Serializable]
    public class CompanyDetails
    {
        public string Name { get; set; }
        public int TotalEmloyees { get; set; }
        public int Id { get; set; }        
        public string SID { get; set; }

        public ArrayList Employees { get; set; } = new ArrayList();

        public List<string> EmployeeList { get; set; } = new List<string>();

        public IDictionary<int, string> EmployeePairs { get; set; } = new Dictionary<int, string>();
        public Queue<string> EmployeeQueue { get; set; } = new Queue<string>();

        public CompanyDetails(string companyName, int totalEmloyees, int iD)
        {
            Name = companyName;
            TotalEmloyees = totalEmloyees;
            Id = iD; 
        }
        
        public void Add(string name)
        {
            Employees.Add(name);
            EmployeeList.Add(name);
            EmployeePairs.Add(EmployeeList.Count - 1, name);
            EmployeeQueue.Enqueue(name);
        }
    }
}
