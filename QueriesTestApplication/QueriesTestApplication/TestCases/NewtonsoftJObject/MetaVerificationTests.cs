using Alachisoft.NCache.Client;
using Alachisoft.NCache.Common.Queries;
using Alachisoft.NCache.Runtime.Caching;
using Alachisoft.NCache.Runtime.Dependencies;
using Alachisoft.NCache.Runtime.JSON;
using Alachisoft.NCache.Sample.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Alachisoft.NCache.Runtime;
using System.Data.SqlClient;
using QueriesTestApplication.Utils;
using Alachisoft.NCache.Common.DataStructures.Clustered;

namespace QueriesTestApplication
{
    class MetaVerificationTests
    {

        private int count = 0;
        ICache cache;
        public Dictionary<string, ResultStatus> testResults;
        private Report _report;

        public Dictionary<string, ResultStatus> TestResults
        {
            get { return testResults; }
        }

        public Report Reprt { get => _report; }

        public MetaVerificationTests()
        {            
            cache = CacheManager.GetCache(Common.CacheName);
            testResults = new Dictionary<string, ResultStatus>();
            _report = new Report(nameof(MetaVerificationTests));

        }
        

        // Left Cases

        //public Expiration Expiration { get; set; }          
        //public CacheItemVersion Version { get; set; }
        //public ResyncOptions ResyncOptions { get; set; }                 
        //public CacheSyncDependency SyncDependency { get; set; }        
        //public CacheDependency Dependency { get; set; }

        #region ------------------------- Group Metadata ------------------------- 

        /// <summary>
        /// Adds JObject as MetaData having  information of Group
        /// </summary>
        public void GroupMetadataInJObject()
        {
            var methodName = "GroupMetadataInJObject";
            try
            {
                cache.Clear();

                string JsonString = "{'group':'DevTeam'}";
                //var MetaData = JObject.Parse(JsonString);
                JsonObject MetaData = JsonObject.Parse(JsonString) as JsonObject;


                string key1 = "key_GroupMetadataInJObject";
                var val = GetProduct();

                string query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                QueryCommand queryCommand = new QueryCommand(query);

                queryCommand.Parameters.Add("@key1", key1);
                queryCommand.Parameters.Add("@val", val);
                queryCommand.Parameters.Add("@metadata", MetaData);

                var result = cache.SearchService.ExecuteNonQuery(queryCommand);

                var cacheItem = cache.GetCacheItem(key1);

                var GroupAttribute = (JsonValue)cacheItem.Group;

                if (GroupAttribute.Value.ToString() == "DevTeam")                
                    _report.AddPassedTestCase(methodName, "Success: Add Group Metadata In JObject ");                  
                
                else                
                    throw new Exception("Failure:  Add Group Metadata In JObject");

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);               
            }

        }

        #endregion


        #region ------------------------- Priority Metadata -------------------------

        /// <summary>
        /// Adds JObject as MetaData having  information of Priority
        /// </summary>
        public void PriorityMetadataInJObject()
        {
            var methodName = "PriorityMetadataInJObject";
            try
            {
                cache.Clear();

                string JsonString = "{'priority':'AboveNormal'}";
                //var MetaData = JObject.Parse(JsonString);
                JsonObject MetaData = JsonObject.Parse(JsonString) as JsonObject;

                string key1 = "key_PriorityMetadataInJObject";
                var val = GetProduct();

                string query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                QueryCommand queryCommand = new QueryCommand(query);

                queryCommand.Parameters.Add("@key1", key1);
                queryCommand.Parameters.Add("@val", val);
                queryCommand.Parameters.Add("@metadata", MetaData);

                var result = cache.SearchService.ExecuteNonQuery(queryCommand);

                var cacheItem = cache.GetCacheItem(key1);

                var PriorityAttribute = cacheItem.Priority;
                
                if (PriorityAttribute==CacheItemPriority.AboveNormal)
                {
                    _report.AddPassedTestCase(methodName, "Success: Add Priority Metadata In JObject ");
                   
                }
                else
                {
                    throw new Exception("Failure:  Add Priority Metadata In JObject");
                    
                }

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
               
            }

        }

        #endregion


        #region ------------------------- Db Dependency -------------------------

