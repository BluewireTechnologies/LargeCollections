using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
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
        private QueueConsumerFactory factory;

        public TableWriter(SqlConnection cn, List<IColumnPropertyMapping<T>> columns, string tableName)
        {
            factory = new QueueConsumerFactory(cn, columns, tableName);
            timeout = TimeSpan.FromSeconds(factory.BulkCopy.BulkCopyTimeout);
            instance = new BackgroundWriter(factory);
        }

        private readonly BackgroundWriter instance;

        private TimeSpan timeout;
        public TimeSpan Timeout
        {
            get { return timeout; }
            set
            {
                if(value != timeout)
                {
                    instance.Configure(bulkCopy => bulkCopy.BulkCopyTimeout = (int) value.TotalSeconds);
                    timeout = value;
                }
            }
        }

        public int Write(IEnumerable<T> items)
        {
            var count = 0;
            var batchDuration = GetBatchDuration();
            using(var enumerator = items.GetEnumerator())
            {
                while (enumerator.MoveNext()) QueueItemBatch(enumerator, batchDuration, ref count);
            }
            return count;
        }

        private TimeSpan GetBatchDuration()
        {
            var duration = TimeSpan.FromSeconds(Timeout.TotalSeconds / 2);
            if (duration == TimeSpan.Zero) return TimeSpan.FromSeconds(5);
            return duration;
        }

        private void QueueItemBatch(IEnumerator<T> items, TimeSpan duration, ref int count)
        {
            var end = DateTime.Now + duration;
            instance.CheckState();
            do
            {
                instance.Add(items.Current);
                count++;
            } while (end > DateTime.Now && items.MoveNext());
        }

        public int Write(T item)
        {
            instance.CheckState();
            instance.Add(item);
            return 1;
        }

        public void Dispose()
        {
            try
            {
                instance.Dispose();
            }
            catch (AggregateException ex)
            {
                throw ex.Flatten().InnerException;
            }
            finally
            {
                factory.Dispose();
            }
        }


        class QueueConsumerFactory : IDisposable
        {
            public SqlBulkCopy BulkCopy { get; private set; }
            private readonly List<IColumnPropertyMapping<T>> columns;

            public QueueConsumerFactory(SqlConnection cn, List<IColumnPropertyMapping<T>> columns, string tableName)
            {
                this.BulkCopy = new SqlBulkCopy(cn)
                {
                    DestinationTableName = tableName,
                    BatchSize = 500
                };

                for (var i = 0; i < columns.Count; i++)
                {
                    this.BulkCopy.ColumnMappings.Add(i, columns[i].Name);
                }

                this.columns = columns;
            }

            public Action CreateConsumer(IEnumerable<T> queue)
            {
                return () =>
                {
                    using (var reader = new ObjectDataReader<T>(columns, queue.GetEnumerator()))
                    {
                        this.BulkCopy.WriteToServer(reader);
                    }
                };
            }

            public void Dispose()
            {
                this.BulkCopy.Close();
            }
        }

        class BackgroundWriter : IDisposable
        {
            private readonly QueueConsumerFactory factory;
            private Task bulkWriterTask;
            private BlockingCollection<T> currentQueue = new BlockingCollection<T>();
            
            public BackgroundWriter(QueueConsumerFactory factory)
            {
                this.factory = factory;
                bulkWriterTask = Task.Factory.StartNew(factory.CreateConsumer(currentQueue.GetConsumingEnumerable()));
            }

            public void Configure(Action<SqlBulkCopy> configure)
            {
                configure(factory.BulkCopy);
                Reopen();
            }

            private void Reopen()
            {
                currentQueue.CompleteAdding();
                currentQueue = new BlockingCollection<T>();
                var consumeQueue = factory.CreateConsumer(currentQueue.GetConsumingEnumerable());
                bulkWriterTask = bulkWriterTask.ContinueWith(t => {
                    t.Wait();
                    consumeQueue();
                });
            }

            public void Add(T item)
            {
                currentQueue.Add(item);
            }

            public void CheckState()
            {
                if (bulkWriterTask.IsFaulted) bulkWriterTask.Wait();
            }

            public void Dispose()
            {
                currentQueue.CompleteAdding();
                bulkWriterTask.Wait();
            }
        }
    }
}