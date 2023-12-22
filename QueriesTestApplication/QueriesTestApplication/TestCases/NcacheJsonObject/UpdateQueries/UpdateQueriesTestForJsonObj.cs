using Alachisoft.NCache.Client;
using Alachisoft.NCache.Common.Monitoring;
using Alachisoft.NCache.JNIBridge.Net;
using Alachisoft.NCache.MetricServer.DataModel;
using Alachisoft.NCache.Runtime.CacheManagement;
using Alachisoft.NCache.Runtime.Caching;
using Alachisoft.NCache.Runtime.JSON;
using Alachisoft.NCache.Sample.Data;
using Quartz.Util;
using QueriesTestApplication.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Reflection;
using ReportHelper = QueriesTestApplication.Utils.ReportHelper;

namespace QueriesTestApplication
{
    class UpdateQueriesTestForJsonObject
    {
        ICache cache;
        public Dictionary<string, ResultStatus> testResults;
        int count = 0;
        List<Product> productList;
        List<Product> complexProductList;
        int _totalItemToInsert = 100;

        Report _report;

        public UpdateQueriesTestForJsonObject()
        {
            cache = CacheManager.GetCache(Common.CacheName);
            testResults = new Dictionary<string, ResultStatus>();
            productList = new List<Product>();
            complexProductList = new List<Product>();

            _report = new Report(nameof(UpdateQueriesTestForJsonObject));

            PopulateProductList();
        }

        public Report Report { get => _report; }

        public Dictionary<string, ResultStatus> TestResults
        {
            get { return testResults; }
        }


        #region --------------------------------- Add Operation  ---------------------------------

        public void AddJsonValueAsAttribute()
        {
            cache.Clear();
            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "Add new attribute in json string";
            Exception exception = new Exception(description);
            try
            {


                PopulateCache();

                string jsonStringKey = "ProductManufacturerName";
                string jsonStringValue = "Alachisoft";

                JsonValue jsonKey = (JsonValue)jsonStringKey;
                JsonValue jsonValue = (JsonValue)jsonStringValue;

                object obj = jsonValue.ToString();

                string arrayName = "Name123";
                string query = $"Update Alachisoft.NCache.Sample.Data.Product Add {jsonStringKey} = @jsonValue where  Category = @beverages";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@jsonValue", jsonValue);

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand, out var dictionary);

                Helper.ThrowExceptionIfNoUpdates(updated);
                Helper.ValidateDictionary(dictionary);

                foreach (var item in productList)
                {
                    if (item.Category == "Beverages")
                    {
                        var jsonObjFromCache = cache.Get<JsonObject>("product" + item.Id);

                        if (jsonObjFromCache.ContainsAttribute(jsonStringKey))
                        {
                            var attributeValueFromCache = jsonObjFromCache.GetAttributeValue(jsonStringKey).Value.ToString();

                            if (!attributeValueFromCache.Equals(jsonStringValue))
                                throw exception;

                        }
                        else
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

        public void AddNewJsonArray()
        {
            cache.Clear();
            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "Add new jsonArray to product";
            Exception exception = new Exception(description);
            try
            {
                PopulateCache();

                string jsonString = "[{\"name\":\"phone\",\"model\":\"p30PRO\"}]";
                JsonArray jsonArray = new JsonArray(jsonString);

                string arrayName = "Name123";
                string query = $"Update Alachisoft.NCache.Sample.Data.Product Add this.{arrayName}= @jsonArray where  Category = @beverages";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@jsonArray", jsonArray);
                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                foreach (var item in productList)
                {
                    if (item.Category == "Beverages")
                    {
                        var jsonObjFromCache = cache.Get<JsonObject>("product" + item.Id);

                        if (jsonObjFromCache.ContainsAttribute(arrayName))
                        {
                            var arrayFromCache = jsonObjFromCache.GetAttributeValue(arrayName) as JsonArray;
                            if (arrayFromCache.Count != jsonArray.Count)
                                throw exception;
                        }
                        else
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

        public void AddJsonObject()
        {
            cache.Clear();
            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = " Add json object by partial update";

            var keys = PopulateCacheAndGetKeys();

            try
            {

                var order = new Order { OrderID = 10, ShipCity = "rawalpindi.Islamabad", ShipCountry = "Pakistan.ASia" };
                JsonObject jsonObject = Helper.GetJsonOrder(order);

                JsonValueBase.Parse(jsonObject.ToString());


                string query = "Update Alachisoft.NCache.Sample.Data.Product Add this.Order = @jOrder ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@jOrder", jsonObject);

                var write = new WriteThruOptions(WriteMode.WriteThru);

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand, out var dictionary, write);

                Helper.ValidateDictionary(dictionary);

                foreach (var key in keys)
                {
                    _ = cache.Get<JsonObject>(key);
                    var prod = cache.Get<Product>(key);

                    if (!prod.Order.Equals(order))
                        throw new Exception(description);
                }

                _report.AddPassedTestCase(methodName, description);


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }

        }

        public void AddJsonObjectWithSlashes()
        {
            cache.Clear();
            string methodName = MethodBase.GetCurrentMethod().Name;

            var keys = PopulateCacheAndGetKeys();

            try
            {

                var order = new Order { OrderID = 10, ShipCity = "rawalpindi\\\\Islamabad", ShipCountry = "Pakistan\\\\ASia" };
                JsonObject jsonObject = Helper.GetJsonOrder(order);

                JsonValueBase.Parse(jsonObject.ToString());


                string query = "Update Alachisoft.NCache.Sample.Data.Product Add this.Order = @jOrder ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@jOrder", jsonObject);

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand, out var dictionary);

                Helper.ValidateDictionary(dictionary);

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

        public void AddJsonObjectAtSpecificIndex()
        {
            cache.Clear();
            string methodName = MethodBase.GetCurrentMethod().Name;

            var keys = PopulateCacheAndGetKeys();
            string description = "Add array in object";

            try
            {

                const int arraySize = 2;

                var order = new Order { OrderID = 10, ShipCity = "rawalpindi-Islamabad", ShipCountry = "Pakistan-ASia" };
                var orderArray = new Order[arraySize] { order, order };


                JsonObject jsonObj = new JsonObject(Newtonsoft.Json.JsonConvert.SerializeObject(order));

                string query = "Update Alachisoft.NCache.Sample.Data.Product Add this.Images[0] = @jsonObj ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@jsonObj", jsonObj);

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand, out var dictionary);

                Helper.ValidateDictionary(dictionary);



                var exception = new Exception(description);

                foreach (string key in keys)
                {
                    var jObject = cache.Get<Newtonsoft.Json.Linq.JObject>(key);
                    var jsonObject = cache.Get<JsonObject>(key);

                    var imagesArray = ((JsonObject)jsonObject.GetAttributeValue("Images")).GetAttributeValue("$values") as JsonArray;

                    // verify 1st index is still image. It should not be order 
                    var imageObj = Newtonsoft.Json.JsonConvert.DeserializeObject<Image>(imagesArray[1].ToString());

                    var updatedOrder = Newtonsoft.Json.JsonConvert.DeserializeObject<Order>(imagesArray[0].ToString());

                    if (updatedOrder.ShipCountry != order.ShipCountry)
                        throw exception;

                }

                Helper.ValidateDictionary(dictionary);
                _report.AddPassedTestCase(methodName, description);


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }

        }

        public void AddJsonObjectAtSpecificIndexWithSlash()
        {
            cache.Clear();
            string methodName = MethodBase.GetCurrentMethod().Name;

            var keys = PopulateCacheAndGetKeys();
            string description = "Add array in object with backslash";

            try
            {

                const int arraySize = 2;

                var order = new Order { OrderID = 10, ShipCity = "rawalpindi\\\\Islamabad", ShipCountry = "Pakistan\\\\]ASia" };
                var orderArray = new Order[arraySize] { order, order };


                JsonObject jsonObj = new JsonObject(Newtonsoft.Json.JsonConvert.SerializeObject(order));

                string query = "Update Alachisoft.NCache.Sample.Data.Product Add this.Images[0] = @jsonObj ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@jsonObj", jsonObj);

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand, out var dictionary);

                Helper.ValidateDictionary(dictionary);



                var exception = new Exception(description);

                foreach (string key in keys)
                {
                    var jObject = cache.Get<Newtonsoft.Json.Linq.JObject>(key);
                    var jsonObject = cache.Get<JsonObject>(key);

                    var imagesArray = ((JsonObject)jsonObject.GetAttributeValue("Images")).GetAttributeValue("$values") as JsonArray;

                    // verify 1st index is still image. It should not be order 
                    var imageObj = Newtonsoft.Json.JsonConvert.DeserializeObject<Image>(imagesArray[1].ToString());

                    var updatedOrder = Newtonsoft.Json.JsonConvert.DeserializeObject<Order>(imagesArray[0].ToString());

                    if (updatedOrder.ShipCountry != order.ShipCountry)
                        throw exception;

                }

                Helper.ValidateDictionary(dictionary);
                _report.AddPassedTestCase(methodName, description);


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }

        }

        public void AddJsonArrayInObject()
        {
            cache.Clear();
            string methodName = MethodBase.GetCurrentMethod().Name;

            var keys = PopulateCacheAndGetKeys();

            try
            {

                const int arraySize = 2;

                var order = new Order { OrderID = 10, ShipCity = "rawalpindiIslamabad", ShipCountry = "PakistanASia" };
                var orderArray = new Order[arraySize] { order, order };
                JsonArray jsonArray = new JsonArray();

                for (int i = 0; i < arraySize; i++)
                {
                    jsonArray.Add(Helper.GetJsonOrder(order));

                }

                string query = "Update Alachisoft.NCache.Sample.Data.Product Add this.Order = @jArray ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@jArray", jsonArray);
                //todo why this directly throws exception
                var updated = cache.SearchService.ExecuteNonQuery(queryCommand, out var dictionary);

                Helper.ValidateDictionary(dictionary);

                foreach (var key in keys)
                {
                    var jsonObj = cache.Get<JsonObject>(key);
                    var array = jsonObj["Order"] as JsonArray;

                    if (array.Count != arraySize)
                        throw new Exception("Unable to add array");
                    if (((JsonObject)array[0]).GetAttributeValue("ShipCity").Value.ToString() != order.ShipCity)
                    {
                        throw new Exception("Unable to add array");

                    }
                    // var prod = cache.Get<Product>(key);


                }

                _report.AddPassedTestCase(methodName, "Add order in product");


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }

        }

