using Alachisoft.NCache.Client;
using Alachisoft.NCache.Runtime.Caching;
using Alachisoft.NCache.Runtime.Dependencies;
using Alachisoft.NCache.Sample.Data;
using fastJSON;
using Jil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using QueriesTestApplication.Providers;
using Sigil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;

namespace QueriesTestApplication.TestCases.General
{
    internal class GeneralTests : NCacheTestBase
    {
        [Test]
        public void TestExtractToDirectory()
        {
            var dest = "E:\\zips\\extracted";
            string source = "E:\\zips\\config.zip";

            //Directory.Delete(dest, true);       
            
            ZipFile.ExtractToDirectory(source, dest);


        }
        
        [Test]
        public void TestJsonSerialization()
        {
            int count = 1000_000;

            var items = new List<string>();

            for (int i = 0; i < count; i++)
                items.Add(JsonConvert.SerializeObject(GetProduct(i), new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All }));

            Console.WriteLine("Item Count = " + count);
            string prompt = "Time spent on {0} = {1}ms";

            var x = GetProduct(10);
            var y = JsonConvert.SerializeObject(x, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });

            Stopwatch stopwatch1 = Stopwatch.StartNew();
            for (int i = 0; i < count; i++)
            {
                y = items[i];
                var x2 = JToken.Parse(y);
            }
            stopwatch1.Stop();

            Console.WriteLine(prompt, "NewtonSoft JToken.Parse", stopwatch1.ElapsedMilliseconds);

            var stopwatch2 = Stopwatch.StartNew();
            object x3 = null;
            for (int i = 0; i < count; i++)
            {
                y = items[i];
                x3 = JsonNode.Parse(y);
            }            
            stopwatch2.Stop();
            Console.WriteLine(prompt, "System.Text.Json JsonNode.Parse", stopwatch2.ElapsedMilliseconds);

            var stopwatch3 = Stopwatch.StartNew();
            object x4 = null;
            for (int i = 0; i < count; i++)
            {
                y = items[i];
                x4 = Jil.JSON.Deserialize<object>(y);
            }
            stopwatch3.Stop();
            Console.WriteLine(prompt, "Jil.JSON.Deserialize<object>", stopwatch3.ElapsedMilliseconds);

            var stopwatch4 = Stopwatch.StartNew();
            object x5 = null;
            for (int i = 0; i < count; i++)
            {
                y = items[i];
                x5 = fastJSON.JSON.Parse(y);
            }
            stopwatch4.Stop();
            Console.WriteLine(prompt, "fastJSON.JSON.Parse>", stopwatch4.ElapsedMilliseconds);

            //var stopwatch5 = Stopwatch.StartNew();
            //object x6 = null;
            //for (int i = 0; i < count; i++)
            //{
            //    x6 = ServiceStack.JSON.parse(y);
            //}
            //stopwatch5.Stop();
            //Console.WriteLine(prompt, "ServiceStack.JSON.parse", stopwatch5.ElapsedMilliseconds);

            //var stopwatch6 = Stopwatch.StartNew();
            //object x7 = null;
            //for (int i = 0; i < count; i++)
            //{
            //    x7 = ServiceStack.JS.ParseObject(y);
            //}
            //stopwatch6.Stop();
            //Console.WriteLine(prompt, "ServiceStack.JS.ParseObject", stopwatch6.ElapsedMilliseconds);

            var stopwatch7 = Stopwatch.StartNew();
            object x8 = null;
            for (int i = 0; i < count; i++)
            {
                y = items[i];
                x8 = Utf8Json.JsonSerializer.Deserialize<object>(y);
            }
            stopwatch7.Stop();
            Console.WriteLine(prompt, "Utf8Json", stopwatch7.ElapsedMilliseconds);

            var stopwatch8 = Stopwatch.StartNew();
            object x9 = null;
            for (int i = 0; i < count; i++)
            {
                y = items[i];
                x9 = SpanJson.JsonSerializer.Generic.Utf8.Deserialize<object>(Encoding.UTF8.GetBytes(y));
            }
            stopwatch8.Stop();
            Console.WriteLine(prompt, "SpanJson", stopwatch8.ElapsedMilliseconds);

