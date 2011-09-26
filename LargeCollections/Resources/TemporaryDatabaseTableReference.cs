﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using LargeCollections.Storage.Database;

namespace LargeCollections.Resources
{
    public class TemporaryDatabaseTableReference<T> : DatabaseTableReference<T>
    {
        public SqlConnection Connection { get; private set; }

        public TemporaryDatabaseTableReference(SqlConnection connection, DatabaseTableSchema<T> schema)
            : base(schema, "##temp_" + Guid.NewGuid().ToString("N"))
        {
            Connection = connection;
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