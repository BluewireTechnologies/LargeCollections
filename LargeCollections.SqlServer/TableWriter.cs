using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace LargeCollections.SqlServer
{
    public struct TableWriterOptions
    {
        /// <summary>
        /// Timeout to use for SqlBulkCopy.
        /// </summary>
        public TimeSpan? Timeout { get; set; }
        /// <summary>
        /// Maximum number of items to buffer at a time. Add will block/wait once the buffer is full.
        /// Default: 4096.
        /// </summary>
        public int? BufferSize { get; set; }
    }

    public class TableWriter<T> : IDisposable
    {
        private readonly Task bulkWriterTask;
        private readonly BlockingCollection<T> queue;
        private readonly List<IColumnPropertyMapping<T>> columns;
        /// <summary>
        /// Cancelled when Abort or Dispose is called.
        /// </summary>
        private readonly CancellationTokenSource abortCts = new CancellationTokenSource();
        /// <summary>
        /// Cancelled by the writer loop when it completes or aborts.
        /// </summary>
        private readonly CancellationTokenSource abortedCts = new CancellationTokenSource();

        public TableWriter(SqlSession session, List<IColumnPropertyMapping<T>> columns, string tableName, TableWriterOptions options = default(TableWriterOptions))
        {
            this.columns = columns;
            queue = new BlockingCollection<T>(options.BufferSize ?? 4096);
            bulkWriterTask = Task.Run(() => WriterLoop(session, tableName, options));
        }

        private async Task WriterLoop(SqlSession session, string tableName, TableWriterOptions options)
        {
            try
            {
                using (var bulkCopy = new SqlBulkCopy(session.Connection, SqlBulkCopyOptions.TableLock, session.Transaction))
                {
                    bulkCopy.DestinationTableName = tableName;
                    bulkCopy.BatchSize = 50;
                    if (options.Timeout != null) bulkCopy.BulkCopyTimeout = (int)options.Timeout.Value.TotalSeconds;

                    for (var i = 0; i < columns.Count; i++)
                    {
                        bulkCopy.ColumnMappings.Add(i, columns[i].Name);
                    }

                    var consumer = queue.GetConsumingEnumerable(abortCts.Token);
                    using (var iterator = consumer.GetEnumerator())
                    using (var reader = new ObjectDataReader<T>(columns, iterator))
                    {
                        await bulkCopy.WriteToServerAsync(reader, abortCts.Token);
                    }
                }
            }
            finally
            {
                abortedCts.Cancel();
            }
        }

        public int Write(IEnumerable<T> items)
        {
            var count = 0;
            foreach (var item in items)
            {
                if (queue.TryAdd(item, -1, abortedCts.Token))
                {
                    count++;
                }
                else
                {
                    if (bulkWriterTask.IsFaulted)
                    {
                        WaitAndRethrowExceptionsAsync(CancellationToken.None).GetAwaiter().GetResult();
                    }
                    // If we're not aborted, wait 10ms for the queue to clear slightly.
                    Thread.Sleep(10);
                }
            }
            return count;
        }

        public async Task<int> WriteAsync(IEnumerable<T> items, CancellationToken token = default)
        {
            var count = 0;
            using (var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(abortedCts.Token, token))
            {
                foreach (var item in items)
                {
                    if (queue.TryAdd(item, -1, linkedToken.Token))
                    {
                        count++;
                    }
                    else
                    {
                        token.ThrowIfCancellationRequested();
                        if (bulkWriterTask.IsFaulted)
                        {
                            await WaitAndRethrowExceptionsAsync(token).ConfigureAwait(false);
                        }
                        // If we're not aborted, wait 10ms for the queue to clear slightly.
                        await Task.Delay(10, token).ConfigureAwait(false);
                    }
                }
            }
            return count;
        }


        /// <summary>
        /// Closes the buffer and waits for it to clear.
        /// </summary>
        public void Complete() => CompleteAsync().GetAwaiter().GetResult();

        /// <summary>
        /// Closes the buffer and waits for it to clear.
        /// </summary>
        public async Task CompleteAsync(CancellationToken token = default)
        {
            queue.CompleteAdding();
            await WaitAndRethrowExceptionsAsync(token).ConfigureAwait(false);
        }

        /// <summary>
        /// Aborts the operation. Does not wait for it to finish.
        /// </summary>
        public void Abort()
        {
            abortCts.Cancel();
        }

        /// <summary>
        /// Aborts the operation, then waits for it to finish. Does not throw.
        /// </summary>
        public void AbortAndWait() => AbortAndWaitAsync().GetAwaiter().GetResult();

        /// <summary>
        /// Aborts the operation, then waits for it to finish. Does not throw.
        /// </summary>
        public async Task AbortAndWaitAsync(CancellationToken token = default)
        {
            Abort();
            await WaitForWriterToTerminate(token).ConfigureAwait(false);
        }

        /// <summary>
        /// Aborts the operation if it is still running. Waits for the background loop to shut down.
        /// </summary>
        public void Dispose() => AbortAndWait();

        private async Task WaitAndRethrowExceptionsAsync(CancellationToken token)
        {
            await WaitForWriterToTerminate(token).ConfigureAwait(false);
            try
            {
                await bulkWriterTask.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new BackgroundWriterAbortedException(ex);
            }
        }

        private async Task WaitForWriterToTerminate(CancellationToken token)
        {
            // We can't cancel an `await` on an existing Task, so instead we wait on a pair of tokens.
            using (var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(abortedCts.Token, token))
            {
                await linkedToken.Token;
            }
            token.ThrowIfCancellationRequested();
        }
    }
}
