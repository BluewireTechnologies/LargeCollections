using System;
using System.Collections.Generic;
using System.Linq;

namespace LargeCollections.Operations
{
    public class SetUnionSortPreservingMerge<T> : SortPreservingMergeBase<T>
    {
        protected override IEnumerator<T> MergeEnumerators(SortedEnumeratorList<T> sortedEnumerators)
        {
            using (sortedEnumerators)
            {
                if (sortedEnumerators.AdvanceAll())
                {
                    do
                    {
                        var first = sortedEnumerators.First();
                        yield return first.Current;
                        sortedEnumerators.Advance(first);
                    } while (sortedEnumerators.Any());
                }
            }
        }
    }
}