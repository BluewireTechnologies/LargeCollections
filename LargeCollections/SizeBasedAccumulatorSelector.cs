using System.Collections.Generic;
using System.IO;
using LargeCollections.Collections;
using LargeCollections.Storage;

namespace LargeCollections
{
    public class SizeBasedAccumulatorSelector : IAccumulatorSelector
    {
        private readonly long backingStoreThreshold;

        private static readonly SerialiserSelector serialisers = new SerialiserSelector();

        public SizeBasedAccumulatorSelector() : this(DEFAULT_BACKING_STORE_THRESHOLD)
        {
            
        }

        public SizeBasedAccumulatorSelector(long backingStoreThreshold)
        {
            this.backingStoreThreshold = backingStoreThreshold;
        }

        private const long DEFAULT_BACKING_STORE_THRESHOLD = 100000;

        /// <summary>
        /// Get an accumulator suitable for a set of the specified size.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="totalSizeOfCollection"></param>
        /// <returns></returns>
        public IAccumulator<T> GetAccumulator<T>(long totalSizeOfCollection)
        {
            if(totalSizeOfCollection > backingStoreThreshold)
            {
                var file = Path.GetTempFileName();
                return new FileAccumulator<T>(file, serialisers.Get<T>());
            }
            else
            {
                return new InMemoryAccumulator<T>();
            }
        }

        /// <summary>
        /// Get an accumulator for a set of unknown size.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IAccumulator<T> GetAccumulator<T>()
        {
            return new HybridAccumulator<T>(backingStoreThreshold);
        }

        class HybridAccumulator<T> : IAccumulator<T>
        {
            private readonly long backingStoreThreshold;
            private InMemoryAccumulator<T> inMemoryAccumulator;
            private IAccumulator<T> accumulator;

            public HybridAccumulator(long backingStoreThreshold)
            {
                this.backingStoreThreshold = backingStoreThreshold;
                accumulator = inMemoryAccumulator = new InMemoryAccumulator<T>();
            }

            public ILargeCollection<T> Complete()
            {
                return accumulator.Complete();
            }

            public void Add(T item)
            {
                if(Count == backingStoreThreshold && inMemoryAccumulator != null)
                {
                    var file = Path.GetTempFileName();
                    accumulator = new FileAccumulator<T>(file, serialisers.Get<T>());
                    accumulator.AddRange(inMemoryAccumulator.GetBuffer());
                    inMemoryAccumulator.Dispose();
                    inMemoryAccumulator = null;
                }
                accumulator.Add(item);
            }

            public void AddRange(IEnumerable<T> items)
            {
                foreach(var item in items)
                {
                    Add(item);
                }
            }

            public long Count { get { return accumulator.Count; } }

            public void Dispose()
            {
                accumulator.Dispose();
            }
        }
    }
}
