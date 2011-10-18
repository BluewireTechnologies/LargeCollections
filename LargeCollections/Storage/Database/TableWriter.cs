using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace LargeCollections.Storage.Database
{
    public interface IEntityWriter<in T> : IDisposable
    {
        int Write(IEnumerable<T> items);
        int Write(T item);
    }

    public class TableWriter<T> : IEntityWriter<T>
    {
        public TableWriter(SqlConnection cn, List<IColumnPropertyMapping<T>> columns, string tableName)
        {
            bulkWriter = new SqlBulkCopy(cn)
            {
                DestinationTableName = tableName,
                BatchSize = 500
            };

            for (var i = 0; i < columns.Count; i++)
            {
                bulkWriter.ColumnMappings.Add(i, columns[i].Name);
            }

            bulkWriterTask = Task.Factory.StartNew(() => {
                using (var reader = new ObjectDataReader<T>(columns, queue.GetConsumingEnumerable().GetEnumerator()))
                {
                    bulkWriter.WriteToServer(reader);
                }
                bulkWriter.Close();
            });
        }

        public TimeSpan Timeout
        {
            get { return TimeSpan.FromSeconds(bulkWriter.BulkCopyTimeout); }
            set { bulkWriter.BulkCopyTimeout = (int)value.TotalSeconds; }
        }
        
        private BlockingCollection<T> queue = new BlockingCollection<T>();

        private Task bulkWriterTask;
        private SqlBulkCopy bulkWriter;
        
        private void CheckWriterIsLive()
        {
            if(bulkWriterTask.IsFaulted) bulkWriterTask.Wait();
        }

        public int Write(IEnumerable<T> items)
        {
            CheckWriterIsLive();
            var count = 0;
            foreach(var item in items)
            {
                queue.Add(item);
                count++;
            }
            return count;
            
        }

        public int Write(T item)
        {
            CheckWriterIsLive();
            queue.Add(item);
            return 1;
        }

        public void Dispose()
        {
            queue.CompleteAdding();
            try
            {
                bulkWriterTask.Wait();
            }
            catch(AggregateException ex) 
            {
                throw new BackingStoreException("Could not complete bulk insert.", ex.Flatten().InnerException);
            }
        }
    }
}