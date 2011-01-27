using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LargeCollections.Collections;

namespace LargeCollections
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
