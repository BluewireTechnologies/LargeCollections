using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using LargeCollections.Resources.Diagnostics;
using log4net;

namespace LargeCollections.Resources
{
    /// <summary>
    /// Reference-counting marker for a resource.
    /// Threadsafe, although instances returned from Acquire() are not.
    /// </summary>
    public class ReferenceCountedResource : IReferenceCountedResource
    {
        private static ILog log = LogManager.GetLogger(typeof (ReferenceCountedResource));

        public ReferenceCountedResource()
        {
            Trace = Diagnostics.CaptureTrace();
            log.DebugFormat("Constructed resource : {0}", this);
        }

        public int RefCount { get; private set; }

        private bool released;

        /// <summary>
        /// Acquire an instance of this resource, for use on the calling thread.
        /// DO NOT share instances between threads.
        /// </summary>
        /// <returns></returns>
        public IDisposable Acquire()
        {
            lock (this)
            {
                if (released) throw new ObjectDisposedException(this.ToString(), "Resource has been released");
                return AddReference(new Reference(this));
            }
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
            lock (this)
            {
                if (released) throw new ObjectDisposedException(this.ToString(), "Resource has been released");
                references.Remove(reference);
                RefCount--;
                log.DebugFormat("Released resource : {0}", this);

                if (RefCount != 0) return; // Reference is still live.

                this.released = true;
            }
            // This must be done out of the lock, since cleanup may take time.
            log.DebugFormat("Disposing resource : {0}", this);
            GC.SuppressFinalize(this);
            CleanUp();
        }

        public readonly string Trace;

        protected virtual void CleanUp()
        {
        }

        private readonly IList<IDisposable> references = new SynchronizedCollection<IDisposable>();
        

#if DEBUG || DEBUG_REFERENCE_COUNTS
        public static ILeakedResourceCounter Diagnostics = new TracingLeakedResourceCounter();
#else
        public static ILeakedResourceCounter Diagnostics = new NontracingLeakedResourceCounter();
#endif

        ~ReferenceCountedResource()
        {
            if (!released)
            {
                if (RefCount == 0) return; // construction failed.

                Diagnostics.Leaked(this);

                log.DebugFormat("Resource leaked : {0}", this);
                // really unsafe:
                CleanUp();
                Debug.Fail(String.Format("Resource was not released before finalisation. {0}", this));
            }
        }

        /// <summary>
        /// Reference to a Reference-Counted Resource.
        /// NOT guaranteed threadsafe. These must NEVER be shared across threads.
        /// </summary>
        /// <remarks>
        /// Actually this probably is threadsafe, but you're still not meant to share it
        /// across threads because it makes it more difficult to ensure it's only disposed
        /// once.
        /// </remarks>
        class Reference : IDisposable
        {
            private readonly ReferenceCountedResource resource;
            private int disposed;
            public Reference(ReferenceCountedResource resource)
            {
                this.resource = resource;
                Trace = Diagnostics.CaptureTrace();
            }

            public readonly string Trace;

            public void Dispose()
            {
                if (Interlocked.Increment(ref disposed) == 1)
                {
                    resource.Release(this);
                }
            }
        }
    }
}