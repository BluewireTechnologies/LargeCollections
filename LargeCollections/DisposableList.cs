using System;
using System.Collections.Generic;

namespace LargeCollections
{
    public class DisposableList<T> : List<T>, IDisposable where T : IDisposable
    {
        public DisposableList()
        {
        }

        public DisposableList(IEnumerable<T> items) : base(items)
        {
        }

        public void Dispose()
        {
            foreach (var item in this)
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
    }
}