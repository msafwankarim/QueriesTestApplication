using Alachisoft.NCache.Runtime.DatasourceProviders;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QueriesTestApplication.Providers
{
    internal class DualDataBaseWriteThru : IWriteThruProvider
    {

        string sqlServerConnectionString = string.Empty;
        string mySqlConnectionString = string.Empty;

        SqlConnection sqlServerConnection = null;
        MySqlConnection mySqlConnection = null;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Init(IDictionary<string, string> parameters, string cacheName)
        {
            if (!parameters.ContainsKey("sqlServerConnectionString") || !parameters.ContainsKey("mySqlConnectionString"))
                throw new ArgumentException("sqlServerConnectionString and mySqlConnectionString are required");

            sqlServerConnectionString = parameters["sqlServerConnectionString"];
            mySqlConnectionString = parameters["mySqlConnectionString"];
        }

        public OperationResult WriteToDataSource(WriteOperation operation)
        {
            throw new NotImplementedException();
        }

        public ICollection<OperationResult> WriteToDataSource(ICollection<WriteOperation> operations)
        {
            throw new NotImplementedException();
        }

        public ICollection<OperationResult> WriteToDataSource(ICollection<DataTypeWriteOperation> dataTypeWriteOperations)
        {
            throw new NotImplementedException();
        }

        public OperationResult WriteToDataSource(QueryWriteOperation queryWriteOperation)
        {
            SqlConnection connection = null;
            DbCommand command = null;

            switch (queryWriteOperation.Hint?.ToLower())
            {
                case "sqlserver":
                    command = GetSqlServerCommand(queryWriteOperation.Query, queryWriteOperation.Parameters);
                    break;
                case "mysql":
                    command = GetMySqlCommand(queryWriteOperation.Query, queryWriteOperation.Parameters);
                    break;
            }

            int affectedRows = command.ExecuteNonQuery();

            return null;
        }

        private MySqlCommand GetMySqlCommand(string query, IDictionary<string, object> parameters)
        {
            MySqlConnection connection = EnsureMySqlDBConnection(ref mySqlConnection, mySqlConnectionString);
            MySqlCommand command = new MySqlCommand(query, connection);

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    command.Parameters.AddWithValue(query, parameter.Value);
                }
            }

            return command;
        }

        private SqlCommand GetSqlServerCommand(string query, IDictionary<string, object> parameters)
        {
            SqlConnection connection = EnsureSqlServerDBConnection(ref sqlServerConnection, sqlServerConnectionString);
            SqlCommand command = new SqlCommand(query, connection);

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    command.Parameters.AddWithValue(query, parameter.Value);
                }
            }

            return command;
        }

        private SqlConnection EnsureSqlServerDBConnection(ref SqlConnection connection, string connectionString)
        {
            if (connection == null)
                connection = new SqlConnection(connectionString);

            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            return connection;
        }

        private MySqlConnection EnsureMySqlDBConnection(ref MySqlConnection connection, string connectionString)
        {
            if (connection == null)
                connection = new MySqlConnection(connectionString);

            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            return connection;
        }
    }
}
