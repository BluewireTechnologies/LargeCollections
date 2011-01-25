using System.Collections;

namespace LargeCollections
{
    public interface IHasUnderlying
    {
        object Underlying { get; }
    }
}