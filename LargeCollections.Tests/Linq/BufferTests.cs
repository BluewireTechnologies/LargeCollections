using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LargeCollections.Linq;
using MbUnit.Framework;

namespace LargeCollections.Tests.Linq
{
    [TestFixture, CheckResources]
    public class BufferTests
    {
        [Test]
        public void BufferingRetainsSortOrderMetaInformation()
        {
            var set = new[] {1, 2, 3, 4}.UsesSortOrder(Comparer<int>.Default);

            using(var buffered = set.GetEnumerator().BufferInMemory())
            {
                Assert.IsNotNull(buffered.GetUnderlying<ISorted<int>>());
                Assert.AreEqual(Comparer<int>.Default, buffered.GetUnderlying<ISorted<int>>().SortOrder);
            }
        }
    }
}
