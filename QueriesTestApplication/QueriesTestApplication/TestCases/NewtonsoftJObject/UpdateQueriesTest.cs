using Alachisoft.NCache.Client;
using Alachisoft.NCache.Runtime.Caching;
using Alachisoft.NCache.Runtime.Exceptions;
using Alachisoft.NCache.Runtime.JSON;
using Alachisoft.NCache.Sample.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QueriesTestApplication.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using ReportHelper = QueriesTestApplication.Utils.ReportHelper;

namespace QueriesTestApplication
{
    class UpdateQueriesTest
    {
        ICache cache;
        public Dictionary<string, ResultStatus> testResults;
        int count = 0;
        List<Product> productList;
        List<Product> complexProductList;

        Report _report;

        public UpdateQueriesTest()
        {
            cache = CacheManager.GetCache(Common.CacheName);
            testResults = new Dictionary<string, ResultStatus>();
            productList = new List<Product>();
            complexProductList = new List<Product>();

            _report = new Report(nameof(UpdateQueriesTest));

            PopulateProductList();
        }

        public Report Report { get => _report; }

        public Dictionary<string, ResultStatus> TestResults
        {
            get { return testResults; }
        }

        #region --------------------------------- Copy Operation ---------------------------------


        /// <summary>
        /// Inserts 5000 Items in cache.
        /// Then copies value of Order.ShipCity to Order.ShipCountry for all 5000 objects, using Move.
        /// </summary>
        public void CopyQuery1()
        {
            int updated = 0;
            cache.Clear();
            // PopulateCache();
            string ProductKey = "product1";
            string methodName = MethodBase.GetCurrentMethod().Name;

            count++;

            for (int i = 0; i < 5000; i++)
            {
                cache.Insert(i.ToString(), new Product() { Id = i, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 35, Order = new Order { OrderID = 10, ShipCity = "rawalpindi", ShipCountry = "Pakistan" } });
            }


            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Copy Order.ShipCountry = Order.ShipCity ";

                QueryCommand queryCommand = new QueryCommand(query);
                // queryCommand.Parameters.Add("Id", Convert.ToInt32(1));
                IDictionary<string, Exception> dictionary = new Dictionary<string, Exception>();
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);
                int TotalFailedOperations = 0;
                
                foreach (var val in dictionary)
                {
                    TotalFailedOperations++;
                }

                TotalFailedOperations = 0;
                int TotalSuccessedOperations = 0;

                for (int i = 0; i < 5000; i++)
                {

                    var result = cache.Get<JsonObject>(i.ToString());
                    var OrderInCache = (JsonObject)result.GetAttributeValue("Order");

                    var Country = OrderInCache.GetAttributeValue("ShipCountry");

                    if (Country.Value.ToString() == "rawalpindi")
                    {
                        TotalSuccessedOperations++;
                        continue;
                    }
                    else
                    {
                        TotalFailedOperations++;
                        ReportHelper.PrintError($"Key of Failed object is : {i} \n Country is {Country.Value.ToString()}");
                    }
                }

