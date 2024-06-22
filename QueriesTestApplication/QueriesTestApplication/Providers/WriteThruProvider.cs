using Alachisoft.NCache.Caching.AutoExpiration;
using Alachisoft.NCache.Client;
using Alachisoft.NCache.Runtime.Caching;
using Alachisoft.NCache.Runtime.DatasourceProviders;
using Alachisoft.NCache.Sample.Data;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;

namespace QueriesTestApplication.Providers
{
    public class WriteThruProvider : IWriteThruProvider
    {
        string filePath = "C:\\\\writeThruLogs.txt";

        string connectionString = string.Empty;
        SqlConnection sqlServerConnection = null;

        public void Init(IDictionary<string, string> parameters, string cacheName)
        {
            if (!parameters.ContainsKey("connectionString"))
                throw new ArgumentException("connectionString required");

            connectionString = parameters["connectionString"];
        }

        public ICollection<OperationResult> WriteToDataSource(ICollection<WriteOperation> operations)
        {
            return null;
        }

        public ICollection<OperationResult> WriteToDataSource(ICollection<DataTypeWriteOperation> dataTypeWriteOperations)
        {
            return null;
        }

        private void Log(string message)
        {
            lock(filePath)
            {
                File.AppendAllText(filePath, $"\n  {DateTime.Now}\t {message}");
            }
        }

        public OperationResult WriteToDataSource(WriteOperation operation)
        {

            var item = operation.ProviderItem.GetValue<Person>();

            if (item == null)
                return null;

            EnsureDBConnection();

            SqlCommand command = new SqlCommand($"INSERT INTO Alachisoft.NCache.Sample.Data.Person (FirstName, LastName, Age, City, Phone) VALUES ('{item.FirstName}', '{item.LastName}', '{item.Age}', '{item.City}', '{item.Phone}');", sqlServerConnection);

            command.ExecuteNonQuery();

            return null;
        }

        private SqlConnection EnsureDBConnection()
        {
            if (sqlServerConnection == null)
                sqlServerConnection = new SqlConnection(connectionString);

            if (sqlServerConnection.State != System.Data.ConnectionState.Open)
                sqlServerConnection.Open();

            return sqlServerConnection;
        }

        public OperationResult WriteToDataSource(QueryWriteOperation queryWriteOperation)
        {
            string query = string.Empty;

            switch (queryWriteOperation.Hint?.ToLower())
            {
                case "city":
                    query = GetUpdateCityQuery(queryWriteOperation.Query);
                    break;

                case "phone":
                    query = GetUpdatePhoneQuery(queryWriteOperation.Query);
                    break;
            }

            EnsureDBConnection();

            SqlCommand command = new SqlCommand(query, sqlServerConnection);

            if(queryWriteOperation.Parameters != null)
            {
                foreach (var param in queryWriteOperation.Parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }
            }

            int affectedRows = command.ExecuteNonQuery();

            return null;
        }

        private string GetUpdatePhoneQuery(string query)
        {
            return "UPDATE dbo.Person SET City = @newcity WHERE City = @oldcity";
        }

        private string GetUpdateCityQuery(string query)
        {
            return "UPDATE dbo.Person SET Phone = @phone WHERE Id = @id";
        }


        public void Dispose()
        {
            File.AppendAllText(filePath, $"\n Dispose => Method called at . {DateTime.Now}");

            sqlServerConnection?.Close();
            sqlServerConnection?.Dispose();
        }
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
}
