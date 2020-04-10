using System;
using System.Data;
using System.Linq.Expressions;

namespace LargeCollections.SqlServer
{
    public class ColumnPropertyMapping<T,TProp> : PropertyMappingBase<T, TProp>, IColumnPropertyMapping<T>
    {
        public ColumnPropertyMapping(string name, Expression<Func<T, TProp>> get, SqlDbType type, int? width = null) : this(name, get, null, type, width)
        {
        }

        public ColumnPropertyMapping(string name, Expression<Func<T, TProp>> get, Action<T, TProp> set, SqlDbType type, int? width = null) : base(get, set)
        {
            Name = name;
            DbType = type;
            Width = width;
        }

        public string Name { get; private set; }
        public SqlDbType DbType { get; private set; }
        public int? Width { get; private set; }
    }
}