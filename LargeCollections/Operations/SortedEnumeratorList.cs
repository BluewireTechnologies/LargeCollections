using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LargeCollections.Linq;
using LargeCollections.Resources;
using Extensions = LargeCollections.Linq.Extensions;

namespace LargeCollections.Operations
{
    public class SortedEnumeratorList<T> : IDisposableEnumerable<IEnumerator<T>>, ISorted<IEnumerator<T>>, ICounted
    {
        public IComparer<T> Comparison { get; private set; }
        private List<IEnumerator<T>> sortedList = new List<IEnumerator<T>>();
        private DisposableList<IEnumerator<T>> enumerators;



        public SortedEnumeratorList(IEnumerable<IEnumerator<T>> enumerators) : this(enumerators, e => e)
        {
        }

        public SortedEnumeratorList(IEnumerable<IEnumerator<T>> enumerators, Func<IEnumerator<T>, IEnumerator<T>> mapping)
        {
            this.enumerators = new DisposableList<IEnumerator<T>>(enumerators.EvaluateSafely(mapping));
            try
            {
                Comparison = this.enumerators.GetCommonSortOrder();
                SortOrder = new EnumeratorComparer(Comparison);
                // initialise the list.
                sortedList = new List<IEnumerator<T>>(this.enumerators.Select(e => new CachingEnumerator<T>(e)).Cast<IEnumerator<T>>());
            }
            catch
            {
                this.enumerators.Dispose();
                throw;
            }
            
        }

        class EnumeratorComparer : IComparer<IEnumerator<T>>
        {
            private readonly IComparer<T> comparison;

            public EnumeratorComparer(IComparer<T> comparison)
            {
                this.comparison = comparison;
            }

            public int Compare(IEnumerator<T> x, IEnumerator<T> y)
            {
                return comparison.Compare(x.Current, y.Current);
            }
        }

        
        private int GetInsertionIndex(List<IEnumerator<T>> orderedList, IEnumerator<T> queue)
        {
            var index = orderedList.BinarySearch(queue, SortOrder);
            if (index < 0) return ~index;
            return index;
        }

        public bool Advance(IEnumerator<T> enumerator)
        {
            if (enumerator.MoveNext())
            {
                sortedList.Remove(enumerator);
                // more items in that queue.
                var insertionIndex = GetInsertionIndex(sortedList, enumerator);
                // if the next item is not the lowest current value, move the queue to the right location.
                sortedList.Insert(insertionIndex, enumerator);
                return true;
            }
            else
            {
                // no more items in that queue. discard it.
                sortedList.Remove(enumerator);
                enumerator.Dispose();
                return false;
            }
        }

        public bool AdvanceAll()
        {
            sortedList = new List<IEnumerator<T>>(sortedList.Where(e => e.MoveNext()));
            sortedList.Sort(SortOrder);
            return sortedList.Any();
        }

        public IEnumerator<IEnumerator<T>> GetEnumerator()
        {
            return sortedList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            enumerators.Dispose();
        }

        public IComparer<IEnumerator<T>> SortOrder { get; private set; }

        public long Total
        {
            get { return enumerators.Count; }
        }

        public long Count
        {
            get { return sortedList.Count; }
        }

        /// <summary>
        /// Returns true if none of the underlying enumerators have terminated yet.
        /// </summary>
        /// <returns></returns>
        public bool All()
        {
            return enumerators.Count == sortedList.Count;
        }
    }
}