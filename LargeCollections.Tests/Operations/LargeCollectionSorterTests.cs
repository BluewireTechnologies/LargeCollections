using System.Collections;
using System.Collections.Generic;
using LargeCollections.Collections;
using LargeCollections.Linq;
using LargeCollections.Operations;
using MbUnit.Framework;
using Moq;

namespace LargeCollections.Tests.Operations
{
    [TestFixture]
    public class LargeCollectionSorterTests
    {
        private Mock<IAccumulatorSelector> MockSelector()
        {
            var selector = new Mock<IAccumulatorSelector>();
            selector.Setup(s => s.GetAccumulator<int>()).Returns(() => new InMemoryAccumulator<int>());
            selector.Setup(s => s.GetAccumulator<int>(It.IsAny<IEnumerator>())).Returns(() => new InMemoryAccumulator<int>());
            selector.Setup(s => s.GetAccumulator<int>(It.IsAny<long>())).Returns(() => new InMemoryAccumulator<int>());
            return selector;
        }


        private LargeCollectionSorter GetSorter(int batchSize, IAccumulatorSelector selector)
        {
            return new LargeCollectionSorter(selector, batchSize);
        }

        [Test]
        public void CanSortEmptyCollection()
        {
            var sorter = GetSorter(10, MockSelector().Object);
            using (var collection = InMemoryAccumulator<int>.From(new int[0]))
            {
                using (var singlePassCollection = new SinglePassCollection<int>(collection))
                {
                    var sorted = sorter.Sort(singlePassCollection, Comparer<int>.Default);
                    Assert.IsEmpty(sorted.Buffer());
                }
            }
        }

        [Test]
        public void CanSortSingleBatchCollection()
        {
            var sorter = GetSorter(10, MockSelector().Object);
            using(var collection = InMemoryAccumulator<int>.From(new[] {5, 2, 3, 1, 4}))
            {
                using(var singlePassCollection = new SinglePassCollection<int>(collection))
                {
                    var sorted = sorter.Sort(singlePassCollection, Comparer<int>.Default);
                    AssertSorted(collection, sorted.Buffer());
                }
            }
        }

        private static void AssertSorted(ILargeCollection<int> original, ILargeCollection<int> sorted)
        {
            Assert.Count((int)original.Count, sorted);
            Assert.AreElementsEqualIgnoringOrder(original, sorted);
            Assert.Sorted(sorted, SortOrder.Increasing);
        }

        [Test]
        public void CanSortMultipleBatchCollection()
        {
            var sorter = GetSorter(5, MockSelector().Object);
            using (var collection = InMemoryAccumulator<int>.From(new[] { 7, 5, 2, 12, 9, 3, 8, 10, 1, 6, 4, 11 }))
            {
                using (var singlePassCollection = new SinglePassCollection<int>(collection))
                {
                    var sorted = sorter.Sort(singlePassCollection, Comparer<int>.Default);
                    AssertSorted(collection, sorted.Buffer());
                }
            }
        }

        /// <summary>
        /// With a threshold of 10 000, a 1 000 000-item set would get a backing store, but each batch would be
        /// only 1000 items in size. If batch storage is selected by batch size, we'd get 1000 batches of 1000 items
        /// stored in memory.
        /// </summary>
        [Test]
        public void BatchAccumulatorsAreSelectedByTotalSize()
        {
            var selector = MockSelector();
            var batchSize = 5;
            var sorter = GetSorter(batchSize, selector.Object);
            using (var collection = InMemoryAccumulator<int>.From(new[] { 7, 5, 2, 12, 9, 3, 8, 10, 1, 6, 4, 11 }))
            {
                using (var singlePassCollection = new SinglePassCollection<int>(collection))
                {
                    sorter.Sort(singlePassCollection, Comparer<int>.Default);
                }
            }
            selector.Verify(s => s.GetAccumulator<int>(batchSize), Times.Never());
        }

        [TearDown]
        public void TearDown()
        {
            Utils.AssertReferencesDisposed();
        }
    }
}
