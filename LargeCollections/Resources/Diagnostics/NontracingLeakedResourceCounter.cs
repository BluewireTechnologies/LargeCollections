using System;
using System.Collections.Generic;

namespace LargeCollections.Resources.Diagnostics
{
    public class NontracingLeakedResourceCounter : ILeakedResourceCounter
    {
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
    }
}