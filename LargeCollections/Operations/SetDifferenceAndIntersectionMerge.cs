using System;
using System.Collections.Generic;
using System.Linq;

namespace LargeCollections.Operations
{
    public class SetDifferenceAndIntersectionMerge<T> : ISortedMerge<T>
    {
        private readonly IAccumulatorSelector accumulatorSelector;

        public SetDifferenceAndIntersectionMerge(IAccumulatorSelector accumulatorSelector)
        {
            this.accumulatorSelector = accumulatorSelector;
        }

        protected IEnumerator<T>[] GetMatchingEnumerators(SortedEnumeratorList<T> enumerators)
        {
            var current = enumerators.First().Current;
            return enumerators.TakeWhile(e => enumerators.Comparison.Compare(e.Current, current) == 0).ToArray();
        }

        protected virtual IEnumerator<T> WrapSource(IEnumerator<T> source)
        {
            return source;
        }

        public IEnumerator<T> Merge(IEnumerable<IEnumerator<T>> enumerators)
        {
            var sortedEnumerators = new SortedEnumeratorList<T>(enumerators, WrapSource);
            if (sortedEnumerators.Count != 2) throw new ArgumentException("SetDifferenceAndIntersection applies only to pairs of collections.");
            return new GuardedDisposalEnumerator<T>(MergeEnumerators(sortedEnumerators), sortedEnumerators);
        }

        private IEnumerator<T> MergeEnumerators(SortedEnumeratorList<T> sortedEnumerators)
        {
            if (sortedEnumerators.AdvanceAll())
            {
                using (var intersectionAccumulator = accumulatorSelector.GetAccumulator<T>())
                {
                    do
                    {
                        var enumeratorsMatchingTheLowest = GetMatchingEnumerators(sortedEnumerators);
                        var lowest = enumeratorsMatchingTheLowest.First();
                        if (enumeratorsMatchingTheLowest.Count() == 1)
                        {
                            yield return lowest.Current;
                        }
                        else
                        {
                            intersectionAccumulator.Add(lowest.Current);
                        }
                        foreach (var enumerator in enumeratorsMatchingTheLowest)
                        {
                            sortedEnumerators.Advance(enumerator);
                        }
                    } while (sortedEnumerators.Any());

                    using(var intersection = intersectionAccumulator.Complete())
                    {
                        foreach(var item in intersection)
                        {
                            yield return item;
                        }
                    }
                }
            }
        }
    }
}