namespace LargeCollections
{
    public interface IHasBackingStore<out T>
    {
        T BackingStore { get; }
    }
}