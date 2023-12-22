using Alachisoft.NCache.Client;
using Alachisoft.NCache.Runtime.JSON;
using Alachisoft.NCache.Sample.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using QueriesTestApplication.Utils;
using QueriesTestApplication.VerifyEscapeSequences;
using System.Diagnostics.CodeAnalysis;
using System.Data.SqlClient;
using Novell.Directory.Ldap.Asn1;
using System.Text;

namespace QueriesTestApplication
{
    class Program
    {
        public static bool OneGo { get; set; } = true;
        public static bool DonotSkipAnyCase { get; set; } = true;

        static void Main(string[] args)
        {
            try
            {
                // provider names
                // CustomDependeny => custom , bulkDependencyProvider , notifyDependencyProvider , read , write

                Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
                Console.BufferHeight = short.MaxValue - 10;

                // JsonHelper.TestArrayWithIndexing();
                // TempTests();

                Common.CacheName = "replicated";

                 //new MetaVerificationTestForJsonObj().TestTagMetadataWithByAnyTag();
                // new MetaVerificationTests().VerifyDbDependency();
                // new UpdateQueriesTestForJsonObject().AddObjectAtIndexThatDoesnotExist();

                //RunOnAllTopologies();
                RunAllCases();

                ReportHelper.PrintHeader("\n\n ------------------------------------------------------- ALL DONE -----------------------------------------------");


            }
            catch (Exception ex)
            {
                Console.WriteLine("Execution failed due to following exception:" + ex.Message);
            }


        }

        private static void RunAllCases()
        {
            RunMetaVerificationTests();
            RunInlineQueryTestForUpdate0();
            RunInsertQueriesTests();
            UpsertQueriesWithMeta();
            InOperatorTests();
            EscapeSequencesVerifier();
        }


        private static void RunOnAllTopologies()
        {
            string[] CacheNames = new string[4] { "por", "partition", "replicated", "mirror" };
            for (int i = 0; i < 4; i++)
            {
                Common.CacheName = CacheNames[i];

                string message = $"Running TestCases on  {Common.CacheName}";

                ReportHelper.PrintHeader($"\n{GetSymbol(message.Length,'=')}");
                ReportHelper.PrintHeader($"\n{message.ToUpper()}");
                ReportHelper.PrintHeader($"\n{GetSymbol(message.Length, '=')}");


                RunAllCases();


                ReportHelper.PrintInfo($"\n **** Testing done on {Common.CacheName } ");
                ReportHelper.PrintHeader($"\n\n{GetSymbol(1000)}");

            }

        }



        #region -------------------------- MetaVerification Tests  --------------------------

        private static void RunMetaVerificationTests()
        {
            object[] parameters = null;
            MetaVerificationTests metaTest = new MetaVerificationTests();

            MethodInfo[] methodInfos = typeof(MetaVerificationTests).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);


            foreach (var mi in methodInfos)
            {
                try
                {
                    mi.Invoke(metaTest, parameters);
                }
                catch (Exception ex)
                {
                    ReportHelper.PrintError(mi.Name + ex.Message);
                }
            }

            foreach (var val in metaTest.TestResults)
            {
                if (val.Value == ResultStatus.Failure)
                    Console.WriteLine(val.Key + ":Failed");

            }

            metaTest.Reprt.PrintReport();
            PromptInputIfNeeded();
            RunMetaVerificationTestsForJsonObj();

        }

        private static void RunMetaVerificationTestsForJsonObj()
        {
            object[] parameters = null;
            MetaVerificationTestForJsonObj metaTest = new MetaVerificationTestForJsonObj();

            MethodInfo[] methodInfos = typeof(MetaVerificationTestForJsonObj).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var mi in methodInfos)
            {
                try
                {
                    if (!DonotSkipAnyCase)
                    {
                        if (mi.Name.Contains("Expiration"))
                            continue;

                    }

                    mi.Invoke(metaTest, parameters);
                }
                catch (Exception ex)
                {
                    ReportHelper.PrintError(mi.Name + ex.Message);
                }
            }


