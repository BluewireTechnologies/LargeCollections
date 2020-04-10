using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LargeCollections.Collections;
using LargeCollections.Operations;
using LargeCollections.Resources;

namespace LargeCollections.Linq
{
    public static class Extensions
    {
        public static IEnumerator<IEnumerable<T>> Batch<T>(this IEnumerator<T> enumerator, int batchSize)
        {
            return new BatchedSinglePassCollection<T>(enumerator, batchSize).InheritsCount(enumerator);
        }

        public static IEnumerator<IEnumerable<T>> Batch<T>(this ILargeCollection<T> collection, int batchSize)
        {
            return new BatchedSinglePassCollection<T>(collection.GetEnumerator(), batchSize).InheritsCount(collection);
        }

        
        public static ILargeCollection<T> Concat<T>(this ILargeCollection<T> first, ILargeCollection<T> second)
        {
            return new ConcatenatedLargeCollection<T>(first, second);
        }

        public static IEnumerator<T> Concat<T>(this IEnumerator<T> first, IEnumerator<T> second)
        {
            return new ConcatenatedEnumerator<T>(first, second).InheritsCount(first, second);
        }

        public static IDisposableEnumerable<T> BufferInMemory<T>(this IEnumerator<T> enumerator)
        {
            return enumerator.Buffer(new InMemoryAccumulator<T>());
        }

        public static IDisposableEnumerable<T> Buffer<T>(this IEnumerator<T> enumerator, IAccumulator<T> accumulator)
        {
            return accumulator.UseSafely(a =>
                enumerator.UseSafely(e =>
                {
                    while (e.MoveNext())
                    {
                        a.Add(e.Current);
                    }
                    return a.Complete().InheritsSortOrder(e);
                }));
        }

        public static IEnumerator<T> BufferOnce<T>(this IEnumerator<T> enumerator, IAccumulator<T> accumulator)
        {
            return enumerator.Buffer(accumulator).UseSafely(b => b.GetEnumerator());
        }

        public static IDisposableEnumerable<T> AsDisposable<T>(this IEnumerable<T> enumerable)
        {
            return enumerable as IDisposableEnumerable<T> ?? new DisposableEnumerable<T>(enumerable);
        }

        public static long Count<T>(this IEnumerator<T> enumerator)
        {
            var counted = enumerator.GetUnderlying<ICounted>();
            if(counted == null) throw new InvalidOperationException("Not a counted enumerator.");
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
            if(enumerable is IDisposable) ((IDisposable)enumerable).Dispose();
        }

        public object Underlying
        {
            get { return enumerable; }
        }
    }
}
