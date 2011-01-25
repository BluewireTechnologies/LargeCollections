using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace LargeCollections.Linq
{
    public static class CountInheritance
    {
        public static IEnumerable<T> InheritsCount<T>(this IEnumerable<T> collection, object source)
        {
            var count = TryGetCount(collection, source);
            if (count == null) return collection;
            return new CountedEnumerable<T>(collection, count.Value);
        }

        private static long? TryGetCount(object inheritor, object source)
        {
            if (inheritor.GetUnderlying<ICounted>() != null) throw new InvalidOperationException("Inheritor is already counted.");

            var counted = source.GetUnderlying<ICounted>();
            if (counted == null) return null;

            long count = counted.Count;
            var mapping = inheritor.GetUnderlying<IMappedCount>();
            if (mapping != null)
            {
                return mapping.MapCount(count);
            }
            return count;
        }

        public static IEnumerator<T> InheritsCount<T>(this IEnumerator<T> enumerator, object source)
        {
            var count = TryGetCount(enumerator, source);
            if (count == null) return enumerator;
            return new CountedEnumerator<T>(enumerator, count.Value);
        }

        class CountedEnumerable<T> : IEnumerable<T>, ICounted, IHasUnderlying
        {
            private readonly IEnumerable<T> underlying;

            public CountedEnumerable(IEnumerable<T> underlying, long count)
            {
                Count = count;
                this.underlying = underlying;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return new CountedEnumerator<T>(underlying.GetEnumerator(), Count);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public long Count { get; private set; }

            public object Underlying
            {
                get { return underlying; }
            }
        }

        class CountedEnumerator<T> : IEnumerator<T>, ICounted, IHasUnderlying
        {
            private IEnumerator<T> underlying;

            public CountedEnumerator(IEnumerator<T> underlying, long count)
            {
                this.underlying = underlying;
                Count = count;
            }

            public object Current
            {
                get { return underlying.Current; }
            }

            public bool MoveNext()
            {
                return underlying.MoveNext();
            }

            public void Reset()
            {
                underlying.Reset();
            }

            public long Count { get; private set; }

            public object Underlying { get { return underlying; } }

            T IEnumerator<T>.Current
            {
                get { return underlying.Current; }
            }

            public void Dispose()
            {
                underlying.Dispose();
            }
        }
    }
}
