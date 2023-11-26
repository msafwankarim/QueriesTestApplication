using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace QueriesTestApplication.Utils
{
    public static class ResourceMessages
    {
        public static string ReportHeader = "\n********************** Report for class {0} ********************** \n";

        public static string FailedTestCaseMessage = "{0}) Test Case {1} FAILED due to Reason {2}";
        public static string FailedTestCaseHeader = "\nFAILED Cases ({1}) :\n";
        public static string PassesTestCaseHeader = "\nPASSES Cases ({1}) :\n";
        public static string PassedTestCaseMessage = "{0}) Test Case {1} PASSED. Info : {2}";

        public static string GotNullObject = "Object obtained from cache is null.";

    }
}
