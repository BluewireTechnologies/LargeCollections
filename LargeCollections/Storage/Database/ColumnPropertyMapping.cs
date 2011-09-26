using System;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace LargeCollections.Storage.Database
{
    public class ColumnPropertyMapping<T,TProp> : IColumnPropertyMapping<T>
    {
        private readonly Func<T, TProp> get;
        private readonly Action<T, TProp> set;

        public ColumnPropertyMapping(string name, Expression<Func<T, TProp>> get, SqlDbType type, int? width = null) : this(name, get, null, type, width)
        {
        }

        public ColumnPropertyMapping(string name, Expression<Func<T, TProp>> get, Action<T, TProp> set, SqlDbType type, int? width = null)
        {
            this.get = get.Compile();
            this.set = set;
            var member = Utils.GetReferencedMemberOrNull(get);
            if(member != null)
            {
                PropertyName = member.Name;
                if(this.set == null)
                {
                    this.set = Utils.TryCreateSetter<T, TProp>(member);
                }
            }
            Name = name;
            DbType = type;
            Width = width;
        }

        public object Get(T source)
        {
            return get(source);
        }

        public void Set(T target, object value)
        {
            if(set == null) throw new ReadOnlyException("Property mapping for column 'name' is read-only.");
            set(target, (TProp)value);
        }

        public bool CanDeserialise
        {
            get { return set != null; }
        }

        public string Name { get; private set; }
        public SqlDbType DbType { get; private set; }
        public int? Width { get; private set; }
        public Type Type { get { return typeof(TProp); } }


        public string PropertyName {get; private set;}
    }
}