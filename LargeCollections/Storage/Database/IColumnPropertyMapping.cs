using System;
using System.Data;

namespace LargeCollections.Storage.Database
{
    public interface IColumnPropertyMapping<T>
    {
        object Get(T source);
        string Name { get; }
        SqlDbType DbType { get; }
        int? Width { get; }
        Type Type { get; }
    }
}