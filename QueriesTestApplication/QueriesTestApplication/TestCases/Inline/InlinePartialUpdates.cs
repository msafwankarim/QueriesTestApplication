using Alachisoft.NCache.Client;
using Alachisoft.NCache.Common.Monitoring;
using Alachisoft.NCache.JNIBridge.Net;
using Alachisoft.NCache.Runtime.CacheManagement;
using Alachisoft.NCache.Runtime.JSON;
using Alachisoft.NCache.Sample.Data;
using NUnit.Framework;
using Quartz.Util;
using QueriesTestApplication.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Reflection;
using System.Text;
using ReportHelper = QueriesTestApplication.Utils.ReportHelper;

namespace QueriesTestApplication
{
    [TestFixture]
    public class InlinePartialUpdates
    {
        ICache cache;
        public Dictionary<string, ResultStatus> testResults;
        int count = 0;
        List<Product> productList;
        List<Product> complexProductList;
        int _totalItemToInsert = 100;

        Report _report;

        public InlinePartialUpdates()
        {
            cache = CacheManager.GetCache(Common.CacheName);
            testResults = new Dictionary<string, ResultStatus>();
            productList = new List<Product>();
            complexProductList = new List<Product>();

            _report = new Report(nameof(InlinePartialUpdates));

            PopulateProductList();
        }

        public Report Report { get => _report; }

        public Dictionary<string, ResultStatus> TestResults
        {
            get { return testResults; }
        }


        [OneTimeTearDown]
        public void Dispose()
        {
            cache?.Dispose();
        }

        #region --------------------------------- Add Operation  ---------------------------------

      
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
                string query = $"Update Alachisoft.NCache.Sample.Data.Product Add this.{arrayName}=" +
                    " '[{\"name\":\"phone\",\"model\":\"p30PRO\"}]' where  Category = @beverages";
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


                string query = $"Update Alachisoft.NCache.Sample.Data.Product Add this.Order = '{jsonObject.ToString()}' ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                
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


                string query = $"Update Alachisoft.NCache.Sample.Data.Product Add this.Order = '{jsonObject.ToString()}' ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                
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

                string query = $"Update Alachisoft.NCache.Sample.Data.Product Add this.Images[0] = '{jsonObj}' ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@jsonObj", jsonObj);

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                


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

                string query = $"Update Alachisoft.NCache.Sample.Data.Product Add this.Images[0] = '{jsonObj}' ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                


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

                string query = $"Update Alachisoft.NCache.Sample.Data.Product Add this.Order = '{jsonArray }'";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                //todo why this directly throws exception
                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                
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

                string query = $"Update Alachisoft.NCache.Sample.Data.Product Add this.Order = '{jsonArray }'";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                //todo why this directly throws exception
                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                
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


                string query = $"Update Alachisoft.NCache.Sample.Data.Product Add this.Order1[0] = '{jsonObject}' ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                

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


