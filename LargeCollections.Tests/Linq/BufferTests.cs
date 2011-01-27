using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LargeCollections.Linq;
using MbUnit.Framework;

namespace LargeCollections.Tests.Linq
{
    [TestFixture, CheckResources]
    public class BufferTests
    {
        [Test]
        public void BufferingInMemoryRetainsSortOrderMetaInformation()
        {
            var set = new[] {1, 2, 3, 4}.UsesSortOrder(Comparer<int>.Default);

            using(var buffered = set.GetEnumerator().BufferInMemory())
            {
                Assert.IsNotNull(buffered.GetUnderlying<ISorted<int>>());
                Assert.AreEqual(Comparer<int>.Default, buffered.GetUnderlying<ISorted<int>>().SortOrder);
            }
        }

        [Test]
        public void BufferingRetainsSortOrderMetaInformation()
        {
            var set = new[] { 1, 2, 3, 4 }.UsesSortOrder(Comparer<int>.Default);
            var operations = new LargeCollectionOperations(new DummyAccumulatorSelector());

            using (var buffered = operations.Buffer(set.GetEnumerator()))
            {
                Assert.IsNotNull(buffered.GetUnderlying<ISorted<int>>());
                Assert.AreEqual(Comparer<int>.Default, buffered.GetUnderlying<ISorted<int>>().SortOrder);
            }
        }


        [Test]
        public void BufferOnceIfDifferent_ReturnsOriginalEnumerator_If_OperationReturnsOriginalEnumerator()
        {
            var set = new[] { 1, 2, 3, 4 }.Cast<int>();
            var operations = new LargeCollectionOperations(new DummyAccumulatorSelector());

            var enumerator = set.GetEnumerator();

            using (var notBuffered = operations.BufferOnceIfDifferent(enumerator, e => e))
            {
                Assert.AreSame(enumerator, notBuffered);
            }
        }

        [Test]
        public void BufferOnceIfDifferent_ReturnsNewEnumerator_If_OperationDoesNotReturnOriginalEnumerator()
        {
            var set = new[] { 1, 2, 3, 4 }.Cast<int>();
            var operations = new LargeCollectionOperations(new DummyAccumulatorSelector());

            var enumerator = set.GetEnumerator();

            var operationResult = new[] { 5, 6, 7, 8 }.UsesSortOrder(Comparer<int>.Default).GetEnumerator();

            using (var buffered = operations.BufferOnceIfDifferent(enumerator, e => operationResult))
            {
                Assert.AreNotSame(enumerator, buffered);
                Assert.AreNotSame(operationResult, buffered);
            }
        }
    }
}
