using Alachisoft.NCache.Caching.Statistics;
using Alachisoft.NCache.Client;
using Alachisoft.NCache.Runtime;
using Alachisoft.NCache.Runtime.CacheManagement;
using Alachisoft.NCache.Runtime.Caching;
using Alachisoft.NCache.Runtime.Exceptions;
using Alachisoft.NCache.Runtime.JSON;
using Alachisoft.NCache.Runtime.Serialization;
using Alachisoft.NCache.Runtime.Serialization.IO;
using Alachisoft.NCache.Sample.Data;
using Alachisoft.NCache.Serialization.Formatters;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QueriesTestApplication.TestCases.NewApi
{
    public class CompactClass : ICompactSerializable
    {
        BucketStatistics statistics = new BucketStatistics();
        public void Deserialize(CompactReader reader)
        {
            reader.SkipObject();
            reader.SkipObject();
        }

        public void Serialize(CompactWriter writer)
        {
            writer.WriteObject(statistics);
            writer.WriteObject(new BucketStatistics[] { statistics });
        }
    }

    [TestFixture]
    public class GeneralQueriesTests : NCacheTestBase
    {
        QueryExecutionOptions DEFAULT_QUERY_OPTIONS = CreateQueryExecutionOptions(true);

        [Test]
        public void Test_UpdateQuery_WithParmeterizedTag()
        {
            var x = JsonValueBase.Parse("[\'x-ray\']");

            cache.Clear();
            for (int i = 0; i < 1; i++)
            {
                cache.Insert($"k{i}", GetProduct(i));
            }

            var query = new QueryCommand($"UPDATE {typeof(Product).FullName} SET-META $Tag$ = @tag WHERE Id = 0");
            query.Parameters.Add("@tag", "['x-ray']");

            var rows = cache.QueryService.ExecuteNonQuery(query).AffectedRows;

            Assert.IsTrue(rows == 1);

            var item = cache.GetCacheItem("k0");

            Assert.IsTrue(item.Tags.Contains(new Tag("x-ray")));
        }

        [Test]
        public void Test_UpdateQuery_WithParmeterizedNamedTag()
        {
            cache.Clear();
            for (int i = 0; i < 1; i++)
            {
                cache.Insert($"k{i}", GetProduct(i));
            }

            var query = new QueryCommand($"UPDATE {typeof(Product).FullName} SET-META $namedtag$ = @tag");
            query.Parameters.Add("@tag", "[{ 'type': 'string', 'discount': 'hehe' }]");

            var rows = cache.QueryService.ExecuteNonQuery(query, new QueryExecutionOptions(true));


            var item = cache.GetCacheItem("k0");

            Assert.IsTrue(item.NamedTags.Contains("discount"));
        }

        [Test]
        public async Task Test_InsertInto_Files()
        {
            cache.Clear();
            string query = "INSERT INTO System.String (KEY, VALUE, Meta) VALUES ('product_1164', 'product_1011', '{ \"tags\":[\"Garments\", \"Imported\"], \"NamedTags\": [{\"discount\":0.5, \"type\": \"double\"}]}' )";

            QueryCommand command = new QueryCommand(query);

            var x = cache.QueryService.ExecuteNonQuery(command);

            var echo = cache.GetCacheItem("product_1164");

            Assert.IsNotNull(echo);
            Assert.That(echo.Tags.Contains(new Tag("Imported")));

            cache.Insert("x", GetProduct(10));

            query = $"UPDATE Alachisoft.NCache.Sample.Data.Product SET Images[1].ImageFormats[0].Format = \"GIF\" WHERE Name = 'IPhone'";

            command = new QueryCommand(query);

            var n = await cache.QueryService.ExecuteNonQueryAsync(command);

            query = $"UPDATE Alachisoft.NCache.Sample.Data.Product SET Category = \"Phone\" WHERE Name = 'IPhone'";

            command = new QueryCommand(query);

            n = await cache.QueryService.ExecuteNonQueryAsync(command, null);

            echo = cache.GetCacheItem("x");
            
            Assert.IsNotNull(echo);
            Assert.That(echo.GetValue<Product>().Images[1].ImageFormats[0].Format == "GIF");

        }

        [Test]
        public void Test_FileDependency_NoInterval()
        {

            cache.Clear();

            //File.WriteAllText(file, "Initial");

            var query = new QueryCommand("INSERT INTO System.String (Key, Value) VALUES ('k1', 'val1')");

            query.Parameters.Add("@meta", "{\"priority\": \"Default\",\"group\": \"China\",\"tags\": [\"China\"],\"namedtags\": [{\"type\": \"string\", \"Country\": \"China\"},{\"type\": \"int\", \"Id\": \"100000000\"},{\"type\": \"string\", \"Name\": \"Jackson\"}],\"Expiration\": {\"type\": \"Absolute\",\"interval\": 922337203687},\"dependency\": [{\"file\": {\"fileNames\": [\"c:/qa/temp/\"],\"interval\": \"0\"}}]}");

            cache.QueryService.ExecuteNonQuery(query);


            var echo = cache.Get<string>("k1");
        }

        [Test]
        public void Test_UpdateQuery_SetOperation()
        {
            cache.Clear();

            CompanyDetails details = new CompanyDetails("Alachisoft", 60, 24);

            for (int i = 0; i < 1; i++)
            {
                cache.Insert("key" + i, details);
            }
            var queryCommand = new QueryCommand($"UPDATE {details.GetType().FullName} SET Name = \"Diyatech\"");

            var result = cache.QueryService.ExecuteNonQuery(queryCommand, CreateQueryExecutionOptions(true, null, TimeSpan.FromDays(1)));

            Assert.That(result.FailedOperations.Count, Is.EqualTo(0));

            for (int i = 0; i < 1; i++)
            {
                var item = cache.Get<CompanyDetails>("key" + i);
                Assert.NotNull(item);
                Assert.That(item.Name, Is.EqualTo("Diyatech"));
            }
        }

        [Test]
        public void Test_UpdateQuery_SetOperation_UpperCase()
        {
            cache.Clear();

            CompanyDetails details = new CompanyDetails("Alachisoft", 60, 24);

            cache.Insert("key", details);

            VerifyExecuteReaderCount($"SELECT * FROM {typeof(CompanyDetails).FullName} WHERE Name = 'alachisoft'", 1);

            var queryCommand = new QueryCommand($"UPDATE {details.GetType().FullName} SET Name = @name");
            queryCommand.Parameters.Add("@name", "DIYATECH");

            var result = cache.QueryService.ExecuteNonQuery(queryCommand, CreateQueryExecutionOptions(true, null, TimeSpan.FromDays(1)));

            var item = cache.Get<CompanyDetails>("key");

            Assert.NotNull(item);
            Assert.That(item.Name, Is.EqualTo("DIYATECH"));


            VerifyExecuteReaderCount($"SELECT * FROM {typeof(CompanyDetails).FullName} WHERE Name = 'diyatech'", 1);
            VerifyExecuteReaderCount($"SELECT * FROM {typeof(CompanyDetails).FullName} WHERE Name = 'DiYatech'", 1);
            VerifyExecuteReaderCount($"SELECT * FROM {typeof(CompanyDetails).FullName} WHERE Name = 'DIYATECH'", 1);

            var selectCommand = new QueryCommand($"SELECT * FROM {typeof(CompanyDetails).FullName} WHERE Name = @name");
            selectCommand.Parameters.Add("@name", "diyatech");

            VerifyExecuteReaderCount(selectCommand, 1);

            selectCommand = new QueryCommand($"SELECT * FROM {typeof(CompanyDetails).FullName} WHERE Name = @name");
            selectCommand.Parameters.Add("@name", "diYAtech");

            VerifyExecuteReaderCount(selectCommand, 1);

            selectCommand = new QueryCommand($"SELECT * FROM {typeof(CompanyDetails).FullName} WHERE Name = @name");
            selectCommand.Parameters.Add("@name", "DIYATECH");

            VerifyExecuteReaderCount(selectCommand, 1);

        }

        [Test]
        public void Test_UpdateQuery_CopyOperation_ShouldFail()
        {
            cache.Clear();

            CompanyDetails details = new CompanyDetails("Alachisoft", 60, 24);
            cache.Insert("key", details);

            var queryCommand = new QueryCommand($"UPDATE {details.GetType().FullName} COPY SID = @Name");

            queryCommand.Parameters.Add("@Name", "Alachisoft");

            var result = cache.QueryService.ExecuteNonQuery(queryCommand, CreateQueryExecutionOptions(true));

            Assert.That(result.FailedOperations.Count == 1);

            var returnedValue = cache.Get<CompanyDetails>("key");

            Assert.IsNull(returnedValue.SID, null);
        }

        [Test]
        public void Test_UpdateQuery_CopyOperation_ShouldSucceed()
        {
            cache.Clear();

            CompanyDetails details = new CompanyDetails("Alachisoft", 60, 24);
            cache.Insert("key", details);

            var queryCommand = new QueryCommand($"UPDATE {details.GetType().FullName} COPY SID = @Name");

            queryCommand.Parameters.Add("@Name", "Name");

            var reader = cache.QueryService.ExecuteNonQuery(queryCommand);

            var returnedValue = cache.Get<CompanyDetails>("key");

            Assert.That(returnedValue.SID, Is.EqualTo(returnedValue.Name));
        }

        [Test]
        public void Temp()
        {
            object obj = new BucketStatistics();
            Console.WriteLine((ICompactSerializable)obj);
        }

        //[Test]
        public void Test_InsertNItems_Threaded()
        {
            cache.Clear();


            int n = 10_000;

            int perhead = n / 5;


            List<Task> threads = new List<Task>();

            for (int i = 1; i <= 5; i++)
            {
                threads.Add(Task.Factory.StartNew(() => AddData(i, perhead, bigData: true, useQuery: false)));
                Thread.Sleep(500);
            }


            Task.WaitAll(threads.ToArray());

        }

        public void AddData(int iter, int n, bool bigData = false, bool useQuery = false)
        {
            var query = "INSERT INTO {0} (KEY, VALUE) VALUES ('{1}', '{2}')";

            for (int j = 1; j <= n; j++)
            {
                try
                {
                    string key = j.ToString();
                    object value = bigData ? GetProduct(j) : "updated-" + (iter * n) + j;
                    var fqn = value.GetType().FullName;

                    Console.WriteLine($"key: {key}; Id: {j}");

                    if (useQuery)
                        cache.QueryService.ExecuteNonQuery(new QueryCommand(string.Format(query, fqn, key, value)));
                    else
                        cache.Insert(key, value);
                }
                catch (Exception e)
                {
                    Console.WriteLine("i = " + iter + "j = " + j);
                    Console.WriteLine(e);
                }
            }
        }

        //[Test]
        public void InsertNItems_Client1()
        {
            int n = 20_000;

            cache.Insert("x", "x");

            List<Task> threads = new List<Task>();

            for (int i = 0; i < 5; i++)
            {
                threads.Add(Task.Factory.StartNew(() =>
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (!cache.Contains("k" + i + "" + j))
                        {
                            Console.WriteLine();
                        }
                    }
                }));
                Thread.Sleep(1000);
            }

            Task.WaitAll(threads.ToArray());
        }

        //[Test]
        public void InsertNItems_Client2()
        {
            cache.Clear();

            Thread.Sleep(2000);

            AddData(1, 100_00);
        }

        //[Test]
        public void Test_LargeUpdateQuery_ExpectTimeout()
        {
            bool skipData = true;
            int count = 400_000;
            var list = new Dictionary<string, CacheItem>();

            if (!skipData)
            {
                cache.Clear();
                for (int i = 0; i < count; i++)
                {
                    var item = new CacheItem(GetProduct(i));

                    item.Priority = CacheItemPriority.High;
                    item.NamedTags = new NamedTagsDictionary();
                    item.NamedTags.Add("productType", "phone");
                    item.Tags = new Tag[] { new Tag("communication") };

                    list.Add($"prod{i}2", item);

                    if (count % 10000 == 0)
                    {
                        cache.InsertBulk(list);
                        list.Clear();
                    }
                }
            }

            if (list.Count > 0)
            {
                cache.InsertBulk(list);
                list.Clear();
            }

            string query = @$"
                    UPDATE {typeof(Product).FullName} 
                    SET this.ClassName = '""Phone""'
                ".Trim();

            var queryCommand = new QueryCommand(query);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var task = cache.QueryService.ExecuteNonQueryAsync(queryCommand, DEFAULT_QUERY_OPTIONS);
            var x = task.Result;
            stopwatch.Stop();
            Console.WriteLine("Time spend on query = " + stopwatch.ElapsedMilliseconds + "ms");


        }

        //        [Test]
        public void date()
        {
            object x = double.MinValue;
            var y = (JsonValue)x;
        }

        [Test]
        public void Test_UpdateQueryException()
        {
            cache.Clear();
            int count = 2;
            bool bulk = true;

            var fqn = typeof(Product).FullName;

            var list = new Dictionary<string, CacheItem>();

            for (int i = 0; i < count; i++)
            {
                var item = new CacheItem(GetProduct(i));

                //item.Priority = CacheItemPriority.High;
                item.NamedTags = new NamedTagsDictionary();
                item.NamedTags.Add("productType", "phone");
                item.Tags = new Tag[] { new Tag("communication") };
                item.Group = "G7";

                if (bulk)
                {
                    list.Add($"prod{i}", item);

                    if (count % 10000 == 0)
                    {
                        cache.InsertBulk(list);
                        list.Clear();
                    }
                }
                else
                {
                    cache.Insert($"prod{i}", item);
                }
            }

            if (list.Count > 0)
            {
                cache.InsertBulk(list);
                list.Clear();
            }

            VerifyExecuteReaderCount($"SELECT * FROM {fqn} WHERE Id >= {count / 2}", count / 2);
            VerifyExecuteReaderCount($"SELECT * FROM {fqn} WHERE ClassName = 'Phone'", 0);
            VerifyExecuteReaderCount($"SELECT * FROM {fqn} WHERE ClassName = 'Product - ClassName'", count);

            string query = @$"
                    UPDATE {typeof(Product).FullName} 
                    SET this.ClassName = '""Phone""'       
                ".Trim();

            var queryCommand = new QueryCommand(query);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var task = cache.QueryService.ExecuteNonQuery(queryCommand, new QueryExecutionOptions(true) { Timeout = TimeSpan.FromDays(1) });
            stopwatch.Stop();
            Console.WriteLine("Time spend on query = " + stopwatch.Elapsed.TotalMilliseconds + "ms");

            VerifyExecuteReaderCount($"SELECT * FROM {fqn} WHERE Id >= {count / 2}", count / 2);
            VerifyExecuteReaderCount($"SELECT * FROM {fqn} WHERE ClassName = 'Product - ClassName'", 0);
            VerifyExecuteReaderCount($"SELECT * FROM {fqn} WHERE ClassName = 'Phone'", count);
        }

        private int VerifyExecuteReaderCount(string query, int expectedCount)
        {
            var command = new QueryCommand(query);
            int counter = 0;
            var reader = cache.QueryService.ExecuteReader(command, false);
            while (reader.Read())
                counter++;

            Assert.That(counter, Is.EqualTo(expectedCount));

            return counter;
        }

        private int VerifyExecuteReaderCount(QueryCommand command, int expectedCount)
        {
            int counter = 0;
            var reader = cache.QueryService.ExecuteReader(command, false);
            while (reader.Read())
                counter++;

            Assert.That(counter, Is.EqualTo(expectedCount));

            return counter;
        }

        [Test]
        public void Test_UpdateQuery_WithMetadata()
        {
            int count = 1;
            cache.Clear();

            var list = new Dictionary<string, CacheItem>();

            for (int i = 0; i < count; i++)
            {
                var item = new CacheItem(GetProduct(i));

                item.Priority = CacheItemPriority.High;
                item.NamedTags = new NamedTagsDictionary();
                item.NamedTags.Add("productType", "phone");
                item.Tags = new Tag[] { new Tag("communication") };

                cache.Insert($"p{i}", item);

            }

            string query = @$"
                    UPDATE {typeof(Product).FullName} 
                    SET this.Id = '""Phone""'    
                    SET-META $group$ = '""updated""'
                ".Trim();

            var queryCommand = new QueryCommand(query);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var task = cache.QueryService.ExecuteNonQuery(queryCommand);

            stopwatch.Stop();
            Console.WriteLine("Time spend on query = " + stopwatch.ElapsedMilliseconds + "ms");


        }

        [Test]
        public void Test_UpdateQuery_Add_Operation()
        {
            cache.Clear();

            cache.Insert("key", GetProduct(1));

            string AbcParameter = "abc";
            JsonValue JSonAbc = (JsonValue)AbcParameter;

            string query = $"Update {typeof(Product).FullName} ADD {"Abc"} = @abc";

            var queryCommand = new QueryCommand(query);
            queryCommand.Parameters.Add("@abc", JSonAbc);

            var rows = cache.QueryService.ExecuteNonQuery(queryCommand, CreateQueryExecutionOptions(true));
            rows = cache.QueryService.ExecuteNonQuery(queryCommand, CreateQueryExecutionOptions(true));

            var item = cache.Get<JsonObject>("key");
            Assert.IsTrue(item.ContainsAttribute("Abc"));
            Assert.IsTrue(item["Abc"].ToString().Equals("\"abc\""));
        }

        //[Test]
        public async Task Test_UpdateQuery_Add_Operation_MultiClient()
        {
            cache.Clear();

            AddData(1, 10_000, bigData: true);

            VerifyExecuteReaderCount($"SELECT * FROM {typeof(Product).FullName} WHERE Id <= 10000", 10_000);

            
        }


        [Test]
        public void Test_SetMeta_Group_ShouldSucceed()
        {
            cache.Clear();

            var details = new CompanyDetails("Alachisoft", 50, 20);

            cache.Insert("d", GetProduct(1));

            var queryCommand = new QueryCommand($"UPDATE {typeof(Product).FullName} SET-META $group$ = @group");
            queryCommand.Parameters.Add("@group", "hello");

            var results = cache.QueryService.ExecuteNonQuery(queryCommand, CreateQueryExecutionOptions(true));

            Assert.IsTrue(results.AffectedRows == 1);

            Thread.Sleep(1000);

            var echo = cache.GetCacheItem("d");


            Assert.That(echo.Group, Is.EqualTo("hello"));
        }

        [Test]
        public void Test_SetMeta_Group_MultipleInlineGroups()
        {
            cache.Clear();

            string query = $"Update {typeof(Product).FullName} SET-META $group$ = \"['g1', 'g2']\"";

            cache.Insert("key", GetProduct(1));

            Assert.Throws(Is.InstanceOf<Exception>()
                .And.Message.Contains("Failed to parse 'Group' metadata in the given SQ"),
                () => cache.QueryService.ExecuteNonQuery(new QueryCommand(query)));
        }

        [Test]
        public void Test_SetMeta_Group_JsonArrayGroups()
        {
            cache.Clear();
            JsonArray array = new JsonArray
            {
                "g1",
                "g2"
            };

            var query = new QueryCommand($"Update {typeof(Product).FullName} SET-META $group$ = @group");

            cache.Insert("key", GetProduct(1));

            query.Parameters.Add("@group", array);

            Assert.Throws(Is.InstanceOf<Exception>()
                .And.Message.Contains("Failed to parse 'Group' metadata in the given SQ"),
                () => cache.QueryService.ExecuteNonQuery(query));
        }


        [Test]
        public void UpdateQuery_DuplicateTags()
        {
            cache.Clear();

            var item = new CacheItem(GetProduct(1))
            {
                Tags = new Tag[] { new Tag("Alpha"), new Tag("Beta") }
            };

            cache.Insert("key", item);

            var cItem = cache.GetCacheItem("key");

            //--- Now verify that returned object have same tag(s) ... 
            if (cItem == null)
                throw new Exception("'NULL' is returned instead of a cache item.");
            if (cItem.Tags == null)
                throw new Exception("'NULL' is returned instead of any 'Tag'.");


            var tagsArray = new JsonArray
            {
                "Gama1",
                "Alpha",
                "Gama2"
            };

            var queryCommand = new QueryCommand($"UPDATE {typeof(Product).FullName} SET-META $Tag$ = @tag");
            queryCommand.Parameters.Add("@tag", tagsArray);

            var rowsEffected = cache.QueryService.ExecuteNonQuery(queryCommand, DEFAULT_QUERY_OPTIONS);

            cItem = cache.GetCacheItem("key");

            //--- Now verify that returned object have same tag(s) ... 
            if (cItem == null)
                throw new Exception("'NULL' is returned instead of a cache item.");
            if (cItem.Tags == null)
                throw new Exception("'NULL' is returned instead of any 'Tag'.");
            if (cItem.Tags.Count().ToString() != "4")
                throw new Exception(cItem.Tags.Count().ToString() + " is returned instaed of 4");
            if (!cItem.Tags[0].TagName.Contains("Alpha") && cItem.Tags[1].TagName.Contains("Beta") && cItem.Tags[2].TagName.Contains("Gama1") && cItem.Tags[2].TagName.Contains("Gama2"))
                throw new Exception("Returned cache item have an invalid tag [" + cItem.Tags[0].TagName + "] instead of tag [ " + tagsArray[0].Value + " ].");
        }

        [Test]
        public void UpdateQuery_RemoveNamedTag()
        {
            cache.Clear();

            string key = "NamedTags Key";
            NamedTagsDictionary dic = new NamedTagsDictionary();
            dic.Add("NamedTagKey", "NamedTagValue");
            CacheItem item = new CacheItem("NamedTags value");
            item.NamedTags = dic;

            cache.Add(key, item);

            CacheItem cItem = cache.GetCacheItem(key);

            if (cItem.NamedTags != null)
            {
                if (dic.Count != cItem.NamedTags.Count)
                    throw new Exception("NamedTags donot match.");
            }

            JsonArray namedTagsArray = new JsonArray();

            Tuple<string, string> namedTags = Tuple.Create("NamedTagKey", "NamedTagValue");
            namedTagsArray.Add(namedTags.Item1);

            string query = $"Update System.String Remove-meta $NamedTag$ = @namedTags";

            QueryCommand queryCommand = new QueryCommand(query);
            queryCommand.Parameters.Add("@namedTags", namedTagsArray);

            var reader = cache.QueryService.ExecuteNonQuery(queryCommand);

            cItem = cache.GetCacheItem(key);


            Assert.IsNull(cItem.NamedTags);
        }

        [Test]
        public void Test_ComplexObject_MoveOperation_ShouldFail()
        {
            cache.Clear();

            var product = GetProduct(10);

            cache.Insert("item", product);

            var jsonStr = JsonConvert.SerializeObject(product.Images[0].ImageFormats[1]);

            var json = new JsonObject(jsonStr);

            var queryCommand = new QueryCommand($"UPDATE {typeof(Product).FullName} MOVE Images[0].ImageFormats[0] = @val");
            queryCommand.Parameters.Add("@val", json);

            var results = cache.QueryService.ExecuteNonQuery(queryCommand, CreateQueryExecutionOptions(true));

        }

        [Test]
        public void Test_NonQuery_In_ExecuteReader()
        {
            cache.Clear();

            var product = GetProduct(10);

            cache.Insert("item", product);

            var jsonStr = JsonConvert.SerializeObject(product.Images[0].ImageFormats[1]);

            var json = new JsonObject(jsonStr);

            var queryCommand = new QueryCommand($"UPDATE {typeof(Product).FullName} MOVE X = Y");

            Assert.Throws<OperationFailedException>(() => cache.QueryService.ExecuteReader(queryCommand, false));

            queryCommand = new QueryCommand("INSERT INTO Diyatech.QA.Data.Product (KEY,VALUE) VALUES ('k', 'val')");

            Assert.Throws<OperationFailedException>(() => cache.QueryService.ExecuteReader(queryCommand, false));


        }

        [Test]
        public void Test_Update_NamedTags_Inproc()
        {
            cache.Clear();
            var company = new CompanyDetails("Diyatech", 10, 1);

            var item = new CacheItem(company)
            {
                NamedTags = new NamedTagsDictionary()
            };


            cache.Insert("com", item);


            JsonArray namedTags = new JsonArray();
            JsonObject FlashDiscount = new JsonObject();
            FlashDiscount.AddAttribute("FlashDiscount", "");
            FlashDiscount.AddAttribute("type", "string");
            namedTags.Add(FlashDiscount);

            var queryCommand = new QueryCommand($"UPDATE {typeof(CompanyDetails).FullName} SET-META $NamedTag$ =@nametag");
            queryCommand.Parameters.Add("@nametag", namedTags);


            var rows = cache.QueryService.ExecuteNonQuery(queryCommand, CreateQueryExecutionOptions(true));

            Assert.IsTrue(rows.AffectedRows == 1);

            var echo = cache.GetCacheItem("com");

            Assert.IsNotNull(echo);
            Assert.IsTrue(echo.NamedTags.Contains("FlashDiscount"));
        }

        [Test]
        public void Test_Update_NamedTags_ObjectInWhereClause_ShouldFail()
        {
            cache.Clear();
            var company = new CompanyDetails("Diyatech", 10, 1);

            cache.Insert("com", company);

            JsonArray namedTags = new JsonArray();
            JsonObject FlashDiscount = new JsonObject();
            FlashDiscount.AddAttribute("FlashDiscount", "");
            FlashDiscount.AddAttribute("type", "string");
            namedTags.Add(FlashDiscount);

            var queryCommand = new QueryCommand($"UPDATE {typeof(CompanyDetails).FullName} TEST Id = 1 WHERE Id = @nametag");
            queryCommand.Parameters.Add("@nametag", namedTags);


            Assert.Throws(Is.InstanceOf<Exception>(), () => cache.QueryService.ExecuteNonQuery(queryCommand));
        }

        [Test]
        public void Test_Update_TestOperation_AffectedRows()
        {
            cache.Clear();
            var company = new CompanyDetails("Diyatech", 10, 1);

            cache.Insert("com", company);


            var queryCommand = new QueryCommand($"UPDATE {typeof(CompanyDetails).FullName} TEST Id = 1");


            var result = cache.QueryService.ExecuteNonQuery(queryCommand, CreateQueryExecutionOptions(true));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.AffectedRows == 0);
        }

        [Test]
        public void Test_Select_ObjectInWhereClause_ShouldFail()
        {
            cache.Clear();
            var company = new CompanyDetails("Diyatech", 10, 1);

            cache.Insert("com", company);

            JsonArray namedTags = new JsonArray();
            JsonObject FlashDiscount = new JsonObject();
            FlashDiscount.AddAttribute("FlashDiscount", "");
            FlashDiscount.AddAttribute("type", "string");
            namedTags.Add(FlashDiscount);

            var queryCommand = new QueryCommand($"SELECT {typeof(CompanyDetails).FullName} WHERE Id = @nametag");
            queryCommand.Parameters.Add("@nametag", namedTags);


            Assert.Throws(Is.InstanceOf<Exception>(), () => cache.QueryService.ExecuteReader(queryCommand));
        }

        [Test]
        public void Test_Select_PrimitiveInWhereClause_ShouldPass()
        {
            cache.Clear();
            var company = new CompanyDetails("Diyatech", 10, 1);

            cache.Insert("com", company);

            JsonArray namedTags = new JsonArray();
            JsonObject FlashDiscount = new JsonObject();
            FlashDiscount.AddAttribute("FlashDiscount", "");
            FlashDiscount.AddAttribute("type", "string");
            namedTags.Add(FlashDiscount);

            var queryCommand = new QueryCommand($"SELECT * FROM {typeof(CompanyDetails).FullName} WHERE Id = @nametag");
            queryCommand.Parameters.Add("@nametag", 1);


            cache.QueryService.ExecuteReader(queryCommand);
        }

        [Test]
        public void Test_Delete_ObjectInWhereClause_ShouldFail()
        {
            cache.Clear();
            var company = new CompanyDetails("Diyatech", 10, 1);

            cache.Insert("com", company);

            JsonArray namedTags = new JsonArray();
            JsonObject FlashDiscount = new JsonObject();
            FlashDiscount.AddAttribute("FlashDiscount", "");
            FlashDiscount.AddAttribute("type", "string");
            namedTags.Add(FlashDiscount);

            var queryCommand = new QueryCommand($"DELETE FROM {typeof(CompanyDetails).FullName} WHERE Id = @nametag");
            queryCommand.Parameters.Add("@nametag", namedTags);


            Assert.Throws(Is.InstanceOf<Exception>(), () => cache.QueryService.ExecuteNonQuery(queryCommand));
        }

        [Test]
        public void Test_Delete_ObjectInWhereClause_ShouldPass()
        {
            cache.Clear();
            var company = new CompanyDetails("Diyatech", 10, 1);

            cache.Insert("com", company);

            JsonArray namedTags = new JsonArray();
            JsonObject FlashDiscount = new JsonObject();
            FlashDiscount.AddAttribute("FlashDiscount", "");
            FlashDiscount.AddAttribute("type", "string");
            namedTags.Add(FlashDiscount);

            var queryCommand = new QueryCommand($"DELETE FROM {typeof(CompanyDetails).FullName} WHERE Id = @nametag");
            queryCommand.Parameters.Add("@nametag", 1);

            cache.QueryService.ExecuteNonQuery(queryCommand);

            Assert.IsTrue(cache.Count == 0);
        }

        [Test]
        public void Test_UpdateQuery_NamedTagArray()
        {
            cache.Clear();
            cache.Insert("x", GetProduct(10));

            JsonArray namedTags = GetNamedTagsArray();
            var queryCommand = new QueryCommand($"UPDATE {typeof(Product).FullName} SET-META $NamedTag$ = @nametag");
            queryCommand.Parameters.Add("@nametag", namedTags);

            var reader = cache.QueryService.ExecuteNonQuery(queryCommand);

            var item = cache.GetCacheItem("x");
        }

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
            JsonArray NameTagsArray = new JsonArray
            {
                FlashDiscount,
                Discount,
                Percentage
            };

            return NameTagsArray;
        }

        [Test]

        public void Test_delete_query_with_tag()
        {
            string query = $"DELETE FROM {typeof(string).FullName} WHERE $Tag$ = ?";
            
            Hashtable values = new Hashtable();

            Alachisoft.NCache.Runtime.Caching.Tag tag = new Tag("test");

            values.Add("$Tag$", tag); // -> Here the "Alachisoft.NCache.Runtime.Caching.Tag" object is sent to query parameters
            var qc = new QueryCommand(query);
            
            var Tags = new Tag[] { tag };
            
            qc.Parameters.Add("$Tag$", tag);

            var item = new CacheItem("x")
            {
                Tags = Tags
            };



            cache.Insert("key", item);
            var result12 = cache.QueryService.ExecuteNonQuery(qc);


            Assert.IsFalse(cache.Contains("key"));


        }

        [Test]
        public void Test_RemoveMeta_Group_AffectedRows()
        {
            cache.Clear();

            for (int i = 0; i < 20; i++)
            {
                var details = new CompanyDetails("Alachisoft" + i, 50, i);
                var cacheItem = new CacheItem(details);

                if (i != 0 && i % 5 == 0)
                    cacheItem.Group = "g1";

                cache.Insert("k-" + i, cacheItem);
            }

            var queryCommand = new QueryCommand($"UPDATE {typeof(CompanyDetails).FullName} REMOVE-META $group$ = @group");
            queryCommand.Parameters.Add("@group", "g1");

            var results = cache.QueryService.ExecuteNonQuery(queryCommand, CreateQueryExecutionOptions());

            Assert.That(results.AffectedRows, Is.EqualTo(3));

            Thread.Sleep(1000);

            var echo = cache.GetCacheItem("k-5");


            Assert.That(echo.Group, Is.Null);
        }

        [Test]
        public void Test_ComplexObject_MoveOperation()
        {
            cache.Clear();
            string key = "k";
            var product = GetProduct(10);
            var format1 = product.Images[0].ImageFormats[1];

            cache.Insert(key, product);

            var returnedValue = cache.Get<JsonObject>(key).ToString();

            QueryExecutionOptions executionOptions = new QueryExecutionOptions(true);

            var queryCommand = new QueryCommand($"UPDATE {typeof(Product).FullName} MOVE Images[0].ImageFormats[0] = Images[0].ImageFormats[1]");

            var effectedRows = cache.QueryService.ExecuteNonQuery(queryCommand, executionOptions);

            var echo = cache.Get<Product>("k");

            Assert.IsNotNull(echo);

            Assert.That(echo.Images[0].ImageFormats.Length, Is.EqualTo(4));
            Assert.That(echo.Images[0].ImageFormats[0], Is.EqualTo(format1));


        }

        //[Test]
        public void Test_CacheSize()
        {
            cache.Clear();
            StringBuilder data = new StringBuilder();

            for(int i = 0; i < 308; i++)
            {
                data.Append("i");
            }

            var bytes = JsonBinaryFormatter.ToByteArray(data.ToString(), null);

            for(int i = 0; i < 100_000; i++)
            {
                cache.Insert("tck-" + i, data.ToString());
            }
        }


        //[Test]
        public void Test_WriteThru_Sample()
        {
            cache.Clear();

            // Initially Populating Data
            List<Person> people = new List<Person>()
            {
                new Person()
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Age = 32,
                    City = "New York"
                },
                new Person()
                {
                    FirstName = "Elizabeth",
                    LastName = "Brown",
                    Age = 48,
                    City = "New York"
                },

                new Person()
                {
                    FirstName = "Evan",
                    LastName = "Hunter",
                    Age = 37,
                    City = "Virginia"
                },

            };

            foreach (var person in people)
            {
                cache.Insert("k1", new CacheItem(person));
            }

            var query = new QueryCommand("UPDATE dbo.Person SET City = @newcity WHERE City = @oldcity");

            query.Parameters.Add("@newcity", "Virginia");
            query.Parameters.Add("@oldcity", "New York");

            WriteThruOptions writeThruOptions = new WriteThruOptions(WriteMode.WriteThru)
            {
                Hint = "City"
            };

            QueryExecutionOptions queryExecutionOptions = new QueryExecutionOptions(writeThruOptions);

            QueryResult result = cache.QueryService.ExecuteNonQuery(query, queryExecutionOptions);

            Console.WriteLine($"{result.AffectedRows} rows affected");

        }

        public void Test_WriteThru_Sample2()
        {
            cache.Clear();

            // Initially Populating Data
            List<Person> people = new List<Person>()
            {
                new Person()
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Age = 32,
                    City = "New York"
                },
                new Person()
                {
                    FirstName = "Elizabeth",
                    LastName = "Brown",
                    Age = 48,
                    City = "New York"
                },

                new Person()
                {
                    FirstName = "Evan",
                    LastName = "Hunter",
                    Age = 37,
                    City = "Virginia"
                },

            };

            foreach (var person in people)
            {
                cache.Insert("k1", new CacheItem(person));
            }

            var query = new QueryCommand("UPDATE dbo.Person SET City = @newcity WHERE City = @oldcity");

            query.Parameters.Add("@newcity", "Virginia");
            query.Parameters.Add("@oldcity", "New York");

            WriteThruOptions writeThruOptions = new WriteThruOptions(WriteMode.WriteThru)
            {
                Hint = "SQLServer"
            };

            QueryExecutionOptions queryExecutionOptions = new QueryExecutionOptions(writeThruOptions);

            QueryResult result = cache.QueryService.ExecuteNonQuery(query, queryExecutionOptions);

            Console.WriteLine($"{result.AffectedRows} rows affected");
        }

        //[Test]
        public void Test_CustomDependency()
        {
            QueryCommand queryCommand = new QueryCommand("INSERT INTO System.String (KEY,VALUE,META) VALUES (@key, @val, @meta)");
            queryCommand.Parameters.Add("@key", "key");
            queryCommand.Parameters.Add("@val", "hello");
            queryCommand.Parameters.Add("@meta", $@"{{'dependency': [{{'custom': {{'providername': 'providerName','parm':{{'id':'LoggerInfo',connectionstring:'connectionString'}} }}}}]}}");

            cache.QueryService.ExecuteNonQuery(queryCommand);
        }
    }
}
