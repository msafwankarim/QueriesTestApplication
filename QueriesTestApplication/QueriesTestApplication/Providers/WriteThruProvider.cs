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
using System.Reflection;
using System.Text;

namespace QueriesTestApplication.Providers
{
    public class WriteThruProvider : IWriteThruProvider
    {
        string filePath = "C:\\\\writeThruLogs.txt";

        public void Dispose()
        {
            File.AppendAllText(filePath, $"\n Dispose => Method called at . {DateTime.Now}");
        }

        public void Init(IDictionary parameters, string cacheId)
        {
            File.AppendAllText(filePath, $"\n Init => Method called at . {DateTime.Now}");
        }

        public OperationResult WriteToDataSource(WriteOperation operation)
        {
            File.AppendAllText(filePath, $"\n {MethodBase.GetCurrentMethod().Name} => Method called at . {DateTime.Now}");
            return null;
        }

        public ICollection<OperationResult> WriteToDataSource(ICollection<WriteOperation> operations)
        {
            File.AppendAllText(filePath, $"\n {MethodBase.GetCurrentMethod().Name} => Method called at . {DateTime.Now}");
            return null;
        }

        public ICollection<OperationResult> WriteToDataSource(ICollection<DataTypeWriteOperation> dataTypeWriteOperations)
        {
           File.AppendAllText(filePath, $"\n {MethodBase.GetCurrentMethod().Name} => Method called at . {DateTime.Now}");

            return null;
        }

        public OperationResult WriteToDataSource(QueryWriteOperation queryWriteOperation)
        {
           File.AppendAllText(filePath, $"\n {MethodBase.GetCurrentMethod().Name} => Method called at . {DateTime.Now}");
            return null;
        }

        public ICollection<OperationResult> WriteToDataSource(ICollection<QueryWriteOperation> queryWriteOperations)
        {
           File.AppendAllText(filePath, $"\n {MethodBase.GetCurrentMethod().Name} => Method called at . {DateTime.Now}");
            return null;
        }
    }

}
