using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LargeCollections.Collections;
using LargeCollections.FileSystem;
using LargeCollections.Linq;
using LargeCollections.Operations;
using NUnit.Framework;

namespace LargeCollections.Tests.Operations
{
    [TestFixture, CheckResources]
    public class SetIntersectionMergeTests
    {
        private IEnumerable<int> Sorted(params int[] items)
        {
            return items.UsesSortOrder(Comparer<int>.Default);
        }

        private ILargeCollection<int> Unsorted(params int[] items)
        {
            return InMemoryAccumulator<int>.From(items);
        }

        [Test]
        public void SingleSortedEnumerableIsInvariant()
        {
            var items = Sorted(1, 2, 3, 4, 5);
            var merged = new SortedEnumerableMerger<int>(new[] { items }, new SetIntersectionMerge<int>());

            CollectionAssert.AreEqual(items, merged);
        }

        [Test]
        public void MultipleSortedEnumerablesOnlyYieldElementsOccurringInAllEnumerables()
        {
            var itemSets = new[] {
                Sorted(1, 2, 4, 5, 8, 9),
                Sorted(1, 3, 4, 7, 9, 10),
                Sorted(2, 4, 5, 6, 9)
            };
            var merged = new SortedEnumerableMerger<int>(itemSets, new SetIntersectionMerge<int>());

            CollectionAssert.AreEqual(new[] { 4, 9 }, merged);
        }

        [Test]
        public void MultipleSortedEnumerablesDoNotYieldElementsOccurringMultipleTimesInASingleEnumerable()
        {
            var itemSets = new[] {
                Sorted(1, 2, 4, 4, 4, 5, 8, 9),
                Sorted(1, 3, 7, 9, 10),
                Sorted(2, 5, 6, 9)
            };
            var merged = new SortedEnumerableMerger<int>(itemSets, new SetIntersectionMerge<int>());

            CollectionAssert.AreEqual(new[] { 9 }, merged);
        }

        private IEnumerable<Guid> GenerateGuids(int count)
        {
            while (count-- > 0)
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

            using (var largeCollection = LargeCollectionIntersection(setA, setB))
            {
                var enumerable = EnumerableIntersection(setA, setB).ToArray();
                var lc = largeCollection.ToArray();

                CollectionAssert.AreEquivalent(enumerable, lc);
                Assert.AreEqual(enumerable.Length, lc.Length);
            }
        }

        private static readonly IAccumulatorSelector accumulatorSelector = new SizeBasedAccumulatorSelector(0);

        private static IDisposableEnumerable<Guid> LargeCollectionIntersection(IEnumerable<Guid> setA, IEnumerable<Guid> setB)
        {
            var operations = new LargeCollectionOperations(accumulatorSelector);

            return operations.Intersection(
                    operations.BufferOnce(setA.GetEnumerator()),
                    operations.BufferOnce(setB.GetEnumerator()))
                .BufferInMemory();
        }


        public static IEnumerable<Guid> EnumerableIntersection(IEnumerable<Guid> index, IEnumerable<Guid> source)
        {
            var inIndex = new HashSet<Guid>(index);
            var inIndexAndSource = new HashSet<Guid>(); // intersection


            foreach (var id in source)
            {
                if (inIndex.Contains(id))
                {
                    inIndexAndSource.Add(id);
                }
            }

            return inIndexAndSource;
        }
    }
}
