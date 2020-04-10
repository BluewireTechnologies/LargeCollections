using System;
using System.Collections.Generic;
using System.Linq;
using LargeCollections.Core;
using LargeCollections.Operations;
using NUnit.Framework;

namespace LargeCollections.Tests.Operations
{
    [TestFixture, CheckResources]
    public class SetUnionSortPreservingMergeTests
    {
        [Test]
        public void SortedEnumerableMergerRequiresSortedInputs()
        {
            var items = new[] { 1, 2, 3, 4, 5 };
            Assert.Catch<InvalidOperationException>(() =>
                new SortedEnumerableMerger<int>(new[] { items }, new SetUnionSortPreservingMerge<int>()));
        }

        [Test]
        public void SingleSortedEnumerableIsInvariant()
        {
            var items = Sorted(1, 2, 3, 4, 5);
            var merged = new SortedEnumerableMerger<int>(new[] {items}, new SetUnionSortPreservingMerge<int>());

            CollectionAssert.AreEqual(items, merged);
        }

        private IEnumerable<int> Sorted(params int[] items)
        {
            return items.UsesSortOrder(Comparer<int>.Default);
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

            CollectionAssert.IsOrdered(merged, Comparer<int>.Default);
        }

        [Test]
        public void RetainsSortOrderMetaInformation()
        {
            var itemSets = new[] {
                Sorted(2, 4, 6, 7, 9, 12),
                Sorted(1, 3, 4, 7),
                Sorted(5, 8, 10, 11),
            };
            var merged = new SortedEnumerableMerger<int>(itemSets, new SetUnionSortPreservingMerge<int>());

            Assert.IsNotNull(merged.GetUnderlying<ISorted<int>>());
            Assert.AreEqual(Comparer<int>.Default, merged.GetUnderlying<ISorted<int>>().SortOrder);
        }
    }
}
