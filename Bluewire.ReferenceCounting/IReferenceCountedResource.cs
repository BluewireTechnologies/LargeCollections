using System;

namespace Bluewire.ReferenceCounting
{
    public interface IReferenceCountedResource
    {
        int RefCount { get; }
        IDisposable Acquire();
    }
}