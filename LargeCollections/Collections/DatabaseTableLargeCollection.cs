using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using LargeCollections.Resources;

namespace LargeCollections.Collections
{
    public class DatabaseTableLargeCollection<T> : LargeCollectionWithBackingStore<T, DatabaseTableReference<T>>
    {
        private readonly SqlConnection connection;

        public DatabaseTableLargeCollection(SqlConnection connection, DatabaseTableReference<T> reference, long itemCount) : base(reference, itemCount)
        {
            this.connection = connection;
        }

        protected override IEnumerator<T> GetEnumeratorImplementation()
        {
            using (var command = connection.CreateCommand())
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
