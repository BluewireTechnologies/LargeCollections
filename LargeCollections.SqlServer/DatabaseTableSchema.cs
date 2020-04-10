using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace LargeCollections.SqlServer
{
    public interface IDatabaseTableSchema<T> : IEnumerable<IColumnPropertyMapping<T>>
    {
        void CreateTable(SqlConnection cn, string tableName);
        void AddIndex(SqlConnection cn, string tableName, string columnName);

        string PrimaryKey { get; }
        TableWriter<T> GetWriter(SqlConnection cn, string tableName);

        NameValueObjectFactory<string, T> GetRecordFactory();
    }

    public class DatabaseTableSchema<T> : IDatabaseTableSchema<T>
    {
        public void Add<TProp>(string name, Expression<Func<T, TProp>> get, SqlDbType type, int? width = null)
        {
            Add(new ColumnPropertyMapping<T, TProp>(name, get, type, width));
        }

        public void PrimaryKey(string name)
        {
            PrimaryKey(name, true);
        }

        public void PrimaryKey(string name, bool clustered)
        {
            if (!properties.Any(p => p.Name == name)) throw new InvalidOperationException("Column does not exist: " + name);
            if (primaryKey != null) throw new InvalidOperationException("Primary key already defined");
            primaryKey = name;
            this.clustered = clustered;
        }


        public void Add(IColumnPropertyMapping<T> mapping)
        {
            if(properties.Any(p => p.Name == mapping.Name)) throw new InvalidOperationException("Property mapping already exists");
            properties.Add(mapping);
        }

        private readonly List<IColumnPropertyMapping<T>> properties = new List<IColumnPropertyMapping<T>>();
        private string primaryKey;
        private bool clustered;

        public void CreateTable(SqlConnection cn, string tableName)
        {
            var columns = properties.Select(FormatColumn).ToArray();
            var sql = String.Format("create table [{0}]({1});", tableName, String.Join(",\n", columns));
            using (var command = cn.CreateCommand())
            {
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
            }
        }

        private string FormatColumn(IColumnPropertyMapping<T> column)
        {
            var columnDefinition = String.Format("[{0}] {1}", column.Name, FormatType(column.DbType, column.Width));
            if (column.Name == primaryKey)
            {
                if (!clustered) columnDefinition += " NONCLUSTERED";
                columnDefinition += " PRIMARY KEY";
            }
            return columnDefinition;
        }

        public void AddIndex(SqlConnection cn, string tableName, string columnName)
        {
            if (!properties.Any(p => p.Name == columnName)) throw new InvalidOperationException("Column does not exist: " + columnName);
            var createSql = String.Format("CREATE NONCLUSTERED INDEX [idx_{0}_{2}] ON [{1}]([{2}])", tableName.TrimStart('#'), tableName, columnName);
            using (var command = cn.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = createSql;
                command.ExecuteNonQuery();
            }
        }

        string  IDatabaseTableSchema<T>.PrimaryKey
        {
            get { return primaryKey; }
        }

        private string FormatType(SqlDbType type, int? width)
        {
            var typeString = "[" + type.ToString() + "]";
            if (type == SqlDbType.Variant) typeString = "[sql_variant]";
            if (width.HasValue)
            {
                typeString += "(" + width + ")";
            }
            return typeString;
        }

        public IEnumerator<IColumnPropertyMapping<T>> GetEnumerator()
        {
            return properties.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return properties.GetEnumerator();
        }


        public TableWriter<T> GetWriter(SqlConnection cn, string tableName)
        {
            return new TableWriter<T>(cn, properties, tableName);
        }


        public NameValueObjectFactory<string, T> GetRecordFactory()
        {
            return new NameValueObjectFactory<string, T>(this);
        }
    }
}
