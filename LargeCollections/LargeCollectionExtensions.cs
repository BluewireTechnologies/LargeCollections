using System;
using System.Collections;
using System.Diagnostics;
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
            if (collection is IHasUnderlying<IEnumerable>)
            {
                var underlying = ((IHasUnderlying<IEnumerable>) collection).Underlying;
                if(underlying != null) return underlying.GetBackingStore<T>();
            }
            return default(T);
        }

        public static T GetBackingStore<T>(this IEnumerator enumerator)
        {
            if (enumerator is IHasUnderlying<IEnumerable>)
            {
                return ((IHasUnderlying<IEnumerable>)enumerator).Underlying.GetBackingStore<T>();
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

        private static TInterface GetUnderlying<TInterface, TCollection>(TCollection item) where TInterface : class where TCollection : class
        {   
            if(item is TInterface)
            {
                return item as TInterface;
            }
            var wrappingItem = item as IHasUnderlying<TCollection>;
            if (wrappingItem == null) return null;
            return GetUnderlying<TInterface, TCollection>(wrappingItem.Underlying);
        }

        public static T GetUnderlying<T>(this IEnumerable item) where T : class
        {
            return GetUnderlying<T, IEnumerable>(item);
        }
        public static T GetUnderlying<T>(this IEnumerator item) where T : class
        {
            return GetUnderlying<T, IEnumerator>(item);
        }

        public static T GetOperator<T>(this IAccumulatorSelector accumulatorSelector, Func<T> createDefault)
        {
            if(accumulatorSelector is IOperatorCache)
            {
                return ((IOperatorCache)accumulatorSelector).GetInstance(createDefault);
            }
            return createDefault();
        }

        private static IEnumerable<ISortedCollection<T>> AssertAllSorted<T>(IEnumerable<ISortedCollection<T>> sortedEnumerables)
        {
            Debug.Assert(sortedEnumerables.Any(), "Collection was empty");
            if (sortedEnumerables.All(e => e != null))
            {
                if (sortedEnumerables.All(e => e.SortOrder != null))
                {
                    return sortedEnumerables;
                }
            }
            Debug.Fail("All input collections must be sorted.");
            throw new InvalidOperationException("All input collections must be sorted.");
        }

        private static IComparer<T> GetSingleSortOrder<T>(IEnumerable<ISortedCollection<T>> enumerables)
        {
            var sortOrders = enumerables.Select(c => c.SortOrder).Distinct();
            if(sortOrders.Count() != 1)
            {
                Debug.Fail("All input collections must be sorted in the same way.");
                throw new InvalidOperationException("All input collections must be sorted in the same way.");
            }
            return sortOrders.Single();
        }

        public static IComparer<T> GetCommonSortOrder<T>(this IList<IEnumerable<T>> enumerables)
        {
            return GetSingleSortOrder(AssertAllSorted(enumerables.Select(GetUnderlying<ISortedCollection<T>, IEnumerable<T>>)));
        }

        public static IComparer<T> GetCommonSortOrder<T>(this IList<IEnumerator<T>> enumerators)
        {
            return GetSingleSortOrder(AssertAllSorted<T>(enumerators.Select(GetUnderlying<ISortedCollection<T>, IEnumerator<T>>)));
        }
    }
}
