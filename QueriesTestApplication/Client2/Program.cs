using Alachisoft.NCache.Client;
using Alachisoft.NCache.Common;
using Alachisoft.NCache.Runtime.Caching;
using Alachisoft.NCache.Sample.Data;
using System.Text.Json.Nodes;

namespace Client2
{
    internal class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();

            Test_WriteThru_Sample("write-thru-sample");

        }

        public static void Test_WriteThru_Sample(string cacheName)
        {
            ICache cache = CacheManager.GetCache(cacheName);
            
            PopulateCache(cache);

            var query = new QueryCommand("UPDATE Alachisoft.NCache.Sample.Data.Person SET City = @newcity WHERE City = @oldcity");

            query.Parameters.Add("@newcity", "San Francisco");
            query.Parameters.Add("@oldcity", "New York");

            WriteThruOptions writeThruOptions = new WriteThruOptions(WriteMode.WriteThru)
            {
                Hint = "UpdateCity"
            };

            QueryExecutionOptions queryExecutionOptions = new QueryExecutionOptions(writeThruOptions);

            QueryResult result = cache.QueryService.ExecuteNonQuery(query, queryExecutionOptions);

            Console.WriteLine($"{result.AffectedRows} rows affected");

        }

        private static void PopulateCache(ICache cache)
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
                    City = "New York",
                    Phone = "0123456789"
                },
                new Person()
                {
                    FirstName = "Elizabeth",
                    LastName = "Brown",
                    Age = 48,
                    City = "New York",
                    Phone = "0123456789"
                },

                new Person()
                {
                    FirstName = "Evan",
                    LastName = "Hunter",
                    Age = 37,
                    City = "San Francisco",
                    Phone = "0123456789"
                },

            };

            foreach (var person in people)
            {
                cache.Insert($"k-{Guid.NewGuid()}", new CacheItem(person), new WriteThruOptions(WriteMode.WriteThru));
            }
        }
    }
}
