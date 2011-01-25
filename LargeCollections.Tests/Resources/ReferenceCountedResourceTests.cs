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
    [TestFixture]
    public class ReferenceCountedResourceTests
    {
        [Test]
        public void CanDetectLeakedUnusedResources()
        {
            var resource = new WeakReference(new ReferenceCountedResource(), true);

            var leakedResources = ReferenceCountedResource.GetLeakedResources();
            Assert.Count(1, leakedResources);
            Assert.AreSame(resource.Target, leakedResources.Single());
        }

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
    }
}
