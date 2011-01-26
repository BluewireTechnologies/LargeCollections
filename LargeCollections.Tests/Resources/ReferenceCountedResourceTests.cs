using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            var leakedResources = ReferenceCountedResource.GetLeakedResources();
            Assert.Count(1, leakedResources);
            Assert.AreSame(resource.Target, leakedResources.Single());
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
                var leakedResources = ReferenceCountedResource.GetLeakedResources();
                Assert.IsEmpty(leakedResources);
            }
        }

        class UnconstructableResource : ReferenceCountedResource
        {
            private readonly Action onCleanup;

            public UnconstructableResource(Action onCleanup)
            {
                this.onCleanup = onCleanup;
                throw new Exception("can't construct");
            }

            protected override void CleanUp()
            {
                onCleanup();
            }
        }
    }
}
