using System;
using System.IO;

namespace LargeCollections.Resources
{
    public class FileReference : ReferenceCountedResource
    {
        private readonly string fileName;

        public FileReference(string fileName)
        {
            this.fileName = fileName;
        }

        protected override void CleanUp()
        {
            if (File.Exists) File.Delete();
        }

        public FileInfo File { get { return new FileInfo(fileName); } }

        public override string ToString()
        {
            return String.Format("{{{0} refs: {1}}}", RefCount, fileName);
        }
    }
}
