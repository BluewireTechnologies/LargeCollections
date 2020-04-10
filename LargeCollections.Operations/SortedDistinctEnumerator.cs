using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using LargeCollections.Core;

namespace LargeCollections.Operations
{
    public class SortedDistinctEnumerator<T> : IEnumerator<T>, ISorted<T>
    {
        private readonly IEnumerator<T> enumerator;

        public SortedDistinctEnumerator(IEnumerator<T> enumerator)
        {
            if (!(enumerator is ISorted<T>))
            {
                Debug.Fail("Underlying enumerator must be sorted in order for SortedDistinctEnumerator to operate");
                throw new InvalidOperationException("Underlying enumerator must be sorted in order for SortedDistinctEnumerator to operate");
            }
            this.enumerator = enumerator;
            SortOrder = ((ISorted<T>) enumerator).SortOrder;
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

        public IComparer<T> SortOrder { get; private set; }
    }
}