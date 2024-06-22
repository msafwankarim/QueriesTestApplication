using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Alachisoft.NCache.Runtime.JSON;
using Alachisoft.NCache.Sample.Data;

namespace QueriesTestApplication.Utils
{
    public static class Helper
    {

        private static string _testOperationExceptionMessage = "Specified value not equals to test value";
        private static string _incorrectMeta = "Incorrect meta information defined. Failed to parse ";

        public static string TestOperationExceptionMessage { get => _testOperationExceptionMessage; }

        public static void ValidateDictionary(IDictionary<string, Exception> dictionary)
        {
            if (dictionary.Count > 0)
            {
                foreach (var value in dictionary.Values)
                {
                    throw new Exception($"Failed operation returned. Message : {value.ToString()}");
                }
            }
        }

        public static void ThrowExceptionIfNoUpdates(int totalUpdatesdItems)
        {
            if(totalUpdatesdItems == 0)
                throw new Exception("No items is updated");
        }

        public static Product GetProduct()
        {
            return new Product() { Id = 1, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 35, Order = new Order { OrderID = 10, ShipCity = "rawalpindi", ShipCountry = "Pakistan" }, Images = new Image[1] { new Image() } };

        }


        public static JsonObject GetJsonOrder(Order order)
        {
            var jsonorder = new JsonObject();
            jsonorder.AddAttribute("OrderID", order.OrderID);
            jsonorder.AddAttribute("ShipCity", order.ShipCity);
            jsonorder.AddAttribute("ShipCountry", order.ShipCountry);
            return jsonorder;
        }


        #region -------------------------- Exception Validations -------------------------- 

        public static bool IsInCorrectMetaException(Exception ex)
        {
            return ex.Message.Contains(_incorrectMeta);
        }

        public static bool ISProductBinarySerializedException(Exception ex)
        {
            return ex.Message.Contains("Unable to cast object of type 'Alachisoft.NCache.Sample.Data.Product' to type 'Alachisoft.NCache.Runtime.JSON.JsonObject'.");

        }

        public static bool IsTargetNotFoundException(Exception ex)
        {
            return ex.Message.Contains("Unable to find target location at the specified segment");
        }

        public static bool IsIncorrectFormatException(Exception ex)
        {
            return ex.Message.Contains("Incorrect query format");
        }

        public static bool IsIndexOutOfBoundException(Exception ex)
        {
            return ex.Message.Contains("Index was outside the bounds of the array");
        }

        public static bool IsTestOperationException(Exception ex)
        {
            return ex.Message.Contains(_testOperationExceptionMessage);
        }

        #endregion


        public static JsonArray GetNamedTagsArray()
        {

            //NameTags
            JsonObject FlashDiscount = new JsonObject();
            FlashDiscount.AddAttribute("FlashDiscount", "NoFlashDiscount");
            FlashDiscount.AddAttribute("type", "string");

            JsonObject Discount = new JsonObject();
            Discount.AddAttribute("Discount", "Yes");
            Discount.AddAttribute("type", "string");

            JsonObject Percentage = new JsonObject();
            double percentage = 1.5;
            Percentage.AddAttribute("Percentage", percentage);
            Percentage.AddAttribute("type", "double");

            //NameTagArray Containing all the NameTags
            JsonArray NameTagsArray = new JsonArray();
            NameTagsArray.Add(FlashDiscount);
            NameTagsArray.Add(Discount);
            NameTagsArray.Add(Percentage);

            return NameTagsArray;
        }

        
    }
}
