using System.Collections;
using System.Collections.Generic;

namespace LargeCollections
{
    public class SortedDistinctEnumerator<T> : IEnumerator<T>
    {
        private readonly IEnumerator<T> enumerator;

        public SortedDistinctEnumerator(IEnumerator<T> enumerator)
        {
            this.enumerator = enumerator;
        }

        public void Dispose()
        {
            enumerator.Dispose();
        }


        public bool MoveNext()
        {
            var current = Current;
            while (enumerator.MoveNext())
            {
                Current = enumerator.Current;
                if (!Equals(Current, current))
                {
                    return true;
                }
            }
            return false;
        }

        public void Reset()
        {
            enumerator.Reset();
            Current = enumerator.Current;
        }

        public T Current { get; private set; }

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}