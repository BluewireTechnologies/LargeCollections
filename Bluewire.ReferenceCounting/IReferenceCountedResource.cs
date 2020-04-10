using System;
using System.Linq;

namespace LargeCollections.Resources
{
    public interface IReferenceCountedResource
    {
        int RefCount { get; }
        IDisposable Acquire();
    }
}