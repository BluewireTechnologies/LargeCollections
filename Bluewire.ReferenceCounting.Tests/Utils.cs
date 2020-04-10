using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Bluewire.ReferenceCounting.Tests
{
    public static class Utils
    {
        public static IEnumerable<T> Times<T>(this int count, Func<T> generate)
        {
            while (count-- > 0) yield return generate();
        }

        public static void AssertReferencesDisposed()
        {
            try
            {
                Assert.AreEqual(0, ReferenceCountedResource.Diagnostics.CountLeaks());
            }
            finally
            {
                ReferenceCountedResource.Diagnostics.Reset();
            }
        }
    }
}
