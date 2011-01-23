using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace LargeCollections
{
    public interface IItemSerialiser<T>
    {
        void Write(Stream stream, T item);
        T Read(Stream stream);
    }

    public class DefaultItemSerialiser<T> : IItemSerialiser<T>
    {
        private readonly BinaryFormatter serializer = new BinaryFormatter();

        public void Write(Stream stream, T item)
        {
            serializer.Serialize(stream, item);
        }

        public T Read(Stream stream)
        {
            return (T)serializer.Deserialize(stream);
        }
    }

    public class BufferedItemWriter<T> : IDisposable
    {
        private readonly Stream stream;
        private readonly IItemSerialiser<T[]> serialiser;

        private int ptr = 0;
        private T[] buffer = new T[128];

        public BufferedItemWriter(Stream stream, IItemSerialiser<T[]> serialiser)
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

    public class BufferedItemReader<T> : IDisposable
    {
        private readonly Stream stream;
        private readonly IItemSerialiser<T[]> serialiser;

        private int ptr = 0;
        private T[] buffer;

        public BufferedItemReader(Stream stream, IItemSerialiser<T[]> serialiser)
        {
            this.stream = stream;
            this.serialiser = serialiser;
        }

        public T Read()
        {
            if (buffer == null || ptr >= buffer.Length)
            {
                Load();
            }
            return buffer[ptr++];
        }

        private void Load()
        {
            buffer = serialiser.Read(stream);
            ptr = 0;
        }

        public void Dispose()
        {
            stream.Dispose();
        }
    }
}