            metaTest.Report.PrintReport();
            PromptInputIfNeeded();
            MetaVerificationInUpdateQuery();
        }

        private static void MetaVerificationInUpdateQuery()
        {
            object[] parameters = null;
            MetaVerificationInUpdateQuery metaTest = new MetaVerificationInUpdateQuery();

            MethodInfo[] methodInfos = typeof(MetaVerificationInUpdateQuery).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var mi in methodInfos)
            {
                try
                {
                    mi.Invoke(metaTest, parameters);
                }
                catch (Exception ex)
                {

                    ReportHelper.PrintError(mi.Name + ex.Message);

                }
            }

            metaTest.Report.PrintReport();
            PromptInputIfNeeded();

            MetaVerificationInInlineQuery();
        }

        private static void MetaVerificationInInlineQuery()
        {
            object[] parameters = null;
            MetaVerificationInInlineQuery inlineMetaVerification = new MetaVerificationInInlineQuery();

            MethodInfo[] methodInfos = typeof(MetaVerificationInInlineQuery).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var mi in methodInfos)
            {
                try
                {
                    mi.Invoke(inlineMetaVerification, parameters);
                }
                catch (Exception ex)
                {
                    ReportHelper.PrintError(mi.Name + ex.Message);
                }

            }

            inlineMetaVerification.Report.PrintReport();

            PromptInputIfNeeded();
        }

        #endregion


        #region  --------------------------  UpdateQuery Tests --------------------------

        private static void RunInlineQueryTestForUpdate0()
        {
            object[] parameters = null;
            InlinePartialUpdates inlineQuerytests = new InlinePartialUpdates();


            MethodInfo[] methodInfos = typeof(InlinePartialUpdates).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var mi in methodInfos)
            {
                try
                {
                    if (mi.Name == "PopulateCache" || mi.Name == "PopulateCacheAndGetKeys" || mi.Name == "PopulateCacheWithMeta" || mi.Name == "ExpandProductList")
                        continue;

                    mi.Invoke(inlineQuerytests, parameters);
                }
                catch (Exception ex)
                {
                    ReportHelper.PrintError(mi.Name + ex.Message);

                }

            }

            inlineQuerytests.Report.PrintReport();

            PromptInputIfNeeded();

            RunInlineQueryTestForUpdate();
        }

        private static void RunInlineQueryTestForUpdate()
        {
            object[] parameters = null;
            InlineQueryTestForUpdate inlineQuerytests = new InlineQueryTestForUpdate();

            // metaTest.Add1(Console.ReadLine());
            MethodInfo[] methodInfos = typeof(InlineQueryTestForUpdate).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var mi in methodInfos)
            {
                try
                {
                    mi.Invoke(inlineQuerytests, parameters);
                }
                catch (Exception ex)
                {
                    ReportHelper.PrintError(mi.Name + ex.Message);
                }

            }
            foreach (var val in inlineQuerytests.testResults)
            {
                if (val.Value == ResultStatus.Failure)
                    Console.WriteLine(val.Key + ":Failed");

            }

            inlineQuerytests.Report.PrintReport();

            PromptInputIfNeeded();

            RunUpdateQueriesTestsForJsonObject();



        }



        private static void RunUpdateQueriesTestsForJsonObject()
        {
            object[] parameters = null;
            UpdateQueriesTestForJsonObject updateQueriesTest = new UpdateQueriesTestForJsonObject();

            MethodInfo[] methodInfos = typeof(UpdateQueriesTestForJsonObject).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);


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

            updateQueriesTest.Report.PrintReport();
            PromptInputIfNeeded();

        }

        #endregion


        #region -------------------------- InsertQuery tests --------------------------

        private static void RunInsertQueriesTests()
        {
            object[] parameters = null;
            InsertQueriesTest insertQueriesTest = new InsertQueriesTest();


            MethodInfo[] methodInfos = typeof(InsertQueriesTest).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
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



            insertQueriesTest.Report.PrintReport();

            RunInsertQueriesTestsForJsonObject();
            PromptInputIfNeeded();
        }


        private static void RunInsertQueriesTestsForJsonObject()
        {
            object[] parameters = null;
            InsertQueriesTestForJsonObj insertQueriesTest = new InsertQueriesTestForJsonObj();

            MethodInfo[] methodInfos = typeof(InsertQueriesTestForJsonObj).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            // insertQueriesTest.AddJsonObject(); todo needs to be fixed

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

            PromptInputIfNeeded();
        }


        #endregion


        #region -------------------------- Upsert tests --------------------------

        private static void UpsertQueriesWithMeta()
        {
            object[] parameters = null;
            UpsertQueriesWithMeta upsertQueriesTest = new UpsertQueriesWithMeta();

            MethodInfo[] methodInfos = typeof(UpsertQueriesWithMeta).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var mi in methodInfos)
            {

                try
                {
                    mi.Invoke(upsertQueriesTest, parameters);
                }
                catch (Exception ex)
                {
                    ReportHelper.PrintError(mi.Name + ex.Message);
                }


            }

            upsertQueriesTest.Report.PrintReport();

            PromptInputIfNeeded();
            RunInlineQueryTests();

        }

        private static void RunInlineQueryTests()
        {
            object[] parameters = null;
            InlineQueryTestsForInsertUpsert metaTest = new InlineQueryTestsForInsertUpsert();
            MethodInfo[] methodInfos = typeof(InlineQueryTestsForInsertUpsert).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);


            foreach (var mi in methodInfos)
            {
                try
                {
                    mi.Invoke(metaTest, parameters);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("unhandeled exception" + $"{ex.Message}");

                }

            }

            foreach (var val in metaTest.TestResults)
            {
                if (val.Value == ResultStatus.Failure)
                    Console.WriteLine(val.Key + ":Failed");

            }
        }


        #endregion


        private static void InOperatorTests()
        {
            object[] parameters = null;
            InOperatorTesting inOperatortTest = new InOperatorTesting();
            Common.PrintClassName("InOperatorTesting");

            MethodInfo[] methodInfos = typeof(InOperatorTesting).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var mi in methodInfos)
            {
                try
                {
                    mi.Invoke(inOperatortTest, parameters);
                }
                catch (Exception ex)
                {


                }


            }
            foreach (var val in inOperatortTest.TestResults)
            {
                if (val.Value == ResultStatus.Failure)
                    Console.WriteLine(val.Key + ":Failed");
            }

            Console.WriteLine("Total methods called " + inOperatortTest.TestResults.Count);
            PromptInputIfNeeded();
        }

        private static void TempTests()
        {
            JsonValue val = (JsonValue)Common.CacheName;
            var str = JsonConvert.SerializeObject(val); // datatype automatically added on serializing JsonValue

            string city = "rawalpindi\\Islamabad";

            JsonObject jsonObject = new JsonObject() { };
            jsonObject.AddAttribute(nameof(city), city);

            string jsonString = jsonObject.ToString(); // jsonString is made by jsonObject, but it is not valid because of one slash in city
            try
            {
                var token = JToken.Parse(jsonString);

            }
            catch (Exception ex)
            {
                // when nwetonsoft will serialize it will automatically add slashes add will remove them in deserialization
                jsonString = JsonConvert.SerializeObject(city).ToString();
                var token = JToken.Parse(jsonString);
            }


            JToken one = (JToken)"1";
            bool res = one.Type is JTokenType.String;


            var startAfterTime = DateTime.Now + TimeSpan.FromSeconds(120);




            object obj = @"{ 
                                'tags':['price','sale'],  
                                'namedtags':[
                                               { 'discount':'0.5','type':'decimal'},
                                               { 'sale':'offer','type':'string'}
                                            ]
                             }";

            obj = JsonValueBase.Parse((string)obj);

            if (obj is JsonObject)
            {

            }


        }

        private static void EscapeSequencesVerifier()
        {
            object[] parameters = null;
            BackslashVerifier slashVerifier = new BackslashVerifier();
            Common.PrintClassName(nameof(BackslashVerifier));

            MethodInfo[] methodInfos = typeof(BackslashVerifier).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var mi in methodInfos)
            {
                try
                {
                    mi.Invoke(slashVerifier, parameters);
                }
                catch (Exception)
                {

                }

            }
            slashVerifier.Report.PrintReport();
            PromptInputIfNeeded();
        }

        private static void PromptInputIfNeeded()
        {
            ReportHelper.PrintHeader("\n--------------------------------------------------------------------------------------------------------------------------\n");


            if (!OneGo)
                Console.ReadLine();
        }

        public static string GetSymbol(int length,char symbol = '*')
        {
            var builder = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                builder.Append(symbol);
            }

            return builder.ToString();

        }
    }

}

