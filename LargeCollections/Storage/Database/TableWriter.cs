using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace LargeCollections.Storage.Database
{
    public class TableWriter<T> : IDisposable
    {
        List<IColumnPropertyMapping<T>> columns;

        public TableWriter(SqlConnection cn, List<IColumnPropertyMapping<T>> columns, string tableName)
        {
            this.columns = columns;
            bulkWriter = new SqlBulkCopy(cn)
            {
                DestinationTableName = tableName,
                BatchSize = 500
            };

            for (var i = 0; i < columns.Count; i++)
            {
                bulkWriter.ColumnMappings.Add(i, columns[i].Name);
            }
        }

        public TimeSpan Timeout
        {
            get { return TimeSpan.FromSeconds(bulkWriter.BulkCopyTimeout); }
            set { bulkWriter.BulkCopyTimeout = (int)value.TotalSeconds; }
        }

        private SqlBulkCopy bulkWriter;

        public int Write(IEnumerable<T> items)
        {
            using (var reader = new ObjectDataReader<T>(columns, items.GetEnumerator()))
            {
                bulkWriter.WriteToServer(reader);
                return reader.GetFinalCount();
            }
        }

        public int Write(T item)
        {
            return Write(new [] {item});
        }

        public void Dispose()
        {
            bulkWriter.Close();
        }
    }
}