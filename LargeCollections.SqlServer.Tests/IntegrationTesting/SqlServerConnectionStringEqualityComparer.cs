using System;
using System.Collections.Generic;

namespace LargeCollections.SqlServer.Tests.IntegrationTesting
{
    /// <summary>
    /// Tests for equivalence of server instance and database name. Case-insensitive.
    /// </summary>
    public sealed class SqlServerConnectionStringEqualityComparer : IEqualityComparer<SqlServerConnectionString>
    {
        public bool Equals(SqlServerConnectionString x, SqlServerConnectionString y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;

            return StringComparer.OrdinalIgnoreCase.Equals(NormaliseServer(x.Server), NormaliseServer(y.Server))
                   && SqlServerSymbolComparer.Instance.Equals(x.Database, y.Database);
        }

        private static string NormaliseServer(string server)
        {
            if (server.Equals("(local)", StringComparison.OrdinalIgnoreCase)) return "localhost";
            if (server.StartsWith("(local)\\", StringComparison.OrdinalIgnoreCase)) return "localhost" + server.Substring("(local)".Length);
            return server;
        }

        public int GetHashCode(SqlServerConnectionString obj)
        {
            unchecked
            {
                var hashCode = StringComparer.OrdinalIgnoreCase.GetHashCode(NormaliseServer(obj.Server));
                hashCode = (hashCode * 397) ^ SqlServerSymbolComparer.Instance.GetHashCode(obj.Database);
                return hashCode;
            }
        }
    }
}
