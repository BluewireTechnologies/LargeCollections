using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using LargeCollections.Resources;
using LargeCollections.Storage.Database;

namespace LargeCollections.Collections
{
    public class DatabaseTableLargeCollection<T> : LargeCollectionWithBackingStore<T, DatabaseTableReference<T>>
    {
        private readonly SqlConnection connection;

        public DatabaseTableLargeCollection(SqlConnection connection, DatabaseTableReference<T> reference, long itemCount) : base(reference, itemCount)
        {
            this.connection = connection;
        }

        private IEnumerator<T> GetSimpleEnumeratorImplementation(string fieldName)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = String.Format("SELECT [{0}] FROM [{1}]", fieldName, BackingStore.TableName);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return (T)reader[0];
                    }
                }
            }
        }

        private IEnumerator<T> GetMultiColumnEnumeratorImplementation(NameValueObjectFactory<string,T> factory)
        {
            using (var command = connection.CreateCommand())
            {

                command.CommandType = CommandType.Text;
                command.CommandText = String.Format("SELECT [{0}] FROM [{1}]", String.Join("], [", factory.RequiredNames.ToArray()), BackingStore.TableName);

                using (var reader = command.ExecuteReader())
                {
                    using (var tableReader = new TableReader<T>(reader, factory))
                    {
                        while (tableReader.MoveNext())
                        {
                            yield return tableReader.Current;
                        }
                    }
                }
            }
        }

        protected override IEnumerator<T> GetEnumeratorImplementation()
        {
            var factory = BackingStore.Schema.GetRecordFactory();
            if (factory.RequiredNames.Count() == 1) return GetSimpleEnumeratorImplementation(factory.RequiredNames.Single());

            return GetMultiColumnEnumeratorImplementation(factory);
        }
    }
}
