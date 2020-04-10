using System;
using System.Data;
using System.Data.SqlClient;

namespace LargeCollections.SqlServer
{
    public static class DatabaseTable
    {
        public static void Delete(SqlConnection cn, string tableName)
        {
            using (var command = cn.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = String.Format("DROP TABLE [{0}]", tableName);
                command.ExecuteNonQuery();
            }
        }

        public static bool Exists(SqlConnection cn, string tableName)
        {
            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = "SELECT Count(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName";
                cmd.Parameters.AddWithValue("tableName", tableName);
                cmd.CommandType = CommandType.Text;
                return ((int)cmd.ExecuteScalar()) > 0;
            }
        }
    }
}