using System.Collections.Generic;
using System.Linq;
using LargeCollections.Linq;

namespace LargeCollections.Operations
{
    public abstract class SortPreservingMergeBase<T> : ISortedMerge<T>
    {
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
            return new GuardedDisposalEnumerator<T>(MergeEnumerators(sortedEnumerators).UsesSortOrder(sortedEnumerators.Comparison), sortedEnumerators);
        }

        

        protected abstract IEnumerator<T> MergeEnumerators(SortedEnumeratorList<T> sortedEnumerators);
    }
}