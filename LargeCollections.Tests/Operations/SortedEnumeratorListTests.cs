using System;
using LargeCollections.Collections;
using LargeCollections.Operations;
using MbUnit.Framework;

namespace LargeCollections.Tests.Operations
{
    [TestFixture, CheckResources]
    public class SortedEnumeratorListTests
    {
        private IDisposableEnumerable<int> Unsorted(params int[] items)
        {
            return InMemoryAccumulator<int>.From(items);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UnsortedInputSetsCauseAnException()
        {
            // not wrapped with an ISorted<int>, therefore 'unsorted'.
            using (var setA = Unsorted(2, 4, 6, 7, 7, 9, 12))
            {
                using (var setB = Unsorted(1, 3, 3, 4, 7))
                {
                    new SortedEnumeratorList<int>(new[] { setA.GetEnumerator(), setB.GetEnumerator() });
                }
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExceptionDuringConstructionCausesDisposalOfEnumerators()
        {
            // not wrapped with an ISorted<int>, therefore 'unsorted'.
            using (var setA = Unsorted(2, 4, 6, 7, 7, 9, 12))
            {
                using (var setB = Unsorted(1, 3, 3, 4, 7))
                {
                    new SortedEnumeratorList<int>(new[] { setA.GetEnumerator(), setB.GetEnumerator() });
                }
            }

            // this happens in teardown anyway, but be explicit about it.
            Utils.AssertReferencesDisposed();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExceptionDuringEnumeratorWrappingCausesDisposalOfEnumerators()
        {
            // not wrapped with an ISorted<int>, therefore 'unsorted'.
            using (var setA = Unsorted(2, 4, 6, 7, 7, 9, 12))
            {
                using (var setB = Unsorted(1, 3, 3, 4, 7))
                {
                    new SortedEnumeratorList<int>(new[] { setA.GetEnumerator(), setB.GetEnumerator() }, e => { throw new InvalidOperationException(); });
                }
            }

            // this happens in teardown anyway, but be explicit about it.
            Utils.AssertReferencesDisposed();
        }
    }
}