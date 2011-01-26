using System.Collections.Generic;
using System.Linq;
using LargeCollections.Linq;

namespace LargeCollections.Operations
{
    public class ConcatenatedLargeCollection<T> : MultipleCollection<T>, ILargeCollection<T>
    {
        private readonly ILargeCollection<T>[] collections;
        
        public ConcatenatedLargeCollection(params ILargeCollection<T>[] collections) : base(collections)
        {
            this.collections = collections;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new ConcatenatedEnumerator<T>(collections.Select(c => c.GetEnumerator()).ToArray()).InheritsCount(collections);
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
