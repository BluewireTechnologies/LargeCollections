using System;
using System.Collections.Generic;
using System.Linq;
using LargeCollections.Linq;
using LargeCollections.Operations;
using MbUnit.Framework;

namespace LargeCollections.Tests.Operations
{
    [TestFixture, CheckResources]
    public class SetDifferenceAndIntersectionMergeTests
    {
        private IEnumerable<int> Sorted(params int[] items)
        {
            return items.UsesSortOrder(Comparer<int>.Default);
        }

        private void AssertGrouped<T>(T[] result, T[] difference, T[] intersection)
        {
            var maybeDifference = result.Take(difference.Length).ToArray();
            var maybeIntersection = result.Skip(difference.Length).ToArray();

            Assert.Multiple(() =>
            {
                Assert.AreElementsEqualIgnoringOrder(difference, maybeDifference);
                Assert.AreElementsEqualIgnoringOrder(intersection, maybeIntersection);
            });
        }

        [Test]
        public void ItemsInBothSetsAppearAfterThoseAppearingInOnlyOne()
        {
            var itemSets = new[] {
                Sorted(2, 4, 6, 7, 9, 11, 12),
                Sorted(1, 3, 4, 5, 7, 8, 10, 11)
            };
            var merged = new SortedEnumerableMerger<int>(itemSets, new SetDifferenceAndIntersectionMerge<int>(accumulatorSelector));

            AssertGrouped(merged.ToArray(), new[] {1, 2, 3, 5, 6, 8, 9, 10, 12}, new[] {4, 7, 11});
        }

        private IEnumerable<Guid> GenerateGuids(int count)
        {
            while(count-- > 0)
            {
                yield return Guid.NewGuid();
            }
        }



        [Test]
        public void Fuzz_GuidSets_LargeCollectionProducesSameResultsAsEnumerables()
        {
            var shared = GenerateGuids(100).ToArray();
            var setA = shared.Concat(GenerateGuids(100)).ToArray();
            var setB = shared.Concat(GenerateGuids(100)).ToArray();

            using (var largeCollection = LargeCollectionDifferenceAndIntersection(setA, setB))
            {
                var difference = SetDifferenceMergeTests.EnumerableDifference(setA, setB).ToArray();
                var intersection = SetIntersectionMergeTests.EnumerableIntersection(setA, setB).ToArray();

                AssertGrouped(largeCollection.ToArray(), difference, intersection);
            }
        }

        private static IAccumulatorSelector accumulatorSelector = new SizeBasedAccumulatorSelector(0);

        private static IDisposableEnumerable<Guid> LargeCollectionDifferenceAndIntersection(IEnumerable<Guid> setA, IEnumerable<Guid> setB)
        {
            var operations = new LargeCollectionOperations(accumulatorSelector);

            return operations.DifferenceWithIntersection(
                    operations.BufferOnce(setA.GetEnumerator()),
                    operations.BufferOnce(setB.GetEnumerator()))
                .BufferInMemory();
        }
    }
}