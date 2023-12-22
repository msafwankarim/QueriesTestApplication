using Alachisoft.NCache.Client;
using Alachisoft.NCache.Common.Net;
using Alachisoft.NCache.Runtime.JSON;
using Alachisoft.NCache.Sample.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace QueriesTestApplication
{
    class InlineQueryTestsForInsertUpsert
    {
        private int count = 0;
        ICache cache;
        public Dictionary<string, ResultStatus> testResults;
        List<Product> productList;
        public InlineQueryTestsForInsertUpsert()
        {
            cache = CacheManager.GetCache(Common.CacheName);
            testResults = new Dictionary<string, ResultStatus>();
            productList = new List<Product>();
        }

        public Dictionary<string, ResultStatus> TestResults
        {
            get { return testResults; }
        }

        /// <summary>
        /// Adds an inline JSON array in cache. Then Gets it back as JSON array
        /// </summary>
        public void AddJsonArrayInline()
        {

            var methodName = MethodBase.GetCurrentMethod().Name;
            try
            {
                cache.Clear();
                string key = "abc";

                string insertQuery = "Insert into AnyClass (Key,Value) "
                                     + "Values ('abc','[\"ABC\",\"DEF\",\"GHT\"]')";

                QueryCommand queryCommand = new QueryCommand(insertQuery);
                var insertresult = cache.SearchService.ExecuteNonQuery(queryCommand);


                var returned = new JsonArray(cache.Get<string>(key));

                if (returned != null && returned[0].Value.ToString() == "ABC")
                {
                    Console.WriteLine("Success: Add JSON Array with inline data");
                    testResults.Add(methodName, ResultStatus.Success);
                }
                else
                {
                    throw new Exception("Failure: Add JSON with inline data");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failure: Add JSON with inline data");
                testResults.Add(methodName, ResultStatus.Failure);
            }
        }

        /// <summary>
        /// Adds a JsonString in cache to verify JSON String DataType
        /// </summary>
        public void AddJsonStringInline()
        {

            var methodName = "AddJsonStringInline";
            try
            {

                cache.Clear();
                string key = "abc";

                string insertQuery = "Insert into AnyClass (Key,Value) "
                                     + "Values ('abc','\"Aqib Naveed\"')";

                QueryCommand queryCommand = new QueryCommand(insertQuery);
                var insertresult = cache.SearchService.ExecuteNonQuery(queryCommand);

                var returned = cache.Get<object>(key);

                if (returned is string)
                {
                    Console.WriteLine("Success: Add JSON string with inline data");
                    testResults.Add(methodName, ResultStatus.Success);
                }
                else
                {
                    Console.WriteLine("Failure: Add string with inline data");
                    testResults.Add(methodName, ResultStatus.Failure);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failure: Add JSON string with inline data");
                testResults.Add(methodName, ResultStatus.Failure);
            }
        }
        /// <summary>
        ///  Add JSON Object having all datatypes in cache by inline
        /// </summary>
        public void VerifyJsonDatatypesInline()
        {

            var methodName = "VerifyJsonDatatypesInline";
            try
            {
                cache.Clear();
                string key = "abc";

                string insertQuery = "Upsert into AnyClass (Key,Value) " +
                    "Values ('abc'," +
                    "'{\"Name\":\"Chai\"," +
                    "\"Id\":20," +
                    "\"Category\":null," +
                    "\"Expirable\":false," +
                    "\"Order\":{\"OrderID\":20}," +
                    "\"Manufacturers\":[\"Alachisoft\"]}')";

                QueryCommand queryCommand = new QueryCommand(insertQuery);
                var insertresult = cache.SearchService.ExecuteNonQuery(queryCommand);

                var returned = new JsonObject(cache.Get<string>(key));

                if (returned != null)
                {
                    Console.WriteLine("Success: Add JSON object having all Datatypes with inline data");
                    testResults.Add(methodName, ResultStatus.Success);
                }
                else
                {
                    Console.WriteLine("Failure: Success: Add JSON object having all Datatypes with inline data");
                    testResults.Add(methodName, ResultStatus.Failure);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failure: Success: Add JSON object having all Datatypes with inline data");
                testResults.Add(methodName, ResultStatus.Failure);
            }

        }

        //public void Add1(String query)
        //{
        //    var methodName = "Add3";
        //    try
        //    {
        //        cache.Clear();
        //        string key = "abc";
        //        string value = "{ 'airport': {'airportname': 'Heathrow', 'city': 'London', 'country'': 'United Kingdom'}}";
        //        string insertQuery = query;// $"Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value) Values (abc, {value})";
        //        QueryCommand queryCommand = new QueryCommand(insertQuery);
        //        var insertresult = cache.SearchService.ExecuteNonQuery(queryCommand);

        //        var returned = cache.Get<object>(key);
        //        if (returned != null)
        //            Console.WriteLine("Success: Add a key-value pair with inline data");
        //        testResults.Add(methodName, ResultStatus.Success);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Failure: Add a key-value pair with Empty string value");
        //        testResults.Add(methodName, ResultStatus.Failure);
        //    }

        //}


        //--- Add a key-value pair 

        /// <summary>
        /// Inserts a String having \\ slashes in cache
        /// </summary>
        public void Add1()
        {
            var methodName = "Add1";
            try
            {
                cache.Clear();
                string key = "abc";
                string insertQuery = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value) Values ('abc', '\"F:\\\\Indices - a\"')";
                QueryCommand queryCommand = new QueryCommand(insertQuery);
                var insertresult = cache.SearchService.ExecuteNonQuery(queryCommand);

                var returned = cache.Get<object>(key);
                if (returned != null)
                    Console.WriteLine("Success: Add a key-value pair with inline data");
                testResults.Add(methodName, ResultStatus.Success);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failure: Add a key-value pair with Empty string value");
                testResults.Add(methodName, ResultStatus.Failure);
            }

        }
        //--- Add through json serialization
        public void Add2()
        {
            var methodName = "Add2";
            try
            {
                cache.Clear();
                Employee employee = new Employee
                {
                    FirstName = "James",
                    LastName = "Newton-King",
                    Roles = new List<string>
                    {
                      "Admin"
                    }
                };
                var val = JsonConvert.SerializeObject(employee);

                val = JsonConvert.SerializeObject(GetProduct());
                string key = "abc";
                string insertQuery = $"Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value) Values ('abc', '{val}')";
                QueryCommand queryCommand = new QueryCommand(insertQuery);
                var insertresult = cache.SearchService.ExecuteNonQuery(queryCommand);

                var returned = cache.Get<object>(key);
                if (returned != null)
                    Console.WriteLine("Success: Add primitime json data");
                testResults.Add(methodName, ResultStatus.Success);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failure: Add primitime json data");
                testResults.Add(methodName, ResultStatus.Failure);
            }

        }

        //--- Add primitime json data array
        public void Add3()
        {
            var methodName = "Add3";
            try
            {
                cache.Clear();
                string key = "abc";
                string insertQuery = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value) Values ('abc', '[\"name\",\"class\"]')";
                QueryCommand queryCommand = new QueryCommand(insertQuery);
                var insertresult = cache.SearchService.ExecuteNonQuery(queryCommand);

                var returned = cache.Get<object>(key);
                if (returned != null)
                    Console.WriteLine("Success: Add primitime json data");
                testResults.Add(methodName, ResultStatus.Success);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failure: Add primitime json data");
                testResults.Add(methodName, ResultStatus.Failure);
            }

        }

        // add string in inline query
        public void Add4()
        {
            var methodName = "Add4";
            try
            {
                cache.Clear();
                var insertquery = "Insert into Alachisoft.Ncache.Sample.Data.Product (key,value) values ('product1','\"bottle\"')";
                QueryCommand qc = new QueryCommand(insertquery);
                var insResult = cache.SearchService.ExecuteNonQuery(qc);

                if (insResult > 0)
                {
                    try
                    {
                        var upsertquery = "Upsert into Alachisoft.Ncache.Sample.Data.Product (key,value) values ('product1','\"bottle\"')";
                        qc = new QueryCommand(upsertquery);
                        var upsertResult = cache.SearchService.ExecuteNonQuery(qc);
                        if (upsertResult > 0)
                        {

                            var result = cache.Get<string>("product1");
                            if (result == "bottle")
                            {
                                testResults.Add(methodName, ResultStatus.Success);
                                Console.WriteLine("Successful:run an inline insert query and then on same key run upsert key, then verify updated item is returned. ");
                                count++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                testResults.Add(methodName, ResultStatus.Failure);
                Console.WriteLine("Failure:run an inline insert query and then on same key run upsert key, then verify updated item is returned. ");
            }

        }
        public void VerifyMetaData()
        {
            count++;
            var methodName = "VerifyMetaData";
            try
            {
                cache.Clear();
                var key = GetKey();
                var val = JsonConvert.SerializeObject(GetProduct());
                var data = "{\"tags\":[\"price\",\"sale\"], \"namedtags\":[ {\"discount\":0.5,\"type\":\"decimal\"}, { \"sale\":\"offer\",\"type\":\"string\"}]}";
                string query = $"Insert Into Alachisoft.Ncache.Sample.Data.Product (key,value,meta) values ('{key}','{val}','{data}')";
                QueryCommand qc = new QueryCommand(query);
                var result = cache.SearchService.ExecuteNonQuery(qc);
                var item = cache.GetCacheItem(key);
                var tags = item.Tags;
                var namedTags = item.NamedTags;

                for (int i = 0; i < tags.Length; i++)
                {
                    if (tags[i].TagName == "price" || tags[i].TagName == "sale")
                        continue;
                    else
                        throw new Exception("Invalid tag found");

                }
                if (namedTags.Count == 2)
                {
                    if (namedTags.Contains("discount") && namedTags.Contains("sale"))
                    {
                        Console.WriteLine("Success:Verify added tags and named tags through cache item");
                        testResults.Add(methodName, ResultStatus.Success);
                    }
                }
                else
                {
                    testResults.Add(methodName, ResultStatus.Failure);
                    Console.WriteLine("Failure: Verify added tags and named tags through cache item ");
                }

            }
            catch (Exception ex)
            {
                testResults.Add(methodName, ResultStatus.Failure);
                Console.WriteLine("Failure: Verify added tags and named tags through cache item ");
            }

        }
        public void VerifyMetaData2()
        {
            count++;
            int reveived = 0;
            var methodName = "VerifyMetaData2";
            try
            {
                cache.Clear();
                var prodDict = GetDictionaryOfProductsToAdd(100);
                List<string> namedTagggedKeys = new List<string>();


                for (int i = 0; i < 100; i++)
                {
                    var key = GetKey() + i;
                    var val = JsonConvert.SerializeObject(prodDict[key]);
                    if (i % 2 == 0)
                    {
                        var data = "{\"namedtags\":[{\"FlashSaleDiscount\":0.5,\"type\":\"decimal\"}]}";
                        string query = $"Insert Into Alachisoft.Ncache.Sample.Data.Product (key,value,meta) values ('{key}','{val}','{data}')";
                        QueryCommand qc = new QueryCommand(query);
                        var result = cache.SearchService.ExecuteNonQuery(qc);
                    }
                    else
                    {
                        string query = $"Insert Into Alachisoft.Ncache.Sample.Data.Product (key,value) values ('{key}','{val}')";
                        QueryCommand qc = new QueryCommand(query);
                        var result = cache.SearchService.ExecuteNonQuery(qc);
                    }

                }


                string searchQuery = "SELECT $Value$ FROM Alachisoft.NCache.Sample.Data.Product WHERE FlashSaleDiscount = @discount ";
                QueryCommand searchQueryCommand = new QueryCommand(searchQuery);
                searchQueryCommand.Parameters.Add("@discount", Convert.ToDecimal(0.5));
                ICacheReader reader = cache.SearchService.ExecuteReader(searchQueryCommand);

                if (reader.FieldCount > 0)
                {
                    while (reader.Read())
                    {
                        reveived++;
                        Product result = reader.GetValue<Product>(1);
                    }
                    if (reveived == namedTagggedKeys.Count)
                    {
                        testResults.Add(methodName, ResultStatus.Success);
                        Console.WriteLine("Success: verify meta by adding with named tags and then getting through those search.");
                    }
                }
            }
            catch (Exception ex)
            {
                testResults.Add(methodName, ResultStatus.Failure);
                Console.WriteLine($"Failure: verify meta by adding with tags and then getting through those tags.  => {ex.Message}");
            }
        }

        public void AllMetadataInSingleQuotes()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;

            cache.Clear();

            cache.Insert("masterKey0","masterKey0");
            cache.Insert("masterKey1","masterKey1");

            string key = "myKey";
            string meta = @" 
                                    {       /*

                                                  This is valid json string that contins all metadata supported
                                            
                                            */
                                        Priority    :   'Low'          ,  // key is without quotes
                                        'group'       :   'DevTeam'      ,
                                        'tags'        :   ['Important Product', 'Imported Product'] ,
                                        'namedtags'   :   [
                                                            {
                                                                'type' : 'long' ,
                                                                'discont' : 70
                                                            },
                        
                                                            {
                                                                'type'     : 'string' ,
                                                                'catagory' : 'Product'
                                                            },   // trailing comma supported
                                                          ],
                      
                      
                                        'dependency' :  [
                                                            {
                                                                'key' : {
                                                                            'keys' : ['masterKey0','masterKey1'],                           
                                                                            'type' : 'removeonly'                           
                                                                        }
                                                            },
                        
                                                            {
                                                                'file' : {
                                                                             'fileNames' : ['C:\\dependencyFile.txt'],                           
                                                                             'interval'  :  10                           
                                                                         }
                                                            }
                                                        ],

                                    'resyncOptions' :  {
							                                    'ResyncOnExpiration' : 'true' ,
							                                    'providerName'       : 'read'
					                                    },

                                       'expiration' :  {
							                                    'type'     : 'sliding' ,
							                                    'interval' :  10
				                                       }  

                                    }
                                  ";

            try
            {
                var insertquery = "Insert into Alachisoft.NCache.Sample.Data.Product (key,value,meta) values ('myKey','\"bottle\"', @metadata)";

                QueryCommand qc = new QueryCommand(insertquery);
                qc.Parameters.Add("@metadata", meta);
                var insResult = cache.SearchService.ExecuteNonQuery(qc);

                if (insResult == 0)
                    throw new System.Exception("no item updated by query");

                var cacheItem = cache.GetCacheItem(key);

                if (cacheItem.Priority.ToString() != "Low")
                    throw new System.Exception("priority not updated");


                if (cacheItem.Tags.Length != 2 || !(cacheItem.Tags[0].TagName == "Imported Product" || cacheItem.Tags[1].TagName == "Imported Product"))
                    throw new System.Exception("tag not updated");


                if (cacheItem.Dependency.Dependencies.Count != 2)
                    throw new System.Exception("dependency not updated");

                if (cacheItem.ResyncOptions.ProviderName != "read")
                    throw new System.Exception("resync provider not updated");

                if (!cacheItem.Expiration.Type.ToString().Contains("Sliding"))
                    throw new System.Exception("priority not updated");


                testResults.Add(methodName, ResultStatus.Success);


            }
            catch (System.Exception ex)
            {

                testResults.Add(methodName, ResultStatus.Failure);
                Console.WriteLine($"Failure: add all metadata in single quotes. ex => {ex.Message}");
            }
        }


        private string GetKey()
        {
            Random rnd = new Random();
            return "CacheKey_1";
        }

        private Product GetProduct()
        {

            return new Product() { Id = 1, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 357, Order = new Alachisoft.NCache.Sample.Data.Order { OrderID = 10, ShipCity = "rawalpindi" } };
        }

        private Dictionary<string, Product> GetDictionaryOfProductsToAdd(int number)
        {
            var productDict = new Dictionary<string, Product>();
            for (int i = 0; i < number; i++)
            {
                productDict.Add(GetKey() + i, GetProduct());

            }
            return productDict;
        }

    }
    public class Employee
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IList<string> Roles { get; set; }
    }
}
