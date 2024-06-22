using Alachisoft.NCache.Client;
using Alachisoft.NCache.Runtime;
using Alachisoft.NCache.Runtime.Caching;
using Alachisoft.NCache.Runtime.JSON;
using Alachisoft.NCache.Sample.Data;
using NUnit.Framework;
using NUnit.Framework.Internal;

using QueriesTestApplication.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace QueriesTestApplication
{
    [TestFixture]
    public class LockingTests
    {
        private static ICache cache;

        static LockingTests()
        {
            //cache = CacheManager.GetCache(ConfigurationManager.AppSettings["cache"] ?? "demoCache");
        }

        [Test]
        public void Test_OptimisticLockingJsonPatchPositive()
        {
            cache.Clear();

            Product product = Helper.GetProduct();

            var version = cache.Insert("k1", product);

            var updatePortion = "'updatedName'";

            JsonPatch patch = new JsonPatch();
            patch.Add("Name", JsonValueBase.Parse(updatePortion));            
            cache.JsonPatchService.Update("k1", patch, version);

            var echo = cache.Get<Product>("k1");

            Assert.That(echo.Name, Is.EqualTo("updatedName"));
        }

        public void Test_OptimisticLockingJsonPatchNegative()
        {
            cache.Clear();


            Product product = Helper.GetProduct();

            var version = cache.Insert("k1", product);

            var updatePortion = "'updatedName'";

            JsonPatch patch = new JsonPatch();
            patch.Add("Name", JsonValueBase.Parse(updatePortion));

            var randomVersion = cache.Insert("k2", "Dummy");

            cache.JsonPatchService.Update("k1", patch, randomVersion);

            var echo = cache.Get<Product>("k1");
        }

        public void Test_PessimisticLockingPositive()
        {
            cache.Clear();

            Product product = Helper.GetProduct();

            var version = cache.Insert("k1", product);

            LockHandle lockHandle;

            var lockAcquired = cache.Lock("k1", TimeSpan.FromSeconds(60), out lockHandle);

            var updatePortion = "'updatedName'";

            JsonPatch patch = new JsonPatch();
            patch.Add("Name", JsonValueBase.Parse(updatePortion));

            cache.JsonPatchService.Update("k1", patch, null, lockHandle);

            var echo = cache.Get<Product>("k1");

        }

        public void Test_PessimisticLockingNegative()
        {
            cache.Clear();

            Product product = Helper.GetProduct();

            var version = cache.Insert("k1", product);

            LockHandle lockHandle;

            cache.Insert("k2", "Dummy");
            var lockAcquired1 = cache.Lock("k1", TimeSpan.FromSeconds(60), out lockHandle);
            var lockAcquired = cache.Lock("k2", TimeSpan.FromSeconds(60), out lockHandle);

            var updatePortion = "'updatedName'";

            JsonPatch patch = new JsonPatch();                    

            patch.Add("Name", JsonValueBase.Parse(updatePortion));

            patch.Replace("Name", "Mobile");

            patch.Test("Order.OrderID", "10");

            patch.Move("ClassName", "Category");

            patch.Copy("Images[1]", "Images[2]");

            patch.Remove("Images[*]");

            patch.RemoveTag(new Tag("sale"));

            patch.SetGroup("MyGroup");

            patch.AddTag(new Tag("discounted"));
            patch.AddTag(new Tag[] { new Tag("sale") });

            NamedTagsDictionary namedTags = new NamedTagsDictionary();
            namedTags.Add("discounted", true);
            patch.AddNamedTag(namedTags);            

            cache.JsonPatchService.Update("k1", jsonPatch: patch, writeThruOptions: null, lockHandle);

            var echo = cache.Get<Product>("k1");

        }

        [Test]
        public void Test_UpdateQueryBasic()
        {
            var query = new QueryCommand("UPDATE Alachisoft.NCache.Sample.Data.Product SET Name = '\"Tea\"' where Id = ?");
            query.Parameters.Add("Id", 1);
            var rowsEffected = cache.SearchService.ExecuteNonQuery(query);
        }

        [Test]
        public void Test_OperationsDuringStateQueries()
        {
            int count = 2000;
            cache.Clear();

            for (int i = 0; i < count; i++)
            {
                Product product = Helper.GetProduct();
                product.Id = i;

                cache.Insert("key" + i, product);                
            }

            for (int i = 0; i < count; i++)
            {
                var query = new QueryCommand("UPDATE Alachisoft.NCache.Sample.Data.Product SET Name = '\"Tea\"' where Id = ?");
                query.Parameters.Add("Id", i);

                IDictionary<string, Exception> errors = new Dictionary<string,Exception>();
                var rows = cache.SearchService.ExecuteNonQuery(query);
                
                if(errors != null && errors.Count > 0)
                {
                    foreach(var error in errors)
                    {
                        Console.WriteLine(error.Value);
                    }
                }

                Thread.Sleep(50);
            }
            int failCounter = 0;
            for (int i = 0; i < count; i++)
            {
                var prod = cache.Get<Product>("key" + i);
                try
                {
                    Assert.That(prod.Name, Is.EqualTo("Tea"));
                }
                catch (AssertionException)
                {
                    failCounter++;
                    Console.WriteLine("Name not updated for " + prod.Id);
                }
            }

            Assert.That(failCounter, Is.EqualTo(0), $"{failCounter} operations failed");
        }

        [Test]
        public void Test_OperationsDuringStatePatch()
        {
            int count = 2000;
            cache.Clear();

            for (int i = 0; i < count; i++)
            {
                Product product = Helper.GetProduct();
                product.Id = i;

                cache.Insert("key" + i, product);
            }

            for (int i = 0; i < count; i++)
            {
                IJsonPatch patch = new JsonPatch().Replace("Name", "Tea");

                var version = cache.JsonPatchService.Update($"key{i}", patch);

                Thread.Sleep(50);
            }
            int failCounter = 0;
            for (int i = 0; i < count; i++)
            {
                var prod = cache.Get<Product>("key" + i);
                try
                {
                    Assert.That(prod.Name, Is.EqualTo("Tea"));
                }
                catch (AssertionException)
                {
                    failCounter++;
                    Console.WriteLine("Name not updated for " + prod.Id);
                }
            }

            Assert.That(failCounter, Is.EqualTo(0), $"{failCounter} operations failed");
        }

        [OneTimeSetUp]
        public void EnsureCacheConnected()
        {
            if (cache == null)
                cache = CacheManager.GetCache(Common.CacheName);
        }

        [OneTimeTearDown]
        public void Dispose()
        {
            cache?.Dispose();
        }

        //[TestMethod]
        //[DynamicData(nameof(CacheNames))]
        public void Test_BridgeReplication(string primaryCacheName, string secondaryCacheName)
        {
            int count = 2;

            ICache primaryCache = CacheManager.GetCache(primaryCacheName);
            ICache secondaryCache = CacheManager.GetCache(secondaryCacheName);

            primaryCache.Clear();
            secondaryCache.Clear();

            Thread.Sleep(1000);

            for (int i = 0; i < count; i++)
            {
                Product product = Helper.GetProduct();
                product.Id = i;

                primaryCache.Insert("key" + i, product);
                Thread.Sleep(100);
            }

            for (int i = 0; i < count; i++)
            {
                var query = new QueryCommand("UPDATE Alachisoft.NCache.Sample.Data.Product SET Name = '\"Tea\"' where Id = ?");
                query.Parameters.Add("Id", i);

                var rows = primaryCache.SearchService.ExecuteNonQuery(query);

                Console.WriteLine(rows + " rows updated");

                Thread.Sleep(400);
            }

            Thread.Sleep(1000);

            

            for (int i = 0; i < count; i++)
            {
                var prod = primaryCache.Get<Product>("key" + i);
                var replica = secondaryCache.Get<Product>("key" + i);

                //Assert.AreEqual("Tea", prod.Name);
                //Assert.AreEqual("Tea", replica.Name);
            }
        }
    }
}
