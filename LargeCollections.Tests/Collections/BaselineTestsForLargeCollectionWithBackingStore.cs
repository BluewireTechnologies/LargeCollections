using System;
using System.Data;
using System.Linq;
using NUnit.Framework;

namespace LargeCollections.Tests.Collections
{
    public abstract class BaselineTestsForLargeCollectionWithBackingStore<TBackingStore>
    {
        protected abstract LargeCollectionTestHarness<TBackingStore> CreateHarness();

        [Test]
        public void AccumulatorBecomesReadOnly_When_CollectionIsCreated()
        {
            Assert.Throws<ReadOnlyException>(() =>
            {
                using (var harness = CreateHarness())
                using (var accumulator = harness.GetAccumulator())
                {
                    accumulator.Add(1);
                    using (accumulator.Complete())
                    {
                        accumulator.Add(2);
                    }
                }
            });
        }

        [Test]
        public void CanUseCollection_Before_AccumulatorIsDisposed()
        {
            using (var harness = CreateHarness())
            using (var accumulator = harness.GetAccumulator())
            {
                accumulator.Add(1);
                accumulator.Add(2);
                using (var collection = accumulator.Complete())
                {
                    collection.ToArray();
                }
            }
        }

        [Test]
        public void CannotCompleteAccumulatorTwice()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                using (var harness = CreateHarness())
                using (var accumulator = harness.GetAccumulator())
                {
                    accumulator.Add(1);
                    accumulator.Add(2);
                    using (accumulator.Complete())
                    {
                    }
                    using (accumulator.Complete())
                    {
                    }
                }
            });
        }

        [Test]
        public void CanSafelyDisposeMultipleTimes()
        {
            using (var harness = CreateHarness())
            using (var collection = harness.GetCollection(new[] { 1, 2, 3 }))
            {
                collection.Dispose();
            }
        }

        [Test]
        public void AccumulatorCleansUpBackingStore_If_NoCollectionIsCreated()
        {
            using (var harness = CreateHarness())
            {
                var accumulator = harness.GetAccumulator();
                using (accumulator)
                {
                    accumulator.Add(1);
                }
                Assert.IsFalse(harness.BackingStoreExists(accumulator));
            }
        }

        [Test]
        public void CleansUpBackingStore_WhenDisposed ()
        {
            using (var harness = CreateHarness())
            using (var collection = harness.GetCollection(new[] { 1, 2, 3 }))
            {
                Assert.IsTrue(harness.BackingStoreExists(collection));
                collection.Dispose();
                Assert.IsFalse(harness.BackingStoreExists(collection));
            }
        }

        [Test]
        public void CanCleanUpEmptyCollection()
        {
            using (var harness = CreateHarness())
            using (harness.GetCollection(new int[] { }))
            {
            }
        }

        [Test]
        public void DoesNotCleanUpBackingStore_WhenIterationIsComplete()
        {
            using (var harness = CreateHarness())
            using (var collection = harness.GetCollection(new[] { 1, 2, 3 }))
            {
                Assert.IsTrue(harness.BackingStoreExists(collection));
                collection.ToArray();
                Assert.IsTrue(harness.BackingStoreExists(collection));
            }
        }

        [Test]
        public void CanReadCompletedCollection()
        {
            var elements = new[] { 1, 2, 3 };
            using (var harness = CreateHarness())
            using (var collection = harness.GetCollection(elements))
            {
                CollectionAssert.AreEquivalent(elements, collection);
            }
        }
    }
}
