﻿using System;
using System.Collections.Generic;
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

        [Test]
        public void UncountedSourceEnumerator_AsCounted_ReturnsNewBufferedEnumerator()
        {
            var set = new[] { 1, 2, 3, 4 }.Cast<int>();
            var operations = new LargeCollectionOperations(new DummyAccumulatorSelector());

            // .NET enumerators are not ICounted.
            var enumerator = set.GetEnumerator();

            using (var buffered = operations.AsCounted(enumerator))
            {
                Assert.AreNotSame(enumerator, buffered);
            }
        }

        [Test]
        public void CountedSourceEnumerator_AsCounted_ReturnsSameEnumerator()
        {
            var operations = new LargeCollectionOperations(new DummyAccumulatorSelector());

            using (var set = InMemoryAccumulator<int>.From(new[] { 1, 2, 3, 4 }))
            {
                var enumerator = set.GetEnumerator();

                using (var buffered = operations.AsCounted(enumerator))
                {
                    Assert.AreSame(enumerator, buffered);
                }
            }
        }



        [Test]
        public void Buffer_CleansUpResources_If_EnumeratorThrowsException()
        {
            var enumerator = new Mock<IEnumerator<int>>();
            enumerator.Setup(e => e.MoveNext()).Throws(new InvalidOperationException());

            Assert.Catch<InvalidOperationException>(() => enumerator.Object.BufferInMemory());

            enumerator.Verify(e => e.Dispose());
        }

        [Test]
        public void Buffer_CleansUpResources_If_EnumeratorDisposalThrowsException()
        {
            var enumerator = new Mock<IEnumerator<int>>();
            enumerator.Setup(e => e.Dispose()).Throws(new InvalidOperationException());

            Assert.Catch<InvalidOperationException>(() => enumerator.Object.BufferInMemory());

            enumerator.Verify(e => e.Dispose());
        }
    }
}
