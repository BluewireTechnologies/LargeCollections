using System.Data.SqlClient;

namespace LargeCollections.SqlServer.Tests.IntegrationTesting
{
    public class SqlServerConnectionString
    {
        public SqlServerConnectionString(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            Server = builder.DataSource;
            Database = builder.InitialCatalog;
            Value = builder.ConnectionString;
        }

        public string Server { get; }
        public string Database { get; }
        public string Value { get; }

        public override string ToString() => Value;
    }
}
