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

        public static IDisposableEnumerable<T> Buffer<T>(this IEnumerator<T> enumerator)
        {
            return enumerator.Buffer(new InMemoryAccumulator<T>());
        }

        public static IDisposableEnumerable<T> Buffer<T>(this IEnumerator<T> enumerator, IAccumulator<T> accumulator)
        {
            using (accumulator)
            {
                while (enumerator.MoveNext())
                {
                    accumulator.Add(enumerator.Current);
                }
                return accumulator.Complete().InheritsSortOrder(enumerator);
            }
        }

        public static IEnumerator<T> BufferOnce<T>(this IEnumerator<T> enumerator, IAccumulator<T> accumulator)
        {
            using (var collection = enumerator.Buffer(accumulator))
            {
                return collection.GetEnumerator();
            }
        }

        public static IDisposableEnumerable<T> AsDisposable<T>(this IEnumerable<T> enumerable)
        {
            return enumerable as IDisposableEnumerable<T> ?? new DisposableEnumerable<T>(enumerable);
        }

        public static IList<T> EvaluateSafely<T>(this IEnumerable<T> enumerable)
        {
            using(var list = new DisposableList<T>())
            {
                foreach(var entry in enumerable)
                {
                    list.Add(entry);
                }
                var allEntries = list.ToArray();
                list.Clear(); // prevent disposing, since we successfully evaluated the enumerable.
                return allEntries;
            }
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
