using System.Collections.Generic;
using System.IO;
using LargeCollections.Core.Collections;
using LargeCollections.Core.Storage;

namespace LargeCollections.FileSystem
{
    public class DiskBasedLargeCollection<T> : LargeCollectionWithBackingStore<T, FileReference>
    {
        private readonly IItemSerialiser<T> serialiser;

        public DiskBasedLargeCollection(FileReference backingStore, long itemCount, IItemSerialiser<T> serialiser) : base(backingStore, itemCount)
        {
            this.serialiser = serialiser;
        }

        protected override IEnumerator<T> GetEnumeratorImplementation()
        {
            using (var reader = new BufferedItemReader<T>(File.OpenRead(BackingStore.File.FullName), serialiser))
            {
                for (var i = 0; i < Count; i++)
                {
                    yield return reader.Read();
                }
            }
        }
    }
}
