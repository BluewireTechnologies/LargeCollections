using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using LargeCollections.Resources;

namespace LargeCollections.Collections
{
    public class DatabaseTableAccumulator<T> : IAccumulator<T>, IHasBackingStore<DatabaseTableReference>
    {
        private readonly SqlDbType columnType;
        private TemporaryDatabaseTableReference tableReference;
        private IDisposable reference;
        private bool completed;

        public DatabaseTableAccumulator(SqlConnection connection, SqlDbType columnType)
        {
            this.columnType = columnType;
            tableReference = new TemporaryDatabaseTableReference(connection);

            reference = tableReference.Acquire();
        }

        private SqlBulkCopy writer;

        private void BeginWrite()
        {
            if (writer != null) return;

            // create temp table
            tableReference.Create(columnType);
            // create bulk copy object
            writer = new SqlBulkCopy(tableReference.Connection) { DestinationTableName = tableReference.TableName };
        }

        private void EndWrite()
        {
            if (writer == null) return;
            writer.Close();
        }

        public ILargeCollection<T> Complete()
        {
            if (completed) throw new InvalidOperationException();
            completed = true;
            EndWrite();
            if (Count == 0) return new InMemoryLargeCollection<T>(new List<T>(), null);
            tableReference.ApplyIndex();
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
            writer.WriteToServer(new BulkInsertableEnumerator<T>(items.GetEnumerator(), columnType, () => Count++));
        }

       
        public long Count { get; private set; }

        public void Dispose()
        {
            EndWrite();
            reference.Dispose();
        }

        public DatabaseTableReference BackingStore
        {
            get { return tableReference; }
        }
    }
}