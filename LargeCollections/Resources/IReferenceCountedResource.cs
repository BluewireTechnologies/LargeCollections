using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LargeCollections.Resources
{
    public interface IReferenceCountedResource
    {
        int RefCount { get; }
        IDisposable Acquire();
    }

    public class ReferenceCountedResource : IReferenceCountedResource
    {
        public ReferenceCountedResource()
        {
            Trace = CaptureTrace();
        }

        public int RefCount { get; private set; }

        private bool released;

        public IDisposable Acquire()
        {
            if(released) throw new ObjectDisposedException(this.ToString(), "Resource has been released");
            RefCount++;
            return AddReference(new Reference(this));
        }

        private IDisposable AddReference(IDisposable reference)
        {
            references.Add(reference);
            return reference;
        }

        private void Release(IDisposable reference)
        {
            if (released) throw new ObjectDisposedException(this.ToString(), "Resource has been released");
            references.Remove(reference);
            RefCount--;
            if (RefCount == 0)
            {
                released = true;
                GC.SuppressFinalize(this);
                CleanUp();
            }
        }
    
        public string Trace { get; private set; }

        protected virtual void CleanUp()
        {
        }

        private IList<IDisposable> references = new List<IDisposable>();
        private static List<IReferenceCountedResource> leakedResources = new List<IReferenceCountedResource>();

#if DEBUG
        private static string CaptureTrace()
        {
            return new StackTrace(2).ToString();
        }
#else
        private string CaptureTrace()
        {
            return null;
        }
#endif
        public static IEnumerable<IReferenceCountedResource> GetLeakedResources()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            var resources = leakedResources.ToArray();
            leakedResources.Clear();
            return resources;
        }

        ~ReferenceCountedResource()
        {
            if (!released)
            {
#if DEBUG
                leakedResources.Add(this);
#endif
                // really unsafe:
                CleanUp();
                Debug.Fail(String.Format("Resource was not released before finalisation. {0}", this));
            }
        }

        class Reference : IDisposable
        {
            private readonly ReferenceCountedResource resource;
            private bool disposed;
            public Reference(ReferenceCountedResource resource)
            {
                this.resource = resource;
                trace = CaptureTrace();
            }

            private string trace;

            public void Dispose()
            {
                if(!disposed)
                {
                    disposed = true;
                    resource.Release(this);
                }
            }
        }
    }

    
}