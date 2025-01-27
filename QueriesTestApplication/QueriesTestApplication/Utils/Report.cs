﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace QueriesTestApplication.Utils
{
    public class Report
    {
        public bool RemoveStackTrace = true;

        IDictionary<string, object> _failedTestCases;
        IDictionary<string, string> _passedTestCases;

        public static List<string> _expectedFailTestCases = new List<string>()
        {
            "TestJsonArray", // test case fails because server side has serialized array which means that the objects inside the array are wrapped i.e $value {}
            nameof(MetaVerificationTestForJsonObj.VerifyNotifyExtensibleDependency), // notification deopendency requires cosmos db
            nameof(MetaVerificationTestForJsonObj.VerifySyncDependency), // cache sync dependency not supported
            nameof(MetaVerificationTestForJsonObj.VerifyCacheItemVersion), // cache sync dependency not supported
        };

        bool _printExpectedFailedCases = false;

        public bool PrintExpectedFailedCases { get => _printExpectedFailedCases; }


        string _className;

        public Report(string className)
        {
            _className = className;
            _failedTestCases = new Dictionary<string, object>();
            _passedTestCases = new Dictionary<string, string>();
        }
        public void AddPassedTestCase(string testCaseName, string message)
        {
            _passedTestCases.Add(testCaseName, message);
        }

        public void AddFailedTestCase(string testCaseName, Exception exception)
        {
            _failedTestCases.Add(testCaseName, exception.Message);
        }

        public void PrintFailedTestCases()
        {
            ReportHelper.PrintHeader(string.Format(ResourceMessages.FailedTestCaseHeader, _className, _failedTestCases.Count)); ;

            if (_failedTestCases.Count == 0)
                return;

            int serialNumber = 1;

            foreach (var testCase in _failedTestCases)
            {

                if (_expectedFailTestCases.Contains(testCase.Key) && !_printExpectedFailedCases)
                    continue;

                string msg = testCase.Value.ToString();

                if (RemoveStackTrace)
                    RemoveStackTraceFromMessage(ref msg);

                ReportHelper.PrintError(string.Format(ResourceMessages.FailedTestCaseMessage, serialNumber++, testCase.Key, ""));
                Console.Write(msg);
            }
        }

        public void PrintPassedTestCases()
        {
            ReportHelper.PrintHeader(string.Format(ResourceMessages.PassesTestCaseHeader, _className, _passedTestCases.Count)); ;

            if (_passedTestCases.Count == 0)
                return;
            int serialNumber = 1;
            foreach (var testCase in _passedTestCases)
            {
                ReportHelper.PrintInfo(string.Format(ResourceMessages.PassedTestCaseMessage, serialNumber++, testCase.Key, testCase.Value));
            }
        }

        public void PrintReport()
        {
            ReportHelper.PrintHeader(string.Format(ResourceMessages.ReportHeader, _className)); ;

            PrintFailedTestCases();
            PrintPassedTestCases();
        }

        private void RemoveStackTraceFromMessage(ref string value)
        {
            if (value == null)
                value = "";

            value = value.Split("at Alachisoft.NCache")[0];

        }

    }
}
