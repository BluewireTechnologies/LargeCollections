﻿using System;
using System.Data;
using System.Linq.Expressions;

namespace LargeCollections.SqlServer
{
    public class PropertyMappingBase<T, TProp>
    {
        public PropertyMappingBase(Expression<Func<T, TProp>> get, Action<T, TProp> set, string propertyName = null)
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
            if (propertyName != null) PropertyName = propertyName;
        }

        private readonly Func<T, TProp> get;
        private readonly Action<T, TProp> set;

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

            set(target, HandleCastExceptions(value));
        }

        private TProp HandleCastExceptions(object value)
        {
            try
            {
                return (TProp)value;
            }
            catch (Exception ex)
            {
                var typeOfValue = ReferenceEquals(value, null) ? "<null>" : String.Format("object of type {0}", value.GetType());
                throw new InvalidCastException(String.Format("Cannot cast {0} to {1}", typeOfValue, Type.Name), ex);
            }
        }
    }
}
