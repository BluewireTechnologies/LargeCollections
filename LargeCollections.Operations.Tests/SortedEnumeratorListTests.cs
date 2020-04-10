using System;
using LargeCollections.Core;
using LargeCollections.Core.Collections;
using LargeCollections.Tests;
using NUnit.Framework;

namespace LargeCollections.Operations.Tests
{
    [TestFixture, CheckResources]
    public class SortedEnumeratorListTests
    {
        private IDisposableEnumerable<int> Unsorted(params int[] items)
        {
            return InMemoryAccumulator<int>.From(items);
        }

        [Test]
        public void UnsortedInputSetsCauseAnException()
        {
            // not wrapped with an ISorted<int>, therefore 'unsorted'.
            using (var setA = Unsorted(2, 4, 6, 7, 7, 9, 12))
            {
                using (var setB = Unsorted(1, 3, 3, 4, 7))
                {
                    Assert.Catch<InvalidOperationException>(() => 
                        new SortedEnumeratorList<int>(new[] { setA.GetEnumerator(), setB.GetEnumerator() }));
                }
            }
        }

        [Test]
        public void ExceptionDuringConstructionCausesDisposalOfEnumerators()
        {
            // not wrapped with an ISorted<int>, therefore 'unsorted'.
            using (var setA = Unsorted(2, 4, 6, 7, 7, 9, 12))
            {
                using (var setB = Unsorted(1, 3, 3, 4, 7))
                {
                    Assert.Catch<InvalidOperationException>(() =>
                        new SortedEnumeratorList<int>(new[] { setA.GetEnumerator(), setB.GetEnumerator() }));
                }
            }

            // this happens in teardown anyway, but be explicit about it.
            Utils.AssertReferencesDisposed();
        }

        [Test]
        public void ExceptionDuringEnumeratorWrappingCausesDisposalOfEnumerators()
        {
            // not wrapped with an ISorted<int>, therefore 'unsorted'.
            using (var setA = Unsorted(2, 4, 6, 7, 7, 9, 12))
            {
                using (var setB = Unsorted(1, 3, 3, 4, 7))
                {
                    Assert.Catch<InvalidOperationException>(() =>
                        new SortedEnumeratorList<int>(new[] { setA.GetEnumerator(), setB.GetEnumerator() }, e => { throw new InvalidOperationException(); }));
                }
            }

            // this happens in teardown anyway, but be explicit about it.
            Utils.AssertReferencesDisposed();
        }
    }
}