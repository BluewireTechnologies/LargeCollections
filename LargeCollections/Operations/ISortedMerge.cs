using System;
using System.Collections.Generic;

namespace LargeCollections.Operations
{
    public interface ISortedMerge<T>
    {
        bool MoveNext(IList<IEnumerator<T>> enumerators, Func<IEnumerator<T>, bool> advance);

        T GetCurrent(IList<IEnumerator<T>> enumerators);
        bool MoveFirst(IList<IEnumerator<T>> enumerators, Func<IEnumerator<T>, bool> advance);
        IEnumerator<T> WrapSource(IEnumerator<T> enumerator);
    }
}