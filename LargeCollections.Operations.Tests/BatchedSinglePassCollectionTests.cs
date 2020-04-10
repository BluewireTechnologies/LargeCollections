using System.Collections.Generic;
using Bluewire.ReferenceCounting;
using LargeCollections.Core.Collections;
using LargeCollections.Tests;
using NUnit.Framework;

namespace LargeCollections.Operations.Tests
{
    [TestFixture, CheckResources]
    public class BatchedSinglePassCollectionTests
    {
        private static IEnumerator<int> GetCollection(params int[] items)
        {
            return InMemoryAccumulator<int>.From(items).UseSafely(c => c.GetEnumerator());
        }

        [Test]
        public void CollectionSmallerThanBatchSize_Becomes_OneBatch()
        {
            using(var batcher = new BatchedSinglePassCollection<int>(GetCollection(1, 2, 3, 4, 5), 10))
            {
                Assert.IsTrue(batcher.MoveNext());
                CollectionAssert.AreEqual(new []{ 1, 2, 3, 4, 5 }, batcher.Current);
                Assert.IsFalse(batcher.MoveNext());
            }
        }

        [Test]
        public void CollectionSameSizeAsBatch_Becomes_OneBatch()
        {
            using (var batcher = new BatchedSinglePassCollection<int>(GetCollection(1, 2, 3, 4, 5), 5))
            {
                Assert.IsTrue(batcher.MoveNext());
                CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, batcher.Current);
                Assert.IsFalse(batcher.MoveNext());
            }
        }

        [Test]
        public void CollectionLargerThanBatchSize_Becomes_MultipleBatches()
        {
            using (var batcher = new BatchedSinglePassCollection<int>(GetCollection(1, 2, 3, 4, 5), 2))
            {
                Assert.IsTrue(batcher.MoveNext());
                Assert.IsTrue(batcher.MoveNext());
                Assert.IsTrue(batcher.MoveNext());
                Assert.IsFalse(batcher.MoveNext());
            }
        }

        [Test]
        public void BatchedCollectionRetainsAllItems()
        {
            var batches = new List<int>();
            using (var batcher = new BatchedSinglePassCollection<int>(GetCollection(1, 2, 3, 4, 5), 2))
            {
                while(batcher.MoveNext())
                {
                    batches.AddRange(batcher.Current);
                }
            }
            CollectionAssert.AreEqual(new []{ 1, 2, 3, 4, 5 }, batches);
        }
    }
}
