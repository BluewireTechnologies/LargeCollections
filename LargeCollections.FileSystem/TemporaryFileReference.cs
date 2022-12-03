using System;

namespace LargeCollections.FileSystem
{
    public class TemporaryFileReference : FileReference
    {
        public TemporaryFileReference(string fileName) : base(fileName)
        {
            if (File.Exists && File.Length > 0) throw new InvalidOperationException("File already exists: " + fileName);
        }

        protected override void CleanUp()
        {
            if (File.Exists) File.Delete();
        }
    }
}
