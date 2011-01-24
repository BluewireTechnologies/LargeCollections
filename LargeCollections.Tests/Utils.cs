using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LargeCollections.Resources;
using MbUnit.Framework;

namespace LargeCollections.Tests
{
    public static class Utils
    {
        public static IEnumerable<T> Times<T>(this int count, Func<T> generate)
        {
            while (count-- > 0) yield return generate();
        }

        public static void AssertReferencesDisposed()
        {
            var resources = ReferenceCountedResource.GetLeakedResources();
            Assert.IsEmpty(resources);
        }
    }
}
