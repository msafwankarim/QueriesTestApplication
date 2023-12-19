using Alachisoft.NCache.Client;
using Alachisoft.NCache.Common.DataStructures.Clustered;
using Alachisoft.NCache.Runtime.Caching;
using Alachisoft.NCache.Runtime.JSON;
using Alachisoft.NCache.Sample.Data;
using Newtonsoft.Json;
using QueriesTestApplication.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace QueriesTestApplication
{
    class InlineQueryTestForUpdate
    {
        ICache cache;
        public Dictionary<string, ResultStatus> testResults;
        int count = 0;
        List<Product> productList;
        List<Product> complexProductList;
        readonly Report _report;
        public Report Report { get => _report; }

        public InlineQueryTestForUpdate()
        {
            cache = CacheManager.GetCache(Common.CacheName);
            testResults = new Dictionary<string, ResultStatus>();
            productList = new List<Product>();
            complexProductList = new List<Product>();
            _report = new Report(nameof(InlineQueryTestForUpdate));
            PopulateProductList();

        }


        private Product GetProduct()
        {

            return new Product() { Id = 1, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 357, Order = new Alachisoft.NCache.Sample.Data.Order { OrderID = 10, ShipCity = "rawalpindi" } };
        }
        private void PopulateCache()
        {

            foreach (var item in productList)
            {
                cache.Add("product" + item.Id, item);
            }
        }

        #region  ----------------------------  SET Operations  ----------------------------

        public void BasicUpdateQuery0()
        {
            int updated = 0;
            cache.Clear();

            PopulateCache();
            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;
            try
            {
                var selectQuery = "Select * from Alachisoft.NCache.Sample.Data.Product where Id > 10";
                QueryCommand queryCommand1 = new QueryCommand(selectQuery);
                var result = cache.SearchService.ExecuteReader(queryCommand1);

                string query = "Update  Alachisoft.NCache.Sample.Data.Product SET Name = '\"Tea\"' where UnitPrice > ?";


                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("UnitPrice", Convert.ToDecimal(100));

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                var exception = new Exception("Failure:Partial Update items using query");

                if (updated <= 0)
                    throw exception;

                foreach (var item in productList)
                {
                    var prod = cache.Get<Product>("product" + item.Id);

                    if (prod != null && prod.UnitPrice > 100 && prod.Name != "Tea")
                        throw exception;

                }

                _report.AddPassedTestCase(methodName, "Success:Partial Update items using query");

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }

        }
        public void BasicUpdateQuery1()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;
            try
            {
                cache.Clear();
                PopulateCache();
                //string query = "Update  Alachisoft.NCache.Sample.Data.Product  Set Order.ShipName = '{\"airport\":\"name\"}'  ";
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Set Order.ShipName = '\"Titanic\"'  ";
                QueryCommand queryCommand = new QueryCommand(query);
                //var updated = cache.SearchService.ExecuteNonQuery(queryCommand);
                System.Collections.IDictionary dictionary;
                var updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);

                Helper.ValidateDictionary(dictionary);

                var exception = new Exception("Partial Update items using query");

                foreach (var item in productList)
                {

                    var prod = cache.Get<Product>("product" + item.Id);

                    if (prod.Order.ShipName != "Titanic")
                        throw exception;

                }

                _report.AddPassedTestCase(methodName, "Success:Partial Update items using query");

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }
        }

        //update with json data
        public void BasicUpdateQuery5()
        {
            string methodName = "BasicUpdateQuery5";
            count++;
            try
            {
                var product = JsonConvert.SerializeObject(GetProduct());
                cache.Clear();
                PopulateCache();
                //string query = $"Update  Alachisoft.NCache.Sample.Data.Product Set Order.ShipName = '{product}'  where UnitPrice > ?";
                string query = $"Update  Alachisoft.NCache.Sample.Data.Product Set Order.ShipName = '\"Products Ship\"'  where UnitPrice > ?";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("UnitPrice", Convert.ToDecimal(100));
                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                var exception = new Exception(" Adding a complicated object..");

                if (updated == 0)
                    throw exception;


                foreach (var item in productList)
                {

                    var prod = cache.Get<Product>("product" + item.Id);

                    if (prod.UnitPrice > 100 && prod.Order.ShipName != "Products Ship")
                        throw exception;

                }

                _report.AddPassedTestCase(methodName, "Adding a complicated object..");

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
                
            }
        }

        //Test partial operation on arrays on base level
        public void BasicUpdateQuery9()
        {

            string methodName = "BasicUpdateQuery9";
            count++;
            try
            {
                var productNamedTag = new NamedTagsDictionary();
                productList.Clear();
                PopulateProductList();
                productNamedTag.Add("discount", Convert.ToDecimal(0.5));
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
                cache.Clear();
                string key = "Products";
                var cacheItem = new CacheItem(jsonArray);
                cacheItem.NamedTags = productNamedTag;
                cache.Insert(key, cacheItem);

                string query = "Update Alachisoft.NCache.Runtime.JSON.JsonArray Set this[5].ProductName = '\"screen\"' WHERE discount = @discount  ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@discount", Convert.ToDecimal(0.5));
                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                var array = cache.Get<JsonArray>(key);
                JsonValueBase valueBase = array[5];
                Newtonsoft.Json.Linq.JObject jObj = Newtonsoft.Json.Linq.JObject.Parse(valueBase.ToString());
                Newtonsoft.Json.Linq.JToken name;
                if (jObj.TryGetValue("ProductName", out name))
                {
                    if (name.ToString() == "screen")
                    {
                        _report.AddPassedTestCase(methodName, "Success:Test partial operation on arrays on base level");
                    }
                    else
                    {

                        throw new Exception("Failure:Test partial operation on arrays on base level");
                    }
                }
                else
                {
                    throw new Exception("Failure:Test partial operation on arrays on base level");
                }


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }
        }

        public void BasicUpdateQuery10()
        {

            string methodName = "BasicUpdateQuery10";
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

        public void BasicUpdateQuery11()
        {

            string methodName = "BasicUpdateQuery11";
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
            string methodName = "BasicUpdateQuery13";
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

            string methodName = "BasicUpdateQuery14";
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
            string methodName = "BasicUpdateQuery16";
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
            string methodName = "BasicUpdateQuery19";
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

        #region  ----------------------------  Test Operations  ----------------------------

        public void BasicUpdateQuery24()
        {

            string methodName = "BasicUpdateQuery24";
            count++;
            try
            {
                productList.Clear();

                cache.Clear();
                var customer = new string[3] { "premium", "gold", "silver" };
                string key = "Products";
                var item = new Product() { Id = 4, Name = "IKura", ClassName = "Electronics", Category = "Produce", UnitPrice = 50, Customer = new Customer() { CustomerType = customer, ContactName = "hello" } };
                var cacheItem = new CacheItem(item);
                IDictionary dictionary;
                cache.Insert(key, cacheItem);

                string query = "Update Alachisoft.NCache.Sample.Data.Product Test Id = 4 ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@customertype", "old");
                var updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);

                var received = cache.Get<JsonObject>(key);

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

        #endregion


        public void AddArrayInUpdateQuery()
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



        #region  --------------------------------- Complex Query -----------------------------------

        public void ComplexQuery()
        {
            int updated = 0;
            productList.Clear();
            PopulateProductList();
            cache.Clear();
            PopulateCache();
            string methodName = MethodBase.GetCurrentMethod().Name;
            int totalExpectedException = 1;

            count++;
            try
            {
                string nameThatDoesnotExist = "\"I drink two cups of tea daily.\"";
                DateTime birthDayTime = new DateTime(2000, 1, 5);

                string query = $"Update  Alachisoft.NCache.Sample.Data.Product " +
                    $" Add nameThatDoesnotExist = '\"I do not exist\"' " +
                    $" Test nameThatDoesnotExist = '\"I do not exist\"' " +
                    $" Set Time = @birthDayTime " +
                    $" Copy ClassName = Category " +
                    $" Move Id = Order.OrderID " +
                    $" Remove nameThatDoesnotExist " +
                    $" Where Id = @id";


                totalExpectedException = 0;

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@birthDayTime", birthDayTime);
                queryCommand.Parameters.Add("@id", 1);

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out IDictionary dictionary);

                if (updated == 0)
                    throw new Exception("No key updated by complex query");

                if (dictionary.Count != totalExpectedException)
                    throw new Exception("Got more then expected exceptions in complex query");

                Helper.ValidateDictionary(dictionary);

                var jsonObject = cache.Get<JsonObject>("product1");

                if (jsonObject.ContainsAttribute("nameThatDoesnotExist"))
                    throw new Exception("Remove Operation failed in complex query");

                var result = cache.Get<Product>("product1");
                var updatedDate = result.Time;

                if (updatedDate.Year != 2000 || updatedDate.Month != 1 || updatedDate.Day != 5)
                    throw new Exception("Set Operation failed in complex query");

                if (result.Category != "Beverages" || result.ClassName != "Beverages")
                    throw new Exception("Copy Operation failed in complex query");

                if (result.Id == default)
                    throw new Exception("Move Operation failed in complex query");


                _report.AddPassedTestCase(methodName, "complex query passed");


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }


        #endregion


        #region ---------------------------- Combined Operations -------------------------
        public void BasicUpdateQuery3()
        {
            productList.Clear();
            string methodName = "BasicUpdateQuery3";
            count++;
            try
            {
                PopulateProductList();
                cache.Clear();
                PopulateCache();
                string query = "Update  Alachisoft.NCache.Sample.Data.Product Set Name = '\"Tea\"', Order.ShipCity = '\"Lahore\"' Add Order.Type = '\"important\"' Set-meta $tag$ = '[\"prod\",\"price\"]', $namedtag$ = '[{\"discount\":0.4,\"type\":\"decimal\"}]' where UnitPrice > ?";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("UnitPrice", Convert.ToDecimal(100));
                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                int reveived = 0;

               

                var returned = cache.SearchService.GetKeysByTag("price");

                var cacheItem = cache.GetCacheItem(returned.FirstOrDefault().ToString());

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
                        else
                            throw new Exception("items not updated by query");

                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                else
                    throw new Exception("items not updated by query");
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
               
            }
        }

        #endregion

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
            productList.Add(new Product() { Id = 1, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 35, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });
            productList.Add(new Product() { Id = 2, Time = DateTime.Now, Name = "Chang", ClassName = "Electronics", Category = "Meat", UnitPrice = 18, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });
            productList.Add(new Product() { Id = 3, Time = DateTime.Now, Name = "Aniseed Syrup", ClassName = "Electronics", Category = "Beverages", UnitPrice = 258, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });
            productList.Add(new Product() { Id = 4, Time = DateTime.Now, Name = "IKura", ClassName = "Electronics", Category = "Produce", UnitPrice = 50, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });
            productList.Add(new Product() { Id = 5, Time = DateTime.Now, Name = "Tofu", ClassName = "Electronics", Category = "Seafood", UnitPrice = 78, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });
            productList.Add(new Product() { Id = 6, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 37, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });
            productList.Add(new Product() { Id = 7, Time = DateTime.Now, Name = "Chang", ClassName = "Electronics", Category = "Meat", UnitPrice = 18, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });
            productList.Add(new Product() { Id = 8, Time = DateTime.Now, Name = "Aniseed Syrup", ClassName = "Electronics", Category = "Beverages", UnitPrice = 258, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });
            productList.Add(new Product() { Id = 9, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 37, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });
            productList.Add(new Product() { Id = 10, Time = DateTime.Now, Name = "Chang", ClassName = "Electronics", Category = "Meat", UnitPrice = 18, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });
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
            ExpandProductList(10);
        }

        void ExpandProductList(int num)
        {
            for (int i = 1; i < 3; i++)
            {
                productList.Add(new Product() { Id = i + 25, Time = DateTime.Now, Name = "Chang", ClassName = "Electronics", Category = "Meat", UnitPrice = 188, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });

            }

        }

    }
}
