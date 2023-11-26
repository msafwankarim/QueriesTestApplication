using Alachisoft.NCache.Client;
using Alachisoft.NCache.JNIBridge.Net;
using Alachisoft.NCache.Runtime.Caching;
using Alachisoft.NCache.Runtime.Enum;
using Alachisoft.NCache.Runtime.Exceptions;
using Alachisoft.NCache.Runtime.JSON;
using Alachisoft.NCache.Sample.Data;
using log4net.Extended.Core;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using QueriesTestApplication.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using ReportHelper = QueriesTestApplication.Utils.ReportHelper;
using System.ServiceModel.Channels;
using System.Linq.Expressions;
using System.ServiceModel;

namespace QueriesTestApplication
{
    class UpdateQueriesTestForJsonObject
    {
        ICache cache;
        public Dictionary<string, ResultStatus> testResults;
        int count = 0;
        List<Product> productList;
        List<Product> complexProductList;

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
                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                foreach (var item in productList)
                {
                    if (item.Category == "Beverages")
                    {
                        var jsonObjFromCache = cache.Get<JsonObject>("product" + item.Id);

                        if (jsonObjFromCache.ContainsAttribute(jsonStringKey))
                        {
                            var attributeValueFromCache = jsonObjFromCache.GetAttributeValue(jsonStringKey).ToString();

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

            var keys = PopulateCacheAndGetKeys();

            try
            {
                PopulateCache();

                var order = new Order { OrderID = 10, ShipCity = "rawalpindi\\Islamabad", ShipCountry = "Pakistan\\ASia" };
                var jorder = Helper.GetJsonOrder(order);

                string query = "Update Alachisoft.NCache.Sample.Data.Product Add this.Order = @jOrder ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@jOrder", jorder);

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
                PopulateCache();

                const int arraySize = 2;

                var order = new Order { OrderID = 10, ShipCity = "rawalpindi\\Islamabad", ShipCountry = "Pakistan\\ASia" };
                var orderArray = new Order[arraySize] { order, order };


                JsonObject jsonArray = new JsonObject(Newtonsoft.Json.JsonConvert.SerializeObject(orderArray));


                string query = "Update Alachisoft.NCache.Sample.Data.Product Add this.Images[0] = @jArray ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@jArray", jsonArray);

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand, out var dictionary);

                Helper.ValidateDictionary(dictionary);


                // ToDo Validataion after discussion with sir taimoor

                var exception = new Exception(description);

                foreach (string key in keys)
                {
                    var jsonObject = cache.Get<JsonObject>(key);
                    //JObject jsonObject = JObject.Parse(result.ToString());

                    var imagesArray = (JsonArray)jsonObject.GetAttributeValue("Images");

                    // verify 1st index is still image. It should not be order 
                    var imageObj = Newtonsoft.Json.JsonConvert.DeserializeObject<Image>(imagesArray[1].ToString());

                    var updatedOrder = Newtonsoft.Json.JsonConvert.DeserializeObject<Order[]>(imagesArray[0].ToString());

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

        public void AddJsonArrayInObject()
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
                JsonArray jsonArray = new JsonArray();
                jsonArray.Add(Helper.GetJsonOrder(order));

                string query = "Update Alachisoft.NCache.Sample.Data.Product Add this.Order = @jArray ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@jArray", jsonArray);

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand, out var dictionary);

                Helper.ValidateDictionary(dictionary);

                foreach (var key in keys)
                {
                    var jsonObj = cache.Get<JsonObject>(key);
                    var array = jsonObj["Order"] as JsonArray;

                    if (array.Count != arraySize)
                        throw new Exception("Unable to add array");
                    if (((JsonObject)array[0]).GetAttributeValue("ShipCity").ToString() != order.ShipCity)
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
                PopulateCache();

                const int arraySize = 2;

                var order = new Order { OrderID = 10, ShipCity = "rawalpindi\\Islamabad", ShipCountry = "Pakistan\\ASia" };
                var orderArray = new Order[arraySize] { order, order };

                JsonObject jsonArray = new JsonObject(Newtonsoft.Json.JsonConvert.SerializeObject(orderArray));


                string query = "Update Alachisoft.NCache.Sample.Data.Product Add this.Order1[0] = @jOrder ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@jOrder", jsonArray);

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
                PopulateCache();

                const int arraySize = 2;

                var order = new Order { OrderID = 10, ShipCity = "rawalpindi\\Islamabad", ShipCountry = "Pakistan\\ASia" };
                var orderArray = new Order[arraySize] { order, order };

