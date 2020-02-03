using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using LargeCollections.Resources;
using NUnit.Framework;

namespace LargeCollections.Tests.Resources
{
    [TestFixture, CheckResources]
    public class ReferenceCountedResourceTests
    {
        [Test]
        public void CanDetectLeakedReferencedResources()
        {
            var resource = CreateWeaklyHeldResource();

            Assert.AreEqual(1, ReferenceCountedResource.Diagnostics.CountLeaks());
            ReferenceCountedResource.Diagnostics.Reset();

            GC.KeepAlive(resource);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private WeakReference CreateWeaklyHeldResource()
        {
            var instance = new ReferenceCountedResource();
            instance.Acquire();
            return new WeakReference(instance, true);
        }

        [Test]
        public void ExceptionInDerivedConstructor_DoesNotCauseCleanup()
        {
            var exception = Assert.Catch<Exception>(() => new UnconstructableResource(Assert.Fail));
            Assert.That(exception.Message, Does.Contain("can't construct"));
        }

        [Test]
        public void ExceptionInDerivedConstructor_DoesNotCauseLeak()
        {
            try
            {
                Assert.Catch<Exception>(() => new UnconstructableResource(() => { }));
            }
            finally
            {
                Assert.AreEqual(0, ReferenceCountedResource.Diagnostics.CountLeaks());
            }
        }

        [Test]
        public void ReferenceCounterIsThreadSafe()
        {
            var cleanupCount = 0;
            var resource = new TestResource(() =>
            {
                if (Interlocked.Increment(ref cleanupCount) > 1) throw new ObjectDisposedException("Cleanup was called multiple times.");
            });

            var errors = new ConcurrentBag<Exception>();

            var threads = 10.Times(() => new Thread(() => {
                for (var i = 0; i < 500; i++)
                {
                    IDisposable instance;
                    try
                    {
                        // Acquire may legitimately refuse to acquire the reference.
                        instance = resource.Acquire();
                    }
                    catch (ObjectDisposedException)
                    {
                        return;
                    }
                    try
                    {
                        // Dispose must never fail to release it.
                        instance.Dispose();
                    }
                    catch (ObjectDisposedException ex)
                    {
                        errors.Add(ex);
                        throw;
                    }
                }
            })).ToArray();

            foreach (var t in threads) t.Start();

            foreach (var t in threads) t.Join();

            Assert.IsEmpty(errors);
        }

        class TestResource : ReferenceCountedResource
        {
            private readonly Action onCleanup;

            public TestResource(Action onCleanup)
            {
                this.onCleanup = onCleanup;
            }

            protected override void CleanUp()
            {
                onCleanup();
            }
        }

        class UnconstructableResource : TestResource
        {
            public UnconstructableResource(Action onCleanup) : base(onCleanup)
            {
                throw new Exception("can't construct");
            }
        }
    }
}
