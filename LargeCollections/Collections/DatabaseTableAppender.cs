﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using LargeCollections.Resources;
using LargeCollections.Storage.Database;

namespace LargeCollections.Collections
{
    public class DatabaseTableAppender<T> : IAppendable<T>, IDisposable, IHasBackingStore<DatabaseTableReference<T>>
    {
        protected TemporaryDatabaseTableReference<T> TableReference;
        private IDisposable reference;
        private bool completed;

        public DatabaseTableAppender(SqlConnection connection, DatabaseTableSchema<T> schema)
        {
            this.TableReference = new TemporaryDatabaseTableReference<T>(connection, schema);
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
            writer.Dispose();
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
            Count += writer.Write(items);
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