using Alachisoft.NCache.Client;
using Alachisoft.NCache.Runtime.JSON;
using Alachisoft.NCache.Sample.Data;
using QueriesTestApplication.Utils;
using System;
using System.Collections.Generic;

using System.Reflection;

namespace QueriesTestApplication
{
    public class InsertQueriesTestForJsonObj
    {
        private int count = 0;
        ICache cache;
        public Dictionary<string, ResultStatus> testResults;
        public Dictionary<string, TestResult> testResults1;
        List<Product> productList;
        private Report _report;

        public Report Report { get => _report; }

        public InsertQueriesTestForJsonObj()
        {
            cache = CacheManager.GetCache(Common.CacheName);
            testResults = new Dictionary<string, ResultStatus>();
            testResults1 = new Dictionary<string, TestResult>();
            productList = new List<Product>();
        }

        public Dictionary<string, ResultStatus> TestResults
        {
            get { return testResults; }
        }


        /// <summary>
        /// Adds a JsonArray in cache by NamedParams.
        /// Then verifies its type by getting it back from cache
        /// </summary>
        public void AddJsonArray()
        {
            var methodName = "AddJsonArray";
            try
            {
                cache.Clear();
                PopulateProductList();
                string key = GetKey();

                var val = new JsonArray();
                val.Add(1);
                val.Add(2);
                val.Add(3);

                string insertQuery = "Insert into abc (Key,Value) Values ( @cachekey, @val)";
                QueryCommand queryCommand = new QueryCommand(insertQuery);
                queryCommand.Parameters.Add("@cachekey", key);
                queryCommand.Parameters.Add("@val", val);

                var insertresult = cache.SearchService.ExecuteNonQuery(queryCommand);

                var result = cache.Get<JsonArray>(key);


                if (result.Count == 3)                
                    _report.AddPassedTestCase(methodName,"Success: Add a JsonArray in Cache and then verify it's type");                    
                
                else                
                    throw new Exception("Failure: Add a JsonArray in Cache and then verify it's type");
                

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
              
            }

        }
                
        /// <summary>
        /// Adds a JSON Object of Product class in cache.
        /// </summary>
        public void AddJsonObject()
        {
            cache.Clear();
            var methodName = "AddJsonObject";

            Product product = new Product() { Id = 1, Time = DateTime.Now, Name = "Chai", Manufacturers = new string[2] { "AlachiSoft", "DiyaTech" }, ClassName = "Electronics", Category = "Beverages", UnitPrice = 35, Order = new Alachisoft.NCache.Sample.Data.Order { OrderID = 10, ShipCity = "rawalpindi", ShipCountry = "Pakistan" } };
            cache.Insert("1", product);
            try
            {

                string key = "abc";
                var val = cache.Get<JsonObject>("1");
                cache.Remove("1");

                string query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value) Values (@key1, @val)";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@key1", key);
                queryCommand.Parameters.Add("@val", val);

                var insertresult = cache.SearchService.ExecuteNonQuery(queryCommand);

                var returned = cache.Get<Product>(key);

