using System;
using System.Collections;
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
    public class BatchedSinglePassCollection<T> : IEnumerator<IEnumerable<T>>, IHasUnderlying<IEnumerator>
    {
        private readonly IEnumerator<T> source;
        private readonly int batchSize;
        private List<T> batch;

        public BatchedSinglePassCollection(IEnumerator<T> source, int batchSize)
        {
            this.source = source;
            var countable = source.GetUnderlying<ICountable>();
            if(countable != null)
            {
                Underlying = new CountedEnumerator(this, ((countable.Count - 1) / batchSize) + 1);
            }
            else
            {
                Underlying = this;
            }

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

        public IEnumerator Underlying { get; private set; }

        class CountedEnumerator : IEnumerator, ICountable, IHasUnderlying<IEnumerator>
        {
            public CountedEnumerator(IEnumerator underlying, long count)
            {
                Underlying = underlying;
                Count = count;
            }

            public object  Current
            {
                get { return Underlying.Current; }
            }

            public bool  MoveNext()
            {
                return Underlying.MoveNext();
            }

            public void  Reset()
            {
                Underlying.Reset();
            }

            public long Count { get; private set; }

            public IEnumerator Underlying { get; private set; }
        }
    }
}