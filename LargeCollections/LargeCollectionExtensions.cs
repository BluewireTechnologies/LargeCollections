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
            var underlying = collection.GetUnderlying<IHasBackingStore<T>>();
            if (underlying == null) return default(T);
            return underlying.BackingStore;
        }

        public static T GetBackingStore<T>(this IEnumerator enumerator)
        {
            var underlying = enumerator.GetUnderlying<IHasBackingStore<T>>();
            if (underlying == null) return default(T);
            return underlying.BackingStore;
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

        public static TInterface GetUnderlying<TInterface>(this object item) where TInterface : class
        {   
            if(item is TInterface)
            {
                return item as TInterface;
            }
            var wrappingItem = item as IHasUnderlying;
            if (wrappingItem == null) return null;
            return GetUnderlying<TInterface>(wrappingItem.Underlying);
        }
        
        public static T GetOperator<T>(this IAccumulatorSelector accumulatorSelector, Func<T> createDefault)
        {
            if(accumulatorSelector is IOperatorCache)
            {
                return ((IOperatorCache)accumulatorSelector).GetInstance(createDefault);
            }
            return createDefault();
        }

        private static IEnumerable<ISorted<T>> AssertAllSorted<T>(IEnumerable<ISorted<T>> sortedEnumerables)
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

        private static IComparer<T> GetSingleSortOrder<T>(IEnumerable<ISorted<T>> enumerables)
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
            return GetSingleSortOrder(AssertAllSorted(enumerables.Select(GetUnderlying<ISorted<T>>)));
        }

        public static IComparer<T> GetCommonSortOrder<T>(this IList<IEnumerator<T>> enumerators)
        {
            return GetSingleSortOrder(AssertAllSorted(enumerators.Select(GetUnderlying<ISorted<T>>)));
        }
    }
}
