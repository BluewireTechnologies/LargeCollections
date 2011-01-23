using System.Collections.Generic;

namespace LargeCollections
{
    public class InMemoryLargeCollection<T> : ILargeCollection<T>
    {
        private ICollection<T> @internal;

        public InMemoryLargeCollection(List<T> contents)
        {
            Count = contents.Count;
            var array = new T[Count];
            contents.CopyTo(array);
            @internal = array;
            
        }

        public IEnumerator<T> GetEnumerator()
        {
            return @internal.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return @internal.GetEnumerator();
        }

        public void Dispose()
        {
            if(@internal != null)
            {
                @internal = null;
            }
        }

        public long Count { get; private set; }
    }
}
