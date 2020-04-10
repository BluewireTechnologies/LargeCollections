using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using LargeCollections.Core;
using LargeCollections.Core.Storage;

namespace LargeCollections.FileSystem
{
    public class FileAccumulator<T> : IAccumulator<T>, IHasBackingStore<FileReference>
    {
        public string FileName { get; private set; }
        private readonly FileReference fileResource;
        public FileReference BackingStore { get { return fileResource; } }

        private readonly IItemSerialiser<T> serialiser;
        private readonly BufferedItemWriter<T> writer;
        private IDisposable reference;
        private bool completed = false;

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
            if(completed) throw new ReadOnlyException();
            writer.Write(item);
            Count++;
        }

        public void AddRange(IEnumerable<T> items)
        {
            if (completed) throw new ReadOnlyException();
            foreach (var item in items)
            {
                writer.Write(item);
                Count++;
            }
        }

        public long Count { get; private set; }

        public ILargeCollection<T> Complete()
        {
            if (completed) throw new InvalidOperationException();
            completed = true;
            writer.Dispose();
            return new DiskBasedLargeCollection<T>(fileResource, Count, serialiser);
        }

        public void Dispose()
        {
            writer.Dispose();
            reference.Dispose();
        }
    }
}