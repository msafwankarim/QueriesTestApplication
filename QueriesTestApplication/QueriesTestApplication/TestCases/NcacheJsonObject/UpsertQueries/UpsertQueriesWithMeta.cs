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
using System.Collections;

namespace QueriesTestApplication
{
    class UpsertQueriesWithMeta
    {
        private readonly ICache _cache;
        private readonly Report _report;
        private readonly int _cleanIntervalSeconds = 5;
        private string _key = "key";

        private int DependencyWaitTime { get => (_cleanIntervalSeconds + 1) * 1000; }

        public Report Report { get => _report; }

        public UpsertQueriesWithMeta()
        {
            _cache = CacheManager.GetCache(Common.CacheName);
            _report = new Report(nameof(UpsertQueriesWithMeta));
        }

        public void AddAndGetSimpleItemForUpsert()
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            try
            {
                _cache.Clear();

                string key1 = _key;
                var val = GetProduct();

                string query = "Upsert into Alachisoft.NCache.Sample.Data.Product (Key,Value) Values (@key1, @val)";

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


        public void AboveNormalPriority()
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            try
            {
                _cache.Clear();

                var item = InsertCacheItem();

                JsonObject MetaData = new JsonObject();
                MetaData.AddAttribute("Priority", "AboveNormal");

                string key1 = _key;
                var val = GetProduct();

                string query = "Upsert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

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

        #endregion


        #region --------------------------------- Group  -------------------------------

        public void GroupMetadataInJsonObject()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;

            try
            {
                _cache.Clear();

                var item = InsertCacheItem();

                string JsonString = "{\"group\":\"DevTeam\"}";
                var MetaData = new JsonObject(JsonString);

                string key1 = _key;
                var val = GetProduct();

                string query = "Upsert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

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
                var item = InsertCacheItem();

                var MetaData = new JsonObject();
                JsonArray tags = new JsonArray();
                tags.Add("Important Product");

                MetaData.AddAttribute("tags", tags);

                string key1 = _key;
                var val = GetProduct();

                string query = "Upsert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

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

        #endregion


        #region --------------------------------- Named Tags ---------------------------

        public void NameTagMetadataInJSONObject()
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            try
            {
                _cache.Clear();

                var preInsert = InsertCacheItem();


                JsonObject MetaData = new();

                MetaData.AddAttribute("namedtags", Helper.GetNamedTagsArray());


                string key1 = _key;
                var val = GetProduct();

                string query = "Upsert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@key1", key1);
                queryCommand.Parameters.Add("@val", val);
                queryCommand.Parameters.Add("@metadata", MetaData);

                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);
                var cacheItem = _cache.GetCacheItem(key1);

                var Attributes = cacheItem.NamedTags;

                foreach (DictionaryEntry namedTag in Attributes)
                {
                    if (namedTag.Key.ToString() == "FlashDiscount" && namedTag.Value.ToString() != "NoFlashDiscount")
                        throw new Exception("named tag is not updated ");
                }

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
            string Itemkey = _key;
            Product item = GetProduct();
            string description = "Verify key dependency ";

            try
            {
                var preInsert = InsertCacheItem();


                var masterKey = Guid.NewGuid().ToString();
                _cache.Insert(masterKey, item);

                string query = "Upsert INTO Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";


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



        #endregion


        #region --------------------------------- Resync Options  ----------------------

        private void VerifyResyncOption()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            _cache.Clear();
            string Itemkey = _key;
            Product item = GetProduct();

            try
            {

                var preInsert = InsertCacheItem();


                string providerName = "read";
                string query = "Upsert INTO Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";


                JsonObject MetaData = new JsonObject();

                JsonObject expiration = new JsonObject();
                expiration.AddAttribute("type", "sliding");
                expiration.AddAttribute("interval", ExpirationTimes.SlidingExpirationSeconds);

                MetaData.AddAttribute("expiration", expiration);


                JsonObject options = new JsonObject();
                options.AddAttribute("ResyncOnExpiration", "true");
                options.AddAttribute("providerName", providerName);

                MetaData.AddAttribute("ResyncOptions", options);


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

      
        private void AbsoluteExpirationInJsonObject()
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            try
            {
                _cache.Clear();

                var preInsert = InsertCacheItem();


                JsonObject MetaData = new JsonObject();

                JsonObject expiration = new JsonObject();
                expiration.AddAttribute("type", "absolute");
                expiration.AddAttribute("interval", ExpirationTimes.AbosluteExpirationSeconds);

                MetaData.AddAttribute("expiration", expiration);

                string key1 = _key;
                var val = GetProduct();

                string query = "Upsert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

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
                Thread.Sleep(sleepTime + 1000);

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



        #endregion


        #region --------------------------------- File Dependency ----------------------



        private void VerifyFileDependencyWithArrayOfFiles()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            _cache.Clear();
            string Itemkey = _key;
            Product item = GetProduct();

            try
            {
                var preInsert = InsertCacheItem();

                string filePath = "C:\\\\dependencyFile.txt";
                File.AppendAllText(filePath, $"\n {methodName} => This file is used to verify file dependency in ncache by queries. {DateTime.Now}");

                string query = "Upsert INTO Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

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



        #endregion


        #region --------------------------------- Wrong Query ----------------------



        private void WrongQuery()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            _cache.Clear();
            string Itemkey = _key;
            Product item = GetProduct();

            try
            {
                var preInsert = InsertCacheItem();

                string filePath = "C:\\\\dependencyFile.txt";
                File.AppendAllText(filePath, $"\n {methodName} => This file is used to verify file dependency in ncache by queries. {DateTime.Now}");

                string query = "Upsert INTO Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata) where id = 1";

                var jsonString = "{\"dependency\": [{\"file\": {\"filenames\":[" + $"\"{filePath}\"" + "]}}]}";

                var jsonObj = new JsonObject(jsonString);


                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@key1", Itemkey);
                queryCommand.Parameters.Add("@val", item);
                queryCommand.Parameters.Add("@metadata", jsonObj);

                var result = _cache.SearchService.ExecuteNonQuery(queryCommand);
                
                throw new Exception("Failure: wrong query");

            }
            catch (Exception ex)
            {
                if(Helper.IsIncorrectFormatException(ex))
                    _report.AddPassedTestCase(methodName, "incorrect query ");
                else
                    _report.AddFailedTestCase(methodName, ex);
            }



        }



        #endregion


        #region --------------------------------- Multiple Dependencies -----------------


        public void KeyAndFileDependency()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string Itemkey = _key;
            Product item = GetProduct();
            string description = "Verify key and file dependency with one master key with Remove enum";

            try
            {
                var preInsert = InsertCacheItem();


                string filePath = "C:\\\\dependencyFile.txt";
                File.AppendAllText(filePath, $"\n {methodName} => This file is used to verify file dependency in ncache by queries. {DateTime.Now}");

                var masterKey = Guid.NewGuid().ToString();
                _cache.Insert(masterKey, item);

                string query = "Upsert INTO Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";


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

        private CacheItem GetCacheItem()
        {

            var product = new Product() { Id = 1, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 357, Order = new Alachisoft.NCache.Sample.Data.Order { OrderID = 10, ShipCity = "rawalpindi" } };

            CacheItem item = new CacheItem(product);
            item.NamedTags = MetaInfo.NamedTags;
            item.Tags = MetaInfo.Tags;
            item.Group = MetaInfo.Group;
            item.Priority = MetaInfo.Priority;

            return item;

        }

        private CacheItem InsertCacheItem()
        {
            _cache.Insert(_key,GetCacheItem());
            return GetCacheItem();
        }


    }

    class MetaInfo
    {

        public static  NamedTagsDictionary NamedTags { get => GetNamedTags(); }
        public static Tag[] Tags { get => GetTags(); }
        public static string Group { get => "Diyatech"; }
        public static CacheItemPriority Priority { get => CacheItemPriority.Normal; }


        private static Tag[] GetTags()
        {
            Tag[] tags = new Tag[2];
            tags[0] = new Tag("East Coast Product");
            tags[1] = new Tag("Important Product");
            return tags;
        }

        private static NamedTagsDictionary GetNamedTags()
        {
            var namedTags = new NamedTagsDictionary();
            namedTags.Add("discount", Convert.ToDecimal(0.5));     
            return namedTags;
        }
    }

}
