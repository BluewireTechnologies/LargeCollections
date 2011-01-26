using System;
using System.IO;
using System.Linq;

namespace LargeCollections.Storage
{
    public class BufferedItemWriter<T> : IDisposable
    {
        private readonly Stream stream;
        private readonly IItemSerialiser<T> serialiser;

        public BufferedItemWriter(Stream stream, IItemSerialiser<T> serialiser)
        {
            this.stream = new BufferedStream(stream, 64*1024); // 64kb write buffer.
            this.serialiser = serialiser;
        }

        public void Write(T item)
        {
            serialiser.Write(stream, item);
        }
        
        public void Dispose()
        {
            stream.Dispose();
        }
    }
}