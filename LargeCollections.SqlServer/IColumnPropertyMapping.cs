using System;
using System.Data;

namespace LargeCollections.SqlServer
{
    public interface IPropertyMapping
    {
        string PropertyName { get; }
        Type Type { get; }
    }

    public interface IPropertyMapping<out TSelector, in TEntity> : IPropertyMapping
    {
        object Get(TEntity source);
        void Set(TEntity target, object value);
        bool CanDeserialise { get; }
        TSelector Name { get; }
    }

    public interface INamePropertyMapping<in TEntity> : IPropertyMapping<string, TEntity>
    {
    }

    public interface IColumnPropertyMapping<in T> : INamePropertyMapping<T>
    {
        SqlDbType DbType { get; }
        int? Width { get; }
    }
}