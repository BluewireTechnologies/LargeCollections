using System;
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
            return new BatchedSinglePassCollection<T>(collection.AsSinglePass(), batchSize).InheritsCount(collection);
        }

        
        public static ILargeCollection<T> Concat<T>(this ILargeCollection<T> first, ILargeCollection<T> second)
        {
            return new ConcatenatedLargeCollection<T>(first, second);
        }

        public static ISinglePassCollection<T> Concat<T>(this ISinglePassCollection<T> first, ISinglePassCollection<T> second)
        {
            return new ConcatenatedSinglePassCollection<T>(first, second);
        }
        
        public static ISinglePassCollection<T> AsSinglePass<T>(this ILargeCollection<T> collection)
        {
            return new SinglePassCollection<T>(collection);
        }

        public static ILargeCollection<T> Buffer<T>(this IEnumerator<T> enumerator)
        {
            return enumerator.Buffer(new InMemoryAccumulator<T>());
        }

        public static ILargeCollection<T> Buffer<T>(this IEnumerator<T> enumerator, IAccumulator<T> accumulator)
        {
            using (accumulator)
            {
                while (enumerator.MoveNext())
                {
                    accumulator.Add(enumerator.Current);
                }
                return accumulator.Complete();
            }
        }

        public static ISinglePassCollection<T> BufferOnce<T>(this IEnumerator<T> enumerator, IAccumulator<T> accumulator)
        {
            using (var collection = enumerator.Buffer(accumulator))
            {
                return collection.AsSinglePass();
            }
        }
    }
}
