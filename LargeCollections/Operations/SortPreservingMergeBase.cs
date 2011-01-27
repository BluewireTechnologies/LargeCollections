using System;
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
            return new GuardedDisposalEnumerator(MergeEnumerators(sortedEnumerators).UsesSortOrder(sortedEnumerators.Comparison), sortedEnumerators);
        }

        class GuardedDisposalEnumerator : IEnumerator<T>, IHasUnderlying
        {
            private readonly IEnumerator<T> underlying;
            private readonly IDisposable guarded;

            public GuardedDisposalEnumerator(IEnumerator<T> underlying, IDisposable guarded)
            {
                this.underlying = underlying;
                this.guarded = guarded;
            }

            public T Current
            {
                get { return underlying.Current; }
            }

            public void Dispose()
            {
                underlying.Dispose();
                guarded.Dispose();
            }

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                return underlying.MoveNext();
            }

            public void Reset()
            {
                underlying.Reset();
            }

            public object Underlying
            {
                get { return underlying; }
            }
        }

        protected abstract IEnumerator<T> MergeEnumerators(SortedEnumeratorList<T> sortedEnumerators);
    }
}