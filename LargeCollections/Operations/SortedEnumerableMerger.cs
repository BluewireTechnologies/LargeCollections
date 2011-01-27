using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LargeCollections.Operations
{
    public class SortedEnumerableMerger<T> : MultipleCollection<T>, IEnumerable<T>, ISorted<T>
    {
        private readonly IList<IEnumerable<T>> enumerables;
        private readonly IComparer<T> comparison;
        private readonly ISortedMerge<T> merger;

        public SortedEnumerableMerger(IList<IEnumerable<T>> enumerables, ISortedMerge<T> merger) : base(enumerables.ToArray())
        {
            this.comparison = enumerables.GetCommonSortOrder();
            this.enumerables = enumerables;
            
            this.merger = merger;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return merger.Merge(enumerables.Select(e => e.GetEnumerator()));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IComparer<T> SortOrder
        {
            get { return comparison; }
        }
    }
}