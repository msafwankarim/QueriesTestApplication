using Alachisoft.NCache.Client;
using Alachisoft.NCache.Common.Queries;
using Alachisoft.NCache.Common.Snmp.Oids;
using Alachisoft.NCache.Runtime.Caching;
using Alachisoft.NCache.Runtime.JSON;
using Alachisoft.NCache.Sample.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QueriesTestApplication.Utils;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace QueriesTestApplication
{
    public class InsertQueriesTest
    {
        private int count = 0;
        ICache cache;
        public Dictionary<string, ResultStatus> testResults;
        public Dictionary<string, TestResult> testResults1;
        List<Product> productList;
        Report _report;
        public InsertQueriesTest()
        {
            cache = CacheManager.GetCache(Common.CacheName);
            testResults = new Dictionary<string, ResultStatus>();
            testResults1 = new Dictionary<string, TestResult>();
            productList = new List<Product>();
            _report = new Report(nameof(UpdateQueriesTest));
        }

        public Report Report { get => _report; }

        public Dictionary<string, ResultStatus> TestResults
        {
            get { return testResults; }
        }

        public void PrintReport() { _report.PrintReport(); }


        //public void Add0(string query)
        //{
        //    var methodName = "Add0";

        //    try
        //    {

        //        cache.Clear();
        //        string key = GetKey();
        //        var val = GetProduct();

        //        //string query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value) Values (" + key + ", @val)";
        //        QueryCommand queryCommand = new QueryCommand(query);
        //        //queryCommand.Parameters.Add("@val", val);
        //        var result = cache.SearchService.ExecuteNonQuery(queryCommand);
        //        if (result > 0 && cache.Contains(key))
        //        {
        //            var obj = cache.Get<object>(key);
        //            if (obj == null)
        //                throw new Exception("Case failed.");
        //        }

        //        Console.WriteLine("Successful: Add a key-value pair with Insert. Precondition: key not present.");
        //        testResults.Add(methodName, ResultStatus.Success);
        //        count++;
        //    }
        //    catch (Exception ex)
        //    {
        //        testResults.Add(methodName, ResultStatus.Failure);
        //        Console.WriteLine("Failure: Add a key-value pair with Insert. Precondition: key not present.");
        //    }

        //}
        ////--- Add a key-value pair. Precondition: key not present.


        public void AddJArray()
        {
            var methodName = "AddJArray";
            try
            {
                cache.Clear();
                PopulateProductList();
                string key = GetKey();

                string Description = "Adding numbers in Cache!";

                JArray val = new JArray();
                val.Add(Description);
                val.Add(1);
                val.Add(2);
                val.Add(3);

                string insertQuery = "Insert into abc (Key,Value) Values ( @cachekey, @val)";
                QueryCommand queryCommand = new QueryCommand(insertQuery);
                queryCommand.Parameters.Add("@cachekey", key);
                queryCommand.Parameters.Add("@val", val);

                var insertresult = cache.SearchService.ExecuteNonQuery(queryCommand);

                var result = cache.Get<JArray>(key);

                if (result.Count == 4 && result[0].ToString() == Description)
                    _report.AddPassedTestCase(methodName, "Success: Add a JArray in Cache and then verify it's type");
                else
                    throw new Exception("Failure: Add a JArray in Cache and then verify it's type");


            }
            catch (Exception ex)
            {
                count++;
                _report.AddFailedTestCase(methodName, ex);
            }

        }

        public void Add00()
        {
            var methodName = "Add00";

            try
            {
                cache.Clear();
                string key = GetKey();
                var val = GetProduct();

                string query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value) Values (" + key + ", @val)";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@val", val);
                var result = cache.SearchService.ExecuteNonQuery(queryCommand);
                if (result > 0 && cache.Contains(key))
                {
                    var obj = cache.Get<object>(key);
                    if (obj == null)
                        throw new Exception("Case failed.");
                }

                _report.AddPassedTestCase(methodName, "Successful: Add a key-value pair with Insert. Precondition: key not present.");
                testResults.Add(methodName, ResultStatus.Success);
                count++;
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
                testResults.Add(methodName, ResultStatus.Failure);
                Console.WriteLine("Failure: Add a key-value pair with Insert. Precondition: key not present.");
            }

        }

        //--- Add a key-value pair. Precondition: key not present.
        public void Add1()
        {
            var methodName = "Add1";
            try
            {
                cache.Clear();
                string key = GetKey();
                var val = GetProduct();

                string query = "Upsert into Alachisoft.NCache.Sample.Data.Product (Key,Value) Values (" + key + ", @val)";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@val", val);
                var result = cache.SearchService.ExecuteNonQuery(queryCommand);

                if (result > 0 && cache.Contains(key))
                {
                    var obj = cache.Get<object>(key);
                    if (obj == null)
                        throw new Exception("Case failed.");
                }
                testResults.Add(methodName, ResultStatus.Success);
                count++;
                _report.AddPassedTestCase(methodName, "Successful: Add a key-value pair by upsert. Precondition: key not present.");

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
                testResults.Add(methodName, ResultStatus.Failure);
                Console.WriteLine("Failure: Add a key-value pair by upsert. Precondition: key not present.");
            }

        }


        //--- Add a key-value pair. Precondition: key present.

        public void Add2()
        {
            var methodName = "Add2";
            try
            {
                cache.Clear();
                string key = GetKey();
                var val = GetProduct();
                cache.Add(key, val);
                //insert should throw exception while upsert should work fine
                string upsertQuery = "Upsert into Alachisoft.NCache.Sample.Data.Product (Key,Value) Values (" + key + ", @val)";
                string insertQuery = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value) Values (" + key + ", @val)";

                QueryCommand queryCommand = new QueryCommand(upsertQuery);
                queryCommand.Parameters.Add("@val", val);
                var upsertresult = cache.SearchService.ExecuteNonQuery(queryCommand);

                if (upsertresult > 0)
                {
                    try
                    {
                        queryCommand = new QueryCommand(insertQuery);
                        queryCommand.Parameters.Add("@val", val);
                        var insertresult = cache.SearchService.ExecuteNonQuery(queryCommand);
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("already"))
                        {
                            testResults.Add(methodName, ResultStatus.Success);
                            _report.AddPassedTestCase(methodName, "Successful: Add a key-value pair with insert and upsert while key is present");
                            count++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }


        }

        //--- Add a key-value pair with Empty string value. Precondition: key not present.
        public void Add3()
        {
            var methodName = "Add3";
            try
            {
                cache.Clear();
                string key = GetKey();
                var val = "";
                //  cache.Add(key, val);

                string insertQuery = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value) Values (@cachekey, @val)";
                QueryCommand queryCommand = new QueryCommand(insertQuery);
                queryCommand.Parameters.Add("@cachekey", key);
                queryCommand.Parameters.Add("@val", val);
                var insertresult = cache.SearchService.ExecuteNonQuery(queryCommand);

                if (insertresult > 0 && cache.Contains(key))
                {
                    object obj = cache.Get<object>(key);
                    if (obj.ToString().Equals(string.Empty) == false)
                        throw new Exception(obj + " is returned instead of an EMPTY string.");
                    else
                        _report.AddPassedTestCase(methodName, "Successful: Add a key-value pair with Empty string value");
                    testResults.Add(methodName, ResultStatus.Success);
                    count++;
                }
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }

        }
        //--- Add a key-value pair with 'null' value. Add 1st overload. Precondition: key not present.
        public void Add4()
        {
            var methodName = "Add4";
            try
            {
                cache.Clear();
                string key = GetKey();
                Product val = null;
                // cache.Add(key, val);

                string insertQuery = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value) Values (" + key + ", @val)";
                QueryCommand queryCommand = new QueryCommand(insertQuery);
                queryCommand.Parameters.Add("@val", val);
                var insertresult = cache.SearchService.ExecuteNonQuery(queryCommand);
                _report.AddPassedTestCase(methodName, "Success: Add a key-value pair with 'null' value using insert.");

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
                // todo why null check is added below 
                if (ex.Message.Contains("null"))
                {
                    count++;
                    Console.WriteLine("Failure: Add a key-value pair with 'null' value using insert.");
                    testResults.Add(methodName, ResultStatus.Failure);
                }
            }

        }

        //--- Add a key-value pair with 'null' value. Add 1st overload. Precondition: key not present.
        public void Add5()
        {
            var methodName = "Add5";
            try
            {
                cache.Clear();
                string key = GetKey();
                Product val = null;
                // cache.Add(key, val);

                string insertQuery = "Upsert into Alachisoft.NCache.Sample.Data.Product (Key,Value) Values (" + key + ", @val)";
                QueryCommand queryCommand = new QueryCommand(insertQuery);
                queryCommand.Parameters.Add("@val", val);
                var insertresult = cache.SearchService.ExecuteNonQuery(queryCommand);
                testResults.Add(methodName, ResultStatus.Success);
                _report.AddPassedTestCase(methodName, "Success: Add a key-value pair with 'null' value using upsert.");
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

                if (ex.Message.Contains("null"))
                {
                    testResults.Add(methodName, ResultStatus.Failure);
                    count++;
                    Console.WriteLine("Failure: Add a key-value pair with 'null' value using upsert.");
                }
            }

        }

        //--- Add a key-value pair with 'empty' key.  Precondition: key not present.
        public void Add6()
        {
            var methodName = "Add6";
            try
            {
                cache.Clear();
                string key = "";
                Product val = GetProduct();
                //  cache.Add(key, val);

                string insertQuery = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value) Values (\"" + key + "\", @val)";
                string upsertQuery = "Upsert into Alachisoft.NCache.Sample.Data.Product (Key,Value) Values (\"" + key + "\", @val)";
                try
                {
                    QueryCommand queryCommand = new QueryCommand(insertQuery);
                    queryCommand.Parameters.Add("@val", val);
                    var insertresult = cache.SearchService.ExecuteNonQuery(queryCommand);
                }
                catch (Exception ex)
                {
                    try
                    {
                        QueryCommand queryCommand = new QueryCommand(upsertQuery);
                        queryCommand.Parameters.Add("@val", val);
                        var upsertresult = cache.SearchService.ExecuteNonQuery(queryCommand);
                    }
                    catch (Exception)
                    {
                        testResults.Add(methodName, ResultStatus.Success);
                        _report.AddPassedTestCase(methodName, "Successful: Add a key-value pair with 'empty' key");
                        count++;
                    }
                }

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }

        }

        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //                        Verify Query Format
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------



        //  Provide Query with wrong format misspell Insert
        public void VerifyQueryFormat0()
        {
            var methodName = "VerifyQueryFormat0";
            try
            {
                cache.Clear();
                string key = GetKey();
                var val = GetProduct();

                var query = "Insrt into Alachisoft.NCache.Sample.Data.Product (Key,Value) Values ('key','val')";
                QueryCommand queryCommand = new QueryCommand(query);
                cache.SearchService.ExecuteNonQuery(queryCommand);
                testResults.Add(methodName, ResultStatus.Failure);
                throw new Exception(@"{Failure: Provide Query with wrong format misspell Insert ");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Failure"))
                    _report.AddFailedTestCase(methodName, ex);
                else
                    _report.AddPassedTestCase(methodName, @"{Successful: Provide Query with wrong format misspell Insert ");
            }

        }

        //  Provide Query with wrong format after Into directly start with key,val
        public void VerifyQueryFormat1()
        {
            var methodName = "VerifyQueryFormat1";
            try
            {
                cache.Clear();
                var query = "Insert into (Key, Value) Values('key','val')";
                QueryCommand queryCommand = new QueryCommand(query);
                cache.SearchService.ExecuteNonQuery(queryCommand);
                testResults.Add(methodName, ResultStatus.Failure);
                throw new Exception(@"{Failure: Provide Query with wrong format after Into directly start with key,val ");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Failure"))
                    _report.AddFailedTestCase(methodName, ex);
                else
                    _report.AddPassedTestCase(methodName, @"{Successful: Provide Query with wrong format after Into directly start with key,val ");
            }

        }

        //  Provide Query with wrong format (key, value)
        public void VerifyQueryFormat2()
        {
            var methodName = "VerifyQueryFormat2";
            try
            {
                cache.Clear();
                var query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key, Value) Values('key',  'val')";
                QueryCommand queryCommand = new QueryCommand(query);
                cache.SearchService.ExecuteNonQuery(queryCommand);
                testResults.Add(methodName, ResultStatus.Failure);
                throw new Exception(@"{Failure: Provide Query with wrong format after Into directly start with key,val ");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Failure"))
                    _report.AddFailedTestCase(methodName, ex);
                else
                    _report.AddPassedTestCase(methodName, @"{Successful: Provide Query with wrong format (key, value) ");
            }

        }


        //use reserved keys in query
        public void VerifyQueryFormat3()
        {
            var methodName = "VerifyQueryFormat3";
            try
            {
                cache.Clear();
                var query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value) Values('Insert','Value')";
                QueryCommand queryCommand = new QueryCommand(query);
                cache.SearchService.ExecuteNonQuery(queryCommand);
                testResults.Add(methodName, ResultStatus.Failure);
                throw new Exception(@"{Failure: use reserved keys in query");

            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Failure"))
                    _report.AddFailedTestCase(methodName, ex);
                else
                    _report.AddPassedTestCase(methodName, @"{Successful: use reserved keys in query");
      
            }

        }

        //  Provide Query with wrong format misspell Insert
        public void VerifyQueryFormat4()
        {
            var methodName = "VerifyQueryFormat4";
            try
            {
                cache.Clear();
                string key = GetKey();
                var val = GetProduct();

                var query = "Upset into Alachisoft.NCache.Sample.Data.Product (Key,Value) Values ('key','val')";
                QueryCommand queryCommand = new QueryCommand(query);
                cache.SearchService.ExecuteNonQuery(queryCommand);
                testResults.Add(methodName, ResultStatus.Failure);
                throw new Exception(@"Failure :  Provide upsert Query with wrong format misspell Insert");

            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Failure"))
                    _report.AddFailedTestCase(methodName, ex);
                else
                    _report.AddPassedTestCase(methodName, @"{Successful: Provide upsert Query with wrong format misspell Insert ");
               
            }

        }

        //  Provide Query with wrong format after Into directly start with key,val
        public void VerifyQueryFormat5()
        {
            var methodName = "VerifyQueryFormat5";
            try
            {
                cache.Clear();
                var query = "upsert into (Key, Value) Values('key','val')";
                QueryCommand queryCommand = new QueryCommand(query);
                cache.SearchService.ExecuteNonQuery(queryCommand);
                testResults.Add(methodName, ResultStatus.Failure);
                throw new Exception("Failure: Provide upsert Query with wrong format after Into directly start with key, val");
            }
            catch (Exception ex)
            {

                if (ex.Message.Contains("Failure"))
                    _report.AddFailedTestCase(methodName, ex);
                else
                    _report.AddPassedTestCase(methodName, @"{Successful: Provide upsert Query with wrong format after Into directly start with key,val ");

               
            }

        }

        //  Provide Query with wrong format (key, value)
        public void VerifyQueryFormat6()
        {
            var methodName = "VerifyQueryFormat6";
            try
            {
                cache.Clear();
                var query = "Upsert into Alachisoft.NCache.Sample.Data.Product (Key, Value) Values('key',  'val')";
                QueryCommand queryCommand = new QueryCommand(query);
                cache.SearchService.ExecuteNonQuery(queryCommand);
                testResults.Add(methodName, ResultStatus.Failure);
                throw new Exception("{Failure: : Provide upsert Query with wrong format(key, value) ");

            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Failure"))
                    _report.AddFailedTestCase(methodName, ex);
                else
                    _report.AddPassedTestCase(methodName, @"{Successful: Provide upsert Query with wrong format (key, value) ");
                              
            }

        }






        //use reserved keys in query
        public void VerifyQueryFormat7()
        {
            var methodName = "VerifyQueryFormat7";
            try
            {
                cache.Clear();
                var query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value) Values('Insert','Value')"; // ToDo discuss 
                QueryCommand queryCommand = new QueryCommand(query);
                cache.SearchService.ExecuteNonQuery(queryCommand);
                testResults.Add(methodName, ResultStatus.Failure);
                throw new Exception(@"{Failure: use reserved keys in upsert query ");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Failure"))
                    _report.AddFailedTestCase(methodName, ex);
                else
                    _report.AddPassedTestCase(methodName, @"{Successful: use reserved keys in upsert query ");
              
            }

        }

        //provide key value meta in key, value
        public void VerifyQueryFormat8()
        {
            var methodName = "VerifyQueryFormat8";
            try
            {
                cache.Clear();
                var query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value,Meta) Values('product','Val')";
                QueryCommand queryCommand = new QueryCommand(query);
                cache.SearchService.ExecuteNonQuery(queryCommand);
                testResults.Add(methodName, ResultStatus.Failure);
                throw new Exception(@"{Failure: use reserved keys in upsert query");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Failure"))
                    _report.AddFailedTestCase(methodName, ex);
                else
                    _report.AddPassedTestCase(methodName, @"{Successful: provide key value meta in (key, value)");
            }

        }


        //provide key value  in key, value.meta
        public void VerifyQueryFormat9()
        {
            var methodName = "VerifyQueryFormat9";
            try
            {
                cache.Clear();
                var query = "Insert into Alachisoft.NCache.Sample.Data.Product (Key,Value) Values('product','phone',@data)";
                QueryCommand queryCommand = new QueryCommand(query);
                queryCommand.Parameters.Add("@data", @"{
                    'tags':['price','sale'],  
            'namedtags':[{ 'discount':'0.5','type':'$number$'},{ 'sale':'offer','type':'$text$'}],
            'priority':'high',
                ");
                cache.SearchService.ExecuteNonQuery(queryCommand);
                testResults.Add(methodName, ResultStatus.Failure);
                throw new Exception(@"{Failure: provide key value meta in (key, value)");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Failure"))
                    _report.AddFailedTestCase(methodName, ex);
                else
                    _report.AddPassedTestCase(methodName, @"{Successful: provide key value meta in (key, value)");
            }

        }
        //provide type name that is not indexed
        public void VerifyQueryFormat10()
        {
            var methodName = "VerifyQueryFormat10";
            try
            {
                cache.Clear();
                var query = "Insert into a.b.c (Key,Value) Values (product,@val)";

                QueryCommand queryCommand = new QueryCommand(query);
                var jobj = JsonConvert.SerializeObject(GetProduct());
                JsonObject jsonObject = new JsonObject(jobj);
                queryCommand.Parameters.Add("@val", jsonObject);
                cache.SearchService.ExecuteNonQuery(queryCommand);

                testResults.Add(methodName, ResultStatus.Success);
                _report.AddPassedTestCase(methodName, @"{Success:/provide type name that is not indexed");
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);              
            }

        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //                        verify inline query 
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        //run an inline insert query and then on same key run upsert key
        public void VerifyInlineQuery()
        {
            var methodName = "VerifyInlineQuery";
            try
            {
                cache.Clear();
                var insertquery = "Insert into Alachisoft.Ncache.Sample.Data.Product (key,value) values ('product1','\"phone\"')";
                QueryCommand qc = new QueryCommand(insertquery);
                var insResult = cache.SearchService.ExecuteNonQuery(qc);

                if (insResult > 0)
                {
                    try
                    {
                        var upsertquery = "Upsert into Alachisoft.Ncache.Sample.Data.Product (key,value) values ('product1','\"bottle\"')";
                        qc = new QueryCommand(upsertquery);
                        var upsertResult = cache.SearchService.ExecuteNonQuery(qc);
                        if (upsertResult > 0)
                        {

                            var result = cache.Get<string>("product1");
                            if (result == "bottle")
                            {
                                testResults.Add(methodName, ResultStatus.Success);
                                _report.AddPassedTestCase(methodName, "Successful:run an inline insert query and then on same key run upsert key, then verify updated item is returned. ");
                                count++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }

        }


        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //                        Verify Named Params support in Insert and Upsert
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        // insert using named params 
        public void NamedParams0()
        {
            var methodName = "NamedParams0";
            try
            {
                cache.Clear();
                var key = GetKey();
                var val = GetProduct();

                string query = "Insert Into Alachisoft.Ncache.Sample.Data.Product (key,value) values (@cachekey,@val)";
                QueryCommand qc = new QueryCommand(query);
                qc.Parameters.Add("@cachekey", key);
                qc.Parameters.Add("@val", val);

                var result = cache.SearchService.ExecuteNonQuery(qc);
                if (result > 0 && cache.Contains(key))
                {
                    testResults.Add(methodName, ResultStatus.Success);
                    _report.AddPassedTestCase(methodName, "Successful: insert using named params  ");
                    count++;
                }
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }

        }




        // upsert using named params 
        public void NamedParams1()
        {
            var methodName = "NamedParams1";
            try
            {
                cache.Clear();
                var key = GetKey();
                var val = GetProduct();

                string query = "Upsert Into Alachisoft.Ncache.Sample.Data.Product (key,value) values (@cachekey,@val)";
                QueryCommand qc = new QueryCommand(query);
                qc.Parameters.Add("@cachekey", key);
                qc.Parameters.Add("@val", val);

                var result = cache.SearchService.ExecuteNonQuery(qc);
                if (result > 0 && cache.Contains(key))
                {
                    testResults.Add(methodName, ResultStatus.Success);
                    _report.AddPassedTestCase(methodName, "Successful: upsert using named params  ");
                    count++;
                }
            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
                                
            }

        }


        //provide named param that is not in dictionary in upsert
        public void NamedParams2()
        {
            var methodName = "NamedParams2";
            try
            {
                cache.Clear();
                var key = GetKey();
                var val = GetProduct();

                string query = "Upsert Into Alachisoft.Ncache.Sample.Data.Product (key,value) values (@cachekey,@val)";
                QueryCommand qc = new QueryCommand(query);
                qc.Parameters.Add("@cache", key);
                qc.Parameters.Add("@val", val);

                var result = cache.SearchService.ExecuteNonQuery(qc);
                testResults.Add(methodName, ResultStatus.Failure);
                throw new Exception("Failure :provide named param that is not in dictionary in upsert");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Failure"))
                    _report.AddFailedTestCase(methodName, ex);
                else
                    _report.AddPassedTestCase(methodName, "Success : provide named param that is not in dictionary in upsert");
               
                count++;

                // throw;
            }

        }


        //provide named param that is not in dictionary in insert
        public void NamedParams3()
        {
            var methodName = "NamedParams3";
            try
            {
                cache.Clear();
                var key = GetKey();
                var val = GetProduct();

                string query = "Insert Into Alachisoft.Ncache.Sample.Data.Product (key,value) values (@cachekey,@val)";
                QueryCommand qc = new QueryCommand(query);
                qc.Parameters.Add("@cache", key);
                qc.Parameters.Add("@val", val);

                var result = cache.SearchService.ExecuteNonQuery(qc);
                testResults.Add(methodName, ResultStatus.Failure);
                throw new Exception("Failure : Provide named param that is not in dictionary in upsert");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Failure"))
                    _report.AddFailedTestCase(methodName, ex);
                else
                    _report.AddPassedTestCase(methodName, "Success : Provide named param that is not in dictionary in upsert");
                count++;

                // throw;
            }

        }

        //provide object in key to insert 
        public void NamedParams4()
        {
            var methodName = "NamedParams4";
            try
            {
                cache.Clear();
                var key = GetProduct();
                var val = GetProduct();

                string query = "Insert Into Alachisoft.Ncache.Sample.Data.Product (key,value) values (@cachekey,@val)";
                QueryCommand qc = new QueryCommand(query);
                qc.Parameters.Add("@cachekey", key);
                qc.Parameters.Add("@val", val);

                var result = cache.SearchService.ExecuteNonQuery(qc);
                testResults.Add(methodName, ResultStatus.Failure);
                throw new Exception("Failure: Provide object in key to insert ");

            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Failure"))
                    _report.AddFailedTestCase(methodName, ex);
                else
                    _report.AddPassedTestCase(methodName, "Successful: Provide object in key to insert ");
                count++;

                // throw;
            }

        }


        //                        Verify meta data
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        //Verify added tags and named tags through cache item
        public void VerifyMetaData0()
        {
            count++;
            var methodName = "VerifyMetaData0";
            try
            {
                cache.Clear();
                var key = GetKey();
                var val = GetProduct();

                string query = "Insert Into Alachisoft.Ncache.Sample.Data.Product (key,value,meta) values (@cachekey,@val,@data)";
                QueryCommand qc = new QueryCommand(query);
                qc.Parameters.Add("@cachekey", key);
                qc.Parameters.Add("@val", val);
                qc.Parameters.Add("@data", @"{
                                             'tags':['price','sale'],  
                                              'namedtags':[
                                                            { 'discount':'0.5','type':'decimal'},
                                                            { 'sale':'offer','type':'string'}
                                                          ]
                                               }");
                var result = cache.SearchService.ExecuteNonQuery(qc);
                var item = cache.GetCacheItem(key);
                var tags = item.Tags;
                var namedTags = item.NamedTags;

                for (int i = 0; i < tags.Length; i++)
                {
                    if (tags[i].TagName == "price" || tags[i].TagName == "sale")
                    {
                        continue;

                    }
                    else
                        throw new Exception("Invalid tag found");

                }
                if (namedTags.Count == 2)
                {
                    if (namedTags.Contains("discount") && namedTags.Contains("sale"))
                    {
                        _report.AddPassedTestCase(methodName, "Success:Verify added tags and named tags through cache item");
                        testResults.Add(methodName, ResultStatus.Success);
                    }

                }
                else
                {
                    testResults.Add(methodName, ResultStatus.Failure);
                    throw new Exception("Failure: Verify added tags and named tags through cache item ");

                }

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

               
                // throw;
            }

        }

        //verify meta by adding with tags and then getting through those tags.
        public void VerifyMetaData1()
        {
            count++;
            var methodName = "VerifyMetaData1";
            try
            {
                cache.Clear();
                var prodDict = GetDictionaryOfProductsToAdd(100);
                List<string> taggedKeys = new List<string>();

                for (int i = 0; i < 100; i++)
                {
                    var key = GetKey() + i;

                    if (i % 2 == 0)
                    {
                        string query = "Insert Into Alachisoft.Ncache.Sample.Data.Product (key,value,meta) values (@cachekey,@val,@data)";
                        QueryCommand qc = new QueryCommand(query);
                        qc.Parameters.Add("@cachekey", key);
                        qc.Parameters.Add("@val", prodDict[key]);
                        taggedKeys.Add(key);
                        qc.Parameters.Add("@data", @"{
                                             'tags':['Important Product','Imported Products']
                                              }");
                        var result = cache.SearchService.ExecuteNonQuery(qc);
                    }
                    else
                    {
                        string query = "Insert Into Alachisoft.Ncache.Sample.Data.Product (key,value) values (@cachekey,@val)";
                        QueryCommand qc = new QueryCommand(query);
                        qc.Parameters.Add("@cachekey", key);
                        qc.Parameters.Add("@val", prodDict[key]);
                        var result = cache.SearchService.ExecuteNonQuery(qc);

                    }
                    //var result = cache.SearchService.ExecuteNonQuery(qc);
                }

                var receivedKeys = cache.SearchService.GetKeysByTag("Important Product");
                if (receivedKeys.Count == taggedKeys.Count)
                {
                    foreach (var k in receivedKeys)
                    {
                        if (receivedKeys.Contains(k))
                        {
                            continue;

                        }
                        else
                        {
                            throw new Exception("recived keys dont contain added key.");
                        }


                    }
                    testResults.Add(methodName, ResultStatus.Success);
                    _report.AddPassedTestCase(methodName, "Success: verify meta by adding with tags and then getting through those tags.");
                }

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }

            // throw;
        }


        //verify meta by adding with named tags and then getting through those named tags.
        public void VerifyMetaData2()
        {
            count++;
            int reveived = 0;
            var methodName = "VerifyMetaData2";
            try
            {
                cache.Clear();
                var prodDict = GetDictionaryOfProductsToAdd(100);
                List<string> namedTagggedKeys = new List<string>();


                for (int i = 0; i < 100; i++)
                {
                    var key = GetKey() + i;

                    if (i % 2 == 0)
                    {
                        string query = "Insert Into Alachisoft.Ncache.Sample.Data.Product (key,value,meta) values (@cachekey,@val,@data)";
                        QueryCommand qc = new QueryCommand(query);
                        qc.Parameters.Add("@cachekey", key);
                        qc.Parameters.Add("@val", prodDict[key]);
                        namedTagggedKeys.Add(key);
                        qc.Parameters.Add("@data", @"{
                                              'namedtags':[{'FlashSaleDiscount':0.5,'type':'decimal'}],
                                              }");
                        var result = cache.SearchService.ExecuteNonQuery(qc);
                    }
                    else
                    {
                        string query = "Insert Into Alachisoft.Ncache.Sample.Data.Product (key,value) values (@cachekey,@val)";
                        QueryCommand qc = new QueryCommand(query);
                        qc.Parameters.Add("@cachekey", key);
                        qc.Parameters.Add("@val", prodDict[key]);

                        var result = cache.SearchService.ExecuteNonQuery(qc);
                    }

                }

                string searchQuery = "SELECT $Value$ FROM Alachisoft.Ncache.Sample.Data.Product WHERE FlashSaleDiscount = @discount ";
                QueryCommand searchQueryCommand = new QueryCommand(searchQuery);
                searchQueryCommand.Parameters.Add("@discount", Convert.ToDecimal(0.5));
                ICacheReader reader = cache.SearchService.ExecuteReader(searchQueryCommand);

                if (reader.FieldCount > 0)
                {
                    while (reader.Read())
                    {
                        reveived++;
                        Product result = reader.GetValue<Product>(1);

                    }
                    if (reveived == namedTagggedKeys.Count)
                    {
                        testResults.Add(methodName, ResultStatus.Success);
                        _report.AddPassedTestCase(methodName, "Success: verify meta by adding with named tags and then getting through those search.");
                    }

                }

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);
            }
            // throw;
        }


        //verify meta by adding with absolute expiration
        private void VerifyMetaData3()
        {
            count++;
            int reveived = 0;
            var methodName = "VerifyMetaData3";
            try
            {
                cache.Clear();
                var prodDict = GetDictionaryOfProductsToAdd(100);
                List<string> namedTagggedKeys = new List<string>();


                for (int i = 0; i < 100; i++)
                {
                    var key = GetKey() + i;//CacheKey_11


                    string query = "Insert Into Alachisoft.Ncache.Sample.Data.Product (key,value,meta) values (@cachekey,@val,@data)";
                    QueryCommand qc = new QueryCommand(query);
                    qc.Parameters.Add("@cachekey", key);
                    qc.Parameters.Add("@val", prodDict[key]);
                    namedTagggedKeys.Add(key);
                    qc.Parameters.Add("@data", @"{
                                              'namedtags':[{'FlashSaleDiscount':0.5,'type':'decimal'}],
                                              'expiration':{'type':'absolute','interval':2}
                                              }");
                    var result = cache.SearchService.ExecuteNonQuery(qc);

                }

                Console.WriteLine("Waiting for 20 Seconds to verify absolute expiration");
                Thread.Sleep(20000);

                long itemsInCache = cache.Count;
                if (itemsInCache > 0)
                {
                    testResults.Add(methodName, ResultStatus.Failure);
                    throw new Exception("Failure: verify meta by adding with absolute expiration");
                }
                else
                {
                    testResults.Add(methodName, ResultStatus.Success);
                    _report.AddPassedTestCase(methodName, "Success: verify meta by adding with absolute expiration");
                }

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }
            // throw;
        }


        //verify meta by adding with sliding expiration
        private void VerifyMetaData4()
        {
            count++;
            int reveived = 0;
            var methodName = "VerifyMetaData4";
            try
            {
                cache.Clear();
                var prodDict = GetDictionaryOfProductsToAdd(100);
                List<string> namedTagggedKeys = new List<string>();


                for (int i = 0; i < 100; i++)
                {
                    var key = GetKey() + i;


                    string query = "Insert Into Alachisoft.Ncache.Sample.Data.Product (key,value,meta) values (@cachekey,@val,@data)";
                    QueryCommand qc = new QueryCommand(query);
                    qc.Parameters.Add("@cachekey", key);
                    qc.Parameters.Add("@val", prodDict[key]);
                    namedTagggedKeys.Add(key);
                    qc.Parameters.Add("@data", @"{
                                              'namedtags':[{'FlashSaleDiscount':0.5,'type':'decimal'}],
                                               'expiration':{'type':'sliding','interval':'2'}
                                              }");
                    var result = cache.SearchService.ExecuteNonQuery(qc);

                }

                Console.WriteLine("Waiting for 20 seconds to verify Sliding Dependency");
                Thread.Sleep(20000);

                long itemsInCache = cache.Count;
                if (itemsInCache > 0)
                {
                    testResults.Add(methodName, ResultStatus.Failure);
                    throw new Exception("Failure: Add items with Sliding dependency");
                }
                else
                {
                    testResults.Add(methodName, ResultStatus.Success);
                    _report.AddPassedTestCase(methodName, "Success: Add items with Sliding dependency");

                }

            }
            catch (Exception ex)
            {
                _report.AddFailedTestCase(methodName, ex);

            }
            // throw;
        }

        //                        Helper Methods
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private string GetKey()
        {
            Random rnd = new Random();
            return "CacheKey_1";
        }

        private Product GetProduct()
        {

            return new Product() { Expirable = false, Manufacturers = new[] { "Alachisoft" }, Id = 1, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 357, Order = new Alachisoft.NCache.Sample.Data.Order { OrderID = 10, ShipCity = "rawalpindi" } };
        }

        private Dictionary<string, Product> GetDictionaryOfProductsToAdd(int number)
        {
            var productDict = new Dictionary<string, Product>();
            for (int i = 0; i < number; i++)
            {
                productDict.Add(GetKey() + i, GetProduct());

            }
            return productDict;
        }

        private void PopulateProductList()
        {
            productList.Add(new Product() { Id = 1, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 357 });
            productList.Add(new Product() { Id = 2, Time = DateTime.Now, Name = "Chang", ClassName = "Electronics", Category = "Meat", UnitPrice = 188 });
            productList.Add(new Product() { Id = 3, Time = DateTime.Now, Name = "Aniseed Syrup", ClassName = "Electronics", Category = "Beverages", UnitPrice = 258 });
            productList.Add(new Product() { Id = 4, Time = DateTime.Now, Name = "IKura", ClassName = "Electronics", Category = "Produce", UnitPrice = 50 });
            productList.Add(new Product() { Id = 5, Time = DateTime.Now, Name = "Tofu", ClassName = "Electronics", Category = "Seafood", UnitPrice = 78 });
            productList.Add(new Product() { Id = 6, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 357 });
            productList.Add(new Product() { Id = 7, Time = DateTime.Now, Name = "Chang", ClassName = "Electronics", Category = "Meat", UnitPrice = 188 });
            productList.Add(new Product() { Id = 8, Time = DateTime.Now, Name = "Aniseed Syrup", ClassName = "Electronics", Category = "Beverages", UnitPrice = 258 });
            productList.Add(new Product() { Id = 9, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 357 });
            productList.Add(new Product() { Id = 10, Time = DateTime.Now, Name = "Chang", ClassName = "Electronics", Category = "Meat", UnitPrice = 188 });
            productList.Add(new Product() { Id = 11, Time = DateTime.Now, Name = "Aniseed Syrup", ClassName = "Electronics", Category = "Beverages", UnitPrice = 258 });
            productList.Add(new Product() { Id = 12, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 357 });
            productList.Add(new Product() { Id = 13, Time = DateTime.Now, Name = "Chang", ClassName = "Electronics", Category = "Meat", UnitPrice = 188 });
            productList.Add(new Product() { Id = 14, Time = DateTime.Now, Name = "Aniseed Syrup", ClassName = "Electronics", Category = "Beverages", UnitPrice = 258 });
            productList.Add(new Product() { Id = 15, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 357 });
            productList.Add(new Product() { Id = 16, Time = DateTime.Now, Name = "Chang", ClassName = "Electronics", Category = "Meat", UnitPrice = 188 });
            productList.Add(new Product() { Id = 17, Time = DateTime.Now, Name = "Aniseed Syrup", ClassName = "Electronics", Category = "Beverages", UnitPrice = 258 });
            productList.Add(new Product() { Id = 18, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 357 });
            productList.Add(new Product() { Id = 19, Time = DateTime.Now, Name = "Chang", ClassName = "Electronics", Category = "Meat", UnitPrice = 188, Order = new Alachisoft.NCache.Sample.Data.Order { OrderID = 10, ShipCity = "rawalpindi" } });
            productList.Add(new Product() { Id = 20, Time = DateTime.Now, Name = "Aniseed Syrup", ClassName = "Electronics", Category = "Beverages", UnitPrice = 258, Order = new Alachisoft.NCache.Sample.Data.Order { OrderID = 10, ShipCity = "rawalpindi" } });
            productList.Add(new Product() { Id = 21, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 357, Order = new Alachisoft.NCache.Sample.Data.Order { OrderID = 10, ShipCity = "rawalpindi" } });
            productList.Add(new Product() { Id = 22, Time = DateTime.Now, Name = "Chang", ClassName = "Electronics", Category = "Meat", UnitPrice = 188, Order = new Alachisoft.NCache.Sample.Data.Order { OrderID = 10, ShipCity = "rawalpindi" } });
            productList.Add(new Product() { Id = 23, Time = DateTime.Now, Name = "Aniseed Syrup", ClassName = "Electronics", Category = "Beverages", UnitPrice = 258, Order = new Alachisoft.NCache.Sample.Data.Order { OrderID = 10, ShipCity = "rawalpindi" } });
            productList.Add(new Product() { Id = 24, Time = DateTime.Now, Name = "Chai", ClassName = "Electronics", Category = "Beverages", UnitPrice = 357, Order = new Alachisoft.NCache.Sample.Data.Order { OrderID = 10, ShipCity = "rawalpindi" } });
            productList.Add(new Product() { Id = 25, Time = DateTime.Now, Name = "Chang", ClassName = "Electronics", Category = "Meat", UnitPrice = 188, Order = new Alachisoft.NCache.Sample.Data.Order { OrderID = 10, ShipCity = "rawalpindi" } });
        }
    }

    public enum ResultStatus
    {
        Success,
        Failure
    }
}
