using System;
using System.Collections.Generic;
using System.Linq;
using LargeCollections.Linq;
using LargeCollections.Operations;
using MbUnit.Framework;

namespace LargeCollections.Tests.Operations
{
    [TestFixture]
    public class SortedDistinctEnumeratorTests
    {
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SortedDistinctEnumeratorThrowsExceptionIfSetIsNotSorted()
        {
            var set = new List<int> { 1, 2, 2, 3, 5, 5, 5, 6, 7, 8, 9, 10, 10 };

            new SortedDistinctEnumerator<int>(set.GetEnumerator());
        }

        [Test]
        public void SortedDistinctEnumeratorRemovesAdjacentDuplicates()
        {
            var set = new List<int> {1, 2, 2, 3, 5, 5, 5, 6, 7, 8, 9, 10, 10}.UsesSortOrder(Comparer<int>.Default);

            var distinctList = new List<int>();
            using(var distinctSet = new SortedDistinctEnumerator<int>(set.GetEnumerator()))
            {
                while (distinctSet.MoveNext())
                {
                    distinctList.Add(distinctSet.Current);
                }
            }

            Assert.AreElementsEqual(distinctList, set.Distinct());
        }
    }
}
