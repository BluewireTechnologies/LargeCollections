using System.Collections;

namespace LargeCollections
{
    public interface IHasUnderlying<T>
    {
        T Underlying { get; }
    }
}