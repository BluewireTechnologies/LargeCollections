using System;
using System.Collections;
using System.Collections.Generic;

namespace LargeCollections.Core.Operations
{
    /// <summary>
    /// Reads an ISinglePassCollection as a series of batches of the specified size.
    /// </summary>
    /// <remarks>
    /// Consumes the specified ISinglePassCollection. Do not attempt to use that collection elsewhere.
    /// The contents of a batch should be processed and copied before calling MoveNext(); semantics are
    /// similar to IDataReader.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    internal class BatchedSinglePassCollection<T> : IEnumerator<IEnumerable<T>>, IMappedCount
    {
        private readonly IEnumerator<T> source;
        private readonly int batchSize;
        private List<T> batch;

        public BatchedSinglePassCollection(IEnumerator<T> source, int batchSize)
        {
            this.source = source;
            this.batchSize = batchSize;
            batch = new List<T>(batchSize);
            Current = batch;
        }

        public IEnumerable<T> Current { get; private set; }

        public void Dispose()
        {
            source.Dispose();
            Current = null;
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            if (!source.MoveNext())
            {
                Dispose();
                return false;
            }

            batch.Clear();
            do
            {
                batch.Add(source.Current);
            } while (batch.Count < batchSize && source.MoveNext());
            return true;
        }



        public void Reset()
        {
            throw new NotSupportedException();
        }

        long IMappedCount.MapCount(long sourceCount)
        {
            return ((sourceCount - 1)/batchSize) + 1;
        }
    }
}
