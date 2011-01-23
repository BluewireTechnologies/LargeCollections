using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LargeCollections
{
    public class SortedEnumerableMerger<T> : IEnumerable<T>
    {
        private readonly IList<IEnumerable<T>> enumerables;
        private readonly IComparer<T> comparison;
        private readonly ISortedMerge<T> merger;

        public SortedEnumerableMerger(IList<IEnumerable<T>> enumerables, IComparer<T> comparison, ISortedMerge<T> merger)
        {
            this.enumerables = enumerables;
            this.comparison = comparison;
            this.merger = merger;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new SortedEnumeratorMerger<T>(enumerables.Select(e => e.GetEnumerator()).ToList(), comparison, merger);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class SortedEnumeratorMerger<T> : IEnumerator<T>
    {
        private readonly DisposableList<IEnumerator<T>> enumerators;
        private readonly IComparer<T> comparison;
        private readonly ISortedMerge<T> merger;
        private readonly List<IEnumerator<T>> batchesByHeadValue;
        private bool first = true;

        public SortedEnumeratorMerger(IList<IEnumerator<T>> enumerators, IComparer<T> comparison, ISortedMerge<T> merger)
        {
            this.enumerators = new DisposableList<IEnumerator<T>>(enumerators);
            this.comparison = comparison;
            this.merger = merger;
            batchesByHeadValue = new List<IEnumerator<T>>();

        }

        private void MoveFirst()
        {
            // initialise the list.
            foreach (var queue in enumerators.Select(e => merger.WrapSource(e)).Where(e => e.MoveNext()))
            {
                batchesByHeadValue.Insert(GetInsertionIndex(0, batchesByHeadValue, queue), queue);
            }
        }

        private int GetInsertionIndex(int start, List<IEnumerator<T>> orderedList, IEnumerator<T> queue)
        {
            var index = orderedList.FindIndex(start, t => comparison.Compare(t.Current, queue.Current) >= 0);
            if (index < 0) return orderedList.Count;
            return index;
        }

        public T Current { get { return merger.GetCurrent(batchesByHeadValue); } }

        public void Dispose()
        {
            enumerators.Dispose();
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            if (first)
            {
                MoveFirst();
                first = false;
                return merger.MoveFirst(batchesByHeadValue, Advance);
            }
            return merger.MoveNext(batchesByHeadValue, Advance);
        }

        private bool Advance(IEnumerator<T> enumerator)
        {
            if (enumerator.MoveNext())
            {
                batchesByHeadValue.Remove(enumerator);
                // more items in that queue.
                var insertionIndex = GetInsertionIndex(0, batchesByHeadValue, enumerator);
                // if the next item is not the lowest current value, move the queue to the right location.
                batchesByHeadValue.Insert(insertionIndex, enumerator);
                return true;
            }
            else
            {
                // no more items in that queue. discard it.
                batchesByHeadValue.Remove(enumerator);
                enumerator.Dispose();
                return false;
            }
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }
    }

    class SortedEnumeratorList<T> : IEnumerable<IEnumerator<T>>
    {
        public IComparer<T> Comparison { get; private set; }
        private LinkedList<SortedItemSource> sortedList = new LinkedList<SortedItemSource>();


        public SortedEnumeratorList(IList<IEnumerator<T>> enumerators, IComparer<T> comparison)
        {
            Comparison = comparison;
        }

        public class SortedItemSource : IEnumerator<T>
        {
            private readonly IEnumerator<T> enumerator;
            public LinkedListNode<SortedItemSource> ListNode { get; private set; }
        
            public SortedItemSource(IEnumerator<T> enumerator)
            {
                this.enumerator = enumerator;
                ListNode = new LinkedListNode<SortedItemSource>(this);
            }

            public T Current
            {
                get { return enumerator.Current; }
            }

            public void Dispose()
            {
                enumerator.Dispose();
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                return enumerator.MoveNext();
            }

            public void Reset()
            {
                enumerator.Reset();
            }
        }

        public bool Advance(IEnumerator<T> enumerator)
        {
            var itemSource = (SortedItemSource) enumerator;
            return false;
        }


        public IEnumerator<IEnumerator<T>> GetEnumerator()
        {
            return sortedList.ToArray().Cast<IEnumerator<T>>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}