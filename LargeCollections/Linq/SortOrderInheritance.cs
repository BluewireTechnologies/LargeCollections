using System;
using System.Collections;
using System.Collections.Generic;

namespace LargeCollections.Linq
{
    public static class SortOrderInheritance
    {
        public static IDisposableEnumerable<T> UsesSortOrder<T>(this IEnumerable<T> collection, IComparer<T> sortOrder)
        {
            return new SortedEnumerable<T>(collection, sortOrder);
        }

        public static IEnumerator<T> UsesSortOrder<T>(this IEnumerator<T> enumerator, IComparer<T> sortOrder)
        {
            return new SortedEnumerator<T>(enumerator, sortOrder);
        }

        public static IDisposableEnumerable<T> InheritsSortOrder<T>(this IEnumerable<T> collection, object source)
        {
            var sortOrder = TryGetSortOrder<T>(collection, source);
            if (sortOrder == null) return collection.AsDisposable();
            return new SortedEnumerable<T>(collection, sortOrder);
        }

        private static IComparer<T> TryGetSortOrder<T>(object inheritor, object source)
        {
            if (inheritor.GetUnderlying<ISorted<T>>() != null) throw new InvalidOperationException("Inheritor is already sorted.");

            var sorted = source.GetUnderlying<ISorted<T>>();
            if (sorted == null) return null;
            return sorted.SortOrder;
        }

        public static IEnumerator<T> InheritsSortOrder<T>(this IEnumerator<T> enumerator, object source)
        {
            var sortOrder = TryGetSortOrder<T>(enumerator, source);
            if (sortOrder == null) return enumerator;
            return new SortedEnumerator<T>(enumerator, sortOrder);
        }

        class SortedEnumerable<T> : DisposableEnumerable<T>, ISorted<T>
        {
            public SortedEnumerable(IEnumerable<T> enumerable, IComparer<T> order) : base(enumerable)
            {
                SortOrder = order;
            }

            public override IEnumerator<T> GetEnumerator()
            {
                return new SortedEnumerator<T>(base.GetEnumerator(), SortOrder);
            }

            public IComparer<T> SortOrder { get; private set; }
        }

        class SortedEnumerator<T> : IEnumerator<T>, ISorted<T>, IHasUnderlying
        {
            private readonly IEnumerator<T> enumerator;

            public SortedEnumerator(IEnumerator<T> enumerator, IComparer<T> order)
            {
                this.enumerator = enumerator;
                SortOrder = order;
            }


            public T Current { get { return enumerator.Current; } }

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
                throw new NotSupportedException();
            }

            public IComparer<T> SortOrder { get; private set; }

            public object Underlying
            {
                get { return enumerator; }
            }
        }
    }
}