                JsonObject jsonArray = new JsonObject(Newtonsoft.Json.JsonConvert.SerializeObject(orderArray));


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
            string methodName = "AddOperationUsingJsonObject";
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
                PopulateCache();

                const int arraySize = 2;

                var order = new Order { OrderID = 10, ShipCity = "rawalpindi\\Islamabad", ShipCountry = "Pakistan\\ASia" };
                var orderArray = new Order[arraySize] { order, order };

                var jsonArray = new JsonArray(JsonConvert.SerializeObject(orderArray));


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
                    var imageObj = JsonConvert.DeserializeObject<Image>(imagesArray[1].ToString());

                    var updatedOrderArray = JsonConvert.DeserializeObject<Order[]>(imagesArray[0].ToString());

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
                PopulateCache();

                const int arraySize = 2;

                var order = new Order { OrderID = 10, ShipCity = "rawalpindi\\Islamabad", ShipCountry = "Pakistan\\ASia" };
                var orderArray = new Order[arraySize] { order, order };

                var jsonArray = new JsonArray(JsonConvert.SerializeObject(orderArray));


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

                string serializedImage = JsonConvert.SerializeObject(image);
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

                    if (result.Images[updateIndex].ImageFormats[updateIndex].Format.Equals(ImageFormat))
                        _report.AddPassedTestCase(methodName, "Set operation to attribute of array with in array");
                    else
                        throw new Exception("Set operation to attribute of array with in array");

                }

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
                PopulateCache();

                const int arraySize = 2;

                var order = new Order { OrderID = 10, ShipCity = "rawalpindi\\Islamabad", ShipCountry = "Pakistan\\ASia" };
                var orderArray = new Order[arraySize] { order, order };

                var jsonArray = new JsonArray(JsonConvert.SerializeObject(orderArray));


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
                PopulateCache();

                const int arraySize = 2;

                var order = new Order { OrderID = 10, ShipCity = "rawalpindi\\Islamabad", ShipCountry = "Pakistan\\ASia" };
                var orderArray = new Order[arraySize] { order, order };

