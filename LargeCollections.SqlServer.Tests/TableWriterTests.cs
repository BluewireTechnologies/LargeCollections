using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LargeCollections.SqlServer.Tests.IntegrationTesting;
using NUnit.Framework;

namespace LargeCollections.SqlServer.Tests
{
    [TestFixture]
    public class TableWriterTests
    {
        private readonly DatabaseTableSchema<int> schema = new DatabaseTableSchema<int> { { "value", i => i, SqlDbType.Int } };

        [Test]
        public async Task CanInsert10KItemsInto100TempTables()
        {
            var connectionString = TestDatabase.LocalTempDb.Get();
            var semaphore = new SemaphoreSlim(10);

            await Task.WhenAll(Enumerable.Range(0, 100).Select(_ => StartWriter()));

            async Task StartWriter()
            {
                await semaphore.WaitAsync();
                try
                {
                    await WriteItemsToTempTable(connectionString, 10000);
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        private async Task WriteItemsToTempTable(string connectionString, int count)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    var session = new SqlSession(connection, transaction);
                    var tableReference = new TemporaryDatabaseTableReference<int>(session, schema, $"#temp_{Guid.NewGuid():N}");
                    using (tableReference.Acquire())
                    {
                        using (var writer = tableReference.Create())
                        {
                            await writer.WriteAsync(Enumerable.Range(0, count));
                            await writer.CompleteAsync();
                        }
                    }
                }
            }
        }
    }
}
