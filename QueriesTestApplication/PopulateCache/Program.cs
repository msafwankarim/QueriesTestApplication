using Alachisoft.NCache.Client;
using Alachisoft.NCache.Runtime.JSON;
using Alachisoft.NCache.Sample.Data;
using Newtonsoft.Json;
using System.Collections;
using System.Diagnostics;
using System.Text;

namespace PopulateCache
{
    internal class Program
    {
        struct CommandLineArgs
        {
            public string Method { get; set; } = "clear";
            public string Cache { get; set; } = "demoCache";

            public int ItemCount { get; set; } = 1000;

            public int Iterations { get; set; } = 1;

            public CommandLineArgs() { }
        }

        static CommandLineArgs Parse(string[] args)
        {
            CommandLineArgs commandLineArgs = new CommandLineArgs();

            if(args == null || args.Length == 0 ) 
                return commandLineArgs;

            int counter = 0;
            while( counter < args.Length )
            {
                var command = args[counter];

                if (string.IsNullOrEmpty(command))
                    continue;

                switch(command)
                {
                    case "-m":
                        commandLineArgs.Method = args[++counter];
                        break;

                    case "-c":
                        commandLineArgs.Cache = args[++counter];
                        break;

                    case "-ic":
                        commandLineArgs.ItemCount = int.Parse(args[++counter]);
                        break;

                    case "-i":
                        commandLineArgs.Iterations = int.Parse(args[++counter]);
                        break;

                    default:
                        throw new Exception("Unknown option " + command);
                }

                counter++;
            }

            return commandLineArgs;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("args: " + string.Join(", ", args));
            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();

            CommandLineArgs commandLineArgs = Parse(args);

            for (int i = 1; i <= commandLineArgs.Iterations; i++)
            {
                try
                {
                    RunProgram(commandLineArgs);
                } catch (Exception e)
                {
                    Console.WriteLine(e.ToString());

                    Console.WriteLine("Press enter to continue...");
                    Console.ReadLine();
                }
            }

        }

        private static void RunProgram(CommandLineArgs commandLineArgs)
        {
            ICache cache = CacheManager.GetCache(commandLineArgs.Cache);

            switch (commandLineArgs.Method)
            {
                case "clear":
                    cache.Clear();
                    break;

                case "insert":
                    InsertItems(cache, commandLineArgs.ItemCount);
                    break;

                case "update":
                    RunUpdateQuery(cache, commandLineArgs);
                    break;

            }
        }

        private static void RunUpdateQuery(ICache cache, CommandLineArgs args)
        {
            string query = $"UPDATE {typeof(Product).FullName} ADD OrderObj = @order WHERE Id < 10000";

            var queryCommand = new QueryCommand(query);
            var serialziedOrder = JsonConvert.SerializeObject(new Order() { OrderID = 24, ShipAddress = "Kings Landing", OrderDate = DateTime.Now, ShipCity = "Islamabad", ShipCountry = "Pakistan" });
            
            queryCommand.Parameters.Add("@order", serialziedOrder);

            var queryResult = cache.QueryService.ExecuteNonQuery(queryCommand, new QueryExecutionOptions(true));

            Console.WriteLine(query);
            Console.WriteLine();

            Console.WriteLine("Rows Affected: " + queryResult.AffectedRows);
            Console.WriteLine("Exception count: " + queryResult.FailedOperations.Count);

            foreach (var item in queryResult.FailedOperations)
            {
                Console.WriteLine("key: " + item.Key + "\nException: " + item.Value.ToString());
            }

            Debug.Assert(queryResult.AffectedRows == args.ItemCount);

            if (queryResult.AffectedRows != args.ItemCount)
            {
                Console.WriteLine($"Warning: queryResult.AffectedRows: {queryResult.AffectedRows} != args.ItemCount: {args.ItemCount}");
                Console.WriteLine("Verifying Cache");
                VerfiyCache(cache);
            }
        }

        private static void VerfiyCache(ICache cache)
        {
            IEnumerator cacheIterator = cache.GetEnumerator();

            while(cacheIterator.MoveNext())
            {
                DictionaryEntry entry = (DictionaryEntry)cacheIterator.Current;

                var item = cache.GetCacheItem((string)entry.Key);

                Debug.Assert(item != null, "key = " + entry.Key);

                var value = item.GetValue<JsonObject>();

                Debug.Assert(value.ContainsAttribute("OrderObj"), "key = " + entry.Key);
            }
        }

        private static void InsertItems(ICache cache, int itemCount)
        {
            for(int i = 0; i <  itemCount; i++) 
            {
                cache.Insert("k" + i, GetProduct(i));
            }
        }

        static object GetProduct(int id )
        {
            var product = new Product(true)
            {
                Id = id,
                Name = "IPhone",
                ProductName = "NCache-1",
                Category = "Phone - Category",
                ClassName = typeof(Product).Name + " - ClassName",
                Images = new Image[]
                {
                    new Image
                    {
                        ImageFormats = new ImageFormat[] {
                            new ImageFormat("JPEG"),
                            new ImageFormat("PNG"),
                            new ImageFormat("WEBP"),
                            new ImageFormat("GIF")
                        },

                        FileName = "product-image-1",
                        CreatedAt = DateTime.Now,
                        Data = new byte[] { 255, 216, 255, 217 }, // ffd8 ffd9
                        Width = 1920,
                        Height = 1080,
                    },
                    new Image
                    {
                        ImageFormats = new ImageFormat[] {
                            new ImageFormat("JPEG"),
                            new ImageFormat("PNG"),
                            new ImageFormat("WEBP"),
                            new ImageFormat("GIF")
                        },

                        FileName = "product-image-1",
                        CreatedAt = DateTime.Now,
                        Data = new byte[] { 89, 50, 78, 47 }, // png header
                        Width = 1920,
                        Height = 1080,
                    }
                },
                Manufacturers = new string[] { "Apple" },
                UnitPrice = 1_50_000,
                UnitsAvailable = 99,
                Customer = new Customer
                {
                    CompanyName = "Diyatech",
                    City = "Vancouver",
                    CustomerID = "23432",
                    Address = "Canada",
                    CustomerType = new string[] { "Normal" },
                    Country = "Canada",
                    ContactName = "John Doe",
                    ContactNo = "323461213",
                    PostalCode = "34563",
                    Fax = "343-324235-435"
                },
                Expirable = false,
                QuantityPerUnit = "99",
                Time = DateTime.Now
            };

            return product;
        }
    }
}
