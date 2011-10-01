﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace LargeCollections.Storage.Database
{
    public class NameValueObjectFactory<TSelector, TEntity> : NameValueObjectFactory<TSelector, TEntity, IPropertyMapping<TSelector, TEntity>>
    {
        public NameValueObjectFactory(IEnumerable<IPropertyMapping<TSelector, TEntity>> columns)
            : base(columns)
        {
        }
    }

    public class NameValueObjectFactory<TSelector, TEntity, TMapping> where TMapping : IPropertyMapping<TSelector, TEntity>
    {
        private Factory factory;

        public NameValueObjectFactory(IEnumerable<TMapping> columns)
        {
            var columnsByProperty = columns.Where(c => c.PropertyName != null).ToDictionary(c => c.PropertyName, StringComparer.InvariantCultureIgnoreCase);
            this.factory = typeof(TEntity).GetConstructors().Select(c => TryCreateFactory(c, columnsByProperty)).FirstOrDefault();
            if(factory == null) throw new InvalidOperationException("Could not generate a deserialiser for type '{0}'. No viable constructor could be found.");
            
        }

        class Factory
        {
            private readonly ConstructorInfo constructorInfo;
            private readonly TMapping[] constructorParams;
            private readonly TMapping[] remainingColumns;

            public Factory(ConstructorInfo constructorInfo, TMapping[] constructorParams, TMapping[] remainingColumns)
            {
                this.constructorInfo = constructorInfo;
                this.constructorParams = constructorParams;
                this.remainingColumns = remainingColumns;
            }

            public TEntity Create(Action<TMapping, Action<object>> copy, Func<TMapping, object> get)
            {
                var instance = (TEntity)constructorInfo.Invoke(constructorParams.Select(get).ToArray());
                foreach (var column in remainingColumns)
                {
                    copy(column, v => column.Set(instance, v));
                }
                return instance;
            }

            public IEnumerable<TSelector> RequiredNames
            {
                get { return constructorParams.Concat(remainingColumns).Select(c => c.Name); }
            }
        }

        private static Factory TryCreateFactory(ConstructorInfo constructorInfo, Dictionary<string, TMapping> columns)
        {
            var constructorParamColumns = TryGetConstructorParameterListColumns(constructorInfo, columns);
            if (constructorParamColumns == null) return null;

            var remainingColumns = columns.Values.Except(constructorParamColumns).Where(c => c.CanDeserialise).ToArray();

            return new Factory(constructorInfo, constructorParamColumns, remainingColumns);
        }

        private static TMapping[] TryGetConstructorParameterListColumns(ConstructorInfo constructorInfo, Dictionary<string, TMapping> columns)
        {
            var parameters = constructorInfo.GetParameters();
            var paramColumnNames = new List<TMapping>();
            foreach(var parameter in parameters)
            {
                TMapping mapping;
                if(!columns.TryGetValue(parameter.Name, out mapping)) return null;
                if(mapping.Type != parameter.ParameterType) return null;
                paramColumnNames.Add(mapping);
            }
            return paramColumnNames.ToArray();
        }


        public TEntity ReadRecord(Action<TMapping, Action<object>> copy)
        {
            return factory.Create(copy, GetterFromCopier(copy));
        }


        public TEntity ReadRecord(Action<TMapping, Action<object>> copy, Func<TMapping, object> get)
        {
            return factory.Create(copy, get);
        }


        private static readonly object UNSET = new object();
        private static Func<TMapping, object> GetterFromCopier(Action<TMapping, Action<object>> copier)
        {
            return c =>
                {
                    object value = UNSET;
                    copier(c, v => value = v);
                    if(ReferenceEquals(value, UNSET)) throw new InvalidOperationException(String.Format("Unable to read value for required property {0}", c.PropertyName));
                    return value;
                };
        }


        public TEntity ReadRecord(Func<TMapping, object> get)
        {
            return factory.Create((m, set) => set(get(m)), get);
        }

        public IEnumerable<TSelector> RequiredNames
        {
            get { return factory.RequiredNames; }
        }
    }
}
