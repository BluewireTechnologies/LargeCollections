using System.Linq;
using LargeCollections.Collections;
using MbUnit.Framework;

namespace LargeCollections.Tests
{
    [TestFixture]
    public class InMemoryLargeCollectionTests
    {
        [Test]
        public void DisposeDoesNotInvalidateBackingStore()
        {
            using(var collection = InMemoryAccumulator<int>.From(new[] {1, 2, 3, 4}))
            {
                collection.Dispose();

                Assert.Count(4, collection.ToArray());


            }

        }
    }
}
