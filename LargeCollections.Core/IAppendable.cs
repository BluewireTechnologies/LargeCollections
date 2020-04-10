using System.Collections.Generic;

namespace LargeCollections.Core
{
    public interface IAppendable<T>
    {
        void Add(T item);
        void AddRange(IEnumerable<T> items);
        long Count { get; }
    }
}