using LargeCollections.Core.Collections;

namespace LargeCollections.Core
{
    public class DummyAccumulatorSelector : IAccumulatorSelector
    {
        public IAccumulator<T> GetAccumulator<T>(long totalSizeOfCollection)
        {
            return new InMemoryAccumulator<T>();
        }

        public IAccumulator<T> GetAccumulator<T>()
        {
            return new InMemoryAccumulator<T>();
        }

        public IAccumulator<T> GetAccumulator<T>(System.Collections.IEnumerator source)
        {
            return new InMemoryAccumulator<T>();
        }
    }
}
