using System.IO;

namespace LargeCollections
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
            do
            {
                fileName = Path.Combine(tempRoot, Path.GetRandomFileName());
            } while (File.Exists(fileName));
            using(File.Create(fileName))
            {
            }
            return fileName;
        }
    }
}