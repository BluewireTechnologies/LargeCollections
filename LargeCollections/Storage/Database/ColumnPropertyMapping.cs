using System;
using System.Data;

namespace LargeCollections.Storage.Database
{
    public class ColumnPropertyMapping<T,TProp> : IColumnPropertyMapping<T>
    {
        private readonly Func<T, TProp> get;

        public ColumnPropertyMapping(string name, Func<T, TProp> get, SqlDbType type, int? width = null)
        {
            this.get = get;
            Name = name;
            DbType = type;
            Width = width;
        }

        public object Get(T source)
        {
            return get(source);
        }

        public string Name { get; private set; }
        public SqlDbType DbType { get; private set; }
        public int? Width { get; private set; }
        public Type Type { get { return typeof(TProp); } }
    }
}