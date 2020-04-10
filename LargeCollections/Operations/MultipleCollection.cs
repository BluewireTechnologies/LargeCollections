using System;
using System.Collections.Generic;
using Bluewire.ReferenceCounting;
using LargeCollections.Resources;

namespace LargeCollections.Operations
{
    public abstract class MultipleCollection<T> : IHasBackingStore<IReferenceCountedResource>, IDisposable
    {
        private readonly IDisposable resourceReference;

        protected MultipleCollection(params IEnumerable<T>[] collections)
        {
            BackingStore = collections.CollectResources();
            resourceReference = BackingStore.Acquire();
        }

        public virtual void Dispose()
        {
            resourceReference.Dispose();
        }

        public IReferenceCountedResource BackingStore { get; private set; }
    }
}