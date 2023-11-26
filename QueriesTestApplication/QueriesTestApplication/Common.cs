using System;
using System.Collections.Generic;
using System.Text;

namespace QueriesTestApplication
{
    public class Common
    {        
        public  static string CacheName { get; set; }
        public static Dictionary<string, Exception> Exceptions { get; set; }
        public enum Status
        {
            Passed,
            Failed
        }

        public static void PrintClassName(string className)
        {
            Console.WriteLine($"\n\n***********************************  {className}  ***********************************\n\n");
        }

        public static void PrintFailedTestCasesStartedMessages()
        {
            Console.WriteLine($"\n***********************************  {"Failed Cases"}  ***********************************\n");
        }

        public static void PrintFailedTestCaseMessage(string TestCaseName, Exception exception) {
            Console.WriteLine($"FAILURE : TestCase {TestCaseName} Failed with Exception {exception.Message}");
        }
        
    }
}
