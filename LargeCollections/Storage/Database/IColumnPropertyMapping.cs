using System;
using System.ComponentModel;
using System.Data;

namespace LargeCollections.Storage.Database
{
    public interface IPropertyMapping<out TSelector, in TEntity>
    {
        object Get(TEntity source);
        void Set(TEntity target, object value);
        bool CanDeserialise { get; }
        TSelector Name { get; }
        string PropertyName { get; }
        Type Type { get; }
        TypeConverter TypeConverter { get; }
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