using System;
using System.IO;
using System.Linq;
using LargeCollections.Storage;
using MbUnit.Framework;

namespace LargeCollections.Tests.Storage
{
    [TestFixture]
    public class GuidBatchSerialiserTests
    {
        private GuidBatchSerialiser serialiser = new GuidBatchSerialiser();
        private void Write(Stream stream, params Guid[][] batches)
        {
            foreach (var batch in batches)
            {
                serialiser.Write(stream, batch);
            }
            stream.Flush();
        }

        [Test]
        public void CanSerialiseGuidArray()
        {
            using(var stream = new MemoryStream())
            {
                Write(stream, 50.Times(Guid.NewGuid).ToArray());

                Assert.IsTrue(stream.Length > 0);
            }
        }

        [Test]
        public void CanDeserialiseGuidArray()
        {
            var ids = 50.Times(Guid.NewGuid).ToArray();
            using (var stream = new MemoryStream())
            {
                Write(stream, ids);

                stream.Position = 0;
                Guid[] deserialised = null;
                serialiser.Read(stream, ref deserialised);
                Assert.AreElementsEqual(ids, deserialised);
            }
        }

        [Test]
        public void CanRoundTripMultipleGuidArrays()
        {
            var ids = 20.Times(() => 20.Times(Guid.NewGuid).ToArray()).ToArray();
            using (var stream = new MemoryStream())
            {
                Write(stream, ids);

                stream.Position = 0;
                foreach (var batch in ids)
                {
                    Guid[] deserialised = null;
                    serialiser.Read(stream, ref deserialised);
                    Assert.AreElementsEqual(batch, deserialised);
                }
            }
        }

        [Test]
        public void CanUseSerialiserForMultipleStreamsSimultaneously()
        {
            var setA = 20.Times(Guid.NewGuid).ToArray();
            var setB = 20.Times(Guid.NewGuid).ToArray();
            using (var streamA = new MemoryStream())
            {
                using (var streamB = new MemoryStream())
                {
                    Write(streamA, setA);
                    Write(streamB, setB);

                    streamA.Position = 0;
                    streamB.Position = 0;

                    Guid[] readSetA = null;
                    Guid[] readSetB = null;
                    serialiser.Read(streamA, ref readSetA);
                    serialiser.Read(streamB, ref readSetB);
                    
                    Assert.AreElementsEqual(setA, readSetA);
                    Assert.AreElementsEqual(setB, readSetB);
                }
            }
        }
    }
}
