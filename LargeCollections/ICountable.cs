namespace LargeCollections
{
    public interface ICountable
    {
        /// <summary>
        /// Total number of items in the collection.
        /// </summary>
        long Count { get; }
    }
}