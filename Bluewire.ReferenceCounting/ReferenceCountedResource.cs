using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using LargeCollections.Resources.Diagnostics;

namespace LargeCollections.Resources
{
    /// <summary>
    /// Reference-counting marker for a resource.
    /// Threadsafe, although instances returned from Acquire() are not.
    /// </summary>
    public class ReferenceCountedResource : IReferenceCountedResource
    {
        public ReferenceCountedResource()
        {
            Trace = Diagnostics.CaptureTrace();
            DebugLog("Constructed resource : {0}", this);
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
            lock (references)
            {
                if (released) throw new ObjectDisposedException(this.ToString(), "Resource has been released");
                return AddReference(new Reference(this));
            }
        }

        private IDisposable AddReference(Reference reference)
        {
            RefCount++;
            DebugLog("Acquired resource : {0}", this);
            references.Add(reference);
            return reference;
        }

        private void Release(Reference reference)
        {
            lock (references)
            {
                if (released) throw new ObjectDisposedException(this.ToString(), "Resource has been released");
                references.Remove(reference);
                RefCount--;
                DebugLog("Released resource : {0}", this);

                if (RefCount != 0) return; // Reference is still live.

                this.released = true;
            }
            // This must be done out of the lock, since cleanup may take time.
            DebugLog("Disposing resource : {0}", this);
            GC.SuppressFinalize(this);
            CleanUp();
        }

        public readonly string Trace;

        protected virtual void CleanUp()
        {
        }

        protected virtual void WasLeaked()
        {
        }

        private readonly IList<Reference> references = new List<Reference>();
        
        private ITracedReference[] GetReferencesOnCrash()
        {
            lock (references)
            {
                return references.Cast<ITracedReference>().ToArray();
            }
        }

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
                WasLeaked();

                DebugLog("Resource leaked : {0}", this);
                // really unsafe:
                try
                {
                    CleanUp();
                }
                catch (Exception ex)
                {
                    Diagnostics.OnFinalizerCrash(ex, this, GetReferencesOnCrash());
                    throw; // Crash the application. This is intentional; shouldn't get here!
                }
                finally
                {
                    Debug.Fail(String.Format("Resource was not released before finalisation. {0}", this));
                }
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
        class Reference : IDisposable, ITracedReference
        {
            private readonly ReferenceCountedResource resource;
            private int disposed;
            public Reference(ReferenceCountedResource resource)
            {
                this.resource = resource;
                Trace = Diagnostics.CaptureTrace();
            }

            // Public field, provided for VS debugger. Properties might be inaccessible on a finaliser thread.
            public readonly string Trace;

            public void Dispose()
            {
                if (Interlocked.Increment(ref disposed) == 1)
                {
                    resource.Release(this);
                }
            }

            public string GetAcquisitionTrace()
            {
                return Trace;
            }
        }

        private static void DebugLog(string format, params object[] parameters)
        {
            if (Debug.Listeners.Count == 0) return;
            Debug.WriteLine(String.Format(format, parameters), typeof(ReferenceCountedResource).FullName);
        }
    }
}