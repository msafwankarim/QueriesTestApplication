using Alachisoft.NCache.Runtime.JSON;
using Newtonsoft.Json.Linq;
using QueriesTestApplication.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using JArray = Newtonsoft.Json.Linq.JArray;
using Newtonsoft.Json.Linq;
using System.Text.Json.Serialization;

namespace QueriesTestApplication.VerifyEscapeSequences
{
    internal class BackslashVerifier
    {
        public string _city = @"Islamabad\Pakistan";
        public string _jsonCity = @"Islamabad\\Pakistan";

        Report _report;
        public Report Report { get => _report; }

        internal BackslashVerifier()
        {
            _report = new Report(nameof(BackslashVerifier));
        }


        public void VerifyInvalidSlashInArray()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "verify single backslash  ";

            try
            {
                JsonArray jsonArray = new JsonArray();
                jsonArray.Add(_city);

                string jsonString = jsonArray.ToString();
                var jsonToken = JToken.Parse(jsonString);

                JArray jArray = new JArray();
                jArray.Add(_city);

                string jString = jArray.ToString();
                var jToken = JToken.Parse(jString);

                bool jsonResult = jsonArray.Contains(_jsonCity);
                bool jResult = jArray.Contains(_jsonCity);

                if (!IsJsonAndJResultEqual(jsonResult, jResult))
                    throw new Exception(StringResources.ContainsNotEqual);

                jsonResult = jsonArray.Contains(_city);
                jResult = jArray.Contains(_city);

                if (!IsJsonAndJResultEqual(jsonResult, jResult))
                    throw new Exception(StringResources.ContainsNotEqual);


                _report.AddPassedTestCase(methodName, description);

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }

        public void VerifyValidSlashInArray()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "verify double backslash  ";

            try
            {
                JsonArray jsonArray = new JsonArray();
                jsonArray.Add(_jsonCity);

                string jsonString = jsonArray.ToString();
                var jsonToken = JToken.Parse(jsonString);

                JArray jArray = new JArray();
                jArray.Add(_jsonCity);

                string jString = jArray.ToString();
                var jToken = JToken.Parse(jString);

                bool jsonResult = jsonArray.Contains(_jsonCity);
                bool jResult = jArray.Contains(_jsonCity);

                if (!IsJsonAndJResultEqual(jsonResult, jResult))
                    throw new Exception(StringResources.ContainsNotEqual);

                jsonResult = jsonArray.Contains(_city);
                jResult = jArray.Contains(_city);

                if (!IsJsonAndJResultEqual(jsonResult, jResult))
                    throw new Exception(StringResources.ContainsNotEqual);


                _report.AddPassedTestCase(methodName, description);

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }



        public void VerifyInValidSlashInJObject()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "verify double backslash  ";

            try
            {
                JsonObject jsonObject = new JsonObject();
                jsonObject.AddAttribute(nameof(_city), _city);

                string jsonString = jsonObject.ToString();
                var jsonToken = JToken.Parse(jsonString);

                JObject jObject = new JObject();
                jObject.Add(nameof(_city), _city);

                string jString = jObject.ToString();
                var jToken = JToken.Parse(jString);

                bool jsonResult = jsonObject.GetAttributeValue(nameof(_city)).Value.ToString() == _city;
                bool jResult = jObject[nameof(_city)].ToString() == _city;

                if (!IsJsonAndJResultEqual(jsonResult, jResult))
                    throw new Exception(StringResources.ContainsNotEqual);

                _report.AddPassedTestCase(methodName, description);

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }

        public void VerifyValidSlashInJObject()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "verify double backslash  ";

            try
            {
                JsonObject jsonObject = new JsonObject();
                jsonObject.AddAttribute(nameof(_jsonCity),_jsonCity);

                string jsonString = jsonObject.ToString();
                var jsonToken = JToken.Parse(jsonString);

                JObject jObject = new JObject();
                jObject.Add(nameof(_jsonCity), _jsonCity);

                string jString = jObject.ToString();
                var jToken = JToken.Parse(jString);
                                
                bool jsonResult = jsonObject.GetAttributeValue(nameof(_jsonCity)).Value.ToString() == _jsonCity;
                bool jResult = jObject[nameof(_jsonCity)].ToString() == _jsonCity;

                if (!IsJsonAndJResultEqual(jsonResult, jResult))
                    throw new Exception(StringResources.ContainsNotEqual);

                _report.AddPassedTestCase(methodName, description);

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }



        public void VerifyInValidSlashInJObjectByRemovingAttrribute()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "verify double backslash  ";

            try
            {
                JsonObject jsonObject = new JsonObject();
                jsonObject.AddAttribute(nameof(_city), _city);

                string jsonString = jsonObject.ToString();
                var jsonToken = JToken.Parse(jsonString);

                JObject jObject = new JObject();
                jObject.Add(nameof(_city), _city);

                string jString = jObject.ToString();
                var jToken = JToken.Parse(jString);
               
                bool jsonResult = jsonObject.RemoveAttribute(nameof(_city)); ;
                bool jResult = jObject.Remove(nameof(_city));

                if (!IsJsonAndJResultEqual(jsonResult, jResult))
                    throw new Exception(StringResources.ContainsNotEqual);

                _report.AddPassedTestCase(methodName, description);

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }

        public void VerifyValidSlashInJObjectByRemovingAttrribute()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string description = "verify double backslash  ";

            try
            {
                JsonObject jsonObject = new JsonObject();
                jsonObject.AddAttribute(nameof(_jsonCity), _jsonCity);

                string jsonString = jsonObject.ToString();
                var jsonToken = JToken.Parse(jsonString);

                JObject jObject = new JObject();
                jObject.Add(nameof(_jsonCity), _jsonCity);

                string jString = jObject.ToString();
                var jToken = JToken.Parse(jString);

                bool jsonResult = jsonObject.RemoveAttribute(nameof(_jsonCity)); ;
                bool jResult = jObject.Remove(nameof(_jsonCity));

                if (!IsJsonAndJResultEqual(jsonResult, jResult))
                    throw new Exception(StringResources.ContainsNotEqual);

                _report.AddPassedTestCase(methodName, description);

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }


        }



        #region ----------------------------- Helper ----------------------------- 
        private bool IsJsonAndJResultEqual(bool jsonContainResult, bool jContainResult)
        {
            if (jsonContainResult && jContainResult)
                return true;

            if (!jsonContainResult && !jContainResult)
                return true;

            return false;
        }

        #endregion

    }

    public static class StringResources
    {
        public static string ContainsNotEqual = "Contains is not equal for Newtonsoft and ncache api";

    }
}
