using System.Collections.Generic;
using LargeCollections.Collections;
using MbUnit.Framework;

namespace LargeCollections.Tests.Collections
{
    [TestFixture, CheckResources]
    public class InMemoryLargeCollectionTests
    {
        [Test]
        public void DisposeDoesNotInvalidateBackingStore()
        {
            using(var collection = InMemoryAccumulator<int>.From(new[] {1, 2, 3, 4}))
            {
                var enumerator = collection.GetEnumerator();
                collection.Dispose();

                var list = new List<int>();
                while(enumerator.MoveNext()) list.Add(enumerator.Current);

                Assert.Count(4, list);


            }

        }
    }
}
