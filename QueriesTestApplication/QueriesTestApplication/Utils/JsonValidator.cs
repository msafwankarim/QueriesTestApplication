using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Data = Alachisoft.NCache.Sample.Data;
using Helper = QueriesTestApplication.Utils;

namespace QueriesTestApplication.Utils
{
    public static class JsonHelper
    {   
        static JsonSerializerSettings _serializerSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };

        public static void TestArrayWithIndexing()
        {
            try
            {
                _serializerSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };
                string segment = "0";         

                Data.Image[] array = new Data.Image[2] { new Data.Image(), new Data.Image() };

                string serializedTarget = JsonConvert.SerializeObject(array, _serializerSettings);
                JObject target = JsonConvert.DeserializeObject<JObject>(serializedTarget,_serializerSettings);

                if (target["$type"].ToString().Contains("[]"))                  
                {
                    var  t1 = JsonConvert.DeserializeObject<JArray>(target["$values"].ToString(), _serializerSettings);
                }




                TraverseJsonArray(target,segment);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }



        }

        public static object TraverseJsonArray(object target, string segment)
        {
            if (string.IsNullOrWhiteSpace(segment))
                return target;
            JArray arrayToken = target as JArray;


            int index = ValidateAndNormailzeIndex(arrayToken, segment);

            return arrayToken[index];
        }

        private static int ValidateAndNormailzeIndex(JArray target, string segment)
        {
            int index;
            if (segment.Equals("-", StringComparison.InvariantCultureIgnoreCase))
                index = target.Count;
            else if (!int.TryParse(segment, out index))
                Console.WriteLine("Failed");

            if (0 < index && index > target.Count)
                throw new IndexOutOfRangeException();
            return index;
        }



    }
}
