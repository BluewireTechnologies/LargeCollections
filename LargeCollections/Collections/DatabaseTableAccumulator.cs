using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using LargeCollections.Resources;
using LargeCollections.Storage.Database;

namespace LargeCollections.Collections
{
    public class DatabaseTableAccumulator<T> : IAccumulator<T>, IHasBackingStore<DatabaseTableReference<T>>
    {
        private readonly SqlDbType columnType;
        private TemporaryDatabaseTableReference<T> tableReference;
        private IDisposable reference;
        private bool completed;

        public DatabaseTableAccumulator(SqlConnection connection, SqlDbType columnType)
        {
            this.columnType = columnType;
            tableReference = new TemporaryDatabaseTableReference<T>(connection, new DatabaseTableSchema<T> { { "value", i => i, columnType } });

            reference = tableReference.Acquire();
        }

        private TableWriter<T> writer;

        private void BeginWrite()
        {
            if (writer != null) return;

            // create temp table
            writer = tableReference.Create();
        }

        private void EndWrite()
        {
            if (writer == null) return;
            writer.Dispose();
        }

        public ILargeCollection<T> Complete()
        {
            if (completed) throw new InvalidOperationException();
            completed = true;
            EndWrite();
            if (Count == 0) return new InMemoryLargeCollection<T>(new List<T>(), null);
            tableReference.ApplyIndex("value");
            return new DatabaseTableLargeCollection<T>(tableReference, Count);
        }

        public void Add(T item)
        {
            AddRange(new [] { item });
        }

        public void AddRange(IEnumerable<T> items)
        {
            if (completed) throw new ReadOnlyException();
            BeginWrite();
            Count += writer.Write(items);
        }

       
        public long Count { get; private set; }

        public void Dispose()
        {
            EndWrite();
            reference.Dispose();
        }

        public DatabaseTableReference<T> BackingStore
        {
            get { return tableReference; }
        }
    }
}