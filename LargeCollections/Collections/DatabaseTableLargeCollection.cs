using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using LargeCollections.Resources;

namespace LargeCollections.Collections
{
    public class DatabaseTableLargeCollection<T> : LargeCollectionWithBackingStore<T, DatabaseTableReference<T>>
    {
        public DatabaseTableLargeCollection(DatabaseTableReference<T> reference, long itemCount) : base(reference, itemCount)
        {
        }

        protected override IEnumerator<T> GetEnumeratorImplementation()
        {
            using (var command = BackingStore.Connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = String.Format("SELECT [{0}] FROM [{1}]", BackingStore.Schema.Single().Name, BackingStore.TableName);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return (T)reader[0];
                    }
                }
            }
        }
    }
}
