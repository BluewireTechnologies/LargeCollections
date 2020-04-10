using System;
using System.Collections.Generic;
using Bluewire.ReferenceCounting;
using LargeCollections.Resources;

namespace LargeCollections.Collections
{
    public class InMemoryLargeCollection<T> : ILargeCollection<T>, IHasBackingStore<IReferenceCountedResource>
    {
        private ICollection<T> @internal;

        private IDisposable reference;
        public InMemoryLargeCollection(List<T> contents, IReferenceCountedResource resource)
        {
            Count = contents.Count;
            var array = new T[Count];
            contents.CopyTo(array);
            @internal = array;
            BackingStore = resource;
            if (resource != null) reference = resource.Acquire();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new LargeCollectionEnumerator<T>(this, @internal.GetEnumerator());
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            if (reference != null) reference.Dispose();
        }

        public long Count { get; private set; }

        public IReferenceCountedResource BackingStore { get; private set; }
    }
}
