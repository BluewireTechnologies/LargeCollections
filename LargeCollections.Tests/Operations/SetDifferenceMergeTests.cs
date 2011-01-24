using System;
using System.Collections.Generic;
using System.Linq;
using LargeCollections.Collections;
using LargeCollections.Operations;
using MbUnit.Framework;

namespace LargeCollections.Tests.Operations
{
    [TestFixture]
    public class SetDifferenceMergeTests
    {
        [Test]
        public void SingleSortedEnumerableIsInvariant()
        {
            var items = new[] { 1, 2, 3, 4, 5 };
            var merged = new SortedEnumerableMerger<int>(new[] { items }, Comparer<int>.Default, new SetDifferenceMerge<int>());

            Assert.AreElementsEqual(items, merged);
        }

        [Test]
        public void MultipleSortedEnumerablesDoNotYieldElementsOccurringInMultipleEnumerables()
        {
            var itemSets = new[] {
                new [] {2, 4, 6, 7, 9, 12 },
                new [] {1, 3, 4, 7 },
                new [] {5, 8, 10, 11 },
            };
            var merged = new SortedEnumerableMerger<int>(itemSets, Comparer<int>.Default, new SetDifferenceMerge<int>());

            Assert.AreElementsEqual(new []{ 1, 2, 3, 5, 6, 8, 9, 10, 11, 12 }, merged);
        }
        
        [Test]
        public void MultipleSortedEnumerablesDoYieldElementsOccurringMultipleTimesInAnEnumerable()
        {
            var itemSets = new[] {
                new [] {2, 4, 6, 7, 7, 9, 12 },
                new [] {1, 3, 3, 4, 7 },
                new [] {5, 8, 10, 11 },
            };
            var merged = new SortedEnumerableMerger<int>(itemSets, Comparer<int>.Default, new SetDifferenceMerge<int>());

            Assert.AreElementsEqual(new[] { 1, 2, 3, 5, 6, 8, 9, 10, 11, 12 }, merged);
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
            var setA = GenerateGuids(100).ToArray();
            var setB = GenerateGuids(100).ToArray();

            var largeCollection = LargeCollectionDifference(setA, setB).ToArray();
            var enumerable = EnumerableDifference(setA, setB).ToArray();

            Assert.AreElementsEqualIgnoringOrder(enumerable, largeCollection);
        }

        private static IAccumulatorSelector accumulatorSelector = new SizeBasedAccumulatorSelector(0);

        private static ILargeCollection<Guid> LargeCollectionDifference(IEnumerable<Guid> setA, IEnumerable<Guid> setB)
        {
            var largeSetA = Load(setA);
            var largeSetB = Load(setB);

            var sorter = new LargeCollectionSorter(accumulatorSelector);
            using (var sorted = sorter.Sort(largeSetA))
            {
                largeSetA = new SinglePassCollection<Guid>(sorted);
            }

            using (var sorted = sorter.Sort(largeSetB))
            {
                largeSetB = new SinglePassCollection<Guid>(sorted);
            }

            using (var accumulator = accumulatorSelector.GetAccumulator<Guid>(Math.Max(largeSetA.Count, largeSetB.Count)))
            {
                using (var difference = new SortedEnumeratorMerger<Guid>(new List<IEnumerator<Guid>> { largeSetA, largeSetB }, Comparer<Guid>.Default, new SetDifferenceMerge<Guid>()))
                {
                    while (difference.MoveNext())
                    {
                        accumulator.Add(difference.Current);
                    }
                }
                return accumulator.Complete();
            }
        }

        private static ISinglePassCollection<Guid> Load(IEnumerable<Guid> set)
        {
            using (var accumulator = accumulatorSelector.GetAccumulator<Guid>())
            {
                accumulator.AddRange(set);
                using (var collection = accumulator.Complete())
                {
                    return new SinglePassCollection<Guid>(collection);
                }
            }
        }


        private static IEnumerable<Guid> EnumerableDifference(IEnumerable<Guid> index, IEnumerable<Guid> source)
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