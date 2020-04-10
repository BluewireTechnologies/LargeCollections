using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LargeCollections.Resources.Diagnostics
{
    public class NontracingLeakedResourceCounter : ILeakedResourceCounter
    {
        public void SetCrashLogDirectory(string crashLogDirectory)
        {
            crashLog = new CrashLog(crashLogDirectory);
        }

        private volatile CrashLog crashLog;

        private int count;

        public int CountLeaks()
        {
            GCBarrier();
            return count;
        }

        private static void GCBarrier()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public IEnumerable<IReferenceCountedResource> GetLeaks()
        {
            return new IReferenceCountedResource[CountLeaks()];
        }

        public void Leaked(IReferenceCountedResource resource)
        {
            count++;
        }

        public string CaptureTrace()
        {
            return null;
        }

        public void Reset()
        {
            GCBarrier();
            count = 0;
        }

        public void OnFinalizerCrash(Exception exception, ReferenceCountedResource resource, IEnumerable<ITracedReference> references)
        {
            try
            {
                var log = crashLog;
                if (log == null) return;
                log.Log(exception, resource, references.Count());
            }
            catch (Exception ex)
            {
                // Already crashing if this was called.
                Debug.Fail(String.Format("Failed to log crash details: {0}", ex.Message));
            }
        }
    }
}