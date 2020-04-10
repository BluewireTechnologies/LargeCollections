using System;
using System.Collections.Generic;

namespace LargeCollections
{
    public interface IDisposableEnumerable<T> : IEnumerable<T>, IDisposable
    {
    }

    public interface ILargeCollection<T> : IDisposableEnumerable<T>, ICounted
    {
    }
}