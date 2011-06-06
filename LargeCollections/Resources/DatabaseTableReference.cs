using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace LargeCollections.Resources
{
    public class DatabaseTableReference : ReferenceCountedResource
    {
        public SqlConnection Connection { get; private set; }
        public string TableName { get; private set; }

        public DatabaseTableReference(SqlConnection connection, string tableName)
        {
            this.Connection = connection;
            TableName = tableName;
        }

        public string FieldName { get { return "value"; } }

        public virtual bool Exists()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return String.Format("{{{0} refs: {1}}}", RefCount, TableName);
        }
    }

    public class TemporaryDatabaseTableReference : DatabaseTableReference
    {
        public TemporaryDatabaseTableReference(SqlConnection connection)
            : base(connection, "##temp_" + Guid.NewGuid().ToString("N"))
        {
        }

        private bool exists;
        public void Create(SqlDbType columnType)
        {
            var createSql = String.Format("CREATE TABLE [{0}]([{1}] [{2}])", TableName, FieldName, columnType == SqlDbType.Variant ? "sql_variant" : columnType.ToString());
            using (var command = Connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = createSql;
                command.ExecuteNonQuery();
                exists = true;
            }
        }

        public void ApplyIndex()
        {
            if(!exists) throw new InvalidOperationException("Cannot create index. Table has not yet been created.");
            var createSql = String.Format("CREATE NONCLUSTERED INDEX [idx_{0}] ON [{1}]([{2}])", TableName.Substring(1), TableName, FieldName);
            using (var command = Connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = createSql;
                command.ExecuteNonQuery();
            }
        }

        public override bool Exists()
        {
            return exists;
        }

        protected override void CleanUp()
        {
            if (exists)
            {
                using (var command = Connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = String.Format("DROP TABLE [{0}]", TableName);
                    command.ExecuteNonQuery();
                    exists = false;
                }
            }
        }
    }

}
