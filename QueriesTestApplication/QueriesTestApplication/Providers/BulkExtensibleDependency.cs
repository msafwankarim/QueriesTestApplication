using Alachisoft.NCache.Caching.AutoExpiration;
using Alachisoft.NCache.Common.DataStructures;
using Alachisoft.NCache.Runtime.CustomDependencyProviders;
using Alachisoft.NCache.Runtime.Dependencies;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QueriesTestApplication.Providers
{
    public class BulkDependency : Alachisoft.NCache.Runtime.Dependencies.BulkExtensibleDependency
    {
        public override bool HasChanged => VerifyBulkDependency();

        public override void EvaluateBulk(IEnumerable<BulkExtensibleDependency> dependencies)
        {
            if (dependencies == null)
                return;

            foreach (var dependency in dependencies)
            {
                dependency.Expire();
            }
        }

        public override bool Initialize()
        {
            return true;
        }

        private bool VerifyBulkDependency()
        {
            string filePath = "C:\\dependencyFile.txt";
            File.AppendAllText(filePath, $"\n {nameof(VerifyBulkDependency)} => Has changed called for bulk dependency at {DateTime.Now}");
            return true;
        }
    }

    public class BulkDependencyPrvider : ICustomDependencyProvider
    {
        public Alachisoft.NCache.Runtime.Dependencies.ExtensibleDependency CreateDependency(string key, IDictionary<string, string> dependencyParameters)
        {
            return new BulkDependency();
        }

        public void Dispose()
        {
        }

        public void Init(IDictionary<string, string> parameters, string cacheName)
        {
        }
    }
}
