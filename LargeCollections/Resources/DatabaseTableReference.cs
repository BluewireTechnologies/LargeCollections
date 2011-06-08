using System;
using System.Data.SqlClient;
using LargeCollections.Storage.Database;

namespace LargeCollections.Resources
{
    public class DatabaseTableReference<T> : ReferenceCountedResource
    {
        public SqlConnection Connection { get; private set; }
        public string TableName { get; private set; }

        public DatabaseTableReference(SqlConnection connection, DatabaseTableSchema<T> schema, string tableName)
        {
            this.Connection = connection;
            Schema = schema;
            TableName = tableName;
        }

        public IDatabaseTableSchema<T> Schema { get; private set; }
        
        public virtual bool Exists()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return String.Format("{{{0} refs: {1}}}", RefCount, TableName);
        }
    }
}
