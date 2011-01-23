using System.Collections.Generic;

namespace LargeCollections
{
    public interface IAppendable<T>
    {
        void Add(T item);
        void AddRange(IEnumerable<T> items);
        long Count { get; }
    }
}