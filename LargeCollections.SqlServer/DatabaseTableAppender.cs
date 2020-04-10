using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using LargeCollections.Core;

namespace LargeCollections.SqlServer
{
    public class DatabaseTableAppender<T> : IAppendable<T>, IDisposable, IHasBackingStore<DatabaseTableReference<T>>
    {
        protected TemporaryDatabaseTableReference<T> TableReference;
        private IDisposable reference;
        private bool completed;

        /// <summary>
        /// Create a temporary table reference with the specified name on the provided connection with the specified schema.
        /// </summary>
        public DatabaseTableAppender(SqlConnection connection, DatabaseTableSchema<T> schema, string tableName)
        {
            InitialiseTableReference(new TemporaryDatabaseTableReference<T>(connection, schema, tableName));
        }

        /// <summary>
        /// Create a unique temporary table reference on the provided connection with the specified schema.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="schema"></param>
        public DatabaseTableAppender(SqlConnection connection, DatabaseTableSchema<T> schema)
        {
            InitialiseTableReference(new TemporaryDatabaseTableReference<T>(connection, schema));
        }

        /// <summary>
        /// Use the specified table reference as the backing store.
        /// </summary>
        /// <remarks>
        /// A reference will be acquired by the constructor.
        /// The table will be created when writing begins.
        /// </remarks>
        public DatabaseTableAppender(TemporaryDatabaseTableReference<T> tableReference)
        {
            InitialiseTableReference(tableReference);
        }

        private void InitialiseTableReference(TemporaryDatabaseTableReference<T> tableReference)
        {
            this.TableReference = tableReference;
            reference = this.TableReference.Acquire();
        }

        private TableWriter<T> writer;

        protected virtual void OnWriterOpened(TableWriter<T> writer)
        {
        }

        protected void EnsureUnderlyingWriterIsOpen()
        {
            if (writer != null) return;

            // create temp table
            writer = this.TableReference.Create();
            OnWriterOpened(writer);
        }

        private void EndWrite()
        {
            if (writer == null) return;
            try
            {
                writer.Dispose();
            }
            catch (Exception ex)
            {
                throw new BackingStoreException("Could not complete bulk insert.", ex);
            }
        }

        protected void CloseUnderlyingWriter()
        {
            if (completed) throw new InvalidOperationException();
            completed = true;
            EndWrite();
        }

        protected void ReleaseReference()
        {
            reference.Dispose();
        }

        public void Add(T item)
        {
            AddRange(new [] { item });
        }

        public void AddRange(IEnumerable<T> items)
        {
            if (completed) throw new ReadOnlyException();
            EnsureUnderlyingWriterIsOpen();
            try
            {
                Count += writer.Write(items);
            }
            catch (BackgroundWriterAbortedException ex)
            {
                throw new BackingStoreException("Could not perform bulk insert.", ex);
            }
        }

        public long Count { get; private set; }

        public void Dispose()
        {
            try
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            finally
            {
                // This must be released last, and it MUST be released:
                ReleaseReference();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            EndWrite();
        }

        public DatabaseTableReference<T> BackingStore
        {
            get { return this.TableReference; }
        }
    }
}