        public void AddJsonArrayInObjectWithSlashes()
        {
            cache.Clear();
            string methodName = MethodBase.GetCurrentMethod().Name;

            var keys = PopulateCacheAndGetKeys();

            try
            {

                const int arraySize = 2;

                var order = new Order { OrderID = 10, ShipCity = "rawalpindi\\\\Islamabad", ShipCountry = "Pakistan\\\\ASia" };
                var orderArray = new Order[arraySize] { order, order };
                JsonArray jsonArray = new JsonArray();

                for (int i = 0; i < arraySize; i++)
                {
                    jsonArray.Add(Helper.GetJsonOrder(order));

                }

                string query = "Update Alachisoft.NCache.Sample.Data.Product Add this.Order = @jArray ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@jArray", jsonArray);
                //todo why this directly throws exception
                var updated = cache.SearchService.ExecuteNonQuery(queryCommand, out var dictionary);

                Helper.ValidateDictionary(dictionary);

                foreach (var key in keys)
                {
                    var jsonObj = cache.Get<JsonObject>(key);
                    var array = jsonObj["Order"] as JsonArray;

                    if (array.Count != arraySize)
                        throw new Exception("Unable to add array");
                    if (((JsonObject)array[0]).GetAttributeValue("ShipCity").Value.ToString() != order.ShipCity)
                    {
                        throw new Exception("Unable to add array");

                    }
                    // var prod = cache.Get<Product>(key);


                }

                _report.AddPassedTestCase(methodName, "Add order in product");


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }

        }

        public void AddWithWrongQuery()
        {
            cache.Clear();
            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "Add to throw exception because of wrong syntax";

            try
            {
                PopulateCache();

                string query = "Update Alachisoft.NCache.Sample.Data.Product Add this.Order.\" = \" \" WrongValue \" ";
                QueryCommand queryCommand = new QueryCommand(query);

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

        public void AddThrowException()
        {
            cache.Clear();
            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "Add should throw exception if used as set";

            try
            {
                PopulateCache();

                var order = new Order { OrderID = 10, ShipCity = "rawalpindi", ShipCountry = "Pakistan" };
                var jsonorder = new JsonObject();
                jsonorder.AddAttribute("OrderID", order.OrderID);
                jsonorder.AddAttribute("ShipCity", order.ShipCity);
                jsonorder.AddAttribute("ShipCountry", order.ShipCountry);

                string query = "Update Alachisoft.NCache.Sample.Data.Product Add this.Order = this.Order.OrderID ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@jOrder", jsonorder);

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

        public void AddObjectAtIndexThatDoesnotExist()
        {
            cache.Clear();
            string methodName = MethodBase.GetCurrentMethod().Name;

            var keys = PopulateCacheAndGetKeys();
            string description = "Add element at specific index of array that and the array doesnot exist";

            try
            {


                const int arraySize = 2;

                var order = new Order { OrderID = 10, ShipCity = "rawalpindiIslamabad", ShipCountry = "PakistanASia" };

                JsonObject jsonObject = Helper.GetJsonOrder(order);


                string query = "Update Alachisoft.NCache.Sample.Data.Product Add this.Order1[0] = @jOrder ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@jOrder", jsonObject);

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand, out var dictionary);

                Helper.ValidateDictionary(dictionary);

                


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
                var order = new Order { OrderID = 10, ShipCity = "rawalpindiIslamabad", ShipCountry = "PakistanASia" };

                JsonObject jsonArray = Helper.GetJsonOrder(order);


                string query = "Update Alachisoft.NCache.Sample.Data.Product Add this.Images[10] = @jArray ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@jArray", jsonArray);

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand, out var dictionary);

                Helper.ValidateDictionary(dictionary);


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

        // I Gave him Object to add and it added array having values of my given object 
        public void AddOperationUsingJsonObject()
        {
            int updated = 0;
            cache.Clear();
            PopulateCache();
            string methodName = MethodBase.GetCurrentMethod().Name;

            count++;
            try
            {
                string JsonString = "{\"CompanyName\":\"EverGiven\",\"CompanyId\":3760}";
                //JObject ShippingComapny = JObject.Parse(JsonString);
                JsonObject ShippingComapny = new JsonObject(JsonString);

                string query = "Update Alachisoft.NCache.Sample.Data.Product Add ShippingComapny = @ShippingComapny Where Id=?";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@ShippingComapny", ShippingComapny);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary dictionary;


                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);


                var result = cache.Get<JsonObject>("product1");
                var ShippingComapnyFromCache = (JsonObject)result.GetAttributeValue("ShippingComapny");
                var name = ShippingComapnyFromCache.GetAttributeValue("CompanyName");

                if (name.Value.ToString() == "EverGiven")
                    _report.AddPassedTestCase(methodName, "Success: Partial Update, Add Operation using JSON Object");

                else
                    throw new Exception("Failure: Partial Update, Add Operation using JSON Object ");


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }

        }


        #endregion


        #region --------------------------------- Set Operation    -------------------------------

