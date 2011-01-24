using System;
using System.IO;
using System.Linq;

namespace LargeCollections.Storage
{
    public class BufferedItemWriter<T> : IDisposable
    {
        private readonly Stream stream;
        private readonly IItemSerialiser<T> serialiser;

        private int ptr = 0;
        private T[] buffer = new T[128];

        public BufferedItemWriter(Stream stream, IItemSerialiser<T> serialiser)
        {
            this.stream = stream;
            this.serialiser = serialiser;
        }

        public void Write(T item)
        {
            buffer[ptr++] = item;
            if(ptr >= buffer.Length)
            {
                Flush();
            }
        }

        private void Flush()
        {
            if (ptr == 0) return;
            var array = buffer;
            if(ptr < array.Length)
            {
                array = array.Take(ptr).ToArray();
            }
            serialiser.Write(stream, array);
            ptr = 0;
        }

        public void Dispose()
        {
            Flush();
            stream.Dispose();
        }
    }
}