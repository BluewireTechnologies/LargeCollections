using System;
using Bluewire.ReferenceCounting;

namespace LargeCollections.SqlServer
{
    public class DatabaseTableReference<T> : ReferenceCountedResource
    {
        public string TableName { get; private set; }

        public DatabaseTableReference(DatabaseTableSchema<T> schema, string tableName)
        {
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
