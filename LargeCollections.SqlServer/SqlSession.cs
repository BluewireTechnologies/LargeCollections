using System.Data.SqlClient;

namespace LargeCollections.SqlServer
{
    /// <summary>
    /// Encapsulates a database connection and an optional local transaction.
    /// </summary>
    public class SqlSession
    {
        public SqlConnection Connection { get; }
        public SqlTransaction Transaction { get; }

        public SqlSession(SqlConnection connection, SqlTransaction transaction = null)
        {
            Connection = connection;
            Transaction = transaction;
        }

        public SqlCommand CreateCommand()
        {
            var command = Connection.CreateCommand();
            if (Transaction != null) command.Transaction = Transaction;
            return command;
        }
    }
}
