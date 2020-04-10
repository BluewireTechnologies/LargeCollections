using System;
using System.Collections.Generic;
using System.Data;
using Bluewire.ReferenceCounting;
using LargeCollections.Resources;

namespace LargeCollections.Collections
{
    public class InMemoryAccumulator<T> : IAccumulator<T>, IHasBackingStore<IReferenceCountedResource>
    {
        /// <summary>
        /// Used to ensure correctness. Resource leaks are not a problem with InMemoryLargeCollections since
        /// the GC deals with them, but if the calling code expects only an ILargeCollection then some accumulator
        /// implementations may return a disk- or database-backed collection, which would be sensitive to such leaks.
        /// </summary>
        class MemoryResourceReference : ReferenceCountedResource
        {
            protected override void CleanUp()
            {
            }
        }

        private bool completed = false;
        private IDisposable reference;
        public InMemoryAccumulator()
        {
            BackingStore = new MemoryResourceReference();
            reference = BackingStore.Acquire();
        }

        public void Dispose()
        {
            reference.Dispose();
        }



        private readonly List<T> buffer = new List<T>();

        public void Add(T item)
        {
            if (completed) throw new ReadOnlyException();
            buffer.Add(item);
        }

        public void AddRange(IEnumerable<T> items)
        {
            if(completed) throw new ReadOnlyException();
            buffer.AddRange(items);
        }

        public long Count
        {
            get { return buffer.Count; }
        }

        public List<T> GetBuffer()
        {
            return buffer;
        }

        public ILargeCollection<T> Complete()
        {
            if (completed) throw new InvalidOperationException("Accumulator has already completed.");
            completed = true;
            return new InMemoryLargeCollection<T>(buffer, BackingStore);
        }

        public static ILargeCollection<T> From(IEnumerable<T> items)
        {
            using(var accumulator = new InMemoryAccumulator<T>())
            {
                accumulator.AddRange(items);
                return accumulator.Complete();
            }
        }

        private static readonly ILargeCollection<T> empty = new InMemoryLargeCollection<T>(new List<T>(), null);

        public static ILargeCollection<T> Empty()
        {
            return empty;
        }

        public IReferenceCountedResource BackingStore { get; private set; }
    }
}