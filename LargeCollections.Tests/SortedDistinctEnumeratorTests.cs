using System.Collections.Generic;
using System.Linq;
using MbUnit.Framework;

namespace LargeCollections.Tests
{
    [TestFixture]
    public class SortedDistinctEnumeratorTests
    {
        [Test]
        public void Test()
        {
            var set = new List<int> {1, 2, 2, 3, 5, 5, 5, 6, 7, 8, 9, 10, 10};

            var distinctList = new List<int>();
            using(var distinctSet = new SortedDistinctEnumerator<int>(set.GetEnumerator()))
            {
                while (distinctSet.MoveNext())
                {
                    distinctList.Add(distinctSet.Current);
                }
            }

            Assert.AreElementsEqual(distinctList, set.Distinct());
        }
    }
}
