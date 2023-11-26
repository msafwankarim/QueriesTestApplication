using System;
using System.Collections.Generic;
using System.Text;

namespace QueriesTestApplication
{
     public class TestResult
     {
        public TestResult() { }

        public Common.Status ResultStatus;
        public Exception Exception { get; set; }
       
    }
}
