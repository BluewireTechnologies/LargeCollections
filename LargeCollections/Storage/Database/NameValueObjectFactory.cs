using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace LargeCollections.Storage.Database
{
    public class NameValueObjectFactory<TEntity>
    {
        private Factory factory;

        public NameValueObjectFactory(IEnumerable<INamePropertyMapping<TEntity>> columns)
        {
            var columnsByProperty = columns.Where(c => c.PropertyName != null).ToDictionary(c => c.PropertyName, StringComparer.InvariantCultureIgnoreCase);
            this.factory = typeof(TEntity).GetConstructors().Select(c => TryCreateFactory(c, columnsByProperty)).FirstOrDefault();
            if(factory == null) throw new InvalidOperationException("Could not generate a deserialiser for type '{0}'. No viable constructor could be found.");
            
        }

        class Factory
        {
            private readonly ConstructorInfo constructorInfo;
            private readonly string[] constructorParamNames;
            private readonly INamePropertyMapping<TEntity>[] remainingColumns;

            public Factory(ConstructorInfo constructorInfo, string[] constructorParamNames, INamePropertyMapping<TEntity>[] remainingColumns)
            {
                this.constructorInfo = constructorInfo;
                this.constructorParamNames = constructorParamNames;
                this.remainingColumns = remainingColumns;
            }

            public TEntity Create(Func<string, object> get)
            {
                var instance = (TEntity)constructorInfo.Invoke(constructorParamNames.Select(get).ToArray());
                foreach (var column in remainingColumns)
                {
                    column.Set(instance, get(column.Name));
                }
                return instance;
            }

            public IEnumerable<string> RequiredNames
            {
                get { return constructorParamNames.Concat(remainingColumns.Select(c => c.Name)); }
            }
        }

        private static Factory TryCreateFactory(ConstructorInfo constructorInfo, Dictionary<string, INamePropertyMapping<TEntity>> columns)
        {
            var constructorParamColumnNames = TryGetConstructorParameterListColumns(constructorInfo, columns);
            if(constructorParamColumnNames == null) return null;

            var remainingColumns = columns.Values.Where(c => c.CanDeserialise && !constructorParamColumnNames.Contains(c.Name)).ToArray();

            return new Factory(constructorInfo, constructorParamColumnNames, remainingColumns);
        }

        private static string[] TryGetConstructorParameterListColumns(ConstructorInfo constructorInfo, Dictionary<string, INamePropertyMapping<TEntity>> columns)
        {
            var parameters = constructorInfo.GetParameters();
            var paramColumnNames = new List<string>();
            foreach(var parameter in parameters)
            {
                INamePropertyMapping<TEntity> mapping;
                if(!columns.TryGetValue(parameter.Name, out mapping)) return null;
                if(mapping.Type != parameter.ParameterType) return null;
                paramColumnNames.Add(mapping.Name);
            }
            return paramColumnNames.ToArray();
        }

        public TEntity ReadRecord(Func<string, object> record)
        {
            return factory.Create(record);
        }

        public IEnumerable<string> RequiredNames
        {
            get { return factory.RequiredNames; }
        }
    }
}
