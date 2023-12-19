using Alachisoft.NCache.Client;
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
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.CodeDom;
using System.Collections;

namespace QueriesTestApplication
{
    class MetaVerificationInInlineQuery
    {
        readonly ICache cache;

        readonly Report _report;

        readonly int _cleanIntervalSeconds = 5000 + 1000; // milliSeconds
        public Report Report { get => _report; }
        List<Product> productList;

        string _groupName = "products";


        public MetaVerificationInInlineQuery()
        {
            cache = CacheManager.GetCache(Common.CacheName);
            _report = new Report(nameof(MetaVerificationInInlineQuery));
            productList = new List<Product>();
        }


        #region ----------------------------------------------- Set-Meta ----------------------------------------------- 


        #region --------------------------------- Group  -------------------------------
        public void UpdateGroup()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "Update group by meta query";


            try
            {
                cache.Clear();
                PopulateCacheWithMeta();

                string groupname = "vip_prod";
                string catagory = "Beverages";

                string query = $"Update  Alachisoft.NCache.Sample.Data.Product Set-meta $group$ = '\"{groupname}\"' where Category = @beverages";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", catagory);

                var updated = cache.QueryService.ExecuteNonQuery(queryCommand);

                foreach (var item in productList)
                {
                    if (item.Category != catagory)
                        continue;

                    var prod = cache.Get<Product>("product" + item.Id);
                    var cacheItem = cache.GetCacheItem("product" + item.Id);
                    var jsonProd = cache.Get<JsonObject>("product" + item.Id);

                    if (cacheItem.Group != groupname)
                        throw new Exception(description);

                }

                // some extra verification
                var returned = cache.SearchService.GetGroupKeys(groupname);
                if (returned.Count != updated)
                    throw new Exception(description);

                _report.AddPassedTestCase(methodName, description);


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }
        }

        public void UpdateGroupWithSomeSetAttributes()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "Update group attribute with some set operations";


