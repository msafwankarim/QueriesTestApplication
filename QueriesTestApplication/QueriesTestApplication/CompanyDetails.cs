using System;
using System.Collections.Generic;
using System.Text;

namespace QueriesTestApplication
{
    public class CompanyDetails
    {
        public string Name { get; set; }
        public int TotalEmloyees { get; set; }
        public int Id { get; set; }
       
        public CompanyDetails(string companyName, int totalEmloyees, int iD)
        {
            Name = companyName;
            TotalEmloyees = totalEmloyees;
            Id = iD; 
        }
    }
}
