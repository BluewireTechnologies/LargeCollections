using System;
using System.Collections.Generic;

namespace LargeCollections
{
    public interface ICounted
    {
        /// <summary>
        /// Total number of items in the collection.
        /// </summary>
        long Count { get; }
    }

    public interface IMappedCount
    {
        long MapCount(long sourceCount);
    }

    public interface ISorted<T>
    {
        IComparer<T> SortOrder { get; }
    }
}