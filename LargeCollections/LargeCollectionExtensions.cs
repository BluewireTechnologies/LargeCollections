using System;
using System.Collections;

namespace LargeCollections
{
    public static class LargeCollectionExtensions
    {
        public static T GetBackingStore<T>(this IEnumerable collection)
        {
            if(collection is IHasBackingStore<T>)
            {
                return ((IHasBackingStore<T>)collection).BackingStore;
            }
            if (collection is IHasUnderlyingCollection)
            {
                var underlying = ((IHasUnderlyingCollection) collection).UnderlyingCollection;
                if(underlying != null) return underlying.GetBackingStore<T>();
            }
            return default(T);
        }

        public static IDisposable Acquire<T>(this ILargeCollection<T> collection)
        {
            var backingStore = collection.GetBackingStore<IReferenceCountedResource>();
            if (backingStore != null) return backingStore.Acquire();
            return new EmptyDisposable();
        }

        class EmptyDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }

        public static T GetBackingStore<T>(this IEnumerator enumerator)
        {
            if (enumerator is IHasUnderlyingCollection)
            {
                return ((IHasUnderlyingCollection)enumerator).UnderlyingCollection.GetBackingStore<T>();
            }
            return default(T);
        }
    }
}
