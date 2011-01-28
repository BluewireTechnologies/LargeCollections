﻿using System;
using System.IO;

namespace LargeCollections.Storage
{
    public class GuidSerialiser : IItemSerialiser<Guid>
    {
        public void Write(Stream stream, Guid item)
        {
            var bytes = item.ToByteArray();
            stream.Write(bytes, 0, bytes.Length);
        }

        [ThreadStatic]
        private byte[] loadGuid;
        public Guid Read(Stream stream)
        {
            loadGuid = loadGuid ?? new byte[16];
            if (stream.Read(loadGuid, 0, loadGuid.Length) < loadGuid.Length)
            {
                throw new InvalidOperationException("Read past end of stream");
            }
            return new Guid(loadGuid);
        }
    }
}