        public void SetJsonValue()
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

                JsonValue jsonValue = (JsonValue)ImageFileName;

                int indexToUpdate = 1;

                string query = $"Update Alachisoft.NCache.Sample.Data.Product Set Images[{indexToUpdate}].FileName = @FileName ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@FileName", jsonValue);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary dictionary;


                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);

                Helper.ValidateDictionary(dictionary);

                var result = cache.Get<Product>("product1");

                if (result.Images[indexToUpdate].FileName.Contains(ImageFileName))
                    _report.AddPassedTestCase(methodName, "Set Operation to Update Attribute Inside Array");
                else
                    throw new Exception("Set Operation to Update Attribute Inside Array");


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }

        public void SetJsonbject()
        {
            int updated = 0;
            cache.Clear();
            PopulateCache();
            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "Set Operation using JsonObject";

            count++;

            try
            {
                string shipCity = "Texas";
                var order = new Order { OrderID = 11, ShipCity = shipCity, ShipCountry = "US" };
                var jsonOrder = Helper.GetJsonOrder(order);

                string query = "Update Alachisoft.NCache.Sample.Data.Product Set Order = @MyOrder Where Id=?";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@MyOrder", jsonOrder);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary dictionary;

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);

                var testProperDeserialization = cache.Get<Product>("product1");
                var result = cache.Get<JsonObject>("product1");
                var Order = (JsonObject)result.GetAttributeValue("Order");
                var OrderCountry = Order.GetAttributeValue("ShipCity");

                if (OrderCountry.Value.ToString().Equals(shipCity))
                    _report.AddPassedTestCase(methodName, description);

                else
                    throw new Exception(description);


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }

        public void SetJsonArrayAtZerothIndex()
        {
            cache.Clear();
            string methodName = MethodBase.GetCurrentMethod().Name;

            var keys = PopulateCacheAndGetKeys();
            string description = "Set json array at 0 index of array";

            try
            {

                const int arraySize = 2;

                var order = new Order { OrderID = 10, ShipCity = "rawalpindi\\\\Islamabad", ShipCountry = "Pakistan\\\\ASia" };
                var orderArray = new Order[arraySize] { order, order };

                var jsonArray = new JsonArray(Newtonsoft.Json.JsonConvert.SerializeObject(orderArray));


                string query = "Update Alachisoft.NCache.Sample.Data.Product Set this.Images[0] = @jsonArray ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@jsonArray", jsonArray);

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand, out var dictionary);

                Helper.ValidateDictionary(dictionary);

                var exception = new Exception(description);

                foreach (string key in keys)
                {
                    var jsonObject = cache.Get<Newtonsoft.Json.Linq.JObject>(key);
                    //JObject jsonObject = JObject.Parse(result.ToString());

                    var imagesArray = (Newtonsoft.Json.Linq.JArray)jsonObject["Images"]["$values"];

                    // verify 1st index is still image. It should not be order 
                    var imageObj = Newtonsoft.Json.JsonConvert.DeserializeObject<Image>(imagesArray[1].ToString());

                    var updatedOrderArray = Newtonsoft.Json.JsonConvert.DeserializeObject<Order[]>(imagesArray[0].ToString());

                    if (updatedOrderArray.Length != arraySize)
                        throw exception;

                    foreach (Order updatedOrderObj in updatedOrderArray)
                    {
                        if (!updatedOrderObj.Equals(order))
                            throw exception;
                    }


                }

                Helper.ValidateDictionary(dictionary);


                _report.AddPassedTestCase(methodName, description);

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }

        }

        public void SetJsonArrayAtLastIndex()
        {
            cache.Clear();
            string methodName = MethodBase.GetCurrentMethod().Name;

            var keys = PopulateCacheAndGetKeys();
            string description = "Set array at last index of array";

            try
            {

                const int arraySize = 2;

                var order = new Order { OrderID = 10, ShipCity = "rawalpindi\\\\Islamabad", ShipCountry = "Pakistan\\\\ASia" };
                var orderArray = new Order[arraySize] { order, order };

                var jsonArray = new JsonArray(Newtonsoft.Json.JsonConvert.SerializeObject(orderArray));


                string query = "Update Alachisoft.NCache.Sample.Data.Product Set this.Images[2] = @jsonArray ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@jsonArray", jsonArray);

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand, out var dictionary);

                Helper.ValidateDictionary(dictionary);

                var exception = new Exception(description);

                foreach (string key in keys)
                {
                    var jsonObject = cache.Get<Newtonsoft.Json.Linq.JObject>(key);
                    //JObject jsonObject = JObject.Parse(result.ToString());

                    var imagesArray = (Newtonsoft.Json.Linq.JArray)jsonObject["Images"]["$values"];

                    // verify 1st index is still image. It should not be order 
                    var imageObj = Newtonsoft.Json.JsonConvert.DeserializeObject<Image>(imagesArray[1].ToString());

                    var updatedOrder = Newtonsoft.Json.JsonConvert.DeserializeObject<Order[]>(imagesArray[2].ToString());

                    if (updatedOrder.Length != arraySize)
                        throw exception;

                    foreach (var item in updatedOrder)
                    {
                        if (!item.Equals(order))
                            throw exception;
                    }


                }

                Helper.ValidateDictionary(dictionary);


                _report.AddPassedTestCase(methodName, description);

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }

        }

        public void SetOperationToUpdateAttributeOfArray()
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

                string serializedImage = Newtonsoft.Json.JsonConvert.SerializeObject(image);
                JsonObject jsonObject = new JsonObject(serializedImage);

                int indexToUpdate = 1;

                string query = $"Update Alachisoft.NCache.Sample.Data.Product Set Images[{indexToUpdate}].FileName = @FileName ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@FileName", ImageFileName);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary dictionary;


                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);

                Helper.ValidateDictionary(dictionary);

                var result = cache.Get<Product>("product1");

                if (result.Images[indexToUpdate].FileName.Equals(ImageFileName))
                    _report.AddPassedTestCase(methodName, "Set Operation to Update Attribute Inside Array");
                else
                    throw new Exception("Set Operation to Update Attribute Inside Array");


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }

        public void SetOperationToUpdateArrayWithinArray()
        {
            int updated = 0;
            cache.Clear();
            IList<string> keys = PopulateCacheAndGetKeys();
            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;

            try
            {

                string ImageFormat = "UpdatedImageFormat";


                int updateIndex = 0;
                string query = $"Update Alachisoft.NCache.Sample.Data.Product Set Images[{updateIndex}].ImageFormats[{updateIndex}].Format = @ImageFormat ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@ImageFormat", ImageFormat);

                IDictionary dictionary;

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);

                Helper.ValidateDictionary(dictionary);

                foreach (string key in keys)
                {
                    var result = cache.Get<Product>(key);

                    if (!result.Images[updateIndex].ImageFormats[updateIndex].Format.Equals(ImageFormat))
                        throw new Exception("Set operation to attribute of array with in array");

                }

                _report.AddPassedTestCase(methodName, "Set operation to attribute of array with in array");


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }

        public void SetOperationWithWrongArrayIndex()
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


                string query = $"Update Alachisoft.NCache.Sample.Data.Product Set Images[-1].ImageFormats[0].Format = @ImageFormat ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@ImageFormat", ImageFormat);

                IDictionary dictionary;

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);

                Helper.ValidateDictionary(dictionary);

                // Contrtol should not reach here

                _report.AddFailedTestCase(methodName, new Exception("Expected Incorrect query Exception, but didn't get it."));
                // _report.AddFailedTestCase(methodName, new Exception("Expected a Target Not Found Exception, but didn't get it."));


            }
            catch (Exception ex)
            {
                if (Helper.IsIncorrectFormatException(ex))
                    _report.AddPassedTestCase(methodName, $"Incorrect quert -> Got Exception: {ex.Message.Split("\n")[0]}");
                else
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

                const int arraySize = 2;

                var order = new Order { OrderID = 10, ShipCity = "rawalpindi\\\\Islamabad", ShipCountry = "Pakistan\\\\ASia" };
                var orderArray = new Order[arraySize] { order, order };

                var jsonArray = new JsonArray(Newtonsoft.Json.JsonConvert.SerializeObject(orderArray));


                string query = "Update Alachisoft.NCache.Sample.Data.Product Set this.Images[10] = @jsonArray  ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@jsonArray", jsonArray);

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand, out var dictionary);

                Helper.ValidateDictionary(dictionary);


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

        public void SetOperationWithAesterick()
        {
            cache.Clear();
            string methodName = MethodBase.GetCurrentMethod().Name;

            var keys = PopulateCacheAndGetKeys();
            string description = "Set operaton containing aesterick symbol";

            try
            {

                const int arraySize = 2;

                var order = new Order { OrderID = 10, ShipCity = "rawalpindi\\\\Islamabad", ShipCountry = "Pakistan\\\\ASia" };
                var orderArray = new Order[arraySize] { order, order };

                var jsonArray = new JsonArray(Newtonsoft.Json.JsonConvert.SerializeObject(orderArray));


                string query = "Update Alachisoft.NCache.Sample.Data.Product Set this.Images[*] = @jsonArray  ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@jsonArray", jsonArray);

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand, out var dictionary);

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

        public void VerifyNpExceptionIsThrownIfKeyNotExistForUpdate()
        {
            int updated = 0;
            cache.Clear();
            PopulateCache();
            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;
            string description = "No exception should be thrown in update query if key not exist";
            try
            {

                string query = "Update  Alachisoft.NCache.Sample.Data.Product Set Order.ShipCity = '\"abcd\"' test Category = '\"meat\"' where Name = ?";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Name", Guid.NewGuid().ToString());
                IDictionary dictionary;

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);

                Helper.ValidateDictionary(dictionary);

                if (updated > 0)
                    throw new Exception(description);

                _report.AddPassedTestCase(methodName, description);
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }
        }


        //Adds array as added by AddOperationUsingJsonObject Method
        public void SetOperationUsingJsonObject()
        {
            int updated = 0;
            cache.Clear();
            PopulateCache();
            string methodName = MethodBase.GetCurrentMethod().Name;

            count++;

            try
            {
                //var order = new Order { OrderID = 11, ShipCity = "Texas", ShipCountry = "US" };
                //var jorder = JObject.Parse(JsonConvert.SerializeObject(order));

                JsonObject order = new JsonObject();
                order.AddAttribute("OrderID", 11);
                order.AddAttribute("ShipCity", "Texas");
                order.AddAttribute("ShipCountry", "US");

                string query = "Update Alachisoft.NCache.Sample.Data.Product Set Order = @MyOrder Where Id=?";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@MyOrder", order);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary dictionary;


                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);


                var result = cache.Get<JsonObject>("product1");
                var Order = (JsonObject)result.GetAttributeValue("Order");
                var OrderCountry = Order.GetAttributeValue("ShipCity");

                if (OrderCountry.Value.ToString() == "Texas")
                {
                    _report.AddPassedTestCase(methodName, "Success: Partial Update, Set Operation using JSON bject");

                }
                else
                {
                    throw new Exception("Failure:Partial Update, Set Operation using JSON bject ");
                }

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }

     

        #endregion


        #region --------------------------------- Copy Operation  --------------------------------


        /// <summary>
        /// Inserts 5000 Items in cache.
        /// Then copies value of Order.ShipCity to Order.ShipCountry for all 5000 objects, using Move.
        /// </summary>
        private void CopyQuery1()
        {
            int updated = 0;
            cache.Clear();
            // PopulateCache();
            string ProductKey = "product1";
            string methodName = MethodBase.GetCurrentMethod().Name;

            count++;



            for (int i = 0; i < _totalItemToInsert; i++)
            {
                cache.Insert(i.ToString(), new Product() { Id = i, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 35, Order = new Order { OrderID = 10, ShipCity = "rawalpindi", ShipCountry = "Pakistan" } });
            }


            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Copy Order.ShipCountry = Order.ShipCity ";

                QueryCommand queryCommand = new QueryCommand(query);
                // queryCommand.Parameters.Add("Id", Convert.ToInt32(1));
                IDictionary dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);
                int TotalFailedOperations = 0;

                Helper.ValidateDictionary(dictionary);

                foreach (DictionaryEntry val in dictionary)
                {
                    TotalFailedOperations++;
                }

                TotalFailedOperations = 0;
                int TotalSuccessedOperations = 0;

                for (int i = 0; i < _totalItemToInsert; i++)
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

            for (int i = 0; i < _totalItemToInsert; i++)
            {
                cache.Insert(i.ToString(), new Product() { Id = i, Time = DateTime.Now, Name = null, ClassName = "Electronics", Category = "Beverages", UnitPrice = 35, Order = new Order { OrderID = 10, ShipCity = "rawalpindi", ShipCountry = "Pakistan" } });
            }


            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Copy Order.ShipCountry = Name";

                QueryCommand queryCommand = new QueryCommand(query);
                // queryCommand.Parameters.Add("Id", Convert.ToInt32(1));
                IDictionary dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);
                int TotalFailedOperations = 0;
                Helper.ValidateDictionary(dictionary);


                int TotalSuccessedOperations = 0;

                for (int i = 0; i < _totalItemToInsert; i++)
                {

                    var result = cache.Get<JsonObject>(i.ToString());
                    var OrderInCache = (JsonObject)result.GetAttributeValue("Order");

                    var Country = OrderInCache.GetAttributeValue("ShipCountry");

                    if (Country.Value != null)
                        throw new Exception("Null value of attribute is not copied");
                }

                _report.AddPassedTestCase(methodName, "Seccess:Partial Update items using CopyQuery1 ");

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
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

            var product = new Product() { Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 35, Order = new Order { OrderID = 10, ShipCity = "rawalpindi", ShipCountry = "Pakistan", ShipName = "EverGreen" } };

            for (int i = 0; i < _totalItemToInsert; i++)
            {
                product.Id = i;
                cache.Insert(i.ToString(), product);
            }


            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Copy Order.ShipCountry = Order.ShipCity ,Order.ShipName=Name";

                QueryCommand queryCommand = new QueryCommand(query);
                // queryCommand.Parameters.Add("Id", Convert.ToInt32(1));
                IDictionary dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);
                int TotalFailedOperations = 0;

                Helper.ValidateDictionary(dictionary);

                foreach (DictionaryEntry val in dictionary)
                {
                    TotalFailedOperations++;
                }

                TotalFailedOperations = 0;
                int TotalSuccessedOperations = 0;

                for (int i = 0; i < _totalItemToInsert; i++)
                {

                    var result = cache.Get<JsonObject>(i.ToString());
                    var OrderInCache = (JsonObject)result.GetAttributeValue("Order");

                    var shipCity = OrderInCache.GetAttributeValue("ShipCity");
                    var Country = OrderInCache.GetAttributeValue("ShipCountry");

                    if (shipCity.Value.ToString() == Country.Value.ToString())
                    {
                        var orderShipName = OrderInCache.GetAttributeValue("ShipName").Value.ToString();
                        var name = result.GetAttributeValue("Name").Value.ToString();

                        if (name != orderShipName || orderShipName.Contains(product.Order.ShipName))
                            throw new Exception("Copy attribute didnot copied successfully");
                        TotalSuccessedOperations++;
                        continue;
                    }
                    else
                    {
                        throw new Exception("Failure:Partial Update items using Copy2Attributes ");
                    }
                }

                _report.AddPassedTestCase(methodName, "Seccess:Partial Update items using Copy2Attributes ");

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
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

                IDictionary dictionary;

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);
                Helper.ValidateDictionary(dictionary);

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


                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out IDictionary dictionary);

                var exception = new Exception("Failure:Partial Update item to copy array indexes");

                if (updated == 0)
                    throw new Exception("No Item is updated");

                Helper.ValidateDictionary(dictionary);


                _ = cache.Get<JsonObject>(ProductKey);

                var res = cache.Get<Product>(ProductKey);

                // JArray.Insert is used in server side . this insert adds the new element at top and moves other one step ahead
                // So the element of array that is to be copied is places at start of array
                if (res.Images[0].FileName == res.Images[2].FileName)
                    _report.AddPassedTestCase(methodName, "Success: Partial Update to copy array from 1st index to 0th ndex");
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
            var keys = PopulateCacheAndGetKeys();
            string ProductKey = "product1";
            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "Copy object to index of array";

            count++;
            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Copy this.Images[0] = this.Order Where Id=?";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));


                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out IDictionary dictionary);

                var exception = new Exception("Failure:Partial Update item to copy array indexes");
                Helper.ValidateDictionary(dictionary);

                foreach (var key in keys)
                {
                    var product = cache.Get<JsonObject>(key);

                    var images = (JsonArray)((JsonObject)product.GetAttributeValue("Images")).GetAttributeValue("$values");

                    if (product.GetAttributeValue("Id").Value.ToString() == "1")
                    {
                        var order = ((JsonObject)images[0]).GetAttributeValue("OrderID").Value.ToString();
                        var orderCompare = ((JsonObject)product.GetAttributeValue("Order")).GetAttributeValue("OrderID").Value.ToString();
                        if (order == orderCompare)
                        {
                            _report.AddPassedTestCase(methodName, description);
                            break;
                        }
                        else
                        {
                            throw exception;
                        }
                    }
                }

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

                IDictionary dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);

                Helper.ValidateDictionary(dictionary);
                Helper.ThrowExceptionIfNoUpdates(updated);

                var jsonProduct = cache.Get<JsonObject>(ProductKey);
                string id = jsonProduct.GetAttributeValue("Id").Value.ToString();
                string order = jsonProduct.GetAttributeValue("Order").Value.ToString();

                if (id == order)
                    _report.AddPassedTestCase(methodName, description);
                else
                    throw new Exception(description);


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }


        }

        public void CopyOnAttributeThatDoesnotExist()
        {
            int updated;
            cache.Clear();
            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "Copy destination attribute to non existing source attribute";

            count++;

            for (int i = 0; i < _totalItemToInsert; i++)
            {
                cache.Insert(i.ToString(), new Product() { Id = i, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 35, Order = new Order { OrderID = 10, ShipCity = "rawalpindi", ShipCountry = "Pakistan" } });
            }


            try
            {
                string AttributeThatDoesnotExist;

                string query = $"Update  Alachisoft.NCache.Sample.Data.Product  Copy Order.{nameof(AttributeThatDoesnotExist)} = Order.ShipCity ";

                QueryCommand queryCommand = new QueryCommand(query);
                // queryCommand.Parameters.Add("Id", Convert.ToInt32(1));
                IDictionary dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);
                Helper.ValidateDictionary(dictionary);

                for (int i = 0; i < _totalItemToInsert; i++)
                {
                    var product = cache.Get<JsonObject>(i.ToString());
                    var order = (JsonObject)product.GetAttributeValue("Order");

                    if (!order.ContainsAttribute("AttributeThatDoesnotExist"))
                        throw new Exception("the non exististing source attribute is not created by update query");

                    string shipCity = order.GetAttributeValue("ShipCity").Value.ToString();
                    string value = order.GetAttributeValue(nameof(AttributeThatDoesnotExist)).Value.ToString();

                    if (shipCity != value)
                        throw new Exception("the  destination attribute is not successfully copied to non existing source attribute");
                }

                _report.AddPassedTestCase(methodName, description);

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }

        public void CopyAnAttributeThatDoesnotExist()
        {
            int updated = 0;
            cache.Clear();
            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "Copy destination attribute that doesnot exist";


            count++;

            for (int i = 0; i < _totalItemToInsert; i++)
            {
                cache.Insert(i.ToString(), new Product() { Id = i, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 35, Order = new Order { OrderID = 10, ShipCity = "rawalpindi", ShipCountry = "Pakistan" } });
            }


            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Copy Order.ShipCountry = Order.AttributeThatDoesnotExist ";

                QueryCommand queryCommand = new QueryCommand(query);
                // queryCommand.Parameters.Add("Id", Convert.ToInt32(1));
                IDictionary dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);
                int TotalFailedOperations = 0;
                Helper.ValidateDictionary(dictionary);
                throw new Exception(description);
            }
            catch (Exception ex)
            {
                if (Helper.IsTargetNotFoundException(ex))
                {
                    _report.AddPassedTestCase(methodName, description);
                }
                else
                {
                    _report.AddFailedTestCase(methodName, ex);

                }
            }


        }
        #endregion


        #region  --------------------------------- Move Operation    ------------------------------

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

                IDictionary dictionary;

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);
                Helper.ValidateDictionary(dictionary);

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
            IList<string> keys = PopulateCacheAndGetKeys();
            string ProductKey = "product1";
            string methodName = MethodBase.GetCurrentMethod().Name;

            count++;
            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Move Order.ShipCountry = Order.ShipCity ,Order.OrderID=Category ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));


                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out IDictionary dictionary);
                Helper.ValidateDictionary(dictionary);


                foreach (string key in keys)
                {
                    var product = cache.Get<JsonObject>(key);
                    int id = int.Parse(product.GetAttributeValue("Id").Value.ToString());

                    var expectedValues = productList.
                       Where(prod => prod.Id == id).
                       Select(prod => new { prod.Order.ShipCity, prod.Category }).
                       FirstOrDefault();
                    

                    JsonObject order = (JsonObject)product.GetAttributeValue("Order");

                    if (order.ContainsAttribute("ShipCity") || product.ContainsAttribute("Category"))
                        throw new Exception("Move attribute didnot removed the source attribute");

                    string ShipCountry = order.GetAttributeValue("ShipCountry").Value.ToString();
                    string orderId = order.GetAttributeValue("OrderID").Value.ToString();

                    if (ShipCountry == expectedValues.ShipCity && orderId == expectedValues.Category.ToString())
                        continue;
                    else
                        throw new Exception("Failure:Partial Update to moves two attributes in one query");
                }

                _report.AddPassedTestCase(methodName, "Success: Partial Update to moves two attributes in one query");

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

            string methodName = MethodBase.GetCurrentMethod().Name;

            for (int i = 0; i < _totalItemToInsert; i++)
            {
                cache.Insert(i.ToString(), new Product() { Id = i, Time = DateTime.Now, Name = null, ClassName = "Electronics", Category = "Beverages", UnitPrice = 35, Order = new Order { OrderID = 10, ShipCity = "rawalpindi", ShipCountry = "Pakistan" } });
            }

            count++;
            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Move Order.ShipCountry = Name ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);
                Helper.ValidateDictionary(dictionary);

                for (int i = 0; i < _totalItemToInsert; i++)
                {
                    var product = cache.Get<JsonObject>(i.ToString());

                    if (product.ContainsAttribute("Name"))
                        throw new Exception("source attribute with null value is not removed by move query");

                    if (product.GetAttributeValue("Name") == null || product.GetAttributeValue("Name").Value == null)
                        continue;
                    else
                        throw new Exception("value of source attribute is not null");

                }


                _report.AddPassedTestCase(methodName, "copy attribute with null value");

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
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

                IDictionary dictionary;


                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);
                Helper.ValidateDictionary(dictionary);


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
            IList<string> keys = PopulateCacheAndGetKeys();
            string methodName = MethodBase.GetCurrentMethod().Name;

            count++;
            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Move Order.AttributeThatDoesnotEixst = Order.ShipCity ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);
                Helper.ValidateDictionary(dictionary);

                foreach (string key in keys)
                {
                    var product = cache.Get<JsonObject>(key);
                    var order = (JsonObject)product.GetAttributeValue("Order");

                    int id = int.Parse(product.GetAttributeValue("Id").Value.ToString());

                    if (product.ContainsAttribute("ShipCity"))
                        throw new Exception("source attribute is not removed by move query");

                    var expectedValue = productList
                        .Where(prod => prod.Id == id)
                        .Select(prod => new { prod.Order.ShipCity }) 
                        .FirstOrDefault();

                    string value = order.GetAttributeValue("AttributeThatDoesnotEixst").Value.ToString();

                    if (value != expectedValue.ShipCity)
                        throw new Exception("source attribute is not copied by move query");
                }

                _report.AddPassedTestCase(methodName, "move query to copy on attribute that does not exist");
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }

        public void MoveAnAttributeThatDoesnotEixst()
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

                IDictionary dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);
                Helper.ValidateDictionary(dictionary);

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


                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out IDictionary dictionary);

                var exception = new Exception("Failure:Partial Update item to copy array indexes");

                if (updated == 0)
                    throw exception;

                Helper.ValidateDictionary(dictionary);


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
            var keys = PopulateCacheAndGetKeys();
            string ProductKey = "product1";
            string methodName = MethodBase.GetCurrentMethod().Name;

            count++;
            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Move this.Images[0] = this.Order ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));


                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out IDictionary dictionary);

                var exception = new Exception("Failure:Partial Update item to move array indexes");
                Helper.ValidateDictionary(dictionary);

                foreach (string key in keys)
                {
                    var product = cache.Get<JsonObject>(key);

                    int id = int.Parse(product.GetAttributeValue("Id").Value.ToString());

                    if (product.ContainsAttribute("Order"))
                        throw new Exception("source attribute is not removed by move query");

                    var expectedValue = productList
                        .Where(prod => prod.Id == id)
                        .Select(prod => new { prod.Order.ShipCity })
                        .FirstOrDefault();
                        

                    var order = (JsonObject)((JsonArray)((JsonObject)product.GetAttributeValue("Images")).GetAttributeValue("$values"))[0];
                    string value = order.GetAttributeValue("ShipCity").Value.ToString();

                    if (value != expectedValue.ShipCity)
                        throw new Exception("source object is not copied  to array index  by move query");
                }

                _report.AddPassedTestCase(methodName, "move query to copy object to index of array");



            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }

        #endregion


        #region --------------------------------- Remove Operation -------------------------------

        public void RemoveAttributeFromJsonString()
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


                    JsonObject jObject = cache.Get<JsonObject>("product" + item.Id);
                    var order = (JsonObject)jObject.GetAttributeValue("Order");
                    if (order != null)

                    {

                        if (order.ContainsAttribute("OrderID"))
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


                    JsonObject jObject = cache.Get<JsonObject>("product" + item.Id);

                    if (jObject.GetAttributeValue("Order") != null)
                        throw new Exception(description);

                }

                _report.AddPassedTestCase(methodName, description);

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }

        }

        public void RemoveArrayFromJsonString()
        {

            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;
            try
            {

                cache.Clear();

                PopulateCache();
                string description = "Remove Array from json string ";


                string query = "Update  Alachisoft.NCache.Sample.Data.Product Remove this.Images ";
                QueryCommand queryCommand = new QueryCommand(query);
                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                foreach (var item in productList)
                {
                    var entry = cache.Get<Product>("product" + item.Id);

                    if (entry.Images != default)
                        throw new Exception(description);


                    JsonObject jObject = cache.Get<JsonObject>("product" + item.Id);
                    var images = (JsonObject)jObject.GetAttributeValue("Images");
                    if (images != null)
                        throw new Exception(description);

                }

                _report.AddPassedTestCase(methodName, description);

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }

        }

        public void RemoveSpecificArrayIndexFromJsonString()
        {

            string methodName = MethodBase.GetCurrentMethod().Name;
            count++;
            try
            {

                cache.Clear();

                PopulateCache();
                string description = "Remove value at specific index of array";


                string query = "Update  Alachisoft.NCache.Sample.Data.Product Remove this.Images[0] ";
                QueryCommand queryCommand = new QueryCommand(query);
                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                foreach (var item in productList)
                {
                    int arrayLengthBeforeQuery = item.Images.Length;
                    string fileName = item.Images[0].FileName;

                    var entry = cache.Get<Product>("product" + item.Id);

                    if (entry.Images.Length >= arrayLengthBeforeQuery)
                        throw new Exception(description);

                    if (entry.Images[0].FileName.Equals(fileName))
                        throw new Exception("Item is removed at wrong index");

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
                var updated = cache.SearchService.ExecuteNonQuery(queryCommand, out var failedOperation);

                Helper.ValidateDictionary(failedOperation);

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


        #region  --------------------------------- Test Operation --------------------------------- 
        private void TestAttributeValue()
        {
            int updated = 0;
            cache.Clear();
            PopulateCache();
            string methodName = MethodBase.GetCurrentMethod().Name;

            count++;
            try
            {
                string name = "Chai";

                JsonValue jsonValue = (JsonValue)name;

                string query = "Update  Alachisoft.NCache.Sample.Data.Product Test Name = @val, Order.OrderID=10 Where Id=?";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));
                queryCommand.Parameters.Add("@val", jsonValue);

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out var dictionary);

                Helper.ValidateDictionary(dictionary);

                _report.AddPassedTestCase(methodName, "Test json attribute value");


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }

        private void TestAttributeWithInlineValue()
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

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out var dictionary);

                Helper.ValidateDictionary(dictionary);

                _report.AddPassedTestCase(methodName, "Test json attribute value");


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }

        private void TestAttributeValueThatDoesnotExist()
        {
            int updated = 0;
            cache.Clear();
            PopulateCache();
            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "Test json attribute value that doesnot exist";

            count++;
            try
            {
                string name = "Chai has been drunk";

                JsonValue jsonValue = (JsonValue)name;

                string query = "Update  Alachisoft.NCache.Sample.Data.Product Test Name = @val, Order.OrderID=10 Where Id=?";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));
                queryCommand.Parameters.Add("@val", jsonValue);

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out var dictionary);

                Helper.ValidateDictionary(dictionary);
                _report.AddFailedTestCase(methodName, new Exception(description));

            }
            catch (Exception ex)
            {
                if (Helper.IsTestOperationException(ex))
                    _report.AddPassedTestCase(methodName, description);
                else
                    _report.AddFailedTestCase(methodName, ex);
            }


        }

        public void TestJsonObject()
        {
            int updated = 0;
            cache.Clear();

            string methodName = MethodBase.GetCurrentMethod().Name;

            count++;
            try
            {
                var date = DateTime.Now;
                var order = new Order { OrderID = 10, ShipCity = "rawalpindi", ShipCountry = "Pakistan" , ShipName = null, ShipAddress = null, Product = null, OrderDate = date };
                                                

                for (int i = 0; i < _totalItemToInsert; i++)
                {
                    cache.Insert(i.ToString(), new Product() { Id = i, Time = date, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 35, Order = order });
                }

                var serializedOrder = Newtonsoft.Json.JsonConvert.SerializeObject(order).ToString();
                var jorder = new JsonObject(serializedOrder);

                string query = "Update Alachisoft.NCache.Sample.Data.Product Test Order = @jorder Where Id=?";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@jorder", jorder);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out var dictionary);

                Helper.ValidateDictionary(dictionary);

                _report.AddPassedTestCase(methodName, "Test Operation in Update query using Json Object");
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }

        public void TestJsonObjectThatDoesnotExist()
        {
            int updated = 0;
            cache.Clear();
            PopulateCache();
            string methodName = MethodBase.GetCurrentMethod().Name;

            count++;
            try
            {

                var order = new Order { OrderID = 10, ShipCity = "rawalpindiiiiiiiiiiiiiiiii", ShipCountry = "Pakistan" };
                var jorder = Helper.GetJsonOrder(order);

                string query = "Update Alachisoft.NCache.Sample.Data.Product Test Order = @ProductName Where Id=?";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@ProductName", jorder);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);

                Helper.ValidateDictionary(dictionary);

                _report.AddFailedTestCase(methodName, new Exception("Test Json Object with wrong values didn't throw exception"));
            }
            catch (Exception ex)
            {
                if (Helper.IsTestOperationException(ex))
                    _report.AddPassedTestCase(methodName, "Test Operation with Json object that didnot exist throwed exception");
                else
                    _report.AddFailedTestCase(methodName, ex);
            }

        }

        public void TestSimpleJsonArray()
        {
            int updated = 0;
            cache.Clear();
            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "Test Operation in Update query using JsonArray";
           
            count++;
            try
            {
                var subArray = new FormatSubArray() { Name = "Sub Array" };
                var subArray1 = new FormatSubArray[1] { subArray };

                var array = new Image[1] { new Image() { ImageFormats = new ImageFormat[1] { new ImageFormat() { formatSubArray = subArray1 } }  }  };
                
                for (int i = 0; i < _totalItemToInsert; i++)
                {
                    cache.Insert(i.ToString(),
                        new Product()
                        {
                            Id = i,
                            Time = DateTime.Now,
                            Name = "Chai",
                            ClassName = "Electronics",
                            Category = "Beverages",
                            UnitPrice = 35,
                            Order = new Order { OrderID = 10, ShipCity = "rawalpindi", ShipCountry = "Pakistan" },
                            Images = array
                        });
                }

                string serializedArray = Newtonsoft.Json.JsonConvert.SerializeObject(subArray1);

                foreach (var item in productList)
                {                   

                    string query = "Update Alachisoft.NCache.Sample.Data.Product Test Images[0].ImageFormats[0].formatSubArray = @jsonArray Where Id=?";
                    JsonArray jsonArray = new JsonArray(serializedArray);

                    QueryCommand queryCommand = new QueryCommand(query);
                    queryCommand.Parameters.Add("@jsonArray", jsonArray);
                    queryCommand.Parameters.Add("Id", item.Id);


                    updated = cache.SearchService.ExecuteNonQuery(queryCommand, out var dictionary);

                    Helper.ValidateDictionary(dictionary);
                }


                _report.AddPassedTestCase(methodName, description);
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }

        public void TestJsonArray()
        {
            int updated = 0;
            cache.Clear();
            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "Test Operation in Update query using JsonArray";

            count++;
            try
            {

                var array = new Image[3] { new Image(), new Image(), new Image() };

                for (int i = 0; i < _totalItemToInsert; i++)
                {
                    cache.Insert(i.ToString(),
                        new Product()
                        {
                            Id = i,
                            Time = DateTime.Now,
                            Name = "Chai",
                            ClassName = "Electronics",
                            Category = "Beverages",
                            UnitPrice = 35,
                            Order = new Order { OrderID = 10, ShipCity = "rawalpindi", ShipCountry = "Pakistan" },
                            Images = array
                        });
                }

                string serializedArray = Newtonsoft.Json.JsonConvert.SerializeObject(array);

                foreach (var item in productList)
                {

                    string query = "Update Alachisoft.NCache.Sample.Data.Product Test Images = @jsonArray Where Id=?";
                    JsonArray jsonArray = new JsonArray(serializedArray);

                    QueryCommand queryCommand = new QueryCommand(query);
                    queryCommand.Parameters.Add("@jsonArray", jsonArray);
                    queryCommand.Parameters.Add("Id", item.Id);

                    // test case fails because server side has serialized array which means that the objects inside the array are wrapped i.e $value {}

                    updated = cache.SearchService.ExecuteNonQuery(queryCommand, out var dictionary);

                    Helper.ValidateDictionary(dictionary);
                }


                _report.AddPassedTestCase(methodName, description);
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }

        public void TestJsonArrayWithWrongValue()
        {
            int updated = 0;
            cache.Clear();
            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "Test Operation in Update query using JsonArray with wrong value";

            count++;
            try
            {
                var obj = new FormatSubArray() { Name = "Sub Array" };
                var subArray = new FormatSubArray[1] { obj };

                var array = new Image[1] { new Image() { ImageFormats = new ImageFormat[1] { new ImageFormat() { formatSubArray = subArray } } } };

                for (int i = 0; i < _totalItemToInsert; i++)
                {
                    cache.Insert(i.ToString(),
                        new Product()
                        {
                            Id = i,
                            Time = DateTime.Now,
                            Name = "Chai",
                            ClassName = "Electronics",
                            Category = "Beverages",
                            UnitPrice = 35,
                            Order = new Order { OrderID = 10, ShipCity = "rawalpindi", ShipCountry = "Pakistan" },
                            Images = array
                        });
                }

                subArray[0].Name = "I donot exist!";
                string serializedArray = Newtonsoft.Json.JsonConvert.SerializeObject(subArray);

                foreach (var item in productList)
                {

                    string query = "Update Alachisoft.NCache.Sample.Data.Product Test Images[0].ImageFormats[0].formatSubArray = @jsonArray Where Id=?";
                    JsonArray jsonArray = new JsonArray(serializedArray);

                    QueryCommand queryCommand = new QueryCommand(query);
                    queryCommand.Parameters.Add("@jsonArray", jsonArray);
                    queryCommand.Parameters.Add("Id", item.Id);

                    updated = cache.SearchService.ExecuteNonQuery(queryCommand, out var dictionary);

                    try
                    {
                        Helper.ValidateDictionary(dictionary);
                        throw new Exception("no exception returned if wrong array is given");
                    }
                    catch (Exception ex)
                    {
                        if (Helper.IsTestOperationException(ex))
                            continue;
                        else
                            throw;
                    }
                }

                _report.AddPassedTestCase(methodName, description);
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }

        public void TestJsonArrayThatDoesnotExist()
        {
            int updated = 0;
            cache.Clear();
            PopulateCache();
            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "Test Operation in Update query using JsonArray that doesnot exist";
            Exception exception = new Exception(description);

            count++;
            try
            {
                var imagesArray = new Image[1] { new Image() { FileName = "This file doesnot exist." } };
                string serializedArray = Newtonsoft.Json.JsonConvert.SerializeObject(imagesArray);
                JsonArray jsonArray = new JsonArray(serializedArray);

                string query = "Update Alachisoft.NCache.Sample.Data.Product Test Images = @jsonArray";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@jsonArray", jsonArray);

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out var dictionary);

                Helper.ValidateDictionary(dictionary);


                _report.AddFailedTestCase(methodName, exception);
            }
            catch (Exception ex)
            {
                if (Helper.IsTestOperationException(ex))
                    _report.AddPassedTestCase(methodName, description);
                else
                    _report.AddFailedTestCase(methodName, ex);
            }


        }

        public void AddObjectByQueryAndThenTestByQuery()
        {
            int updated = 0;
            cache.Clear();
            PopulateCache();
            string methodName = MethodBase.GetCurrentMethod().Name;

            count++;
            try
            {
                // Adding JsonObject In cache to test
                string JsonString = "{\"CompanyName\":\"EverGiven\",\"CompanyId\":3760}";
                JsonObject ShippingComapny = new JsonObject(JsonString);

                string AddQuery = "Update Alachisoft.NCache.Sample.Data.Product Add ShippingComapny = @ShippingComapny Where Id=?";

                QueryCommand AddQueryCommand = new QueryCommand(AddQuery);
                AddQueryCommand.Parameters.Add("@ShippingComapny", ShippingComapny);
                AddQueryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary DictionaryForAddOperaion;
                cache.SearchService.ExecuteNonQuery(AddQueryCommand, out DictionaryForAddOperaion);

                Helper.ValidateDictionary(DictionaryForAddOperaion);

                // A Json object has been added. Now Using Test Query to verify

                var ProductbeforeUpdation = cache.Get<JsonObject>("product1");

                string TestQuery = "Update Alachisoft.NCache.Sample.Data.Product Test ShippingComapny = @ShippingComapny Where Id=?";

                QueryCommand TestQueryCommand = new QueryCommand(TestQuery);
                TestQueryCommand.Parameters.Add("@ShippingComapny", ShippingComapny);
                TestQueryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary dictionary;


                updated = cache.SearchService.ExecuteNonQuery(TestQueryCommand, out dictionary);

                if (dictionary.Count > 0)
                {
                    throw new Exception("Failure:Partial Update, Test Operation using JSON Object");
                }
                else
                {
                    _report.AddPassedTestCase(methodName, "Success: Partial Update, Test Operation using JSON Object");
                }
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }

        #endregion


        #region  --------------------------------- Complex Query -----------------------------------

        public void ComplexQuery()
        {
            int updated = 0;
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
                    $" Add nameThatDoesnotExist = @nameThatDoesnotExist " +
                    $" Test nameThatDoesnotExist = @nameThatDoesnotExist " +
                    $" Set Time = @birthDayTime " +
                    $" Copy ClassName = Category " +
                    $" Move Id = Order.OrderID " +                   
                    $" Remove nameThatDoesnotExist " +                    
                    $" Where Id = @id";

              
               totalExpectedException = 0;

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@nameThatDoesnotExist", nameThatDoesnotExist);
                //queryCommand.Parameters.Add("@birthDayTime", Newtonsoft.Json.JsonConvert.SerializeObject(birthDayTime));
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

                if (result.Id == default )
                    throw new Exception("Move Operation failed in complex query");


                _report.AddPassedTestCase(methodName, "complex query passed");


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }


        #endregion


        #region  --------------------------------- Multiple Operations -----------------------------

        public void MultipleSetAndAddOperations()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "Multiple set and add operations ";


            try
            {
                cache.Clear();
                PopulateCache();

                string groupname = "vip_prod";
                string teaName = "PinkTea";
                string importance = "VIP_Product";
                string city = "VIP_Product";

                string query = "Update  Alachisoft.NCache.Sample.Data.Product Set this.Name = @tea, this.Order.ShipCity = @lahore Add this.Order.Type = @important  Set-meta $group$ = @group where Category = @beverages";
                query = "Update  Alachisoft.NCache.Sample.Data.Product Set this.Name = @tea, this.Order.ShipCity = @lahore Add this.Order.Type = @important   where Category = @beverages";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@tea", teaName);
                queryCommand.Parameters.Add("@important", importance);
                queryCommand.Parameters.Add("@lahore", city);
                queryCommand.Parameters.Add("@group", groupname);


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
                            cacheItem.Group != groupname;

                        if (!allValuesMatches)
                            throw new Exception(description);

                    }
                }

                // some extra verification
                var returned = cache.SearchService.GetGroupKeys(groupname);
                if (returned.Count == updated)
                    throw new Exception(description);

                _report.AddPassedTestCase(methodName, description);


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }
        }

        #endregion


        #region  --------------------------------- Helper Methods --------------------------------- 



        private void PopulateCache(int totalItemToAdd = -1)
        {
            int itemsAdded = 0;
            foreach (var item in productList)
            {
                cache.Add("product" + item.Id, item);

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

            CacheItem cacheItem = new (null);

            foreach (var item in productList)
            {
                key = "product" + item.Id;
                keys.Add(key);

                 cacheItem.SetValue(item);

                cache.Add(key, cacheItem);

                itemsAdded++;
                if (totalItemToAdd != -1 && itemsAdded == totalItemToAdd)
                    break;
            }

            return keys;
        }

        private void PopulateProductList()
        {
            //productList.Add(new Product() { Id = 1, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 35, Order = new Order { OrderID = 10, ShipCity = "rawalpindi", ShipCountry = "Pakistan" }, Images = new Image[3] { new Image(), new Image(), new Image() } });
            //return;


            productList.Add(new Product() { Id = 1, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 35, Order = new Order { OrderID = 10, ShipCity = "rawalpindi", ShipCountry = "Pakistan" }, Images = new Image[3] { new Image(), new Image(), new Image() } });
            productList.Add(new Product() { Id = 2, Time = DateTime.Now, Name = "Chang", ClassName = "Electronics", Category = "Meat", UnitPrice = 18, Order = new Order { OrderID = 10, ShipCity = "Amsterdam", ShipCountry = "America" }, Images = new Image[3] { new Image(), new Image(), new Image() } });
            productList.Add(new Product() { Id = 3, Time = DateTime.Now, Name = "Aniseed Syrup", ClassName = "Electronics", Category = "Beverages", UnitPrice = 258, Order = new Order { OrderID = 10, ShipCity = "rawalpindi", ShipCountry = "Pakistan", Product = new Product() { Time = DateTime.Now } }, Images = new Image[3] { new Image(), new Image(), new Image() } });
            productList.Add(new Product() { Id = 4, Time = DateTime.Now, Name = "IKura", ClassName = "Electronics", Category = "Produce", UnitPrice = 50, Order = new Order { OrderID = 10, ShipCity = "lahore", ShipCountry = "Pakistan" }, Images = new Image[3] { new Image(), new Image(), new Image() } });
            //productList.Add(new Product() { Id = 5, Time = DateTime.Now, Name = "Tofu", ClassName = "Electronics", Category = "Seafood", UnitPrice = 78, Images = new Image[3] { new Image(), new Image(), new Image() } });
            productList.Add(new Product() { Id = 6, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 37, Order = new Order { OrderID = 10, ShipCity = "multan", ShipCountry = "Pakistan" }, Images = new Image[3] { new Image(), new Image(), new Image() } });
            productList.Add(new Product() { Id = 7, Time = DateTime.Now, Name = "Chang", ClassName = "Electronics", Category = "Meat", UnitPrice = 18, Order = new Order { OrderID = 10, ShipCity = "tokoyo", ShipCountry = "japan" }, Images = new Image[3] { new Image(), new Image(), new Image() } });
            productList.Add(new Product() { Id = 8, Time = DateTime.Now, Name = "Aniseed Syrup", ClassName = "Electronics", Category = "Beverages", UnitPrice = 258, Order = new Order { OrderID = 10, ShipCity = "karachi", ShipCountry = "Pakistan", Product = new Product() { Time = DateTime.Now } }, Images = new Image[3] { new Image(), new Image(), new Image() } });
            productList.Add(new Product() { Id = 9, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 37, Order = new Order { OrderID = 10, ShipCity = "london", ShipCountry = "England" }, Images = new Image[3] { new Image(), new Image(), new Image() } });
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

        private Product GetProduct()
        {

            return new Product() { Id = 1, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 357, Order = new Alachisoft.NCache.Sample.Data.Order { OrderID = 10, ShipCity = "rawalpindi" } };
        }

        #endregion

    }

}
