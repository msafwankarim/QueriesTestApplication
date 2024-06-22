using Alachisoft.NCache.Client;
using Alachisoft.NCache.Parser;
using Alachisoft.NCache.Runtime;
using Alachisoft.NCache.Runtime.Caching;
using Alachisoft.NCache.Runtime.JSON;
using Alachisoft.NCache.Sample.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using QueriesTestApplication.Utils;
using System.Collections.Generic;

namespace QueriesTestApplication.TestCases.Patch
{
    internal class BackingSourceTests : NCacheTestBase
    {
        private readonly WriteThruOptions WRITE_THRU_OPTIONS = new WriteThruOptions(WriteMode.WriteThru, "write-thru");
        private readonly WriteThruOptions WRITE_BEHIND_OPTIONS = new WriteThruOptions(WriteMode.WriteBehind, "write-thru");

        [Test]
        public void TestJson()
        {
            var x = GetProduct(2);

            var x2 = new ExtendedProduct(23, "Phone", "Comm");

            var y = JsonConvert.SerializeObject(x2, new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.All,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            });

            var jObject = JsonConvert.DeserializeObject<JObject>(y);

            jObject["ProductName"] = "Greater Pakistan";

            y = JsonConvert.SerializeObject(jObject);

            var h = JsonConvert.DeserializeObject<Product>(y);

            //Assert.That(h.ProductName, Is.EqualTo("Greater Pakistan"));

