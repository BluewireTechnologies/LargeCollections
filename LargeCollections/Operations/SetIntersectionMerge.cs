using System;
using System.Collections.Generic;
using System.Linq;

namespace LargeCollections.Operations
{
    public class SetIntersectionMerge<T> : SortPreservingMergeBase<T>
    {
        protected override IEnumerator<T> WrapSource(IEnumerator<T> source)
        {
            return new SortedDistinctEnumerator<T>(source);
        }

        protected override IEnumerator<T> MergeEnumerators(SortedEnumeratorList<T> sortedEnumerators)
        {
            if (sortedEnumerators.AdvanceAll())
            {
                do
                {
                    var enumeratorsMatchingTheLowest = GetMatchingEnumerators(sortedEnumerators);
                    if (enumeratorsMatchingTheLowest.Count() == sortedEnumerators.Total)
                    {
                        yield return sortedEnumerators.First().Current;
                        if (!sortedEnumerators.AdvanceAll()) break;
                    }
                    else
                    {
                        foreach (var enumerator in enumeratorsMatchingTheLowest)
                        {
                            sortedEnumerators.Advance(enumerator);
                        }
                    }
                } while (sortedEnumerators.All());
            }
        }
    }
}