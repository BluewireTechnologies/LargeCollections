using System;
using System.Collections.Generic;

namespace LargeCollections
{
    public interface ICountable
    {
        /// <summary>
        /// Total number of items in the collection.
        /// </summary>
        long Count { get; }
    }

    public interface ISortedCollection<T>
    {
        IComparer<T> SortOrder { get; }
    }
}