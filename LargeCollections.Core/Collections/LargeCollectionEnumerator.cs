﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace LargeCollections.Core.Collections
{
    public class LargeCollectionEnumerator<T> : IEnumerator<T>, ICounted, IHasUnderlying
    {
        private readonly ILargeCollection<T> collection;
        private IEnumerator<T> enumerator;
        private IDisposable resource;

        public LargeCollectionEnumerator(ILargeCollection<T> collection, IEnumerator<T> underlyingEnumerator)
        {
            this.collection = collection;
            enumerator = underlyingEnumerator;
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

        public object Underlying
        {
            get { return collection; }
        }
    }
}
