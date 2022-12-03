using System;
using System.IO;
using System.Threading;

namespace LargeCollections.Core.Storage
{
    public class GuidSerialiser : IItemSerialiser<Guid>
    {
        public void Write(Stream stream, Guid item)
        {
            var bytes = item.ToByteArray();
            stream.Write(bytes, 0, bytes.Length);
        }

        private static readonly ThreadLocal<byte[]> perThreadByteArray = new ThreadLocal<byte[]>(() => new byte[16]);
        public Guid Read(Stream stream)
        {
            var loadGuid = perThreadByteArray.Value;
            if (stream.Read(loadGuid, 0, loadGuid.Length) < loadGuid.Length)
            {
                throw new InvalidOperationException("Read past end of stream");
            }
            return new Guid(loadGuid);
        }
    }
}