                if (TotalFailedOperations == 0)
                {
                    _report.AddPassedTestCase(methodName, "Seccess:Partial Update items using CopyQuery1 ");
                    testResults.Add(methodName, ResultStatus.Success);
                }
                else
                {
                    throw new Exception("Failure:Partial Update items using CopyQuery1 ");
                }
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
                testResults.Add(methodName, ResultStatus.Failure);
            }

        }
        private void CopyNullAttribute()
        {
            int updated = 0;
            cache.Clear();
            // PopulateCache();
            string ProductKey = "product1";
            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;

            for (int i = 0; i < 5000; i++)
            {
                cache.Insert(i.ToString(), new Product() { Id = i, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 35, Order = new Order { OrderID = 10, ShipCity = "rawalpindi", ShipCountry = "Pakistan" } });
            }


            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Copy Order.ShipCountry = null";

                QueryCommand queryCommand = new QueryCommand(query);
                // queryCommand.Parameters.Add("Id", Convert.ToInt32(1));
                IDictionary<string, Exception> dictionary = new Dictionary<string, Exception>();
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);
                int TotalFailedOperations = 0;
                
                foreach (var val in dictionary)
                {
                    TotalFailedOperations++;
                }

                TotalFailedOperations = 0;
                int TotalSuccessedOperations = 0;

                for (int i = 0; i < 5000; i++)
                {

                    var result = cache.Get<JsonObject>(i.ToString());
                    var OrderInCache = (JsonObject)result.GetAttributeValue("Order");

                    var Country = OrderInCache.GetAttributeValue("ShipCountry");

                    if (Country.Value.ToString() == null)
                    {
                        TotalSuccessedOperations++;
                        continue;
                    }
                    else
                    {
                        TotalFailedOperations++;
                        ReportHelper.PrintError($"Key of Failed object is : {i} \n Country is {Country.Value.ToString()}");
                    }
                }

                if (TotalFailedOperations == 0)
                {
                    _report.AddPassedTestCase(methodName, "Seccess:Partial Update items using CopyQuery1 ");
                    testResults.Add(methodName, ResultStatus.Success);
                }
                else
                {
                    throw new Exception("Failure:Partial Update items using CopyQuery1 ");
                }
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
                testResults.Add(methodName, ResultStatus.Failure);
            }

        }
        private void Copy2Attributes()
        {
            int updated = 0;
            cache.Clear();
            // PopulateCache();
            string ProductKey = "product1";
            string methodName = MethodBase.GetCurrentMethod().Name;

            count++;

            for (int i = 0; i < 5000; i++)
            {
                cache.Insert(i.ToString(), new Product() { Id = i, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 35, Order = new Order { OrderID = 10, ShipCity = "rawalpindi", ShipCountry = "Pakistan" } });
            }


            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Copy Order.ShipCountry = Order.ShipCity ,Order.Category=Order.ClassName";

                QueryCommand queryCommand = new QueryCommand(query);
                // queryCommand.Parameters.Add("Id", Convert.ToInt32(1));
                IDictionary<string, Exception> dictionary = new Dictionary<string,Exception>();
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);
                int TotalFailedOperations = 0;
                
                foreach (var val in dictionary)
                {
                    TotalFailedOperations++;
                }

                TotalFailedOperations = 0;
                int TotalSuccessedOperations = 0;

                for (int i = 0; i < 5000; i++)
                {

                    var result = cache.Get<JsonObject>(i.ToString());
                    var OrderInCache = (JsonObject)result.GetAttributeValue("Order");

                    var Category = OrderInCache.GetAttributeValue("Category");
                    var Country = OrderInCache.GetAttributeValue("ShipCountry");

                    if (Country.Value.ToString() == "rawalpindi" && Category.Value.ToString() == "Electronics")
                    {
                        TotalSuccessedOperations++;
                        continue;
                    }
                    else
                    {
                        TotalFailedOperations++;
                        ReportHelper.PrintError($"Key of Failed object is : {i} \n Country is {Country.Value.ToString()} & Category is {Category.Value.ToString()}");
                    }
                }

                if (TotalFailedOperations == 0)
                {
                    _report.AddPassedTestCase(methodName, "Seccess:Partial Update items using Copy2Attributes ");
                    testResults.Add(methodName, ResultStatus.Success);
                }
                else
                {
                    throw new Exception("Failure:Partial Update items using Copy2Attributes ");
                }
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
                testResults.Add(methodName, ResultStatus.Failure);
            }

        }

        /// <summary>
        /// Copies value of source attribute to destination attribute
        /// </summary>
        /// <remarks>
        /// If Destination Attribute doesnot exist, it is created.   
        /// </remarks>
        private void CopyQuery()
        {
            int updated = 0;
            cache.Clear();
            PopulateCache();
            string ProductKey = "product1";
            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;
            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Copy Order.ShipCountry = Order.ShipCity Where Id=?";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary<string, Exception> dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);
                
                

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
                Console.WriteLine("RunTime " + elapsedTime);

                var prod = cache.Get<JsonObject>(ProductKey);
                JsonObject order = new JsonObject();
                order = (JsonObject)prod.GetAttributeValue("Order");
                string ShipCountry = (string)order.GetAttributeValue("ShipCountry").Value;
                if (ShipCountry == "rawalpindi")
                    _report.AddPassedTestCase(methodName, "Success: Partial Update items using Copy query");
                else
                    throw new Exception("Failure:Partial Update items using Copy query");


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
                testResults.Add(methodName, ResultStatus.Failure);
            }


        }


        private void CopyArrayToArray()
        {
            int updated = 0;
            cache.Clear();
            PopulateCache();
            string ProductKey = "product1";
            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;
            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Copy this.Images[0] = this.Images[1] Where Id=?";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));


                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                var exception = new Exception("Failure:Partial Update item to copy array indexes");

                if (updated == 0)
                    throw exception;

                

                _ = cache.Get<JsonObject>(ProductKey);

                var res = cache.Get<Product>(ProductKey);

                if (res.Images[0].FileName == res.Images[1].FileName)
                    _report.AddPassedTestCase(methodName, "Success: Partial Update item to copy array indexes");
                else
                    throw exception;

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }


        }
        private void CopyArrayToObject()
        {
            int updated = 0;
            cache.Clear();
            PopulateCache();
            string ProductKey = "product1";
            string methodName = MethodBase.GetCurrentMethod().Name;

            count++;
            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Copy this.Images[0] = this.Order Where Id=?";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));


                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                var exception = new Exception("Failure:Partial Update item to copy array indexes");
                                throw exception;
            }
            catch (Exception ex)
            {
                if (Helper.IsIncorrectFormatException(ex))
                {
                    _report.AddPassedTestCase(methodName, "CopyArrayToObject : Passed");
                }
                else
                {
                    _report.AddFailedTestCase(methodName, ex);

                }

            }


        }
        public void CopyProductIdToOrderObject()
        {
            int updated = 0;
            cache.Clear();
            var keys = PopulateCacheAndGetKeys();
            string ProductKey = "product1";
            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "Copy product id to order object";
            count++;
            try
            {
                string query = $"Update  Alachisoft.NCache.Sample.Data.Product  Copy this.Order = this.Id Where Id = 1 ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary<string, Exception> dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                // product ID should not be replaced by order object

                

                throw new Exception(description);

                var proda1 = cache.Get<Product>(ProductKey);  // exception
                var prod1 = cache.Get<JsonObject>(ProductKey);




            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }


        }
        public void CopyOnAttributeThatDoesnotExist()
        {
            int updated = 0;
            cache.Clear();
            // PopulateCache();
            string ProductKey = "product1";
            string methodName = MethodBase.GetCurrentMethod().Name;

            count++;

            for (int i = 0; i < 5000; i++)
            {
                cache.Insert(i.ToString(), new Product() { Id = i, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 35, Order = new Order { OrderID = 10, ShipCity = "rawalpindi", ShipCountry = "Pakistan" } });
            }


            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Copy Order.AttributeThatDoesnotExist = Order.ShipCity ";

                QueryCommand queryCommand = new QueryCommand(query);
                // queryCommand.Parameters.Add("Id", Convert.ToInt32(1));
                IDictionary<string, Exception> dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);
                                throw new Exception("CopyOnAttributeThatDoesnotExist : Failed");
            }
            catch (Exception ex)
            {
                if (Helper.IsTargetNotFoundException(ex))
                {
                    _report.AddPassedTestCase(methodName, "CopyOnAttributeThatDoesnotExist : Passed");

                }
                else
                {
                    _report.AddFailedTestCase(methodName, ex);

                }
            }


        }
        public void CopyAnAttributeThatDoesnotExist()
        {
            int updated = 0;
            cache.Clear();
            // PopulateCache();
            string ProductKey = "product1";
            string methodName = MethodBase.GetCurrentMethod().Name;

            count++;

            for (int i = 0; i < 5000; i++)
            {
                cache.Insert(i.ToString(), new Product() { Id = i, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 35, Order = new Order { OrderID = 10, ShipCity = "rawalpindi", ShipCountry = "Pakistan" } });
            }


            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Copy Order.ShipCountry = Order.AttributeThatDoesnotExist ";

                QueryCommand queryCommand = new QueryCommand(query);
                // queryCommand.Parameters.Add("Id", Convert.ToInt32(1));
                IDictionary<string, Exception> dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);
                int TotalFailedOperations = 0;
                                throw new Exception("CopyOnAttributeThatDoesnotExist : Failed");
            }
            catch (Exception ex)
            {
                if (Helper.IsTargetNotFoundException(ex))
                {
                    _report.AddPassedTestCase(methodName, "CopyOnAttributeThatDoesnotExist : Passed");

                }
                else
                {
                    _report.AddFailedTestCase(methodName, ex);

                }
            }


        }
        #endregion


        #region  --------------------------------- Move Operation ---------------------------------

        /// <summary>
        /// Copies the Value of given source attribute to the destination attribute and then removes the source attribute.
        /// </summary>
        private void MoveQuery()
        {
            int updated = 0;
            cache.Clear();
            PopulateCache();
            string ProductKey = "product1";
            string methodName = MethodBase.GetCurrentMethod().Name;

            count++;
            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Move Order.ShipCountry = Order.ShipCity ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary<string, Exception> dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                
                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
                Console.WriteLine("RunTime " + elapsedTime);

                var prod = cache.Get<JsonObject>(ProductKey);
                JsonObject order = (JsonObject)prod.GetAttributeValue("Order");
                if (order.ContainsAttribute("ShipCity"))
                {
                    throw new Exception("Failure:Partial Update items using Move query");
                    testResults.Add(methodName, ResultStatus.Failure);
                    return;
                }

                string ShipCountry = (string)order.GetAttributeValue("ShipCountry").Value;
                if (ShipCountry == "rawalpindi")
                    _report.AddPassedTestCase(methodName, "Success: Partial Update items using query");
                else
                    throw new Exception("Failure:Partial Update items using Move query");


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }
        private void MoveDoubleAttributes()
        {
            int updated = 0;
            cache.Clear();
            PopulateCache();
            string ProductKey = "product1";
            string methodName = MethodBase.GetCurrentMethod().Name;

            count++;
            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Move Order.ShipCountry = Order.ShipCity ,Order.Category=Order.ClassName ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary<string, Exception> dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
                Console.WriteLine("RunTime " + elapsedTime);

                var prod = cache.Get<JsonObject>(ProductKey);
                JsonObject order = (JsonObject)prod.GetAttributeValue("Order");
                if (order.ContainsAttribute("ShipCity"))
                {
                    throw new Exception("Failure:Partial Update items using Move query");
                    testResults.Add(methodName, ResultStatus.Failure);
                    return;
                }

                string ShipCountry = (string)order.GetAttributeValue("ShipCountry").Value;
                var Cateory = (string)order.GetAttributeValue("Category").Value;
                if (ShipCountry == "rawalpindi" && Cateory == "Electronics")
                    _report.AddPassedTestCase(methodName, "Success: Partial Update items using query");
                else
                    throw new Exception("Failure:Partial Update items using Move query");


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }
        private void MoveNullAttribute()
        {
            int updated = 0;
            cache.Clear();
            PopulateCache();
            string ProductKey = "product1";
            string methodName = MethodBase.GetCurrentMethod().Name;

            count++;
            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Move Order.ShipCountry = null ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary<string, Exception> dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                
                throw new Exception("Failure:Partial Update items using Move query");


            }
            catch (Exception ex)
            {
                if (Helper.IsTargetNotFoundException(ex))
                {
                    _report.AddPassedTestCase(methodName, "MoveNullAttribute :Passed");

                }
                else
                {
                    _report.AddFailedTestCase(methodName, ex);

                }
            }


        }
        private void MoveQueryWithWhereCaluse()
        {
            int updated = 0;
            cache.Clear();
            PopulateCache();
            string ProductKey = "product1";
            string methodName = MethodBase.GetCurrentMethod().Name;

            count++;
            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Move Order.ShipCountry = Order.ShipCity Where Id=?";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary<string, Exception> dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
                Console.WriteLine("RunTime " + elapsedTime);

                var prod = cache.Get<JsonObject>(ProductKey);
                JsonObject order = (JsonObject)prod.GetAttributeValue("Order");
                if (order.ContainsAttribute("ShipCity"))
                {
                    throw new Exception("Failure:Partial Update items using Move query");
                    testResults.Add(methodName, ResultStatus.Failure);
                    return;
                }

                string ShipCountry = (string)order.GetAttributeValue("ShipCountry").Value;
                if (ShipCountry == "rawalpindi")
                    _report.AddPassedTestCase(methodName, "Success: Partial Update items using query");
                else
                    throw new Exception("Failure:Partial Update items using Move query");


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }
        private void MoveOnAttributeThatDoesnotEixst()
        {
            int updated = 0;
            cache.Clear();
            PopulateCache();
            string ProductKey = "product1";
            string methodName = MethodBase.GetCurrentMethod().Name;

            count++;
            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Move Order.AttributeThatDoesnotEixst = Order.ShipCity";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary<string, Exception> dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);
                
                throw new Exception("Failure:Partial Update items using Move query");


            }
            catch (Exception ex)
            {
                if (Helper.IsTargetNotFoundException(ex))
                {
                    _report.AddPassedTestCase(methodName, "MoveOnAttributeThatDoesnotEixst :Passed");

                }
                else
                {
                    _report.AddFailedTestCase(methodName, ex);

                }
            }


        }

        private void MoveAnAttributeThatDoesnotEixst()
        {
            int updated = 0;
            cache.Clear();
            PopulateCache();
            string ProductKey = "product1";
            string methodName = MethodBase.GetCurrentMethod().Name;

            count++;
            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Move Order.ShipCountry = Order.AttributeThatDoesnotEixst";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary<string, Exception> dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);
                
                throw new Exception("Failure:Partial Update items using Move query");


            }
            catch (Exception ex)
            {
                if (Helper.IsTargetNotFoundException(ex))
                {
                    _report.AddPassedTestCase(methodName, "AttributeThatDoesnotEixst :Passed");

                }
                else
                {
                    _report.AddFailedTestCase(methodName, ex);

                }
            }


        }
        private void MoveArrayToArray()
        {
            int updated = 0;
            cache.Clear();
            PopulateCache();
            string ProductKey = "product1";
            string methodName = MethodBase.GetCurrentMethod().Name;

            count++;
            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Move this.Images[0] = this.Images[1] Where Id=?";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));


                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                var exception = new Exception("Failure:Partial Update item to copy array indexes");

                if (updated == 0)
                    throw exception;

                

                _ = cache.Get<JsonObject>(ProductKey);

                var res = cache.Get<Product>(ProductKey);

                if (res.Images[0].FileName == res.Images[1].FileName)
                    _report.AddPassedTestCase(methodName, "Success: Partial Update item to copy array indexes");
                else
                    throw exception;

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }


        }
        private void MoveArrayToObject()
        {
            int updated = 0;
            cache.Clear();
            PopulateCache();
            string ProductKey = "product1";
            string methodName = MethodBase.GetCurrentMethod().Name;

            count++;
            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Move this.Images[0] = this.Order ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));


                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                var exception = new Exception("Failure:Partial Update item to move array indexes");
                                throw exception;
            }
            catch (Exception ex)
            {
                if (Helper.IsIncorrectFormatException(ex))
                {
                    _report.AddPassedTestCase(methodName, "MoveArrayToObject : Passed");
                }
                else
                {
                    _report.AddFailedTestCase(methodName, ex);

                }

            }


        }

        #endregion


        #region --------------------------------- Test Operation ---------------------------------


        /// <summary>
        /// Checks if the the value given for specified attributes is the same as in Cache or not
        /// If the Given value for an attribute is not the same as in Cache, Exception is returned in Hashtable
        /// </summary>
        private void TestQuery()
        {
            int updated = 0;
            cache.Clear();
            PopulateCache();
            string methodName = MethodBase.GetCurrentMethod().Name;

            count++;
            try
            {

                string query = "Update  Alachisoft.NCache.Sample.Data.Product Test Name = '\"Chai\"', Order.OrderID=10 Where Id=?";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary<string, Exception> dictionary = new Dictionary<string, Exception>();
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
                Console.WriteLine("RunTime " + elapsedTime);

                if (dictionary.Count > 0)
                    throw new Exception("Failure:Partial Update items using Test query");
                else
                    _report.AddPassedTestCase(methodName, "Success: Partial Update items using Test query");



            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }


        /// <summary>
        /// Checks if the value given in JObject is the same as in cache, for the specified path
        /// </summary>
        public void TestOperationUsingJObject()
        {
            int updated = 0;
            cache.Clear();
            PopulateCache();
            string methodName = MethodBase.GetCurrentMethod().Name;

            count++;
            try
            {

                var order = new Order { OrderID = 10, ShipCity = "rawalpindi", ShipCountry = "Pakistan" };
                var jorder = JObject.Parse(JsonConvert.SerializeObject(order));

                string query = "Update Alachisoft.NCache.Sample.Data.Product Test Order = @ProductName Where Id=?";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@ProductName", jorder);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary<string, Exception> dictionary = new Dictionary<string, Exception>();
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
                Console.WriteLine("RunTime " + elapsedTime);

                if (dictionary.Count > 0)
                {
                    throw new Exception("Failure:Partial Update, Test Operation using JObject");
                }
                else
                {
                    _report.AddPassedTestCase(methodName, "Success: Partial Update, Test Operation using JObject");
                }

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }

        public void TestOperationUsingJObject1()
        {
            int updated = 0;
            cache.Clear();
            PopulateCache();
            string methodName = MethodBase.GetCurrentMethod().Name;

            count++;
            try
            {

                var order = new Order { OrderID = 10, ShipCity = "rawalpindiiiiiiiiiiiiiiiii", ShipCountry = "Pakistan" };
                var jorder = JObject.Parse(JsonConvert.SerializeObject(order));

                string query = "Update Alachisoft.NCache.Sample.Data.Product Test Order = @ProductName Where Id=?";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@ProductName", jorder);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary<string, Exception> dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);


                
                _report.AddFailedTestCase(methodName, new Exception("Test Object with wrong values didn't throw exception"));



            }
            catch (Exception ex)
            {
                if (Helper.IsTestOperationException(ex))
                    _report.AddPassedTestCase(methodName, "Test Operation with wrong value returnd failed operation");
                else
                    _report.AddFailedTestCase(methodName, ex);
            }


        }

        #endregion


        #region --------------------------------- Set Operation ---------------------------------

        /// <summary>
        /// Assigns JObject to the Given attribute using SET in update Query
        /// </summary>
        public void SetOperationUsingJObject()
        {
            int updated = 0;
            cache.Clear();
            PopulateCache();
            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;

            try
            {
                var order = new Order { OrderID = 11, ShipCity = "Texas", ShipCountry = "US" };
                var jorder = JObject.Parse(JsonConvert.SerializeObject(order));

                string query = "Update Alachisoft.NCache.Sample.Data.Product Set Order = @MyOrder Where Id=?";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@MyOrder", jorder);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary<string, Exception> dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
                Console.WriteLine("RunTime " + elapsedTime);

                var testProperDeserialization = cache.Get<Product>("product1");
                var result = cache.Get<JsonObject>("product1");
                var Order = (JsonObject)result.GetAttributeValue("Order");
                var OrderCountry = Order.GetAttributeValue("ShipCity");

                if (OrderCountry.Value.ToString() == "Texas")
                {
                    _report.AddPassedTestCase(methodName, "Success: Partial Update, Set Operation using JObject");

                    testResults.Add(methodName, ResultStatus.Success);
                }
                else
                {
                    throw new Exception("Failure:Partial Update, Set Operation using JObject");
                }

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

                testResults.Add(methodName, ResultStatus.Failure);
            }


        }

        public void SetOperationUsingJObject0()
        {
            int updated = 0;
            cache.Clear();
            IList<string> keys = PopulateCacheAndGetKeys();

            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;

            try
            {

                ImageFormat ImageFormat = new ImageFormat();
                ImageFormat.Format = "UpdatedImageFormat";

                Image image = new Image();
                image.ImageFormats = new ImageFormat[2] { ImageFormat, ImageFormat };


                var jImage = JObject.Parse(JsonConvert.SerializeObject(image));

                string query = $"Update Alachisoft.NCache.Sample.Data.Product Set Images[*] = @Image ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@Image", jImage);

                IDictionary<string, Exception> dictionary;
                Stopwatch stopwatch = new Stopwatch();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                
                foreach (string key in keys)
                {
                    var testProperDeserialization = cache.Get<Product>(key);
                    var result = cache.Get<JsonObject>(key);
                    JObject jsonObject = JObject.Parse(result.ToString());

                    var imagesArray = (JArray)jsonObject["Images"]["$values"];

                    foreach (var imageToken in imagesArray)
                    {
                        JArray imageFormats = (JArray)imageToken["ImageFormats"]["$values"];

                        foreach (var imgFormat in imageFormats)
                        {

                            if (!String.Equals(ImageFormat.Format, imgFormat["Format"].ToString()))
                                throw new Exception("Failed to Replace all objects of array (Images[*] = )");

                        }

                    }
                }


                _report.AddPassedTestCase(methodName, "Set Operation to Replace all objects of array (Images[*] = )");



            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);


            }


        }

        public void SetOperationUsingJObject1()
        {
            int updated = 0;
            cache.Clear();
            IList<string> keys = PopulateCacheAndGetKeys();

            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;

            try
            {
                ImageFormat ImageFormat = new ImageFormat();
                ImageFormat.Format = "UpdatedImageFormat";

                var jImage = JObject.Parse(JsonConvert.SerializeObject(ImageFormat));

                string query = $"Update Alachisoft.NCache.Sample.Data.Product Set Images[*].ImageFormats[*] = @jImage ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@jImage", jImage);

                IDictionary<string, Exception> dictionary;
                Stopwatch stopwatch = new Stopwatch();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                
                foreach (string key in keys)
                {
                    var testProperDeserialization = cache.Get<Product>(key);
                    var result = cache.Get<JsonObject>(key);
                    JObject jsonObject = JObject.Parse(result.ToString());

                    var imagesArray = (JArray)jsonObject["Images"]["$values"];

                    foreach (var imageToken in imagesArray)
                    {
                        JArray imageFormats = (JArray)imageToken["ImageFormats"]["$values"];

                        foreach (var imgFormat in imageFormats)
                        {

                            if (!String.Equals(ImageFormat.Format, imgFormat["Format"].ToString()))
                                throw new Exception("Failed to Replace inner array with in all objects of array (Images[*].ImageFormats[*] = )");

                        }

                    }
                }


                _report.AddPassedTestCase(methodName, "Set Operation to Replace inner array with in all objects of array (Images[*].ImageFormats[*] = )");



            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);


            }


        }

        public void SetOperationWithReplacingJArrayWithJObject()
        {
            int updated = 0;
            cache.Clear();
            IList<string> keys = PopulateCacheAndGetKeys();

            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;

            try
            {
                FormatSubArray subArray = new FormatSubArray() { Name = "Updated SubArray" };
                ImageFormat ImageFormat = new ImageFormat();
                ImageFormat.Format = "UpdatedImageFormat";
                ImageFormat.formatSubArray = new FormatSubArray[2] { subArray, subArray };

                var jObect = JObject.Parse(JsonConvert.SerializeObject(subArray));

                string query = $"Update Alachisoft.NCache.Sample.Data.Product Set Images[*].ImageFormats[*].formatSubArray = @jObect ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@jObect", jObect);

                IDictionary<string, Exception> dictionary;
                Stopwatch stopwatch = new Stopwatch();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                
                var failException = new Exception("Set Operation has Replaced array structure  to object (Images[*].ImageFormats[*].formatSubArray = JObject)");

                // control should not reach here because i have destroyed the structure by adding jobject instead of jarray
                // and newtonsoft cannot deserialize it  if get API is used

                throw failException;

                foreach (string key in keys)
                {
                    var result = cache.Get<Product>(key);
                    _ = cache.Get<JsonObject>(key);

                    foreach (var image in result.Images)
                    {
                        if (image.ImageFormats.Length < 1)
                            throw failException;

                        foreach (var format in image.ImageFormats)
                        {
                            if (format.formatSubArray.Length < 1)
                                throw failException;

                            foreach (var formatSubArray in format.formatSubArray)
                            {
                                if (formatSubArray.Name == null || formatSubArray.Name != subArray.Name)
                                    throw failException;
                            }

                        }
                    }



                }


                _report.AddPassedTestCase(methodName, "Set Operation to Replace complex array structure (Images[*].ImageFormats[*].SubArray[*] = )");



            }
            catch (Exception ex)
            {
                // ToDo aqib ->  update it after decision 
                _report.AddFailedTestCase(methodName, ex);


            }


        }


        public void SetOperationUsingJObject2()
        {
            int updated = 0;
            cache.Clear();
            IList<string> keys = PopulateCacheAndGetKeys();

            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;

            try
            {
                FormatSubArray subArray = new FormatSubArray() { Name = "Updated SubArray" };
                ImageFormat ImageFormat = new ImageFormat();
                ImageFormat.Format = "UpdatedImageFormat";
                ImageFormat.formatSubArray = new FormatSubArray[2] { subArray, subArray };


                var jArray = JArray.Parse(JsonConvert.SerializeObject(ImageFormat.formatSubArray));

                string query = $"Update Alachisoft.NCache.Sample.Data.Product Set Images[*].ImageFormats[*].formatSubArray = @jArray ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@jArray", jArray);

                IDictionary<string, Exception> dictionary;
                Stopwatch stopwatch = new Stopwatch();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                
                var failException = new Exception("Set Operation failed to Replace complex array structure  (Images[*].ImageFormats[*].SubArray[*] = )");

                foreach (string key in keys)
                {
                    var result = cache.Get<Product>(key);
                    _ = cache.Get<JsonObject>(key);

                    foreach (var image in result.Images)
                    {
                        if (image.ImageFormats.Length < 1)
                            throw failException;

                        foreach (var format in image.ImageFormats)
                        {
                            if (format.formatSubArray.Length < 1)
                                throw failException;

                            foreach (var formatSubArray in format.formatSubArray)
                            {
                                if (formatSubArray.Name == null || formatSubArray.Name != subArray.Name)
                                    throw failException;
                            }

                        }
                    }



                }


                _report.AddPassedTestCase(methodName, "Set Operation to Replace complex array structure (Images[*].ImageFormats[*].SubArray[*] = )");



            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);


            }


        }

        public void SetOperationUsingJObject3()
        {
            int updated = 0;
            cache.Clear();
            IList<string> keys = PopulateCacheAndGetKeys();

            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;

            try
            {
                ImageFormat ImageFormat = new ImageFormat();
                ImageFormat.Format = "UpdatedImageFormat";

                var jImage = JObject.Parse(JsonConvert.SerializeObject(ImageFormat));

                int imageIndex = 1;

                string query = $"Update Alachisoft.NCache.Sample.Data.Product Set Images[{imageIndex}].ImageFormats[*] = @jImage ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@jImage", jImage);

                IDictionary<string, Exception> dictionary;
                Stopwatch stopwatch = new Stopwatch();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                
                foreach (string key in keys)
                {
                    var result = cache.Get<JsonObject>(key);
                    JObject jsonObject = JObject.Parse(result.ToString());

                    var imagesArray = (JArray)jsonObject["Images"]["$values"];

                    int i = 0;

                    foreach (var imageToken in imagesArray)
                    {
                        JArray imageFormats = (JArray)imageToken["ImageFormats"]["$values"];

                        foreach (var imgFormat in imageFormats)
                        {

                            if (!String.Equals(ImageFormat.Format, imgFormat["Format"].ToString()) && i == imageIndex)
                                throw new Exception("Failed to Replace inner array with in all objects of array (Images[0].ImageFormats[*] = )");


                            if (String.Equals(ImageFormat.Format, imgFormat["Format"].ToString()) && i != imageIndex)
                                throw new Exception("Set operation replaced at wrong index (Images[0].ImageFormats[*] = )");

                        }

                        i++;

                    }
                }


                _report.AddPassedTestCase(methodName, "Set Operation to Replace inner array with in all objects of array (Images[0].ImageFormats[*] = )");


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);


            }


        }


        public void SetOperationWithArrayUsingJObject()
        {
            int updated = 0;
            cache.Clear();
            PopulateCache(1);
            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;

            try
            {
                string ImageFileName = "UpdatedSkeleton";
                var image = new Image() { FileName = ImageFileName };

                var jImage = JObject.Parse(JsonConvert.SerializeObject(image));

                int indexToUpdate = 1;

                //string query = "Update Alachisoft.NCache.Sample.Data.Product Set Images = @jImage";
                string query = $"Update Alachisoft.NCache.Sample.Data.Product Set Images[{indexToUpdate}].FileName = @FileName ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@FileName", ImageFileName);
                queryCommand.Parameters.Add("@jImage", jImage);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary<string, Exception> dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
                Console.WriteLine("RunTime " + elapsedTime);

                
                var result = cache.Get<JsonObject>("product1");
                JObject jsonObject = JObject.Parse(result.ToString());

                var imagesArray = (JArray)jsonObject["Images"]["$values"];
                // var imagesArray = (JArray)jsonObject["Images"];

                int i = 0;

                foreach (var imageToken in imagesArray)
                {
                    string updatedFileName = imageToken["FileName"].Value<string>();

                    if (String.Equals(updatedFileName, ImageFileName) && i != indexToUpdate)
                        throw new Exception("Data updated at wrong index by Set Operation");
                    i++;

                }

                if (imagesArray[indexToUpdate]["FileName"].ToString() != ImageFileName)
                    throw new Exception("Data is not updated at specfied index by Set Operation");

                _report.AddPassedTestCase(methodName, "Set Operation With Array Using JObject");

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }

        public void SetOperationWithArrayWithinArrayUsingJObject()
        {
            int updated = 0;
            cache.Clear();
            IList<string> keys = PopulateCacheAndGetKeys();
            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;

            try
            {

                string ImageFormat = "UpdatedFormat";
                var image = new Image() { };

                var jImage = JObject.Parse(JsonConvert.SerializeObject(image));

                int updateIndex = 0;
                string query = $"Update Alachisoft.NCache.Sample.Data.Product Set Images[{updateIndex}].ImageFormats[{updateIndex}].Format = @ImageFormat ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@ImageFormat", ImageFormat);

                IDictionary<string, Exception> dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
                Console.WriteLine("RunTime " + elapsedTime);

                
                foreach (string key in keys)
                {
                    var result = cache.Get<JsonObject>(key);
                    JObject jsonObject = JObject.Parse(result.ToString());

                    var imagesArray = (JArray)jsonObject["Images"]["$values"];


                    JToken imageToken = imagesArray[updateIndex];

                    JArray imageFormats = (JArray)imageToken["ImageFormats"]["$values"];
                    int i = 0;
                    foreach (var imgFormat in imageFormats)
                    {
                        if (!String.Equals(ImageFormat, imgFormat["Format"].ToString()) && i == updateIndex)
                            throw new Exception("Data not updated at specified index by Set Operation for array within array");

                        if (String.Equals(ImageFormat, imgFormat["Format"].ToString()) && i != updateIndex)
                            throw new Exception("Data updated at wrong index by Set Operation for array within array");

                        i++;

                    }

                }


                _report.AddPassedTestCase(methodName, "Set Operation With Array Using JObject");

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }

        public void SetOperationWithAesterickArrayUsingJObject()
        {
            int updated = 0;
            cache.Clear();
            IList<string> keys = PopulateCacheAndGetKeys();
            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;

            try
            {
                string ImageFileName = "UpdatedSkeleton";
                var image = new Image() { FileName = ImageFileName };

                var jImage = JObject.Parse(JsonConvert.SerializeObject(image));

                string query = $"Update Alachisoft.NCache.Sample.Data.Product Set Images[*].FileName = @FileName ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@FileName", ImageFileName);
                queryCommand.Parameters.Add("@jImage", jImage);

                IDictionary<string, Exception> dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
                Console.WriteLine("RunTime " + elapsedTime);

                
                foreach (string key in keys)
                {
                    var result = cache.Get<JsonObject>(key);
                    JObject jsonObject = JObject.Parse(result.ToString());

                    var imagesArray = (JArray)jsonObject["Images"]["$values"];

                    foreach (var imageToken in imagesArray)
                    {
                        string updatedFileName = imageToken["FileName"].Value<string>();

                        if (!String.Equals(updatedFileName, ImageFileName))
                            throw new Exception("Data updated at wrong index by Set Operation with *");

                    }
                }


                _report.AddPassedTestCase(methodName, "Set Operation With Array Using JObject");

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }

        public void SetOperationWithAesterickForArrayWithinArrayUsingJObject()
        {
            int updated = 0;
            cache.Clear();
            IList<string> keys = PopulateCacheAndGetKeys();
            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;

            try
            {
                string ImageFormat = "UpdatedFormat";
                var image = new Image() { };

                var jImage = JObject.Parse(JsonConvert.SerializeObject(image));

                string query = $"Update Alachisoft.NCache.Sample.Data.Product Set Images[*].ImageFormats[*].Format = @ImageFormat ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@ImageFormat", ImageFormat);

                IDictionary<string, Exception> dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
                Console.WriteLine("RunTime " + elapsedTime);

                
                foreach (string key in keys)
                {
                    var result = cache.Get<JsonObject>(key);
                    JObject jsonObject = JObject.Parse(result.ToString());

                    var imagesArray = (JArray)jsonObject["Images"]["$values"];

                    foreach (var imageToken in imagesArray)
                    {
                        JArray imageFormats = (JArray)imageToken["ImageFormats"]["$values"];

                        foreach (var imgFormat in imageFormats)
                        {
                            if (!String.Equals(ImageFormat, imgFormat["Format"].ToString()))
                                throw new Exception("Data not updated at specified index by Set Operation with * for array within array");

                        }
                    }
                }


                _report.AddPassedTestCase(methodName, "Set Operation With Array Using JObject");

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }

        public void SetOperationWithAesterickForArrayWithinArrayUsingJObject1()
        {
            int updated = 0;
            cache.Clear();
            IList<string> keys = PopulateCacheAndGetKeys();

            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;

            try
            {
                string ImageFormat = "UpdatedFormat";
                var image = new Image() { };

                int imageIndex = 1;

                var jImage = JObject.Parse(JsonConvert.SerializeObject(image));

                string query = $"Update Alachisoft.NCache.Sample.Data.Product Set Images[{imageIndex}].ImageFormats[*].Format = @ImageFormat ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@ImageFormat", ImageFormat);

                IDictionary<string, Exception> dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
                Console.WriteLine("RunTime " + elapsedTime);

                
                foreach (string key in keys)
                {
                    var result = cache.Get<JsonObject>(key);
                    JObject jsonObject = JObject.Parse(result.ToString());

                    var imagesArray = (JArray)jsonObject["Images"]["$values"];
                    int i = 0;
                    foreach (var imageToken in imagesArray)
                    {
                        JArray imageFormats = (JArray)imageToken["ImageFormats"]["$values"];

                        foreach (var imgFormat in imageFormats)
                        {
                            if (!String.Equals(ImageFormat, imgFormat["Format"].ToString()) && i == imageIndex)
                                throw new Exception("Data not updated at specified index by Set Operation with * for array within array 1");

                            if (String.Equals(ImageFormat, imgFormat["Format"].ToString()) && i != imageIndex)
                                throw new Exception("Data  updated at qrong index by Set Operation with * for array within array 1");

                        }
                        i++;
                    }
                }


                _report.AddPassedTestCase(methodName, "Set Operation With Array Using JObject");

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }

        public void SetOperationWithAesterickForArrayWithinArrayUsingJObject2()
        {
            int updated = 0;
            cache.Clear();
            IList<string> keys = PopulateCacheAndGetKeys();

            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;

            try
            {
                string ImageFormat = "UpdatedFormat";
                var image = new Image() { };

                int formatIndex = 1;

                var jImage = JObject.Parse(JsonConvert.SerializeObject(image));

                string query = $"Update Alachisoft.NCache.Sample.Data.Product Set Images[*].ImageFormats[{formatIndex}].Format = @ImageFormat ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@ImageFormat", ImageFormat);

                IDictionary<string, Exception> dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
                Console.WriteLine("RunTime " + elapsedTime);

                
                foreach (string key in keys)
                {
                    var result = cache.Get<JsonObject>(key);
                    JObject jsonObject = JObject.Parse(result.ToString());

                    var imagesArray = (JArray)jsonObject["Images"]["$values"];

                    foreach (var imageToken in imagesArray)
                    {
                        JArray imageFormats = (JArray)imageToken["ImageFormats"]["$values"];
                        int i = 0;
                        foreach (var imgFormat in imageFormats)
                        {

                            if (!String.Equals(ImageFormat, imgFormat["Format"].ToString()) && i == formatIndex)
                                throw new Exception("Data not updated at specified index by Set Operation with * for array within array 1");

                            if (String.Equals(ImageFormat, imgFormat["Format"].ToString()) && i != formatIndex)
                                throw new Exception("Data  updated at qrong index by Set Operation with * for array within array 1");
                            i++;
                        }

                    }
                }


                _report.AddPassedTestCase(methodName, "Set Operation With Array Using JObject");

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }

        public void SetOperationOnAttributeThatDoesnotExist()
        {
            int updated = 0;
            cache.Clear();
            IList<string> keys = PopulateCacheAndGetKeys();

            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;

            try
            {
                string ImageFormat = "InvalidFormat";
                var image = new Image() { };

                int formatIndex = 1;

                var jImage = JObject.Parse(JsonConvert.SerializeObject(image));

                string query = $"Update Alachisoft.NCache.Sample.Data.Product Set Images[*].AttributeThatDoesnotExist = @ImageFormat ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@ImageFormat", ImageFormat);

                IDictionary<string, Exception> dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
                Console.WriteLine("RunTime " + elapsedTime);

                
                // Contrtol should not reach here

                _report.AddFailedTestCase(methodName, new Exception("Expected a Target Not Found Exception, but didn't get it."));


            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Unable to find target location at the specified segment"))
                    _report.AddPassedTestCase(methodName, $"Got Target not found exception:");
                else
                    _report.AddFailedTestCase(methodName, new Exception($"Expected a Target Not Found Exception, but get {ex.Message}"));

            }


        }

        public void SetOperationWithWrongArrayJObject()
        {
            int updated = 0;
            cache.Clear();
            IList<string> keys = PopulateCacheAndGetKeys();

            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;

            try
            {
                string ImageFormat = "InvalidFormat";
                var image = new Image() { };

                int formatIndex = 1;

                var jImage = JObject.Parse(JsonConvert.SerializeObject(image));

                string query = $"Update Alachisoft.NCache.Sample.Data.Product Set Images[*].ImageFormats[*].InvalidFormat = @ImageFormat ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@ImageFormat", ImageFormat);

                IDictionary<string, Exception> dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
                Console.WriteLine("RunTime " + elapsedTime);

                
                // Contrtol should not reach here

                _report.AddFailedTestCase(methodName, new Exception("Expected a Target Not Found Exception, but didn't get it."));


            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Unable to find target location at the specified segment"))
                    _report.AddPassedTestCase(methodName, $"Got Target not found exception:");
                else
                    _report.AddFailedTestCase(methodName, new Exception($"Expected a Target Not Found Exception, but get {ex.Message}"));

            }


        }

        public void SetOperationWithWrongArrayJObject1()
        {
            int updated = 0;
            cache.Clear();
            IList<string> keys = PopulateCacheAndGetKeys();

            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;

            try
            {
                string ImageFormat = "InvalidFormat";
                var image = new Image() { };

                int formatIndex = 1;

                var jImage = JObject.Parse(JsonConvert.SerializeObject(image));

                string query = $"Update Alachisoft.NCache.Sample.Data.Product Set Images[*].ImageFormatsj[0].InvalidFormat = @ImageFormat ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@ImageFormat", ImageFormat);

                IDictionary<string, Exception> dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
                Console.WriteLine("RunTime " + elapsedTime);

                
                // Contrtol should not reach here

                _report.AddFailedTestCase(methodName, new Exception("Expected a Target Not Found Exception, but didn't get it."));


            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Unable to find target location at the specified segment"))
                    _report.AddPassedTestCase(methodName, $"Got Target not found exception");
                else
                    _report.AddFailedTestCase(methodName, new Exception($"Expected a Target Not Found Exception, but get {ex.Message}"));


            }


        }

        public void SetOperationWithWrongArrayJObject2()
        {
            int updated = 0;
            cache.Clear();
            IList<string> keys = PopulateCacheAndGetKeys();

            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;

            try
            {
                string ImageFormat = "InvalidFormat";
                var image = new Image() { };

                int formatIndex = 1;

                var jImage = JObject.Parse(JsonConvert.SerializeObject(image));

                string query = $"Update Alachisoft.NCache.Sample.Data.Product Set Images[*].ImageFormats[F].Format = @ImageFormat ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@ImageFormat", ImageFormat);

                IDictionary<string, Exception> dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
                Console.WriteLine("RunTime " + elapsedTime);

                
                // Contrtol should not reach here

                _report.AddFailedTestCase(methodName, new Exception("Expected a Target Not Found Exception, but didn't get it."));


            }
            catch (Exception ex)
            {
                _report.AddPassedTestCase(methodName, $"Got Exception: {ex.Message.Split("\n")[0]}");

            }


        }

        public void SetOperationWithWrongArrayJObject3()
        {
            int updated = 0;
            cache.Clear();
            IList<string> keys = PopulateCacheAndGetKeys();

            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;

            try
            {
                string ImageFormat = "InvalidFormat";
                var image = new Image() { };

                int formatIndex = 1;

                var jImage = JObject.Parse(JsonConvert.SerializeObject(image));

                string query = $"Update Alachisoft.NCache.Sample.Data.Product Set Images[-1].ImageFormats[0].Format = @ImageFormat ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@ImageFormat", ImageFormat);

                IDictionary<string, Exception> dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
                Console.WriteLine("RunTime " + elapsedTime);

                
                // Contrtol should not reach here

                _report.AddFailedTestCase(methodName, new Exception("Expected a Target Not Found Exception, but didn't get it."));


            }
            catch (Exception ex)
            {
                _report.AddPassedTestCase(methodName, $"Got Exception: {ex.Message.Split("\n")[0]}");

            }


        }

        public void SetArrayAtZerothIndex()
        {
            cache.Clear();
            string methodName = MethodBase.GetCurrentMethod().Name;

            var keys = PopulateCacheAndGetKeys();
            string description = "Set array at 0 index of array";

            try
            {
                PopulateCache();

                const int arraySize = 2;

                var order = new Order { OrderID = 10, ShipCity = "rawalpindi\\Islamabad", ShipCountry = "Pakistan\\ASia" };
                var orderArray = new Order[arraySize] { order, order };

                var jArray = JObject.Parse(JsonConvert.SerializeObject(order));


                string query = "Update Alachisoft.NCache.Sample.Data.Product Set this.Images[0] = @jArray ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@jArray", jArray);

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                
                var exception = new Exception(description);

                foreach (string key in keys)
                {
                    var jsonObject = cache.Get<JObject>(key);
                    //JObject jsonObject = JObject.Parse(result.ToString());

                    var imagesArray = (JArray)jsonObject["Images"]["$values"];

                    // verify 1st index is still image. It should not be order 
                    var imageObj = JsonConvert.DeserializeObject<Image>(imagesArray[1].ToString());

                    var updatedOrder = JsonConvert.DeserializeObject<Order[]>(imagesArray[0].ToString());

                    if (updatedOrder.Length != arraySize)
                        throw exception;

                    foreach (var item in updatedOrder)
                    {
                        if (!item.Equals(order))
                            throw exception;
                    }


                }

                

                _report.AddPassedTestCase(methodName, description);

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }

        }

        public void SetArrayAtLastIndex()
        {
            cache.Clear();
            string methodName = MethodBase.GetCurrentMethod().Name;

            var keys = PopulateCacheAndGetKeys();
            string description = "Set array at last index of array";

            try
            {
                PopulateCache();

                const int arraySize = 2;

                var order = new Order { OrderID = 10, ShipCity = "rawalpindi\\Islamabad", ShipCountry = "Pakistan\\ASia" };
                var orderArray = new Order[arraySize] { order, order };

                var jArray = JArray.Parse(JsonConvert.SerializeObject(orderArray));


                string query = "Update Alachisoft.NCache.Sample.Data.Product Set this.Images[2] = @jArray ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@jArray", jArray);

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                
                var exception = new Exception(description);

                foreach (string key in keys)
                {
                    var jsonObject = cache.Get<JObject>(key);
                    //JObject jsonObject = JObject.Parse(result.ToString());

                    var imagesArray = (JArray)jsonObject["Images"]["$values"];

                    // verify 1st index is still image. It should not be order 
                    var imageObj = JsonConvert.DeserializeObject<Image>(imagesArray[1].ToString());

                    var updatedOrder = JsonConvert.DeserializeObject<Order[]>(imagesArray[2].ToString());

                    if (updatedOrder.Length != arraySize)
                        throw exception;

                    foreach (var item in updatedOrder)
                    {
                        if (!item.Equals(order))
                            throw exception;
                    }


                }

                

                _report.AddPassedTestCase(methodName, description);

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }

        }


        public void SetArrayAtOutOBoundIndex()
        {
            cache.Clear();
            string methodName = MethodBase.GetCurrentMethod().Name;

            var keys = PopulateCacheAndGetKeys();
            string description = "Set array at OutOfBound index of array";

            try
            {
                PopulateCache();

                const int arraySize = 2;

                var order = new Order { OrderID = 10, ShipCity = "rawalpindi\\Islamabad", ShipCountry = "Pakistan\\ASia" };
                var orderArray = new Order[arraySize] { order, order };

                var jArray = JArray.Parse(JsonConvert.SerializeObject(orderArray));


                string query = "Update Alachisoft.NCache.Sample.Data.Product Set this.Images[10] = @jArray ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@jArray", jArray);

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                

                _report.AddFailedTestCase(methodName, new Exception(description));

            }
            catch (Exception ex)
            {
                if (Helper.IsIndexOutOfBoundException(ex))
                    _report.AddPassedTestCase(methodName, description);
                else
                    _report.AddFailedTestCase(methodName, ex);

            }

        }

       

        //public void VerifyExceptionIsThrownIfKeyNotExistForUpdate()
        //{
        //    int updated = 0;
        //    cache.Clear();
        //    PopulateCache();
        //    string methodName = MethodBase.GetCurrentMethod().Name;
        //    count++;
        //    string description = "Get exception in update query if key not exist";
        //    try
        //    {

        //        string query = "Update  Alachisoft.NCache.Sample.Data.Product Set Order.ShipCity = '\"abcd\"' test Category = '\"meat\"' where Name = ?";
        //        QueryCommand queryCommand = new QueryCommand(query);
        //        queryCommand.Parameters.Add("Name", Guid.NewGuid().ToString());
        //        IDictionary<string, Exception> dictionary;
        //        Stopwatch stopwatch = new Stopwatch();
        //        stopwatch.Start();

        //        updated = cache.SearchService.ExecuteNonQuery(queryCommand);


        //        stopwatch.Stop();
        //        TimeSpan ts = stopwatch.Elapsed;
        //        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
        //        ts.Hours, ts.Minutes, ts.Seconds,
        //         ts.Milliseconds / 10);
        //        Console.WriteLine("RunTime " + elapsedTime);

        //        //Control should not reach here
        //        _report.AddFailedTestCase(methodName,new Exception(description));
        //    }
        //    catch (Exception ex)
        //    {
        //        _report.AddPassedTestCase(methodName, $"Got Exception : {ex.Message}");
        //    }

        //}


        //Test partial operation on arrays in inner level

        public void BasicUpdateQuery10()
        {

            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;
            try
            {
                productList.Clear();

                cache.Clear();
                var customer = new string[3] { "premium", "gold", "silver" };
                string key = "Products";
                var item = new Product() { Id = 4, Name = "IKura", ClassName = "Electronics", Category = "Produce", UnitPrice = 50, Customer = new Customer() { CustomerType = customer } };
                var cacheItem = new CacheItem(item);

                cache.Insert(key, cacheItem);

                string query = "Update Alachisoft.NCache.Sample.Data.Product Set Customer.CustomerType[2] = @customertype ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@customertype", "old");
                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);


                var received = cache.Get<Product>(key);

                if (received.Customer.CustomerType[2] == "old")
                {
                    _report.AddPassedTestCase(methodName, "Success:Test partial operation on arrays in inner level");
                }
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }
        }

        //Test partial operation on arrays in inner level with this
        public void BasicUpdateQuery11()
        {

            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;
            try
            {
                productList.Clear();

                cache.Clear();
                var customer = new string[3] { "premium", "gold", "silver" };
                string key = "Products";
                var item = new Product() { Id = 4, Name = "IKura", ClassName = "Electronics", Category = "Produce", UnitPrice = 50, Customer = new Customer() { CustomerType = customer } };
                var cacheItem = new CacheItem(item);

                cache.Insert(key, cacheItem);

                string query = "Update Alachisoft.NCache.Sample.Data.Product Set this.Customer.CustomerType[2] = @customertype ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@customertype", "old");
                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);


                var received = cache.Get<Product>(key);

                if (received.Customer.CustomerType[2] == "old")
                {
                    _report.AddPassedTestCase(methodName, "Success:Test partial operation on arrays in inner level with this");
                }
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
                Console.WriteLine("Failure:Test partial operation on arrays in inner level with this");
            }
        }

        // Partial Update items using query result will apply on some but will be failed on others ...
        //Verify new behaviour that exception is thrown on first exception.
        public void BasicUpdateQuery13()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;
            try
            {
                cache.Clear();
                productList.Clear();
                PopulateProductList();
                PopulateCache();
                string query = "Update  Alachisoft.NCache.Sample.Data.Product Set this.Name = '\"Tea\"', Order.ShipCity = '\"Lahore\"' where UnitPrice > ?";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("UnitPrice", Convert.ToDecimal(5));
                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                //  _report.AddFailedTestCase(methodName, new Exception("Failure:Verify new behaviour that exception is thrown on first exception."));
                //   IDK why it should fail here

                _report.AddPassedTestCase(methodName, "Success:Verify new behaviour that exception is thrown on first exception.");


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);


            }
        }

        //Test the support of $value$
        public void BasicUpdateQuery14()
        {

            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;
            try
            {
                productList.Clear();

                cache.Clear();
                var customer = new string[3] { "premium", "gold", "silver" };
                string key = "Products";
                var item = new Product() { Id = 4, Name = "IKura", ClassName = "Electronics", Category = "Produce", UnitPrice = 50, Customer = new Customer() { CustomerType = customer } };
                var cacheItem = new CacheItem(item);

                cache.Insert(key, cacheItem);

                string query = "Update Alachisoft.NCache.Sample.Data.Product Set $value$ = @customertype ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@customertype", "old");
                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);


                var received = cache.Get<string>(key);

                if (received.ToString() == "old")
                {
                    _report.AddPassedTestCase(methodName, "Success:Test the support of $value$");
                }
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
                Console.WriteLine("Failure:Test the support of $value$");
            }
        }

        // verify on basic int 

        public void BasicUpdateQuery16()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;
            try
            {
                cache.Clear();

                string group = "basic-integers";

                for (int i = 0; i < 100; i++)
                {
                    var cacheItem = new CacheItem(5);
                    cacheItem.Group = group;
                    cache.Insert("group:" + i, cacheItem);
                }


                var kletys = cache.SearchService.GetGroupKeys(group);

                string query = "Update System.Int32 Set $value$ = 1000 where $Group$ = ?";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("$Group$", group);
                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);
                int correctResponse = 0;
                if (updated == 100)
                {
                    for (int i = 0; i < 100; i++)
                    {
                        var result = cache.Get<object>("group:" + i);
                        if (Convert.ToInt32(result) == 1000)
                        {
                            correctResponse++;
                        }

                    }

                }

                if (correctResponse == updated)
                {
                    _report.AddPassedTestCase(methodName, "Success: verify on basic int ");
                }
                else
                {
                    throw new Exception("Failure: verify on basic int ");
                }
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }
        }

        //verify date is updated successfully 

        public void BasicUpdateQuery19()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;
            try
            {
                // productList.Add(new Product() { Id = 1, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 357, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });
                int received = 0;
                cache.Clear();
                productList.Clear();
                PopulateProductList();
                PopulateCache();
                var date = new DateTime(2022, 12, 6);
                string query = "Update  Alachisoft.NCache.Sample.Data.Product Set this.Time = @date where Category = @beverages";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@tea", "Tea");
                queryCommand.Parameters.Add("@important", "Important");
                queryCommand.Parameters.Add("@lahore", "Lahore");
                queryCommand.Parameters.Add("@date", date);
                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                foreach (var item in productList)
                {

                    var prod = cache.Get<Product>("product" + item.Id);
                    if (item.Category == "Beverages")
                    {
                        if (prod.Time != date)
                        {
                            _report.AddFailedTestCase(methodName, new Exception("Failure: verify date is updated successfully  "));
                        }
                        else
                        {
                            received++;
                        }
                    }
                }
                if (received == updated)
                {
                    _report.AddPassedTestCase(methodName, "Success: verify date is updated successfully  ");
                }

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
                Console.WriteLine("Failure: verify order is mantained or not ");
            }
        }


        #endregion


        #region --------------------------------- Add Operations ---------------------------------


        public void BasicUpdateQuery22()
        {
            cache.Clear();
            string methodName = MethodBase.GetCurrentMethod().Name;

            try
            {
                PopulateCache();
                string query = "Update Alachisoft.NCache.Sample.Data.Product Add this.Name123 = '[{\"name\":\"phone\",\"model\":\"p30PRO\"}]' where  Category = @beverages";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);
                foreach (var item in productList)
                {
                    if (item.Category == "Beverages")
                    {
                        var ret = cache.Get<Product>("product" + item.Id);
                    }
                }

                //ToDo add verification
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
                throw;
            }

        }

        public void AddThrowException()
        {
            cache.Clear();
            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "Add should throw exception if used as set";

            try
            {
                PopulateCache();

                var order = new Order { OrderID = 10, ShipCity = "rawalpindi", ShipCountry = "Pakistan" };
                var jorder = JObject.Parse(JsonConvert.SerializeObject(order));


                string query = "Update Alachisoft.NCache.Sample.Data.Product Add this.Order = this.Order.OrderID ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@jOrder", jorder);

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                _report.AddFailedTestCase(methodName, new Exception(description));

            }
            catch (Exception ex)
            {
                if (Helper.IsIncorrectFormatException(ex))
                    _report.AddPassedTestCase(methodName, description);
                else
                    _report.AddFailedTestCase(methodName, ex);
            }

        }

        public void AddOrderInProduct()
        {
            cache.Clear();
            string methodName = MethodBase.GetCurrentMethod().Name;

            var keys = PopulateCacheAndGetKeys();

            try
            {
                PopulateCache();

                var order = new Order { OrderID = 10, ShipCity = "rawalpindi\\Islamabad", ShipCountry = "Pakistan\\ASia" };
                var jorder = JObject.Parse(JsonConvert.SerializeObject(order));


                string query = "Update Alachisoft.NCache.Sample.Data.Product Add this.Order = @jOrder ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@jOrder", jorder);

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                
                foreach (var key in keys)
                {
                    _ = cache.Get<JsonObject>(key);
                    var prod = cache.Get<Product>(key);

                    if (!prod.Order.Equals(order))
                        throw new Exception("Unable to add order in prodct");
                }

                _report.AddPassedTestCase(methodName, "Add order in product");


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }

        }

        public void AddArrayInProduct()
        {
            cache.Clear();
            string methodName = MethodBase.GetCurrentMethod().Name;

            var keys = PopulateCacheAndGetKeys();

            try
            {
                PopulateCache();

                const int arraySize = 2;

                var order = new Order { OrderID = 10, ShipCity = "rawalpindi\\Islamabad", ShipCountry = "Pakistan\\ASia" };
                var orderArray = new Order[arraySize] { order, order };

                var jArray = JArray.Parse(JsonConvert.SerializeObject(orderArray));


                string query = "Update Alachisoft.NCache.Sample.Data.Product Add this.Order = @jOrder ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@jOrder", jArray);

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                
                foreach (var key in keys)
                {
                    var jsonObj = cache.Get<JsonObject>(key);
                    var jObj = cache.Get<JObject>(key);

                    var array = jObj["Order"] as JArray;

                    if (array.Count != arraySize)
                        throw new Exception("Unable to add array");

                    // var prod = cache.Get<Product>(key);


                }

                _report.AddPassedTestCase(methodName, "Add order in product");


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }

        }

        public void AddObjectAtIndexOfNonExistingIndexInProduct()
        {
            cache.Clear();
            string methodName = MethodBase.GetCurrentMethod().Name;

            var keys = PopulateCacheAndGetKeys();
            string description = "Add element at specific index of array that and the array doesnot exist";

            try
            {
                PopulateCache();

                const int arraySize = 2;

                var order = new Order { OrderID = 10, ShipCity = "rawalpindi\\Islamabad", ShipCountry = "Pakistan\\ASia" };
                var orderArray = new Order[arraySize] { order, order };

                var jArray = JArray.Parse(JsonConvert.SerializeObject(orderArray));


                string query = "Update Alachisoft.NCache.Sample.Data.Product Add this.Order1[0] = @jOrder ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@jOrder", jArray);

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                

                _report.AddFailedTestCase(methodName, new Exception(description));

            }
            catch (Exception ex)
            {
                if (Helper.IsTargetNotFoundException(ex))
                    _report.AddPassedTestCase(methodName, description);
                else
                    _report.AddFailedTestCase(methodName, ex);

            }

        }

        public void AddArrayAtOutOBoundIndex()
        {
            cache.Clear();
            string methodName = MethodBase.GetCurrentMethod().Name;

            var keys = PopulateCacheAndGetKeys();
            string description = "Add array at OutOfBound index of array";

            try
            {
                PopulateCache();

                const int arraySize = 2;

                var order = new Order { OrderID = 10, ShipCity = "rawalpindi\\Islamabad", ShipCountry = "Pakistan\\ASia" };
                var orderArray = new Order[arraySize] { order, order };

                var jArray = JArray.Parse(JsonConvert.SerializeObject(orderArray));


                string query = "Update Alachisoft.NCache.Sample.Data.Product Add this.Images[10] = @jArray ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@jArray", jArray);

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                

                _report.AddFailedTestCase(methodName, new Exception(description));

            }
            catch (Exception ex)
            {
                if (Helper.IsIndexOutOfBoundException(ex))
                    _report.AddPassedTestCase(methodName, description);
                else
                    _report.AddFailedTestCase(methodName, ex);

            }

        }


        public void AddArrayAtSpecificIndex()
        {
            cache.Clear();
            string methodName = MethodBase.GetCurrentMethod().Name;

            var keys = PopulateCacheAndGetKeys();
            string description = "Add array in object";

            try
            {
                PopulateCache();

                const int arraySize = 2;

                var order = new Order { OrderID = 10, ShipCity = "rawalpindi\\Islamabad", ShipCountry = "Pakistan\\ASia" };
                var orderArray = new Order[arraySize] { order, order };

                var jArray = JArray.Parse(JsonConvert.SerializeObject(orderArray));


                string query = "Update Alachisoft.NCache.Sample.Data.Product Add this.Images[0] = @jArray ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@jArray", jArray);

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                

                // ToDo Validataion after discussion with sir taimoor

                var exception = new Exception(description);

                foreach (string key in keys)
                {
                    var jsonObject = cache.Get<JObject>(key);
                    //JObject jsonObject = JObject.Parse(result.ToString());

                    var imagesArray = (JArray)jsonObject["Images"]["$values"];

                    // verify 1st index is still image. It should not be order 
                    var imageObj = JsonConvert.DeserializeObject<Image>(imagesArray[1].ToString());

                    var updatedOrder = JsonConvert.DeserializeObject<Order[]>(imagesArray[0].ToString());

                    if (updatedOrder.Length != arraySize)
                        throw exception;

                    foreach (var item in updatedOrder)
                    {
                        if (!item.Equals(order))
                            throw exception;
                    }


                }

                

                _report.AddPassedTestCase(methodName, description);


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }

        }

        #endregion


        #region --------------------------------- Remove Operations ---------------------------------

        public void RemoveObjectFromJsonString()
        {

            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;
            try
            {

                cache.Clear();

                PopulateCache();
                string description = "Remove Order object from json string ";


                string query = "Update  Alachisoft.NCache.Sample.Data.Product Remove this.Order ";
                QueryCommand queryCommand = new QueryCommand(query);
                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                foreach (var item in productList)
                {
                    var entry = cache.Get<Product>("product" + item.Id);

                    if (entry.Order != null)
                        throw new Exception(description);


                    var jObject = cache.Get<JObject>("product" + item.Id);

                    if (jObject.TryGetValue("Order", out _))
                        throw new Exception(description);

                }

                _report.AddPassedTestCase(methodName, description);

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }

        }

        public void RemoveOrderIdFromJsonString()
        {

            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;
            try
            {

                cache.Clear();

                PopulateCache();
                string description = "Remove OrderId from json string ";


                string query = "Update  Alachisoft.NCache.Sample.Data.Product Remove this.Order.OrderID ";
                QueryCommand queryCommand = new QueryCommand(query);
                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                foreach (var item in productList)
                {
                    var entry = cache.Get<Product>("product" + item.Id);

                    if (entry.Order.OrderID != default)
                        throw new Exception(description);


                    var jObject = cache.Get<JObject>("product" + item.Id);

                    if (jObject.TryGetValue("Order", out var order))
                    {
                        if (order.Contains("OrderID"))
                            throw new Exception(description);

                    }
                    else
                        throw new Exception(description);

                }

                _report.AddPassedTestCase(methodName, description);

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }

        }

        public void RemoveValueThatDoesnotExist()
        {

            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "Remove value taht doesnot exist from json string ";

            count++;
            try
            {

                cache.Clear();

                PopulateCache();


                string query = "Update  Alachisoft.NCache.Sample.Data.Product Remove this.Order.ValueThatDoesnotExist ";
                QueryCommand queryCommand = new QueryCommand(query);
                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                
                _report.AddFailedTestCase(methodName, new Exception(description));


            }
            catch (Exception ex)
            {
                if (Helper.IsTargetNotFoundException(ex))
                    _report.AddPassedTestCase(methodName, description);
                else
                    _report.AddFailedTestCase(methodName, ex);

            }

        }


        #endregion


        #region --------------------------------- Combination of Partial Operations ---------------------------------

        public void TestOperationUsingJObject2()
        {
            int updated = 0;
            cache.Clear();
            PopulateCache();
            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;
            try
            {

                var order = new Order { OrderID = 10, ShipCity = "rawalpindi", ShipCountry = "Pakistan" };
                var jorder = JObject.Parse(JsonConvert.SerializeObject(order));

                string query = "Update Alachisoft.NCache.Sample.Data.Product Remove Order.ShipCity Test Order = @ProductName Where Id=?";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@ProductName", jorder);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary<string, Exception> dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                

                _report.AddFailedTestCase(methodName, new Exception("Test Object with wrong values didn't throw exception"));



            }
            catch (Exception ex)
            {
                if (Helper.IsTestOperationException(ex))
                    _report.AddPassedTestCase(methodName, "Test Operation with wrong value returnd failed operation");
                else
                    _report.AddFailedTestCase(methodName, ex);
            }


        }

        public void BasicUpdateQuery0()
        {
            int updated = 0;
            cache.Clear();
            PopulateCache();
            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;
            try
            {

                string query = "Update  Alachisoft.NCache.Sample.Data.Product Set Order.ShipCity = '\"abcd\"' test Category = '\"meat\"' where UnitPrice > ?";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("UnitPrice", Convert.ToDecimal(100));
                IDictionary<string, Exception> dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                var result = cache.SearchService.ExecuteNonQuery(queryCommand);
                //dictionary = result.FailedOperations;
                //foreach (var val in dictionary)
                //{
                //    string ex = val.Value.ToString();
                //    string key = val.Key.ToString();
                //    if (ex.Contains("Specified value not equals to test value"))
                //    {
                //        var prod = cache.Get<Product>(key);
                //        if (prod != null && prod.UnitPrice > 100 && prod.Category != "meat")
                //            continue;

                //    }
                //    throw new Exception("test case failed");

                //}
                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                 ts.Milliseconds / 10);
                Console.WriteLine("RunTime " + elapsedTime);

                _report.AddPassedTestCase(methodName, "Success:Partial Update items using query");
                testResults.Add(methodName, ResultStatus.Success);
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }

        }
        // Partial Update items using query
        public void BasicUpdateQuery1()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;
            try
            {
                cache.Clear();
                PopulateCache();
                string query = "Update  Alachisoft.NCache.Sample.Data.Product Set Name = '\"Tea\"', Order.ShipCity = '\"Lahore\"' Add Order.Type = '\"Important\"'  where Category = ?";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Category", "Beverages");
                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);
                foreach (var item in productList)
                {
                    var prod = cache.Get<Product>("product" + item.Id);
                    if (item.Category == "Beverages" && prod.Name != "Tea" && prod.Order.ShipCity != "Lahore")
                    {
                        Console.WriteLine("Failure:Partial Update items using query");
                        testResults.Add(methodName, ResultStatus.Failure);
                    }
                }
                _report.AddPassedTestCase(methodName, "Success:Partial Update items using query");
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
                Console.WriteLine("Failure:Partial Update items using query");
            }
        }

        // Partial Update items using query where one partial operation should fail
        public void BasicUpdateQuery2()
        {
            string methodName = MethodBase.GetCurrentMethod().Name; count++;
            try
            {
                cache.Clear();
                PopulateCache();
                string query = "Update Alachisoft.NCache.Sample.Data.Product Set Name.Class = '\"Tea\"', Order.ShipCity = '\"Lahore\"' Add Order.Type = '\"Important\"'  where Category = ?";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Category", "Beverages");
                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);
                foreach (var item in productList)
                {

                    var prod = cache.Get<Product>("product" + item.Id);
                    if (item.Category == "Beverages" && prod.Name == "Tea" && prod.Order.ShipCity == "Lahore")
                    {
                        throw new Exception("Failure:Partial Update items using query");
                    }
                }
                _report.AddPassedTestCase(methodName, "Success:Partial Update items using query");
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
                Console.WriteLine("Success:Partial Update items using query");
            }
        }
        //partial operations using named params
        //case sensitivity is an issue
        public void BasicUpdateQuery3()
        {
            string methodName = MethodBase.GetCurrentMethod().Name; count++;
            try
            {
                cache.Clear();
                PopulateCache();
                string query = "Update  Alachisoft.NCache.Sample.Data.Product Set Name = @tea,Order.ShipCity = @lahore Add Order.Type = @important where Category = @beverages";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@tea", "Tea");
                queryCommand.Parameters.Add("@important", "Important");
                queryCommand.Parameters.Add("@lahore", "Lahore");
                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);
                foreach (var item in productList)
                {

                    var prod = cache.Get<Product>("product" + item.Id);
                    if (item.Category == "Beverages" && prod.Name != "Tea" && prod.Order.ShipCity != "Lahore")
                    {
                        Console.WriteLine("Failure:Partial Update items using query");
                        testResults.Add(methodName, ResultStatus.Failure);
                    }
                }
                _report.AddPassedTestCase(methodName, "Success:Partial Update items using query");
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
                Console.WriteLine("Failure:Partial Update items using query");
            }
        }

        public void BasicUpdateQuery4()
        {
            productList.Clear();
            string methodName = MethodBase.GetCurrentMethod().Name; count++;
            try
            {

                cache.Clear();
                PopulateCache();
                string query = "Update  Alachisoft.NCache.Sample.Data.Product Set Name = @tea, Order.ShipCity = @lahore Add Order.Type = @important Set-meta $tag$ = @tags, $namedtag$ = @ntags where UnitPrice > ?";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("UnitPrice", Convert.ToDecimal(100));
                queryCommand.Parameters.Add("@tea", "Tea");
                queryCommand.Parameters.Add("@important", "Important");
                queryCommand.Parameters.Add("@lahore", "Lahore");
                queryCommand.Parameters.Add("@tags", "['prod','price']");
                queryCommand.Parameters.Add("@ntags", "[{'discount':0.4,'type':'decimal'}]");
                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);
                int reveived = 0;
                var returned = cache.SearchService.GetKeysByTag("price");
                if (returned.Count == updated)
                {
                    string searchQuery = "SELECT $Value$ FROM Alachisoft.NCache.Sample.Data.Product WHERE discount = @discount ";
                    QueryCommand searchQueryCommand = new QueryCommand(searchQuery);
                    searchQueryCommand.Parameters.Add("@discount", Convert.ToDecimal(0.4));
                    try
                    {
                        ICacheReader reader = cache.SearchService.ExecuteReader(searchQueryCommand);

                        if (reader.FieldCount > 0)
                        {
                            while (reader.Read())
                            {
                                reveived++;
                                Product result = reader.GetValue<Product>(1);

                            }


                        }
                        if (reveived == updated)
                        {
                            _report.AddPassedTestCase(methodName, "Success:Partial operations with meta data add with tags and named tags then retreive using tags and named tags");
                        }
                    }
                    catch (Exception ex)
                    {
                    }

                }
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
                Console.WriteLine("Failure:Partial operations with meta data add with tags and named tags then retreive using tags and named tags");
            }
        }

        //Test partial operation on complex object
        public void BasicUpdateQuery8()
        {

            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;
            try
            {

                cache.Clear();
                productList.Clear();
                PopulateWithComplexProductList();
                PopulateCache();


                string query = "Update  Alachisoft.NCache.Sample.Data.Product Set Order.Product.Order.OrderID = 15  Remove Order.Product.Order.ShipCity ";
                QueryCommand queryCommand = new QueryCommand(query);
                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                foreach (var item in productList)
                {
                    var entry = cache.Get<Product>("product" + item.Id);
                    if (entry.Order.Product.Order.OrderID != 15)
                    {
                        throw new Exception("Failure:Test partial operation on complex object");
                    }
                }

                _report.AddPassedTestCase(methodName, "Success:Test partial operation on complex object");

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }
            finally { productList.Clear(); }
        }

        //partial operations with meta data add with tags and named tags then retreive using tags and named tags with this

        public void BasicUpdateQuery12()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;
            try
            {
                cache.Clear();
                PopulateCache();
                string query = "Update  Alachisoft.NCache.Sample.Data.Product Set this.Name = @tea, this.Order.ShipCity = @lahore Add this.Order.Type = @important  Set-meta $tag$ = @tags, $namedtag$ = @ntags where Category = @beverages";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@tea", "Tea");
                queryCommand.Parameters.Add("@important", "Important");
                queryCommand.Parameters.Add("@lahore", "Lahore");
                queryCommand.Parameters.Add("@tags", "['prod','price']");
                queryCommand.Parameters.Add("@ntags", "[{'discount':0.4,'type':'decimal'}]");

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);
                foreach (var item in productList)
                {

                    var prod = cache.Get<Product>("product" + item.Id);
                    if (item.Category == "Beverages" && prod.Name != "Tea" && prod.Order.ShipCity != "Lahore")
                    {
                        throw new Exception("Failure:Partial Update items using query");
                    }
                }
                int reveived = 0;
                var returned = cache.SearchService.GetKeysByTag("price");
                if (returned.Count == updated)
                {
                    string searchQuery = "SELECT $Value$ FROM Alachisoft.NCache.Sample.Data.Product WHERE discount = @discount ";
                    QueryCommand searchQueryCommand = new QueryCommand(searchQuery);
                    searchQueryCommand.Parameters.Add("@discount", Convert.ToDecimal(0.4));
                    ICacheReader reader = cache.SearchService.ExecuteReader(searchQueryCommand);

                    if (reader.FieldCount > 0)
                    {
                        while (reader.Read())
                        {
                            reveived++;
                            Product result = reader.GetValue<Product>(1);

                        }


                    }
                    if (reveived == updated)
                    {
                        _report.AddPassedTestCase(methodName, "Success:Partial operations with meta data add with tags and named tags then retreive using tags and named tags with this");
                    }
                }
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
                Console.WriteLine("Failure:Partial operations with meta data add with tags and named tags then retreive using tags and named tags with this");
            }
        }


        //verify order is mantained or not 

        public void BasicUpdateQuery15()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;
            try
            {
                // productList.Add(new Product() { Id = 1, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 357, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });
                int received = 0;
                cache.Clear();

                PopulateCache();
                string query = "Update  Alachisoft.NCache.Sample.Data.Product Remove this.Name Add this.Order.Type = @important, this.Name = @tea Set $value$ = @num where Category = @beverages";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@tea", "Tea");
                queryCommand.Parameters.Add("@important", "Important");
                queryCommand.Parameters.Add("@lahore", "Lahore");
                queryCommand.Parameters.Add("@num", Convert.ToInt32(123));
                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);
                foreach (var item in productList)
                {

                    var prod = cache.Get<object>("product" + item.Id);
                    if (item.Category == "Beverages" && Convert.ToInt32(prod) != 123)
                    {
                        throw new Exception("Failure:Partial Update items using query");

                    }
                    else if (item.Category == "Beverages" && Convert.ToInt32(prod) == 123)
                    {
                        received++;
                    }
                }

                if (received == updated)
                {
                    _report.AddPassedTestCase(methodName, "Success: verify order is mantained or not ");
                }

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
                Console.WriteLine("Failure: verify order is mantained or not ");
            }
        }
        //partial operations with meta data add with group then retreive using group

        public void BasicUpdateQuery20()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;

            count++;
            try
            {
                cache.Clear();
                PopulateCache();
                string groupname = "prod";
                string query = "Update  Alachisoft.NCache.Sample.Data.Product Set this.Name = @tea, this.Order.ShipCity = @lahore Add this.Order.Type = @important  Set-meta $group$ = @group where Category = @beverages";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@tea", "Tea");
                queryCommand.Parameters.Add("@important", "Important");
                queryCommand.Parameters.Add("@lahore", "Lahore");
                queryCommand.Parameters.Add("@group", groupname);


                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);
                foreach (var item in productList)
                {
                    var prod = cache.Get<Product>("product" + item.Id);
                    if (item.Category == "Beverages" && prod.Name != "Tea" && prod.Order.ShipCity != "Lahore")
                    {
                        _report.AddFailedTestCase(methodName, new Exception("Failure:Partial Update items using query"));
                    }
                }
                int reveived = 0;
                var returned = cache.SearchService.GetGroupKeys(groupname);
                if (returned.Count == updated)
                {
                    if (reveived == updated)
                    {
                        _report.AddPassedTestCase(methodName, "Success:Partial operations with meta data add with group then retreive using group");
                    }
                }
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
                Console.WriteLine("Failure:Partial operations with meta data add with group  retreive using group");
            }
        }

        //no partial operation can exist for more than once 
        public void BasicUpdateQuery21()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;
            try
            {
                cache.Clear();
                PopulateCache();
                string query = "Update  Alachisoft.NCache.Sample.Data.Product Set this.Name = @tea Set this.Name = @tea Add this.Order.Type = @important  Set-meta $tag$ = @tags, $namedtag$ = @ntags  Set-meta $tag$ = @tags, $ntag$ = @ntags  where Category = @beverages";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@tea", "Tea");
                queryCommand.Parameters.Add("@important", "Important");
                queryCommand.Parameters.Add("@lahore", "Lahore");
                queryCommand.Parameters.Add("@tags", "['prod','price']");
                queryCommand.Parameters.Add("@ntags", "[{'discount':0.4,'type':'decimal'}]");

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);
                _report.AddFailedTestCase(methodName, new Exception("Failure:no partial operation can exist for more than once "));

            }
            catch (Exception ex)
            {

                _report.AddPassedTestCase(methodName, "Success : no partial operation can exist for more than once ");
            }
        }

        public void BasicUpdateQuery23()
        {
            int updated = 0;
            cache.Clear();
            PopulateCache();
            string methodName = MethodBase.GetCurrentMethod().Name;

            count++;
            try
            {

                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Set this.Name = '\"Tea\"' Copy Order.ShipCountry = Order.ShipCity move this.Time = this.Order.Product.Time where UnitPrice > ?";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("UnitPrice", Convert.ToDecimal(100));
                IDictionary<string, Exception> dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                
                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                 ts.Milliseconds / 10);
                Console.WriteLine("RunTime " + elapsedTime);
                foreach (var item in productList)
                {

                    var prod = cache.Get<Product>("product" + item.Id);
                    if (prod != null && item.UnitPrice > 100 && prod.Name != "Tea")
                    {
                        throw new Exception("Failure:Partial Update items using query");
                    }
                }
                _report.AddPassedTestCase(methodName, "Success:Partial Update items using query");
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }


        }

        #endregion


        #region --------------------------------- Select queries ---------------------------------

        //Populate cache items with meta data , update metadata i.e to remove meta
        public void BasicUpdateQuery5()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;
            try
            {
                int receivedValuesBySearch = 0;
                int receivedValuesBySearchAfterUpdate = 0;
                cache.Clear();
                PopulateCacheWithMeta();
                //Search on items using already existing meta
                string searchQuery = "SELECT $Value$ FROM Alachisoft.NCache.Sample.Data.Product WHERE discount = @discount ";
                QueryCommand searchQueryCommand = new QueryCommand(searchQuery);
                searchQueryCommand.Parameters.Add("@discount", Convert.ToDecimal(0.5));
                ICacheReader reader = cache.SearchService.ExecuteReader(searchQueryCommand);

                if (reader.FieldCount > 0)
                {
                    while (reader.Read())
                    {
                        receivedValuesBySearch++;
                    }
                }

                var receivedValuesByTag1 = cache.SearchService.GetKeysByTag("East Coast Product");
                var receivedValuesByTag2 = cache.SearchService.GetKeysByTag("East Coast Product");
                if (receivedValuesByTag1.Count != receivedValuesByTag2.Count)
                {
                    throw new Exception("Test case failed");
                }
                string query = "Update  Alachisoft.NCache.Sample.Data.Product Set Name = @tea, Order.ShipCity = @important Add Order.Type = @important Remove-meta $tag$ = @tags, $namedtag$ = @ntags  where Category = @beverages";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@tea", "Tea");
                queryCommand.Parameters.Add("@important", "Important");
                queryCommand.Parameters.Add("@lahore", "Important Product");
                queryCommand.Parameters.Add("@tags", "['East Coast Product','Important Product']");
                queryCommand.Parameters.Add("@ntags", "['discount']");
                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                searchQueryCommand = new QueryCommand(searchQuery);
                searchQueryCommand.Parameters.Add("@discount", Convert.ToDecimal(0.5));
                reader = cache.SearchService.ExecuteReader(searchQueryCommand);

                if (reader.FieldCount > 0)
                {
                    while (reader.Read())
                    {
                        receivedValuesBySearchAfterUpdate++;
                    }
                }
                var receivedValuesByTag1AfterUpdate = cache.SearchService.GetKeysByTag("East Coast Product");
                var receivedValuesByTag2AfterUpdate = cache.SearchService.GetKeysByTag("Important Product");
                if ((updated + receivedValuesBySearchAfterUpdate) == productList.Count)
                {
                    if ((receivedValuesByTag1AfterUpdate.Count == receivedValuesByTag2AfterUpdate.Count) && (receivedValuesByTag2AfterUpdate.Count == receivedValuesBySearchAfterUpdate))
                    {
                        Console.WriteLine("Success:Populate cache items with meta data , update metadata i.e to remove meta");
                        testResults.Add(methodName, ResultStatus.Success);
                    }
                }

            }
            catch (OperationFailedException ex)
            {
                _report.AddFailedTestCase(methodName, ex);
                Console.WriteLine("Failure:Populate cache items with meta data , update metadata i.e to remove meta");
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
                Console.WriteLine("Failure:Populate cache items with meta data , update metadata i.e to remove meta");
            }
        }

        //Populate cache items with meta data , update metadata i.e to remove meta on all items with no where clause.
        public void BasicUpdateQuery6()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;
            try
            {
                int receivedValuesBySearch = 0;
                int receivedValuesBySearchAfterUpdate = 0;
                cache.Clear();
                PopulateCacheWithMeta();
                //Search on items using already existing meta
                string searchQuery = "SELECT $Value$ FROM Alachisoft.NCache.Sample.Data.Product WHERE discount = @discount ";
                QueryCommand searchQueryCommand = new QueryCommand(searchQuery);
                searchQueryCommand.Parameters.Add("@discount", Convert.ToDecimal(0.5));
                ICacheReader reader = cache.SearchService.ExecuteReader(searchQueryCommand);

                if (reader.FieldCount > 0)
                {
                    while (reader.Read())
                    {
                        receivedValuesBySearch++;
                    }
                }

                var receivedValuesByTag1 = cache.SearchService.GetKeysByTag("East Coast Product");
                var receivedValuesByTag2 = cache.SearchService.GetKeysByTag("East Coast Product");
                if (receivedValuesByTag1.Count != receivedValuesByTag2.Count)
                {
                    throw new Exception("Test case failed");
                }
                string query = "Update  Alachisoft.NCache.Sample.Data.Product Set Name = @tea Remove-meta $tag$ = @tags, $namedtag$ = @ntags";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@tea", "Tea");
                queryCommand.Parameters.Add("@important", "Important");
                queryCommand.Parameters.Add("@lahore", "Important Product");
                queryCommand.Parameters.Add("@tags", "['East Coast Product','Important Product']");
                queryCommand.Parameters.Add("@ntags", "['discount']");
                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                searchQueryCommand = new QueryCommand(searchQuery);
                searchQueryCommand.Parameters.Add("@discount", Convert.ToDecimal(0.5));
                reader = cache.SearchService.ExecuteReader(searchQueryCommand);

                if (reader.FieldCount > 0)
                {
                    while (reader.Read())
                    {
                        receivedValuesBySearchAfterUpdate++;
                    }
                }
                var receivedValuesByTag1AfterUpdate = cache.SearchService.GetKeysByTag("East Coast Product");
                var receivedValuesByTag2AfterUpdate = cache.SearchService.GetKeysByTag("Important Product");
                if (updated == productList.Count)
                {
                    if ((receivedValuesByTag1AfterUpdate.Count == receivedValuesByTag2AfterUpdate.Count) && (receivedValuesByTag1AfterUpdate.Count == receivedValuesBySearchAfterUpdate))
                    {
                        _report.AddPassedTestCase(methodName, "Success:Populate cache items with meta data , update metadata i.e to remove meta with no where claue");
                    }
                }

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
                Console.WriteLine("Failure:Populate cache items with meta data , update metadata i.e to remove meta");
            }
        }
        //populate cache with items with meta , update meta retreive using updated meta but meta operation should fail on items having no order in them. 

        public void BasicUpdateQuery7()
        {

            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;
            try
            {
                int receivedValuesBySearch = 0;
                int receivedValuesBySearchAfterUpdate = 0;
                cache.Clear();
                PopulateCacheWithMeta();
                //Search on items using already existing meta
                string searchQuery = "SELECT $Value$ FROM Alachisoft.NCache.Sample.Data.Product WHERE discount = @discount ";
                QueryCommand searchQueryCommand = new QueryCommand(searchQuery);
                searchQueryCommand.Parameters.Add("@discount", Convert.ToDecimal(0.5));
                ICacheReader reader = cache.SearchService.ExecuteReader(searchQueryCommand);

                if (reader.FieldCount > 0)
                {
                    while (reader.Read())
                    {
                        receivedValuesBySearch++;
                    }
                }

                var receivedValuesByTag1 = cache.SearchService.GetKeysByTag("East Coast Product");
                var receivedValuesByTag2 = cache.SearchService.GetKeysByTag("East Coast Product");
                if (receivedValuesByTag1.Count != receivedValuesByTag2.Count)
                {
                    throw new Exception("Test case failed");
                }
                string query = "Update  Alachisoft.NCache.Sample.Data.Product Set Name = @tea Set-meta $tag$ = @tags, $namedtag$ = @ntags where Id > 0";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@tea", "Tea");
                queryCommand.Parameters.Add("@important", "Important");
                queryCommand.Parameters.Add("@lahore", "Lahore");
                queryCommand.Parameters.Add("@tags", "['prod','price']");
                queryCommand.Parameters.Add("@ntags", "[{'discount':0.8,'type':'decimal'}]");
                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                searchQueryCommand = new QueryCommand(searchQuery);
                searchQueryCommand.Parameters.Add("@discount", Convert.ToDecimal(0.8));
                reader = cache.SearchService.ExecuteReader(searchQueryCommand);

                if (reader.FieldCount > 0)
                {
                    while (reader.Read())
                    {
                        receivedValuesBySearchAfterUpdate++;
                    }
                }
                var receivedValuesByTag1AfterUpdate = cache.SearchService.GetKeysByTag("East Coast Product");
                var receivedValuesByTag2AfterUpdate = cache.SearchService.GetKeysByTag("East Coast Product");
                if (updated == productList.Count)
                {
                    if (receivedValuesByTag1AfterUpdate == receivedValuesByTag2AfterUpdate && receivedValuesBySearchAfterUpdate == receivedValuesByTag2AfterUpdate.Count)
                    {
                        _report.AddPassedTestCase(methodName, "Success: Populate cache with items with meta , update meta retreive using updated meta but meta operation should fail on items having no order in them.");

                    }
                }


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
                Console.WriteLine("Failure:Populate cache items with meta data , update metadata i.e to remove meta");
            }
        }


        #endregion


        #region --------------------------------- Misc Test cases ---------------------------------

        public void UpdateIntoQuery()
        {

            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "Invalid query format i.e.Insert InTo ";

            count++;
            try
            {

                cache.Clear();

                PopulateCache();


                string query = "Update into Alachisoft.NCache.Sample.Data.Product (Key,Value) Values (@cachekey, @val) ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@cachekey", "key");
                queryCommand.Parameters.Add("@val", "value");

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);


                _report.AddFailedTestCase(methodName, new Exception(description));


            }
            catch (Exception ex)
            {
                if (Helper.IsIncorrectFormatException(ex))
                    _report.AddPassedTestCase(methodName, description);
                else
                    _report.AddFailedTestCase(methodName, ex);
            }

        }

        #endregion


        #region --------------------------------- Helper Methods ---------------------------------

        // verify on basic int with tags and define string as new type should throw exception as per newly defined behaviour or decision
        //public void BasicUpdateQuery17()
        //{
        //    string methodName = "BasicUpdateQuery17";
        //    count++;
        //    try
        //    {
        //        cache.Clear();

        //        string group = "basic-integers";

        //        for (int i = 0; i < 100; i++)
        //        {
        //            var cacheItem = new CacheItem(5);
        //            cacheItem.Group = group;
        //            cache.Insert("group:" + i, cacheItem);
        //        }
        //        var searchQuery = "Select * from System.Int32 where $Group$ = ?";
        //        var searchQueryCommand = new QueryCommand(searchQuery);
        //        searchQueryCommand.Parameters.Add("$Group$", group);
        //        var reader1 = cache.SearchService.ExecuteReader(searchQueryCommand);
        //        int indexedItemsOnInt = 0;
        //        if (reader1.FieldCount > 0)
        //        {
        //            while (reader1.Read())
        //            {
        //                indexedItemsOnInt++;
        //            }
        //        }

        //        var kletys = cache.SearchService.GetGroupKeys(group);
        //        if (indexedItemsOnInt != kletys.Count)
        //            throw new Exception("Failed");

        //        string query = "Update System.Int32 Set $value$ = hello Set-Meta = @data where $Group$ = ?";
        //        QueryCommand queryCommand = new QueryCommand(query);
        //        queryCommand.Parameters.Add("@data", @"{'type':'System.String'}");
        //        queryCommand.Parameters.Add("$Group$", group);
        //        var updated = cache.SearchService.ExecuteNonQuery(queryCommand);
        //        int indexedItemsOnString = 0;

        //        var searchQueryOnString = "Select * from System.String where $Group$ = ?";
        //        searchQueryCommand = new QueryCommand(searchQueryOnString);
        //        searchQueryCommand.Parameters.Add("$Group$", group);
        //         reader1 = cache.SearchService.ExecuteReader(searchQueryCommand);

        //        if (reader1.FieldCount > 0)
        //        {
        //            while(reader1.Read())
        //                indexedItemsOnString++;
        //        }


        //        if (indexedItemsOnString == updated)
        //        {
        //            Console.WriteLine("Success: verify on basic int with tags and define string as new type ");
        //            testResults.Add(methodName, ResultStatus.Success);
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        Console.WriteLine("Failure: verify on basic int ");
        //        testResults.Add(methodName, ResultStatus.Failure);
        //    }
        //}

        //Add value with date time throw exception
        //public void BasicUpdateQuery18()
        //{
        //    string methodName = "BasicUpdateQuery18";
        //    count++;
        //    try
        //    {
        //        cache.Clear();

        //        string group = "basic-integers";

        //        for (int i = 0; i < 100; i++)
        //        {
        //            var cacheItem = new CacheItem(5);
        //            cacheItem.Group = group;
        //            cache.Insert("group:" + i, cacheItem);
        //        }

        //        var date = DateTime.Now;
        //        var kletys = cache.SearchService.GetGroupKeys(group);


        //        string query = "Update System.Int32 Replace $value$ = @date, Set Meta = @data where $Group$ = ?";
        //        QueryCommand queryCommand = new QueryCommand(query);
        //        queryCommand.Parameters.Add("@date", date);
        //        queryCommand.Parameters.Add("@data", @"{'type':'System.DateTime' }");
        //        queryCommand.Parameters.Add("$Group$", group);
        //        var updated = cache.SearchService.ExecuteNonQuery(queryCommand);
        //        int indexedItems = 0;

        //        var searchQueryOnString = "Select * from System.DateTime where $Group$ = ?";
        //        var searchQueryCommand = new QueryCommand(searchQueryOnString);
        //        searchQueryCommand.Parameters.Add("$Group$", group);
        //        var reader1 = cache.SearchService.ExecuteReader(searchQueryCommand);

        //        if (reader1.FieldCount > 0)
        //        {
        //            while (reader1.Read())
        //                indexedItems++;
        //        }

        //        if (indexedItems == updated)
        //        {
        //            Console.WriteLine("Success: verify on basic date time ");
        //            testResults.Add(methodName, ResultStatus.Success);
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        Console.WriteLine("Failure: verify on basic datetime ");
        //        testResults.Add(methodName, ResultStatus.Failure);
        //    }
        //}


        //------------------Helper methods---------------------------------------
        //-----------------------------------------------------------------------

        private Product GetProduct()
        {

            return new Product() { Id = 1, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 357, Order = new Alachisoft.NCache.Sample.Data.Order { OrderID = 10, ShipCity = "rawalpindi" } };
        }

        private void PopulateCache(int totalItemToAdd = -1)
        {
            int itemsAdded = 0;

            if (productList.Count == 0)
                PopulateProductList();

            foreach (var item in productList)
            {
                cache.Insert("product" + item.Id, item);

                itemsAdded++;
                if (totalItemToAdd != -1 && itemsAdded == totalItemToAdd)
                    break;
            }
        }

        private IList<string> PopulateCacheAndGetKeys(int totalItemToAdd = -1)
        {
            IList<string> keys = new List<string>();

            int itemsAdded = 0;
            string key;
            foreach (var item in productList)
            {
                key = "product" + item.Id;
                keys.Add(key);
                cache.Add(key, item);

                itemsAdded++;
                if (totalItemToAdd != -1 && itemsAdded == totalItemToAdd)
                    break;
            }

            return keys;
        }

        private void PopulateCacheWithMeta()
        {
            Tag[] tags = new Tag[2];
            tags[0] = new Tag("East Coast Product");
            tags[1] = new Tag("Important Product");
            var productNamedTag = new NamedTagsDictionary();
            productNamedTag.Add("discount", Convert.ToDecimal(0.5));
            foreach (var prod in productList)
            {

                // new JsonObject(JsonConvert.SerializeObject(item))
                var item = new CacheItem(prod);
                // item.NamedTags = new Alachisoft.NCache.Runtime.Caching.NamedTagsDictionary();
                item.NamedTags = productNamedTag;
                item.Tags = tags;
                cache.Add("product" + prod.Id, item);
            }
        }

        private void PopulateWithComplexProductList()
        {
            productList.Add(new Product()
            {
                Id = 1,
                Name = "Chai",
                ClassName = "Electronics",
                Category = "Beverages",
                UnitPrice = 357,
                Order = new Order
                {
                    OrderID = 10,
                    ShipCity = "rawalpindi",
                    Product = new Product
                    {
                        Id = 11,
                        Name = "boards",
                        Category = "electronics",
                        Order = new Order { OrderID = 11, ShipCity = "rawalpindi" }
                    }
                }
            });
            productList.Add(new Product()
            {
                Id = 2,
                Name = "Chang",
                ClassName = "Electronics",
                Category = "Meat",
                UnitPrice = 188,
                Order = new Order
                {
                    OrderID = 10,
                    ShipCity = "rawalpindi",
                    Product = new Product
                    {
                        Id = 11,
                        Name = "boards",
                        Category = "electronics",
                        Order = new Order { OrderID = 11, ShipCity = "rawalpindi" }
                    }
                }
            }
            );
            productList.Add(new Product()
            {
                Id = 3,
                Name = "Aniseed Syrup",
                ClassName = "Electronics",
                Category = "Beverages",
                UnitPrice = 258,
                Order = new Order
                {
                    OrderID = 10,
                    ShipCity = "rawalpindi",
                    Product = new Product
                    {
                        Id = 11,
                        Name = "boards",
                        Category = "electronics",
                        Order = new Order { OrderID = 11, ShipCity = "rawalpindi" }
                    }
                }
            }
            );
            productList.Add(new Product()
            {
                Id = 4,
                Name = "IKura",
                ClassName = "Electronics",
                Category = "Produce",
                UnitPrice = 50,
                Order = new Order
                {
                    OrderID = 10,
                    ShipCity = "rawalpindi",
                    Product = new Product
                    {
                        Id = 11,
                        Name = "boards",
                        Category = "electronics",
                        Order = new Order { OrderID = 11, ShipCity = "rawalpindi" }
                    }
                }
            });
            productList.Add(new Product()
            {
                Id = 5,
                Name = "Tofu",
                ClassName = "Electronics",
                Category = "Seafood",
                UnitPrice = 78,
                Order = new Order
                {
                    OrderID = 10,
                    ShipCity = "rawalpindi",
                    Product = new Product
                    {
                        Id = 11,
                        Name = "boards",
                        Category = "electronics",
                        Order = new Order { OrderID = 11, ShipCity = "rawalpindi" }
                    }
                }
            });

        }
        private void PopulateProductList()
        {
            //productList.Add(new Product() { Id = 1, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 35, Order = new Order { OrderID = 10, ShipCity = "rawalpindi", ShipCountry = "Pakistan" }, Images = new Image[3] { new Image(), new Image(), new Image() } });
            //return;


            productList.Add(new Product() { Id = 1, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 35, Order = new Order { OrderID = 10, ShipCity = "rawalpindi", ShipCountry = "Pakistan" }, Images = new Image[3] { new Image(), new Image(), new Image() } });
            productList.Add(new Product() { Id = 2, Time = DateTime.Now, Name = "Chang", ClassName = "Electronics", Category = "Meat", UnitPrice = 18, Order = new Order { OrderID = 10 }, Images = new Image[3] { new Image(), new Image(), new Image() } });
            productList.Add(new Product() { Id = 3, Time = DateTime.Now, Name = "Aniseed Syrup", ClassName = "Electronics", Category = "Beverages", UnitPrice = 258, Order = new Order { OrderID = 10, ShipCity = "rawalpindi", ShipCountry = "Pakistan", Product = new Product() { Time = DateTime.Now } }, Images = new Image[3] { new Image(), new Image(), new Image() } });
            productList.Add(new Product() { Id = 4, Time = DateTime.Now, Name = "IKura", ClassName = "Electronics", Category = "Produce", UnitPrice = 50, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" }, Images = new Image[3] { new Image(), new Image(), new Image() } });
            //productList.Add(new Product() { Id = 5, Time = DateTime.Now, Name = "Tofu", ClassName = "Electronics", Category = "Seafood", UnitPrice = 78, Images = new Image[3] { new Image(), new Image(), new Image() } });
            productList.Add(new Product() { Id = 6, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 37, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" }, Images = new Image[3] { new Image(), new Image(), new Image() } });
            productList.Add(new Product() { Id = 7, Time = DateTime.Now, Name = "Chang", ClassName = "Electronics", Category = "Meat", UnitPrice = 18, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" }, Images = new Image[3] { new Image(), new Image(), new Image() } });
            productList.Add(new Product() { Id = 8, Time = DateTime.Now, Name = "Aniseed Syrup", ClassName = "Electronics", Category = "Beverages", UnitPrice = 258, Order = new Order { OrderID = 10, ShipCity = "rawalpindi", ShipCountry = "Pakistan", Product = new Product() { Time = DateTime.Now } }, Images = new Image[3] { new Image(), new Image(), new Image() } });
            productList.Add(new Product() { Id = 9, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 37, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" }, Images = new Image[3] { new Image(), new Image(), new Image() } });
            /* productList.Add(new Product() { Id = 10, Time = DateTime.Now, Name = "Chang", ClassName = "Electronics", Category = "Meat", UnitPrice = 18, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });
             productList.Add(new Product() { Id = 11, Time = DateTime.Now, Name = "Aniseed Syrup", ClassName = "Electronics", Category = "Beverages", UnitPrice = 58, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });
             productList.Add(new Product() { Id = 12, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 37, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });
             productList.Add(new Product() { Id = 13, Time = DateTime.Now, Name = "Chang", ClassName = "Electronics", Category = "Meat", UnitPrice = 88, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });
             productList.Add(new Product() { Id = 14, Time = DateTime.Now, Name = "Aniseed Syrup", ClassName = "Electronics", Category = "Beverages", UnitPrice = 258, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });
             productList.Add(new Product() { Id = 15, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 57, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });
             productList.Add(new Product() { Id = 16, Time = DateTime.Now, Name = "Chang", ClassName = "Electronics", Category = "Meat", UnitPrice = 18, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });
             productList.Add(new Product() { Id = 17, Time = DateTime.Now, Name = "Aniseed Syrup", ClassName = "Electronics", Category = "Beverages", UnitPrice = 28, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });
             productList.Add(new Product() { Id = 18, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 37, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });
             productList.Add(new Product() { Id = 19, Time = DateTime.Now, Name = "Chang", ClassName = "Electronics", Category = "Meat", UnitPrice = 18, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });
             productList.Add(new Product() { Id = 20, Time = DateTime.Now, Name = "Aniseed Syrup", ClassName = "Electronics", Category = "Beverages", UnitPrice = 58, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });
             productList.Add(new Product() { Id = 21, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 37, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });
             productList.Add(new Product() { Id = 22, Time = DateTime.Now, Name = "Chang", ClassName = "Electronics", Category = "Meat", UnitPrice = 18, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });
             productList.Add(new Product() { Id = 23, Time = DateTime.Now, Name = "Aniseed Syrup", ClassName = "Electronics", Category = "Beverages", UnitPrice = 58, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });
             productList.Add(new Product() { Id = 24, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 37, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });
             productList.Add(new Product() { Id = 25, Time = DateTime.Now, Name = "Chang", ClassName = "Electronics", Category = "Meat", UnitPrice = 88, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });
 // ExpandProductList(10);*/
        }

        void ExpandProductList(int num)
        {
            for (int i = 1; i < 3; i++)
            {
                productList.Add(new Product() { Id = i + 25, Time = DateTime.Now, Name = "Chang", ClassName = "Electronics", Category = "Meat", UnitPrice = 188, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });
            }

        }
        public void AddArrayWithTags()
        {
            try
            {
                var productNamedTag = new NamedTagsDictionary();
                int receivedValuesBySearch = 0;
                string key = "Products";
                Tag[] tags = new Tag[2];
                tags[0] = new Tag("East Coast Product");
                tags[1] = new Tag("Important Product");
                productNamedTag.Add("discount", Convert.ToDecimal(0.5));
                productList.Clear();
                PopulateProductList();
                var jsonArray = new JsonArray();
                foreach (Product product in productList)
                {
                    // Create jsonObject and set its attributes
                    // ProductName is string so it needs to be added with JsonValue
                    var jsonProduct = new JsonObject();
                    jsonProduct.AddAttribute("ProductID", product.Id);
                    jsonProduct.AddAttribute("ProductName", (JsonValue)product.Name);
                    jsonProduct.AddAttribute("Category", (JsonValue)product.Category);
                    jsonProduct.AddAttribute("UnitPrice", product.UnitPrice);
                    // Add jsonObjects to the jsonArray
                    jsonArray.Add(jsonProduct);
                }

                var cacheItem = new CacheItem(jsonArray);
                cacheItem.Tags = tags;
                cacheItem.NamedTags = productNamedTag;
                cache.Clear();
                cache.Add(key, cacheItem);
                var receivedValuesByTag1 = cache.SearchService.GetKeysByTag("East Coast Product");
                var receivedValuesByTag2 = cache.SearchService.GetKeysByTag("Important Product");

                string searchQuery = "SELECT * FROM Alachisoft.NCache.Runtime.JSON.JsonArray WHERE discount = @discount ";
                QueryCommand searchQueryCommand = new QueryCommand(searchQuery);
                searchQueryCommand.Parameters.Add("@discount", Convert.ToDecimal(0.5));
                ICacheReader reader = cache.SearchService.ExecuteReader(searchQueryCommand);

                if (reader.FieldCount > 0)
                {
                    while (reader.Read())
                    {
                        receivedValuesBySearch++;
                    }
                }

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }

        }


        #endregion

    }

}
