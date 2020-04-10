using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

namespace LargeCollections.Core.Storage
{
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