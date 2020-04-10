using System;

namespace LargeCollections.Core
{
    public interface IOperatorCache
    {
        T GetInstance<T>(Func<T> create);
    }
}