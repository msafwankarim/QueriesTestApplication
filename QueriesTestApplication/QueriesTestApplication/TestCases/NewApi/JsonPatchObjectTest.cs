using Alachisoft.NCache.Client;
using Alachisoft.NCache.Client.Services;
using Alachisoft.NCache.Runtime;
using Alachisoft.NCache.Runtime.Caching;
using Alachisoft.NCache.Runtime.JSON;
using Alachisoft.NCache.Sample.Data;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QueriesTestApplication.TestCases.NewApi
{
    internal class JsonPatchObjectTest : NCacheTestBase
    {
        Product product;

        public JsonPatchObjectTest() 
        {
            product = GetProduct(2);
        }

        [SetUp]
        public void InitCacheData()
        {
            cache.Clear();

            cache.Insert("k1", product);
        }

        [Test]
        public void Test_Partial_Get()
        {
            var obj = cache.JsonPatchService.GetJson<Order>("k1", "Order");

            Assert.IsNotNull(obj);           
        }

        [Test]
        public void Test_Object_AddOperation()
        {
            
            var patch = new JsonPatch().Add("ExtraField", "ExtraValue");

            var version = cache.JsonPatchService.Update("k1", patch);

            Assert.That(version.Version != 0);

            var obj = cache.Get<JsonObject>("k1");

            Assert.IsNotNull(obj);
            Assert.IsTrue(obj.ContainsAttribute("ExtraField"));
            Assert.IsTrue(obj["ExtraField"].Value?.ToString() == "ExtraValue");
        }

        [Test]
        public void Test_Object_PatchTestOperation()
        {

            var item = cache.GetCacheItem("k1");

            var patch = new JsonPatch().Test("Category", "Cat-1");

            var version = cache.JsonPatchService.Update("k1", patch);

            Assert.That(version.Version == item.Version.Version);

            var obj = cache.Get<Product>("k1");

            Assert.That(obj.Id, Is.EqualTo(product.Id));
        }

        [Test]
        public void Test_Object_ReplaceOperation()
        {           
            var patch = new JsonPatch().Replace("Id", 24);

            cache.JsonPatchService.Update("k1", patch);
            var obj = cache.Get<Product>("k1");

            Assert.IsNotNull(obj);
            Assert.IsTrue(obj.Id == 24);
        }

        [Test]
        public void Test_Object_CopyOperation()
        {

            var patch = new JsonPatch().Copy("Name", "ClassName");

            cache.JsonPatchService.Update("k1", patch);
            var obj = cache.Get<Product>("k1");

            Assert.IsNotNull(obj);
            Assert.IsTrue(obj.ClassName == obj.Name);
        }

        [Test]
        public void Test_Object_MoveOperation()
        {
            var patch = new JsonPatch().Move("Name", "ClassName");

            cache.JsonPatchService.Update("k1", patch);
            var obj = cache.Get<Product>("k1");

            Assert.IsNotNull(obj);
            Assert.IsTrue(obj.ClassName == product.Name);
            Assert.IsNull(obj.Name);
        }

        [Test]
        public void Test_Object_MoveOperation_ShouldFail()
        {
            var patch = new JsonPatch().Move("Images/0", "Images/7");

            Assert.Throws(Is.InstanceOf<Exception>().And.Message.Contain("bound"), () => cache.JsonPatchService.Update("k1", patch));
            var obj = cache.Get<Product>("k1");

            Assert.IsNotNull(obj);
            Assert.IsTrue(obj.Images.Length == 2);
            Assert.IsTrue(obj.Images[0].Data.SequenceEqual(product.Images[0].Data));
        }


        [Test]
        public void Test_Object_RemoveOperation()
        {
            var patch = new JsonPatch().Remove("ClassName");

            cache.JsonPatchService.Update("k1", patch);
            var obj = cache.Get<Product>("k1");

            Assert.IsNotNull(obj);
            Assert.IsNull(obj.ClassName);
        }

        [Test]
        public void TestList()
        {
            cache.Clear();
            string key = "x";
            List<int> numbers = new List<int> { 1, 2, 3, 4, 5 };

            cache.Insert(key, numbers);

            IJsonPatch patch = new JsonPatch().Add("3", int.MaxValue);
            cache.JsonPatchService.Update(key, patch);

            var item = cache.Get<List<int>>(key);

            Assert.IsNotNull(item);
            Assert.IsTrue(item.Count == numbers.Count + 1);
            Assert.IsTrue(item[3] == int.MaxValue);

        }

        [Test]
        public void Test_Bulk_UnsetGroup()
        {
            cache.Clear();

            IDictionary<string, CacheItem> items = new Dictionary<string, CacheItem>();

            for(int i = 0; i < 20; i++)
            {
                var citem = new CacheItem("string");
                
                if (i != 0 && i % 5 == 0)
                    citem.Group = "g1";

                items.Add("k-" + i, citem);
            }


            var failed = cache.InsertBulk(items);

            Assert.IsEmpty(failed);

            var item = cache.GetCacheItem("k-5");
            Assert.That(item.Group, Is.EqualTo("g1"));

            IJsonPatch patch = new JsonPatch().UnsetGroup("g1");

            var exceptions = cache.JsonPatchService.UpdateBulk(items.Keys, patch);

            Assert.IsEmpty(exceptions);
            item = cache.GetCacheItem("k-5");

            Assert.IsNull(item.Group);
            
        }

        [Test]
        public void Test_InsertNamedTag_WithNullNamedTagDictionary()
        {
            cache.Clear();

            cache.Insert("k", new CompanyDetails("details", 10, 1));
            NamedTagsDictionary dictionary = new NamedTagsDictionary();
            Tag[] tags = new Tag[0];

            List<string> namedTags = new List<string>();

            IJsonPatch patch = null;
            Assert.Throws<ArgumentException>(() => patch = new JsonPatch().AddNamedTag(dictionary));

            Assert.Throws<ArgumentException>(() => patch = new JsonPatch().AddTag(tags));

            Assert.Throws<ArgumentException>(() => patch = new JsonPatch().RemoveNamedTag(namedTags));

            Assert.Throws<ArgumentException>(() => patch = new JsonPatch().RemoveTag(tags));

            tags = new Tag[1] { new Tag("myTag") };
            dictionary = new NamedTagsDictionary();
            dictionary.Add("key", "named-tag");

            patch = new JsonPatch().AddTag(tags).AddNamedTag(dictionary);

            cache.JsonPatchService.Update("k", patch);

            var echo = cache.GetCacheItem("k");

            Assert.IsNotNull(echo);
            Assert.IsTrue(echo.Tags.Length == 1);
            Assert.IsTrue(echo.NamedTags.Count == 1);

            namedTags = new List<string>() { "key" };
            patch = new JsonPatch().RemoveTag(tags).RemoveNamedTag(namedTags);

            cache.JsonPatchService.Update("k", patch);

            echo = cache.GetCacheItem("k");

            Assert.IsNotNull(echo);
            Assert.IsTrue(echo.Tags == null || echo.Tags.Length == 0);
            Assert.IsTrue(echo.NamedTags == null || echo.NamedTags.Count == 0);


        }

        [Test]
        public void Test_AddNamedTag_Integer()
        {
            cache.Clear();
            string key = "x";
            Product product = GetProduct(3);
            var cacheItem = new CacheItem(product);

            cache.Insert(key, cacheItem);
            NamedTagsDictionary dic = new NamedTagsDictionary();
            dic.Add("NamedTagKey", int.MaxValue);
            IJsonPatch patch = (new JsonPatch()).AddNamedTag(dic);
            cache.JsonPatchService.Update(key, patch);
        }


        [Test]
        public void Test_Array_AddOperation()
        {

            var patch = new JsonPatch().Add("Images/2", JsonConvert.SerializeObject(product.Images[0]));



            cache.JsonPatchService.Update("k1", patch);

            var obj = cache.Get<Product>("k1");

            Assert.IsNotNull(obj);
            Assert.IsTrue(obj.Images.Length == 3);
            Assert.IsTrue(obj.Images[2].Data.SequenceEqual(obj.Images[0].Data));
        }

        [Test]
        public void Test_Array_ReplaceOperation()
        {
            var patch = new JsonPatch().Replace("Images/0", JsonConvert.SerializeObject(product.Images[1]));

            cache.JsonPatchService.Update("k1", patch);
            var obj = cache.Get<Product>("k1");

            Assert.IsNotNull(obj);
            Assert.IsTrue(obj.Images.Length == 2);
            Assert.IsTrue(obj.Images[1].Data.SequenceEqual(obj.Images[0].Data));
        }

        [Test]
        public void Test_Array_CopyOperation()
        {

            var patch = new JsonPatch().Copy("Images/1", "Images/0");

            cache.JsonPatchService.Update("k1", patch);
            var obj = cache.Get<Product>("k1");

            Assert.IsNotNull(obj);
            Assert.IsTrue(obj.Images.Length == 3);
            Assert.IsTrue(obj.Images[2].Data.SequenceEqual(obj.Images[0].Data));
        }

        [Test]
        public void Test_Array_MoveOperation()
        {
            var patch = new JsonPatch().Move("Images/0", "Images/1");

            cache.JsonPatchService.Update("k1", patch);
            var obj = cache.Get<Product>("k1");

            Assert.IsNotNull(obj);
            Assert.IsTrue(obj.Images.Length == 2);
            Assert.IsTrue(obj.Images[1].Data.SequenceEqual(product.Images[0].Data));
        }

        [Test]
        public void Test_Array_MoveOperation_ShouldFail()
        {
            var patch = new JsonPatch().Move("Images/0", "Images/7");

            Assert.Throws(Is.InstanceOf<Exception>().And.Message.Contain("bound"), () => cache.JsonPatchService.Update("k1", patch));
            var obj = cache.Get<Product>("k1");

            Assert.IsNotNull(obj);
            Assert.IsTrue(obj.Images.Length == 2);
            Assert.IsTrue(obj.Images[0].Data.SequenceEqual(product.Images[0].Data));
        }


        [Test]
        public void Test_Array_RemoveOperation()
        {
            var patch = new JsonPatch().Remove("ClassName");

            cache.JsonPatchService.Update("k1", patch);
            var obj = cache.Get<Product>("k1");

            Assert.IsNotNull(obj);
            Assert.IsNull(obj.ClassName);
        }

        [Test]
        public void Test_Object_LateAdd()
        {
            cache.Clear();

            var product = GetProduct(2);

            var order = product.Order;
            product.Order = null;

            cache.Insert("k1", product);

            IJsonPatch patch = new JsonPatch().Add("Order", JsonConvert.SerializeObject(order));

            cache.JsonPatchService.Update("k1", patch);

            var item = cache.GetCacheItem("k1");

            Assert.IsNotNull(item);

            var value = item.GetValue<Product>();

            Assert.IsNotNull(item);
            Assert.IsNotNull(value.Order);
            Assert.That(value.Order.OrderID, Is.EqualTo(order.OrderID));
        }
    }
}
