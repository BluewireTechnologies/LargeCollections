using System;
using System.Collections.Generic;
using Bluewire.ReferenceCounting;

namespace LargeCollections.Core.Collections
{
    public abstract class LargeCollectionWithBackingStore<TItem, TBackingStore> : ILargeCollection<TItem>, IHasBackingStore<TBackingStore> where TBackingStore : IReferenceCountedResource
    {
        protected LargeCollectionWithBackingStore(TBackingStore backingStore, long itemCount)
        {
            BackingStore = backingStore;
            Count = itemCount;
            reference = BackingStore.Acquire();
        }

        private bool disposed = false;
        private IDisposable reference;
        public long Count { get; private set; }
        public TBackingStore BackingStore { get; private set; }

        protected abstract IEnumerator<TItem> GetEnumeratorImplementation();

        protected void AssertNotDisposed()
        {
            if (disposed) throw new ObjectDisposedException(GetType().Name);
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            AssertNotDisposed();
            return new LargeCollectionEnumerator<TItem>(this, GetEnumeratorImplementation());
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            disposed = true;
            reference.Dispose();
        }
    }
}