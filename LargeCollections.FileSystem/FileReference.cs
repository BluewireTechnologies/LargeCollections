using System;
using System.IO;
using Bluewire.ReferenceCounting;

namespace LargeCollections.FileSystem
{
    public class FileReference : ReferenceCountedResource
    {
        private readonly string fileName;

        public FileReference(string fileName)
        {
            this.fileName = fileName;
        }

        public FileInfo File { get { return new FileInfo(fileName); } }

        public override string ToString()
        {
            return String.Format("{{{0} refs: {1}}}", RefCount, fileName);
        }
    }
}
