using System;
using System.Collections.Generic;
using LargeCollections.Resources;

namespace LargeCollections.Operations
{
    public class ConcatenatedEnumerator<T> : IEnumerator<T>
    {
        private readonly DisposableList<IEnumerator<T>> enumerators;
        private readonly IEnumerator<T> enumeratorEnumerator;

        public ConcatenatedEnumerator(params IEnumerator<T>[] enumerators)
        {
            this.enumerators = new DisposableList<IEnumerator<T>>(enumerators);
            enumeratorEnumerator = InternalEnumerator();
        }

        private IEnumerator<T> InternalEnumerator()
        {
            foreach(var enumerator in enumerators)
            {
                while(enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
            }
        }

        public T Current
        {
            get { return enumeratorEnumerator.Current; }
        }

        public void Dispose()
        {
            enumerators.Dispose();
        }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            return enumeratorEnumerator.MoveNext();
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }
    }
}