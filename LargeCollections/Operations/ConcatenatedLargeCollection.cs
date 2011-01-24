using System;
using System.Collections.Generic;
using System.Linq;
using LargeCollections.Linq;
using LargeCollections.Resources;

namespace LargeCollections.Operations
{
    public class ConcatenatedSinglePassCollection<T> : ISinglePassCollection<T>
    {
        private readonly DisposableList<ISinglePassCollection<T>> enumerators;
        private readonly IEnumerator<T> enumeratorEnumerator;

        public ConcatenatedSinglePassCollection(params ISinglePassCollection<T>[] enumerators)
        {
            this.enumerators = new DisposableList<ISinglePassCollection<T>>(enumerators);
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

        public long Count
        {
            get { return enumerators.Sum(e => e.Count); }
        }
    }

    public class ConcatenatedLargeCollection<T> : MultipleCollection<T>, ILargeCollection<T>
    {
        private readonly ILargeCollection<T>[] collections;
        
        public ConcatenatedLargeCollection(params ILargeCollection<T>[] collections) : base(collections)
        {
            this.collections = collections;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new ConcatenatedSinglePassCollection<T>(collections.Select(c => c.AsSinglePass()).ToArray());
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public long Count
        {
            get { return collections.Sum(c => c.Count); }
        }
    }
}
