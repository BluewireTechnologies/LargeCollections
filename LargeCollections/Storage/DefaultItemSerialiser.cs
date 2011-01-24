using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace LargeCollections.Storage
{
    public interface IItemSerialiser<T>
    {
        void Write(Stream stream, T[] item);
        /// <summary>
        /// Reads a block of data into buffer and returns the number of records read.
        /// </summary>
        /// <remarks>
        /// The buffer array may be reused between calls. If it is not large enough to contain the data, the method implementation should resize it.
        /// </remarks>
        /// <param name="stream"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        int Read(Stream stream, ref T[] buffer);
    }

    public class DefaultItemSerialiser<T> : IItemSerialiser<T>
    {
        private readonly BinaryFormatter serializer = new BinaryFormatter();

        public void Write(Stream stream, T[] item)
        {
            serializer.Serialize(stream, item);
        }

        public int Read(Stream stream, ref T[] buffer)
        {
            buffer = (T[])serializer.Deserialize(stream);
            return buffer.Length;
        }
    }
}