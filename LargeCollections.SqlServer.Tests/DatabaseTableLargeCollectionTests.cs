using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Bluewire.ReferenceCounting.Tests;
using LargeCollections.Core;
using LargeCollections.SqlServer.Tests.IntegrationTesting;
using LargeCollections.Tests.Collections;
using NUnit.Framework;

namespace LargeCollections.SqlServer.Tests
{
    [TestFixture, CheckResources]
    public class DatabaseTableLargeCollectionTests : BaselineTestsForLargeCollectionWithBackingStore<DatabaseTableReference<int>>
    {
        class Harness : LargeCollectionTestHarness<DatabaseTableReference<int>>
        {
            private readonly SqlConnection connection;

            public Harness()
            {
                var connectionString = TestDatabase.LocalTempDb.Get();
                connection = new SqlConnection(connectionString);
                connection.Open();
            }

            public override void Dispose()
            {
                connection.Close();
                base.Dispose();
            }

            public override IAccumulator<int> GetAccumulator()
            {
                return new DatabaseTableAccumulator<int>(connection, SqlDbType.Int);
            }

            public override bool BackingStoreExists(IAccumulator<int> accumulator)
            {
                return BackingStoreExistsOrIsNotApplicable(accumulator);
            }


            protected bool BackingStoreExistsOrIsNotApplicable(object obj)
            {
                var backingStoreOwner = obj.GetUnderlying<IHasBackingStore<DatabaseTableReference<int>>>();
                if (backingStoreOwner == null) return false; //empty collections become InMemoryLargeCollections

                return backingStoreOwner.BackingStore.Exists();
            }

            public override bool BackingStoreExists(ILargeCollection<int> collection)
            {
                return BackingStoreExistsOrIsNotApplicable(collection);
            }
        }

        protected override LargeCollectionTestHarness<DatabaseTableReference<int>> CreateHarness()
        {
            return new Harness();
        }
    }
}
