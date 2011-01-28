using System;
using System.Collections.Generic;

namespace LargeCollections.Resources.Diagnostics
{
    public class NontracingLeakedResourceCounter : ILeakedResourceCounter
    {
        private int count;

        public int CountLeaks()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            return count;
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
            count = 0;
        }
    }
}