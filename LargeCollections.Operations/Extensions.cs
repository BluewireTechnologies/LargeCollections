using System.Collections.Generic;
using Bluewire.ReferenceCounting;
using LargeCollections.Collections;

namespace LargeCollections.Operations
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
    }
}
