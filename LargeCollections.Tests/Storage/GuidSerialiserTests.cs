using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Bluewire.ReferenceCounting.Tests;
using LargeCollections.Core.Storage;
using NUnit.Framework;

namespace LargeCollections.Tests.Storage
{
    [TestFixture, CheckResources]
    public class GuidSerialiserTests
    {
        private readonly GuidSerialiser serialiser = new GuidSerialiser();
        private void Write(Stream stream, params Guid[] items)
        {
            foreach (var batch in items)
            {
                serialiser.Write(stream, batch);
            }
            stream.Flush();
        }

        [Test]
        public void CanSerialiseGuidArray()
        {
            using (var stream = new MemoryStream())
            {
                Write(stream, 50.Times(Guid.NewGuid).ToArray());

                Assert.IsTrue(stream.Length > 0);
            }
        }

        [Test]
        public void CanDeserialiseGuid()
        {
            var id = Guid.NewGuid();
            using (var stream = new MemoryStream())
            {
                Write(stream, id);

                stream.Position = 0;
                Assert.AreEqual(id, serialiser.Read(stream));
            }
        }

        [Test]
        public void CanRoundTripMultipleGuids()
        {
            var ids = 50.Times(Guid.NewGuid).ToArray();
            using (var stream = new MemoryStream())
            {
                Write(stream, ids);

                stream.Position = 0;
                foreach (var batch in ids)
                {
                    Assert.AreEqual(batch, serialiser.Read(stream));
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

                    var readSetA = new List<Guid>();
                    var readSetB = new List<Guid>();
                    for (var i = 0; i < 20; i++)
                    {
                        readSetA.Add(serialiser.Read(streamA));
                        readSetB.Add(serialiser.Read(streamB));
                    }

                    CollectionAssert.AreEqual(setA, readSetA);
                    CollectionAssert.AreEqual(setB, readSetB);
                }
            }
        }

        [Test]
        public void IsThreadsafe()
        {
            var threads = 20.Times(() => new Thread(() =>
            {
                for (var i = 0; i < 50; i++)
                {
                    AssertRoundTrippable(Guid.NewGuid());
                }
            })).ToArray();

            foreach (var t in threads) t.Start();

            foreach (var t in threads) t.Join();
        }

        private void AssertRoundTrippable(Guid guid)
        {
            Assert.AreEqual(guid, RoundTrip(guid));
        }

        private Guid RoundTrip(Guid guid)
        {
            using (var stream = new MemoryStream())
            {
                serialiser.Write(stream, guid);
                stream.Position = 0;
                return serialiser.Read(stream);
            }
        }
    }
}
