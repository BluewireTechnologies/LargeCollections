using System;

namespace LargeCollections
{
    public interface IReferenceCountedResource
    {
        int RefCount { get; }
        IDisposable Acquire();
    }

    public abstract class ReferenceCountedResource : IReferenceCountedResource
    {
        public int RefCount { get; private set; }

        private bool released;

        public IDisposable Acquire()
        {
            if(released) throw new ObjectDisposedException(this.ToString(), "Resource has been released");
            RefCount++;
            return new Reference(this);
        }

        private void Release()
        {
            if (released) throw new ObjectDisposedException(this.ToString(), "Resource has been released");
            RefCount--;
            if (RefCount == 0)
            {
                released = true;
                CleanUp();
            }
        }

        protected abstract void CleanUp();

        class Reference : IDisposable
        {
            private readonly ReferenceCountedResource resource;
            private bool disposed;
            public Reference(ReferenceCountedResource resource)
            {
                this.resource = resource;
            }

            public void Dispose()
            {
                if(!disposed)
                {
                    disposed = true;
                    resource.Release();
                }
            }
        }
    }

    
}