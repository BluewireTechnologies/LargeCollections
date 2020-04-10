﻿using System;
using System.Collections.Generic;

namespace LargeCollections.Resources.Diagnostics
{
    public interface ILeakedResourceCounter
    {
        int CountLeaks();
        IEnumerable<IReferenceCountedResource> GetLeaks();
        void Leaked(IReferenceCountedResource resource);

        string CaptureTrace();
        void Reset();
        void OnFinalizerCrash(Exception exception, ReferenceCountedResource resource, IEnumerable<ITracedReference> references);
    }
}