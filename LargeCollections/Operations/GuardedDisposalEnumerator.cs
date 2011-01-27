using System;
using System.Collections.Generic;

namespace LargeCollections.Operations
{
    class GuardedDisposalEnumerator<T> : IEnumerator<T>, IHasUnderlying
    {
        private readonly IEnumerator<T> underlying;
        private readonly IDisposable guarded;

        public GuardedDisposalEnumerator(IEnumerator<T> underlying, IDisposable guarded)
        {
            this.underlying = underlying;
            this.guarded = guarded;
        }

        public T Current
        {
            get { return underlying.Current; }
        }

        public void Dispose()
        {
            underlying.Dispose();
            guarded.Dispose();
        }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            return underlying.MoveNext();
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