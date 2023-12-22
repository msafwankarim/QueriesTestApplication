using Alachisoft.NCache.Caching.AutoExpiration;
using Alachisoft.NCache.Runtime.CustomDependencyProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QueriesTestApplication.Providers
{
    [Serializable]
    public class NotifyDependency : Alachisoft.NCache.Runtime.Dependencies.NotifyExtensibleDependency
    {

        public override bool Initialize()
        {
            return true;            
        }

        private void InvokeDependency()
        {
            DependencyChanged.Invoke(this);           
        }
    }

    public class NotifyDependencyPrvider : ICustomDependencyProvider
    {
        public Alachisoft.NCache.Runtime.Dependencies.ExtensibleDependency CreateDependency(string key, IDictionary<string, string> dependencyParameters)
        {
            return new NotifyDependency();
        }

        public void Dispose()
        {
        }

        public void Init(IDictionary<string, string> parameters, string cacheName)
        {
        }
    }
}