                var jsonArray = new JsonArray(JsonConvert.SerializeObject(orderArray));


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
            string methodName = "SetOperationUsingJsonObject";
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
            string methodName = nameof(CopyQuery1);
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
                IDictionary dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);
                int TotalFailedOperations = 0;

                Helper.ValidateDictionary(dictionary);

                foreach (DictionaryEntry val in dictionary)
                {
                    TotalFailedOperations++;
                    //ReportHelper.PrintError($"Operation failed for key: {val.Key} with exception : {val.Value.ToString()}");
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
            string methodName = nameof(CopyQuery1);
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
                IDictionary dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);
                int TotalFailedOperations = 0;
                foreach (DictionaryEntry val in dictionary)
                {
                    TotalFailedOperations++;
                    ReportHelper.PrintError($"Operation failed for key: {val.Key} with exception : {val.Value.ToString()}");
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
            string methodName = nameof(CopyQuery1);
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
                IDictionary dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);
                int TotalFailedOperations = 0;
                foreach (DictionaryEntry val in dictionary)
                {
                    TotalFailedOperations++;
                    ReportHelper.PrintError($"Operation failed for key: {val.Key} with exception : {val.Value.ToString()}");
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
            string methodName = "CopyQuery";
            count++;
            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Copy Order.ShipCountry = Order.ShipCity Where Id=?";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);

                foreach (DictionaryEntry val in dictionary)
                {
                    ReportHelper.PrintError($"Operation failed for key: {val.Key} with exception : {val.Value.ToString()}");
                }

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
            string methodName = "CopyQuery";
            count++;
            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Copy this.Images[0] = this.Images[1] Where Id=?";

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
        private void CopyArrayToObject()
        {
            int updated = 0;
            cache.Clear();
            PopulateCache();
            string ProductKey = "product1";
            string methodName = "CopyQuery";
            count++;
            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Copy this.Images[0] = this.Order Where Id=?";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));


                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out IDictionary dictionary);

                var exception = new Exception("Failure:Partial Update item to copy array indexes");
                Helper.ValidateDictionary(dictionary);
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

                IDictionary dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);

                // product ID should not be replaced by order object

                Helper.ValidateDictionary(dictionary);


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
            string methodName = nameof(CopyQuery1);
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
                IDictionary dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);
                Helper.ValidateDictionary(dictionary);
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
            string methodName = nameof(CopyQuery1);
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
                IDictionary dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);
                int TotalFailedOperations = 0;
                Helper.ValidateDictionary(dictionary);
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
            string methodName = nameof(MoveQuery);
            count++;
            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Move Order.ShipCountry = Order.ShipCity ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);

                foreach (DictionaryEntry val in dictionary)
                {
                    ReportHelper.PrintError($"Operation failed for key: {val.Key} with exception : {val.Value.ToString()}");
                }

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
            string methodName = nameof(MoveQuery);
            count++;
            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Move Order.ShipCountry = Order.ShipCity ,Order.Category=Order.ClassName ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);

                foreach (DictionaryEntry val in dictionary)
                {
                    ReportHelper.PrintError($"Operation failed for key: {val.Key} with exception : {val.Value.ToString()}");
                }

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
            string methodName = nameof(MoveQuery);
            count++;
            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Move Order.ShipCountry = null ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);

                foreach (DictionaryEntry val in dictionary)
                {
                    ReportHelper.PrintError($"Operation failed for key: {val.Key} with exception : {val.Value.ToString()}");
                }
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
            string methodName = nameof(MoveQuery);
            count++;
            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Move Order.ShipCountry = Order.ShipCity Where Id=?";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out dictionary);

                foreach (DictionaryEntry val in dictionary)
                {
                    ReportHelper.PrintError($"Operation failed for key: {val.Key} with exception : {val.Value.ToString()}");
                }

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
            string methodName = nameof(MoveQuery);
            count++;
            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Move Order.AttributeThatDoesnotEixst = Order.ShipCity";

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
            string methodName = nameof(MoveQuery);
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
            string methodName = "MoveQuery";
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
            PopulateCache();
            string ProductKey = "product1";
            string methodName = "MoveQuery";
            count++;
            try
            {
                string query = "Update  Alachisoft.NCache.Sample.Data.Product  Move this.Images[0] = this.Order ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));


                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out IDictionary dictionary);

                var exception = new Exception("Failure:Partial Update item to move array indexes");
                Helper.ValidateDictionary(dictionary);
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
                string name = "\"Chai\"";

                JsonValue jsonValue = (JsonValue)name;

                //string query = "Update  Alachisoft.NCache.Sample.Data.Product Test Name = '\"Chai\"', Order.OrderID=10 Where Id=?";
                string query = "Update  Alachisoft.NCache.Sample.Data.Product Test Name = @value, Order.OrderID=10 Where Id=?";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));
                queryCommand.Parameters.Add("value", jsonValue);

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
                string name = "\"Chai has been drunk\"";

                JsonValue jsonValue = (JsonValue)name;

                //string query = "Update  Alachisoft.NCache.Sample.Data.Product Test Name = '\"Chai\"', Order.OrderID=10 Where Id=?";
                string query = "Update  Alachisoft.NCache.Sample.Data.Product Test Name = @value, Order.OrderID=10 Where Id=?";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));
                queryCommand.Parameters.Add("value", jsonValue);

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out var dictionary);

                Helper.ValidateDictionary(dictionary);
                _report.AddFailedTestCase(methodName,new Exception(description));

            }
            catch (Exception ex)
            {
                if(Helper.IsTestOperationException(ex))
                    _report.AddPassedTestCase(methodName, description);
                else
                    _report.AddFailedTestCase(methodName, ex);
            }


        }

        public void TestJsonObject()
        {
            int updated = 0;
            cache.Clear();
            PopulateCache();
            string methodName = MethodBase.GetCurrentMethod().Name;

            count++;
            try
            {
                var order = new Order { OrderID = 10, ShipCity = "rawalpindi", ShipCountry = "Pakistan" };
                var jorder = Helper.GetJsonOrder(order);

                string query = "Update Alachisoft.NCache.Sample.Data.Product Test Order = @ProductName Where Id=?";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@ProductName", jorder);
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

        public void TestJsonArray()
        {
            int updated = 0;
            cache.Clear();
            PopulateCache();
            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "Test Operation in Update query using JsonArray";


            count++;
            try
            {
                foreach (var item in productList)
                {
                    string serializedArray = Newtonsoft.Json.JsonConvert.SerializeObject(item.Images);

                    string query = "Update Alachisoft.NCache.Sample.Data.Product Test Images = @jsonArray Where Id=?";
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

        // Test (exception bcz attribut not added) -> Add -> Test -> Set -> Copy -> Move -> Remove -> Test (exction bcz attrib removed)
        private void ComplexQuery()
        {
            int updated = 0;
            cache.Clear();
            PopulateCache();
            string methodName = MethodBase.GetCurrentMethod().Name;
            int totalExpectedException = 2;

            count++;
            try
            {
                string nameThatDoesnotExist = "\"I drink two cups of tea daily.\"";
                DateTime birthDayTime = new DateTime(2000, 1, 5);


                //string query = "Update  Alachisoft.NCache.Sample.Data.Product Test Name = '\"Chai\"', Order.OrderID=10 Where Id=?";
                string query = $"Update  Alachisoft.NCache.Sample.Data.Product " +
                    $" Test nameThatDoesnotExist = @nameThatDoesnotExist " +
                    $" Add nameThatDoesnotExist = @nameThatDoesnotExist " +
                    $" Test nameThatDoesnotExist = @nameThatDoesnotExist " +
                    $" Set Time = birthDayTime " +
                    $" Copy ClassName = Category " +
                    $" Move Id = UnitPrice " +
                    $" Remove nameThatDoesnotExist " +
                    $" Test nameThatDoesnotExist = @nameThatDoesnotExist " +
                    $" Where Id = @id";



                QueryCommand queryCommand = new QueryCommand(query);

                queryCommand.Parameters.Add("nameThatDoesnotExist", nameThatDoesnotExist);
                queryCommand.Parameters.Add("birthDayTime", birthDayTime);
                queryCommand.Parameters.Add("Id", 1);

                updated = cache.SearchService.ExecuteNonQuery(queryCommand, out IDictionary dictionary);

                if (updated == 0)
                    throw new Exception("No key updated by complex query");

                if (dictionary.Count != totalExpectedException)
                    throw new Exception("Got more then expected exceptions in complex query");

                foreach (var value in dictionary.Values)
                {
                    if (value.ToString().Contains("Specified value not equals to test value"))
                        continue;

                    throw new Exception($"Failed operation returned. Message : {value}");
                }

                var jsonObject = cache.Get<JsonObject>("product1");

                if (jsonObject.ContainsAttribute("nameThatDoesnotExist"))
                    throw new Exception("Remove Operation failed in complex query");

                var result = cache.Get<Product>("product1");
                var updatedDate = result.Time;

                if (updatedDate.Year != 2000 || updatedDate.Month != 1 || updatedDate.Day != 5)
                    throw new Exception("Set Operation failed in complex query");

                if(result.Category != "Beverages" || result.ClassName != "Beverages")
                    throw new Exception("Copy Operation failed in complex query");

                if(result.Id != default || result.UnitPrice != 35)
                    throw new Exception("Move Operation failed in complex query");


                _report.AddPassedTestCase(methodName, "complex query passed: Test -> Add -> Test -> Set -> Copy -> Move -> Remove -> Test");


            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }


        #endregion


        #region  --------------------------------- Helper Methods --------------------------------- 

        private Product GetProduct()
        {

            return new Product() { Id = 1, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 357, Order = new Alachisoft.NCache.Sample.Data.Order { OrderID = 10, ShipCity = "rawalpindi" } };
        }
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

        private void PopulateProductList()
        {
            productList.Add(new Product() { Id = 1, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 35, Order = new Order { OrderID = 10, ShipCity = "rawalpindi", ShipCountry = "Pakistan" }, Images = new Image[3] { new Image(), new Image(), new Image() } });
            /*            productList.Add(new Product() { Id = 2, Time = DateTime.Now, Name = "Chang", ClassName = "Electronics", Category = "Meat", UnitPrice = 18, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });
                        productList.Add(new Product() { Id = 3, Time = DateTime.Now, Name = "Aniseed Syrup", ClassName = "Electronics", Category = "Beverages", UnitPrice = 258, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });
                        productList.Add(new Product() { Id = 4, Time = DateTime.Now, Name = "IKura", ClassName = "Electronics", Category = "Produce", UnitPrice = 50, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });
                        productList.Add(new Product() { Id = 5, Time = DateTime.Now, Name = "Tofu", ClassName = "Electronics", Category = "Seafood", UnitPrice = 78 });
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
                        productList.Add(new Product() { Id = 25, Time = DateTime.Now, Name = "Chang", ClassName = "Electronics", Category = "Meat", UnitPrice = 88, Order = new Order { OrderID = 10, ShipCity = "rawalpindi" } });*/
            // ExpandProductList(10);
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


        #endregion

    }

}
