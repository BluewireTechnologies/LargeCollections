using System;
using System.Collections.Generic;
using System.Linq;
using LargeCollections.Operations;
using MbUnit.Framework;

namespace LargeCollections.Tests.Operations
{
    [TestFixture]
    public class SetUnionSortPreservingMergeTests
    {
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SortedEnumerableMergerRequiresSortedInputs()
        {
            var items = new[] { 1, 2, 3, 4, 5 };
            var merged = new SortedEnumerableMerger<int>(new[] { items }, new SetUnionSortPreservingMerge<int>());

            Assert.AreElementsEqual(items, merged);
        }

        [Test]
        public void SingleSortedEnumerableIsInvariant()
        {
            var items = Sorted(1, 2, 3, 4, 5);
            var merged = new SortedEnumerableMerger<int>(new[] {items}, new SetUnionSortPreservingMerge<int>());

            Assert.AreElementsEqual(items, merged);
        }

        private IEnumerable<int> Sorted(params int[] items)
        {
            return new SortedEnumerable<int>(items, Comparer<int>.Default);
        }

        [Test]
        public void MultipleSortedEnumerablesAreMergedRetainingOrder()
        {
            var itemSets = new[] {
                Sorted(2, 4, 6, 7, 9, 12),
                Sorted(1, 3, 4, 7),
                Sorted(5, 8, 10, 11),
            };
            var merged = new SortedEnumerableMerger<int>(itemSets, new SetUnionSortPreservingMerge<int>());

            Assert.Sorted(merged, SortOrder.Increasing);
        }

        [TearDown]
        public void TearDown()
        {
            Utils.AssertReferencesDisposed();
        }

    }
}
