using System;
using System.Collections.Generic;
using System.IO;

namespace LargeCollections
{
    public interface IAccumulatorSelector
    {
        /// <summary>
        /// Get an accumulator suitable for a set of the specified size.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="totalSizeOfCollection"></param>
        /// <returns></returns>
        IAccumulator<T> GetAccumulator<T>(long totalSizeOfCollection);

        /// <summary>
        /// Get an accumulator for a set of unknown size.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IAccumulator<T> GetAccumulator<T>();
    }

    public class GuidSerialiser : IItemSerialiser<Guid[]>
    {
        [ThreadStatic]
        private byte[] buffer = new byte[0];
        private void EnsureBuffer(int size)
        {
            if (buffer == null || size > buffer.Length)
            {
                buffer = new byte[size];
            }
        }

        [ThreadStatic]
        private Guid[] guidBuffer = new Guid[16];
        private void EnsureGuidBuffer(int length)
        {
            if (guidBuffer == null || length > guidBuffer.Length)
            {
                guidBuffer = new Guid[length];
            }
        }

        public void Write(Stream stream, Guid[] item)
        {
            var length = item.Length;
            var size = 4 + (16*length);

            EnsureBuffer(size);
            
            BitConverter.GetBytes(length).CopyTo(buffer, 0);
            for(int i = 0, offset = 4; i < item.Length; i++, offset+=16)
            {
                item[i].ToByteArray().CopyTo(buffer, offset);
            }
            stream.Write(buffer, 0, size);
        }

        [ThreadStatic]
        private byte[] intBuffer = new byte[4];
        [ThreadStatic]
        private byte[] loadGuid;
        public Guid[] Read(Stream stream)
        {
            if(stream.Read(intBuffer, 0, 4) <= 0)
            {
                throw new InvalidOperationException("Read past end of stream");
            }
            var length = BitConverter.ToInt32(intBuffer, 0);
            EnsureGuidBuffer(length);
            loadGuid = loadGuid ?? new byte[16];
            for(var i = 0; i < length; i++)
            {
                if (stream.Read(loadGuid, 0, loadGuid.Length) <= 0)
                {
                    throw new InvalidOperationException("Read past end of stream");
                }
                guidBuffer[i] = new Guid(loadGuid);
            }
            return guidBuffer;
        }
    }

    class SerialiserSelector
    {
        public SerialiserSelector()
        {
            Add(new GuidSerialiser());
        }

        private Dictionary<Type, object> serialisers = new Dictionary<Type, object>();
        private void Add<TItem>(IItemSerialiser<TItem> instance)
        {
            serialisers.Add(typeof(TItem), instance);
        }

        public IItemSerialiser<T> Get<T>()
        {
            if(serialisers.ContainsKey(typeof(T)))
            {
                return (IItemSerialiser<T>)serialisers[typeof (T)];
            }
            return new DefaultItemSerialiser<T>();
        }
    }

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
                return new FileAccumulator<T>(file, serialisers.Get<T[]>());
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
                    accumulator = new FileAccumulator<T>(file, serialisers.Get<T[]>());
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
