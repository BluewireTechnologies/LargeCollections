using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LargeCollections.Core;
using LargeCollections.Core.Collections;
using LargeCollections.Operations;
using NUnit.Framework;

namespace LargeCollections.Tests.Operations
{
    [TestFixture, CheckResources]
    public class ConcatenatedEnumeratorTests
    {
        private int[] Evaluate(IEnumerator<int> enumerator)
        {
            var list = new List<int>();
            while(enumerator.MoveNext()) list.Add(enumerator.Current);
            return list.ToArray();
        }

        [Test]
        public void CanConcatenateEnumerators()
        {
            var setA = new List<int> {1, 2, 3, 4};
            var setB = new List<int> { 2, 4, 6, 8 };
            var concat = setA.GetEnumerator().Concat(setB.GetEnumerator());

            CollectionAssert.AreEqual(setA.Concat(setB), Evaluate(concat));
        }

        [Test]
        public void ConcatenatedEnumeratorInheritsCount_If_AllEnumeratorsAreCountable()
        {
            using (var setA = InMemoryAccumulator<int>.From(new[] {1, 2, 3, 4}))
            {
                using (var setB = InMemoryAccumulator<int>.From(new[] {2, 4, 6, 8}))
                {
                    using (var concat = setA.GetEnumerator().Concat(setB.GetEnumerator()))
                    {
                        Assert.IsNotNull(concat.GetUnderlying<ICounted>());
                        Assert.AreEqual(8, concat.GetUnderlying<ICounted>().Count);
                    }
                }
            }
        }

        [Test]
        public void ConcatenatedEnumeratorDoesNotInheritCount_If_AnyEnumeratorsAreNotCountable()
        {
            using (var setA = InMemoryAccumulator<int>.From(new[] { 1, 2, 3, 4 }))
            {
                var setB = new List<int> {2, 4, 6, 8};
                using (var concat = setA.GetEnumerator().Concat(setB.GetEnumerator()))
                {
                    Assert.IsNull(concat.GetUnderlying<ICounted>());
                }
            }
        }

        [Test]
        public void ConcatenatedEnumeratorDoesNotInheritSort()
        {
            var setA = new List<int> { 1, 2, 3, 4 }.UsesSortOrder(Comparer<int>.Default);
            var setB = new List<int> { 2, 4, 6, 8 }.UsesSortOrder(Comparer<int>.Default);
            using(var concat = setA.GetEnumerator().Concat(setB.GetEnumerator()))
            {
                Assert.IsNull(concat.GetUnderlying<ISorted<int>>());
            }
        }
    }
}
