using System;
using System.IO;

namespace LargeCollections.Core.Storage
{
    public class BufferedItemReader<T> : IDisposable
    {
        private readonly Stream stream;
        private readonly IItemSerialiser<T> serialiser;

        public BufferedItemReader(Stream stream, IItemSerialiser<T> serialiser)
        {
            this.stream = new BufferedStream(stream);
            this.serialiser = serialiser;
        }

        public T Read()
        {
            return serialiser.Read(stream);
        }

        public void Dispose()
        {
            stream.Dispose();
        }
    }
}