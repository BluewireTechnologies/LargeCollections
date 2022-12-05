using System;
using System.Collections.Generic;
using System.Linq;

namespace LargeCollections.Core.Operations
{
    internal static class CountInheritance
    {
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

        public static IEnumerator<T> InheritsCount<T>(this IEnumerator<T> enumerator, params object[] sources)
        {
            var counts = sources.Select(s => TryGetCount(enumerator, s)).ToArray();
            if (counts.All(c => c.HasValue))
            {
                return new CountedEnumerator<T>(enumerator, counts.Sum(c => c.Value));
            }
            return enumerator;
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
