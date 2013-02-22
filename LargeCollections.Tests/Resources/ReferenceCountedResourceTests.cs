﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Gallio.Framework;
using LargeCollections.Resources;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;

namespace LargeCollections.Tests.Resources
{
    [TestFixture, CheckResources]
    public class ReferenceCountedResourceTests
    {
        [Test]
        public void CanDetectLeakedReferencedResources()
        {
            var instance = new ReferenceCountedResource();
            instance.Acquire();
            var resource = new WeakReference(instance, true);
            instance = null;

            Assert.AreEqual(1, ReferenceCountedResource.Diagnostics.CountLeaks());
            ReferenceCountedResource.Diagnostics.Reset();
        }

        [Test]
        [ExpectedException(typeof(Exception), "can't construct")]
        public void ExceptionInDerivedConstructor_DoesNotCauseCleanup()
        {
            new UnconstructableResource(Assert.Fail);
        }

        [Test]
        [ExpectedException(typeof(Exception), "can't construct")]
        public void ExceptionInDerivedConstructor_DoesNotCauseLeak()
        {
            try
            {
                new UnconstructableResource(() => { });
            }
            finally
            {
                Assert.AreEqual(0, ReferenceCountedResource.Diagnostics.CountLeaks());
            }
        }

        [Test, ThreadedRepeat(50)]
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
