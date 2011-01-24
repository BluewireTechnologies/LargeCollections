using System;
using System.IO;

namespace LargeCollections.Storage
{
    public class GuidBatchSerialiser : IItemSerialiser<Guid>
    {
        [ThreadStatic]
        private byte[] writeBuffer = new byte[0];
        private void EnsureBuffer<T>(ref T[] buffer, int length)
        {
            if (buffer == null || length > buffer.Length)
            {
                buffer = new T[length];
            }
        }

        public void Write(Stream stream, Guid[] item)
        {
            var length = item.Length;
            var size = 4 + (16*length);

            EnsureBuffer(ref writeBuffer, size);

            BitConverter.GetBytes(length).CopyTo(writeBuffer, 0);
            for(int i = 0, offset = 4; i < item.Length; i++, offset+=16)
            {
                item[i].ToByteArray().CopyTo(writeBuffer, offset);
            }
            stream.Write(writeBuffer, 0, size);
        }

        [ThreadStatic]
        private byte[] intBuffer = new byte[4];
        [ThreadStatic]
        private byte[] loadGuid;
        public int Read(Stream stream, ref Guid[] buffer)
        {
            if(stream.Read(intBuffer, 0, 4) <= 0)
            {
                throw new InvalidOperationException("Read past end of stream");
            }
            var length = BitConverter.ToInt32(intBuffer, 0);
            EnsureBuffer(ref buffer, length);
            loadGuid = loadGuid ?? new byte[16];
            for(var i = 0; i < length; i++)
            {
                if (stream.Read(loadGuid, 0, loadGuid.Length) <= 0)
                {
                    throw new InvalidOperationException("Read past end of stream");
                }
                buffer[i] = new Guid(loadGuid);
            }
            return length;
        }
    }
}