using System;
using System.Data;

namespace LargeCollections.Storage.Database
{
    public interface INamePropertyMapping<in T>
    {
        object Get(T source);
        void Set(T target, object value);
        bool CanDeserialise { get; }
        string Name { get; }
        string PropertyName { get; }
        Type Type { get; }
    }

    public interface IColumnPropertyMapping<in T> : INamePropertyMapping<T>
    {
        SqlDbType DbType { get; }
        int? Width { get; }
    }
}