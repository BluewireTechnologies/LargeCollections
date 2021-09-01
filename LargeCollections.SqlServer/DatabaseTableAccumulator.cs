using System.Collections.Generic;
using System.Data;
using LargeCollections.Core;
using LargeCollections.Core.Collections;

namespace LargeCollections.SqlServer
{
    public class DatabaseTableAccumulator<T> : DatabaseTableAppender<T>, IAccumulator<T>
    {
        private readonly SqlSession session;
        public DatabaseTableAccumulator(SqlSession session, SqlDbType columnType) : base(session, new DatabaseTableSchema<T> { { "value", i => i, columnType } })
        {
            this.session = session;
        }

        public ILargeCollection<T> Complete()
        {
            CloseUnderlyingWriter();
            if (Count == 0) return new InMemoryLargeCollection<T>(new List<T>(), null);
            TableReference.ApplyIndex("value");
            return new DatabaseTableLargeCollection<T>(session, TableReference, Count);
        }
    }
}
