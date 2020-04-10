using System;
using System.Linq;

namespace Bluewire.ReferenceCounting
{
    public class MultipleResource : IReferenceCountedResource
    {
        private readonly IReferenceCountedResource[] resources;
        
        public MultipleResource(params IReferenceCountedResource[] resources)
        {
            this.resources = resources;
        }

        public int RefCount
        {
            get { return resources.Max(r => r.RefCount); }
        }

        public IDisposable Acquire()
        {
            return new DisposableList<IDisposable>(resources.EvaluateSafely(r => r.Acquire()));
        }
    }
}
