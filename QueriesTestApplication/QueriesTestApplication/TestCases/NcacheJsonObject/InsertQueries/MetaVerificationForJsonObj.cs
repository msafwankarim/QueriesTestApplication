using Alachisoft.NCache.Client;
using Alachisoft.NCache.Common.Queries;
using Alachisoft.NCache.Runtime.Caching;
using Alachisoft.NCache.Runtime.Dependencies;
using Alachisoft.NCache.Runtime.JSON;
using Alachisoft.NCache.Sample.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

using Alachisoft.NCache.Runtime;
using System.Data.SqlClient;
using QueriesTestApplication.Utils;
using System.Reflection;
using Alachisoft.NCache.Caching.Statistics;
using Alachisoft.NCache.Common;
using System.Configuration.Provider;
using Alachisoft.NCache.Runtime.CacheManagement;
using System.CodeDom;
using System.Linq;

namespace QueriesTestApplication
{
    class MetaVerificationTestForJsonObj
    {
        private readonly ICache _cache;
        private readonly Report _report;
        private readonly int _cleanIntervalSeconds = 5;

        private int DependencyWaitTime  { get => (_cleanIntervalSeconds + 1) * 1000; }

        public Report Report { get => _report; }

        public MetaVerificationTestForJsonObj()
        {
            _cache = CacheManager.GetCache(Common.CacheName);
            _report = new Report(nameof(MetaVerificationTestForJsonObj));
        }

        public void AddAndGetSimpleItem()
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            try
            {
                _cache.Clear();

                string key1 = "abc";
                var val = GetProduct();

                string query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value) Values (@key1, @val)";

                QueryCommand queryCommand = new QueryCommand(query);

                queryCommand.Parameters.Add("@key1", key1);
                queryCommand.Parameters.Add("@val", val);

                var result = _cache.QueryService.ExecuteNonQuery(queryCommand);

                var item = _cache.Get<Product>(key1);

                if (item.Time != val.Time || item.Id != val.Id)
                    throw new Exception("item added and obtained is not same");


