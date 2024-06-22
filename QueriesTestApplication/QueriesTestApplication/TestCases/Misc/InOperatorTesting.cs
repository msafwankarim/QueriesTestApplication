using Alachisoft.NCache.Client;
using Alachisoft.NCache.Sample.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace QueriesTestApplication
{
    public class InOperatorTesting
    {
        ICache cache;
        public Dictionary<string, ResultStatus> testResults;

        public Dictionary<string, ResultStatus> TestResults
        {
            get { return testResults; }
        }

        public InOperatorTesting()
        {
            cache = CacheManager.GetCache(Common.CacheName);
            testResults = new Dictionary<string, ResultStatus>();
        }
        /// <summary>
        /// Verifies IN operator testing by giving array list with ?
        /// </summary>
        private void VerifyPreviousSyntax()
        {
            cache.Clear();
            string methodName = "VerifyPreviousSyntax";
            for (int i = 0; i < 10; i++)
            {
                cache.Insert(i.ToString(), new Product() { Id = i, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 35, Order = new Order { OrderID = 10, ShipCity = "rawalpindi", ShipCountry = "Pakistan" } });
            }
            try
            {
                string query = "SELECT Name, Category From Alachisoft.NCache.Sample.Data.Product WHERE Id IN (?,?)";

                var queryCommand = new QueryCommand(query);
                ArrayList parameters = new ArrayList();
                parameters.Add(1);
                parameters.Add(5);                

                queryCommand.Parameters.Add("Id", parameters);

                ICacheReader reader = cache.SearchService.ExecuteReader(queryCommand);

                if (reader.FieldCount > 0)
                {
                    Console.WriteLine("Seccess: Previous Syntax of IN Operator ");
                    testResults.Add(methodName, ResultStatus.Success);
                    /*while (reader.Read())
                    {
                        // Get the value of the result set
                        string ProductKey = reader.GetValue<string>(0);
                        string ProductName = reader.GetValue<string>(1);
                        string Category = reader.GetValue<string>(2);
                    }*/
                }
                else
                {
                    Console.WriteLine("Failure: Previous Syntax of IN Operator ");
                    testResults.Add(methodName, ResultStatus.Failure);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failure: Previous Syntax of IN Operator ");
                testResults.Add(methodName, ResultStatus.Failure);
            }

        }

        /// <summary>
        /// Verifies IN operator by giving array list as NamedParam (@)
        /// </summary>
        public void VerifyInoperatorWithNamedParam()
        {
            cache.Clear();
            string methodName = "VerifyInoperatorWithNamedParam";
            for (int i = 0; i < 10; i++)
            {
                cache.Insert(i.ToString(), new Product() { Id = i, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 35, Order = new Order { OrderID = 10, ShipCity = "rawalpindi", ShipCountry = "Pakistan" } });
            }

            try
            {
                string query = "SELECT Name, Category From Alachisoft.NCache.Sample.Data.Product WHERE Id IN (@Parameters)";

                var queryCommand = new QueryCommand(query);

                // List<int> parameters = new List<int>();
                ArrayList parameters = new ArrayList();
                parameters.Add(1);
                parameters.Add(5);

                queryCommand.Parameters.Add("@Parameters", parameters);

                ICacheReader reader = cache.SearchService.ExecuteReader(queryCommand);

                if (reader.FieldCount > 0)
                {
                    Console.WriteLine("Succccess: Verify Inoperator With NamedParam");
                    testResults.Add(methodName, ResultStatus.Success);
                    /*while (reader.Read())
                    {
                        // Get the value of the result set
                        string ProductKey = reader.GetValue<string>(0);
                        string ProductName = reader.GetValue<string>(1);
                        string Category = reader.GetValue<string>(2);
                    }*/
                }
                else
                {
                    Console.WriteLine("Failure: Verify Inoperator With NamedParam ");
                    testResults.Add(methodName, ResultStatus.Failure);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failure: Verify Inoperator With NamedParam ");
                testResults.Add(methodName, ResultStatus.Failure);
            }

        }

    }
}
