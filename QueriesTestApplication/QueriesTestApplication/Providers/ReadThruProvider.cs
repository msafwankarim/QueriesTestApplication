using Alachisoft.NCache.Caching.AutoExpiration;
using Alachisoft.NCache.Client;
using Alachisoft.NCache.Common.DataStructures;
using Alachisoft.NCache.Runtime.Caching;
using Alachisoft.NCache.Runtime.CustomDependencyProviders;
using Alachisoft.NCache.Runtime.DatasourceProviders;
using Alachisoft.NCache.Runtime.Dependencies;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QueriesTestApplication.Providers
{
    public class ReadThruProvider : IReadThruProvider
    {

        public void Dispose()
        {
            string filePath = "C:\\\\readThruLogs.txt";
            File.AppendAllText(filePath, $"\n Dispose => Method called at . {DateTime.Now}");
        }

        public void Init(IDictionary<string, string> parameters, string cacheName)
        {
            string filePath = "C:\\\\readThruLogs.txt";
            File.AppendAllText(filePath, $"\n Init => Method called at . {DateTime.Now}");
        }

        public ProviderDataTypeItem<IEnumerable> LoadDataTypeFromSource(string key, DistributedDataType dataType)
        {
            string filePath = "C:\\\\readThruLogs.txt";
            File.AppendAllText(filePath, $"\n LoadDataTypeFromSource(string key, DistributedDataType dataType) => Method called at . {DateTime.Now}");
            return null;
        }

        public ProviderCacheItem LoadFromSource(string key)
        {
            return new ProviderCacheItem(new TestProduct("_name",  "NAME"));
        }

        public IDictionary<string, ProviderCacheItem> LoadFromSource(ICollection<string> keys)
        {
            string filePath = "C:\\\\readThruLogs.txt";
            File.AppendAllText(filePath, $"\n LoadFromSource(ICollection<string> keys) => Method called at . {DateTime.Now}");
            return null;
        }
    }


    class TestProduct
    {
        public string Name { get; set; } = "Default Name";

        private string _name = "default _name";

        public TestProduct() { }

        public TestProduct(string name, string nameProp)
        {
            _name = name;
            Name = nameProp;
        }
    }

}
