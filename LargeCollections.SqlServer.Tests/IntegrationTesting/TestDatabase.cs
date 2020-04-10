using System.Collections.Generic;
using System.Data.SqlClient;
using NUnit.Framework;

namespace LargeCollections.SqlServer.Tests.IntegrationTesting
{
    public class TestDatabase
    {
        public static TestDatabase LocalTempDb => UseFirstAvailable(
            "data source=(local);initial catalog=tempdb;Integrated Security=SSPI;Connect Timeout = 1",
            "data source=(local)\\SQLExpress;initial catalog=tempdb;Integrated Security=SSPI;Connect Timeout = 1"
            );

        private readonly SqlServerConnectionString connectionString;

        public TestDatabase(string connectionString)
        {
            this.connectionString = new SqlServerConnectionString(connectionString);
        }

        public bool IsAvailable => CheckAvailability(connectionString);

        public string Get()
        {
            // We want to rapidly report 'inconclusive' if the database is unavailable.
            if (!IsAvailable) Assert.Ignore($"Database unavailable: {connectionString}");
            return connectionString.Value;
        }

        private static TestDatabase UseFirstAvailable(string firstConnectionString, params string[] fallbackConnectionStrings)
        {
            var first = new TestDatabase(firstConnectionString);
            if (!first.IsAvailable)
            {
                foreach (var connectionString in fallbackConnectionStrings)
                {
                    var fallback = new TestDatabase(connectionString);
                    if (fallback.IsAvailable) return fallback;
                }
            }
            return first;
        }

        private static readonly Dictionary<SqlServerConnectionString, bool> availability = new Dictionary<SqlServerConnectionString, bool>(new SqlServerConnectionStringEqualityComparer());

        private static bool CheckAvailability(SqlServerConnectionString instance)
        {
            lock (availability)
            {
                if (availability.TryGetValue(instance, out var isAvailable)) return isAvailable;
                try
                {
                    using (var cn = new SqlConnection(instance.Value)) { cn.Open(); }
                    availability[instance] = true;
                    return true;
                }
                catch
                {
                    availability[instance] = false;
                    return false;
                }
            }
        }
    }
}
