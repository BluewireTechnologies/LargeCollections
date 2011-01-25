using System;
using System.Collections;
using System.Collections.Generic;
using LargeCollections.Collections;
using LargeCollections.Storage;

namespace LargeCollections
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Threadsafe.
    /// </remarks>
    public class SizeBasedAccumulatorSelector : IAccumulatorSelector, IOperatorCache
    {
        private readonly long backingStoreThreshold;

        private static readonly SerialiserSelector serialisers = new SerialiserSelector();


        public TemporaryFileProvider TemporaryFileProvider { get; set; }


        public SizeBasedAccumulatorSelector() : this(DEFAULT_BACKING_STORE_THRESHOLD)
        {
        }

        public SizeBasedAccumulatorSelector(long backingStoreThreshold)
        {
            TemporaryFileProvider = new TemporaryFileProvider();
            this.backingStoreThreshold = backingStoreThreshold;
        }

        private const long DEFAULT_BACKING_STORE_THRESHOLD = 10000;

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
                var file = TemporaryFileProvider.GetTempFile();
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
            return new HybridAccumulator<T>(TemporaryFileProvider, backingStoreThreshold);
        }


        /// <summary>
        /// Get an accumulator tailored to a set of similar size to that provided.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IAccumulator<T> GetAccumulator<T>(IEnumerator source)
        {
            var countable = source.GetUnderlying<ICounted>();
            if(countable != null)
            {
                return GetAccumulator<T>(countable.Count);
            }
            return new HybridAccumulator<T>(TemporaryFileProvider, backingStoreThreshold);
        }


        class HybridAccumulator<T> : IAccumulator<T>
        {
            private readonly TemporaryFileProvider temporaryFileProvider;
            private readonly long backingStoreThreshold;
            private InMemoryAccumulator<T> inMemoryAccumulator;
            private IAccumulator<T> accumulator;

            public HybridAccumulator(TemporaryFileProvider temporaryFileProvider, long backingStoreThreshold)
            {
                this.temporaryFileProvider = temporaryFileProvider;
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
                    var file = temporaryFileProvider.GetTempFile();
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

        // simplest way to make the cache threadsafe
        [ThreadStatic]
        private IDictionary<Type, object> operatorCache = new Dictionary<Type, object>();
        public T GetInstance<T>(Func<T> create)
        {
            object operatorInstance;
            if (!operatorCache.TryGetValue(typeof(T), out operatorInstance))
            {
                operatorInstance = create();
                operatorCache[typeof (T)] = operatorInstance;
            }
            return (T)operatorInstance;
        }
    }
}
