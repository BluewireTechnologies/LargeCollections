using System;
using System.Collections.Generic;
using System.Linq;

namespace Bluewire.ReferenceCounting
{
    public class DisposableList<T> : List<T>, IDisposable
    {
        public DisposableList()
        {
        }

        public DisposableList(IEnumerable<T> items) : base(items)
        {
        }

        public void Dispose()
        {
            foreach (var item in this.OfType<IDisposable>())
            {
                try
                {
                    item.Dispose();
                }
                catch
                {
                    // clean up as much as possible.
                }
            }
        }

        public static implicit operator T[](DisposableList<T> list)
        {
            return list.ToArray();
        }
    }
}
