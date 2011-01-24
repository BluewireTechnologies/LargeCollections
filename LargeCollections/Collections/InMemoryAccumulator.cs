using System.Collections.Generic;

namespace LargeCollections.Collections
{
    public class InMemoryAccumulator<T> : IAccumulator<T>
    {
        private readonly List<T> buffer = new List<T>();

        public void Add(T item)
        {
            buffer.Add(item);
        }

        public void AddRange(IEnumerable<T> items)
        {
            buffer.AddRange(items);
        }

        public long Count
        {
            get { return buffer.Count; }
        }

        public List<T> GetBuffer()
        {
            return buffer;
        }

        public ILargeCollection<T> Complete()
        {
            return new InMemoryLargeCollection<T>(buffer);
        }

        public void Dispose()
        {
        }

        public static ILargeCollection<T> From(IEnumerable<T> items)
        {
            using(var accumulator = new InMemoryAccumulator<T>())
            {
                accumulator.AddRange(items);
                return accumulator.Complete();
            }
        }

        private static readonly ILargeCollection<T> empty = new InMemoryLargeCollection<T>(new List<T>());

        public static ILargeCollection<T> Empty()
        {
            return empty;
        }
    }
}