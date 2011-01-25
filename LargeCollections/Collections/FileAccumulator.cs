using System;
using System.Collections.Generic;
using System.IO;
using LargeCollections.Resources;
using LargeCollections.Storage;

namespace LargeCollections.Collections
{
    public class FileAccumulator<T> : IAccumulator<T>
    {
        public string FileName { get; private set; }
        private readonly FileReference fileResource;
        private readonly IItemSerialiser<T> serialiser;
        private readonly BufferedItemWriter<T> writer;
        private IDisposable reference;

        public FileAccumulator(string file, IItemSerialiser<T> serialiser)
        {
            FileName = file;

            fileResource = new TemporaryFileReference(file);
            reference = fileResource.Acquire();
            try
            {
                this.serialiser = serialiser;

                this.writer = new BufferedItemWriter<T>(File.OpenWrite(file), serialiser);
            }
            catch
            {
                reference.Dispose();
                throw;
            }
        }

        public void Add(T item)
        {
            writer.Write(item);
            Count++;
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

        public long Count { get; private set; }

        public ILargeCollection<T> Complete()
        {
            return new DiskBasedLargeCollection<T>(fileResource, Count, serialiser);
        }

        public void Dispose()
        {
            writer.Dispose();
            reference.Dispose();
        }
    }
}