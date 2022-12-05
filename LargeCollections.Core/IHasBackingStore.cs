namespace LargeCollections.Core
{
    public interface IHasBackingStore<out T>
    {
        T BackingStore { get; }
    }
}
