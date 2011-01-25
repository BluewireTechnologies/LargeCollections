using System;
using System.Collections.Generic;

namespace LargeCollections
{
    public interface ILargeCollection<T> : IEnumerable<T>, IDisposable, ICounted
    {
    }
}