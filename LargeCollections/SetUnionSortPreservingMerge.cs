using System;
using System.Collections.Generic;
using System.Linq;

namespace LargeCollections
{
    public class SetUnionSortPreservingMerge<T> : ISortedMerge<T>
    {
        public bool MoveNext(IList<IEnumerator<T>> enumerators, Func<IEnumerator<T>, bool> advance)
        {
            if (enumerators.Any())
            {
                advance(enumerators[0]);
                return enumerators.Any();
            }
            return false;
        }

        public T GetCurrent(IList<IEnumerator<T>> enumerators)
        {
            return enumerators.First().Current;
        }


        public bool MoveFirst(IList<IEnumerator<T>> enumerators, Func<IEnumerator<T>, bool> advance)
        {
            return enumerators.Any();
        }


        public IEnumerator<T> WrapSource(IEnumerator<T> enumerator)
        {
            return enumerator;
        }
    }
}