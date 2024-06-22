using Alachisoft.NCache.Client;
using Alachisoft.NCache.Runtime.Caching;
using Alachisoft.NCache.Runtime.JSON;
using Alachisoft.NCache.Sample.Data;
using Newtonsoft.Json;
using NUnit.Framework;
using QueriesTestApplication.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace QueriesTestApplication.TestCases
{
    public class SmallProduct 
    {
        private int _id;
        string _name;

        public string Name { get { return _name; } set { _name = value; } }
        public int Id { get => _id; set { _id = value; } }

        public SmallProduct() { }

        public SmallProduct(int id, string name)
        {
            this._id = id;
            this._name = name;
        }

       
    }

    class SmallProductList : IEnumerable<SmallProduct>
    {
        public IEnumerator<SmallProduct> GetEnumerator()
        {
            return new SmallProductEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    class SmallProductEnumerator : IEnumerator<SmallProduct>
    {
        int counter = 0;  
        public SmallProduct Current => new SmallProduct(counter, (++counter).ToString());

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            counter = 0;
        }

        public bool MoveNext()
        {
            return counter < 10;

        }

        public void Reset()
        {
            counter = 9;
        }
    }

    public class ExtendedProduct : SmallProduct
    {
        private string category;

        public string Categeory { get => category; set => category = value; }

        public ExtendedProduct() { }

        public ExtendedProduct(int id, string name, string cat) : base(id, name)
        {
            this.category = cat;
        }
    }

    public abstract class NCacheTestBase
    {
        protected ICache cache = null;
        protected string cacheName = Common.CacheName;

        [OneTimeSetUp]
        public void Init()
        {
            cache = CacheManager.GetCache(cacheName);
        }

        [OneTimeTearDown]
        public void Dispose()
        {
           cache?.Dispose();
        }

        public static QueryExecutionOptions CreateQueryExecutionOptions(bool returnFailed = false, WriteThruOptions options = null, TimeSpan time = default)
        {
            return new QueryExecutionOptions(returnFailed)
            {
                WriteThruOptions = options,
                Timeout = time,
            };
        }

        public static Product GetProduct(int id)
        {
            var product = new Product(true)
            {
                Id = id,
                Name = "IPhone",
                ProductName = "NCache-1",
                Category = "Cat-" + (id < 10000 ? "1": "N"),
                ClassName = "Product - ClassName",
                Order = new Order
                {
                    OrderID = id,
                    ShipAddress = "Canada",
                    ShipCity = "Vancouver",
                    OrderDate = DateTime.Now,
                },
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

            product.SetPrivateUnitsAvailable(199);

            return product;
        }

        public SmallProduct GetSmallProduct(int id)
        {
            return new SmallProduct(id, "Phone");
        }
    }
}
