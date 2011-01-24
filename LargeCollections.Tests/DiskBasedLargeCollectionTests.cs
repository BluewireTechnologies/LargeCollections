using System.Collections.Generic;
using System.IO;
using System.Linq;
using LargeCollections.Collections;
using LargeCollections.Resources;
using LargeCollections.Storage;
using MbUnit.Framework;

namespace LargeCollections.Tests
{
    [TestFixture]
    public class DiskBasedLargeCollectionTests
    {
        private static ILargeCollection<int> GetCollection(IEnumerable<int> values)
        {
            using(var accumulator = new FileAccumulator<int>(Path.GetTempFileName(), new DefaultItemSerialiser<int>()))
            {
                accumulator.AddRange(values);
                return accumulator.Complete();
            }
        }

        [Test]
        public void AccumulatorCleansUpBackingStore_If_NoCollectionIsCreated()
        {
            var fileName = Path.GetTempFileName();
            using (var accumulator = new FileAccumulator<int>(fileName, new DefaultItemSerialiser<int>()))
            {
                accumulator.Add(1);
            }
            Assert.IsFalse(File.Exists(fileName));
        }

        [Test]
        public void CanSafelyDisposeMultipleTimes()
        {
            using (var collection = GetCollection(new[] { 1, 2, 3 }))
            {
                collection.Dispose();
            }
        }


        [Test]
        public void CleansUpBackingStore_WhenDisposed()
        {
            using (var collection =  GetCollection(new[] { 1, 2, 3 }))
            {
                Assert.IsTrue(collection.GetBackingStore<FileReference>().File.Exists);
                collection.Dispose();
                Assert.IsFalse(collection.GetBackingStore<FileReference>().File.Exists);
            }
        }


        [Test]
        public void DoesNotCleanUpBackingStore_WhenIterationIsComplete()
        {
            using (var collection = GetCollection(new[] { 1, 2, 3 }))
            {
                Assert.IsTrue(collection.GetBackingStore<FileReference>().File.Exists);
                collection.ToArray();
                Assert.IsTrue(collection.GetBackingStore<FileReference>().File.Exists);
            }
        }

        [TearDown]
        public void TearDown()
        {
            Utils.AssertReferencesDisposed();
        }
    }
}
