using System;
using System.Collections.Generic;
using MbUnit.Framework;
using Moq;

namespace LargeCollections.Tests
{
    [TestFixture]
    public class SinglePassCollectionTests
    {
        [Test]
        public void AcquiresReferenceCountedUnderlyingResourceWhenConstructed()
        {
            var underlying = MockUnderlyingCollection();
            var resource = MockResource();
            underlying.As<IHasBackingStore<IReferenceCountedResource>>().SetupGet(b => b.BackingStore).Returns(resource);

            new SinglePassCollection<int>(underlying.Object);
            Assert.AreEqual(1, resource.RefCount);
        }

        [Test]
        public void DisposesReferenceCountedUnderlyingResourceWhenDisposed()
        {
            var underlying = MockUnderlyingCollection();
            var resource = MockResource();

            underlying.As<IHasBackingStore<IReferenceCountedResource>>().SetupGet(b => b.BackingStore).Returns(resource);
            using(new SinglePassCollection<int>(underlying.Object))
            {
            }
            Assert.AreEqual(0, resource.RefCount);
        }

        [Test]
        public void DisposesReferenceCountedUnderlyingResourceOnlyOnce()
        {
            var underlying = MockUnderlyingCollection();
            var resource = MockResource();
            resource.Acquire();

            underlying.As<IHasBackingStore<IReferenceCountedResource>>().SetupGet(b => b.BackingStore).Returns(resource);
            using (var collection = new SinglePassCollection<int>(underlying.Object))
            {
                collection.Dispose();
                collection.Dispose();
                collection.Dispose();
            }
            Assert.AreEqual(1, resource.RefCount);
        }


        [Test]
        public void DisposesReferenceCountedUnderlyingResourceWhenIterationCompletes()
        {
            var underlying = MockUnderlyingCollection();
            var resource = MockResource();

            underlying.As<IHasBackingStore<IReferenceCountedResource>>().SetupGet(b => b.BackingStore).Returns(resource);
            using (var collection = new SinglePassCollection<int>(underlying.Object))
            {
                while (collection.MoveNext()) ;
                Assert.AreEqual(0, resource.RefCount);
            }
            
        }


        private ReferenceCountedResource MockResource()
        {
            return new Mock<ReferenceCountedResource>().Object;
        }
        
        [Test]
        public void DoesNotDisposeUnderlyingCollection()
        {
            var underlying = MockUnderlyingCollection();
            var collection = new SinglePassCollection<int>(underlying.Object);

            collection.Dispose();

            underlying.Verify(u => u.Dispose(), Times.Never());
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void DoesNotSupportReset()
        {
            var underlying = MockUnderlyingCollection();
            using (var collection = new SinglePassCollection<int>(underlying.Object))
            {
                collection.Reset();
            }
        }


        [Test]
        public void CanGetCount()
        {
            var underlying = MockUnderlyingCollection();
            using (var collection = new SinglePassCollection<int>(underlying.Object))
            {
                Assert.AreEqual(underlying.Object.Count, collection.Count);
            }
        }

        [Test]
        public void CanIterate()
        {
            var underlying = MockUnderlyingCollection();
            
            using (var collection = new SinglePassCollection<int>(underlying.Object))
            {
                Assert.IsTrue(collection.MoveNext());
                Assert.IsTrue(collection.MoveNext());
                Assert.IsFalse(collection.MoveNext());
            }
        }

        [Test]
        public void MultipleInstancesWithTheSameUnderlyingCollection_ReleaseBackingStoreCorrectly()
        {
            var underlying = MockUnderlyingCollection();
            var resource = MockResource();
            underlying.As<IHasBackingStore<IReferenceCountedResource>>().SetupGet(b => b.BackingStore).Returns(resource);

            using (var collection1 = new SinglePassCollection<int>(underlying.Object))
            {
                using (var collection2 = new SinglePassCollection<int>(underlying.Object))
                {
                    Assert.AreEqual(2, resource.RefCount);
                }
                Assert.AreEqual(1, resource.RefCount);
                while (collection1.MoveNext()) ;
                Assert.AreEqual(0, resource.RefCount);
            }
        }

        [Test]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void CannotAcquireReleasedBackingStore()
        {
            var underlying = MockUnderlyingCollection();
            var resource = MockResource();
            underlying.As<IHasBackingStore<IReferenceCountedResource>>().SetupGet(b => b.BackingStore).Returns(resource);

            // take and release resource.
            using (new SinglePassCollection<int>(underlying.Object))
            {
            }

            // refcount has gone positive and back to zero, so this fails:
            using (new SinglePassCollection<int>(underlying.Object))
            {
            }
        }

        private Mock<ILargeCollection<int>> MockUnderlyingCollection()
        {
            var underlying = new Mock<ILargeCollection<int>>();
            underlying.Setup(u => u.GetEnumerator()).Returns(MockEnumerator);
            underlying.SetupGet(u => u.Count).Returns(2);
            underlying.Setup(u => u.Dispose());
            return underlying;
        }

        private static IEnumerator<int> MockEnumerator()
        {
            yield return 1;
            yield return 2;
        }
    }
}
