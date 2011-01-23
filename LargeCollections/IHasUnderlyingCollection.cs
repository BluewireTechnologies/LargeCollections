using System.Collections;

namespace LargeCollections
{
    public interface IHasUnderlyingCollection
    {
        IEnumerable UnderlyingCollection { get; }
    }
}