                string query = $"Update Alachisoft.NCache.Sample.Data.Product Add this.Images[10] = '{jsonArray}' ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");
                queryCommand.Parameters.Add("@jArray", jsonArray);

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                

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
                JsonObject ShippingComapny = new JsonObject(JsonString);

                string query = $"Update Alachisoft.NCache.Sample.Data.Product Add ShippingComapny = '{ShippingComapny}' Where Id=?";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary dictionary;


                updated = cache.SearchService.ExecuteNonQuery(queryCommand);


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

                string query = $"Update Alachisoft.NCache.Sample.Data.Product Set Images[{indexToUpdate}].FileName = '\"UpdatedSkeleton\"' ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary<string, Exception> dictionary;


                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                
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

        [Test]
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
                StringBuilder shippingAddress = new StringBuilder("John Doe" +
                                 "1234 Elm Street, Apt. 567" +
                                 "Building XYZ" +
                                 "Suite 890" +
                                 "Cityville, State 12345" +
                                 "Country_");

                for (int i = 0; i < 200; i++)
                {
                    shippingAddress.Append(i.ToString());
                }

                var order = new Order { OrderID = 11, ShipCity = shipCity, ShipCountry = "US"  };
                var jsonOrder = Helper.GetJsonOrder(order);

                jsonOrder.AddAttribute("ShipAddress",shippingAddress.ToString());

                string query = $"Update Alachisoft.NCache.Sample.Data.Product Set Order = '{jsonOrder}' Where Id=?";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary<string, Exception> dictionary;

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

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


                string query = $"Update Alachisoft.NCache.Sample.Data.Product Set this.Images[0] = '{jsonArray}' ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                
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


                string query = $"Update Alachisoft.NCache.Sample.Data.Product Set this.Images[2] = '{jsonArray}' ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                
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

                string query = $"Update Alachisoft.NCache.Sample.Data.Product Set Images[{indexToUpdate}].FileName = '\"{ImageFileName}\"' ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary<string, Exception> dictionary;


                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                
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
                string query = $"Update Alachisoft.NCache.Sample.Data.Product Set Images[{updateIndex}].ImageFormats[{updateIndex}].Format = '\"{ImageFormat}\"' ";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@ImageFormat", ImageFormat);

                IDictionary<string, Exception> dictionary;

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                
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


                string query = $"Update Alachisoft.NCache.Sample.Data.Product Set Images[-1].ImageFormats[0].Format = '{ImageFormat}' ";

                QueryCommand queryCommand = new QueryCommand(query);

                IDictionary<string, Exception> dictionary;

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                
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


                string query = $"Update Alachisoft.NCache.Sample.Data.Product Set this.Images[10] = '{jsonArray}'  ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");

                var updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                

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


                string query = $"Update Alachisoft.NCache.Sample.Data.Product Set this.Images[*] = '{jsonArray}'  ";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@beverages", "Beverages");

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
                IDictionary<string, Exception> dictionary;

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                
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

                string query = $"Update Alachisoft.NCache.Sample.Data.Product Set Order = '{order}' Where Id=?";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary<string, Exception> dictionary;


                updated = cache.SearchService.ExecuteNonQuery(queryCommand);


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

                string query = $"Update  Alachisoft.NCache.Sample.Data.Product Test Name = '{jsonValue}', Order.OrderID=10 Where Id=?";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                
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

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                
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

                string query =$"Update  Alachisoft.NCache.Sample.Data.Product Test Name = '{jsonValue}', Order.OrderID=10 Where Id=?";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

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
                var order = new Order { OrderID = 10, ShipCity = "rawalpindi", ShipCountry = "Pakistan", ShipName = null, ShipAddress = null, Product = null, OrderDate = date };


                for (int i = 0; i < _totalItemToInsert; i++)
                {
                    cache.Insert(i.ToString(), new Product() { Id = i, Time = date, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 35, Order = order });
                }

                var serializedOrder = Newtonsoft.Json.JsonConvert.SerializeObject(order).ToString();
                var jorder = new JsonObject(serializedOrder);

                string query = $"Update Alachisoft.NCache.Sample.Data.Product Test Order = '{jorder}' Where Id=?";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                
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

                string query = $"Update Alachisoft.NCache.Sample.Data.Product Test Order ='{jorder}' Where Id=?";

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary<string, Exception> dictionary;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                
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

                var array = new Image[1] { new Image() { ImageFormats = new ImageFormat[1] { new ImageFormat() { formatSubArray = subArray1 } } } };

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
                    JsonArray jsonArray = new JsonArray(serializedArray);

                    string query = $"Update Alachisoft.NCache.Sample.Data.Product Test Images[0].ImageFormats[0].formatSubArray = '{jsonArray}' Where Id=?";

                    QueryCommand queryCommand = new QueryCommand(query);
                    queryCommand.Parameters.Add("Id", item.Id);


                    updated = cache.SearchService.ExecuteNonQuery(queryCommand);

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
                    JsonArray jsonArray = new JsonArray(serializedArray);
                    string query = $"Update Alachisoft.NCache.Sample.Data.Product Test Images = '{jsonArray}' Where Id=?";

                    QueryCommand queryCommand = new QueryCommand(query);
                    queryCommand.Parameters.Add("Id", item.Id);

                    // test case fails because server side has serialized array which means that the objects inside the array are wrapped i.e $value {}

                    updated = cache.SearchService.ExecuteNonQuery(queryCommand);

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
                    JsonArray jsonArray = new JsonArray(serializedArray);

                    string query = $"Update Alachisoft.NCache.Sample.Data.Product Test Images[0].ImageFormats[0].formatSubArray = '{jsonArray}' Where Id=?";

                    QueryCommand queryCommand = new QueryCommand(query);
                    queryCommand.Parameters.Add("Id", item.Id);

                    updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                    try
                    {
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

                string query = $"Update Alachisoft.NCache.Sample.Data.Product Test Images = '{jsonArray}'";

                QueryCommand queryCommand = new QueryCommand(query);

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                

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

                string AddQuery = $"Update Alachisoft.NCache.Sample.Data.Product Add ShippingComapny = '{ShippingComapny}' Where Id=?";

                QueryCommand AddQueryCommand = new QueryCommand(AddQuery);
                AddQueryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary<string, Exception> DictionaryForAddOperaion;
                cache.SearchService.ExecuteNonQuery(AddQueryCommand);

                
                // A Json object has been added. Now Using Test Query to verify

                var ProductbeforeUpdation = cache.Get<JsonObject>("product1");

                string TestQuery = "Update Alachisoft.NCache.Sample.Data.Product Test ShippingComapny = @ShippingComapny Where Id=?";

                QueryCommand TestQueryCommand = new QueryCommand(TestQuery);
                TestQueryCommand.Parameters.Add("@ShippingComapny", ShippingComapny);
                TestQueryCommand.Parameters.Add("Id", Convert.ToInt32(1));

                IDictionary<string, Exception> dictionary = new Dictionary<string, Exception>();


                updated = cache.SearchService.ExecuteNonQuery(TestQueryCommand);

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
                    $" Add nameThatDoesnotExist = '{nameThatDoesnotExist}'   " +
                    $" Test nameThatDoesnotExist = '{nameThatDoesnotExist}'  " +
                    $" Set Time = @birthDayTime " +
                    $" Copy ClassName = Category " +
                    $" Move Id = Order.OrderID " +
                    $" Remove nameThatDoesnotExist " +
                    $" Where Id = @id";


                totalExpectedException = 0;

                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@birthDayTime", birthDayTime);
                queryCommand.Parameters.Add("@id", 1);

                updated = cache.SearchService.ExecuteNonQuery(queryCommand);

                if (updated == 0)
                    throw new Exception("No key updated by complex query");

                
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

                string query = $"Update  Alachisoft.NCache.Sample.Data.Product Set this.Name = '\"{teaName}\"', this.Order.ShipCity = '\"{city}\"' Add this.Order.Type = '\"{importance}\"'  Set-meta $group$ = '\"{groupname}\"' where Category = 'Beverages'";
                //query =         "Update  Alachisoft.NCache.Sample.Data.Product Set this.Name = @tea, this.Order.ShipCity = @lahore Add this.Order.Type = @important   where Category = @beverages";
                QueryCommand queryCommand = new QueryCommand(query);

                var updated = cache.QueryService.ExecuteNonQuery(queryCommand).AffectedRows;

                Helper.ThrowExceptionIfNoUpdates(updated);

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


        #region  --------------------------------- Helper Methods --------------------------------- 



        private void PopulateCache(int totalItemToAdd = -1)
        {
            int itemsAdded = 0;
            CacheItem caheItem = new CacheItem(null);

            foreach (var item in productList)
            {
                caheItem.SetValue(item);
                cache.Add("product" + item.Id, caheItem);

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

            var cacheItem = new CacheItem(null);
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
