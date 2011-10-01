using System;
using System.ComponentModel;
using System.Data;
using System.Linq.Expressions;

namespace LargeCollections.Storage.Database
{
    public class PropertyMappingBase<T, TProp>
    {
        public PropertyMappingBase(Expression<Func<T, TProp>> get, Action<T, TProp> set)
        {
            this.get = get.Compile();
            this.set = set;
            var member = Utils.GetReferencedMemberOrNull(get);
            if (member != null)
            {
                PropertyName = member.Name;
                if (this.set == null)
                {
                    this.set = Utils.TryCreateSetter<T, TProp>(member);
                }
            }
        }

        private Func<T, TProp> get;
        private Action<T, TProp> set;

        public bool CanDeserialise
        {
            get { return set != null; }
        }

        public Type Type { get { return typeof(TProp); } }
        public string PropertyName { get; private set; }

        public object Get(T source)
        {
            return get(source);
        }

        public void Set(T target, object value)
        {
            if (set == null) throw new ReadOnlyException(String.Format("Property mapping for '{0}' is read-only.", PropertyName));
            set(target, (TProp)value);
        }
    }
}