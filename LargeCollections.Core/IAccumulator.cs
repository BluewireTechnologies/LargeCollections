using System;

namespace LargeCollections
{
    public interface IAccumulator<T> :  IAppendable<T>, IDisposable
    {
        ILargeCollection<T> Complete();
    }
}