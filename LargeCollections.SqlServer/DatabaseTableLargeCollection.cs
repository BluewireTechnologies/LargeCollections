using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using LargeCollections.Core.Collections;

namespace LargeCollections.SqlServer
{
    public class DatabaseTableLargeCollection<T> : LargeCollectionWithBackingStore<T, DatabaseTableReference<T>>
    {
        private readonly SqlSession session;

        private readonly bool readTableUsingCasts;

        public DatabaseTableLargeCollection(SqlSession session, DatabaseTableReference<T> reference, long itemCount) : base(reference, itemCount)
        {
            readTableUsingCasts = ShouldUseCastsForReadingSingleColumn(reference.Schema);
            this.session = session;
        }

        private static bool ShouldUseCastsForReadingSingleColumn(IDatabaseTableSchema<T> schema)
        {
            var factory = schema.GetRecordFactory();
            if(!factory.IsValid)
            {
                // A collection backed by a single-column table can try to cast instead of using a record factory, if
                // the field mapping doesn't define deserialisation.
                if (schema.Count() == 1 && !schema.Single().CanDeserialise) return true;

                factory.Validate();
            }
            return false;
        }

        private IEnumerator<T> GetSimpleEnumeratorImplementation(string fieldName)
        {
            using (var command = session.CreateCommand())
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
            using (var command = session.CreateCommand())
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
            if (readTableUsingCasts)
            {
                return GetSimpleEnumeratorImplementation(BackingStore.Schema.Single().Name);
            }
            var factory = BackingStore.Schema.GetRecordFactory();
            
            return GetMultiColumnEnumeratorImplementation(factory);
        }
    }
}
