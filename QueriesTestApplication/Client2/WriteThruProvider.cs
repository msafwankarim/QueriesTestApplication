using Alachisoft.NCache.Runtime.Caching;
using Alachisoft.NCache.Runtime.DatasourceProviders;
using Alachisoft.NCache.Sample.Data;
using System.Collections;
using System.Data.SqlClient;

namespace QueriesTestApplication.Providers
{
    public class WriteThruProvider : IWriteThruProvider
    {
        // Object used to communicate with the Data source.
        SqlConnection _connection = null;

        // Perform tasks like allocating resources or acquiring connections
        public void Init(IDictionary<string, string> parameters, string cacheName)
        {
            try
            {
                string connString = GetConnectionString(parameters);
                if (!string.IsNullOrEmpty(connString))
                {
                    _connection = new SqlConnection(connString);
                    _connection.Open();
                }
            }
            catch (Exception ex)
            {
                // Handle exception
            }
        }

        // Perform tasks associated with freeing, releasing, or resetting resources.
        public void Dispose()
        {
            _connection?.Dispose();
        }

        public ICollection<OperationResult> WriteToDataSource(ICollection<WriteOperation> operations)
        {
            var operationResult = new List<OperationResult>();
            foreach (WriteOperation operation in operations)
            {
                ProviderCacheItem cacheItem = operation.ProviderItem;
                Product product = cacheItem.GetValue<Product>();

                switch (operation.OperationType)
                {
                    case WriteOperationType.Add:
                        // Insert logic for any Add operation
                        break;
                    case WriteOperationType.Delete:
                        // Insert logic for any Delete operation
                        break;
                    case WriteOperationType.Update:
                        // Insert logic for any Update operation
                        break;
                }
                // Write Thru operation status can be set according to the result
                operationResult.Add(new OperationResult(operation, OperationResult.Status.Success));
            }
            return operationResult;
        }

        public ICollection<OperationResult> WriteToDataSource(ICollection<DataTypeWriteOperation> operations)
        {
            var operationResult = new List<OperationResult>();
            foreach (DataTypeWriteOperation operation in operations)
            {
                var list = new List<Product>();
                ProviderDataTypeItem<object> cacheItem = operation.ProviderItem;
                Product product = (Product)cacheItem.Data;

                switch (operation.OperationType)
                {
                    case DatastructureOperationType.CreateDataType:
                        // Insert logic for creating a new List
                        IList myList = new List<Product>();
                        myList.Add(product.ProductID);
                        break;
                    case DatastructureOperationType.AddToDataType:
                        // Insert logic for any Add operation
                        list.Add(product);
                        break;
                    case DatastructureOperationType.DeleteFromDataType:
                        // Insert logic for any Remove operation
                        list.Remove(product);
                        break;
                    case DatastructureOperationType.UpdateDataType:
                        // Insert logic for any Update operation
                        list.Insert(0, product);
                        break;
                }
                // Write Thru operation status can be set according to the result.
                operationResult.Add(new OperationResult(operation, OperationResult.Status.Success));
            }
            return operationResult;
        }

        //Responsible for write operations on data source
        public OperationResult WriteToDataSource(WriteOperation operation)
        {
            ProviderCacheItem cacheItem = operation.ProviderItem;
            Product product = cacheItem.GetValue<Product>();

            switch (operation.OperationType)
            {
                case WriteOperationType.Add:
                    // Insert logic for any Add operation
                    break;
                case WriteOperationType.Delete:
                    // Insert logic for any Delete operation
                    break;
                case WriteOperationType.Update:
                    // Insert logic for any Update operation
                    break;
            }
            // Write Thru operation status can be set according to the result.
            return new OperationResult(operation, OperationResult.Status.Success);
        }


        public OperationResult WriteToDataSource(QueryWriteOperation queryWriteOperation)
        {
            string query = string.Empty;

            switch (queryWriteOperation.Hint)
            {
                case "UpdateCity":
                    query = GenerateUpdateCityQuery(queryWriteOperation.Query);
                    break;

                case "UpdatePhone":
                    query = GenerateUpdatePhoneQuery(queryWriteOperation.Query);
                    break;
            } 

            SqlCommand command = new SqlCommand(query, _connection);

            if(queryWriteOperation.Parameters != null)
            {
                foreach (var param in queryWriteOperation.Parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }
            }

            int affectedRows = command.ExecuteNonQuery();

            return new OperationResult(queryWriteOperation, OperationResult.Status.Success);
        }

        private string GenerateUpdatePhoneQuery(string query)
        {
            // Generate a query to update phone number
            return "UPDATE dbo.Person SET Phone = @phone WHERE Id = @id";
        }

        private string GenerateUpdateCityQuery(string query)
        {
            // Generate a query to update city
            return "UPDATE dbo.Person SET City = @newcity WHERE City = @oldcity";
        }

        // Parameters specified in Manager are passed to this method
        // These parameters make the connection string
        private string GetConnectionString(IDictionary<string, string> parameters)
        {
            string connectionString = string.Empty;
            string server = parameters["server"] as string, database = parameters["database"] as string;
            string userName = parameters["username"] as string, password = parameters["password"] as string;
            try
            {
                connectionString = string.IsNullOrEmpty(server) ? "" : "Server=" + server + ";";
                connectionString = string.IsNullOrEmpty(database) ? "" : "Database=" + database + ";";
                connectionString += "User ID=";
                connectionString += string.IsNullOrEmpty(userName) ? "" : userName;
                connectionString += ";";
                connectionString += "Password=";
                connectionString += string.IsNullOrEmpty(password) ? "" : password;
                connectionString += ";";
            }
            catch (Exception exp)
            {
                // Handle exception
            }
            return connectionString;
        }
        // Deploy this class on cache
    }
}

namespace Alachisoft.NCache.Sample.Data
{
    [QueryIndexable]
    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public int Id { get; set; }
        public string City { get; set; }
        public string Phone { get; set; }
    }


    internal class Product
    {
        public object? ProductID { get; internal set; }
    }
}
