using System;
using System.Collections.Generic;
using System.Data;
using LargeCollections.Core;

namespace LargeCollections.SqlServer
{
    public class DatabaseTableAppender<T> : IAppendable<T>, IDisposable, IHasBackingStore<DatabaseTableReference<T>>
    {
        private readonly TableWriterOptions options;
        protected TemporaryDatabaseTableReference<T> TableReference;
        private IDisposable reference;
        private bool completed;

        /// <summary>
        /// Create a temporary table reference with the specified name on the provided connection with the specified schema.
        /// </summary>
        public DatabaseTableAppender(SqlSession session, DatabaseTableSchema<T> schema, string tableName, TableWriterOptions options = default(TableWriterOptions))
        {
            this.options = options;
            InitialiseTableReference(new TemporaryDatabaseTableReference<T>(session, schema, tableName));
        }

        /// <summary>
        /// Create a unique temporary table reference on the provided connection with the specified schema.
        /// </summary>
        public DatabaseTableAppender(SqlSession session, DatabaseTableSchema<T> schema, TableWriterOptions options = default(TableWriterOptions))
        {
            this.options = options;
            InitialiseTableReference(new TemporaryDatabaseTableReference<T>(session, schema));
        }

        /// <summary>
        /// Use the specified table reference as the backing store.
        /// </summary>
        /// <remarks>
        /// A reference will be acquired by the constructor.
        /// The table will be created when writing begins.
        /// </remarks>
        public DatabaseTableAppender(TemporaryDatabaseTableReference<T> tableReference, TableWriterOptions options = default(TableWriterOptions))
        {
            this.options = options;
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
            writer = this.TableReference.Create(options);
            OnWriterOpened(writer);
        }

        private void EndWrite()
        {
            if (writer == null) return;
            try
            {
                writer.Complete();
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
