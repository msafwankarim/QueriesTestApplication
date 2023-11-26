using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace QueriesTestApplication.Utils
{
    public static class ReportHelper
    {
        public static void ValidateDictionary(IDictionary dictionary)
        {
            if (dictionary.Count > 0)
            {
                foreach (var value in dictionary.Values)
                {
                    throw new Exception($"Failed operation returned. Message : {value.ToString()}");
                }
            }
        }

        public static void PrintInfo(string info)
        {
            if (info == null)
                return;

            PrintColorMessage(info, ConsoleColor.Green);
        }

        public static void PrintError(string error)
        { 
            if (error == null)
                return;

            PrintColorMessage(error, ConsoleColor.Red);
        }

        public static void PrintHeader(string header)
        {
            if (header == null)
                return;

            PrintColorMessage(header, ConsoleColor.Yellow);
        }

        private static void PrintColorMessage(string info, ConsoleColor color = ConsoleColor.Green)
        {           
            Console.ForegroundColor = color;
            Console.WriteLine(info);
            Console.ResetColor();
        }
    }
}
