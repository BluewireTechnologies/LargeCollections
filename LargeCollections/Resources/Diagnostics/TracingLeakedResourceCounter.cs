using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LargeCollections.Resources.Diagnostics
{
    public class TracingLeakedResourceCounter : ILeakedResourceCounter
    {
        private List<IReferenceCountedResource> leakedResources = new List<IReferenceCountedResource>();

        public void Leaked(IReferenceCountedResource resource)
        {
            leakedResources.Add(resource);
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
            GC.Collect();
            GC.WaitForPendingFinalizers();
            return leakedResources.ToArray();
        }

        public void Reset()
        {
            leakedResources.Clear();
        }
    }
}