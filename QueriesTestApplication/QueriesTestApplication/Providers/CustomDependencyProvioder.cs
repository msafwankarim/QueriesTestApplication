using Alachisoft.NCache.Caching.AutoExpiration;
using Alachisoft.NCache.Runtime.CustomDependencyProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QueriesTestApplication.Providers
{
    [Serializable]
    public class Dependency : Alachisoft.NCache.Runtime.Dependencies.ExtensibleDependency
    {
        public override bool HasChanged => VerifyCustomDependency();

        public override bool Initialize()
        {
            return true;            
        }

        private bool VerifyCustomDependency()
        {
            string filePath = "C:\\dependencyFile.txt";
            File.AppendAllText(filePath, $"\n {nameof(VerifyCustomDependency)} => Has changed called for custom dependency at {DateTime.Now}");
            return true;
        }
    }

    public class CustomDependencyPrvider : ICustomDependencyProvider
    {
        public Alachisoft.NCache.Runtime.Dependencies.ExtensibleDependency CreateDependency(string key, IDictionary<string, string> dependencyParameters)
        {
            return new Dependency();
        }

        public void Dispose()
        {
        }

        public void Init(IDictionary<string, string> parameters, string cacheName)
        {
        }
    }
}
