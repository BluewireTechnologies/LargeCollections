using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LargeCollections.Storage;
using MbUnit.Framework;
using Moq;

namespace LargeCollections.Tests.Storage
{
    [TestFixture]
    public class BufferedItemWriterTests
    {
        [Test]
        public void SingleValueIsWrittenAsSingleElementArray()
        {
            var serialiser = new Mock<IItemSerialiser<int>>();
            serialiser.Setup(s => s.Write(It.IsAny<Stream>(), It.IsAny<int[]>()));
            
            using(var stream = new MemoryStream())
            {
                using (var writer = new BufferedItemWriter<int>(stream, serialiser.Object))
                {
                    writer.Write(1);
                }
                serialiser.Verify(s => s.Write(stream, new [] { 1 }));
            }

            
        }
    }
}
