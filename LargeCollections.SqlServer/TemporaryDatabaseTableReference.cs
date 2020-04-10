using System;
using System.Data;
using System.Data.SqlClient;

namespace LargeCollections.SqlServer
{
    public class TemporaryDatabaseTableReference<T> : DatabaseTableReference<T>
    {
        public SqlConnection Connection { get; private set; }

        public TemporaryDatabaseTableReference(SqlConnection connection, DatabaseTableSchema<T> schema)
            : base(schema, "##temp_" + Guid.NewGuid().ToString("N"))
        {
            Connection = connection;
        }

        public TemporaryDatabaseTableReference(SqlConnection connection, DatabaseTableSchema<T> schema, string tableName)
            : base(schema, ValidateTableName(tableName))
        {
            Connection = connection;
        }

        private static string ValidateTableName(string specifiedTableName)
        {
            if (String.IsNullOrWhiteSpace(specifiedTableName)) throw new ArgumentException("No table name specified.");
            if (!specifiedTableName.StartsWith("#")) throw new ArgumentException(String.Format("Not a valid temporary table name: {0}", specifiedTableName));
            return specifiedTableName;
        }

        private bool exists;
        public TableWriter<T> Create()
        {
            Schema.CreateTable(Connection, TableName);
            var writer = Schema.GetWriter(Connection, TableName);
            exists = true;
            return writer;
        }

        public void ApplyIndex(string columnName)
        {
            if(!exists) throw new InvalidOperationException("Cannot create index. Table has not yet been created.");
            Schema.AddIndex(Connection, TableName, columnName);
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
