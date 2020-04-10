using System;
using System.Collections.Generic;
using Bluewire.ReferenceCounting;
using LargeCollections.Core;

namespace LargeCollections.Tests.Collections
{
    public abstract class LargeCollectionTestHarness<TBackingStore> : IDisposable
    {
        protected LargeCollectionTestHarness()
        {
            ReferenceCountedResource.Diagnostics.Reset();
        }

        public abstract IAccumulator<int> GetAccumulator();

        public abstract bool BackingStoreExists(IAccumulator<int> accumulator);
        public abstract bool BackingStoreExists(ILargeCollection<int> collection);


        public ILargeCollection<int> GetCollection(IEnumerable<int> values)
        {
            using (var accumulator = GetAccumulator())
            {
                accumulator.AddRange(values);
                return accumulator.Complete();
            }
        }

        public TBackingStore GetBackingStore(object obj)
        {
            return obj.GetUnderlying<IHasBackingStore<TBackingStore>>().BackingStore;
        }

        public virtual void Dispose()
        {
            Utils.AssertReferencesDisposed();
        }
    }
}