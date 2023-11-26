using Alachisoft.NCache.Client;
using Alachisoft.NCache.Runtime.JSON;
using Alachisoft.NCache.Sample.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using QueriesTestApplication.Utils;

namespace QueriesTestApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //JsonHelper.TestArrayWithIndexing();

                Common.CacheName = "democache";

                JsonValue val = (JsonValue)Common.CacheName;

                object jVal = val.ToString();

                string jsonString = "[{\"name\":\"phone\",\"model\":\"p30PRO\"}]";
                JsonArray arr = new JsonArray(jsonString);


                var str = JsonConvert.SerializeObject(jVal);
                str = JsonConvert.SerializeObject(arr);


                RunUpdateQueriesTestsForJsonObject();



                //  RunInsertQueriesTests();
                // RunMetaVerificationTests();
                // RunInlineQueryTests();
                // RunInsertQueriesTests();
                // RunInlineQueryTestForUpdate();
                // RunUpdateQueriesTests();
                // InOperatorTests();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Execution failed due to following exception:" + ex.Message);
            }


        }
    
        private static void RunOnAllTopologies()
        {
            string[] CacheNames = new string[4] { "democache1", "Partition", "ReplicatedCache", "MirrorCache" };
           for (int i = 0; i < 4; i++)
           {
                Console.WriteLine($"\n\n ****  Running TestCases on  {CacheNames[i] } ****");

                Common.CacheName=CacheNames[i];               
                RunMetaVerificationTests();
                RunInlineQueryTests();
                RunInsertQueriesTests();
                RunInlineQueryTestForUpdate();
                RunUpdateQueriesTests();
                InOperatorTests();
                Console.WriteLine($"\n\n **** Testing done on {CacheNames[i] }. Press any key to continue ");
                Console.ReadKey();
                Console.Clear();
           }
           
        }
              
        private static void RunInlineQueryTests()
        {
            object[] parameters = null;
            InlineQueryTestsForInsertUpsert metaTest = new InlineQueryTestsForInsertUpsert();
            Common.PrintClassName("InlineQueryTestsForInsertUpsert");
            MethodInfo[] methodInfos = typeof(InlineQueryTestsForInsertUpsert).GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var mi in methodInfos)
            {
                try
                {
                    mi.Invoke(metaTest, parameters);
                }
                catch (Exception)
                {

                }

            }
            foreach (var val in metaTest.TestResults)
            {
                if (val.Value == ResultStatus.Failure)
                    Console.WriteLine(val.Key + ":Failed");

            }
            Console.WriteLine("Total methods called " + metaTest.TestResults.Count);
            Console.ReadLine();
        }

        #region -------------------------- MetaVerification Tests  --------------------------

        private static void RunMetaVerificationTests()
        {
            object[] parameters = null;
            MetaVerificationTests metaTest = new MetaVerificationTests();
            Common.PrintClassName("MetaVerificationTests");

            MethodInfo[] methodInfos = typeof(MetaVerificationTests).GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var mi in methodInfos)
            {
                try
                {
                    mi.Invoke(metaTest, parameters);
                }
                catch (Exception)
                {
                    
                }
            }
            foreach (var val in metaTest.TestResults)
            {
                if (val.Value == ResultStatus.Failure)
                    Console.WriteLine(val.Key + ":Failed");

            }
            Console.WriteLine("Total methods called " + metaTest.TestResults.Count);
            metaTest.Reprt.PrintReport();
            RunMetaVerificationTestsForJsonObj();
            Console.ReadLine();
        } 
        
        private static void RunMetaVerificationTestsForJsonObj()
        {
            object[] parameters = null;
            MetaVerificationTestForJsonObj metaTest = new MetaVerificationTestForJsonObj();
            Common.PrintClassName(nameof(MetaVerificationTestForJsonObj));

            MethodInfo[] methodInfos = typeof(MetaVerificationTestForJsonObj).GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var mi in methodInfos)
            {
                try
                {
                    mi.Invoke(metaTest, parameters);
                }
                catch (Exception)
                {


                }
            }
            foreach (var val in metaTest.TestResults)
            {
                if (val.Value == ResultStatus.Failure)
                    Console.WriteLine(val.Key + ":Failed");

            }

            metaTest.Report.PrintReport();  

            Console.WriteLine("Total methods called " + metaTest.TestResults.Count);
            Console.ReadLine();
        }

        #endregion

        #region  --------------------------  UpdateQuery Tests --------------------------

        private static void RunInlineQueryTestForUpdate()
        {
            object[] parameters = null;
            InlineQueryTestForUpdate inlineQuerytests = new InlineQueryTestForUpdate();
            Common.PrintClassName("InlineQueryTestForUpdate");

            // metaTest.Add1(Console.ReadLine());
            MethodInfo[] methodInfos = typeof(InlineQueryTestForUpdate).GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var mi in methodInfos)
            {
                try
                {
                    mi.Invoke(inlineQuerytests, parameters);
                }
                catch (Exception)
                {

                }

            }
            foreach (var val in inlineQuerytests.testResults)
            {
                if (val.Value == ResultStatus.Failure)
                    Console.WriteLine(val.Key + ":Failed");

            }

            inlineQuerytests.Report.PrintReport();

            Console.WriteLine("Total methods called " + inlineQuerytests.testResults.Count);
            Console.ReadLine();
        }

        private static void RunUpdateQueriesTests()
        {
            object[] parameters = null;
            UpdateQueriesTest updateQueriesTest = new UpdateQueriesTest(); 
            Common.PrintClassName("UpdateQueriesTest");
           
            MethodInfo[] methodInfos = typeof(UpdateQueriesTest).GetMethods(BindingFlags.NonPublic |BindingFlags.Public | BindingFlags.Instance);

            
          //  updateQueriesTest.SetArrayAtOutOBoundIndex();
            //updateQueriesTest.AddArrayAtSpecificIndex();
           // updateQueriesTest.SetOperationUsingJObject0();

            foreach (var mi in methodInfos)
            {
                try
                {
                
                    mi.Invoke(updateQueriesTest, parameters);
                }
                catch (Exception ex)
                {
                    ReportHelper.PrintError(mi.Name + ex.Message);
                }

                //var name= mi.Name;

            }
            foreach (var val in updateQueriesTest.TestResults)
            {
                if (val.Value == ResultStatus.Failure)
                    Console.WriteLine(val.Key + ":Failed");

            }
            Console.WriteLine("Total methods called " + updateQueriesTest.TestResults.Count);

            updateQueriesTest.Report.PrintReport();

            RunUpdateQueriesTestsForJsonObject();

            Console.ReadLine();
        }

        private static void RunUpdateQueriesTestsForJsonObject()
        {
            object[] parameters = null;
            UpdateQueriesTestForJsonObject updateQueriesTest = new UpdateQueriesTestForJsonObject();
            Common.PrintClassName(nameof(UpdateQueriesTestForJsonObject));

            MethodInfo[] methodInfos = typeof(UpdateQueriesTestForJsonObject).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            updateQueriesTest.AddAttribute();

            foreach (var mi in methodInfos)
            {
                try
                {

                    mi.Invoke(updateQueriesTest, parameters);
                }
                catch (Exception ex)
                {
                    ReportHelper.PrintError(mi.Name + ex.Message);

                }

               
            }
            foreach (var val in updateQueriesTest.TestResults)
            {
                if (val.Value == ResultStatus.Failure)
                    Console.WriteLine(val.Key + ":Failed");

            }
            Console.WriteLine("Total methods called " + updateQueriesTest.TestResults.Count);

            updateQueriesTest.Report.PrintReport();

            Console.ReadLine();
        }

        #endregion

        #region -------------------------- InsertQuery tests --------------------------
        private static void RunInsertQueriesTests()
        {
            object[] parameters = null;
            InsertQueriesTest insertQueriesTest = new InsertQueriesTest();
            Common.PrintClassName("InsertQueriesTest");

            

            MethodInfo[] methodInfos = typeof(InsertQueriesTest).GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var mi in methodInfos)
            {
               
                try
                {
                    mi.Invoke(insertQueriesTest, parameters);
                }
                catch (Exception ex)
                {
                    ReportHelper.PrintError(mi.Name + ex.Message);
                }

                //var name= mi.Name;

            }
            if (insertQueriesTest.testResults1.Count > 0)
            {
                foreach (var val in insertQueriesTest.testResults1)                   
                {                    
                    if (val.Value.ResultStatus == Common.Status.Failed)                    
                        Common.PrintFailedTestCaseMessage(val.Key, val.Value.Exception);                  
                }

                foreach (var val in insertQueriesTest.TestResults)
                {
                    if (val.Value == ResultStatus.Failure)
                        Console.WriteLine(val.Key + ":Failed");
                }
            }
            

            Console.WriteLine("Total methods called " + insertQueriesTest.TestResults.Count);

            insertQueriesTest.PrintReport();

            RunInsertQueriesTestsForJsonObject();
            Console.ReadLine();
        }


        private static void RunInsertQueriesTestsForJsonObject()
        {
            object[] parameters = null;
            InsertQueriesTestForJsonObj insertQueriesTest = new InsertQueriesTestForJsonObj();
            Common.PrintClassName(nameof(InsertQueriesTestForJsonObj));



            MethodInfo[] methodInfos = typeof(InsertQueriesTestForJsonObj).GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var mi in methodInfos)
            {

                try
                {
                    mi.Invoke(insertQueriesTest, parameters);
                }
                catch (Exception ex)
                {
                    ReportHelper.PrintError(mi.Name + ex.Message);
                }

               
            }
           
            insertQueriesTest.Report.PrintReport();
  
            Console.ReadLine();
        }


        #endregion

        private static void InOperatorTests()
        {
            object[] parameters = null;
            InOperatorTesting inOperatortTest = new InOperatorTesting();
            Common.PrintClassName("InOperatorTesting");

            MethodInfo[] methodInfos = typeof(InOperatorTesting).GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var mi in methodInfos)
            {
                try
                {
                    mi.Invoke(inOperatortTest, parameters);
                }
                catch (Exception)
                {


                }


            }
            foreach (var val in inOperatortTest.TestResults)
            {
                if (val.Value == ResultStatus.Failure)
                    Console.WriteLine(val.Key + ":Failed");
            }
            Console.WriteLine("Total methods called " + inOperatortTest.TestResults.Count);
            Console.ReadLine();
        }
    }

}

