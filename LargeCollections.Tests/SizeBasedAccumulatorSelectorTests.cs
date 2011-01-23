using MbUnit.Framework;

namespace LargeCollections.Tests
{
    [TestFixture]
    public class SizeBasedAccumulatorSelectorTests
    {
        private static ILargeCollection<int> GetCollection(long threshold, int[] values)
        {
            var selector = new SizeBasedAccumulatorSelector(threshold);
            using (var accumulator = selector.GetAccumulator<int>())
            {
                accumulator.AddRange(values);
                return accumulator.Complete();
            }
        }

        [Test]
        public void CollectionUsesBackingStore_If_CountIsLargerThanThreshold()
        {
            using (var collection = GetCollection(2, new [] { 1, 2, 3 }))
            {
                Assert.IsNotNull(collection.GetBackingStore<object>());
            }
        }


        [Test]
        public void CollectionDoesNotUseBackingStore_If_CountIsNotLargerThanThreshold()
        {
            using (var collection = GetCollection(3, new[] { 1, 2, 3 }))
            {
                Assert.IsNull(collection.GetBackingStore<object>());
            }
        }
    }
}
