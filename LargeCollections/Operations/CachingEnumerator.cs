using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LargeCollections.Operations
{
    public class CachingEnumerator<T> : IEnumerator<T>, IHasUnderlying
    {
        private readonly IEnumerator<T> underlying;
        public CachingEnumerator(IEnumerator<T> underlying)
        {
            this.underlying = underlying;
        }

        public T Current { get; private set; }

        public void Dispose()
        {
            underlying.Dispose();
        }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            if(underlying.MoveNext())
            {
                Current = underlying.Current;
                return true;
            }
            Current = default(T);
            return false;
        }

        public void Reset()
        {
            underlying.Reset();
        }

        public object Underlying
        {
            get { return underlying; }
        }
    }
}
