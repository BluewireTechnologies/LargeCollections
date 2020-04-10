using System;
using System.Collections;
using System.Collections.Generic;

namespace LargeCollections.Core.Storage
{
    public class SerialiserSelector : IEnumerable<Type>
    {
        public SerialiserSelector()
        {
            Add(new GuidSerialiser());
        }

        private Dictionary<Type, object> serialisers = new Dictionary<Type, object>();
        public void Add<TItem>(IItemSerialiser<TItem> instance)
        {
            serialisers.Add(typeof(TItem), instance);
        }

        public IItemSerialiser<T> Get<T>()
        {
            if(serialisers.ContainsKey(typeof(T)))
            {
                return (IItemSerialiser<T>)serialisers[typeof (T)];
            }
            return new DefaultItemSerialiser<T>();
        }

        public IEnumerator GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<Type> IEnumerable<Type>.GetEnumerator()
        {
            return serialisers.Keys.GetEnumerator();
        }
    }
}