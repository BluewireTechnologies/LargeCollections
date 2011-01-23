using System.Collections.Generic;

namespace LargeCollections
{
    /// <summary>
    /// An enumerator interface for transient collections.
    /// </summary>
    /// <remarks>
    /// Implementations of this interface must release their resources when MoveNext() returns false.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public interface ISinglePassCollection<T> : IEnumerator<T>, ICountable
    {
    }
}