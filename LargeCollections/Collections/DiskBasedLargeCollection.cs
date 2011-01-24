using System;
using System.Collections.Generic;
using System.IO;
using LargeCollections.Resources;
using LargeCollections.Storage;

namespace LargeCollections.Collections
{
    public class DiskBasedLargeCollection<T> : ILargeCollection<T>, IHasBackingStore<FileReference>
    {
        private readonly IItemSerialiser<T> serialiser;

        public DiskBasedLargeCollection(FileReference backingStore, long itemCount, IItemSerialiser<T> serialiser)
        {
            this.serialiser = serialiser;
            BackingStore = backingStore;
            Count = itemCount;
            reference = BackingStore.Acquire();
        }

        public IEnumerator<T> GetEnumerator()
        {
            if(disposed) throw new ObjectDisposedException("DiskBasedLargeCollection");
            using(var reader = new BufferedItemReader<T>(File.OpenRead(BackingStore.File.FullName), serialiser))
            {
                for (var i = 0; i < Count; i++)
                {
                    yield return reader.Read();
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private bool disposed = false;
        private IDisposable reference;

        public void Dispose()
        {
            reference.Dispose();
        }

        public long Count { get; private set; }

        public FileReference BackingStore { get; private set; }
    }
}
