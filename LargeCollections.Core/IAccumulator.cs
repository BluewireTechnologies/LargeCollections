using System;

namespace LargeCollections.Core
{
    public interface IAccumulator<T> :  IAppendable<T>, IDisposable
    {
        ILargeCollection<T> Complete();
    }
}