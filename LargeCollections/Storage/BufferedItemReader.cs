using System;
using System.IO;

namespace LargeCollections.Storage
{
    public class BufferedItemReader<T> : IDisposable
    {
        private readonly Stream stream;
        private readonly IItemSerialiser<T> serialiser;

        private int ptr = 0;
        private T[] buffer;
        private int recordCount;

        public BufferedItemReader(Stream stream, IItemSerialiser<T> serialiser)
        {
            this.stream = stream;
            this.serialiser = serialiser;
        }

        public T Read()
        {
            if (buffer == null || ptr >= recordCount)
            {
                Load();
            }
            return buffer[ptr++];
        }

        private void Load()
        {
            recordCount = serialiser.Read(stream, ref buffer);
            ptr = 0;
        }

        public void Dispose()
        {
            stream.Dispose();
        }
    }
}