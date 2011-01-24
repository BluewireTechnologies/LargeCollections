using System;
using System.Collections.Generic;

namespace LargeCollections.Operations
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
    public class BatchedSinglePassCollection<T> : ISinglePassCollection<IEnumerable<T>>
    {
        private readonly ISinglePassCollection<T> source;
        private readonly int batchSize;
        private List<T> batch;

        public BatchedSinglePassCollection(ISinglePassCollection<T> source, int batchSize)
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

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            if(!source.MoveNext())
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

        public long Count
        {
            get { return ((source.Count - 1) / batchSize) + 1; }
        }
    }
}