            var t = JsonValueBase.Parse(@"{ 'dependency': [
                                               {
                                                 'file': {'fileNames' : ['E:\\temp_files\\dependencyFile.txt']}
                                               }
                                            ]
                                           }");
        }

        [Test] 
        public void AggregateSelect()
        {
            cache.Clear();

            cache.Insert("k1", GetProduct(1));
            cache.Insert("k2", GetProduct(2));
            cache.Insert("k3", GetProduct(3));
            cache.Insert("k4", GetProduct(4));

            var command = new QueryCommand($"SELECT SUM(Id) FROM {typeof(Product).FullName} WHERE Id <= 10");
            var reader = cache.QueryService.ExecuteReader(command, false);

            while (reader.Read())
            {
                Assert.That(reader.GetInt32(0), Is.EqualTo(10));
            }
        }

        [Test]
        public void SelectQuery()
        {
            cache.Clear();

            cache.Insert("k1", GetProduct(1));
            cache.Insert("k2", GetProduct(2));
            cache.Insert("k3", GetProduct(3));
            cache.Insert("k4", GetProduct(4));

            ICollection<string> expectedKeys = new List<string>()
            {
                "k1", "k2", "k3", "k4"
            };
            ICollection<string> keys = new List<string>();

            var reader = cache.QueryService.ExecuteReader(new QueryCommand($"SELECT $VALUE$ FROM {typeof(Product).FullName}"), false);

            while(reader.Read())
            {
                Assert.That(expectedKeys.Contains(reader.GetString(0)));
            }            
        }

        [Test]
        public void WriteThru()
        {
            cache.Clear();

            //var item = new CacheItem(GetProduct(2));            

            //item.Priority = CacheItemPriority.High;
            //item.NamedTags = new NamedTagsDictionary();
            //item.NamedTags.Add("productType", "phone");
            //item.Tags = new Tag[] { new Tag("communication") };            

            cache.Insert("prod1", GetProduct(2));   
            var echo2 = cache.Get<JsonObject>("prod1");

            //var x = 


            IJsonPatch patch = new Alachisoft.NCache.Client.JsonPatch()
            //.Test("ProductName", "NewProduct");
            .Test("ProductName", "NCache-1")
            .Replace("ProductName", "IPhone");
            //.Test("_name", "IPhone");

            var result = cache.JsonPatchService.Update("prod1", patch);

            var echo1 = cache.Get<JsonObject>("prod1");
            var echo = cache.Get<Product>("prod1");

            Assert.IsNotNull(echo);
            //Assert.That(echo.ProductName, Is.EqualTo("NewProduct"));
        }

        [Test]
        public void InsertEmptyGroup()
        {
            cache.Clear();
            var query = new QueryCommand("INSERT INTO System.String (Key, Value, Meta) VALUES ('k1', 'v1', @meta)");
            query.Parameters.Add("@meta", "{'group': ['g1']}");

            Assert.Throws<ParserException>(() => cache.QueryService.ExecuteNonQuery(query));

            query.Parameters.Clear();

            var json = new JsonObject();
            json.AddAttribute("Group", "g1");

            query.Parameters.Add("@meta", json);

            cache.QueryService.ExecuteNonQuery(query);

            json = new JsonObject();
            json.AddAttribute("group", new JsonArray("['g1']"));

            query.Parameters.Clear();
            query.Parameters.Add("@meta", json);

            Assert.Throws<ParserException>(() => cache.QueryService.ExecuteNonQuery(query));
        }

        [Test]
        public void WriteThruUpdateQuerySingle()
        {
            cache.Clear();

            var item = new CacheItem(GetProduct(2));

            item.Priority = CacheItemPriority.High;
            item.NamedTags = new NamedTagsDictionary();
            item.NamedTags.Add("productType", "phone");
            item.Tags = new Tag[] { new Alachisoft.NCache.Runtime.Caching.Tag("communication") };

            cache.Insert("prod1", item);
            cache.Insert("prod2", Helper.GetProduct());

            string query = @$"
                UPDATE {typeof(Product).FullName} 
                SET this.ClassName = '""Phone""'
                WHERE Id = 2 OR Id = 1
            "
            .Trim();

            var queryCommand = new QueryCommand(query);
            var rowsEffected = cache.SearchService.ExecuteNonQuery(queryCommand);

            Assert.That(rowsEffected, Is.EqualTo(2));

            var echo = cache.Get<Product>("prod1");

            Assert.IsNotNull(echo);
            Assert.That(echo.ClassName, Is.EqualTo("Phone"));
        }

        
        [Test]
        public void WriteThruUpdateQueryBulk()
        {
            int count = 500;
            cache.Clear();

            for (int i = 0; i < count; i++)
            {
                var item = new CacheItem(GetProduct(i));

                item.Priority = CacheItemPriority.High;
                item.NamedTags = new NamedTagsDictionary();
                item.NamedTags.Add("productType", "phone");
                item.Tags = new Tag[] { new Tag("communication") };

                cache.Insert($"prod{i}", item);
            }

            for (int i = 0; i < count; i++)
            {

                string query = @$"
                    UPDATE {typeof(Product).FullName} 
                    SET this.ClassName = '""Phone""'
                    WHERE Id = {i}
                ".Trim();

                var queryCommand = new QueryCommand(query);
                var rowsEffected = cache.SearchService.ExecuteNonQuery(queryCommand);

                Assert.That(rowsEffected, Is.EqualTo(1));
                var echo = cache.Get<Product>($"prod{i}");

                Assert.IsNotNull(echo);
                Assert.That(echo.ClassName, Is.EqualTo("Phone"));
            }

        }

        [Test]
        public void WriteThruDeleteQuerySingle()
        {
            cache.Clear();
            
            var item = new CacheItem(GetProduct(2));

            item.Priority = CacheItemPriority.High;
            item.NamedTags = new NamedTagsDictionary();
            item.NamedTags.Add("productType", "phone");
            item.Tags = new Tag[] { new Alachisoft.NCache.Runtime.Caching.Tag("communication") };

            cache.Insert("prod1", item);
            cache.Insert("prod2", Helper.GetProduct());


            string query = @$"
                DELETE FROM {typeof(Product).FullName}                 
                WHERE Id = 2
            "
            .Trim();

            var queryCommand = new QueryCommand(query);
            var rowsEffected = cache.QueryService.ExecuteNonQuery(queryCommand);

            Assert.That(rowsEffected, Is.EqualTo(1));

            var echo = cache.Get<Product>("prod1");

            Assert.IsNull(echo);
        }

    }
}
