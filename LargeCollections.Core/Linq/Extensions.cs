using System;
using System.Collections;
using System.Collections.Generic;

namespace LargeCollections.Core.Linq
{
    public static class Extensions
    {
        public static IDisposableEnumerable<T> AsDisposable<T>(this IEnumerable<T> enumerable)
        {
            return enumerable as IDisposableEnumerable<T> ?? new DisposableEnumerable<T>(enumerable);
        }

        public static long Count<T>(this IEnumerator<T> enumerator)
        {
            var counted = enumerator.GetUnderlying<ICounted>();
            if (counted == null) throw new InvalidOperationException("Not a counted enumerator.");
            return counted.Count;
        }
    }

    public class DisposableEnumerable<T> : IDisposableEnumerable<T>, IHasUnderlying
    {
        private readonly IEnumerable<T> enumerable;

        public DisposableEnumerable(IEnumerable<T> enumerable)
        {
            this.enumerable = enumerable;
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            return enumerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            if (enumerable is IDisposable) ((IDisposable)enumerable).Dispose();
        }

        public object Underlying
        {
            get { return enumerable; }
        }
    }
}
