using System;
using System.Collections.Generic;
using System.Linq;

namespace LargeCollections.Operations
{
    public class SetIntersectionMerge<T> : ISortedMerge<T>
    {
        public IEnumerator<T> WrapSource(IEnumerator<T> enumerator)
        {
            return new SortedDistinctEnumerator<T>(enumerator);
        }

        public bool MoveNext(IList<IEnumerator<T>> enumerators, Func<IEnumerator<T>, bool> advance)
        {
            if (enumerators.Any())
            {
                var enumeratorsMatchingTheLowest = GetMatchingEnumerators(enumerators);
                do
                {
                    foreach (var enumerator in enumeratorsMatchingTheLowest)
                    {
                        advance(enumerator);
                    }
                    if (!enumerators.Any()) return false;
                    enumeratorsMatchingTheLowest = GetMatchingEnumerators(enumerators);
                } while (enumeratorsMatchingTheLowest.Count() < enumerators.Count);
                return true;
            }
            return false;
        }

        private IEnumerator<T>[] GetMatchingEnumerators(IList<IEnumerator<T>> enumerators)
        {
            var current = enumerators.First().Current;
            return enumerators.TakeWhile(e => Equals(e.Current, current)).ToArray();
        }

        public T GetCurrent(IList<IEnumerator<T>> enumerators)
        {
            return enumerators.First().Current;
        }


        public bool MoveFirst(IList<IEnumerator<T>> enumerators, Func<IEnumerator<T>, bool> advance)
        {
            if (enumerators.Any())
            {
                var enumeratorsMatchingTheLowest = GetMatchingEnumerators(enumerators);
                while (enumeratorsMatchingTheLowest.Count() < enumerators.Count)
                {
                    foreach (var enumerator in enumeratorsMatchingTheLowest)
                    {
                        advance(enumerator);
                    }
                    if (!enumerators.Any()) return false;
                    enumeratorsMatchingTheLowest = GetMatchingEnumerators(enumerators);
                }
                return true;
            }
            return false;
        }
    }
}