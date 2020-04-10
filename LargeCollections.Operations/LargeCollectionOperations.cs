using System;
using System.Collections.Generic;
using System.Linq;
using Bluewire.ReferenceCounting;

namespace LargeCollections.Operations
{
    public class LargeCollectionOperations
    {
        public IAccumulatorSelector AccumulatorSelector { get; private set; }

        public LargeCollectionOperations(IAccumulatorSelector accumulatorSelector)
        {
            this.AccumulatorSelector = accumulatorSelector;
        }

        public IEnumerator<T> Sort<T>(IEnumerator<T> enumerator)
        {
            var sorter = this.AccumulatorSelector.GetOperator(() => new LargeCollectionSorter(this.AccumulatorSelector));
            return sorter.Sort(enumerator, Comparer<T>.Default);
        }

        public IEnumerator<T> Sort<T>(IEnumerator<T> enumerator, IComparer<T> comparison)
        {
            var sorter = this.AccumulatorSelector.GetOperator(() => new LargeCollectionSorter(this.AccumulatorSelector));
            return sorter.Sort(enumerator, comparison);
        }


        public IDisposableEnumerable<T> Buffer<T>(IEnumerable<T> enumerable)
        {
            return enumerable.GetEnumerator().UseSafely(Buffer);
        }

        public IDisposableEnumerable<T> Buffer<T>(IEnumerator<T> enumerator)
        {
            var countable = enumerator.GetUnderlying<ICounted>();
            if (countable != null)
            {
                return enumerator.Buffer(this.AccumulatorSelector.GetAccumulator<T>(countable.Count));
            }
            return enumerator.Buffer(this.AccumulatorSelector.GetAccumulator<T>());
        }

        public IEnumerator<T> BufferOnce<T>(IEnumerator<T> enumerator)
        {
            return Buffer(enumerator).UseSafely(c => c.GetEnumerator());
        }

        /// <summary>
        /// Buffers the output of an operation if it is not the same object as the input.
        /// </summary>
        /// <remarks>
        /// Used to efficiently buffer the output of operations which may return their argument unchanged, eg. sorts.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerator">Enumerator to process</param>
        /// <param name="operation">Operation which may return the input enumerator</param>
        /// <returns></returns>
        public IEnumerator<T> BufferOnceIfDifferent<T>(IEnumerator<T> enumerator, Func<IEnumerator<T>, IEnumerator<T>> operation)
        {
            var output = operation(enumerator);
            if (ReferenceEquals(enumerator, output)) return output;

            return BufferOnce(output);
        }

        public IEnumerator<T> AsCounted<T>(IEnumerator<T> enumerator)
        {
            if (enumerator.GetUnderlying<ICounted>() != null) return enumerator;
            return BufferOnce(enumerator);
        }

        private IComparer<T> GetPreferredSortOrder<T>(params IEnumerator<T>[] enumerators)
        {
            var sortedEnumerators = enumerators .Select(e => e.GetUnderlying<ISorted<T>>()).Where(s => s != null).ToArray();
            
            var sortOrders = sortedEnumerators
                .GroupBy(s => s.SortOrder)
                .OrderByDescending(s => s.Count())
                .ToArray();

            if (!sortOrders.Any()) return Comparer<T>.Default;

            var preferredOrder = sortOrders.First();

            return preferredOrder.Key;
        }

        public IEnumerator<T> Difference<T>(IEnumerator<T> first, IEnumerator<T> second)
        {
            return Difference(first, second, GetPreferredSortOrder(first, second));
        }

        public IEnumerator<T> Difference<T>(IEnumerator<T> first, IEnumerator<T> second, IComparer<T> comparison)
        {
            var setA = BufferOnceIfDifferent(first, e => Sort(e, comparison));
            var setB = BufferOnceIfDifferent(second, e => Sort(e, comparison));
            return new SetDifferenceMerge<T>().Merge(new List<IEnumerator<T>> { setA, setB });
        }

        public IEnumerator<T> Intersection<T>(IEnumerator<T> first, IEnumerator<T> second)
        {
            return Intersection(first, second, Comparer<T>.Default);
        }

        public IEnumerator<T> Intersection<T>(IEnumerator<T> first, IEnumerator<T> second, IComparer<T> comparison)
        {
            var setA = BufferOnceIfDifferent(first, e => Sort(e, comparison));
            var setB = BufferOnceIfDifferent(second, e => Sort(e, comparison));
            return new SetIntersectionMerge<T>().Merge(new List<IEnumerator<T>> { setA, setB });
        }

        public IEnumerator<T> DifferenceWithIntersection<T>(IEnumerator<T> first, IEnumerator<T> second)
        {
            return DifferenceWithIntersection(first, second, Comparer<T>.Default);
        }

        public IEnumerator<T> DifferenceWithIntersection<T>(IEnumerator<T> first, IEnumerator<T> second, IComparer<T> comparison)
        {
            var setA = BufferOnceIfDifferent(first, e => Sort(e, comparison));
            var setB = BufferOnceIfDifferent(second, e => Sort(e, comparison));
            return new SetDifferenceAndIntersectionMerge<T>(this.AccumulatorSelector).Merge(new List<IEnumerator<T>> { setA, setB });
        }
    }
}