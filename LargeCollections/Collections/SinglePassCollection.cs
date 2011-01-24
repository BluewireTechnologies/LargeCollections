using System;
using System.Collections;
using System.Collections.Generic;

namespace LargeCollections.Collections
{
    public class SinglePassCollection<T> : ISinglePassCollection<T>, IHasUnderlying<IEnumerable>
    {
        private readonly ILargeCollection<T> collection;
        private IEnumerator<T> enumerator;
        private IDisposable resource;

        public SinglePassCollection(ILargeCollection<T> collection)
        {
            this.collection = collection;
            enumerator = collection.GetEnumerator();
            resource = collection.Acquire();
        }

        public T Current
        {
            get { return enumerator.Current; }
        }

        public void Dispose()
        {
            if (enumerator != null)
            {
                enumerator.Dispose();
                resource.Dispose();
                enumerator = null;
            }
        }

        object IEnumerator.Current
        {
            get { return enumerator.Current; }
        }

        public bool MoveNext()
        {
            if (enumerator != null)
            {
                if (enumerator.MoveNext()) return true;
                Dispose();
            }
            return false;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        public long Count
        {
            get { return collection.Count; }
        }

        public IEnumerable Underlying
        {
            get { return collection; }
        }
    }
}