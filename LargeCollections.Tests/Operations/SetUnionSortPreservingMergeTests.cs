using System.Collections.Generic;
using LargeCollections.Operations;
using MbUnit.Framework;

namespace LargeCollections.Tests.Operations
{
    [TestFixture]
    public class SetUnionSortPreservingMergeTests
    {
        [Test]
        public void SingleSortedEnumerableIsInvariant()
        {
            var items = new[] {1, 2, 3, 4, 5};
            var merged = new SortedEnumerableMerger<int>(new[] {items}, Comparer<int>.Default, new SetUnionSortPreservingMerge<int>());

            Assert.AreElementsEqual(items, merged);
        }

        [Test]
        public void MultipleSortedEnumerablesAreMergedRetainingOrder()
        {
            var itemSets = new[] {
                new [] {2, 4, 6, 7, 9, 12 },
                new [] {1, 3, 4, 7 },
                new [] {5, 8, 10, 11 },
            };
            var merged = new SortedEnumerableMerger<int>(itemSets, Comparer<int>.Default, new SetUnionSortPreservingMerge<int>());

            Assert.Sorted(merged, SortOrder.Increasing);
        }

        [TearDown]
        public void TearDown()
        {
            Utils.AssertReferencesDisposed();
        }

    }
}
