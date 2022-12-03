using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Bluewire.ReferenceCounting.Diagnostics
{
    public class TracingLeakedResourceCounter : ILeakedResourceCounter
    {
        public void SetCrashLogDirectory(string crashLogDirectory)
        {
            crashLog = new CrashLog(crashLogDirectory);
        }

        private volatile CrashLog crashLog;

        private readonly List<IReferenceCountedResource> leakedResources = new List<IReferenceCountedResource>();

        public void Leaked(IReferenceCountedResource resource)
        {
            leakedResources.Add(resource);
        }

        private static void GCBarrier()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }


        public string CaptureTrace()
        {
            return new StackTrace(2, true).ToString();
        }

        public int CountLeaks()
        {
            return GetLeaks().Count();
        }

        public IEnumerable<IReferenceCountedResource> GetLeaks()
        {
            GCBarrier();
            return leakedResources.ToArray();
        }

        public void Reset()
        {
            GCBarrier();
            leakedResources.Clear();
        }

        public void OnFinalizerCrash(Exception exception, ReferenceCountedResource resource, IEnumerable<ITracedReference> references)
        {
            try
            {
                var log = crashLog;
                if (log == null) return;
                log.Log(exception, resource, references.ToArray());
            }
            catch (Exception ex)
            {
                // Already crashing if this was called.
                Debug.Fail(String.Format("Failed to log crash details: {0}", ex.Message));
            }
        }
    }
}