            var stopwatch9 = Stopwatch.StartNew();
            object x10 = null;
            for (int i = 0; i < count; i++)
            {
                y = items[i];
                x10 = TinyJson.JSONParser.FromJson<object>(y);
            }
            stopwatch8.Stop();
            Console.WriteLine(prompt, "TinyJson", stopwatch9.ElapsedMilliseconds);

        }

        [Test]
        public void TestStringReplace()
        {
            string prefix = "$StaticNamedTagAttribute$";
            string key = prefix + "Id";
            Random random = new Random();

            int iterations = 10_00_000;
            int outerIterations = 10;

            for (int j = 0; j < outerIterations; j++)
            {
                Stopwatch watch = Stopwatch.StartNew();
                for (int i = 0; i < iterations; i++)
                {
                    if (key.StartsWith(prefix))
                    {
                        //var test = key.Substring(prefix.Length, key.Length - prefix.Length);
                        var test = key.Replace(prefix, "");
                    }

                }
                watch.Stop();
                Console.WriteLine("time = " + watch.Elapsed.TotalMinutes);
            }            

            Console.WriteLine("-------");

            for (int j = 0; j < outerIterations; j++)
            {
                Stopwatch watch = Stopwatch.StartNew();
                for (int i = 0; i < iterations; i++)
                {
                    key = prefix + "id" + i;
                    if (key.StartsWith(prefix))
                    {
                        var test = key.Replace(prefix, "");
                    }
                }
                watch.Stop();
                Console.WriteLine("time = " + watch.Elapsed.TotalMinutes);
            }

            Console.WriteLine("-------");

            for (int j = 0; j < outerIterations; j++)
            {
                Stopwatch watch = Stopwatch.StartNew();
                for (int i = 0; i < iterations; i++)
                {
                    key = "$StaticNamedTagAttribute$id" + i;
                    var span = key.AsSpan();
                    if (MemoryExtensions.Equals(span.Slice(0, prefix.Length), prefix, StringComparison.Ordinal))
                    {
                        var test = span.Slice(prefix.Length, key.Length - prefix.Length).ToString();
                    }
                }
                watch.Stop();
                Console.WriteLine("time = " + watch.Elapsed.TotalMinutes);
            }

            Console.WriteLine("-------");

            for (int j = 0; j < outerIterations; j++)
            {
                Stopwatch watch = Stopwatch.StartNew();
                for (int i = 0; i < iterations; i++)
                {
                    key = prefix + "id" + i;

                    if (FastStartsWith(key, prefix))
                    {
                        var test = key.AsSpan().Slice(prefix.Length, key.Length - prefix.Length).ToString();
                    }
                }
                watch.Stop();
                Console.WriteLine("time = " + watch.Elapsed.TotalMinutes);
            }

            Console.WriteLine("-------");

            for (int j = 0; j < outerIterations; j++)
            {
                Stopwatch watch = Stopwatch.StartNew();
                for (int i = 0; i < iterations; i++)
                {
                    key = prefix + "id" + i;
                    var span = key.AsSpan();
                    if (span.StartsWith(prefix.AsSpan()))
                    {
                        var test = span.Slice(prefix.Length, key.Length - prefix.Length).ToString();
                    }
                }
                watch.Stop();
                Console.WriteLine("time = " + watch.Elapsed.TotalMinutes);
            }

        }

        private bool FastStartsWith(string key, string prefix)
        {

            if (prefix.Length > key.Length)
                return false;

            var keySpan = key.AsSpan().Slice(0, prefix.Length);
            var prefixSpan = prefix.AsSpan();

            for (int i = 0; i < prefix.Length / 2; i++)
            {
                if (!(keySpan[i] == prefixSpan[i] && keySpan[(prefix.Length - i) - 1] == prefix[(prefix.Length - i) - 1]))
                    return false;
            }

            return true;
        }

        [Test]
        public void PollingDependencyTest()
        {
            cache.Clear();

            var key = "1";
            var connectionString = "Server=20.200.20.21,1433\\SQLEXPRESS;Database=northwind;User Id=safwan_karim;Password=4Islamabad;";
            var dboKey = key + ":dbo.Products";

            var item = new CacheItem("v1")
            {
                Dependency = DBDependencyFactory.CreateSqlCacheDependency(connectionString, dboKey)
            };

            cache.Insert(key, item);
            cache.Insert(key, item);

        }


        [Test]
        public void TestStateTransfer()
        {
            int items = 500;

            for(int i = 0; i < items; i++)
            {
                var query = new QueryCommand($"INSERT INTO System.String (Key, Value) VALUES ('k{i}', '{i}')");
                cache.QueryService.ExecuteNonQuery(query);
                query = new QueryCommand($"UPSERT INTO System.String (Key, Value) VALUES ('k{i}', '{i}')");
                cache.QueryService.ExecuteNonQuery(query);

                cache.Add(i.ToString(), i);

                cache.Insert("insert" + i, i);
                Thread.Sleep(200);
            }
        }

        [Test]
        public void PersistenceTest()
        {
            cache.Clear();

            cache.Insert("k1", GetSmallProduct(2));

        }


        [Test]
        public void GetCacheItem()
        {
            var item = cache.Get<SmallProduct>("k1");

            Assert.That(item.Id, Is.EqualTo(2));
            Assert.That(item.Name, Is.EqualTo("Phone"));
        }

        [Test]
        public void CacheItem_MultipleTags()
        {
            cache.Clear();

            var item = new CacheItem("test")
            {
                Tags = new Tag[] { new Tag("t1"), new Tag("t1") }
            };

            cache.Insert("k1", item); ;

            var echo = cache.GetCacheItem("k1");

            Assert.IsTrue(echo.Tags?.Length > 0);

        }

        [Test]
        public void ReadThru_ServerSerialization()
        {
            var keys = Enumerable.Range(0, 3000).Select(i => new CacheItem("key-" + i)).ToDictionary(item => item.GetValue<string>());

            cache.InsertBulk(keys);

        }


        [Test]
        public void InprocCache()
        {
            var cache = CacheManager.GetCache("demoCache");

            var item = cache.Insert("k1", GetProduct(1));
        }

        [Test]
        public void GetItem()
        {
            var cache = CacheManager.GetCache("demoCache");
            var echo = cache.Get<Product>("k1");
        }
    }
}
