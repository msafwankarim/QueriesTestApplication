using Alachisoft.NCache.Client;
using Alachisoft.NCache.Runtime.Caching;
using Alachisoft.NCache.Runtime.Exceptions;
using Alachisoft.NCache.Runtime.JSON;
using Alachisoft.NCache.Sample.Data;
using NUnit.Framework;
using Quartz.Util;
using System;
using System.Linq;
using System.Threading;

namespace QueriesTestApplication.TestCases.Patch
{
    [TestFixture(Category = "JsonPatch")]
    public class JsonPatchTest : NCacheTestBase
    {

        [Test]
        public void TestNamedTagWithBasicInsertAPI()
        {
            cache.Clear();
            var json = JsonValueBase.Parse("{\"name\":\"User\",\"id\":25}") as JsonObject;

            var x = new CacheItem(json);
            
            x.NamedTags = new NamedTagsDictionary();
            x.NamedTags.Add("category", "x1");
            
            cache.Insert("k1", x);

            x = new CacheItem("v2");
            
            x.NamedTags = new NamedTagsDictionary();
            x.NamedTags.Add("category", "x2");
            
            cache.Insert("k2", x);

            var queryCommand = new QueryCommand("select * FROM System.String WHERE category = 'x2'");
            var reader = cache.QueryService.ExecuteReader(queryCommand, false);

            while(reader.Read())
            {
                Assert.That(reader.GetString(0), Is.EqualTo("k2"));
            }
        }

        [Test]
        public void TestNamedTag()
        {
            var json = JsonValueBase.Parse("{\"name\":\"User\",\"id\":25}") as JsonObject;

            json.Type = typeof(Product).FullName;

            cache.Insert("k1", json);

            var namedTag = new NamedTagsDictionary();
            namedTag.Add("gender", "male");

            var patch = new JsonPatch()
                .AddNamedTag(namedTag);

            patch.TypeName = "NCache.User";

            cache.JsonPatchService.Update("k1", patch);

            var item = cache.GetCacheItem("k1");
            var value = item.GetValue<object>();

            Console.WriteLine(value);

            Assert.IsNotNull(item);
            Assert.That(item.NamedTags.Contains("gender"));

            var queryCommand = new QueryCommand("SELECT * FROM NCache.User WHERE gender = 'male'");
            var reader = cache.QueryService.ExecuteReader(queryCommand, false);
            
            while (reader.Read())
            {
                Assert.That(reader.GetString(0), Is.EqualTo("k1"));
            }
        }

        [Test]
        public void TestNamedTagOverwrite()
        {
            cache.Clear();
            var json = JsonValueBase.Parse("{\"name\":\"User\",\"id\":25}") as JsonObject;

            json.Type = typeof(Product).FullName;

            cache.Insert("k1", json);

            var namedTag = new NamedTagsDictionary();
            namedTag.Add("gender", "male");

            var patch = new JsonPatch()
                .AddNamedTag(namedTag);


            cache.JsonPatchService.Update("k1", patch);

            var item = cache.GetCacheItem("k1");
            var value = item.GetValue<object>();

            Console.WriteLine(value);

            Assert.IsNotNull(item);
            Assert.That(item.NamedTags.Contains("gender"));

            var queryCommand = new QueryCommand($"SELECT * FROM {typeof(Product).FullName} WHERE gender = 'male'");
            var reader = cache.QueryService.ExecuteReader(queryCommand, false);

            while (reader.Read())
            {
                Assert.That(reader.GetString(0), Is.EqualTo("k1"));
            }

            namedTag = new NamedTagsDictionary();
            namedTag.Add("additional", true);

            patch = new JsonPatch()
                .AddNamedTag(namedTag);

            patch.TypeName = "NCache.User";
            
            cache.JsonPatchService.Update("k1", patch);

            queryCommand = new QueryCommand($"SELECT * FROM NCache.User WHERE additional = true");
            reader = cache.QueryService.ExecuteReader(queryCommand, false);

            while (reader.Read())
            {
                Console.WriteLine(reader.GetString(0));
            }

            bool v = new Product().ProductName.IsNullOrWhiteSpace();
        }


        [Test]
        public void TestEvents()
        {
            cache.Clear();

            bool eventRecieved = false;

            var item = new CacheItem(GetProduct(3));
            item.SetCacheDataNotification((cacheName, args) =>
            {
                eventRecieved = true;
            }, Alachisoft.NCache.Runtime.Events.EventType.ItemUpdated, Alachisoft.NCache.Runtime.Events.EventDataFilter.DataWithMetadata);

            cache.Insert("p1", item);

            var patch = new JsonPatch().Replace("Name", "product-name");

            cache.JsonPatchService.Update("p1", patch);
            //cache.Insert("p1", "Hello");

            Thread.Sleep(TimeSpan.FromSeconds(5));

            Assert.IsTrue(eventRecieved);
        }


        [Test]
        public void Test_DataStrucutresNotSupported()
        {
            cache.Clear();

            var list = cache.DataStructureService.CreateList<string>("myList");
            list.Add("20");
            list.Add("32");

            var patch = new JsonPatch().SetGroup("g1");

            Assert.Throws<OperationFailedException>(() => cache.JsonPatchService.Update("myList", patch));
        }

        [Test]
        public void Test_ArrayListUpdate()
        {
            cache.Clear();

            CompanyDetails details = new CompanyDetails("Alachisoft", 60, 24);            
            details.Add("Asif");
            details.Add("Jahangir");
            details.Add("Imam");

            cache.Insert("c24", details);

            var patch = new JsonPatch()
                .Replace("Employees/2", "Imam ul Haq")
                .Replace("EmployeeList/2", "Imam ul Haq")
                .Replace("EmployeePairs/2", "Imam ul Haq")
                .Replace("EmployeeQueue/2", "Imam ul Haq");


            var echoJson = cache.Get<JsonObject>("c24");

            cache.JsonPatchService.Update("c24", patch);

            var echo = cache.Get<CompanyDetails>("c24");

            Assert.IsNotNull(echo);
            
            Assert.That(echo.Employees[2], Is.EqualTo("Imam ul Haq"));
            Assert.That(echo.EmployeeList[2], Is.EqualTo("Imam ul Haq"));
            Assert.That(echo.EmployeePairs[2], Is.EqualTo("Imam ul Haq"));
            Assert.That(echo.EmployeeQueue.Contains("Imam ul Haq"));
        }

        [Test]
        public void Test_CopyOperation()
        {
            cache.Clear();

            CompanyDetails details = new CompanyDetails("Alachisoft", 60, 24);

            cache.Insert("k1", details);

            var patch = new JsonPatch()
                .Copy("Name", "SID");

            cache.JsonPatchService.Update("k1", patch);

            var echo = cache.Get<object>("k1");

            Assert.IsNotNull(echo);            
        }
    }
}
