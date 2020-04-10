using System;
using System.Collections.Generic;

namespace LargeCollections.Operations
{
    public interface ISortedMerge<T>
    {
        IEnumerator<T> Merge(IEnumerable<IEnumerator<T>> enumerators);
    }
}