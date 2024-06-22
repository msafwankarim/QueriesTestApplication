using Alachisoft.NCache.Client;
using Alachisoft.NCache.Licensing.DOM;
using Alachisoft.NCache.Runtime.Dependencies;
using Alachisoft.NCache.Runtime.JSON;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueriesTestApplication.TestCases.Dependencies
{
    public class DependencyTests : NCacheTestBase
    {
        [Test]
        public void OleDBInline()
        {
            cache.Clear();

            var product = GetProduct(3);
            var meta = "{\"dependency\":[{\"ole\":{\"connectionstring\":\"Server=20.200.20.56;Database=test;User Id=diyatech;Password=4Islamabad;Provider=SQLNCLI11\",\"dbcachekey\":\"dbo:Products\"}}]}";
            var queryCommand = new QueryCommand(@$"INSERT INTO {typeof(string).FullName} (Key, Value, Meta) VALUES ('k1', 'value', '{meta}')");

            cache.QueryService.ExecuteNonQuery(queryCommand);

            var echo = cache.GetCacheItem("k1");
        }

        [Test]
        public void OleDBJsonObjectInline()
        {
            cache.Clear();

            var product = GetProduct(3);
            var meta = "{\"dependency\":[{\"ole\":{\"connectionstring\":\"Server=20.200.20.56;Database=test;User Id=diyatech;Password=4Islamabad;Provider=SQLNCLI11\",\"dbcachekey\":\"dbo:Products\"}}]}";
            var json = JsonValueBase.Parse(meta);
            
            var queryCommand = new QueryCommand(@$"INSERT INTO {typeof(string).FullName} (Key, Value, Meta) VALUES ('k1', 'value', '{meta}')");

            cache.QueryService.ExecuteNonQuery(queryCommand);

            var echo = cache.GetCacheItem("k1");
        }

        [Test]
        public void OleDBNamedParameter()
        {
            cache.Clear();

            var product = GetProduct(3);
            var meta = "{\"dependency\":[{\"ole\":{\"connectionstring\":\"Server=20.200.20.56;Database=test;User Id=diyatech;Password=4Islamabad;Provider=SQLNCLI11\",\"dbcachekey\":\"dbo:Products\"}}]}";
            var json = JsonValueBase.Parse(meta);

            var queryCommand = new QueryCommand(@$"INSERT INTO {typeof(string).FullName} (Key, Value, Meta) VALUES ('k1', 'value', @meta)");

            queryCommand.Parameters.Add("@meta", json);

            cache.QueryService.ExecuteNonQuery(queryCommand);

            var echo = cache.GetCacheItem("k1");
        }

        bool IsPalindrome(string str)
        {
            // Remove white-space and convert to lowercase for case-insensitive comparison
            string cleanedString = str.ToLower().Replace(" ", "");

            // Check if the cleaned string is a palindrome
            for (int i = 0; i < cleanedString.Length / 2; i++)
            {
                if (cleanedString[i] != cleanedString[cleanedString.Length - i - 1])
                {
                    return false;
                }
            }
            return true;
        }

        [Test]
        public void TestPalinDrome()
        {
            Console.WriteLine(IsPalindrome("raxdar"));
            Console.WriteLine(IsPalindrome("level"));
            Console.WriteLine(IsPalindrome("3553"));
        }
    }
}
