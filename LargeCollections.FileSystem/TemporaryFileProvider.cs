using System;
using System.IO;

namespace LargeCollections.FileSystem
{
    public class TemporaryFileProvider
    {
        private readonly string tempRoot;

        public TemporaryFileProvider(string tempRoot)
        {
            this.tempRoot = tempRoot;
            Directory.CreateDirectory(tempRoot);
        }

        public TemporaryFileProvider() : this(Path.GetTempPath())
        {
        }

        public string GetTempFile()
        {
            string fileName;

            // PARANOIA
            // If it takes more than ten attempts to generate a unique filename, our temporary directory must have quintillions of files in it.
            var attempts = 10;
            do
            {
                if (--attempts < 0)
                {
                    // This probably can't happen, but if it does then the only reasonable response is to give up.
                    throw new OutOfMemoryException("Cannot generate a unique random filename. All attempts were duplicates.");
                }
                fileName = Path.Combine(tempRoot, Path.GetRandomFileName());
            } while (File.Exists(fileName));
            using (File.Create(fileName))
            {
            }
            return fileName;
        }
    }
}