        /// <summary>
        ///  Adds SQL DB Dependency in MetaData 
        /// </summary>
        /// <remarks>
        /// Needs DB Credentials i.e ConnectionString and Query
        /// </remarks>
        private void VerifyDbDependency()
        {
            string methodName = "VerifyDbDependency";
            string Itemkey = "SqlDBDependency";
            Product item = GetProduct();

            try
            {
                string AddItemWithDependencyQuery = "INSERT INTO Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                //var JsonForDependency = @"{'dependency': [{'sql': {'connectionstring': 'Data Source=AQIB-NAVEED;Initial Catalog=Northwind;Integrated Security=True;','querystring': 'SELECT CustomerID, Address, City FROM dbo.Customers;'}}]}";
                var JsonForDependency = "{\"dependency\": [{\"sql\": {\"connectionstring\": \"Data Source=AQIB-NAVEED;Initial Catalog=Northwind;Integrated Security=True;\",\"querystring\": \"SELECT CustomerID, Address, City FROM dbo.Customers;\"}}]}";

                QueryCommand queryCommand = new QueryCommand(AddItemWithDependencyQuery);

                queryCommand.Parameters.Add("@key1", Itemkey);
                queryCommand.Parameters.Add("@val", item);
                queryCommand.Parameters.Add("@metadata", JsonForDependency);

                var result = cache.SearchService.ExecuteNonQuery(queryCommand);

                var cacheItem = cache.GetCacheItem(Itemkey);

                // Explicitly make some change in DB
                
                string connetionString = @"Data Source=AQIB-NAVEED;Initial Catalog=Northwind;Integrated Security=True;";
                SqlConnection cnn = new SqlConnection(connetionString);                
                cnn.Open();

                Random random = new Random ();
                int street = random.Next(1,1000);

                string UpdateQuery = "UPDATE [Northwind].[dbo].[Customers]";
                      UpdateQuery += "SET Address = 'Obere Str."+street+"'";
                      UpdateQuery += "WHERE CustomerID = 'ALFKI' ";

                //Console.WriteLine("Query is "+ UpdateQuery);

                SqlCommand command;
                SqlDataAdapter adapter = new SqlDataAdapter();

                

                command = new SqlCommand(UpdateQuery, cnn);
                adapter.UpdateCommand = command;
                adapter.UpdateCommand.ExecuteNonQuery();


                // After changing in Db
                var returned = cache.Get<Alachisoft.NCache.Sample.Data.Product>(Itemkey);
                if (returned == null)
                {
                    _report.AddPassedTestCase(methodName, "Success: Add SQL DB Dependency ");
                                   
                }
                else
                {
                    throw new Exception("Failed: Add SQL DB Dependency ");
                }
                
            }
            catch (Exception ex) 
            {
                _report.AddFailedTestCase(methodName, ex);

            }



        }

        #endregion


        #region ------------------------- Custom Dependency -------------------------

        /// <summary>
        /// Adds an Item in cache with custom dependency.
        /// Then verifies if the item is still in cache or not after 25 seconds
        /// </summary>
        /// <remarks>
        /// The custom dependecy configured is that the  item expires after 10 seconds.
        /// (i.e hasChanged returns true after 10 seconds)
        /// </remarks>
        private void VerifyCustomDependency()
        {
            string methodName = "VerifyCustomDependency";
            cache.Clear();
            string Itemkey = "KeyForCustomDependency";
            Product item = GetProduct();
            string ProviderName = "custom";


            try
            {
                string AddItemWithDependencyQuery = "INSERT INTO Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";
                               
                var JsonForDependency = "{\"dependency\": [{\"custom\":" +
                    " {\"providername\": \""+ProviderName +"\"," +
                    "\"param\":{\"id\":\"101\"," +
                    "\"connectionstring\":\"NoConnectionStringForTesting\"}}}]}";
              

                QueryCommand queryCommand = new QueryCommand(AddItemWithDependencyQuery);
                queryCommand.Parameters.Add("@key1", Itemkey);
                queryCommand.Parameters.Add("@val", item);
                queryCommand.Parameters.Add("@metadata", JsonForDependency);

                var result = cache.SearchService.ExecuteNonQuery(queryCommand);

                Console.WriteLine("Sleeping for 5 seconds to verify Custom dependency");
                Thread.Sleep(5
                    );

                var cacheItem = cache.GetCacheItem(Itemkey);
                if (cacheItem == null) 
                    _report.AddPassedTestCase(methodName, "Success: Add Custom Dependency ");
                
                else 
                    throw new Exception("Failure: Add Custom Dependency ");                              
                              
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }



        }

        #endregion


        #region ------------------------- Tags -------------------------

        /// <summary>
        /// Adds JObject as MetaData having  information of Tags
        /// </summary>
        public void TagsMetadataInJObject()
          {
            var methodName = "TagsMetadataInJObject";
            try
            {
                cache.Clear();

                string JsonString = "{'tags':['Important Product','Imported Product']}";
                //var MetaData = JObject.Parse(JsonString);
                JsonObject MetaData = JsonObject.Parse(JsonString) as JsonObject;


                string key1 = "abc";
                var val = GetProduct();

                string query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                QueryCommand queryCommand = new QueryCommand(query);

                queryCommand.Parameters.Add("@key1", key1);
                queryCommand.Parameters.Add("@val", val);
                queryCommand.Parameters.Add("@metadata", MetaData);

                var result = cache.SearchService.ExecuteNonQuery(queryCommand);


                Tag[] SearchTags = new Tag[2] { new Tag("Important Product"), new Tag("Imported Product") };
                IDictionary<string, Product> data = cache.SearchService.GetByTags<Product>(SearchTags, TagSearchOptions.ByAnyTag);

                if (data.Count > 0)
                {
                    _report.AddPassedTestCase(methodName, "Success: Add TagsMetaData in JObject");             
                    
                }
                else
                {
                    throw new Exception("Failure: Add TagsMetaData in JObject");
                }
                
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }

        }

