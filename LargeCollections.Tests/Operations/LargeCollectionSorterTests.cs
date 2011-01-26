﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using LargeCollections.Collections;
using LargeCollections.Linq;
using LargeCollections.Operations;
using MbUnit.Framework;
using Moq;

namespace LargeCollections.Tests.Operations
{
    [TestFixture, CheckResources]
    public class LargeCollectionSorterTests
    {
        private Mock<IAccumulatorSelector> MockSelector()
        {
            var selector = new Mock<IAccumulatorSelector>();
            selector.Setup(s => s.GetAccumulator<int>(It.IsAny<IEnumerator>())).Returns(() => new InMemoryAccumulator<int>());
            return selector;
        }

        private IDisposableEnumerable<int> MultipleBatches()
        {
            return InMemoryAccumulator<int>.From(new[] {7, 5, 2, 12, 9, 3, 8, 10, 1, 6, 4, 11});
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
                using (var sorted = sorter.Sort(collection.GetEnumerator(), Comparer<int>.Default).BufferInMemory())
                {
                    Assert.IsEmpty(sorted);
                }
            }
        }

        [Test]
        public void CanSortSingleBatchCollection()
        {
            var sorter = GetSorter(10, MockSelector().Object);
            using(var collection = InMemoryAccumulator<int>.From(new[] {5, 2, 3, 1, 4}))
            {
                using (var sorted = sorter.Sort(collection.GetEnumerator(), Comparer<int>.Default).BufferInMemory())
                {
                    AssertSorted(collection, sorted);
                }
            }
        }

        private static void AssertSorted(ILargeCollection<int> original, IDisposableEnumerable<int> sorted)
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
                using (var sorted = sorter.Sort(collection.GetEnumerator(), Comparer<int>.Default).BufferInMemory())
                {
                    AssertSorted(collection, sorted);
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
            using (var collection = MultipleBatches())
            {
                var enumerator = collection.GetEnumerator();
                sorter.Sort(enumerator, Comparer<int>.Default).Dispose();
                selector.Verify(s => s.GetAccumulator<int>(batchSize), Times.Never());
                selector.Verify(s => s.GetAccumulator<int>(enumerator));
            }
            
        }

        [Test]
        public void IfCollectionAlreadySortedCorrectly_Returns_SameObject()
        {
            var sorter = GetSorter(10, MockSelector().Object);
            using (var collection = InMemoryAccumulator<int>.From(new[] {1, 2, 3, 4, 5}))
            {
                var sortedInput = collection.UsesSortOrder(Comparer<int>.Default).GetEnumerator();
                using(var sortedOutput = sorter.Sort(sortedInput, Comparer<int>.Default))
                {
                    Assert.AreSame(sortedInput, sortedOutput);
                }
            }
        }

        [Test]
        public void IfUnderlyingCollectionAlreadySortedCorrectly_Returns_SameObject()
        {
            var sorter = GetSorter(10, MockSelector().Object);
            using (var collection = InMemoryAccumulator<int>.From(new[] { 1, 2, 3, 4, 5 }))
            {
                var sortedInput = new EnumeratorDecorator(collection.UsesSortOrder(Comparer<int>.Default).GetEnumerator());
                using (var sortedOutput = sorter.Sort(sortedInput, Comparer<int>.Default))
                {
                    Assert.AreSame(sortedInput, sortedOutput);
                }
            }
        }

        [Test]
        public void ResultingEnumeratorHasSortOrderMetaInformation()
        {
            var sorter = GetSorter(5, MockSelector().Object);
            using (var collection = MultipleBatches())
            {
                using (var sorted = sorter.Sort(collection.GetEnumerator(), Comparer<int>.Default))
                {
                    Assert.IsNotNull(sorted.GetUnderlying<ISorted<int>>());
                    Assert.AreEqual(Comparer<int>.Default, sorted.GetUnderlying<ISorted<int>>().SortOrder);
                }
            }
        }

        [Test]
        [ExpectedException(typeof(IOException))]
        public void ResourcesAreCleanedUpCorrectly_If_ExceptionOccursDuringBatchBuffering()
        {
            var batchCount = 0;
            var selector = new Mock<IAccumulatorSelector>();
            selector.Setup(s => s.GetAccumulator<int>(It.IsAny<IEnumerator>())).Returns(() =>
            {
                if (batchCount > 1) throw new IOException();
                batchCount++;
                return new InMemoryAccumulator<int>();
            });

            var sorter = GetSorter(5, selector.Object);

            using (var collection = MultipleBatches())
            {
                try
                {
                    using (sorter.Sort(collection.GetEnumerator(), Comparer<int>.Default))
                    {
                    }
                }
                finally
                {
                    Utils.AssertReferencesDisposed();
                }
            }
        }

        class EnumeratorDecorator : IEnumerator<int>, IHasUnderlying
        {
            private readonly IEnumerator<int> enumerator;
            public void Dispose()
            {
                this.enumerator.Dispose();
            }

            public bool MoveNext()
            {
                return this.enumerator.MoveNext();
            }

            public void Reset()
            {
                this.enumerator.Reset();
            }

            public int Current
            {
                get { return this.enumerator.Current; }
            }

            public EnumeratorDecorator(IEnumerator<int> enumerator)
            {
                this.enumerator = enumerator;
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public object Underlying { get { return enumerator; } }
        }
    }
}
