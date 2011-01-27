using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

namespace LargeCollections.Storage
{
    public interface IItemSerialiser<T>
    {
        void Write(Stream stream, T item);
        /// <summary>
        /// Reads an item from the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        T Read(Stream stream);
    }

    public class DefaultItemSerialiser<T> : IItemSerialiser<T>
    {
        private readonly BinaryFormatter serializer = new BinaryFormatter(){AssemblyFormat = FormatterAssemblyStyle.Simple, TypeFormat = FormatterTypeStyle.TypesWhenNeeded};

        public void Write(Stream stream, T item)
        {
            serializer.Serialize(stream, item);
        }

        public T Read(Stream stream)
        {
            return (T)serializer.Deserialize(stream);
        }
    }
}