                if (returned.Id == product.Id && returned.Order.ShipName == product.Order.ShipName)
                {
                    _report.AddPassedTestCase(methodName, "Success: Add JSON Object");
                   
                    /*Console.WriteLine("ID: " + returned.Id.GetType() + " " + returned.Id);
                    Console.WriteLine("Name: " + returned.Name.GetType() + " " + returned.Name);
                    Console.WriteLine("Expirable: " + returned.Expirable.GetType() + " " + returned.Expirable);
                    Console.WriteLine("Manufacturers:  " + returned.Manufacturers.GetType() + " " + " " + returned.Manufacturers[1]);
                    Console.WriteLine("Order:  " + returned.Order.GetType() + " " + returned.Order.OrderID);
                    Console.ReadKey();*/
                }
                else                
                    throw new Exception("Failure: Add JSON Object ");                 
               

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);               
            }

        }

        public void AddJsonValue()
        {
            cache.Clear();
            string methodName = MethodBase.GetCurrentMethod().Name;
           
            try
            {
                string jsonString = "{\"key\": \"value\"}";
                JsonValue val = (JsonValue)jsonString;

                string key = methodName;               

                string query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value) Values (@key1, @val)";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@key1", key);
                queryCommand.Parameters.Add("@val", val);

                var insertresult = cache.SearchService.ExecuteNonQuery(queryCommand);

                var objFromCache = cache.Get<JsonValue>(key);

                if(objFromCache == null)
                    throw new Exception(ResourceMessages.GotNullObject);

                if (objFromCache.ToString().Contains("vlaue"))                
                    _report.AddPassedTestCase(methodName, "Success: Add JSON Object");   
                else
                    throw new Exception("Failure: Insert Json Value in cache ");


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }

        }

        public void InsertProduct()
        {
             string methodName = MethodBase.GetCurrentMethod().Name;

            try
            {
                cache.Clear();
                string key = GetKey();
                var val = GetProduct();

                string query = $"Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value) Values ( {key} , @val)";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@val", val);
                var result = cache.SearchService.ExecuteNonQuery(queryCommand);
                if (result > 0 && cache.Contains(key))
                {
                    var productFromCache = cache.Get<Product>(key) ?? throw new Exception(ResourceMessages.GotNullObject);

                    if (productFromCache.Id != val.Id)
                        throw new Exception("Id of product inserted and got from cache doesnot match");
                }
                else
                    throw new Exception("Object is inserted to cache by insert query");

                _report.AddPassedTestCase(methodName, "Successful: Add a key-value pair with Insert. Precondition: key not present.");
                count++;
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }

        }

        public void InsertNullObject()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;

            try
            {
                cache.Clear();
                string key = GetKey();
                var val = GetProduct();

                string query = $"Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value) Values ( {key} , @val)";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@val", null);
                var result = cache.SearchService.ExecuteNonQuery(queryCommand);
                
                throw new Exception("didnot get exception after adding null object in cache");
                                
            }
            catch (Exception ex)
            {
                if(ex.Message.Contains("null"))
                    _report.AddPassedTestCase(methodName,$"Got exception for adding null object {ex}");
                else
                    _report.AddFailedTestCase(methodName, ex);
            }

        }


        //                        Helper Methods
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private string GetKey()
        {
            Random rnd = new Random();
            return "CacheKey_1";
        }

        private Product GetProduct()
        {

            return new Product() { Expirable = false, Manufacturers = new[] { "Alachisoft" }, Id = 1, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 357, Order = new Alachisoft.NCache.Sample.Data.Order { OrderID = 10, ShipCity = "rawalpindi" } };
        }

        private void PopulateProductList()
        {
            productList.Add(new Product() { Id = 1, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 357 });
            productList.Add(new Product() { Id = 2, Time = DateTime.Now, Name = "Chang", ClassName = "Electronics", Category = "Meat", UnitPrice = 188 });
            productList.Add(new Product() { Id = 3, Time = DateTime.Now, Name = "Aniseed Syrup", ClassName = "Electronics", Category = "Beverages", UnitPrice = 258 });
            productList.Add(new Product() { Id = 4, Time = DateTime.Now, Name = "IKura", ClassName = "Electronics", Category = "Produce", UnitPrice = 50 });
            productList.Add(new Product() { Id = 5, Time = DateTime.Now, Name = "Tofu", ClassName = "Electronics", Category = "Seafood", UnitPrice = 78 });
            productList.Add(new Product() { Id = 6, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 357 });
            productList.Add(new Product() { Id = 7, Time = DateTime.Now, Name = "Chang", ClassName = "Electronics", Category = "Meat", UnitPrice = 188 });
            productList.Add(new Product() { Id = 8, Time = DateTime.Now, Name = "Aniseed Syrup", ClassName = "Electronics", Category = "Beverages", UnitPrice = 258 });
            productList.Add(new Product() { Id = 9, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 357 });
            productList.Add(new Product() { Id = 10, Time = DateTime.Now, Name = "Chang", ClassName = "Electronics", Category = "Meat", UnitPrice = 188 });
            productList.Add(new Product() { Id = 11, Time = DateTime.Now, Name = "Aniseed Syrup", ClassName = "Electronics", Category = "Beverages", UnitPrice = 258 });
            productList.Add(new Product() { Id = 12, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 357 });
            productList.Add(new Product() { Id = 13, Time = DateTime.Now, Name = "Chang", ClassName = "Electronics", Category = "Meat", UnitPrice = 188 });
            productList.Add(new Product() { Id = 14, Time = DateTime.Now, Name = "Aniseed Syrup", ClassName = "Electronics", Category = "Beverages", UnitPrice = 258 });
            productList.Add(new Product() { Id = 15, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 357 });
            productList.Add(new Product() { Id = 16, Time = DateTime.Now, Name = "Chang", ClassName = "Electronics", Category = "Meat", UnitPrice = 188 });
            productList.Add(new Product() { Id = 17, Time = DateTime.Now, Name = "Aniseed Syrup", ClassName = "Electronics", Category = "Beverages", UnitPrice = 258 });
            productList.Add(new Product() { Id = 18, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 357 });
            productList.Add(new Product() { Id = 19, Time = DateTime.Now, Name = "Chang", ClassName = "Electronics", Category = "Meat", UnitPrice = 188, Order = new Alachisoft.NCache.Sample.Data.Order { OrderID = 10, ShipCity = "rawalpindi" } });
            productList.Add(new Product() { Id = 20, Time = DateTime.Now, Name = "Aniseed Syrup", ClassName = "Electronics", Category = "Beverages", UnitPrice = 258, Order = new Alachisoft.NCache.Sample.Data.Order { OrderID = 10, ShipCity = "rawalpindi" } });
            productList.Add(new Product() { Id = 21, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 357, Order = new Alachisoft.NCache.Sample.Data.Order { OrderID = 10, ShipCity = "rawalpindi" } });
            productList.Add(new Product() { Id = 22, Time = DateTime.Now, Name = "Chang", ClassName = "Electronics", Category = "Meat", UnitPrice = 188, Order = new Alachisoft.NCache.Sample.Data.Order { OrderID = 10, ShipCity = "rawalpindi" } });
            productList.Add(new Product() { Id = 23, Time = DateTime.Now, Name = "Aniseed Syrup", ClassName = "Electronics", Category = "Beverages", UnitPrice = 258, Order = new Alachisoft.NCache.Sample.Data.Order { OrderID = 10, ShipCity = "rawalpindi" } });
            productList.Add(new Product() { Id = 24, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 357, Order = new Alachisoft.NCache.Sample.Data.Order { OrderID = 10, ShipCity = "rawalpindi" } });
            productList.Add(new Product() { Id = 25, Time = DateTime.Now, Name = "Chang", ClassName = "Electronics", Category = "Meat", UnitPrice = 188, Order = new Alachisoft.NCache.Sample.Data.Order { OrderID = 10, ShipCity = "rawalpindi" } });
        }
    }

 
}
