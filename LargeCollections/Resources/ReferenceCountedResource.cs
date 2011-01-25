using System;
using System.Collections.Generic;
using System.Diagnostics;
using log4net;

namespace LargeCollections.Resources
{
    public class ReferenceCountedResource : IReferenceCountedResource
    {
        private static ILog log = LogManager.GetLogger(typeof (ReferenceCountedResource));

        public ReferenceCountedResource()
        {
            Trace = CaptureTrace();
            log.DebugFormat("Constructed resource : {0}", this);
        }

        public int RefCount { get; private set; }

        private bool released;

        public IDisposable Acquire()
        {
            if(released) throw new ObjectDisposedException(this.ToString(), "Resource has been released");
            return AddReference(new Reference(this));
        }

        private IDisposable AddReference(IDisposable reference)
        {
            RefCount++;
            log.DebugFormat("Acquired resource : {0}", this);
            references.Add(reference);
            return reference;
        }

        private void Release(IDisposable reference)
        {
            if (released) throw new ObjectDisposedException(this.ToString(), "Resource has been released");
            references.Remove(reference);
            RefCount--;
            log.DebugFormat("Released resource : {0}", this);
            if (RefCount == 0)
            {
                log.DebugFormat("Disposing resource : {0}", this);
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
                if (RefCount == 0) return; // construction failed.
#if DEBUG
                leakedResources.Add(this);
#endif
                log.DebugFormat("Resource leaked : {0}", this);
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