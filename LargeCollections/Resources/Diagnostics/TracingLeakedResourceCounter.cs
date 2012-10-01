using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LargeCollections.Resources.Diagnostics
{
    public class TracingLeakedResourceCounter : ILeakedResourceCounter
    {
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
    }
}