        /// <summary>
        /// Adds JObject as MetaData, having Invalid Info
        /// </summary>
        public void InvalidMetadataInJObject()
        {
            var methodName = "InvalidMetadataInJObject";
            try
            {
                cache.Clear();

                //string JsonString = "{'tags':['Important Product','Imported Product']}"; //Valid Metada

                string InvalidJsonString = "{'tags':['Important Product''Imported Product']}";//No comma 
                string InvalidJsonString1 = "{'tags':['Important Product','Imported Product'";// No Ending Bracket
                string InvalidJsonString2 = "{'tags'Important Product''Imported Product']}"; // missing ':' symbol                         

                string[] InvalidStrings = new string[3] { InvalidJsonString, InvalidJsonString1, InvalidJsonString2 };
                Random rand = new Random();
                string JsonString = InvalidStrings[rand.Next(0, InvalidStrings.Length)];


                // var MetaData = JObject.Parse(JsonString);
                JsonObject MetaData = JsonObject.Parse(JsonString) as JsonObject;


                string key1 = "abc";
                var val = GetProduct();

                string query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                QueryCommand queryCommand = new QueryCommand(query);

                queryCommand.Parameters.Add("@key1", key1);
                queryCommand.Parameters.Add("@val", val);
                queryCommand.Parameters.Add("@metadata", MetaData);

                var result = cache.SearchService.ExecuteNonQuery(queryCommand);

                Tag[] SearchTags = new Tag[2] { new Tag("Important Product"), new Tag("Imported Product") };
                IDictionary<string, Product> data = cache.SearchService.GetByTags<Product>(SearchTags, TagSearchOptions.ByAllTags);

                if (data.Count > 0)
                {
                    throw new Exception("Failure: Add Invalid Metadata in JObject");
                    testResults.Add(methodName, ResultStatus.Failure);
                }
                else
                {
                    _report.AddPassedTestCase(methodName, "Success: Add Invalid Metadata in JObject");
                    testResults.Add(methodName, ResultStatus.Success);
                }

            }
            catch (Exception ex)
            {
                if(ex.Message.Contains("Invalid JSON string provided"))
                    _report.AddPassedTestCase(methodName, "Success: Add Invalid Metadata in JObject");
                else
                   _report.AddFailedTestCase(methodName, ex);                
            }
        }