                _report.AddPassedTestCase(methodName, "adding and getting simple product");


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }

        }


        #region --------------------------------- Priority  ----------------------------

        //   ..... Cache Item Priority
        //  AboveNormal
        //  BelowNormal
        //  High
        //  Low
        //  Normal
        //  NotRemovable

        public void LowPriority()
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            try
            {
                _cache.Clear();

                JsonObject MetaData = new JsonObject();
                MetaData.AddAttribute("Priority", "Low");

                string key1 = "key_PriorityMetadataInJsonObject";
                var val = GetProduct();

                string query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                QueryCommand queryCommand = new QueryCommand(query);

                queryCommand.Parameters.Add("@key1", key1);
                queryCommand.Parameters.Add("@val", val);
                queryCommand.Parameters.Add("@metadata", MetaData);

                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);

                var cacheItem = _cache.GetCacheItem(key1);

                var PriorityAttribute = cacheItem.Priority;

                if (PriorityAttribute == CacheItemPriority.Low)
                {
                    _report.AddPassedTestCase(methodName, "Success: Add Low Priority Metadata In JSON Object ");
                }
                else
                {
                    throw new Exception("Failure:  Add  Low Priority Metadata In JSON Object");
                }

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }

        }

        public void BelowNormalPriority()
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            try
            {
                _cache.Clear();

                JsonObject MetaData = new JsonObject();
                MetaData.AddAttribute("Priority", "BelowNormal");

                string key1 = "key_PriorityMetadataInJsonObject";
                var val = GetProduct();

                string query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                QueryCommand queryCommand = new QueryCommand(query);

                queryCommand.Parameters.Add("@key1", key1);
                queryCommand.Parameters.Add("@val", val);
                queryCommand.Parameters.Add("@metadata", MetaData);

                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);

                var cacheItem = _cache.GetCacheItem(key1);

                var PriorityAttribute = cacheItem.Priority;

                if (PriorityAttribute == CacheItemPriority.BelowNormal)
                {
                    _report.AddPassedTestCase(methodName, "Success: Add BelowNormal Priority Metadata In JSON Object ");
                }
                else
                {
                    throw new Exception("Failure:  Add  BelowNormal Priority Metadata In JSON Object");
                }

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }

        }

        public void NormalPriority()
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            try
            {
                _cache.Clear();

                JsonObject MetaData = new JsonObject();
                MetaData.AddAttribute("Priority", "Normal");

                string key1 = "key_PriorityMetadataInJsonObject";
                var val = GetProduct();

                string query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                QueryCommand queryCommand = new QueryCommand(query);

                queryCommand.Parameters.Add("@key1", key1);
                queryCommand.Parameters.Add("@val", val);
                queryCommand.Parameters.Add("@metadata", MetaData);

                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);

                var cacheItem = _cache.GetCacheItem(key1);

                var PriorityAttribute = cacheItem.Priority;

                if (PriorityAttribute == CacheItemPriority.Normal)
                {
                    _report.AddPassedTestCase(methodName, "Success: Add Normal Priority Metadata In JSON Object ");
                }
                else
                {
                    throw new Exception("Failure:  Add  Normal Priority Metadata In JSON Object");
                }

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }

        }

        public void AboveNormalPriority()
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            try
            {
                _cache.Clear();

                JsonObject MetaData = new JsonObject();
                MetaData.AddAttribute("Priority", "AboveNormal");

                string key1 = "key_PriorityMetadataInJsonObject";
                var val = GetProduct();

                string query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                QueryCommand queryCommand = new QueryCommand(query);

                queryCommand.Parameters.Add("@key1", key1);
                queryCommand.Parameters.Add("@val", val);
                queryCommand.Parameters.Add("@metadata", MetaData);

                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);

                var cacheItem = _cache.GetCacheItem(key1);

                var PriorityAttribute = cacheItem.Priority;

                if (PriorityAttribute == CacheItemPriority.AboveNormal)
                {
                    _report.AddPassedTestCase(methodName, "Success: Add Priority Metadata In JSON Object ");
                }
                else
                {
                    throw new Exception("Failure:  Add Priority Metadata In JSON Object");
                }

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }

        }

        public void HighPriority()
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            try
            {
                _cache.Clear();

                JsonObject MetaData = new JsonObject();
                MetaData.AddAttribute("Priority", "High");

                string key1 = "key_PriorityMetadataInJsonObject";
                var val = GetProduct();

                string query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                QueryCommand queryCommand = new QueryCommand(query);

                queryCommand.Parameters.Add("@key1", key1);
                queryCommand.Parameters.Add("@val", val);
                queryCommand.Parameters.Add("@metadata", MetaData);

                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);

                var cacheItem = _cache.GetCacheItem(key1);

                var PriorityAttribute = cacheItem.Priority;

                if (PriorityAttribute == CacheItemPriority.High)
                {
                    _report.AddPassedTestCase(methodName, "Success: Add High Priority Metadata In JSON Object ");
                }
                else
                {
                    throw new Exception("Failure:  Add  High Priority Metadata In JSON Object");
                }

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }

        }

        public void NotRemovablePriority()
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            try
            {
                _cache.Clear();

                JsonObject MetaData = new JsonObject();
                MetaData.AddAttribute("Priority", "NotRemovable");

                string key1 = "key_PriorityMetadataInJsonObject";
                var val = GetProduct();

                string query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                QueryCommand queryCommand = new QueryCommand(query);

                queryCommand.Parameters.Add("@key1", key1);
                queryCommand.Parameters.Add("@val", val);
                queryCommand.Parameters.Add("@metadata", MetaData);

                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);

                var cacheItem = _cache.GetCacheItem(key1);

                var PriorityAttribute = cacheItem.Priority;

                if (PriorityAttribute == CacheItemPriority.NotRemovable)
                {
                    _report.AddPassedTestCase(methodName, "Success: Add NotRemovable Priority Metadata In JSON Object ");
                }
                else
                {
                    throw new Exception("Failure:  Add  NotRemovable Priority Metadata In JSON Object");
                }

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }

        }

        public void DefaultPriority()
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            try
            {
                _cache.Clear();

                JsonObject MetaData = new JsonObject();
                MetaData.AddAttribute("Priority", "Default");

                string key1 = "key_PriorityMetadataInJsonObject";
                var val = GetProduct();

                string query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                QueryCommand queryCommand = new QueryCommand(query);

                queryCommand.Parameters.Add("@key1", key1);
                queryCommand.Parameters.Add("@val", val);
                queryCommand.Parameters.Add("@metadata", MetaData);

                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);

                var cacheItem = _cache.GetCacheItem(key1);

                var PriorityAttribute = cacheItem.Priority;

                if (PriorityAttribute == CacheItemPriority.Normal) // value for default priority is normal
                {
                    _report.AddPassedTestCase(methodName, "Success: Add Default Priority Metadata In JSON Object ");
                }
                else
                {
                    throw new Exception("Failure:  Add  Default Priority Metadata In JSON Object");
                }

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }

        }

        #endregion


        #region --------------------------------- Group  -------------------------------

        public void GroupMetadataInJsonObject()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;

            try
            {
                _cache.Clear();

                string JsonString = "{\"group\":\"DevTeam\"}";
                var MetaData = new JsonObject(JsonString);

                string key1 = "key_GroupMetadataInJObject";
                var val = GetProduct();

                string query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                QueryCommand queryCommand = new QueryCommand(query);

                queryCommand.Parameters.Add("@key1", key1);
                queryCommand.Parameters.Add("@val", val);
                queryCommand.Parameters.Add("@metadata", MetaData);

                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);

                var cacheItem = _cache.GetCacheItem(key1);

                var GroupAttribute = (JsonValue)cacheItem.Group;

                if (GroupAttribute.Value.ToString() == "DevTeam")
                    _report.AddPassedTestCase(methodName, "Success: Add Group Metadata In Jsonbject ");

                else
                    throw new Exception("Failure:  Add Group Metadata In JsonObject");

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }

        }

        #endregion


        #region --------------------------------- Tags ---------------------------------

        public void TestTagMetadataWithByAnyTag()
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            try
            {
                _cache.Clear();

                var MetaData = new JsonObject();
                JsonArray tags = new JsonArray();
                tags.Add("Important Product");

                MetaData.AddAttribute("tags", tags);

                string key1 = "abc";
                var val = GetProduct();

                string query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                QueryCommand queryCommand = new QueryCommand(query);

                queryCommand.Parameters.Add("@key1", key1);
                queryCommand.Parameters.Add("@val", val);
                queryCommand.Parameters.Add("@metadata", MetaData);

                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);

                Tag[] SearchTags = new Tag[2] { new Tag("Important Product"), new Tag("Imported Product") };

                _ = _cache.Get<JsonObject>(key1);
                _ = _cache.Get<Product>(key1);

                IDictionary<string, Product> data = _cache.SearchService.GetByTags<Product>(SearchTags, TagSearchOptions.ByAnyTag);

                if (data.Count > 0)
                    _report.AddPassedTestCase(methodName, "Success: Add TagsMetaData in JSON object");
                else
                    throw new Exception("Failure: Add TagsMetaData in JSON object");

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }

        }

        public void TestTagMetadataWithByAllTag()
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            try
            {
                _cache.Clear();

                var MetaData = new JsonObject();
                JsonArray tags = new JsonArray();
                tags.Add("Important Product");
                tags.Add("Imported Product");

                MetaData.AddAttribute("tags", tags);

                string key1 = "abc";
                var val = GetProduct();

                string query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                QueryCommand queryCommand = new QueryCommand(query);

                queryCommand.Parameters.Add("@key1", key1);
                queryCommand.Parameters.Add("@val", val);
                queryCommand.Parameters.Add("@metadata", MetaData);

                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);

                Tag[] SearchTags = new Tag[2] { new Tag("Important Product"), new Tag("Imported Product") };

                _ = _cache.Get<JsonObject>(key1);
                _ = _cache.Get<Product>(key1);

                IDictionary<string, Product> data = _cache.SearchService.GetByTags<Product>(SearchTags, TagSearchOptions.ByAllTags);

                if (data.Count > 0)
                    _report.AddPassedTestCase(methodName, "Success: Add TagsMetaData in JSON object");
                else
                    throw new Exception("Failure: Add TagsMetaData in JSON object");

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }

        }

        public void TagsMetadataInJSONObject()
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            try
            {
                _cache.Clear();

                var MetaData = new JsonObject();
                JsonArray tags = new JsonArray();
                tags.Add("Important Product");
                tags.Add("Imported Product");

                MetaData.AddAttribute("tags", tags);

                string key1 = "abc";
                var val = GetProduct();

                string query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                QueryCommand queryCommand = new QueryCommand(query);

                queryCommand.Parameters.Add("@key1", key1);
                queryCommand.Parameters.Add("@val", val);
                queryCommand.Parameters.Add("@metadata", MetaData);

                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);

                Tag[] SearchTags = new Tag[2] { new Tag("Important Product"), new Tag("Imported Product") };

                _ = _cache.Get<JsonObject>(key1);
                _ = _cache.Get<Product>(key1);

                IDictionary<string, Product> data = _cache.SearchService.GetByTags<Product>(SearchTags, TagSearchOptions.ByAllTags);

                if (data.Count > 0)
                    _report.AddPassedTestCase(methodName, "Success: Add TagsMetaData in JSON object");
                else
                    throw new Exception("Failure: Add TagsMetaData in JSON object");

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }

        }

        #endregion


        #region --------------------------------- Named Tags ---------------------------

        public void NameTagMetadataInJSONObject()
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            try
            {
                _cache.Clear();
                

                JsonObject MetaData = new ();

                MetaData.AddAttribute("namedtags", Helper.GetNamedTagsArray());


                string key1 = "abc";
                var val = GetProduct();

                string query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@key1", key1);
                queryCommand.Parameters.Add("@val", val);
                queryCommand.Parameters.Add("@metadata", MetaData);

                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);
                var cacheItem = _cache.GetCacheItem(key1);

                var Attributes = cacheItem.NamedTags;

                if (Attributes.Contains("Discount") && Attributes.Contains("FlashDiscount") && Attributes.Contains("Percentage"))
                {
                    _report.AddPassedTestCase(methodName, "Success: Add NameTagMetaData in JSON object");
                }
                else
                {
                    throw new Exception("Failure: Add NameTagMetaData in JSON object");

                }
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }

        }     

        #endregion


        #region --------------------------------- Key Dependency -----------------------

        public void KeyDependency()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string Itemkey = "KeyDependency";
            Product item = GetProduct();
            string description = "Verify key dependency ";

            try
            {

                var masterKey = Guid.NewGuid().ToString();
                _cache.Insert(masterKey, item);

                string query = "INSERT INTO Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";


                string keysArray = $"[\"{masterKey}\"]";
                string keyDependency = "{\"keys\" :" + keysArray + "}";

                string jsonString = @"{""dependency"":[{""key"":" + keyDependency + "}]}";

                var jsonObject = new JsonObject(jsonString);

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@key1", Itemkey);
                queryCommand.Parameters.Add("@val", item);
                queryCommand.Parameters.Add("@metadata", jsonObject);

                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);

                var cacheItem = _cache.GetCacheItem(Itemkey);

                if (cacheItem == null)
                    throw new Exception("item not inserted with key dependency");

                if (cacheItem.Dependency == null)
                    throw new Exception("key dependency is not added with cache item");

                //update master key
                _cache.Remove(masterKey);

                if (_cache.GetCacheItem(Itemkey) != null)
                    throw new Exception($"Removing master key didnot triggered key dependency");

                _report.AddPassedTestCase(methodName, description);

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }



        }

        public void KeyDependencyWithArrayOfMasterKeys()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string Itemkey = "KeyDependency";
            Product item = GetProduct();
            string description = "Verify key dependency ";

            try
            {
                int totalKeys = 3;

                var masterKey1 = Guid.NewGuid().ToString();
                var masterKey2 = Guid.NewGuid().ToString();
                var masterKey3 = Guid.NewGuid().ToString();

                _cache.Insert(masterKey1, item);
                _cache.Insert(masterKey2, item);
                _cache.Insert(masterKey3, item);

                string query = "INSERT INTO Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";


                string keysArray = $"[\"{masterKey1}\",\"{masterKey2}\",\"{masterKey3}\"]";

                string KeyDependency = "{\"keys\" :" + keysArray + "}";

                string jsonString = @"{""dependency"":[{""key"":" + KeyDependency + "}]}";

                var jsonObject = new JsonObject(jsonString);

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@key1", Itemkey);
                queryCommand.Parameters.Add("@val", item);
                queryCommand.Parameters.Add("@metadata", jsonObject);

                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);



                var cacheItem = _cache.GetCacheItem(Itemkey);

                if (cacheItem == null)
                    throw new Exception("item not inserted with key dependency");

                if (cacheItem.Dependency == null)
                    throw new Exception("key dependency is not added with cache item");

                using (var keyDependency = cacheItem.Dependency.Dependencies.First() as KeyDependency)
                {
                    if (keyDependency.CacheKeys.Count() != totalKeys)
                        throw new Exception("total master keys doesnot equal total given master keys");
                }

                //update master key
                _cache.Remove(masterKey2);

                if (_cache.GetCacheItem(Itemkey) != null)
                    throw new Exception($"Removing master key didnot triggered key dependency");

                _report.AddPassedTestCase(methodName, description);

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }



        }

        public void KeyDependencyOnRemoveOperation()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string Itemkey = "KeyDependency";
            Product item = GetProduct();
            string description = "Verify key dependency with one master key with Remove enum";

            try
            {

                var masterKey = Guid.NewGuid().ToString();
                _cache.Insert(masterKey, item);

                string query = "INSERT INTO Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";


                string keysArray = $"[\"{masterKey}\"]";
                string keyDependency = "{\"keys\" :" + keysArray + ", \"type\" : \"removeonly\"}";


                string jsonString = "{\"dependency\":[{\"key\":" + keyDependency + "}]}";

                KeyDependency a = new KeyDependency("s");


                var jsonObject = new JsonObject(jsonString);

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@key1", Itemkey);
                queryCommand.Parameters.Add("@val", item);
                queryCommand.Parameters.Add("@metadata", jsonObject);

                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);

                var cacheItem = _cache.GetCacheItem(Itemkey);

                if (cacheItem == null)
                    throw new Exception("item not inserted with key dependency");

                if (cacheItem.Dependency == null)
                    throw new Exception("key dependency is not added with cache item");

                //update master key
                _cache.Insert(masterKey, item);

                if (_cache.GetCacheItem(Itemkey) == null)
                    throw new Exception($"Updating master key triggerd the dependency");

                _cache.Remove(masterKey);

                if (_cache.GetCacheItem(Itemkey) != null)
                    throw new Exception($"Removing master key didnot triggered key dependency");

                _report.AddPassedTestCase(methodName, description);

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }



        }

        private void KeyDependencyOnUpdateOrRemoveOperation()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string Itemkey = "KeyDependency";
            Product item = GetProduct();
            string description = "Verify key dependency with one master key with UpdateOrRemove enum";

            try
            {
                var masterKey = Guid.NewGuid().ToString();
                _cache.Insert(masterKey, item);

                string query = "INSERT INTO Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";


                // string jsonString = "{\"dependency\": [{\"key\": {\"key\":"+ $"\"{Itemkey}\"" + "}}]}";
                string jsonString = "{\"dependency\": [{\"key\": {\"keys\":" + $"[\"{masterKey}\"]" + ", \"type\" : \"UpdateOrRemove\"}}]}";

                var jsonObject = new JsonObject(jsonString);

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@key1", Itemkey);
                queryCommand.Parameters.Add("@val", item);
                queryCommand.Parameters.Add("@metadata", jsonObject);

                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);

                var cacheItem = _cache.GetCacheItem(Itemkey);

                if (cacheItem == null)
                    throw new Exception("item not inserted with key dependency");

                if (cacheItem.Dependency == null)
                    throw new Exception("key dependency is not added with cache item");

                //update master key
                _cache.Insert(masterKey, item);

                if (_cache.GetCacheItem(Itemkey) != null)
                    throw new Exception($"item still exists after updating the master key");

                _report.AddPassedTestCase(methodName, description);



            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }



        }

        #endregion


        #region --------------------------------- SQL Dependency ------------------------

        private void VerifySQLDependency()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string Itemkey = "SqlDBDependency";
            Product item = GetProduct();

            _cache.Clear();

            try
            {
                string AddItemWithDependencyQuery = "INSERT INTO Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                //var JsonForDependency = @"{'dependency': [{'sql': {'connectionstring': 'Data Source=AQIB-NAVEED;Initial Catalog=Northwind;Integrated Security=True;','querystring': 'SELECT CustomerID, Address, City FROM dbo.Customers;'}}]}";
                var JsonForDependency = "{\"dependency\": [{\"sql\": {\"connectionstring\": \"Data Source=AQIB-NAVEED;Initial Catalog=Northwind;Integrated Security=True;\",\"querystring\": \"SELECT CustomerID, Address, City FROM dbo.Customers;\"}}]}";
                var jsonObject = new JsonObject(JsonForDependency);
                QueryCommand queryCommand = new QueryCommand(AddItemWithDependencyQuery);

                queryCommand.Parameters.Add("@key1", Itemkey);
                queryCommand.Parameters.Add("@val", item);
                queryCommand.Parameters.Add("@metadata", jsonObject);

                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);

                CacheItem cacheItem = _cache.GetCacheItem(Itemkey);


                if (!(cacheItem.Dependency.Dependencies.First() is SqlCacheDependency))
                    throw new Exception($"registeres sql dependency but got {cacheItem.Dependency.GetType().FullName}");

                // Explicitly make some change in DB

                string connetionString = @"Data Source=AQIB-NAVEED;Initial Catalog=Northwind;Integrated Security=True;";
                SqlConnection cnn = new SqlConnection(connetionString);
                cnn.Open();

                Random random = new Random();
                int street = random.Next(1, 1000);

                string UpdateQuery = "UPDATE [Northwind].[dbo].[Customers]";
                UpdateQuery += "SET Address = 'Obere Str." + street + "'";
                UpdateQuery += "WHERE CustomerID = 'ALFKI' ";

                //Console.WriteLine("Query is "+ UpdateQuery);

                SqlCommand command;
                SqlDataAdapter adapter = new SqlDataAdapter();

                command = new SqlCommand(UpdateQuery, cnn);
                adapter.UpdateCommand = command;
                adapter.UpdateCommand.ExecuteNonQuery();


                // After changing in Db
                var returned = _cache.Get<Alachisoft.NCache.Sample.Data.Product>(Itemkey);
                if (returned == null)
                {
                    _report.AddPassedTestCase(methodName, "Success: Add SQL DB Dependency ");

                }
                else
                {
                    throw new Exception("triggering DB Dependency didnot removed the item in cache");
                }

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }



        }

        #endregion


        #region --------------------------------- Custom Dependency --------------------
            
        private void VerifyExtensibleDependency()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            _cache.Clear();
            string Itemkey = "KeyForCustomDependency";
            Product item = GetProduct();
            string ProviderName = "custom"; 

            try
            {
                string AddItemWithDependencyQuery = "INSERT INTO Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                var JsonForDependency = "{\"dependency\": [{\"custom\":" +
                    " {\"providername\": \"" + ProviderName + "\"," +
                    "\"param\":{\"id\":\"101\"," +
                    "\"connectionstring\":\"NoConnectionStringForTesting\"}}}]}";

                var jsonObj = new JsonObject(JsonForDependency);

                QueryCommand queryCommand = new QueryCommand(AddItemWithDependencyQuery);
                queryCommand.Parameters.Add("@key1", Itemkey);
                queryCommand.Parameters.Add("@val", item);
                queryCommand.Parameters.Add("@metadata", jsonObj);

                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);

                var cacheItem = _cache.GetCacheItem(Itemkey);

                if (!(cacheItem.Dependency.Dependencies.FirstOrDefault() is  CustomDependency))
                    throw new Exception("Extensible dependency not registered");

                Console.WriteLine($"Sleeping for {DependencyWaitTime/1000} seconds to verify extensible dependency");
                Thread.Sleep(DependencyWaitTime);

                cacheItem = _cache.GetCacheItem(Itemkey);
                if (cacheItem == null)
                    _report.AddPassedTestCase(methodName, "Success: Add extensible Dependency ");

                else
                    throw new Exception("Failure: Add extensible Dependency ");

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }



        }

        private void VerifyBulkExtensibleDependency()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            _cache.Clear();
            string Itemkey = "KeyForBulkExtensibleDependency";
            Product item = GetProduct();
            string ProviderName = "bulkDependencyProvider";

            try
            {
                string AddItemWithDependencyQuery = "INSERT INTO Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                var JsonForDependency = "{\"dependency\": [{\"custom\":" +
                    " {\"providername\": \"" + ProviderName + "\"," +
                    "\"param\":{\"id\":\"101\"," +
                    "\"connectionstring\":\"NoConnectionStringForTesting\"}}}]}";

                var jsonObj = new JsonObject(JsonForDependency);

                QueryCommand queryCommand = new QueryCommand(AddItemWithDependencyQuery);
                queryCommand.Parameters.Add("@key1", Itemkey);
                queryCommand.Parameters.Add("@val", item);
                queryCommand.Parameters.Add("@metadata", jsonObj);

                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);

                var cacheItem = _cache.GetCacheItem(Itemkey);

                if (!(cacheItem.Dependency.Dependencies.FirstOrDefault() is CustomDependency))
                    throw new Exception("bulk extensible dependency not registered");

                Console.WriteLine($"Sleeping for {DependencyWaitTime / 1000} seconds to verify bulk extensible dependency");
                Thread.Sleep(DependencyWaitTime);

                cacheItem = _cache.GetCacheItem(Itemkey);
                if (cacheItem == null)
                    _report.AddPassedTestCase(methodName, "Success: Add bulk extensible Dependency ");

                else
                    throw new Exception("Failure: Add bulk extensible Dependency ");

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }



        }

        public void VerifyNotifyExtensibleDependency()
        {
            
            string methodName = MethodBase.GetCurrentMethod().Name;
            _cache.Clear();
            string Itemkey = "KeyForNotifyExtensibleDependency";
            Product item = GetProduct();
            string ProviderName = "notifyDependencyProvider";

            try
            {
                string AddItemWithDependencyQuery = "INSERT INTO Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                var JsonForDependency = "{\"dependency\": [{\"custom\":" +
                    " {\"providername\": \"" + ProviderName + "\"," +
                    "\"param\":{\"id\":\"101\"," +
                    "\"connectionstring\":\"NoConnectionStringForTesting\"}}}]}";

                var jsonObj = new JsonObject(JsonForDependency);

                QueryCommand queryCommand = new QueryCommand(AddItemWithDependencyQuery);
                queryCommand.Parameters.Add("@key1", Itemkey);
                queryCommand.Parameters.Add("@val", item);
                queryCommand.Parameters.Add("@metadata", jsonObj);

                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);

                var cacheItem = _cache.GetCacheItem(Itemkey);

                if (!(cacheItem.Dependency.Dependencies.FirstOrDefault() is CustomDependency ))
                    throw new Exception("notify extensible dependency not registered");

                int waitTime = _cleanIntervalSeconds + 1;
                Console.WriteLine($"Sleeping for {DependencyWaitTime /1000 } seconds to verify notify extensible dependency");
                Thread.Sleep(DependencyWaitTime);

                cacheItem = _cache.GetCacheItem(Itemkey);
                if (cacheItem == null)
                    _report.AddPassedTestCase(methodName, "Success: Add NotifyExtensible Dependency ");

                else
                    throw new Exception("Failure: Add Custom Dependency ");

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }



        }

        #endregion


        #region --------------------------------- Cache SyncDependency   ---------------

        public void VerifySyncDependency()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            _cache.Clear();
            string Itemkey = "KeyForSyncDependency";
            Product item = GetProduct();

            try
            {


                string remoteCacheId = "remoteCache";

                string query = "INSERT INTO Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                var jsonString = "{\"dependency\": [{\"syncdependency\": {\"remoteCache\":" + $"\"{remoteCacheId}\"" + ",\"key\":" + $"\"{Itemkey}\"" + " }}]}";

                var jsonObj = new JsonObject(jsonString);

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@key1", Itemkey);
                queryCommand.Parameters.Add("@val", item);
                queryCommand.Parameters.Add("@metadata", jsonObj);

                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);

                var cacheItem = _cache.GetCacheItem(Itemkey);

                var syncDependency = cacheItem.SyncDependency;

                if (syncDependency == null)
                    throw new Exception("Failure: syncDependency is not registers with query");

                if (syncDependency.Key != Itemkey)
                    throw new Exception("Failure: key is not equal to rgistered key");

                if (syncDependency.CacheId != remoteCacheId)
                    throw new Exception("Failure: CacheId is not equal to registerd cache id");


                _report.AddPassedTestCase(methodName, "Success: Add File Dependency ");


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }



        }

        #endregion


        #region --------------------------------- Resync Options  ----------------------

        private void VerifyResyncOption()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            _cache.Clear();
            string Itemkey = "KeyForResyncOption";
            Product item = GetProduct();

            try
            {
                string providerName = "read";
                string query = "INSERT INTO Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";


                JsonObject MetaData = new JsonObject();

                JsonObject expiration = new JsonObject();
                expiration.AddAttribute("type", "sliding");
                expiration.AddAttribute("interval", ExpirationTimes.SlidingExpirationSeconds);

                MetaData.AddAttribute("expiration", expiration);


                JsonObject options = new JsonObject();
                options.AddAttribute("ResyncOnExpiration", "true");
                options.AddAttribute("providerName", providerName);

                MetaData.AddAttribute("ResyncOptions",options);
                

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@key1", Itemkey);
                queryCommand.Parameters.Add("@val", item);
                queryCommand.Parameters.Add("@metadata", MetaData);

                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);

                CacheItem cacheItem = _cache.GetCacheItem(Itemkey);

                var optionsFromCache = cacheItem.ResyncOptions;
                if (optionsFromCache == null || optionsFromCache.ResyncOnExpiration == false || optionsFromCache.ProviderName != providerName.ToLower())
                    throw new Exception("Failure: Add Resync Options");
                else
                    _report.AddPassedTestCase(methodName, "Success: Add File Dependency ");


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }



        }

        #endregion


        #region --------------------------------- Expiration ---------------------------

        private void NoneExpirationInJsonObject()
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            try
            {
                _cache.Clear();

                JsonObject MetaData = new JsonObject();

                JsonObject expiration = new JsonObject();
                expiration.AddAttribute("type", "None");
                expiration.AddAttribute("interval", 1);

                MetaData.AddAttribute("expiration", expiration);

                string key1 = "abc";
                var val = GetProduct();

                string query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                QueryCommand queryCommand = new QueryCommand(query);

                queryCommand.Parameters.Add("@key1", key1);
                queryCommand.Parameters.Add("@val", val);
                queryCommand.Parameters.Add("@metadata", MetaData);


                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);

                var ItemFromCache = _cache.GetCacheItem(key1);
                var expirationFromCache = ItemFromCache.Expiration;


                if (expirationFromCache.Type != ExpirationType.None)
                    throw new Exception("Cache item obtanined doesnot have None expiration");

                _report.AddPassedTestCase(methodName, "Success: Add None Expiration in JSON object");



            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }

        }

        private void SlidingExpirationInJsonObject()
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            try
            {
                _cache.Clear();

                JsonObject MetaData = new JsonObject();

                JsonObject expiration = new JsonObject();
                expiration.AddAttribute("type", "sliding");
                expiration.AddAttribute("interval", ExpirationTimes.SlidingExpirationSeconds);

                MetaData.AddAttribute("expiration", expiration);

                string key1 = "abc";
                var val = GetProduct();

                string query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                QueryCommand queryCommand = new QueryCommand(query);

                queryCommand.Parameters.Add("@key1", key1);
                queryCommand.Parameters.Add("@val", val);
                queryCommand.Parameters.Add("@metadata", MetaData);


                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);

                var ItemFromCache = _cache.GetCacheItem(key1);
                var expirationFromCache = ItemFromCache.Expiration;

                if (expirationFromCache.Type != ExpirationType.Sliding)
                    throw new Exception("Cache item obtanined doesnot have sliding expiration");

                int sleepTime = ExpirationTimes.SlidingExpirationSeconds * 1000 + _cleanIntervalSeconds * 1000;
                sleepTime = sleepTime + 2000;
                Console.WriteLine($"Waiting for {sleepTime} milli seconds before verifying sliding expiration");
                Thread.Sleep(sleepTime);

                ItemFromCache = _cache.GetCacheItem(key1);

                if (ItemFromCache == null)
                    _report.AddPassedTestCase(methodName, "Success: Add Sliding Expiration in JSON object");
                else
                    throw new Exception("Failure: Item didnot expired in sliding expiration");



            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }

        }

        private void AbsoluteExpirationInJsonObject()
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            try
            {
                _cache.Clear();

                JsonObject MetaData = new JsonObject();

                JsonObject expiration = new JsonObject();
                expiration.AddAttribute("type", "absolute");
                expiration.AddAttribute("interval", ExpirationTimes.AbosluteExpirationSeconds);

                MetaData.AddAttribute("expiration", expiration);

                string key1 = "abc";
                var val = GetProduct();

                string query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                QueryCommand queryCommand = new QueryCommand(query);

                queryCommand.Parameters.Add("@key1", key1);
                queryCommand.Parameters.Add("@val", val);
                queryCommand.Parameters.Add("@metadata", MetaData);

                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);

                var ItemFromCache = _cache.GetCacheItem(key1);

                if (ItemFromCache.Expiration.Type != ExpirationType.Absolute)
                    throw new Exception("Item obtained does not have absolute expiration");

                int sleepTime = ExpirationTimes.AbosluteExpirationSeconds * 1000 + _cleanIntervalSeconds * 1000;

                Console.WriteLine($"Waiting for {sleepTime} MilliSeconds for Testing Expiration ");
                Thread.Sleep(sleepTime+1000);

                var returned = _cache.Get<Alachisoft.NCache.Sample.Data.Product>(key1);
                if (returned != null)
                {
                    throw new Exception("item didnot expired ");
                }
                else
                {
                    _report.AddPassedTestCase(methodName, "Success:Add Absolute Expiration  in JSON object");
                }

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }

        }

        public void DefaultAbsoluteExpirationInJsonObject()
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            try
            {
                _cache.Clear();

                string key1 = "abc";
                var val = GetProduct();


                var directInsert = _cache.Insert(key1, new CacheItem(val) { Expiration = new Expiration(ExpirationType.DefaultAbsolute) });
                var directGetCacheItem = _cache.GetCacheItem(key1);
                var expectedExpirationType = directGetCacheItem.Expiration.Type;

                _cache.Remove(key1);


                JsonObject MetaData = new JsonObject();

                JsonObject expiration = new JsonObject();
                expiration.AddAttribute("type", "DefaultAbsolute");
                expiration.AddAttribute("interval", ExpirationTimes.DefaultAbsolute);

                MetaData.AddAttribute("expiration", expiration);




                string query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                QueryCommand queryCommand = new QueryCommand(query);

                queryCommand.Parameters.Add("@key1", key1);
                queryCommand.Parameters.Add("@val", val);
                queryCommand.Parameters.Add("@metadata", MetaData);


                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);

                var ItemFromCache = _cache.GetCacheItem(key1);
                var expirationFromCache = ItemFromCache.Expiration;


                if (expirationFromCache.Type != expectedExpirationType)
                    throw new Exception("Cache item obtanined doesnot have DefaultAbsolute expiration");

                var sleepTime = ExpirationTimes.DefaultAbsolute * 1000 + _cleanIntervalSeconds * 1000;
                Console.WriteLine($"Waiting for {sleepTime} milli seconds before verifying DefaultAbsolute expiration");
                Thread.Sleep(sleepTime);

                ItemFromCache = _cache.GetCacheItem(key1);

                if (ItemFromCache == null)
                    _report.AddPassedTestCase(methodName, "Success: Add DefaultAbsolute Expiration in JSON object");
                else
                    throw new Exception("Failure: item didnot expired after inserting with DefaultAbsolute JSON object");



            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }

        }

        private void DefaultAbsoluteLongerExpirationInJsonObject()
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            try
            {
                _cache.Clear();

                string key1 = "abc";
                var val = GetProduct();


                var directInsert = _cache.Insert(key1, new CacheItem(val) { Expiration = new Expiration(ExpirationType.DefaultAbsoluteLonger) });
                var directGetCacheItem = _cache.GetCacheItem(key1);
                var expectedExpirationType = directGetCacheItem.Expiration.Type;

                _cache.Remove(key1);


                JsonObject MetaData = new JsonObject();

                JsonObject expiration = new JsonObject();
                expiration.AddAttribute("type", "DefaultAbsoluteLonger");
                expiration.AddAttribute("interval", ExpirationTimes.DefaultAbsoluteLonger);

                MetaData.AddAttribute("expiration", expiration);


                string query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                QueryCommand queryCommand = new QueryCommand(query);

                queryCommand.Parameters.Add("@key1", key1);
                queryCommand.Parameters.Add("@val", val);
                queryCommand.Parameters.Add("@metadata", MetaData);


                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);

                var ItemFromCache = _cache.GetCacheItem(key1);
                var expirationFromCache = ItemFromCache.Expiration;


                if (expirationFromCache.Type != expectedExpirationType)
                    throw new Exception("Cache item obtanined doesnot have DefaultAbsoluteLonger expiration");

                var sleepTime = ExpirationTimes.DefaultAbsoluteLonger * 1000 + _cleanIntervalSeconds * 1000;
                Console.WriteLine($"Waiting for {sleepTime} milli seconds before verifying DefaultAbsoluteLonger expiration");

                Thread.Sleep(sleepTime);

                ItemFromCache = _cache.GetCacheItem(key1);

                if (ItemFromCache == null)
                    _report.AddPassedTestCase(methodName, "Success: Add DefaultAbsoluteLonger Expiration in JSON object");
                else
                    throw new Exception("Failure: Add DefaultAbsoluteLonger Metadata in JSON object");


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }

        }

        private void DefaultSlidingExpirationInJsonObject()
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            try
            {
                _cache.Clear();

                string key1 = "abc";
                var val = GetProduct();


                var directInsert = _cache.Insert(key1, new CacheItem(val) { Expiration = new Expiration(ExpirationType.DefaultSliding) });
                var directGetCacheItem = _cache.GetCacheItem(key1);
                var expectedExpirationType = directGetCacheItem.Expiration.Type;

                _cache.Remove(key1);


                JsonObject MetaData = new JsonObject();

                JsonObject expiration = new JsonObject();
                expiration.AddAttribute("type", "DefaultSliding");
                expiration.AddAttribute("interval", ExpirationTimes.DefaultSliding);

                MetaData.AddAttribute("expiration", expiration);



                string query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                QueryCommand queryCommand = new QueryCommand(query);

                queryCommand.Parameters.Add("@key1", key1);
                queryCommand.Parameters.Add("@val", val);
                queryCommand.Parameters.Add("@metadata", MetaData);


                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);

                var ItemFromCache = _cache.GetCacheItem(key1);
                var expirationFromCache = ItemFromCache.Expiration;


                if (expirationFromCache.Type != expectedExpirationType)
                    throw new Exception("Cache item obtanined doesnot have DefaultSliding expiration");

                var sleepTime = ExpirationTimes.DefaultSliding * 1000 + _cleanIntervalSeconds * 1000;
                Console.WriteLine($"Waiting for {sleepTime} milli seconds before verifying sliding expiration");
                Thread.Sleep(sleepTime);

                ItemFromCache = _cache.GetCacheItem(key1);

                if (ItemFromCache == null)
                    _report.AddPassedTestCase(methodName, "Success: Add DefaultSliding Expiration in JSON object");
                else
                    throw new Exception("Failure: Add DefaultSliding Metadata in JSON object");



            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }

        }

        private void DefaultSlidingLongerExpirationInJsonObject()
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            try
            {
                _cache.Clear();
                string key1 = "abc";
                var val = GetProduct();

                var directInsert = _cache.Insert(key1, new CacheItem(val) { Expiration = new Expiration(ExpirationType.DefaultSlidingLonger) });
                var directGetCacheItem = _cache.GetCacheItem(key1);
                var expectedExpirationType = directGetCacheItem.Expiration.Type;

                _cache.Remove(key1);

                JsonObject MetaData = new JsonObject();

                JsonObject expiration = new JsonObject();
                expiration.AddAttribute("type", "DefaultSlidingLonger");
                expiration.AddAttribute("interval", ExpirationTimes.DefaultSlidingLonger);

                MetaData.AddAttribute("expiration", expiration);



                string query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                QueryCommand queryCommand = new QueryCommand(query);

                queryCommand.Parameters.Add("@key1", key1);
                queryCommand.Parameters.Add("@val", val);
                queryCommand.Parameters.Add("@metadata", MetaData);


                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);

                var ItemFromCache = _cache.GetCacheItem(key1);
                var expirationFromCache = ItemFromCache.Expiration;


                if (expirationFromCache.Type != expectedExpirationType)
                    throw new Exception("Cache item obtanined doesnot have DefaultSlidingLonger expiration");

                var sleepTime = ExpirationTimes.DefaultSlidingLonger * 1000 + _cleanIntervalSeconds * 1000;
                Console.WriteLine($"Waiting for {sleepTime} milli seconds before verifying sliding longer expiration");

                Thread.Sleep(sleepTime);

                ItemFromCache = _cache.GetCacheItem(key1);

                if (ItemFromCache == null)
                    _report.AddPassedTestCase(methodName, "Success: Add DefaultSlidingLonger Expiration in JSON object");
                else
                    throw new Exception("Failure: Add DefaultSlidingLonger Metadata in JSON object");


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }

        }

        public void WrongExpiration()
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            try
            {
                _cache.Clear();

                string wrongExpiration = "WrongExpiration";

                 JsonObject MetaData = new JsonObject();

                JsonObject expiration = new JsonObject();
                expiration.AddAttribute("type", wrongExpiration);

                MetaData.AddAttribute("expiration", expiration);

                string key1 = "abc";
                var val = GetProduct();

                string query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                QueryCommand queryCommand = new QueryCommand(query);

                queryCommand.Parameters.Add("@key1", key1);
                queryCommand.Parameters.Add("@val", val);
                queryCommand.Parameters.Add("@metadata", MetaData);

                try
                {
                    var result = _cache.SearchService.ExecuteNonQuery(queryCommand);
                }
                catch (Exception ex)
                {
                    if (ex.Message.ToLower().Contains("expiration") && Helper.IsInCorrectMetaException(ex))
                    {
                        _report.AddPassedTestCase(methodName, "add wrong meta in query");
                        return;
                    }
                    else
                        throw;
                }

                throw new Exception("Didnot get exception in wrong expiration");

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }

        }


        #endregion


        #region --------------------------------- File Dependency ----------------------


        private void VerifyFileDependency()
        {


            string methodName = MethodBase.GetCurrentMethod().Name;
            _cache.Clear();
            string Itemkey = "KeyForFileDependency";
            Product item = GetProduct();

            try
            {
                string filePath = "C:\\\\dependencyFile.txt";
                File.AppendAllText(filePath, $"\n {methodName} => This file is used to verify file dependency in ncache by queries. {DateTime.Now}");

                string query = "INSERT INTO Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                var jsonString = "{\"dependency\": [{\"file\": {\"fileNames\":[" + $"\"{filePath}\"" + "]}}]}";

                var jsonObj = new JsonObject(jsonString);



                // FileDependency 
                // SqlCacheDependency 


                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@key1", Itemkey);
                queryCommand.Parameters.Add("@val", item);
                queryCommand.Parameters.Add("@metadata", jsonObj);

                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);

                File.AppendAllText(filePath, $"\n {methodName} => Modifying file to verify dependency at time : {DateTime.Now}");

                var cacheItem = _cache.GetCacheItem(Itemkey);
                if (cacheItem == null)
                    _report.AddPassedTestCase(methodName, "Success: Add File Dependency ");

                else
                    throw new Exception("Failure: Add File Dependency ");

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }



        }

        private void VerifyFileDependencyWithArrayOfFiles()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            _cache.Clear();
            string Itemkey = "KeyForFileDependency";
            Product item = GetProduct();

            try
            {
                string filePath = "C:\\\\dependencyFile.txt";
                File.AppendAllText(filePath, $"\n {methodName} => This file is used to verify file dependency in ncache by queries. {DateTime.Now}");

                string query = "INSERT INTO Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                var jsonString = "{\"dependency\": [{\"file\": {\"filenames\":[" + $"\"{filePath}\"" + "]}}]}";

                var jsonObj = new JsonObject(jsonString);


                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@key1", Itemkey);
                queryCommand.Parameters.Add("@val", item);
                queryCommand.Parameters.Add("@metadata", jsonObj);

                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);

                File.AppendAllText(filePath, $"\n {methodName} => Modifying file to verify dependency at time : {DateTime.Now}");

                var cacheItem = _cache.GetCacheItem(Itemkey);
                if (cacheItem == null)
                    _report.AddPassedTestCase(methodName, "Success: Add File Dependency ");

                else
                    throw new Exception("Failure: Add File Dependency ");

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }



        }

        private void FileDependencyWithStartAfter()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            _cache.Clear();
            string Itemkey = "KeyForFileDependency";
            Product item = GetProduct();

            try
            {
                int startAfter = 5;

                string filePath = "C:\\\\dependencyFile.txt";
                File.AppendAllText(filePath, $"\n {methodName} => This file is used to verify file dependency in ncache by queries. {DateTime.Now}");

                string query = "INSERT INTO Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                var jsonString = "{\"dependency\": [{\"file\":" +
                    " {\"fileNames\":" + $"[\"{filePath}\"]" +
                    ",\"interval\":" + $"\"{startAfter}\""
                    + "}}]}";

                var jsonObj = new JsonObject(jsonString);

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@key1", Itemkey);
                queryCommand.Parameters.Add("@val", item);
                queryCommand.Parameters.Add("@metadata", jsonObj);

                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);

                var cacheItem = _cache.GetCacheItem(Itemkey);
                var dependecy = cacheItem.Dependency.Dependencies.FirstOrDefault() as FileDependency;


                File.AppendAllText(filePath, $"\n {methodName} => Modifying file to verify dependency {DateTime.Now}");

                cacheItem = _cache.GetCacheItem(Itemkey);

                if (cacheItem == null)
                    throw new Exception("File dependency triggered before start after ticks");

                Console.WriteLine($"waiting for {startAfter} seconds for file dependency");
                Thread.Sleep(startAfter * 1000);

                cacheItem = _cache.GetCacheItem(Itemkey);

                if (cacheItem == null)
                    _report.AddPassedTestCase(methodName, "Success: Add File Dependency with start after");
                else
                    throw new Exception("Failure:Add File Dependency with start after");

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }



        }

        #endregion


        #region --------------------------------- Cache item version  ------------------

        public void VerifyCacheItemVersion()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            _cache.Clear();
            string Itemkey = "KeyForCacheItemVersion";
            Product item = GetProduct();

            try
            {
                string guidKey = Guid.NewGuid().ToString();

                CacheItemVersion version = _cache.Insert(guidKey, item);
                ulong itemVersion = version.Version;

                string query = "UPSERT INTO Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                var jsonObj = new JsonObject();
                jsonObj.AddAttribute("CacheItemVersion", itemVersion);



                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@key1", Itemkey);
                queryCommand.Parameters.Add("@val", item);
                queryCommand.Parameters.Add("@metadata", jsonObj);

                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);

                var cacheItem = _cache.GetCacheItem(Itemkey);

                if (cacheItem == null)
                    throw new Exception("Failure: Add item with item version ");

                if (cacheItem.Version.Version == itemVersion)
                    throw new Exception($"Cache item version {itemVersion} is not updated");

                _report.AddPassedTestCase(methodName, "Success: Add item with item version ");


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }



        }

        #endregion


        #region --------------------------------- Multiple Dependencies -----------------


        public void KeyAndFileDependency()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string Itemkey = "KeyAndFileDependency";
            Product item = GetProduct();
            string description = "Verify key and file dependency with one master key with Remove enum";

            try
            {
                string filePath = "C:\\\\dependencyFile.txt";
                File.AppendAllText(filePath, $"\n {methodName} => This file is used to verify file dependency in ncache by queries. {DateTime.Now}");

                var masterKey = Guid.NewGuid().ToString();
                _cache.Insert(masterKey, item);

                string query = "INSERT INTO Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";


                string keysArray = $"[\"{masterKey}\"]";
                string keyDependency = "{\"keys\" :" + keysArray + ", \"type\" : \"removeonly\"}";


                var fileDependency = "{'fileNames':['C:\\\\dependencyFile.txt' ], 'interval' : 5}";

                string jsonString = "{'dependency':[{'key':" + keyDependency + "},{'file':" + fileDependency + "}]}";

                jsonString = jsonString.Replace("'", "\"");

                var jsonObject = new JsonObject(jsonString);

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@key1", Itemkey);
                queryCommand.Parameters.Add("@val", item);
                queryCommand.Parameters.Add("@metadata", jsonObject);

                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);

                var cacheItem = _cache.GetCacheItem(Itemkey);

                if (cacheItem == null)
                    throw new Exception("item not inserted with key and file  dependency");

                if (cacheItem.Dependency == null)
                    throw new Exception(" dependency is not added with cache item");

                //update master key
                _cache.Insert(masterKey, item);

                if (_cache.GetCacheItem(Itemkey) == null)
                    throw new Exception($"Updating master key triggerd the dependency");

                _cache.Remove(masterKey);

                if (_cache.GetCacheItem(Itemkey) != null)
                    throw new Exception($"Removing master key didnot triggered key dependency");

                // now verify file dependency
                _cache.Insert(masterKey, item);
                _cache.SearchService.ExecuteNonQuery(queryCommand);

                File.AppendAllText(filePath, $"\n {methodName} => updating the file to verify dependency {DateTime.Now}");

                Console.WriteLine("waiting for 5 seconds to verify dependency");
                Thread.Sleep(5000);


                if (_cache.GetCacheItem(Itemkey) != null)
                    throw new Exception($"updating the file  didnot triggered key dependency");

                _report.AddPassedTestCase(methodName, description);

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }



        }


        #endregion


        private Product GetProduct()
        {

            return new Product() { Id = 1, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 357, Order = new Alachisoft.NCache.Sample.Data.Order { OrderID = 10, ShipCity = "rawalpindi" } };
        }

    }

    public static class ExpirationTimes
    {
        public const int SlidingExpirationSeconds = 5;
        public const int AbosluteExpirationSeconds = 5;

        public const int DefaultAbsolute = 5;
        public const int DefaultAbsoluteLonger = 10;

        public const int DefaultSliding = 5;
        public const int DefaultSlidingLonger = 10;

    }

}
