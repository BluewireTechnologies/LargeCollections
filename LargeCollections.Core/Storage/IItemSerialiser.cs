using System.IO;

namespace LargeCollections.Core.Storage
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
}