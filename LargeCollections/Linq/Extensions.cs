using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LargeCollections.Collections;
using LargeCollections.Operations;
using LargeCollections.Resources;

namespace LargeCollections.Linq
{
    public class LargeCollectionOperations
    {
        private readonly IAccumulatorSelector accumulatorSelector;

        public LargeCollectionOperations(IAccumulatorSelector accumulatorSelector)
        {
            this.accumulatorSelector = accumulatorSelector;
        }

        public IEnumerator<T> Sort<T>(IEnumerator<T> enumerator)
        {
            var sorter = accumulatorSelector.GetOperator(() => new LargeCollectionSorter(accumulatorSelector));
            return sorter.Sort(enumerator, Comparer<T>.Default);
        }

        public IEnumerator<T> Sort<T>(IEnumerator<T> enumerator, IComparer<T> comparison)
        {
            var sorter = accumulatorSelector.GetOperator(() => new LargeCollectionSorter(accumulatorSelector));
            return sorter.Sort(enumerator, comparison);
        }


        public ILargeCollection<T> Buffer<T>(IEnumerable<T> enumerable)
        {
            using(var enumerator = enumerable.GetEnumerator())
            {
                return Buffer(enumerator);
            }
        }

        public ILargeCollection<T> Buffer<T>(IEnumerator<T> enumerator)
        {
            var countable = enumerator.GetUnderlying<ICountable>();
            if (countable != null)
            {
                return Buffer(enumerator, accumulatorSelector.GetAccumulator<T>(countable.Count));
            }
            return Buffer(enumerator, accumulatorSelector.GetAccumulator<T>());
        }

        private ILargeCollection<T> Buffer<T>(IEnumerator<T> enumerator, IAccumulator<T> accumulator)
        {
            using (accumulator)
            {
                while (enumerator.MoveNext())
                {
                    accumulator.Add(enumerator.Current);
                }
                return accumulator.Complete();
            }
        }

        public IEnumerator<T> Difference<T>(IEnumerator<T> first, IEnumerator<T> second)
        {
            return Difference(first, second, Comparer<T>.Default);
        }

        public IEnumerator<T> Difference<T>(IEnumerator<T> first, IEnumerator<T> second, IComparer<T> comparison)
        {
            var setA = Sort(first, comparison);
            var setB = Sort(second, comparison);
            return new SortedEnumeratorMerger<T>(new List<IEnumerator<T>> { setA, setB }, new SetDifferenceMerge<T>());
        }

        
    }

    public static class Extensions
    {
        public static IEnumerator<IEnumerable<T>> Batch<T>(this IEnumerator<T> enumerator, int batchSize)
        {
            return new BatchedSinglePassCollection<T>(enumerator, batchSize);
        }

        public static IEnumerator<IEnumerable<T>> Batch<T>(this ILargeCollection<T> collection, int batchSize)
        {
            return new BatchedSinglePassCollection<T>(collection.AsSinglePass(), batchSize);
        }

        
        public static ILargeCollection<T> Concat<T>(this ILargeCollection<T> first, ILargeCollection<T> second)
        {
            return new ConcatenatedLargeCollection<T>(first, second);
        }

        public static ISinglePassCollection<T> Concat<T>(this ISinglePassCollection<T> first, ISinglePassCollection<T> second)
        {
            return new ConcatenatedSinglePassCollection<T>(first, second);
        }
        
        public static ISinglePassCollection<T> AsSinglePass<T>(this ILargeCollection<T> collection)
        {
            return new SinglePassCollection<T>(collection);
        }

        public static ILargeCollection<T> Buffer<T>(this IEnumerator<T> enumerator)
        {
            using (var accumulator = new InMemoryAccumulator<T>())
            {
                while (enumerator.MoveNext())
                {
                    accumulator.Add(enumerator.Current);
                }
                return accumulator.Complete();
            }
        }
    }
}
