using System;
using System.Collections;
using System.Collections.Generic;

namespace LargeCollections.Operations
{
    public class SortedEnumerable<T> : IEnumerable<T>, ISortedCollection<T>, IDisposable, IHasUnderlying<IEnumerable>
    {
        private IEnumerable<T> enumerable;

        public SortedEnumerable(IEnumerable<T> enumerable, IComparer<T> order)
        {
            this.enumerable = enumerable;
            SortOrder = order;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new SortedEnumerator<T>(enumerable.GetEnumerator(), SortOrder);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IComparer<T> SortOrder { get; private set; }

        public void Dispose()
        {
            if (enumerable is IDisposable) ((IDisposable) enumerable).Dispose();
        }

        public IEnumerable Underlying
        {
            get { return enumerable; }
        }
    }

    public class SortedEnumerator<T> : IEnumerator<T>, ISortedCollection<T>, IHasUnderlying<IEnumerator>
    {
        private readonly IEnumerator<T> enumerator;

        public SortedEnumerator(IEnumerator<T> enumerator, IComparer<T> order)
        {
            this.enumerator = enumerator;
            SortOrder = order;
        }


        public T Current
        {
            get { return enumerator.Current; }
        }

        public void Dispose()
        {
            enumerator.Dispose();
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            return enumerator.MoveNext();
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        public IComparer<T> SortOrder { get; private set; }

        public IEnumerator Underlying
        {
            get { return enumerator; }
        }
    }
}