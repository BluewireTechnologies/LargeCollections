using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using LargeCollections.Resources;
using LargeCollections.Storage.Database;

namespace LargeCollections.Collections
{
    public class DatabaseTableAccumulator<T> : DatabaseTableAppender<T>, IAccumulator<T>
    {
        private readonly SqlConnection connection;
        public DatabaseTableAccumulator(SqlConnection connection, SqlDbType columnType) : base(connection, new DatabaseTableSchema<T> { { "value", i => i, columnType } })
        {
            this.connection = connection;
        }

        public ILargeCollection<T> Complete()
        {
            CloseUnderlyingWriter();
            if (Count == 0) return new InMemoryLargeCollection<T>(new List<T>(), null);
            TableReference.ApplyIndex("value");
            return new DatabaseTableLargeCollection<T>(connection, TableReference, Count);
        }
    }
}