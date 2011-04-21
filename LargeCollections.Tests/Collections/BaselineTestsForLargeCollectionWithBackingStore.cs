using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using LargeCollections.Resources;
using MbUnit.Framework;

namespace LargeCollections.Tests.Collections
{
    public class BaselineTestsForLargeCollectionWithBackingStore<TBackingStore>
    {
        public IEnumerable<Test> GetTests(Func<LargeCollectionTestHarness<TBackingStore>> harness)
        {
            return new Func<Func<LargeCollectionTestHarness<TBackingStore>>, Test>[] {
                AccumulatorBecomesReadOnly_When_CollectionIsCreated,
                CanUseCollection_Before_AccumulatorIsDisposed,
                CannotCompleteAccumulatorTwice,
                CanSafelyDisposeMultipleTimes,
                AccumulatorCleansUpBackingStore_If_NoCollectionIsCreated,
                CleansUpBackingStore_WhenDisposed,
                DoesNotCleanUpBackingStore_WhenIterationIsComplete
            }.Select(t => t(harness));
        }

        private Test AccumulatorBecomesReadOnly_When_CollectionIsCreated(Func<LargeCollectionTestHarness<TBackingStore>> getHarness)
        {
            return new TestCase("AccumulatorBecomesReadOnly_When_CollectionIsCreated", () =>
                Assert.Throws<ReadOnlyException>(() =>
                {
                    using (var harness = getHarness())
                    using (var accumulator = harness.GetAccumulator())
                    {
                        accumulator.Add(1);
                        using (accumulator.Complete())
                        {
                            accumulator.Add(2);
                        }
                    }
                }));
        }

        private Test CanUseCollection_Before_AccumulatorIsDisposed(Func<LargeCollectionTestHarness<TBackingStore>> getHarness)
        {
            return new TestCase("CanUseCollection_Before_AccumulatorIsDisposed", () =>
            {
                using (var harness = getHarness())
                using (var accumulator = harness.GetAccumulator())
                {
                    accumulator.Add(1);
                    accumulator.Add(2);
                    using (var collection = accumulator.Complete())
                    {
                        collection.ToArray();
                    }
                }
            });
        }

        private Test CannotCompleteAccumulatorTwice(Func<LargeCollectionTestHarness<TBackingStore>> getHarness)
        {
            return new TestCase("CannotCompleteAccumulatorTwice", () =>
                Assert.Throws<InvalidOperationException>(() =>
                {
                    using (var harness = getHarness())
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
                }));
        }

        private Test CanSafelyDisposeMultipleTimes(Func<LargeCollectionTestHarness<TBackingStore>> getHarness)
        {
            return new TestCase("CanSafelyDisposeMultipleTimes", () =>
            {
                using (var harness = getHarness())
                using (var collection = harness.GetCollection(new[] {1, 2, 3}))
                {
                    collection.Dispose();
                }
            });
        }

        private Test AccumulatorCleansUpBackingStore_If_NoCollectionIsCreated(Func<LargeCollectionTestHarness<TBackingStore>> getHarness)
        {
            return new TestCase("AccumulatorCleansUpBackingStore_If_NoCollectionIsCreated", () =>
            {
                using (var harness = getHarness())
                {
                    var accumulator = harness.GetAccumulator();
                    using (accumulator)
                    {
                        accumulator.Add(1);
                    }
                    Assert.IsFalse(harness.BackingStoreExists(accumulator));
                }
            });
        }

        private Test CleansUpBackingStore_WhenDisposed(Func<LargeCollectionTestHarness<TBackingStore>> getHarness)
        {
            return new TestCase("CleansUpBackingStore_WhenDisposed", () =>
            {
                using (var harness = getHarness())
                using (var collection = harness.GetCollection(new[] {1, 2, 3}))
                {
                    Assert.IsTrue(harness.BackingStoreExists(collection));
                    collection.Dispose();
                    Assert.IsFalse(harness.BackingStoreExists(collection));
                }
            });
        }

        private Test DoesNotCleanUpBackingStore_WhenIterationIsComplete(Func<LargeCollectionTestHarness<TBackingStore>> getHarness)
        {
            return new TestCase("DoesNotCleanUpBackingStore_WhenIterationIsComplete", () =>
            {
                using (var harness = getHarness())
                using (var collection = harness.GetCollection(new[] {1, 2, 3}))
                {
                    Assert.IsTrue(harness.BackingStoreExists(collection));
                    collection.ToArray();
                    Assert.IsTrue(harness.BackingStoreExists(collection));
                }
            });
        }

    }
}