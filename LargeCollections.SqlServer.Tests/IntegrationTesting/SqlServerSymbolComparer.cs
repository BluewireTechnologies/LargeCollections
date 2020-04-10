using System;
using System.Collections.Generic;

namespace LargeCollections.SqlServer.Tests.IntegrationTesting
{
    public struct SqlServerSymbolComparer : IEqualityComparer<string>
    {
        public static readonly SqlServerSymbolComparer Instance = new SqlServerSymbolComparer();
        public bool Equals(string x, string y) => StringComparer.OrdinalIgnoreCase.Equals(x, y);
        public int GetHashCode(string obj) => StringComparer.OrdinalIgnoreCase.GetHashCode(obj);
    }
}
