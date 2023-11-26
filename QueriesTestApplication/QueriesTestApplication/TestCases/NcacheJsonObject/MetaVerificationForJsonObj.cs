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

using Newtonsoft.Json.Linq;
using Alachisoft.NCache.Runtime;
using System.Data.SqlClient;
using QueriesTestApplication.Utils;

namespace QueriesTestApplication
{
    class MetaVerificationTestForJsonObj
    {

        private int count = 0;
        ICache cache;
        public Dictionary<string, ResultStatus> testResults;
        private Report _report;

        public Dictionary<string, ResultStatus> TestResults
        {
            get { return testResults; }
        }

        public Report Report { get => _report; }

        public MetaVerificationTestForJsonObj()
        {
            cache = CacheManager.GetCache(Common.CacheName);
            testResults = new Dictionary<string, ResultStatus>();

        }

        #region --------------------------------- Priority  ---------------------------------


        /// <summary>
        /// Adds JSON Object as MetaData having  information of Priority
        /// </summary>
        public void PriorityMetadataInJsonObject()
        {
            var methodName = "PriorityMetadataInJsonObject";
            try
            {
                cache.Clear();

                JsonObject MetaData = new JsonObject();
                MetaData.AddAttribute("Priority", "AboveNormal");

                string key1 = "key_PriorityMetadataInJsonObject";
                var val = GetProduct();

                string query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                QueryCommand queryCommand = new QueryCommand(query);

                queryCommand.Parameters.Add("@key1", key1);
                queryCommand.Parameters.Add("@val", val);
                queryCommand.Parameters.Add("@metadata", MetaData);

                var result = cache.SearchService.ExecuteNonQuery(queryCommand);

                var cacheItem = cache.GetCacheItem(key1);

                var PriorityAttribute = cacheItem.Priority;

                if (PriorityAttribute == CacheItemPriority.AboveNormal)
                {
                    _report.AddPassedTestCase(methodName,"Success: Add Priority Metadata In JSON Object ");
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

        #region --------------------------------- Expiration ---------------------------------

        /// <summary>
        /// Adds JSON Object having Metadata information of Expiration 
        /// </summary>
        /// <remarks>
        /// Waits for 25 seconds, for Clean Interval to Remove Expired object
        /// </remarks>
        private void ExpirationMetadataInJsonObject()
        {
            var methodName = "ExpirationMetadataInJsonObject";
            try
            {
                cache.Clear();

                JsonObject MetaData = new JsonObject();

                JsonObject expiration = new JsonObject();
                expiration.AddAttribute("type", "absolute");
                expiration.AddAttribute("interval", 2);

                MetaData.AddAttribute("expiration", expiration);

                string key1 = "abc";
                var val = GetProduct();

                string query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                QueryCommand queryCommand = new QueryCommand(query);

                queryCommand.Parameters.Add("@key1", key1);
                queryCommand.Parameters.Add("@val", val);
                queryCommand.Parameters.Add("@metadata", MetaData);


                var result = cache.SearchService.ExecuteNonQuery(queryCommand);

                var ItemFromCache = cache.GetCacheItem(key1);

                Console.WriteLine("Waiting for 25 Seconds for Testing Expiration ");
                Thread.Sleep(25000);
                var returned = cache.Get<Alachisoft.NCache.Sample.Data.Product>(key1);
                if (returned != null)
                {
                    throw new Exception("Failure: Add ExpirationMetadata in JSON object");
                }
                else
                {
                    _report.AddPassedTestCase(methodName, "Success: Add ExpirationMetadata in JSON object");
                }

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }

        }

        #endregion

        #region --------------------------------- Named Tags ---------------------------------

        /// <summary>
        ///  Adds JSON Object having Metadata information of NameTags  
        /// </summary>
        public void NameTagMetadataInJSONObject()
        {
            var methodName = "NameTagMetadataInJSONObject";
            try
            {
                cache.Clear();

                //JSON Object to be Added as MetaData
                var MetaData = new JsonObject();

                //NameTags
                JsonObject FlashDiscount = new JsonObject();
                FlashDiscount.AddAttribute("FlashDiscount", "NoFlashDiscount");
                FlashDiscount.AddAttribute("type", "string");

                JsonObject Discount = new JsonObject();
                Discount.AddAttribute("Discount", "Yes");
                Discount.AddAttribute("type", "string");

                //NameTagArray Containing all the NameTags
                JsonArray NameTagsArray = new JsonArray();
                NameTagsArray.Add(FlashDiscount);
                NameTagsArray.Add(Discount);

                //Adding nameTagArray in JsonObject
                MetaData.AddAttribute("namedtags", NameTagsArray);

                string key1 = "abc";
                var val = GetProduct();

                string query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values (@key1, @val,@metadata)";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@key1", key1);
                queryCommand.Parameters.Add("@val", val);
                queryCommand.Parameters.Add("@metadata", MetaData);

                var result = cache.SearchService.ExecuteNonQuery(queryCommand);
                var cacheItem = cache.GetCacheItem(key1);

                var Attributes = cacheItem.NamedTags;

                if (Attributes.Contains("Discount") && Attributes.Contains("FlashDiscount"))
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

        #region --------------------------------- Tags ---------------------------------

        /// <summary>
        /// Adds JSON Object having Metadata information of TAGS
        /// </summary>
        public void TagsMetadataInJSONObject()
        {
            var methodName = "TagsMetadataInJSONObject";
            try
            {
                cache.Clear();

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

                var result = cache.SearchService.ExecuteNonQuery(queryCommand);

                Tag[] SearchTags = new Tag[2] { new Tag("Important Product"), new Tag("Imported Product") };
                //Tag[] SearchTags = new Tag[2] { new Tag("\"Important Product\""), new Tag("Imported Product") };
                IDictionary<string, Product> data = cache.SearchService.GetByTags<Product>(SearchTags, TagSearchOptions.ByAnyTag);

                if (data.Count > 0)
                {
                    _report.AddPassedTestCase(methodName, "Success: Add TagsMetaData in JSON object");
                }
                else
                {
                    throw new Exception("Failure: Add TagsMetaData in JSON object");
                }

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

}
