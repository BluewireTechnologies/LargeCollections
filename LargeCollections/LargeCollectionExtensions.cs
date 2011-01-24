using System;
using System.Collections;
using System.Linq;
using LargeCollections.Collections;
using System.Collections.Generic;
using LargeCollections.Resources;

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

        public static T GetBackingStore<T>(this IEnumerator enumerator)
        {
            if (enumerator is IHasUnderlyingCollection)
            {
                return ((IHasUnderlyingCollection)enumerator).UnderlyingCollection.GetBackingStore<T>();
            }
            return default(T);
        }


        public static IReferenceCountedResource CollectResources(this IEnumerable<IEnumerable> resources)
        {
            return new MultipleResource(resources.Select(r => r.GetBackingStore<IReferenceCountedResource>()).Where(r => r != null).ToArray());
        }

        public static IReferenceCountedResource CollectResources(this IEnumerable<IEnumerator> resources)
        {
            return new MultipleResource(resources.Select(r => r.GetBackingStore<IReferenceCountedResource>()).Where(r => r != null).ToArray());
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

        public static ISinglePassCollection<T> AsSinglePass<T>(this ILargeCollection<T> collection)
        {
            return new SinglePassCollection<T>(collection);
        }

        public static ISinglePassCollection<T> SinglePass<T>(this IAccumulatorSelector accumulatorSelector, Action<IAccumulator<T>> func)
        {
            using(var accumulator = accumulatorSelector.GetAccumulator<T>())
            {
                func(accumulator);
                using(var set = accumulator.Complete())
                {
                    return new SinglePassCollection<T>(set);
                }
            }
        }

        public static T GetOperator<T>(this IAccumulatorSelector accumulatorSelector, Func<T> createDefault)
        {
            if(accumulatorSelector is IOperatorCache)
            {
                return ((IOperatorCache)accumulatorSelector).GetInstance(createDefault);
            }
            return createDefault();
        }
    }
}
