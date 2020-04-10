using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bluewire.ReferenceCounting.Tests;
using LargeCollections.Core;
using LargeCollections.Core.Collections;
using LargeCollections.Tests;
using Moq;
using NUnit.Framework;

namespace LargeCollections.Operations.Tests
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
            Assert.AreEqual((int) original.Count, sorted.Count());
            CollectionAssert.AreEquivalent(original, sorted);
            CollectionAssert.IsOrdered(sorted, Comparer<int>.Default);
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
        public void ResultingEmptyEnumeratorHasSortOrderMetaInformation()
        {
            var sorter = GetSorter(5, MockSelector().Object);
            using (var collection = InMemoryAccumulator<int>.From(new int[0]))
            {
                using (var sorted = sorter.Sort(collection.GetEnumerator(), Comparer<int>.Default))
                {
                    Assert.IsNotNull(sorted.GetUnderlying<ISorted<int>>());
                    Assert.AreEqual(Comparer<int>.Default, sorted.GetUnderlying<ISorted<int>>().SortOrder);
                }
            }
        }


        [Test]
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
                Assert.Catch<IOException>(() => {
                    using (sorter.Sort(collection.GetEnumerator(), Comparer<int>.Default))
                    {
                    }
                });

                Utils.AssertReferencesDisposed();
            }
        }

        [Test]
        public void ResourcesAreCleanedUpCorrectly_If_ExceptionOccursDuringSourceDisposal()
        {
            var sorter = GetSorter(5, MockSelector().Object);

            using (var collection = MultipleBatches())
            {
                Assert.Catch<IOException>(() => {
                    using (var result = sorter.Sort(new EnumeratorThrowsWhenDisposing<int>(collection.GetEnumerator()), Comparer<int>.Default))
                    {
                        result.MoveNext();
                    }
                });
                
                Utils.AssertReferencesDisposed();
            }
        }

        private class EnumeratorDecoratorBase<T> : IEnumerator<T>
        {
            private readonly IEnumerator<T> enumerator;

            public EnumeratorDecoratorBase(IEnumerator<T> enumerator)
            {
                this.enumerator = enumerator;
            }

            public virtual void Dispose()
            {
                this.enumerator.Dispose();
            }

            public virtual bool MoveNext()
            {
                return this.enumerator.MoveNext();
            }

            public virtual void Reset()
            {
                this.enumerator.Reset();
            }

            public virtual T Current
            {
                get { return this.enumerator.Current; }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }


        class EnumeratorThrowsWhenDisposing<T> : EnumeratorDecoratorBase<T>
        {
            public EnumeratorThrowsWhenDisposing(IEnumerator<T> enumerator) : base(enumerator)
            {
            }

            public override void Dispose()
            {
                throw new IOException();
            }
        }

        class EnumeratorDecorator : EnumeratorDecoratorBase<int>, IHasUnderlying
        {
            private readonly IEnumerator<int> enumerator;
            
            public EnumeratorDecorator(IEnumerator<int> enumerator) : base(enumerator)
            {
                this.enumerator = enumerator;
            }


            public object Underlying { get { return enumerator; } }
        }
    }
}
