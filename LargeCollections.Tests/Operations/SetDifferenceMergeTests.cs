using System;
using System.Collections.Generic;
using System.Linq;
using LargeCollections.Linq;
using LargeCollections.Operations;
using NUnit.Framework;

namespace LargeCollections.Tests.Operations
{
    [TestFixture, CheckResources]
    public class SetDifferenceMergeTests
    {
        private IEnumerable<int> Sorted(params int[] items)
        {
            return items.UsesSortOrder(Comparer<int>.Default);
        }

        [Test]
        public void SingleSortedEnumerableIsInvariant()
        {
            var items = Sorted(1, 2, 3, 4, 5);
            var merged = new SortedEnumerableMerger<int>(new[] { items }, new SetDifferenceMerge<int>());

            CollectionAssert.AreEqual(items, merged);
        }

        [Test]
        public void MultipleSortedEnumerablesDoNotYieldElementsOccurringInMultipleEnumerables()
        {
            var itemSets = new[] {
                Sorted(2, 4, 6, 7, 9, 12),
                Sorted(1, 3, 4, 7),
                Sorted(5, 8, 10, 11)
            };
            var merged = new SortedEnumerableMerger<int>(itemSets, new SetDifferenceMerge<int>());

            CollectionAssert.AreEqual(new []{ 1, 2, 3, 5, 6, 8, 9, 10, 11, 12 }, merged);
        }

        [Test]
        public void MultipleSortedEnumerablesDoYieldElementsOccurringMultipleTimesInAnEnumerable()
        {
            var itemSets = new[] {
                Sorted(2, 4, 6, 7, 7, 9, 12),
                Sorted(1, 3, 3, 4, 7),
                Sorted(5, 8, 10, 11)
            };
            var merged = new SortedEnumerableMerger<int>(itemSets, new SetDifferenceMerge<int>());

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 5, 6, 8, 9, 10, 11, 12 }, merged);
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

            using (var largeCollection = LargeCollectionDifference(setA, setB))
            {
                var enumerable = EnumerableDifference(setA, setB).ToArray();

                CollectionAssert.AreEquivalent(enumerable, largeCollection.ToArray());
            }
        }

        private static IAccumulatorSelector accumulatorSelector = new SizeBasedAccumulatorSelector(0);

        private static IDisposableEnumerable<Guid> LargeCollectionDifference(IEnumerable<Guid> setA, IEnumerable<Guid> setB)
        {
            var operations = new LargeCollectionOperations(accumulatorSelector);

            return operations.Difference(
                    operations.BufferOnce(setA.GetEnumerator()),
                    operations.BufferOnce(setB.GetEnumerator()))
                .BufferInMemory();
        }


        public static IEnumerable<Guid> EnumerableDifference(IEnumerable<Guid> index, IEnumerable<Guid> source)
        {
            var inIndex = new HashSet<Guid>(index);
            var inIndexAndSource = new HashSet<Guid>(); // intersection
            var onlyInSource = new List<Guid>();


            foreach (var id in source)
            {
                if (inIndex.Contains(id))
                {
                    inIndexAndSource.Add(id);
                }
                else
                {
                    onlyInSource.Add(id);
                }
            }

            // grab the ids that exist only in the source
            var outputSet = onlyInSource;
            // add those that are only in the index
            foreach (var id in inIndex)
            {
                if (!inIndexAndSource.Contains(id))
                {
                    outputSet.Add(id);
                }
            }
            return outputSet;
        }
    }
}