            try
            {
                cache.Clear();
                PopulateCacheWithMeta();

                string groupname = "vip_prod";
                string teaName = "PinkTea";
                string importance = "VIP_Product";
                string city = "VIP_Product";

                var count = cache.Count;

                string query = $"Update  Alachisoft.NCache.Sample.Data.Product Set this.Name = '\"{teaName}\"', this.Order.ShipCity = @lahore Add this.Order.Type = @important  Set-meta $group$ = '\"{groupname}\"' where Category = @beverages";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@important", importance);
                queryCommand.Parameters.Add("@lahore", city);

               count = cache.Count;

                var updated = cache.QueryService.ExecuteNonQuery(queryCommand);

                foreach (var item in productList)
                {
                    var prod = cache.Get<Product>("product" + item.Id);
                    var cacheItem = cache.GetCacheItem("product" + item.Id);

                    var jsonProd = cache.Get<JsonObject>("product" + item.Id);
                    if (item.Category == "Beverages")
                    {
                        var order = (JsonObject)jsonProd.GetAttributeValue("Order");

                        string addedType = order.GetAttributeValue("Type").ToString();

                        bool allValuesMatches =
                            prod.Name == teaName &&
                            prod.Order.ShipCity == city &&
                            addedType.Contains(importance) &&
                            cacheItem.Group == groupname;

                        if (!allValuesMatches)
                            throw new Exception(description);

                    }
                }

                // some extra verification
                var returned = cache.SearchService.GetGroupKeys(groupname);
                if (returned.Count != updated)
                    throw new Exception(description);

                _report.AddPassedTestCase(methodName, description);


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }
        }


        #endregion


        #region --------------------------------- Tag  ---------------------------------
        public void UpdateTag()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "Update tags by meta query";


            try
            {
                cache.Clear();
                PopulateCacheWithMeta();

                var tagsArray = new JsonArray();
                tagsArray.Add("updatedTag0");
                tagsArray.Add("updatedTag1");
                tagsArray.Add("updatedTag2");

                string catagory = "Beverages";

                string query = $"Update  Alachisoft.NCache.Sample.Data.Product Set-meta $tag$ = '{tagsArray}' where Category = @beverages";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", catagory);

                var updated = cache.QueryService.ExecuteNonQuery(queryCommand);

                foreach (var item in productList)
                {
                    if (item.Category != catagory)
                        continue;

                    var prod = cache.Get<Product>("product" + item.Id);
                    var cacheItem = cache.GetCacheItem("product" + item.Id);
                    var jsonProd = cache.Get<JsonObject>("product" + item.Id);

                    var tagsFromCache = cacheItem.Tags;

                    var tag = tagsFromCache.
                        Where(tag => tag.TagName == tagsArray[1].Value.ToString());

                    if (tag.First().TagName == null )
                        throw new Exception(description);

                }


                _report.AddPassedTestCase(methodName, description);


            }

            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }
        }

        #endregion


        #region --------------------------------- NamedTag  ----------------------------
        public void UpdateNamedTag()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "Update NamedTag by meta query";


            try
            {
                cache.Clear();
                PopulateCacheWithMeta();

                JsonArray namedTags = Helper.GetNamedTagsArray();

                string catagory = "Beverages";

                string query = $"Update  Alachisoft.NCache.Sample.Data.Product Set-meta $NamedTag$ = @namedTag where Category = @beverages";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", catagory);
                queryCommand.Parameters.Add("@namedTag", namedTags);


                var updated = cache.QueryService.ExecuteNonQuery(queryCommand);

                foreach (var item in productList)
                {
                    if (item.Category != catagory)
                        continue;

                    var prod = cache.Get<Product>("product" + item.Id);
                    var cacheItem = cache.GetCacheItem("product" + item.Id);
                    var jsonProd = cache.Get<JsonObject>("product" + item.Id);

                    NamedTagsDictionary Attributes = cacheItem.NamedTags;

                    foreach (DictionaryEntry namedTag in Attributes)
                    {
                        if (namedTag.Key.ToString() == "FlashDiscount" && namedTag.Value.ToString() != "NoFlashDiscount")
                            throw new Exception("named tag is not updated ");
                    }

                    if (Attributes.Contains("Discount") && Attributes.Contains("FlashDiscount") && Attributes.Contains("Percentage"))
                        continue;

                    else
                        throw new Exception(description);
                }

                _report.AddPassedTestCase(methodName, "Success: Add NameTagMetaData in JSON object");

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }
        }

        #endregion




        #endregion


        #region ----------------------------------------------- Remove-Meta --------------------------------------------


        #region --------------------------------- Group  -------------------------------

        public void RemoveGroup()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "remove group by meta query";


            try
            {
                cache.Clear();
                PopulateCacheWithMeta();

                string catagory = "Beverages";

                string query = $"Update  Alachisoft.NCache.Sample.Data.Product REMOVE-META $group$ = '\"{_groupName}\"' where Category = @beverages";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", catagory);

                var updated = cache.QueryService.ExecuteNonQuery(queryCommand);

                foreach (var item in productList)
                {
                    if (item.Category != catagory)
                        continue;

                    var prod = cache.Get<Product>("product" + item.Id);
                    var cacheItem = cache.GetCacheItem("product" + item.Id);
                    var jsonProd = cache.Get<JsonObject>("product" + item.Id);

                    if (cacheItem.Group == _groupName)
                        throw new Exception(description);

                }

                _report.AddPassedTestCase(methodName, description);



            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }
        }

        public void RemoveGroupWithSomeSetAttributes()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "remove group attribute with some set operations";


            try
            {
                cache.Clear();
                PopulateCacheWithMeta();


                string groupname = _groupName;
                string teaName = "PinkTea";
                string importance = "VIP_Product";
                string city = "VIP_Product";

                string query = $"Update  Alachisoft.NCache.Sample.Data.Product Set this.Name = @tea, this.Order.ShipCity = @lahore Add this.Order.Type = @important  remove-meta $group$ = '\"{groupname}\"' where Category = @beverages";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@tea", teaName);
                queryCommand.Parameters.Add("@important", importance);
                queryCommand.Parameters.Add("@lahore", city);


                var updated = cache.QueryService.ExecuteNonQuery(queryCommand);

                foreach (var item in productList)
                {
                    var prod = cache.Get<Product>("product" + item.Id);
                    var cacheItem = cache.GetCacheItem("product" + item.Id);

                    var jsonProd = cache.Get<JsonObject>("product" + item.Id);
                    if (item.Category == "Beverages")
                    {
                        var order = (JsonObject)jsonProd.GetAttributeValue("Order");

                        string addedType = order.GetAttributeValue("Type").ToString();

                        bool allValuesMatches =
                            prod.Name == teaName &&
                            prod.Order.ShipCity == city &&
                            addedType.Contains(importance) &&
                            (cacheItem.Group == null || cacheItem.Group != groupname);

                        if (!allValuesMatches)
                            throw new Exception(description);

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


        #region --------------------------------- Tag  ---------------------------------

        public void RemoveTagThatDoesnotExist()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "remove tags that doesnot exist by remove-meta query";


            try
            {
                cache.Clear();
                PopulateCacheWithMeta();

                var tagsArray = new JsonArray();
                tagsArray.Add("updatedTag0");
                tagsArray.Add("updatedTag1");
                tagsArray.Add("updatedTag2");

                string catagory = "Beverages";

                string query = $"Update  Alachisoft.NCache.Sample.Data.Product remove-meta $tag$ = '{tagsArray}' where Category = @beverages";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", catagory);

                var updated = cache.QueryService.ExecuteNonQuery(queryCommand);

                foreach (var item in productList)
                {
                    if (item.Category != catagory)
                        continue;

                    var prod = cache.Get<Product>("product" + item.Id);
                    var cacheItem = cache.GetCacheItem("product" + item.Id);
                    var jsonProd = cache.Get<JsonObject>("product" + item.Id);

                    var tagsFromCache = cacheItem.Tags;

                    if (tagsFromCache.Length == 0)
                        throw new Exception("all tags are removed ");


                    foreach (Tag tag in tagsFromCache)
                    {
                        if (tagsArray.Contains(tag.TagName ))
                            throw new Exception(description);
                    }
                    
                }

                _report.AddPassedTestCase(methodName, description);


            }

            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }
        }

        public void RemoveTag()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "remove tags by remove-meta query";


            try
            {
                cache.Clear();
                PopulateCacheWithMeta();

                var tagsArray = new JsonArray();
                tagsArray.Add("updatedTag0");
                tagsArray.Add("East Coast Product");


                string catagory = "Beverages";

                string query = $"Update  Alachisoft.NCache.Sample.Data.Product Remove-meta $tag$ = '{tagsArray}'  where Category = @beverages";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", catagory);

                var updated = cache.QueryService.ExecuteNonQuery(queryCommand);

                foreach (var item in productList)
                {
                    if (item.Category != catagory)
                        continue;

                    var prod = cache.Get<Product>("product" + item.Id);
                    var cacheItem = cache.GetCacheItem("product" + item.Id);
                    var jsonProd = cache.Get<JsonObject>("product" + item.Id);

                    var tagsFromCache = cacheItem.Tags;

                    if (tagsFromCache.Length == 0)
                        throw new Exception("all tags are removed");


                    foreach (Tag tag in tagsFromCache)
                    {
                        if (tagsArray.Contains(tag.TagName))
                            throw new Exception(description);
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


        #region --------------------------------- NamedTag  ----------------------------
        public void RemoveNamedTag()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "remove NamedTag by meta query";


            try
            {
                cache.Clear();
                PopulateCacheWithMeta();

                JsonArray namedTagsArray = new JsonArray();
                namedTagsArray.Add(GetNamedTags().Item1); 

                string catagory = "Beverages";

                string query = $"Update  Alachisoft.NCache.Sample.Data.Product Remove-meta $NamedTag$ = '{namedTagsArray}'  where Category = @beverages";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", catagory);

                var updated = cache.QueryService.ExecuteNonQuery(queryCommand);

                foreach (var item in productList)
                {
                    if (item.Category != catagory)
                        continue;

                    var prod = cache.Get<Product>("product" + item.Id);
                    var cacheItem = cache.GetCacheItem("product" + item.Id);
                    var jsonProd = cache.Get<JsonObject>("product" + item.Id);

                    NamedTagsDictionary Attributes = cacheItem.NamedTags;

                    if (Attributes == null || Attributes.Count == 0)
                        continue;
                    
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

        #endregion



        #endregion

        public void ComplexQuery()
        {
            int updated = 0;
            cache.Clear();
            PopulateCacheWithMeta();
            string methodName = MethodBase.GetCurrentMethod().Name;
            int totalExpectedException = 1;

            string groupName = "aqib";
            string newTagToBeAdded = "tagged";
            string newNamedTagToBeAdded = "newNamedTag";

            var tagsArray = new JsonArray();
            tagsArray.Add("Important Product");

            JsonArray namedTags = Helper.GetNamedTagsArray();

            try
            {
                string nameThatDoesnotExist = "\"I drink two cups of tea daily.\"";
                DateTime birthDayTime = new DateTime(2000, 1, 5);

                string query = $"Update  Alachisoft.NCache.Sample.Data.Product " +
                    $" Add nameThatDoesnotExist = @nameThatDoesnotExist " +
                    $" Test nameThatDoesnotExist = @nameThatDoesnotExist " +
                    $" Set Time = @birthDayTime " +
                    $" Copy ClassName = Category " +
                    $" Move Id = Order.OrderID " +
                    $" Remove nameThatDoesnotExist " +
                    $" set-meta $group$ = '\"{groupName}\"', $tag$ = '[\"{newTagToBeAdded}\"]', $namedtag$ = '[ {{\"FlashDiscount\" : \"NoFlashDiscount\", \"type\" : \"string\" }}]' " +
                    $" remove-meta $tag$ = '{tagsArray}', $namedtag$ = '[\"{GetNamedTags().Item1}\"]' " +
                    $" Where Id = @id";


                totalExpectedException = 0;

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@nameThatDoesnotExist", nameThatDoesnotExist);
                queryCommand.Parameters.Add("@groupName", groupName);
                queryCommand.Parameters.Add("@tagToBeRemoved", tagsArray);
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

                // Meta Verification

                var cacheItem = cache.GetCacheItem("product1");

                if (cacheItem.Group != groupName)
                    throw new Exception("group name is not updated");


                var tagsFromCache = cacheItem.Tags;

                if (tagsFromCache.Length == 0)
                    throw new Exception("all tags are removed");

                bool newTagFound = false;

                foreach (Tag tag in tagsFromCache)
                {
                    if (tagsArray.Contains(tag.TagName))
                        throw new Exception("tag is not removed");

                    if(tag.TagName.ToString().Equals(newTagToBeAdded))
                        newTagFound = true;
                }

                if (!newTagFound)
                    throw new Exception("tag is not updated ");

                var namedTagsFromCache = cacheItem.NamedTags;

                if(namedTagsFromCache.Count == 0)
                    throw new Exception("named tag is zero ");

                foreach (DictionaryEntry namedTag in namedTagsFromCache)
                {
                    if(namedTag.Key.ToString() != "FlashDiscount"  || namedTag.Value.ToString() != "NoFlashDiscount")
                        throw new Exception("named tag is not updated ");
                }

               
                _report.AddPassedTestCase(methodName, "complex query passed");


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }

        #region ----------------------------------------------- Helper Methds ----------------------------------------------- 


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

            var count = cache.Count;    
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
            cache.Clear();
            PopulateProductList();

            Tag[] tags = new Tag[2];
            tags[0] = new Tag("East Coast Product");
            tags[1] = new Tag("Important Product");

            NamedTagsDictionary productNamedTag = PrepareNamedTags();

            foreach (var prod in productList)
            {
                // new JsonObject(JsonConvert.SerializeObject(item))
                var item = new CacheItem(prod);
                // item.NamedTags = new Alachisoft.NCache.Runtime.Caching.NamedTagsDictionary();
                item.NamedTags = productNamedTag;
                item.Tags = tags;
                item.Group = _groupName;
                item.Priority = CacheItemPriority.Normal;

                cache.Add("product" + prod.Id, item);
            }
        }

        private static NamedTagsDictionary PrepareNamedTags()
        {
            var productNamedTag = new NamedTagsDictionary();
            productNamedTag.Add(GetNamedTags().Item1, GetNamedTags().Item2);
            return productNamedTag;
        }

        private static Tuple<string, decimal> GetNamedTags()
        {
            Tuple<string, decimal> namedTags =  Tuple.Create("discount", Convert.ToDecimal(0.5));
      
            return namedTags;
        }

        private void PopulateProductList()
        {
            productList.Clear();

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


        #endregion

    }

}
