using System;
using System.Data;

namespace LargeCollections.SqlServer
{
    public class TemporaryDatabaseTableReference<T> : DatabaseTableReference<T>
    {
        public SqlSession Session { get; private set; }

        public TemporaryDatabaseTableReference(SqlSession session, DatabaseTableSchema<T> schema)
            : base(schema, "##temp_" + Guid.NewGuid().ToString("N"))
        {
            Session = session;
        }

        public TemporaryDatabaseTableReference(SqlSession session, DatabaseTableSchema<T> schema, string tableName)
            : base(schema, ValidateTableName(tableName))
        {
            Session = session;
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
            Schema.CreateTable(Session, TableName);
            var writer = Schema.GetWriter(Session, TableName);
            exists = true;
            return writer;
        }

        public void ApplyIndex(string columnName)
        {
            if (!exists) throw new InvalidOperationException("Cannot create index. Table has not yet been created.");
            Schema.AddIndex(Session, TableName, columnName);
        }

        public override bool Exists()
        {
            return exists;
        }

        protected override void CleanUp()
        {
            if (exists)
            {
                using (var command = Session.CreateCommand())
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
