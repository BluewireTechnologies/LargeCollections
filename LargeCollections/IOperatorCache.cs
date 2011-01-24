using System;

namespace LargeCollections
{
    public interface IOperatorCache
    {
        T GetInstance<T>(Func<T> create);
    }
}