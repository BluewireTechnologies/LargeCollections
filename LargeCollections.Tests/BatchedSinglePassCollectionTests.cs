using System.Collections.Generic;
using LargeCollections.Collections;
using MbUnit.Framework;

namespace LargeCollections.Tests
{
    [TestFixture]
    public class BatchedSinglePassCollectionTests
    {
        private static ISinglePassCollection<int> GetCollection(params int[] items)
        {
            using(var collection = InMemoryAccumulator<int>.From(items))
            {
                return new SinglePassCollection<int>(collection);
            }
        }

        [Test]
        public void CollectionSmallerThanBatchSize_Becomes_OneBatch()
        {
            using(var batcher = new BatchedSinglePassCollection<int>(GetCollection(1, 2, 3, 4, 5), 10))
            {
                Assert.AreEqual(1, batcher.Count);
                Assert.IsTrue(batcher.MoveNext());
                Assert.AreElementsEqual(new []{ 1, 2, 3, 4, 5 }, batcher.Current);
                Assert.IsFalse(batcher.MoveNext());
            }
        }

        [Test]
        public void CollectionSameSizeAsBatch_Becomes_OneBatch()
        {
            using (var batcher = new BatchedSinglePassCollection<int>(GetCollection(1, 2, 3, 4, 5), 5))
            {
                Assert.AreEqual(1, batcher.Count);
                Assert.IsTrue(batcher.MoveNext());
                Assert.AreElementsEqual(new[] { 1, 2, 3, 4, 5 }, batcher.Current);
                Assert.IsFalse(batcher.MoveNext());
            }
        }

        [Test]
        public void CollectionLargerThanBatchSize_Becomes_MultipleBatches()
        {
            using (var batcher = new BatchedSinglePassCollection<int>(GetCollection(1, 2, 3, 4, 5), 2))
            {
                Assert.AreEqual(3, batcher.Count);
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
            Assert.AreElementsEqual(new []{ 1, 2, 3, 4, 5 }, batches);
        }

    }
}
