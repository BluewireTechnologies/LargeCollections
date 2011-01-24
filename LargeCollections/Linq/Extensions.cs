using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LargeCollections.Operations;

namespace LargeCollections.Linq
{
    public static class Extensions
    {
        public static ISinglePassCollection<IEnumerable<T>> Batch<T>(this ISinglePassCollection<T> enumerator, int batchSize)
        {
            return new BatchedSinglePassCollection<T>(enumerator, batchSize);
        }

        public static ISinglePassCollection<IEnumerable<T>> Batch<T>(this ILargeCollection<T> collection, int batchSize)
        {
            return new BatchedSinglePassCollection<T>(collection.AsSinglePass(), batchSize);
        }

        public static ILargeCollection<T> Sort<T>(this ISinglePassCollection<T> enumerator, IAccumulatorSelector accumulatorSelector)
        {
            var sorter = accumulatorSelector.GetOperator(() => new LargeCollectionSorter(accumulatorSelector));
            return sorter.Sort(enumerator);
        }

        public static ILargeCollection<T> Sort<T>(this ISinglePassCollection<T> enumerator, IAccumulatorSelector accumulatorSelector, IComparer<T> comparison)
        {
            var sorter = accumulatorSelector.GetOperator(() => new LargeCollectionSorter(accumulatorSelector));
            return sorter.Sort(enumerator, comparison);
        }

        public static ILargeCollection<T> Concat<T>(this ILargeCollection<T> first, ILargeCollection<T> second)
        {
            return new ConcatenatedLargeCollection<T>(first, second);
        }

        public static ISinglePassCollection<T> Concat<T>(this ISinglePassCollection<T> first, ISinglePassCollection<T> second)
        {
            return new ConcatenatedSinglePassCollection<T>(first, second);
        }
    }
}
