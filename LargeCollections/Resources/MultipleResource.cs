using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LargeCollections.Resources
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
            return new DisposableList<IDisposable>(resources.Select(r => r.Acquire()));
        }
    }
}
