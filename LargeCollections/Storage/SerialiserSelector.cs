using System;
using System.Collections.Generic;

namespace LargeCollections.Storage
{
    class SerialiserSelector
    {
        public SerialiserSelector()
        {
            Add(new GuidSerialiser());
        }

        private Dictionary<Type, object> serialisers = new Dictionary<Type, object>();
        private void Add<TItem>(IItemSerialiser<TItem> instance)
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
    }
}