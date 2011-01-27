using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using LargeCollections.Collections;
using LargeCollections.Storage;
using MbUnit.Framework;

namespace LargeCollections.Tests.Collections
{
    [TestFixture, CheckResources]
    public class InMemoryLargeCollectionTests
    {
        [Test]
        [ExpectedException(typeof(ReadOnlyException))]
        public void AccumulatorBecomesReadOnly_When_CollectionIsCreated()
        {
            using (var accumulator = new InMemoryAccumulator<int>())
            {
                accumulator.Add(1);
                using (accumulator.Complete())
                {
                    accumulator.Add(2);
                }
            }
        }

        [Test]
        public void CanUseCollection_Before_AccumulatorIsDisposed()
        {
            using (var accumulator = new InMemoryAccumulator<int>())
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
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotCompleteAccumulatorTwice()
        {
            using (var accumulator = new InMemoryAccumulator<int>())
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
        }


        [Test]
        public void DisposeDoesNotInvalidateBackingStore()
        {
            using(var collection = InMemoryAccumulator<int>.From(new[] {1, 2, 3, 4}))
            {
                var enumerator = collection.GetEnumerator();
                collection.Dispose();

                var list = new List<int>();
                while(enumerator.MoveNext()) list.Add(enumerator.Current);

                Assert.Count(4, list);


            }

        }
    }
}
