using System.Collections.Generic;
using LargeCollections.Operations;

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


        public IDisposableEnumerable<T> Buffer<T>(IEnumerable<T> enumerable)
        {
            using(var enumerator = enumerable.GetEnumerator())
            {
                return Buffer(enumerator);
            }
        }

        public IDisposableEnumerable<T> Buffer<T>(IEnumerator<T> enumerator)
        {
            var countable = enumerator.GetUnderlying<ICounted>();
            if (countable != null)
            {
                return enumerator.Buffer(accumulatorSelector.GetAccumulator<T>(countable.Count));
            }
            return enumerator.Buffer(accumulatorSelector.GetAccumulator<T>());
        }

        public IEnumerator<T> BufferOnce<T>(IEnumerator<T> enumerator)
        {
            using(var collection = Buffer(enumerator))
            {
                return collection.GetEnumerator();
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
}