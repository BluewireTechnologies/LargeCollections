using System;
using System.Collections.Generic;
using System.Data;

namespace LargeCollections.SqlServer
{
    public interface IEntityReader<out T> : IEnumerator<T>
    {
        /// <summary>
        /// Reads the entire resultset as an IEnumerable.
        /// </summary>
        /// <remarks>
        /// BEWARE: This is reading a single-pass IEnumerator. Ensure that any operations applied to the IEnumerable do not perform multiple passes.
        /// </remarks>
        /// <returns></returns>
        IEnumerable<T> ReadAll();
    }

    public class TableReader<T> : IEntityReader<T>
    {
        private readonly IDataReader reader;
        private readonly NameValueObjectFactory<string, T> objectFactory;

        public TableReader(IDataReader reader, NameValueObjectFactory<string, T> objectFactory)
        {
            this.reader = reader;
            this.objectFactory = objectFactory;
        }

        private bool loadedCurrent;
        private T current;

        public T Current
        {
            get
            {
                if(!loadedCurrent)
                {
                    current = objectFactory.ReadRecord(f => ReadValueAsClrType(f.Name));
                    loadedCurrent = true;
                }
                return current;
            }
        }

        private object ReadValueAsClrType(string fieldName)
        {
            var value = reader[fieldName];
            if (value is DBNull)
            {
                return null;
            }
            return value;
        }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            loadedCurrent = false;
            return reader.Read();
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
            reader.Dispose();
        }

        /// <summary>
        /// Reads the entire resultset as an IEnumerable.
        /// </summary>
        /// <remarks>
        /// BEWARE: This is reading a single-pass IEnumerator. Ensure that any operations applied to the IEnumerable do not perform multiple passes.
        /// </remarks>
        /// <returns></returns>
        public IEnumerable<T> ReadAll()
        {
            while (MoveNext())
            {
                yield return Current;
            }
        }
    }
}