        //verify meta by adding with tags and then getting through those tags.
        public void VerifyMetaData1()
        {
            count++;
            var methodName = "VerifyMetaData1";
            try
            {
                //cache.Clear();
                //var item = new CacheItem(GetProduct());
                ////item.Tags = new Tag[2];
                //item.Group = "";
                //var nt = new NamedTagsDictionary();
                ////nt.Add("hi", "");
                //item.NamedTags = nt;
                //cache.Insert(GetKey(),item) ;
                cache.Clear();
                var prodDict = GetDictionaryOfProductsToAdd(100);
                List<string> taggedKeys = new List<string>();

                for (int i = 0; i < 100; i++)
                {
                    var key = GetKey() + i;

                    if (i % 2 == 0)
                    {
                        string query = "Insert Into Alachisoft.Ncache.Sample.Data.Product (key,value,meta) values (@cachekey,@val,@data)";
                        QueryCommand qc = new QueryCommand(query);
                        qc.Parameters.Add("@cachekey", key);
                        qc.Parameters.Add("@val", prodDict[key]);
                        taggedKeys.Add(key);
                        qc.Parameters.Add("@data", @"{
                                             'tags':['Important Product','Imported Products']
                                              }");
                        var result = cache.SearchService.ExecuteNonQuery(qc);
                    }
                    else
                    {
                        string query = "Insert Into Alachisoft.Ncache.Sample.Data.Product (key,value) values (@cachekey,@val)";
                        QueryCommand qc = new QueryCommand(query);
                        qc.Parameters.Add("@cachekey", key);
                        qc.Parameters.Add("@val", prodDict[key]);
                        var result = cache.SearchService.ExecuteNonQuery(qc);

                    }
                    //var result = cache.SearchService.ExecuteNonQuery(qc);
                }

                var receivedKeys = cache.SearchService.GetKeysByTag("Important Product");
                if (receivedKeys.Count == taggedKeys.Count)
                {
                    foreach (var k in receivedKeys)
                    {
                        if (receivedKeys.Contains(k))
                        {
                            continue;

                        }
                        else
                        {
                            throw new Exception("recived keys dont contain added key.");
                        }


                    }
                    _report.AddPassedTestCase(methodName, "Success: verify meta by adding with tags and then getting through those tags.");
                }

                throw new Exception("Test case failed");

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);


            }
            // throw;
        }

        #endregion


        #region ------------------------- NamedTags -------------------------

        /// <summary>
        /// Adds JObject having Metadata information of NamesTags 
        /// </summary>
        public void NameTagMetadataInJObject()
        {
            var methodName = "NameTagMetadataInJObject";
            try
            {
                cache.Clear();
                string NamedTags = "{'namedtags': [{'BlackFridayDiscount': 'No','type': 'string'},{'BlackFridayFlashDiscount': 'Yes', 'type': 'string'}]}";
                //JObject Metadata = JObject.Parse(NamedTags);               
                JsonObject Metadata = JsonObject.Parse(NamedTags) as JsonObject;               

                string key1 = "abc";
                var val = GetProduct();

                string query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@key1", key1);
                queryCommand.Parameters.Add("@val", val);
                queryCommand.Parameters.Add("@metadata", Metadata);

                var result = cache.SearchService.ExecuteNonQuery(queryCommand);
                var cacheItem = cache.GetCacheItem(key1);

                var Attributes= cacheItem.NamedTags;
               
                if(Attributes.Contains("BlackFridayDiscount") && Attributes.Contains("BlackFridayDiscount")) {
                    _report.AddPassedTestCase(methodName, "Success: Add NameTagMetaData in JObject");
                    
                }
                else
                {
                    throw new Exception("Failure: Add NameTagMetaData in JObject");
                   
                }
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

                Console.WriteLine("Failure: Add NameTagMetaData in JObject");
                testResults.Add(methodName, ResultStatus.Failure);
            }

        }

        //verify meta by adding with named tags and then getting through those named tags.
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

                    if (i % 2 == 0)
                    {
                        string query = "Insert Into Alachisoft.Ncache.Sample.Data.Product (key,value,meta) values (@cachekey,@val,@data)";
                        QueryCommand qc = new QueryCommand(query);
                        qc.Parameters.Add("@cachekey", key);
                        qc.Parameters.Add("@val", prodDict[key]);
                        namedTagggedKeys.Add(key);
                        qc.Parameters.Add("@data", @"{
                                              'namedtags':[{'FlashSaleDiscount':0.5,'type':'decimal'}],
                                              }");
                        var result = cache.SearchService.ExecuteNonQuery(qc);
                    }
                    else
                    {
                        string query = "Insert Into Alachisoft.Ncache.Sample.Data.Product (key,value) values (@cachekey,@val)";
                        QueryCommand qc = new QueryCommand(query);
                        qc.Parameters.Add("@cachekey", key);
                        qc.Parameters.Add("@val", prodDict[key]);

                        var result = cache.SearchService.ExecuteNonQuery(qc);
                    }

                }

                string searchQuery = "SELECT $Value$ FROM Alachisoft.Ncache.Sample.Data.Product WHERE FlashSaleDiscount = @discount ";
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
                        _report.AddPassedTestCase(methodName, "Success: verify meta by adding with named tags and then getting through those search.");

                    }

                }

                throw new Exception("Test case failed");

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);


            }
            // throw;
        }


        //Add with named tag type 
        public void VerifyMetaData11()
        {
            count++;
            int reveived = 0;
            var methodName = "VerifyMetaData11";
            try
            {
                cache.Clear();
                var prodDict = GetDictionaryOfProductsToAdd(100);
                List<string> namedTagggedKeys = new List<string>();


                for (int i = 0; i < 100; i++)
                {
                    var key = GetKey() + i;

                    if (i % 2 == 0)
                    {
                        string query = "Insert Into Alachisoft.Ncache.Sample.Data.Product (key,value,meta) values (@cachekey,@val,@data)";
                        QueryCommand qc = new QueryCommand(query);
                        qc.Parameters.Add("@cachekey", key);
                        qc.Parameters.Add("@val", prodDict[key]);
                        namedTagggedKeys.Add(key);
                        qc.Parameters.Add("@data", @"{
                                              'namedtags':[{'important':'high','type':'string'}],
                                              }");
                        var result = cache.SearchService.ExecuteNonQuery(qc);
                    }
                    else
                    {
                        string query = "Insert Into Alachisoft.Ncache.Sample.Data.Product (key,value) values (@cachekey,@val)";
                        QueryCommand qc = new QueryCommand(query);
                        qc.Parameters.Add("@cachekey", key);
                        qc.Parameters.Add("@val", prodDict[key]);

                        var result = cache.SearchService.ExecuteNonQuery(qc);
                    }

                }

                string searchQuery = "SELECT $Value$ FROM Alachisoft.Ncache.Sample.Data.Product WHERE important = @discount ";
                QueryCommand searchQueryCommand = new QueryCommand(searchQuery);
                searchQueryCommand.Parameters.Add("@discount", "high");
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
                        _report.AddPassedTestCase(methodName, "Success: Add with named tag type ");

                    }

                }


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

                testResults.Add(methodName, ResultStatus.Failure);
                Console.WriteLine("Failure: Add with named tag type ");
            }
            // throw;
        }


        //Add with multiple named tag type 
        public void VerifyMetaData12()
        {
            count++;
            int reveived = 0;
            var methodName = "VerifyMetaData12";
            try
            {
                cache.Clear();
                var prodDict = GetDictionaryOfProductsToAdd(100);
                List<string> namedTagggedKeys = new List<string>();


                for (int i = 0; i < 100; i++)
                {
                    var key = GetKey() + i;

                    if (i % 2 == 0)
                    {
                        string query = "Insert Into Alachisoft.Ncache.Sample.Data.Product (key,value,meta) values (@cachekey,@val,@data)";
                        QueryCommand qc = new QueryCommand(query);
                        qc.Parameters.Add("@cachekey", key);
                        qc.Parameters.Add("@val", prodDict[key]);
                        namedTagggedKeys.Add(key);
                        qc.Parameters.Add("@data", @"{
                                              'namedtags':[{'sale111':0.5,'type':'decimal'},{'offer111':5,'type':'int'}],
                                              }");
                        var result = cache.SearchService.ExecuteNonQuery(qc);
                    }
                    else
                    {
                        string query = "Insert Into Alachisoft.Ncache.Sample.Data.Product (key,value) values (@cachekey,@val)";
                        QueryCommand qc = new QueryCommand(query);
                        qc.Parameters.Add("@cachekey", key);
                        qc.Parameters.Add("@val", prodDict[key]);

                        var result = cache.SearchService.ExecuteNonQuery(qc);
                    }

                }

                string searchQuery = "SELECT $Value$ FROM Alachisoft.Ncache.Sample.Data.Product WHERE sale111 = @discount ";
                QueryCommand searchQueryCommand = new QueryCommand(searchQuery);
                searchQueryCommand.Parameters.Add("@discount", Convert.ToDecimal(Convert.ToDecimal(0.5)));
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
                        _report.AddPassedTestCase(methodName, "Success: Add with named tag type ");

                    }

                }


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }
            // throw;
        }


        // add invalid meta in named tags tags
        public void VerifyMetaData13()
        {
            count++;
            int reveived = 0;
            var methodName = "VerifyMetaData13";
            try
            {
                cache.Clear();
                var prodDict = GetDictionaryOfProductsToAdd(100);
                List<string> namedTagggedKeys = new List<string>();


                for (int i = 0; i < 100; i++)
                {
                    var key = GetKey() + i;

                    if (i % 2 == 0)
                    {
                        string query = "Insert Into Alachisoft.Ncache.Sample.Data.Product (key,value,meta) values (@cachekey,@val,@data)";
                        QueryCommand qc = new QueryCommand(query);
                        qc.Parameters.Add("@cachekey", key);
                        qc.Parameters.Add("@val", prodDict[key]);
                        namedTagggedKeys.Add(key);
                        qc.Parameters.Add("@data", @"{
                                              'namedtags':[{'FlashSaleDiscount':0.5,'type':'decimal'},{'FlashSaleDiscount1':54569876541323,'type':'short'}],
                                              }");
                        var result = cache.SearchService.ExecuteNonQuery(qc);
                    }
                    else
                    {
                        string query = "Insert Into Alachisoft.Ncache.Sample.Data.Product (key,value) values (@cachekey,@val)";
                        QueryCommand qc = new QueryCommand(query);
                        qc.Parameters.Add("@cachekey", key);
                        qc.Parameters.Add("@val", prodDict[key]);

                        var result = cache.SearchService.ExecuteNonQuery(qc);
                    }

                }

                string searchQuery = "SELECT $Value$ FROM Alachisoft.Ncache.Sample.Data.Product WHERE FlashSaleDiscount = @discount ";
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
                        throw new Exception("Failure: add invalid meta in named tags tags");
                        // i think exception is expected. this test case needs to be checked
                    }

                }


            }
            catch (Exception ex)
            {
                // i think exception is expected. this test case needs to be checked

                _report.AddFailedTestCase(methodName, ex);


                testResults.Add(methodName, ResultStatus.Success);
                Console.WriteLine("Success: add invalid meta in named tags tags ");
            }
            // throw;
        }


        // add item with named tag then upsert with the same key and different tag it should not be there with old tag.
        public void VerifyMetaData14()
        {
            count++;
            int reveived = 0;
            var methodName = "VerifyMetaData14";
            try
            {
                cache.Clear();
                var nt = new NamedTagsDictionary();
                nt.Add("ProductType", "Important");

                for (int i = 0; i < 100; i++)
                {
                    CacheItem item = new CacheItem(GetProduct());
                    item.NamedTags = nt;
                    cache.Add(GetKey() + i, item);
                }


                string searchQuery = "SELECT $Value$ FROM Alachisoft.NCache.Sample.Data.Product WHERE ProductType = @discount ";
                QueryCommand searchQueryCommand = new QueryCommand(searchQuery);
                searchQueryCommand.Parameters.Add("@discount", "Important");
                ICacheReader reader = cache.SearchService.ExecuteReader(searchQueryCommand);

                if (reader.FieldCount > 0)
                {
                    while (reader.Read())
                    {
                        reveived++;
                    }
                }
                //   List<string> namedTagggedKeys = new List<string>();
                if (reveived == 100)
                {
                    for (int i = 0; i < 100; i++)
                    {
                        var key = GetKey() + i;


                        string query = "Upsert Into Alachisoft.NCache.Sample.Data.Product (key,value,meta) values (@cachekey,@val,@data)";
                        QueryCommand qc = new QueryCommand(query);
                        qc.Parameters.Add("@cachekey", key);
                        qc.Parameters.Add("@val", GetProduct());
                        //      namedTagggedKeys.Add(key);
                        qc.Parameters.Add("@data", @"{
                                              'namedtags':[{'FlashSaleDiscount':0.5,'type':'decimal'}]
                                              }");
                        var result = cache.SearchService.ExecuteNonQuery(qc);
                    }
                }

                searchQuery = "SELECT $Value$ FROM Alachisoft.NCache.Sample.Data.Product WHERE FlashSaleDiscount = @discount ";
                searchQueryCommand = new QueryCommand(searchQuery);
                searchQueryCommand.Parameters.Add("@discount", Convert.ToDecimal(0.5));
                reader = cache.SearchService.ExecuteReader(searchQueryCommand);
                reveived = 0;
                if (reader.FieldCount > 0)
                {
                    while (reader.Read())
                    {
                        reveived++;

                    }
                }
                reveived = 0;
                searchQuery = "SELECT $Value$ FROM Alachisoft.NCache.Sample.Data.Product WHERE ProductType = @discount ";
                searchQueryCommand = new QueryCommand(searchQuery);
                searchQueryCommand.Parameters.Add("@discount", "Important");
                reader = cache.SearchService.ExecuteReader(searchQueryCommand);

                if (reader.FieldCount > 0)
                {
                    while (reader.Read())
                    {
                        reveived++;
                    }
                }

                var citem = cache.GetCacheItem(GetKey() + 4);
                testResults.Add(methodName, ResultStatus.Success);
                _report.AddPassedTestCase(methodName, "Success: add item with named tag then upsert with the same key and different tag it should not be there with old tag. ");

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

                testResults.Add(methodName, ResultStatus.Failure);
                Console.WriteLine("Failure: add item with named tag then upsert with the same key and different tag it should not be there with old tag. ");
            }
            // throw;
        }


        #endregion


        #region ------------------------- Combined Metadata -------------------------


        //verify meta by adding with absolute expiration

        void VerifyMetaData3()
        {
            count++;
            int reveived = 0;
            var methodName = "VerifyMetaData3";
            try
            {
                cache.Clear();
                var prodDict = GetDictionaryOfProductsToAdd(100);
                List<string> namedTagggedKeys = new List<string>();


                for (int i = 0; i < 100; i++)
                {
                    var key = GetKey() + i;


                    string query = "Insert Into Alachisoft.Ncache.Sample.Data.Product (key,value,meta) values (@cachekey,@val,@data)";
                    QueryCommand qc = new QueryCommand(query);
                    qc.Parameters.Add("@cachekey", key);
                    qc.Parameters.Add("@val", prodDict[key]);
                    namedTagggedKeys.Add(key);
                    qc.Parameters.Add("@data", @"{
                                              'namedtags':[{'FlashSaleDiscount':0.5,'type':'decimal'}],
                                               'expiration':{'type':'absolute','interval':'2'}
                                              }");
                    var result = cache.SearchService.ExecuteNonQuery(qc);


                }

                Thread.Sleep(20000);

                long itemsInCache = cache.Count;
                if (itemsInCache > 0)
                {
                    testResults.Add(methodName, ResultStatus.Failure);
                    throw new Exception("Failure: verify meta by adding with absolute expiration");
                }
                else
                {
                    testResults.Add(methodName, ResultStatus.Success);
                    _report.AddPassedTestCase(methodName, "Success: verify meta by adding with absolute expiration");
                }

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

                testResults.Add(methodName, ResultStatus.Failure);
                Console.WriteLine("Failure: verify meta by adding with absolute expiration");
            }
            // throw;
        }

       
        //verify meta by adding with sliding expiration
        void VerifyMetaData4()
        {
            count++;
            int reveived = 0;
            var methodName = "VerifyMetaData4";
            try
            {
                cache.Clear();
                var prodDict = GetDictionaryOfProductsToAdd(100);
                List<string> namedTagggedKeys = new List<string>();


                for (int i = 0; i < 100; i++)
                {
                    var key = GetKey() + i;


                    string query = "Insert Into Alachisoft.Ncache.Sample.Data.Product (key,value,meta) values (@cachekey,@val,@data)";
                    QueryCommand qc = new QueryCommand(query);
                    qc.Parameters.Add("@cachekey", key);
                    qc.Parameters.Add("@val", prodDict[key]);
                    namedTagggedKeys.Add(key);
                    qc.Parameters.Add("@data", @"{
                                              'namedtags':[{'FlashSaleDiscount':0.5,'type':'decimal'}],
                                               'expiration':{'type':'sliding','interval':'2'}
                                              }");
                    var result = cache.SearchService.ExecuteNonQuery(qc);


                }

                Thread.Sleep(20000);

                long itemsInCache = cache.Count;
                if (itemsInCache > 0)
                {
                    throw new Exception("Failure: verify meta by adding with tags and then getting through those tags.");
                }
                else
                {
                    _report.AddPassedTestCase(methodName, "Success: verify meta by adding with tags and then getting through those tags.");
                }

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

                
            }
            // throw;
        }

        //Add a key-pair item, add another key-pair item with a key dependency on item 1. Remove first from cache.
        public void VerifyMetaData5()
        {
            count++;
            int reveived = 0;
            var methodName = "VerifyMetaData5";
            try
            {
                cache.Clear();
                //  var prodDict = GetDictionaryOfProductsToAdd(100);
                string depKey = "dependency";
                var key = GetKey();

                cache.Insert(depKey, "u depend on me");
                string query = "Insert Into Alachisoft.Ncache.Sample.Data.Product (key,value,meta) values (@cachekey,@val,@data)";
                QueryCommand qc = new QueryCommand(query);
                qc.Parameters.Add("@cachekey", key);
                qc.Parameters.Add("@val", GetProduct());
                qc.Parameters.Add("@data", @"{
                                              'namedtags':[{'FlashSaleDiscount':0.5,'type':'decimal'}],
                                               'dependency':[{'key':['dependency']}]
                                              }");
                var result = cache.SearchService.ExecuteNonQuery(qc);

                cache.Remove(depKey);
                var returned = cache.Get<Product>(key);
                if (returned == null)
                {
                    testResults.Add(methodName, ResultStatus.Success);
                    _report.AddPassedTestCase(methodName, "Success: Add a key-pair item, add another key-pair item with a key dependency on item 1. Remove first from cache.");

                }

                throw new Exception("Test case failed");

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }
            // throw;
        }

        //Add a key-pair item add another key-pair item with a key dependency on item 1. Update the first item in cache.
        public void VerifyMetaData6()
        {
            count++;
            int reveived = 0;
            var methodName = "VerifyMetaData6";
            try
            {
                cache.Clear();
                cache.Clear();
                var prodDict = GetDictionaryOfProductsToAdd(100);
                string depKey = "dependency";
                string depKey1 = "dependency1";
                var key = GetKey();

                cache.Insert(depKey, "u depend on me");
                cache.Insert(depKey1, "u depend on me");

                string query = "Insert Into Alachisoft.Ncache.Sample.Data.Product (key,value,meta) values (@cachekey,@val,@data)";
                QueryCommand qc = new QueryCommand(query);
                qc.Parameters.Add("@cachekey", key);
                qc.Parameters.Add("@val", GetProduct());
                qc.Parameters.Add("@data", @"{
                                              'namedtags':[{'FlashSaleDiscount':0.5,'type':'decimal'}],
                                               'dependency':[{'key':['dependency','dependency1']}]
                                              }");
                var result = cache.SearchService.ExecuteNonQuery(qc);
                cache.Insert(depKey1, "Diyatech-10");

                var returned = cache.Get<Product>(key);
                if (returned == null)
                {
                    testResults.Add(methodName, ResultStatus.Success);
                    _report.AddPassedTestCase(methodName, "Success: Add a key-pair item, add another key-pair item with a key dependency on item 1. Remove first from cache.");

                }

                long itemsInCache = cache.Count;
              


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }
            // throw;
        }
        //Add a key-pair item and add another 10 key-pair items with a key dependency in array  on item 1. Update 1 in cache.
        public void VerifyMetaData7()
        {
            count++;
            int reveived = 0;
            var methodName = "VerifyMetaData7";
            try
            {
                cache.Clear();
                var prodDict = GetDictionaryOfProductsToAdd(100);
                var key = GetKey();

                for (int i = 0; i < 10; i++)
                {
                    cache.Insert("dependency" + i, GetProduct());
                }

                string query = "Insert Into Alachisoft.Ncache.Sample.Data.Product (key,value,meta) values (@cachekey,@val,@data)";
                QueryCommand qc = new QueryCommand(query);
                qc.Parameters.Add("@cachekey", key);
                qc.Parameters.Add("@val", GetProduct());
                qc.Parameters.Add("@data", @"{
                                              'namedtags':[{'FlashSaleDiscount':0.5,'type':'decimal'}],
                                               'dependency':[
                                                               {'key': ['dependency0','dependency1','dependency2','dependency3','dependency4','dependency5','dependency6','dependency7','dependency8','dependency9']}
                                                            ]
                                                            }");
                var result = cache.SearchService.ExecuteNonQuery(qc);
                cache.Insert("dependency7", "Diyatech-10");

                var returned = cache.Get<Product>(key);
                if (returned == null)
                {
                    testResults.Add(methodName, ResultStatus.Success);
                    Console.WriteLine("Success: Add a key-pair item and add another 10 key-pair items with a key dependency in array  on item 1. Update 1 in cache.");

                }

                long itemsInCache = cache.Count;
               

            }
            catch (Exception ex)
            {
                testResults.Add(methodName, ResultStatus.Failure);
                Console.WriteLine("Failure: Add a key-pair item and add another 10 key-pair items with a key dependency in array  on item 1. Update 1 in cache.");
            }
            // throw;
        }

        //Add an object with a File based dependency and Modify the file.

        public void VerifyMetaData8()
        {
            count++;
            int reveived = 0;
            var methodName = "VerifyMetaData8";
            try
            {
                string path = "E:\\QueryTesting";
                string filePath = GenerateFileName(path);
                Create(filePath);

                cache.Clear();
                var key = GetKey();

                string query = "Insert Into Alachisoft.Ncache.Sample.Data.Product (key,value,meta) values (@cachekey,@val,@data)";
                QueryCommand qc = new QueryCommand(query);
                qc.Parameters.Add("@cachekey", key);
                qc.Parameters.Add("@val", GetProduct());
                qc.Parameters.Add("@data", @"{
                                              'namedtags':[{'FlashSaleDiscount':0.5,'type':'decimal'}],
                                               'dependency':[{'file':['E:\\QueryTesting\\RandomFile.txt']}]
                                              }");
                var result = cache.SearchService.ExecuteNonQuery(qc);

                var returnedBeforeFileChange = cache.Get<object>(key);

                var cacheItem = cache.GetCacheItem(key);

                Modify(filePath, "From Diyatech 2 Alachisoft");

                //var returned = cache.Get<Product>(key);
                var returned = cache.Get<object>(key);
                if (returned == null)
                {
                    _report.AddPassedTestCase(methodName, "Success: Add an object with a File based dependency and Modify the file.");
                }

                long itemsInCache = cache.Count;

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

               
            }
            // throw;
        }

        //provide invalid dependency
        public void VerifyMetaData10()
        {
            count++;
            int reveived = 0;
            var methodName = "VerifyMetaData10";
            try
            {
                //cache.Clear();
                //var item = new CacheItem(GetProduct());
                //item.Dependency = new FileDependency(new string[5]);
                //cache.Insert(GetKey(), item);
                string path = "E:\\QueryTesting";
                string filePath = GenerateFileName(path);
                Create(filePath);

                cache.Clear();
                var key = GetKey();

                string query = "Insert Into Alachisoft.Ncache.Sample.Data.Product (key,value,meta) values (@cachekey,@val,@data)";
                QueryCommand qc = new QueryCommand(query);
                qc.Parameters.Add("@cachekey", key);
                qc.Parameters.Add("@val", GetProduct());
                qc.Parameters.Add("@data", @"{
                                              'namedtags':[{'FlashSaleDiscount':0.5,'type':'decimal'}],
                                               'dependency':[{'file':['']}]
                                              }");
                var result = cache.SearchService.ExecuteNonQuery(qc);

                {
                    testResults.Add(methodName, ResultStatus.Failure);
                    _report.AddPassedTestCase(methodName, "Failure: provide invalid dependency");

                }

                long itemsInCache = cache.Count;

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);


                testResults.Add(methodName, ResultStatus.Success);
                Console.WriteLine("Success: provide invalid dependency");
            }
            // throw;
        }
        void VerifyMetaData9()
        {
            count++;
            int reveived = 0;
            var methodName = "VerifyMetaData9";
            try
            {

                cache.Clear();
                var key = GetKey();

                string query = "Insert Into Alachisoft.Ncache.Sample.Data.Product (key,value,meta) values (@cachekey,@val,@data)";
                QueryCommand qc = new QueryCommand(query);
                qc.Parameters.Add("@cachekey", key);
                qc.Parameters.Add("@val", GetProduct());
                qc.Parameters.Add("@data", @"{
                                              'namedtags':[{'FlashSaleDiscount':0.5}],
                                               'writethruprovider':
                                                    {'providername':'SqlWriteThruProvider','option':'writethru'}
                                              }");
                var result = cache.SearchService.ExecuteNonQuery(qc);

                testResults.Add(methodName, ResultStatus.Success);
                _report.AddPassedTestCase(methodName, "Success: Add an object with a File based dependency and Modify the file.");



                long itemsInCache = cache.Count;

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

                testResults.Add(methodName, ResultStatus.Failure);
                Console.WriteLine("Failure: Add an object with a File based dependency and Modify the file.");
            }
            // throw;
        }


        #endregion


        #region ------------------------- Helper Methods -------------------------


        //                        Helper Methods
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------
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


        public string GenerateFileName(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            Random rnd = new Random();
            return path + "\\RandomFile" + ".txt";
        }

        public void Create(params string[] paths)
        {
            try
            {
                foreach (string path in paths)
                {
                    Delete(path);
                    StreamWriter writer = new StreamWriter(path, true);
                    writer.WriteLine("");
                    writer.Close();
                }
            }
            catch (Exception exp)
            {
                throw new Exception("Some problem with file creation.\nError: " + exp.ToString());
            }
        }

        public void Delete(params string[] paths)
        {
            try
            {
                foreach (string path in paths)
                {
                    if (File.Exists(path))
                        File.Delete(path);
                }
            }
            catch (Exception exp)
            {
                throw new Exception("Some problem with file deletion.\nError: " + exp.ToString());
            }
        }

        public void Modify(string path, string text)
        {
            try
            {
                StreamWriter writer = new StreamWriter(path, true);
                writer.WriteLine(text + ", Modification Time: " + DateTime.Now.ToString());
                writer.Flush();
                writer.Close();
            }
            catch (Exception exp)
            {
                throw new Exception("Some problem with file modification.\nError: " + exp.ToString());
            }
        }

        #endregion
    }



}
